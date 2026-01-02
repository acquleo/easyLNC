using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using acquleo.Base.Class;
using acquleo.Base.Logger;

namespace acquleo.Base.Logging
{
    /// <summary>
    /// IApplicationLog interface
    /// </summary>
    public class SingletonFactoryProvider : FactoryProvider
    {
        static readonly object synch=new object();
        static SingletonFactoryProvider provider;

        /// <summary>
        /// 
        /// </summary>
        static public FactoryProvider Provider
        {
            get
            {
                lock (synch)
                {
                    if (provider == null) { provider = new SingletonFactoryProvider(); }

                    return provider;
                }
            }
        }
    }
}
