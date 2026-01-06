using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.ServerTransport.TcpIp.Messages
{ 
    public partial class ScreenCaptureStartRes 
    {
        public List<ScreenStreamInfo> ScreenStreamInfos { get; set; } = new List<ScreenStreamInfo>();
    }

    public class ScreenStreamInfo
    {
        public string CaptureId { get; set; } = string.Empty;
        public int MonitorIndex { get; set; } = 0;
        public string StreamType { get; set; } = string.Empty;
        public string StreamParams { get; set; } = string.Empty;
    }
}
