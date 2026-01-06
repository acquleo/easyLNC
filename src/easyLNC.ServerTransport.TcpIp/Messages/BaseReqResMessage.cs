using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.ServerTransport.TcpIp.Messages
{
    public class BaseReqResMessage
    {
        public int MessageId { get; set; }

        public IEnumerable<Header> GetHeaders()
        {
            return new List<Header>();
        }

        public object GetMsgRef()
        {
            return MessageId;
        }
    }
}
