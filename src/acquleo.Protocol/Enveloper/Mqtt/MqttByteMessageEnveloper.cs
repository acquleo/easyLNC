using MQTTnet.Protocol;
using System.Linq;
using acquleo.Protocol.Exceptions;
using acquleo.Protocol.Transport.Mqtt;

namespace acquleo.Protocol.Enveloper.Mqtt
{
    /// <summary>
    /// implements a message enveloper for MqttMessage
    /// </summary>
    public class MqttByteMessageEnveloper : IMessageEnveloper<MqttMessage, MqttMessageEnvelope>
    {
        readonly IMessageSerializer<byte[]> serializer;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        public MqttByteMessageEnveloper(IMessageSerializer<byte[]> serializer)
        {
            this.serializer = serializer;
        }

        /// <inheritdoc/>
        public MqttMessageEnvelope Wrap(MqttMessage data)
        {
            MqttMessageEnvelope topic = new MqttMessageEnvelope();            

            topic.Topic = data.Topic;
            topic.Responsetopic = data.Responsetopic;
            
            topic.Retain = data.Retain;
            topic.Qos = data.Qos;
            topic.FormatIndicatorPayload = data.PayloadFormatIndicator;
            topic.PayloadFormat = string.Empty;
            topic.TransportMessage = data;
            var payloadFormat = data.Userproperties.Find(e => e.Name == nameof(topic.PayloadFormat));
            if (payloadFormat!=null)
            {
                topic.PayloadFormat = payloadFormat.Value;
            }

            if (!serializer.CanDeserialize(new ContentData<byte[]>(data.ContentType, data.Payload)))
            {
                return topic;
            }

            //deserializzo il body
            IMessage obj = this.serializer.Deserialize(new ContentData<byte[]>(data.ContentType, data.Payload));
            topic.Payload = obj;

            return topic;
        }

        /// <inheritdoc/>
        public MqttMessage Unwrap(MqttMessageEnvelope msg)
        {
            MqttMessageEnvelope castmsg = msg;

            MqttMessage data = new MqttMessage();

            if(msg.Payload==null)
            {
                return msg.TransportMessage;
            }
            
            if (!serializer.CanSerialize(msg.Payload))
            {
                throw new SerializeException($@"cannot deserialize  {msg.Payload.GetType()}");
            }

            var serializedContent = this.serializer.Serialize(msg.Payload);

            data.Payload = serializedContent.Data;
            data.Topic = castmsg.Topic;
            data.Responsetopic = castmsg.Responsetopic;
            data.ContentType = serializedContent.ContentType;
            data.Qos = castmsg.Qos;
            data.Retain = castmsg.Retain;
            data.PayloadFormatIndicator = msg.FormatIndicatorPayload;
            data.Userproperties.Add(new acquleo.DataBroker.MQTT.UserPropertyInfo() { Name = nameof(msg.PayloadFormat), Value = msg.PayloadFormat });
            return data;

        }
    }
}
