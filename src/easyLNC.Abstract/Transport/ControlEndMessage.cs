using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.Abstract.Transport
{
    public class ControlEndMessage : acquleo.Protocol.IMessage
    {
        public ControlEndMessage()
        {            

        }

        public ControlEndMessage(byte[] data) 
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
