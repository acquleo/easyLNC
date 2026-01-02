using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Process.Flow
{
    /// <summary>
    /// Result of a flow step execution
    /// </summary>
    /// <typeparam name="Tdata"></typeparam>
    public class FlowStepData<Tdata>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public FlowStepData(FlowStepResult result)
        {
            this.Result = result;
            this.Data = default(Tdata);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="data"></param>
        public FlowStepData(FlowStepResult result, Tdata data)
        {
            this.Result = result;
            this.Data = data;
        }
        /// <summary>
        /// data result
        /// </summary>
        public Tdata Data { get; set; }
        /// <summary>
        /// execution result
        /// </summary>
        public FlowStepResult Result { get; set; }
    }
}
