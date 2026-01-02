using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.Abstract.Transport
{
    public class KeepAliveMessage : acquleo.Protocol.IMessage
    {
        public KeepAliveMessage()
        {            

        }

        public KeepAliveMessage(byte[] data) 
        {

        }


        public byte[] Serialize()
        {
            return Array.Empty<byte>();
        }

        public IEnumerable<Header> GetHeaders()
        {
            return new List<Header>();
        }
    }
}
