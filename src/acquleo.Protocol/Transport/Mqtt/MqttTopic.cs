namespace acquleo.Protocol.Transport.Mqtt
{
    /// <summary>
    /// define a mqtt topic
    /// </summary>
    public class MqttTopic
    {
        /// <summary>
        /// mqtt topic
        /// </summary>
        public string Topic { get; set; } = string.Empty;
        /// <summary>
        /// message quality of service
        /// </summary>
        public byte Qos { get; set; }
    }
}
