using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using acquleo.Base.Class;
using acquleo.Base.Logger;
using acquleo.Base.Logging.Nlog;

namespace acquleo.Base.Logging
{
    /// <summary>
    /// IApplicationLog interface
    /// </summary>
    public class FactoryProvider: ILogFactoryProvider
    {
        IApplicationLogFactory applicationFactory;
        IStructuredLogFactory structuredFactory;
        IFlowLogFactory flowFactory;
        ITraceLogFactory traceFactory;
        IPerfLogFactory perfFactory;
        bool applicationFactoryFirstSet=false;
        bool structuredFactoryFirstSet = false;
        bool flowFactoryFirstSet = false;
        bool traceFactoryFirstSet = false;
        bool perfFactoryFirstSet = false;
        /// <summary>
        /// 
        /// </summary>
        public FactoryProvider()
        {
            //default vengono utilizzati i provider NLOG
            applicationFactory = new NLogApplicationLogFactory();
            flowFactory = new NLogFlowLogFactory();
            traceFactory = new NLogTraceLogFactory();
            perfFactory = new NLogPerfLogFactory();
            structuredFactory = new NLogStructuredLogFactory();
        }

        public void SetStructuredLogFactory(IStructuredLogFactory structuredFactory)
        {
            if (structuredFactory == null) return;

            if (this.structuredFactory != null && structuredFactoryFirstSet) throw new InvalidOperationException("factory cannot be changed");

            structuredFactoryFirstSet= true;

            this.structuredFactory = structuredFactory;
        }

        public void SetPerfLogFactory(IPerfLogFactory perfFactory)
        {
            if (perfFactory == null) return;

            if (this.perfFactory != null && perfFactoryFirstSet) throw new InvalidOperationException("factory cannot changed");

            perfFactoryFirstSet = true;

            this.perfFactory = perfFactory;
        }

        public void SetApplicationLogFactory(IApplicationLogFactory applicationFactory)
        {
            if (applicationFactory == null) return;

            if (this.applicationFactory != null && applicationFactoryFirstSet) throw new InvalidOperationException("factory cannot changed");

            applicationFactoryFirstSet = true;

            this.applicationFactory = applicationFactory;
        }

        public void SetFlowLogFactory(IFlowLogFactory flowFactory)
        {
            if (flowFactory == null) return;

            if (this.flowFactory != null && flowFactoryFirstSet) throw new InvalidOperationException("factory cannot changed");

            flowFactoryFirstSet=true;

            this.flowFactory = flowFactory;
        }

        public void SetTraceLogFactory(ITraceLogFactory traceFactory)
        {
            if (traceFactory == null) return;

            if (this.traceFactory != null && traceFactoryFirstSet) throw new InvalidOperationException("factory cannot changed");

            traceFactoryFirstSet=true;

            this.traceFactory = traceFactory;
        }

        public IStructuredLogFactory GetStructuredLogFactory()
        {
            return this.structuredFactory;
        }

        public IPerfLogFactory GetPerfLogFactory()
        {
            return this.perfFactory;
        }

        public IApplicationLogFactory GetApplicationLogFactory()
        {
            return this.applicationFactory;
        }

        public IFlowLogFactory GetFlowLogFactory()
        {
            return this.flowFactory;
        }

        public ITraceLogFactory GetTraceLogFactory()
        {
            return this.traceFactory;
        }
    }
}
