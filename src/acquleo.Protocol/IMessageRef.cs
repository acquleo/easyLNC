namespace acquleo.Protocol
{
    /// <summary>
    /// Generic message definition that includes a message reference
    /// </summary>
    public interface IMessageRef : IMessage
    {
        /// <summary>
        /// Returns the data used as request respone reference
        /// </summary>
        /// <returns></returns>
        object GetMsgRef();


    }
}
