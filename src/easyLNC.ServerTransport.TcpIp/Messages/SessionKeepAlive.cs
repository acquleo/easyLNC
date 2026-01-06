using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.ServerTransport.TcpIp.Messages
{
    public partial class SessionKeepAlive
    {
        public string SessionId { get; set; } = string.Empty;
    }
}
