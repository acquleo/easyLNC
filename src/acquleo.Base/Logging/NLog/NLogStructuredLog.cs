using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using acquleo.Base.Logger;

namespace acquleo.Base.Logging.Nlog
{
    /// <summary>
    /// Nlog implementation of an IApplicationLog
    /// </summary>
    internal class NLogStructuredLog<T> : BaseNLogLogger, IStructuredLog<T>
    {
        readonly string typefieldname;
        readonly string datafieldname;

        /// <inheritdoc />
        public NLogStructuredLog(NLog.ILogger logger, string targetName, string type, string typefieldname, string datafieldname) :base(logger,targetName, type)
        { 
            this.typefieldname = typefieldname;
            this.datafieldname = datafieldname;
        }

        public void LogIf(object caller, LogLevels level, Func<T> structure)
        {
            NLog.LogEventInfo entryLog = new NLog.LogEventInfo(base.GetNlogLevel(level), this.targetName, string.Empty);
            string dataname = typeof(T).Name;
            entryLog.Properties[this.typefieldname] = dataname;
            entryLog.Properties[this.datafieldname] = structure();
            entryLog.Properties["TetLoggerName"] = this.targetName;
            

            logger.Log(typeof(NLogApplicationLog), entryLog);
        }

        public void LogIf(object caller, LogLevels level, Func<T> structure, params Func<object>[] structures)
        {
            NLog.LogEventInfo entryLog = new NLog.LogEventInfo(base.GetNlogLevel(level), this.targetName, string.Empty);            
            entryLog.Properties["TetLoggerName"] = this.targetName;
            string dataname = typeof(T).Name;
            entryLog.Properties[this.typefieldname] = dataname;
            entryLog.Properties[this.datafieldname] = structure();
            for (int i = 0; i < structures.Length; i++)
            {
                var opt_structure = structures[i]();
                string opt_dataname = opt_structure.GetType().Name;
                if (entryLog.Properties.ContainsKey(opt_dataname))
                {
                    throw new InvalidOperationException($@"a structure of the same type cannot be used twice {opt_dataname}");
                }

                entryLog.Properties[opt_dataname] = opt_structure;
            }            

            logger.Log(typeof(NLogApplicationLog), entryLog);
        }
    }

}
