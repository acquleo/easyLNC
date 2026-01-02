using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading;

namespace acquleo.Base.Threading
{
    /// <summary>
    /// Implementa un esecutore di JOB con sincronizzazione su un oggetto
    /// Se non si utilizza un oggetto che viene comparato per valore va effettuato override di Equals e GetHashCode
    /// Oppure utilizzare il costruttore e fornire una calsse che implementa IEqualityComparer<typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="A">Tipo dell'oggetto argomento di input del job</typeparam>
    /// <typeparam name="R">Tipo dell'oggetto argomento di output del job</typeparam>
    public class GlobalSemaphoreJob<A,R> : CriticalFinalizerObject where A : class
    {
        Semaphore currentSemaphore;
        readonly object syncObj = new object();

        /// <summary>
        /// 
        /// </summary>
        public GlobalSemaphoreJob()
        {
            
        }
        
        /// <summary>
        /// Esegue il job
        /// </summary>
        /// <param name="obj">Oggetto di sincronizzazione</param>
        /// <param name="job">Job</param>
        /// <param name="arg">Argomento del Job</param>
        public R Execute(string obj, Func<A,R> job, A arg)
        {
            checkInvalidChar(obj);

            string sync_key = $@"GlobalSemaphoreJob_{obj}";

            lock (syncObj)
            {
                currentSemaphore = new Semaphore(1, 1, sync_key);
            }

            while (!currentSemaphore.WaitOne(1000)) 
            { 
                Thread.Sleep(1);
            }

            try
            {
                return job(arg);
            }
            finally
            {
                currentSemaphore.Release();

                lock (syncObj)
                {
                    currentSemaphore = null;
                }
            }

        }
        void checkInvalidChar(string id)
        {            
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                if (id.Contains(c.ToString())) throw new ArgumentException($@"Invalid character found {c}");
            }
        }
    }
}
