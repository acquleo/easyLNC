using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Logging
{
    /// <summary>
    /// IApplicationLogFactory interface
    /// </summary>
    public interface IStructuredLogFactory
    {
        /// <summary>
        /// Returns an instance of IStructuredLogBuilder
        /// </summary>
        /// <returns></returns>
        IStructuredLogBuilder GetBuilder();
    }
}
