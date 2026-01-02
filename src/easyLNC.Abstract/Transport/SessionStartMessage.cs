using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.Abstract.Transport
{
    public class SessionStartMessage : acquleo.Protocol.IMessage
    {
        public SessionStartMessage()
        {            

        }

        public SessionStartMessage(byte[] data) 
        { 

        }


        public byte[] Serialize()
        {
            return new byte[0];
        }

        public IEnumerable<Header> GetHeaders()
        {
            return new List<Header>();
        }
    }
}
