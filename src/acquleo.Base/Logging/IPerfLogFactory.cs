using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Logging
{
    /// <summary>
    /// IPerfLogFactory interface
    /// </summary>
    public interface IPerfLogFactory
    {
        /// <summary>
        /// Returns an instance of IPerfLogBuilder
        /// </summary>
        /// <returns></returns>
        IPerfLogBuilder GetBuilder();
    }
}
