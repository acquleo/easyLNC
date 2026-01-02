using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Logging
{
    /// <summary>
    /// IFlowLogFactory interface
    /// </summary>
    public interface IFlowLogFactory
    {
        /// <summary>
        /// Returns an instance of IFlowLogBuilder
        /// </summary>
        /// <returns></returns>
        IFlowLogBuilder GetBuilder();
    }
}
