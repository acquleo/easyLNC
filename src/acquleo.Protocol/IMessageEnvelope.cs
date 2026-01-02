namespace acquleo.Protocol
{
    /// <summary>
    /// Generic message envelope definition
    /// </summary>
    public interface IMessageEnvelope
    {
        /// <summary>
        /// Envelope payload
        /// </summary>
        IMessage Payload { get; }
    }
}
