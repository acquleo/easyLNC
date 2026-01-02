using System;
using System.Runtime.Serialization;

namespace acquleo.Protocol.Exceptions
{
    /// <summary>
    /// base protocol exception
    /// </summary>
    [Serializable]
    public abstract class ProtocolException : Exception
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected ProtocolException() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ProtocolException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        protected ProtocolException(string message) : base(message) { }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerexception"></param>
        protected ProtocolException(string message, Exception innerexception) : base(message, innerexception) { }

    }

    /// <summary>
    /// ArgumentProtocolException
    /// </summary>
    [Serializable]
    public class ArgumentProtocolException : ProtocolException
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected ArgumentProtocolException() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected ArgumentProtocolException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ArgumentProtocolException(string message) : base(message) { }
    }

    /// <summary>
    /// InternalProtocolException
    /// </summary>
    [Serializable]
    public class InternalProtocolException : ProtocolException
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected InternalProtocolException() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected InternalProtocolException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerexception"></param>
        public InternalProtocolException(string message, Exception innerexception) : base(message, innerexception) { }
    }

    /// <summary>
    /// SerializerNotFoundException
    /// </summary>
    [Serializable]
    public class SerializerNotFoundException : ProtocolException
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected SerializerNotFoundException() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected SerializerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public SerializerNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// SerializeException
    /// </summary>
    [Serializable]
    public class SerializeException : ProtocolException
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected SerializeException() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected SerializeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerexception"></param>
        public SerializeException(string message, Exception innerexception) : base(message, innerexception) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public SerializeException(string message) : base(message) { }
    }

    /// <summary>
    /// DeSerializeException
    /// </summary>
    [Serializable]
    public class DeSerializeException : ProtocolException
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected DeSerializeException() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected DeSerializeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerexception"></param>
        public DeSerializeException(string message, Exception innerexception) : base(message, innerexception) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public DeSerializeException(string message) : base(message) { }
    }

}
