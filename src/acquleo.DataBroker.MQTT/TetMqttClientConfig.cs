namespace acquleo.DataBroker.MQTT
{
    public class TetMqttClientConfig
    {
        public string clientid { get; set; } = string.Empty;
        public string host { get; set; } = "127.0.0.1";
        public int port { get; set; } = 1883;
        public int bufferSize { get; set; } = 8192;
        public TetMqttClientTlsConfig tls { get; set; } = new TetMqttClientTlsConfig();
        public TetMqttClientHealthConfig health { get; set; } = new TetMqttClientHealthConfig();
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string encoding { get; set; } = "utf-8";
    }
}