using System.Collections.Generic;
using static Mysqlx.Expect.Open.Types.Condition.Types;
using System.Xml.Linq;

namespace acquleo.Protocol
{


    /// <summary>
    /// Generic message definition
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Returns the message headers
        /// </summary>
        /// <returns></returns>
        IEnumerable<Header> GetHeaders();
    }

    /// <summary>
    /// Elemento header
    /// </summary>
    public class Header
    {
        /// <summary>
        /// Chiave heade
        /// </summary>
        public string Key { get; set; } = string.Empty;
        /// <summary>
        /// Valore header
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }
}