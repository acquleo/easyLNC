namespace acquleo.Protocol
{
    /// <summary>
    /// messsage serializer interface
    /// </summary>
    /// <typeparam name="Tdata"></typeparam>
    public interface IMessageSerializer<Tdata>
    {
        /// <summary>
        /// convert a message type to data type
        /// </summary>
        /// <param name="msg">Imessage</param>
        /// <returns></returns>
        ContentData<Tdata> Serialize(IMessage msg);

        /// <summary>
        /// convert a data type to message type
        /// </summary>
        /// <param name="data">trasnport data</param>
        /// <returns></returns>
        IMessage Deserialize(ContentData<Tdata> data);

        /// <summary>
        /// ritorna se è in grado di deserializare il dato
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        bool  CanDeserialize(ContentData<Tdata> data);

        /// <summary>
        /// ritorna se è in grado di serializare il messaggio
        /// </summary>
        /// convert a message type to data type
        /// <returns></returns>
        bool CanSerialize(IMessage msg);


    }
}