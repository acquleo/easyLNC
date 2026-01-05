using D3D11Rendering;
using easyLNC.Helper.Direct3D11;
using libomtnet;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Device = SharpDX.Direct3D11.Device;

namespace OmtViewer
{
    public partial class Form1 : Form
    {
        Device device;
        D3D11WinFormsRenderer render;
        public Form1()
        {
            InitializeComponent();

            this.HandleCreated += Form1_HandleCreated;
        }

        private void Form1_HandleCreated(object? sender, EventArgs e)
        {
            if (device == null)
                device = Direct3D11Helper.CreateDevice();

            if (this.render == null)
                this.render = new D3D11WinFormsRenderer(device, this.Handle, this.ClientSize.Width, this.ClientSize.Height);

            OMTLogging.SetFilename("omtlog.txt");
            //OMTDiscovery omtdis = OMTDiscovery.GetInstance();
            //var addresses = omtdis.GetAddresses();
            //OMTReceive receiver = new OMTReceive(addresses.FirstOrDefault(), OMTFrameType.Video, OMTPreferredVideoFormat.BGRA, OMTReceiveFlags.None);
            OMTReceive receiver = new OMTReceive($@"omt://127.0.0.1:6400", OMTFrameType.Video, OMTPreferredVideoFormat.BGRA, OMTReceiveFlags.None);


            new Thread(() =>
            {
                while (true)
                {
                    OMTMediaFrame frame = new OMTMediaFrame();
                    if (receiver.Receive(OMTFrameType.Video, 1000, ref frame))
                    {
                        this.Invoke(() =>
                        {
                            var desc = new Texture2DDescription
                            {
                                Width = frame.Width,
                                Height = frame.Height,
                                MipLevels = 1,
                                ArraySize = 1,
                                Format = Format.B8G8R8A8_UNorm,
                                SampleDescription = new SampleDescription(1, 0),
                                Usage = ResourceUsage.Dynamic,
                                BindFlags = BindFlags.ShaderResource,
                                CpuAccessFlags = CpuAccessFlags.Write,
                                OptionFlags = ResourceOptionFlags.None
                            };

                            using (var texture = new Texture2D(device, desc))
                            {
                                var dataBox = device.ImmediateContext.MapSubresource(
                                   texture,
                                   0,
                                   MapMode.WriteDiscard,
                                   SharpDX.Direct3D11.MapFlags.None);

                                unsafe
                                {
                                    System.Buffer.MemoryCopy(
                                        (void*)frame.Data,
                                        (void*)dataBox.DataPointer,
                                        frame.DataLength,
                                        frame.DataLength);
                                }

                                device.ImmediateContext.UnmapSubresource(texture, 0);


                                this.render.Render(texture);
                            
                            }
                        });
                        // Process the received frame
                    }
                }
            }).Start();
        }

        nint m_handle;
        public nint ViewPortHandle => m_handle;
        public int ViewPortWidth => this.ClientSize.Width;
        public int ViewPortHeight => this.ClientSize.Height;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            render?.Resize(this.ClientSize.Width, this.ClientSize.Height);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            m_handle = this.Handle;
            base.OnHandleCreated(e);

            
        }

        public static void CopyBGRAIntoTexture(
    DeviceContext context,
    Texture2D texture,
    IntPtr bgraPtr,
    int width,
    int height,
    int stride)
        {
            Console.WriteLine($"W={width}, H={height}, Stride={stride}");

            var box = new DataBox(
                bgraPtr,
                stride,   // RowPitch
                0);       // SlicePitch (unused for 2D)

            context.UpdateSubresource(
                box,
                texture,
                0);
        }
    }
}
