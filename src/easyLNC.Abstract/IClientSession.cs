using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public class ClientScreenCaptureInfo
    {
        public IScreenCapture ScreenCapture { get; set; }
        public StreamInfo StreamInfo { get; set; }
        public ClientScreenCaptureInfo(IScreenCapture screenCapture, StreamInfo streamInfo)
        {
            ScreenCapture = screenCapture;
            StreamInfo = streamInfo;
        }
    }

    public interface IClientSession
    {
        public string Id { get; }
        IEnumerable<ClientScreenCaptureInfo> GetScreenCaptures();

    }
}
