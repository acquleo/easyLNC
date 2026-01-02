namespace acquleo.Protocol
{

    /// <summary>
    /// Generic message definition
    /// </summary>
    public interface IMessageChunkResponse : IMessageResponse
    {

        /// <summary>
        /// Returns the response message number
        /// </summary>
        /// <returns></returns>
        uint GetMsgNum();

        /// <summary>
        /// Returns the total response message
        /// </summary>
        /// <returns></returns>
        uint GetMsgTot();

    }
}
