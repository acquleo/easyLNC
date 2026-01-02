using System;
using System.Xml.Serialization;

namespace acquleo.Protocol.Serializer
{
    /// <summary>
    /// xml message serializer info
    /// </summary>
    public interface IXmlMessageSerializerInfo
    {
        /// <summary>
        /// Ritorna il serializer del messaggio corrispondente
        /// </summary>
        /// <returns>Restituisce l'oggetto XmlSerializer.</returns>
        string GetContentType(Type type);
        /// <summary>
        /// Ritorna il serializer del messaggio corrispondente
        /// </summary>
        /// <returns>Restituisce l'oggetto XmlSerializer.</returns>
        XmlSerializer GetXmlSerializer(string contentType);
        /// <summary>
        /// Ritorna se il type è conosciuto
        /// </summary>
        /// <returns>esito.</returns>
        bool IsXmlSerializerPresent(Type type);
        /// <summary>
        /// Ritorna se il type è conosciuto
        /// </summary>
        /// <returns>esito.</returns>
        bool IsXmlSerializerPresent(string contentType);

    }
}
