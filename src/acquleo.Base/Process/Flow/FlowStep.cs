using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Process.Flow
{
    /// <summary>
    /// Flow step
    /// </summary>
    /// <typeparam name="Tin">input data type</typeparam>
    /// <typeparam name="Tout">output data type</typeparam>
    public abstract class FlowStep<Tin, Tout> : IFlowOutput<Tout>
    {
        IFlowOutput<Tin> input;
        FlowStepData<Tin> input_data;
        
        protected abstract FlowStepData<Tout> OnProcess(Tin input);

        /// <summary>
        /// attach input to the flow step   
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public FlowStep<Tin, Tout> WithInput(IFlowOutput<Tin> input)
        {
            this.input = input;
            return this;
        }
        /// <summary>
        /// attach input to the flow step   
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public FlowStep<Tout, Tnextout> Then<Tnextout>(FlowStep<Tout, Tnextout> next)
        {
            next.WithInput(this);
            return next;
        }

        /// <summary>
        /// attach input to the flow step   
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public FlowStep<Tout, Tnextout> Then<Tnextout>(FlowStep<Tout, Tnextout> next, Action<FlowStep<Tout, Tnextout>> configure)
        {
            configure(next);
            next.WithInput(this);
            return next;
        }

        /// <summary>
        /// Execute the flow step
        /// </summary>
        /// <returns></returns>
        public FlowStepData<Tout> Execute()
        {
            input_data = input == null ? new FlowStepData<Tin>(FlowStepResult.Executed) : input.Execute();
            return InternalProcess(input_data);
        }

        FlowStepData<Tout> InternalProcess(FlowStepData<Tin> input_data)
        {
            if (input_data.Result == FlowStepResult.Aborted)
            {
                this.Rollback();

                return new FlowStepData<Tout>(FlowStepResult.Aborted);
            }

            FlowStepData<Tout> output = OnProcess(input_data.Data);


            return output;

        }

        /// <summary>
        /// execute the flow step rollback
        /// </summary>
        /// <param name="input_data"></param>
        protected virtual void OnRollback(FlowStepData<Tin> input_data) { }

        /// <summary>
        /// execute the flow step rollback  
        /// </summary>
        public void Rollback()
        {
            if (input_data == null) return;

            this.OnRollback(input_data);

            if (this.input != null)
            {
                this.input.Rollback();
            }
        }
    }
}
