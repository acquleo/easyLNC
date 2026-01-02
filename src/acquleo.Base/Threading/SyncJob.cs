using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace acquleo.Base.Threading
{
    /// <summary>
    /// Implementa un esecutore di JOB con sincronizzazione su un oggetto
    /// Se non si utilizza un oggetto che viene comparato per valore va effettuato override di Equals e GetHashCode
    /// Oppure utilizzare il costruttore e fornire una calsse che implementa IEqualityComparer<typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Tipo dell'oggetto di sincronizzazione</typeparam>
    /// <typeparam name="A">Tipo dell'oggetto argomento di input del job</typeparam>
    /// <typeparam name="R">Tipo dell'oggetto argomento di output del job</typeparam>
    public class SyncJob<T,A,R> where T : class where A : class
    {
        readonly object _lock = new object();
        readonly ConcurrentDictionary<T, object> _objectMap;

        /// <summary>
        /// 
        /// </summary>
        public SyncJob()
        {
            _objectMap = new ConcurrentDictionary<T, object>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comparer"></param>
        public SyncJob(IEqualityComparer<T> comparer)
        {
            _objectMap = new ConcurrentDictionary<T, object>(comparer);
        }

        /// <summary>
        /// Esegue il job
        /// </summary>
        /// <param name="obj">Oggetto di sincronizzazione</param>
        /// <param name="job">Job</param>
        /// <param name="arg">Argomento del Job</param>
        public R Execute(T obj, Func<A,R> job, A arg)
        {
            try
            {
                object obj_sync = null;
                lock (_lock)
                {
                    obj_sync = _objectMap.GetOrAdd(obj, new object());
                }
                lock (obj_sync)
                {
                    return job(arg);
                }
            }
            finally
            {
                lock (_lock)
                {
                    object found;
                    _objectMap.TryRemove(obj, out found);
                }
            }

        }
    }
}
