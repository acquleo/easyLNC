using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using acquleo.Base.Logger;

namespace acquleo.Base.Logging
{
    /// <summary>
    /// IStructuredLog interface
    /// </summary>
    public interface IStructuredLog<T>
    {
        /// <summary>
        /// Writes a log
        /// </summary>
        /// <param name="caller">object caller (may be null)</param>
        /// <param name="level">log level</param>
        /// <param name="structure">log message</param>
        void LogIf(object caller, LogLevels level, Func<T> structure);
        /// <summary>
        /// Writes a log
        /// </summary>
        /// <param name="caller">object caller (may be null)</param>
        /// <param name="level">log level</param>
        /// <param name="structure">log message</param>
        /// <param name="structures">log message optional arguments</param>
        void LogIf(object caller, LogLevels level, Func<T> structure, params Func<object>[] structures);
    }
}
