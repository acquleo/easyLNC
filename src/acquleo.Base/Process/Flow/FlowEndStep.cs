using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Process.Flow
{
    /// <summary>
    /// Flow end step
    /// </summary>
    /// <typeparam name="Tout"></typeparam>
    public class FlowEndStep<Tout> : FlowStep<Tout, Tout>
    {
        /// <summary>
        /// implementation of the flow step
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected override FlowStepData<Tout> OnProcess(Tout input)
        {
            return new FlowStepData<Tout>(FlowStepResult.Executed, input);
        }
    }
}
