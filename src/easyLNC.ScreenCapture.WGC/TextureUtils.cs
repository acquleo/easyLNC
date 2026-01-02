using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.ScreenCapture.WGC
{
    public static class TextureUtils
    {
        /// <summary>
        /// Copies a GPU BGRA texture to CPU memory and returns a pointer to the pinned buffer.
        /// The caller must call FreeBGRA after use.
        /// </summary>
        public static PinnedFrameBuffer GetBGRAFromTexture(DeviceContext context, Texture2D gpuTex, out int width, out int height, out int stride, out int dataLen)
        {
            width = gpuTex.Description.Width;
            height = gpuTex.Description.Height;
            stride = width * 4; // bytes per row, tightly packed

            // Create staging texture
            var desc = new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            };

            using var stagingTex = new Texture2D(gpuTex.Device, desc);

            // Copy GPU texture to staging
            context.CopyResource(gpuTex, stagingTex);

            // Map staging texture
            var dataBox = context.MapSubresource(stagingTex, 0, MapMode.Read, MapFlags.None);

            dataLen = height * stride;

            // Allocate managed buffer and copy row by row

            var buffer = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(
                    dataBox.DataPointer + y * dataBox.RowPitch,
                    buffer,
                    y * stride,
                    stride);
            }

            context.UnmapSubresource(stagingTex, 0);

            // Pin the buffer
            return new PinnedFrameBuffer(buffer);
        }

        /// <summary>
        /// Frees the pinned buffer obtained from GetBGRAFromTexture.
        /// </summary>
        public static void FreeBGRA(nint buffer)
        {

            Marshal.FreeHGlobal(buffer);
        }
    }

    public sealed class PinnedFrameBuffer : IDisposable
    {
        public nint Handle { get; }
        public int Length { get; }

        private GCHandle _handle;
        private byte[] _buffer;

        public PinnedFrameBuffer(byte[] buffer)
        {
            _buffer = buffer;
            _handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Handle = _handle.AddrOfPinnedObject();
            Length = buffer.Length;
        }

        public void Dispose()
        {
            if (_handle.IsAllocated)
                _handle.Free();
        }
    }
}
