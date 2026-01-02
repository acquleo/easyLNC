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
    internal class NLogApplicationLog : BaseNLogLogger, IApplicationLog
    {
        /// <inheritdoc />
        public NLogApplicationLog(NLog.ILogger logger, string targetName, string type) :base(logger,targetName, type)
        { 

        }

        /// <inheritdoc />
        public void Debug(object caller, string message)
        {
            this.Log( LogLevels.Debug, caller, message);
        }

        /// <inheritdoc />
        public void Error(object caller, string message, Exception exception = null)
        {
            this.Log(LogLevels.Error, caller, message, exception);
        }

        /// <inheritdoc />
        public void Fatal(object caller, string message, Exception exception = null)
        {
            this.Log(LogLevels.Fatal, caller, message, exception);
        }

        /// <inheritdoc />
        public void Info(object caller, string message)
        {
            this.Log(LogLevels.Info, caller, message);
        }

        /// <inheritdoc />
        public void Trace(object caller, string message)
        {
            this.Log(LogLevels.Trace, caller, message);
        }

        /// <inheritdoc />
        public void Warning(object caller, string message, Exception exception = null)
        {
            this.Log(LogLevels.Warning, caller, message, exception);
        }

        /// <inheritdoc />
        public void Log(LogLevels level, object caller, string message, Exception exception = null)
        {
            NLog.LogEventInfo entryLog = new NLog.LogEventInfo(base.GetNlogLevel(level), this.targetName, message);
            //entryLog.
            entryLog.Exception = exception;

            logger.Log(typeof(NLogApplicationLog),entryLog);
        }


        /// <inheritdoc />
        public void Log(LogLevels level, string message, Exception exception = null)
        {
            this.Log(level, null, message, exception);
        }

        /// <inheritdoc />
        public void LogIf(LogLevels level, Func<string> message, Func<Exception> exception = null)
        {
            if (!CanLog(level)) return;

            this.Log(level, message(), exception());
        }

        /// <inheritdoc />
        public void LogIf(LogLevels level, object caller, Func<string> message, Func<Exception> exception = null)
        {
            if (!CanLog(level)) return;

            this.Log(level, caller, message(), exception());
        }
        /// <inheritdoc />
        public bool CanLog(LogLevels level)
        {
            return logger.IsEnabled(base.GetNlogLevel(level));
        }

        /// <inheritdoc />
        public void InfoIf(object caller, Func<string> message)
        {
            if (CanLog(LogLevels.Info))
            {
                var log = message();
                Info(caller, log);
            }
        }

        /// <inheritdoc />
        public void ErrorIf(object caller, Func<string> message, Func<Exception> exception = null)
        {
            if (CanLog(LogLevels.Error))
            {
                var log = message();
                var ex = exception?.Invoke();
                Error(caller, log, ex);
            }
        }

        /// <inheritdoc />
        public void WarningIf(object caller, Func<string> message, Func<Exception> exception = null)
        {
            if (CanLog(LogLevels.Warning))
            {
                var log = message();
                var ex = exception?.Invoke();
                Warning(caller, log, ex);
            }
        }

        /// <inheritdoc />
        public void FatalIf(object caller, Func<string> message, Func<Exception> exception = null)
        {
            if (CanLog(LogLevels.Fatal))
            {
                var log = message();
                var ex = exception?.Invoke();
                Fatal(caller, log, ex);
            }
        }

        /// <inheritdoc />
        public void DebugIf(object caller, Func<string> message)
        {
            if (CanLog(LogLevels.Debug))
            {
                var log = message();
                Debug(caller, log);
            }
        }

        /// <inheritdoc />
        public void TraceIf(object caller, Func<string> message)
        {
            if (CanLog(LogLevels.Trace))
            {
                var log = message();
                Trace(caller, log);
            }
        }
    }
}
