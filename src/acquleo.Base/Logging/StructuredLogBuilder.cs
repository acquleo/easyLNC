using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace acquleo.Base.Logging.Nlog
{
    /// <summary>
    /// IApplicationLogBuilder implementation
    /// </summary>
    public abstract class StructuredLogBuilder : IStructuredLogBuilder
    {
        internal const string TYPE = "STRUCT";

        readonly internal string type;
        internal string name = string.Empty;

        protected string typefieldname = "type";
        protected string datafieldname = "value";

        /// <summary>
        /// CTOR
        /// </summary>
        protected StructuredLogBuilder() {
            this.type = TYPE;
        }

        /// <summary>
        /// Returns the IApplicationLog
        /// </summary>
        /// <returns></returns>
        public abstract IStructuredLog<T> Build<T>();

        /// <summary>
        /// Set the json field name for data type
        /// </summary>
        /// <param name="typefieldname"></param>
        /// <returns></returns>
        public IStructuredLogBuilder WithTypeFieldName(string typefieldname)
        {
            this.typefieldname= typefieldname;
            return this;
        }

        /// <summary>
        /// Set the json field name for data
        /// </summary>
        /// <param name="datafieldname"></param>
        /// <returns></returns>
        public IStructuredLogBuilder WithDataFieldName(string datafieldname)
        {
            this.datafieldname = datafieldname;
            return this;
        }


        /// <summary>
        /// Set the class of the object using the logger
        /// </summary>
        /// <param name="clientType"></param>
        /// <returns></returns>
        public IStructuredLogBuilder WithClass(Type clientType)
        {
            this.name= clientType.FullName;
            return this;
        }

        /// <summary>
        /// Set a custom logger Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IStructuredLogBuilder WithCustomName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            this.name = name;
            return this;
        }

        /// <summary>
        /// Set the object using the logger
        /// </summary>
        /// <param name="clientObject"></param>
        /// <returns></returns>
        public IStructuredLogBuilder WithObject(object clientObject)
        {
            return this.WithClass(clientObject.GetType());
        }
    }
}
