using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Logging.Nlog
{
    /// <summary>
    /// Nlog implementation of IPerfLogFactory
    /// </summary>
    public class NLogStructuredLogFactory : IStructuredLogFactory
    {
        /// <summary>
        /// Returns an Nlog implementation of IStructuredLogBuilder
        /// </summary>
        /// <returns></returns>
        public IStructuredLogBuilder GetBuilder()
        {
            return new NLogStructuredLogBuilder();
        }
    }
}
