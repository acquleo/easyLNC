using easyLNC.Abstract;
using easyLNC.Abstract.Frame;
using easyLNC.ScreenCapture.WGC;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using static DxgiScreenCapture;
using Device = SharpDX.Direct3D11.Device;

public class DxgiScreenCapture :  IScreenCapture, IDisposable
{
    private Device _device;
    private OutputDuplication _deskDupl;
        
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
        Output? output = null;
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

        
        // Create duplication
        _deskDupl = output1.DuplicateOutput(_device);

        // Release temporary COM objects
        output1.Dispose();
        output.Dispose();
        factory.Dispose();
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
                        if (frameInfo.LastPresentTime != 0)
                        {
                            var texture = desktopResource.QueryInterface<Texture2D>();

                            var dataPointer = TextureUtils.GetBGRAFromTexture(this._device.ImmediateContext,
                            texture, out int width, out int height, out int stride, out int dataLen);
                                      
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
                            texture.Dispose();
                        }
                    }

                    _deskDupl.ReleaseFrame();
                }

                Thread.Sleep(1000 / 60);
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
