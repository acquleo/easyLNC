using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using acquleo.Base.Logger;

namespace acquleo.Base.Logging
{
    /// <summary>
    /// ILogFactoryProvider interface.
    /// </summary>
    public interface ILogFactoryProvider
    {

        IApplicationLogFactory GetApplicationLogFactory();

        IFlowLogFactory GetFlowLogFactory();

        ITraceLogFactory GetTraceLogFactory();

        IPerfLogFactory GetPerfLogFactory();
    }
}
