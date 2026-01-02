using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.Abstract.Transport
{
    public class MouseButtonActionMessage : acquleo.Protocol.IMessage
    {
        public MouseButtonActionMessage()
        {            

        }

        public MouseButtonActionMessage(byte[] data) 
        {
            this.Button = BitConverter.ToInt32(data, 0);            
            this.X = BitConverter.ToDouble(data, 4);
            this.Y = BitConverter.ToDouble(data, 12);
            this.Action = BitConverter.ToInt32(data, 20);
            this.ScreenIndex = BitConverter.ToInt32(data, 24);
        }

        public int ScreenIndex { get; set; }
        public int Button { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Action { get; set; }

        public byte[] Serialize()
        {
            var keyBytes = BitConverter.GetBytes(this.Button);
            var xBytes = BitConverter.GetBytes(this.X);
            var yBytes = BitConverter.GetBytes(this.Y);
            var actionBytes = BitConverter.GetBytes(this.Action);
            var screenIndexBytes = BitConverter.GetBytes(this.ScreenIndex);
            return keyBytes.Concat(xBytes).Concat(yBytes).Concat(actionBytes).Concat(screenIndexBytes).ToArray();
        }

        public IEnumerable<Header> GetHeaders()
        {
            return new List<Header>();
        }
    }
}
