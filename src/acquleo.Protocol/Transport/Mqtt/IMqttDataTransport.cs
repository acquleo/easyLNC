using System.Collections.Generic;

namespace acquleo.Protocol.Transport.Mqtt
{
    /// <summary>
    /// defines a mqtt data transport
    /// </summary>
    public interface IMqttDataTransport : IPubSubDataTransport<MqttMessage, List<MqttTopic>>
    {

    }
}
