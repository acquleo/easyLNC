
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.Abstract.Transport
{
    public class SendVirtualKeyUpMessage : acquleo.Protocol.IMessage
    {
        public SendVirtualKeyUpMessage()
        {
            
        }
        public SendVirtualKeyUpMessage(byte[] data)
        {
            this.VirtualKey = BitConverter.ToInt32(data, 0);
        }

        public int VirtualKey { get; set; }

        public byte[] Serialize()
        {
            var keyBytes = BitConverter.GetBytes(this.VirtualKey);
            return keyBytes.ToArray();
        }

        public IEnumerable<Header> GetHeaders()
        {
            return new List<Header>();
        }
    }
}
