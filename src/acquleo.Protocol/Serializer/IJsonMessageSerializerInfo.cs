using System;

namespace acquleo.Protocol.Serializer
{
    /// <summary>
    /// Definisce l'interfaccia che restituisce un serializer xml a partire dal tipo di messaggio
    /// </summary>
    public interface IJsonMessageSerializerInfo
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
        Type GetJsonType(string contentType);
        /// <summary>
        /// Ritorna se il content type è compatibile
        /// </summary>
        /// <returns>esito.</returns>
        bool IsJsonTypePresent(string contentType);
        /// <summary>
        /// Ritorna se il type è compatibile
        /// </summary>
        /// <returns>esito.</returns>
        bool IsJsonTypePresent(Type type);
    }
}
