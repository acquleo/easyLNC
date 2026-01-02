using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using acquleo.Protocol.Exceptions;

namespace acquleo.Protocol.Serializer
{
    /// <summary>
    /// Json Byte Message Serializer
    /// </summary>
    public class JsonByteMessageSerializer : IMessageSerializer<byte[]>
    {
        readonly JsonSerializer serializer = new JsonSerializer();
        readonly IJsonMessageSerializerInfo info;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public JsonByteMessageSerializer(IJsonMessageSerializerInfo info)
        {
            this.info = info;
            this.Encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Returns or Sets the XML encoding
        /// </summary>
        public Encoding Encoding
        {
            get;
            set;
        }

        /// <inheritdoc/>
        public bool CanSerialize(IMessage msg)
        {
            if(msg==null) return false;

            return this.info.IsJsonTypePresent(msg.GetType());        }

        /// <inheritdoc/>
        public bool CanDeserialize(ContentData<byte[]> data)
        {
            if (data == null) return false;
            if (data.Data == null) return false;

            return this.info.IsJsonTypePresent(data.ContentType);
        }

        /// <inheritdoc/>
        public IMessage Deserialize(ContentData<byte[]> data)
        {
            //deserializzo l'XML
            IMessage obj = null;

            var payload = data.ContentType;
            if (payload == null)
            {
                throw new SerializerNotFoundException($@"Serializer not found: {data.ContentType}");
            }
            var contract = this.info.GetJsonType(payload);

            try
            {
                using (var mread = new MemoryStream(data.Data))
                {
                    using (var sread = new StreamReader(mread, this.Encoding, true))
                    {
                        using (var jread = new JsonTextReader(sread))
                        {
                            obj = serializer.Deserialize(jread, contract) as IMessage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DeSerializeException($@"Xml Deserialize Exception  {data.ContentType}: ", ex);

            }

            return obj;
        }
        /// <inheritdoc/>
        public ContentData<byte[]> Serialize(IMessage msg)
        {
            string serializerName = info.GetContentType(msg.GetType());
            Type type = info.GetJsonType(serializerName);
            if (type == null)
            {
                throw new SerializerNotFoundException($@"Serializer not found: {serializerName}");
            }

            try
            {
                using (var mwrite = new MemoryStream())
                {
                    using (var swrite = new StreamWriter(mwrite))
                    {

                        using (var jwrite = new JsonTextWriter(swrite))
                        {
                            this.serializer.Serialize(jwrite, msg, type);
                            jwrite.Flush();
                            return new ContentData<byte[]>(serializerName, mwrite.ToArray());
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializeException($@"Xml Serialize Exception {serializerName}: ", ex);
            }
        }
    }
}
