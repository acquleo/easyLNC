using easyLNC.Abstract;
using easyLNC.Abstract.Frame;
using libomtnet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace easyLNC.ScreenStream.OMT
{
    public class OmtScreenStream : IScreenStream, IDisposable
    {
        readonly OMTSend sender;

        IScreenCapture screenCapture;
        public OmtScreenStream(IScreenCapture screenCapture)
        {
            this.screenCapture = screenCapture;
            this.sender = new OMTSend(screenCapture.Screen.Name, OMTQuality.Low);
            this.screenCapture.OnNewFrame += ScreenCapture_OnNewFrame;
        }

        public void Dispose()
        {
            this.sender?.Dispose();
        }

        private void ScreenCapture_OnNewFrame(IScreenCapture arg1, ScreenFrame arg2)
        {
            if(arg2.Type != Abstract.Frame.FrameTypes.UnmanagedMemory)
            {
                return;
            }

            ScreenFrameUnmanagedMemory frame = (ScreenFrameUnmanagedMemory)arg2;

            var time = frame.Timestamp;

            var omtFrame = new OMTMediaFrame()
            {
                Type = OMTFrameType.Video,
                Codec = (int)OMTCodec.BGRA,
                Width = frame.Width,
                Height = frame.Height,
                Timestamp = (long)time.TotalMilliseconds * 10000, // in 100-nanosecond units
                AspectRatio = (float)frame.Width / (float)frame.Height,
                Stride = frame.Stride,
                ColorSpace = OMTColorSpace.BT709,
                Data = frame.Data,
                DataLength = frame.DataLength,
                Flags = OMTVideoFlags.Alpha,
            };

            var omtResult = sender.Send(omtFrame);
        }
    }
}
