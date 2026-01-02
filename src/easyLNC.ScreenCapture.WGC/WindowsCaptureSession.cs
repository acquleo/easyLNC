using acquleo.Base.Logging;
using easyLNC.Abstract;
using easyLNC.Abstract.Frame;
using easyLNC.Helper.Direct3D11;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;

namespace easyLNC.ScreenCapture.WGC
{
    internal class WindowsCaptureSession : IScreenCapture, IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();
        public event Action<Direct3D11CaptureFrame, Device , bool>? OnFrameArrived;
        public event Action<IScreenCapture, ScreenFrame>? OnNewFrame;

        ScreenInfo screen;
        readonly private SharpDX.Direct3D11.Device device;
        private Direct3D11CaptureFramePool? framePool;
        readonly IDirect3DDevice direct3DDevice;
        private GraphicsCaptureSession? session;

        readonly IApplicationLog log;
        public WindowsCaptureSession(ScreenInfo wgcScreen, SharpDX.Direct3D11.Device device)
        {
            this.log = SingletonFactoryProvider.Provider.GetApplicationLogFactory().GetBuilder().WithObject(this).Build();
            this.screen = wgcScreen;
            this.device = device;

            direct3DDevice = easyLNC.Helper.Direct3D11.Direct3D11Helper.CreateDirect3DDeviceFromSharpDXDevice(device)
                ?? throw new InvalidOperationException("Failed to create Direct3D device.");
        }

        public async void StartCapture()
        {
            this.log.InfoIf(this,()=> $@"Starting capture for screen: {this.screen.Name}");

            var _picker = new GraphicsCapturePicker();

            var item = CaptureHelper.CreateItemForMonitor(this.screen.Handle);

            if (item == null)
            {
                throw new InvalidOperationException("Failed to create GraphicsCaptureItem for monitor.");
            }

            framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                                    direct3DDevice,
                                    DirectXPixelFormat.B8G8R8A8UIntNormalized,
                                    2,
                                    item.Size);

            session = framePool.CreateCaptureSession(item);


            //session.IsBorderRequired = false; // TODO: da gestire

            framePool.FrameArrived += framePool_FrameArrived;
            session.StartCapture();
        }

        SizeInt32 lastSize = new SizeInt32();

        public ScreenInfo Screen => this.screen;

        TimeSpan lastTimestapn = TimeSpan.Zero;
        private void framePool_FrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            var newSize = false;
            using (var frame = sender.TryGetNextFrame())
            {
                if(frame.ContentSize.Width != lastSize.Width ||
                   frame.ContentSize.Height != lastSize.Height)
                {
                    lastSize = frame.ContentSize;
                    newSize = true;
                }
                
                using (var texture = Direct3D11Helper.CreateSharpDXTexture2D(frame.Surface))
                {
                    if (lastTimestapn == TimeSpan.Zero)
                    {
                        lastTimestapn = frame.SystemRelativeTime;
                    }

                    var time = frame.SystemRelativeTime - lastTimestapn;

                    var dataPointer = TextureUtils.GetBGRAFromTexture(this.device.ImmediateContext,
                        texture, out int width, out int height, out int stride, out int dataLen);

                    this.OnNewFrame?.Invoke(this, new ScreenFrameUnmanagedMemory()
                    {
                        
                        Height = frame.ContentSize.Height,
                        Width = frame.ContentSize.Width,
                        Data = dataPointer.Handle,
                        DataLength = dataLen,
                        Type =  FrameTypes.UnmanagedMemory,
                        Format =  PixelFormats.B8G8R8A8,
                        Timestamp = time,
                        Stride = stride
                    });

                    dataPointer.Dispose();
                }

                this.OnFrameArrived?.Invoke(frame, device, newSize);
            }
        }
        public void Dispose()
        {
            session?.Dispose();
            if(framePool!=null) framePool.FrameArrived -= framePool_FrameArrived;
            framePool?.Dispose();
            direct3DDevice?.Dispose();
        }

        public bool IsRunning()
        {
            return true;
        }
    }
}
