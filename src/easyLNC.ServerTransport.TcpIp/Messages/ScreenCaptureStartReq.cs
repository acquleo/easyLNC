using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.ServerTransport.TcpIp.Messages
{ 
    public partial class ScreenCaptureStartReq : BaseSessionReq
    {
        public List<int> ScreenList { get; set; } = new List<int>();
    }
}
