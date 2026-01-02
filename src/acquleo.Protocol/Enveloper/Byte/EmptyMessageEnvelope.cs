namespace acquleo.Protocol.Enveloper.Byte
{
    /// <summary>
    /// Generic message envelope definition
    /// </summary>
    public class EmptyMessageEnvelope : IMessageEnvelope
    {
        /// <summary>
        /// envelope message
        /// </summary>
        public IMessage Payload { get; set; }
    }
}
