using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Logging
{
    /// <summary>
    /// IApplicationLogFactory interface
    /// </summary>
    public interface IApplicationLogFactory
    {
        /// <summary>
        /// Returns an instance of IApplicationLogBuilder
        /// </summary>
        /// <returns></returns>
        IApplicationLogBuilder GetBuilder();
    }
}
