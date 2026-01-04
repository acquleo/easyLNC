using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.Abstract.Transport
{
    public class KvmProtocolEnveloper : acquleo.Protocol.IMessageSerializer<byte[]>
    {
        Dictionary<string, Type> _messageTypes = new Dictionary<string, Type>
        {
            { nameof(VirtualKeyDown), typeof(VirtualKeyDown) },
            { nameof(VirtualKeyUp), typeof(VirtualKeyUp) },
            { nameof(ControlStartMessage), typeof(ControlStartMessage) },
            { nameof(ControlEndMessage), typeof(ControlEndMessage) },
            { nameof(MouseMove), typeof(MouseMove) },
            { nameof(MouseButtonAction), typeof(MouseButtonAction) },
            { nameof(MouseWheel), typeof(MouseWheel) },
            { nameof(KeepAlive), typeof(KeepAlive) },
            { nameof(MouseEnterScreen), typeof(MouseEnterScreen) },
            { nameof(MouseLeaveScreen), typeof(MouseLeaveScreen) },



        };

        public bool CanDeserialize(ContentData<byte[]> data)
        {
            return _messageTypes.ContainsKey(data.ContentType);
        }

        public bool CanSerialize(IMessage msg)
        {
            return _messageTypes.ContainsKey(msg.GetType().Name);
        }

        public IMessage Deserialize(ContentData<byte[]> data)
        {
            var type = _messageTypes[data.ContentType];

            return (IMessage)Activator.CreateInstance(type, data.Data) ?? throw new InvalidOperationException();

        }

        public ContentData<byte[]> Serialize(IMessage msg)
        {
            var bytes = (byte[])msg.GetType().InvokeMember("Serialize", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, msg, null);
            return new ContentData<byte[]>(msg.GetType().Name, bytes);
        }
    }
}
