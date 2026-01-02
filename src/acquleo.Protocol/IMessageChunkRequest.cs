namespace acquleo.Protocol
{
    /// <summary>
    /// types of chunk request
    /// </summary>
    public enum ChunkTypes
    {
        /// <summary>
        /// chunk request disabled
        /// </summary>
        Disabled = 0,
        /// <summary>
        /// chunk request by service
        /// </summary>
        ByService = 1,
        /// <summary>
        /// chunk request by element
        /// </summary>
        ElementPerMessage = 2,
        /// <summary>
        /// chunk request by number of message
        /// </summary>
        NumOfMessage = 3,
    }

    /// <summary>
    /// Generic message definition
    /// </summary>
    public interface IMessageChunkRequest : IMessageRequest
    {

        /// <summary>
        /// Returns che request chunk type
        /// </summary>
        /// <returns></returns>
        ChunkTypes GetChunkType();

        /// <summary>
        /// Returns the request chunk size
        /// </summary>
        /// <returns></returns>
        uint GetChunkSize();

    }
}
