using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Process.Flow
{
    /// <summary>
    /// Interface for flow output   
    /// </summary>
    /// <typeparam name="Tout"></typeparam>
    public interface IFlowOutput<Tout>
    {
        /// <summary>
        /// Execute the flow step
        /// </summary>
        /// <returns></returns>
        FlowStepData<Tout> Execute();
        /// <summary>
        /// Rollback the flow step
        /// </summary>  
        void Rollback();
    }

    /// <summary>
    /// Interface for flow output   
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    public interface IFlowInput<Tin>
    {

    }
}
