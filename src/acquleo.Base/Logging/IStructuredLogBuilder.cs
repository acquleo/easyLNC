using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Logging
{
    /// <summary>
    /// IApplicationLogBuilder interface
    /// </summary>
    public interface IStructuredLogBuilder
    {
        /// <summary>
        /// Returns the IApplicationLog
        /// </summary>
        /// <returns></returns>
        IStructuredLog<T> Build<T>();
        /// <summary>
        /// Set a custom logger Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        IStructuredLogBuilder WithCustomName(string name);
        /// <summary>
        /// Set the class of the object using the logger
        /// </summary>
        /// <param name="clientType"></param>
        /// <returns></returns>
        IStructuredLogBuilder WithClass(Type clientType);
        /// <summary>
        /// Set the object using the logger
        /// </summary>
        /// <param name="clientObject"></param>
        /// <returns></returns>
        IStructuredLogBuilder WithObject(object clientObject);
    }
}
