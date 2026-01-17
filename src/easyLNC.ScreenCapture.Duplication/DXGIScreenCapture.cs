using easyLNC.Abstract;
using easyLNC.Abstract.Frame;
using easyLNC.ScreenCapture.WGC;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using static DxgiScreenCapture;
using Device = SharpDX.Direct3D11.Device;

public class DxgiScreenCapture :  IScreenCapture, IDisposable
{
    private Device _device;
    private OutputDuplication _deskDupl;
    Output? output = null;
    
    private Texture2D stagingTexture;
    private Texture2D currentFrameTexture;
    private Texture2D previousFrameTexture;

    private Thread _captureThread;
    private bool _running;
    public event Action<IScreenCapture, ScreenFrame>? OnNewFrame;
    ScreenInfo screen;
    int fps = 60;

    public DxgiScreenCapture(ScreenInfo wgcScreen, SharpDX.Direct3D11.Device device, int outputIndex = 0)
    {
        this._device = device;
        this.screen = wgcScreen;

        Initialize();
    }
    public bool IsRunning()
    {
        return true;
    }
    public ScreenInfo Screen => this.screen;
    public int Width { get; private set; }
    public int Height { get; private set; }
    private void Initialize()
    {
        // Create Direct3D 11 device
        var factory = new Factory1();
        
        var dxgiDevice = _device.QueryInterface<SharpDX.DXGI.Device>();
        var adapter = dxgiDevice.Adapter;


        // Get output (monitor)
        var outputCOunt = adapter.GetOutputCount();
        for (int i = 0; i < outputCOunt; i++)
        {
            var tmp_output = adapter.GetOutput(i);
            if(tmp_output.Description.DeviceName==this.screen.Name)
            {
                output = tmp_output;
            }
        }
        if (output == null)
        {
            throw new InvalidOperationException($@"output not found {this.screen.Name}");
        }
        var output1 = output.QueryInterface<Output1>();

        var bounds = output1.Description.DesktopBounds;
        Width = bounds.Right - bounds.Left;
        Height = bounds.Bottom - bounds.Top;


        CreateTextures();

        // Create duplication
        _deskDupl = output1.DuplicateOutput(_device);


        
        // Release temporary COM objects
        output1.Dispose();
        factory.Dispose();
    }
    private bool CheckResolutionChange()
    {
        var output1 = output.QueryInterface<Output1>();

        var bounds = output1.Description.DesktopBounds;
        int newWidth = bounds.Right - bounds.Left;
        int newHeight = bounds.Bottom - bounds.Top;

        if (newWidth != Width || newHeight != Height)
        {
            Console.WriteLine($"Resolution changed: {Width}x{Height} -> {newWidth}x{newHeight}");
            Width = newWidth;
            Height = newHeight;
            return true;
        }
        return false;
    }

    private void RecreateResources()
    {
        var output1 = output.QueryInterface<Output1>();

        // Release duplication
        _deskDupl?.Dispose();

        // Recreate textures with new size
        CreateTextures();

        // Recreate duplication
        _deskDupl = output1.DuplicateOutput(_device);
    }
    private void CreateTextures()
    {
        // Dispose old textures if they exist
        stagingTexture?.Dispose();
        currentFrameTexture?.Dispose();
        previousFrameTexture?.Dispose();

        // Staging texture for CPU readback
        var stagingDesc = new Texture2DDescription
        {
            CpuAccessFlags = CpuAccessFlags.Read,
            BindFlags = BindFlags.None,
            Format = Format.B8G8R8A8_UNorm,
            Width = Width,
            Height = Height,
            MipLevels = 1,
            ArraySize = 1,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Staging
        };
        stagingTexture = new Texture2D(_device, stagingDesc);

        // GPU textures for incremental updates
        var gpuDesc = new Texture2DDescription
        {
            CpuAccessFlags = CpuAccessFlags.None,
            BindFlags = BindFlags.None,
            Format = Format.B8G8R8A8_UNorm,
            Width = Width,
            Height = Height,
            MipLevels = 1,
            ArraySize = 1,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default
        };
        currentFrameTexture = new Texture2D(_device, gpuDesc);
        previousFrameTexture = new Texture2D(_device, gpuDesc);
    }

    public void Start()
    {
        if (_running)
            return;

        _running = true;
        _captureThread = new Thread(CaptureLoop)
        {
            IsBackground = true
        };
        _captureThread.Start();
    }

    public void Stop()
    {
        _running = false;
        _captureThread?.Join();
    }

    private void CaptureLoop()
    {
        while (_running)
        {
            try
            {
                // Try to acquire next frame
                if (_deskDupl.TryAcquireNextFrame(500, out var frameInfo, out var desktopResource) == Result.Ok)
                {
                    using (desktopResource)
                    {
                        // Check for resolution changes
                        bool resolutionChanged = CheckResolutionChange();
                        if (resolutionChanged)
                        {
                            _deskDupl.ReleaseFrame();
                            RecreateResources();
                            continue; // Skip this frame, next one will have correct size
                        }

                        bool hasScreenUpdate = frameInfo.TotalMetadataBufferSize > 0 ||
                                      frameInfo.LastPresentTime > 0 ||
                                      frameInfo.AccumulatedFrames > 0;


                        if (hasScreenUpdate)
                        {
                            using var screenTexture = desktopResource.QueryInterface<Texture2D>();

                            // Check if we have move/dirty rect metadata
                            bool hasMetadata = frameInfo.TotalMetadataBufferSize > 0;

                            if (hasMetadata)
                            {
                                // Get move rectangles
                                OutputDuplicateMoveRectangle[] moveRects = null;
                                int moveCount = 0;

                                if (frameInfo.TotalMetadataBufferSize > 0)
                                {
                                    // Allocate max possible size for move rects
                                    int maxMoveRects = frameInfo.TotalMetadataBufferSize / Marshal.SizeOf(typeof(OutputDuplicateMoveRectangle));
                                    if (maxMoveRects > 0)
                                    {
                                        moveRects = new OutputDuplicateMoveRectangle[maxMoveRects];
                                        int moveSize;
                                        var move_bufferSize = maxMoveRects * Marshal.SizeOf(typeof(OutputDuplicateMoveRectangle));
                                        try
                                        {
                                            _deskDupl.GetFrameMoveRects(move_bufferSize, moveRects, out moveSize);
                                            moveCount = moveSize / Marshal.SizeOf(typeof(OutputDuplicateMoveRectangle));
                                        }
                                        catch
                                        {
                                            moveCount = 0;
                                        }
                                    }
                                }

                                // Get dirty rectangles
                                RawRectangle[] dirtyRects = null;
                                int dirtyCount = 0;

                                // Allocate buffer for dirty rects (use total size as upper bound)
                                int maxDirtyRects = Math.Max(1, frameInfo.TotalMetadataBufferSize / Marshal.SizeOf(typeof(RawRectangle)));
                                dirtyRects = new RawRectangle[maxDirtyRects];
                                var bufferSize = maxDirtyRects * Marshal.SizeOf(typeof(RawRectangle));

                                try
                                {
                                    _deskDupl.GetFrameDirtyRects(bufferSize, dirtyRects, out var dirtySize);
                                    dirtyCount = dirtySize / Marshal.SizeOf(typeof(Rectangle));
                                }
                                catch (SharpDXException ex)
                                {
                                    // E_INVALIDARG means no dirty rects available
                                    if (ex.ResultCode.Code != unchecked((int)0x80070057))
                                        throw;
                                    dirtyCount = 0;
                                }

                                // Start with previous frame
                                _device.ImmediateContext.CopyResource(previousFrameTexture, currentFrameTexture);

                                // Apply move rectangles
                                for (int i = 0; i < moveCount; i++)
                                {
                                    var move = moveRects[i];
                                    int _width = move.DestinationRect.Right - move.DestinationRect.Left;
                                    int _height = move.DestinationRect.Bottom - move.DestinationRect.Top;

                                    if (_width > 0 && _height > 0)
                                    {
                                        var srcBox = new ResourceRegion
                                        {
                                            Left = move.SourcePoint.X,
                                            Top = move.SourcePoint.Y,
                                            Front = 0,
                                            Right = move.SourcePoint.X + _width,
                                            Bottom = move.SourcePoint.Y + _height,
                                            Back = 1
                                        };

                                        _device.ImmediateContext.CopySubresourceRegion(
                                            screenTexture, 0, srcBox,
                                            currentFrameTexture, 0,
                                            move.DestinationRect.Left, move.DestinationRect.Top);
                                    }
                                }

                                // Apply dirty rectangles
                                for (int i = 0; i < dirtyCount; i++)
                                {
                                    var dirty = dirtyRects[i];
                                    if (dirty.Right > dirty.Left && dirty.Bottom > dirty.Top)
                                    {
                                        var dirtyBox = new ResourceRegion
                                        {
                                            Left = dirty.Left,
                                            Top = dirty.Top,
                                            Front = 0,
                                            Right = dirty.Right,
                                            Bottom = dirty.Bottom,
                                            Back = 1
                                        };

                                        _device.ImmediateContext.CopySubresourceRegion(
                                            screenTexture, 0, dirtyBox,
                                            currentFrameTexture, 0,
                                            dirty.Left, dirty.Top);
                                    }
                                }
                            }
                            else
                            {
                                // No metadata - full frame copy
                                _device.ImmediateContext.CopyResource(screenTexture, currentFrameTexture);
                            }

                            // Save current frame as previous for next iteration
                            _device.ImmediateContext.CopyResource(currentFrameTexture, previousFrameTexture);

                            var dataPointer = TextureUtils.GetBGRAFromTexture(this._device.ImmediateContext,
                            screenTexture, out int width, out int height, out int stride, out int dataLen);
                            


                            this.OnNewFrame?.Invoke(this, new ScreenFrameUnmanagedMemory()
                            {

                                Height = height,
                                Width = width,
                                Data = dataPointer.Handle,
                                DataLength = dataLen,
                                Type = FrameTypes.UnmanagedMemory,
                                Format = PixelFormats.B8G8R8A8,
                                Timestamp = TimeSpan.FromTicks(frameInfo.LastPresentTime),
                                Stride = stride
                            });

                            dataPointer.Dispose();

                            //var bitmap = CopyTextureToBitmap(texture);
                            //OnFrameCaptured?.Invoke(bitmap);
                            screenTexture.Dispose();
                        }
                    }

                    _deskDupl.ReleaseFrame();
                }

                //Thread.Sleep(1000 / 60);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Failure)
                {
                    // Ignore timeout exceptions, handle others if needed
                }
            }


        }

    }

    public void Dispose()
    {
        Stop();
        _deskDupl?.Dispose();
        _device?.Dispose();
    }
}
