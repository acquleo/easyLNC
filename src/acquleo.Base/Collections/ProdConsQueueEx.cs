// ------------------------------------------------------------------------
//Società: T&TSistemi s.r.l.
//Anno: 2008 
//Progetto: AMIL5
//Autore: Acquisti Leonardo
//Nome modulo software: TetSistemi.Commons.dll
//Data ultima modifica: $LastChangedDate: 2015-12-14 15:47:15 +0100 (lun, 14 dic 2015) $
//Versione: $Rev: 425 $
// ------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace acquleo.Base.Collections
{
    /// <summary>
    /// Implementa una coda con produttori multipli e singolo consumatore
    /// </summary>
    public class ProdConsQueueEx : IDisposable
    {
        #region Delegate
        /// <summary>
        /// Delega la ricezione dello scodamento di un oggetto dalla coda
        /// </summary>
        /// <param name="obj">Oggetto dalla coda.</param>
        public delegate void dObjectDequeue(object obj);
        #endregion

        #region Variable Declaration
        private System.Collections.Queue queue;
        private int max_el = 0;
        bool dequeue_ris = false;
        bool enqueue_ris = false;
        Semaphore m_Semaphore;
        Thread mConsumerThread;
        bool mRunning = false;
        dObjectDequeue mCallback;
        #endregion

        #region Constructor
        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="_max_el">Numero massimo di elementi sulla coda</param>
        /// <param name="callback">Callback di ricezione degli elementi scodati</param>
        public ProdConsQueueEx(int _max_el, dObjectDequeue callback)
        {
            mCallback = callback;
            max_el = _max_el;
            m_Semaphore = new Semaphore(0, max_el);
            queue = new System.Collections.Queue(_max_el);
            //Inizializzo il Consumatore
            mRunning = true;
            mConsumerThread = new Thread(new ThreadStart(ConsumerThread));
            mConsumerThread.Name = this.GetType().Name;
            mConsumerThread.Start();
        }
        #endregion

        #region Public Method
        /// <summary>
        /// Inserisce un oggetto sulla coda
        /// </summary>
        /// <param name="obj">Oggetto da inserire sulla coda</param>
        /// <returns>Ritorna l'esito dell'operazione</returns>
        public bool Enqueue(object obj)
        {

            try
            {
                if (mRunning)
                {
                    lock (this)
                    {
                        enqueue_ris = false;
                        if (queue.Count <= max_el)
                        {
                            queue.Enqueue(obj);
                            enqueue_ris = true;

                            m_Semaphore.Release();
                        }
                        else
                        {
                            //Coda piena
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {

            }
            return enqueue_ris;
        }

        /// <summary>
        /// Remove all items from the queue
        /// </summary>
        public void Clear()
        {
            queue.Clear();
        }

        #endregion

        #region Private Method

        private bool Dequeue(ref object obj)
        {

            try
            {               
                m_Semaphore.WaitOne();


                lock (this)
                {
                    dequeue_ris = false;
                    if (queue.Count > 0)
                    {
                        obj = queue.Dequeue();
                        dequeue_ris = true;
                    }
                    else
                    {
                        //Coda vuota
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                //Libera la risorsa
            }
            return dequeue_ris;
        }

        void ConsumerThread()
        {
            while (mRunning)
            {
                object obj = null;
                if (this.Dequeue(ref obj))
                {
                    if (obj != null)
                    {
                        if (this.mCallback != null)
                        {
                            this.mCallback(obj);
                        }
                    }
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Ritorna se la coda è vuota
        /// </summary>
        public bool IsEmpty
        {
            get { return this.queue.Count == 0; }
        }

        /// <summary>
        /// Ritorna il numero di elementi in coda
        /// </summary>
        public int QueueLength
        {
            get
            {
                return this.queue.Count;
            }
        }

        /// <summary>
        /// Ritorna il numero massimo di elementi in coda
        /// </summary>
        public int Capacity
        {
            get
            {
                return this.max_el;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Libera le risorse
        /// </summary>
        public void Dispose()
        {
            //Faccio in modo che esca dal ciclo
            mRunning = false;
            //Pulisco la coda
            queue.Clear();
            //rilascio il semaforo
            m_Semaphore.Release();
            //chiudo il semaforo
            m_Semaphore.Close();
            //aspetto il termine del Thread
            this.mConsumerThread.Join();

        }

        #endregion
    }
}
