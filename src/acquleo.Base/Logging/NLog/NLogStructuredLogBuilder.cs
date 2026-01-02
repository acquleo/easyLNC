using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace acquleo.Base.Logging.Nlog
{
    /// <summary>
    /// Nlog implementation of an ApplicationLogBuilder
    /// </summary>
    public class NLogStructuredLogBuilder : StructuredLogBuilder
    {        
        /// <summary>
        /// ctor
        /// </summary>
        public NLogStructuredLogBuilder() {

            
        }



        /// <summary>
        /// Returns the Nlog implementation of an IApplicationLog
        /// </summary>
        /// <returns></returns>
        public override IStructuredLog<T> Build<T>()
        {
            string cName = $@"{this.type}_{this.name}";
            return new NLogStructuredLog<T>(NLog.LogManager.GetLogger(cName), cName, this.type, this.typefieldname, this.datafieldname);
        }

    }
}
