using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Logging.Nlog
{
    /// <summary>
    /// Nlog implementation of IFlowLogFactory
    /// </summary>
    public class NLogFlowLogFactory : IFlowLogFactory
    {
        /// <summary>
        /// Returns an Nlog implementation of IFlowLogBuilder
        /// </summary>
        /// <returns></returns>
        public IFlowLogBuilder GetBuilder()
        {
            return new NLogFlowLogBuilder();
        }
    }
}
