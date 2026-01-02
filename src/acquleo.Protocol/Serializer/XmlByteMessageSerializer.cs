using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using acquleo.Protocol.Exceptions;

namespace acquleo.Protocol.Serializer
{
    /// <summary>
    /// xml byte message serializer
    /// </summary>
    public class XmlByteMessageSerializer : IMessageSerializer<byte[]>
    {
        readonly IXmlMessageSerializerInfo info;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public XmlByteMessageSerializer(IXmlMessageSerializerInfo info)
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
            if (msg == null) return false;

            return this.info.IsXmlSerializerPresent(msg.GetType());
        }

        /// <inheritdoc/>
        public bool CanDeserialize(ContentData<byte[]> data)
        {
            if (data == null) return false;
            if (data.Data == null) return false;

            return this.info.IsXmlSerializerPresent(data.ContentType);
        }

        /// <inheritdoc />
        public IMessage Deserialize(ContentData<byte[]> data)
        {
            //deserializzo l'XML
            IMessage obj = null;

            var payload = data.ContentType;

            var serializer = this.info.GetXmlSerializer(payload);
            if (serializer == null)
            {
                throw new SerializerNotFoundException($@"Serializer not found: {payload}");
            }

            try
            {
                using (var mread = new MemoryStream(data.Data))
                {
                    using (var sread = new StreamReader(mread, this.Encoding, true))
                    {
                        obj = serializer.Deserialize(sread) as IMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DeSerializeException($@"Xml Deserialize Exception  {payload}: ", ex);

            }

            return obj;
        }
        /// <inheritdoc />
        public ContentData<byte[]> Serialize(IMessage msg)
        {
            string serializerName = this.info.GetContentType(msg.GetType());

            //rimuovo il namespace dall'xml
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            //ricavo il serializer
            XmlSerializer serializer = info.GetXmlSerializer(serializerName);
            if (serializer == null)
            {
                throw new SerializerNotFoundException($@"Serializer not found: {serializerName}");
            }

            try
            {
                using (var mwrite = new MemoryStream())
                {
                    XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
                    {
                        // If set to true XmlWriter would close MemoryStream automatically and using would then do double dispose
                        // Code analysis does not understand that. That's why there is a suppress message.
                        CloseOutput = false,
                        Encoding = Encoding,
                        OmitXmlDeclaration = false,
                        Indent = true
                    };
                    using (XmlWriter xw = XmlWriter.Create(mwrite, xmlWriterSettings))
                    {
                        serializer.Serialize(xw, msg, namespaces);
                    }

                    return new ContentData<byte[]>(serializerName, mwrite.ToArray());
                }
            }
            catch (Exception ex)
            {
                throw new SerializeException($@"Xml Serialize Exception {serializerName}: ", ex);
            }
        }
    }
}
