namespace acquleo.DataBroker.MQTT
{
    public class TetMqttClientTlsConfig
    {
        public bool useTls { get; set; } = false;
        public bool allowUntrustedCertificates { get; set; } = false;
        public bool ignoreCertificateChainErrors { get; set; } = false;
        public bool ignoreCertificateRevocationErrors { get; set; } = false;
        public bool disableCertificateValidation { get; set; } = false;
        public string certificateHostName { get; set; } = string.Empty;
    }
}