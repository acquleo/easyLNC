using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Logging.Nlog
{
    /// <summary>
    /// Nlog implementation of IPerfLogFactory
    /// </summary>
    public class NLogPerfLogFactory : IPerfLogFactory
    {
        /// <summary>
        /// Returns an Nlog implementation of IPerfLogBuilder
        /// </summary>
        /// <returns></returns>
        public IPerfLogBuilder GetBuilder()
        {
            return new NLogPerfLogBuilder();
        }
    }
}
