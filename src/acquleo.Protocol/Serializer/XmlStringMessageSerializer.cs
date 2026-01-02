using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using acquleo.Protocol.Exceptions;

namespace acquleo.Protocol.Serializer
{
    /// <summary>
    /// xml string message serializer
    /// </summary>
    public class XmlStringMessageSerializer : IMessageSerializer<string>
    {
        readonly IXmlMessageSerializerInfo info;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public XmlStringMessageSerializer(IXmlMessageSerializerInfo info)
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
        public bool CanDeserialize(ContentData<string> data)
        {
            if (data == null) return false;
            if (data.Data == null) return false;

            return this.info.IsXmlSerializerPresent(data.ContentType);
        }

        /// <inheritdoc />
        public IMessage Deserialize(ContentData<string> data)
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
                using (var sread = new StringReader(data.Data))
                {
                    obj = serializer.Deserialize(sread) as IMessage;
                }
            }
            catch (Exception ex)
            {
                throw new DeSerializeException($@"Xml Deserialize Exception  {payload}: ", ex);

            }

            return obj;
        }
        /// <inheritdoc />
        public ContentData<string> Serialize(IMessage msg)
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
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
                {
                    // If set to true XmlWriter would close MemoryStream automatically and using would then do double dispose
                    // Code analysis does not understand that. That's why there is a suppress message.
                    CloseOutput = false,
                    Encoding = Encoding,
                    OmitXmlDeclaration = false,
                    Indent = true
                };
                using (var swrite = new StringWriterWithEncoding(this.Encoding))
                {
                    using (XmlWriter xw = XmlWriter.Create(swrite, xmlWriterSettings))
                    {
                        serializer.Serialize(xw, msg, namespaces);
                    }
                    return new ContentData<string>(serializerName, swrite.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new SerializeException($@"Xml Serialize Exception {serializerName}: ", ex);
            }
        }
    }
}
