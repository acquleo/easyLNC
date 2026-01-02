using System.IO;
using System.Text;

namespace acquleo.Protocol.Serializer
{
    /// <summary>
    /// string writer with encoding
    /// </summary>
    public class StringWriterWithEncoding : StringWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="encoding"></param>
        public StringWriterWithEncoding(Encoding encoding)
        {
            Encoding = encoding;
        }

        /// <summary>
        /// encoding
        /// </summary>
        public override Encoding Encoding { get; }
    }
}
