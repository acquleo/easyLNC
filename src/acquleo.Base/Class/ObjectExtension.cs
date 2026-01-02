using System;
using System.Collections.Generic;
using System.Text;

namespace acquleo.Base.Class
{
    /// <summary>
    /// Extensions of the type object
    /// </summary>
    public static class ObjectExtension
    {
        /// <summary>
        /// verify if the object is of type Tclass
        /// </summary>
        /// <typeparam name="Tclass">Cast Type</typeparam>
        /// <param name="msg">extended object</param>
        /// <returns></returns>
        public static bool Is<Tclass>(this object msg)
        {
            return msg is Tclass;
        }

        /// <summary>
        /// cast the object to Tclass
        /// </summary>
        /// <typeparam name="Tclass">Cast Type</typeparam>
        /// <param name="msg">extended object</param>
        /// <returns></returns>
        public static Tclass As<Tclass>(this object msg)
        {
            return (Tclass)msg;
        }
    }
}
