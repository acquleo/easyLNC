using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.Abstract.Transport
{
    public class MouseWheelMessage : acquleo.Protocol.IMessage
    {
        public MouseWheelMessage()
        {            

        }

        public MouseWheelMessage(byte[] data) 
        {
            this.Delta = BitConverter.ToInt32(data, 0);

        }

        public int Delta { get; set; }

        public byte[] Serialize()
        {
            var keyBytes = BitConverter.GetBytes(this.Delta);
            return keyBytes;
        }

        public IEnumerable<Header> GetHeaders()
        {
            return new List<Header>();
        }
    }
}
