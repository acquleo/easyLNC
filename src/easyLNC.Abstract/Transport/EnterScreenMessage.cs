using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.Abstract.Transport
{
    public class EnterScreenMessage : acquleo.Protocol.IMessage
    {
        public EnterScreenMessage()
        {            

        }

        public EnterScreenMessage(byte[] data) 
        {
            this.ScreenIndex = BitConverter.ToInt32(data, 0);
        }

        public int ScreenIndex { get; set; }

        public byte[] Serialize()
        {
            var keyBytes = BitConverter.GetBytes(this.ScreenIndex);
            return keyBytes.ToArray();
        }

        public IEnumerable<Header> GetHeaders()
        {
            return new List<Header>();
        }
    }
}
