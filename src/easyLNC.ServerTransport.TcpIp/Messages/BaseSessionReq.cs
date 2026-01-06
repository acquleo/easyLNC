using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.ServerTransport.TcpIp.Messages
{
    public partial class BaseSessionReq : IMessageRequest
    {
        public string SessionId { get; set; } = string.Empty;

        public IEnumerable<Header> GetHeaders()
        {
            throw new NotImplementedException();
        }

        public object GetMsgRef()
        {
            throw new NotImplementedException();
        }
    }
}
