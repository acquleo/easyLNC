using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.Abstract.Transport
{
    public class MouseMoveMessage : acquleo.Protocol.IMessage
    {
        public MouseMoveMessage()
        {            

        }

        public MouseMoveMessage(byte[] data) 
        {
            this.ScreenIndex = BitConverter.ToInt32(data, 0);
            this.X = BitConverter.ToDouble(data, 4);
            this.Y = BitConverter.ToDouble(data, 12);
        }

        public int ScreenIndex { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public byte[] Serialize()
        {
            var keyBytes = BitConverter.GetBytes(this.ScreenIndex);
            var xBytes = BitConverter.GetBytes(this.X);
            var yBytes = BitConverter.GetBytes(this.Y);
            return keyBytes.Concat(xBytes).Concat(yBytes).ToArray();
        }

        public IEnumerable<Header> GetHeaders()
        {
            return new List<Header>();
        }
    }
}
