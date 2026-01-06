using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.ServerTransport.TcpIp.Messages
{
    public partial class ScreenCaptureEndReq : BaseReqResMessage
    {
        public int ScreenIndex { get; set; } = 0;
    }
}
