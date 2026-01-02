using acquleo.DataBroker.MQTT;
using System.Collections.Generic;

namespace acquleo.Protocol.Transport.Mqtt
{
    /// <summary>
    /// define a mqtt message
    /// </summary>
    public class MqttMessage
    {
        /// <summary>
        /// retain flag
        /// </summary>
        public bool Retain { get; set; }
        /// <summary>
        /// message topic
        /// </summary>
        public string Topic { get; set; } = string.Empty;
        /// <summary>
        /// message content type
        /// </summary>
        public string ContentType { get; set; } = string.Empty;
        /// <summary>
        /// response topic
        /// </summary>
        public string Responsetopic { get; set; } = string.Empty;
        /// <summary>
        /// message payload
        /// </summary>
        public byte[] Payload { get; set; }
        /// <summary>
        /// message payload format indicator
        /// </summary>
        public byte PayloadFormatIndicator { get; set; }
        /// <summary>
        /// message quality of service
        /// </summary>
        public byte Qos { get; set; }
        /// <summary>
        /// additional user properties
        /// </summary>
        public List<UserPropertyInfo> Userproperties { get; set; } = new List<UserPropertyInfo>();
    }
}
