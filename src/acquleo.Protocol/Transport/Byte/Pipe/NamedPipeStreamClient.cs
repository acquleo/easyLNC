using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using TetSistemi.Base.Interfaces;
using TetSistemi.Base.Logging;
using acquleo.Protocol.Transport.Data.Endpoint;

namespace acquleo.Protocol.Transport.Byte.Pipe
{

    /// <summary>
    /// Client NamedPipe
    /// </summary>
    public class NamedPipeStreamClient : IDataTransport<byte[]>, IEnabler,
        IDisposable
    {
        #region Private fields
        readonly IApplicationLog logger;
        NamedPipeClientStream pipeStream;
        readonly string pipeName;
        readonly string serverName;
        bool isEnabled;
        PipeStateObject stateObj;
        readonly AsyncCallback m_pfnCallBack;
        bool isConnected;
        readonly ManualResetEvent firstConnectEvent;
        readonly AutoResetEvent threadEvent = new AutoResetEvent(false);
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverName">Server Name</param>
        /// <param name="name">Pipe Name</param>
        public NamedPipeStreamClient(string serverName, string name)
            : this(name)
        {
            this.logger = SingletonFactoryProvider.Provider.GetApplicationLogFactory().GetBuilder().WithObject(this).Build();
            firstConnectEvent = new ManualResetEvent(false);
            this.pipeName = name;
            this.serverName = serverName;
            m_pfnCallBack = new AsyncCallback(OnDataReceived);

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Pipe Name</param>
        public NamedPipeStreamClient(string name)
        {
            this.logger = SingletonFactoryProvider.Provider.GetApplicationLogFactory().GetBuilder().WithObject(this).Build();
            firstConnectEvent = new ManualResetEvent(false);
            this.pipeName = name;
            this.serverName = ".";
            m_pfnCallBack = new AsyncCallback(OnDataReceived);

        }
        #endregion

        #region IConnectionStatus

        /// <summary>
        /// Returns the connection status
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
        }

        /// <summary>
        /// Sets or gets the Check connection time
        /// </summary>
        public TimeSpan CheckConnectionTime { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Gets or sets the buffer size
        /// </summary>
        public int BufferSize { get; set; } = 1024;

        /// <summary>
        /// Gets or sets the connect timeout
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(1);
        #endregion

        #region Events
        /// <inheritdoc/>
        public event dDataReceivedAsync<byte[]> DataReceivedAsync;
        /// <inheritdoc/>
        public event dDataTransportAsync<byte[]> DataTransportAvailable;
        /// <inheritdoc/>
        public event dDataTransportAsync<byte[]> DataTransportUnavailable;
        /// <inheritdoc/>
        public event dDataTransportTraceAsync<byte[]> DataTransportTraceAsync;
        #endregion

        #region Private method
        private void CheckConnectionThread()
        {
            try
            {
                do
                {
                    if (!this.IsConnected)
                    {
                        try
                        {
                            this.InitPipe();

                            pipeStream.Connect((int)this.ConnectTimeout.TotalMilliseconds);

                            OnConnected();

                            this.WaitForData();
                        }
                        catch (Exception ex)
                        {
                            this.logger.ErrorIf(this, () => $@"error connect pipe {this.pipeName}", () => ex);
                            OnDisconnected();
                        }
                    }

                }
                while (!threadEvent.WaitOne(CheckConnectionTime));


            }
            catch (ThreadInterruptedException)
            {
                //do nothing
            }

        }

        private async void OnDataReceived(IAsyncResult asyn)
        {
            PipeStateObject theSSO = (PipeStateObject)asyn.AsyncState;

            try
            {
                //  Determina il numero di byte ricevuti.
                int iRx = theSSO.pipe.EndRead(asyn);
                if (iRx > 0)
                {
                    byte[] dataReceive = new byte[iRx];
                    Array.Copy(theSSO.buffer, dataReceive, iRx);

                    await FireOnReceive(dataReceive);

                    this.WaitForData();
                }
                else
                {
                    this.OnDisconnected();
                }
            }
            catch (Exception ex)
            {
                this.OnDisconnected();
            }
        }

        private void InitPipe()
        {
            if (pipeStream != null)
            {
                pipeStream.Dispose();
                pipeStream = null;
            }

            pipeStream = new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut,
                         PipeOptions.Asynchronous, System.Security.Principal.TokenImpersonationLevel.None,
                          System.IO.HandleInheritability.None);
            
            stateObj = new PipeStateObject(pipeStream, this.BufferSize);

        }

        private void WaitForData()
        {
            pipeStream.BeginRead(stateObj.buffer, 0, stateObj.BufferSize, m_pfnCallBack, stateObj);
        }

        private async Task FireOnReceive(byte[] _data)
        {

            if (DataReceivedAsync != null)
            {
                //  Esegue la comunicazione dell'evento attraverso l'apposito delegate.
                await Task.WhenAll(Array.ConvertAll(
                         DataReceivedAsync.GetInvocationList(),
                         e => ((dDataReceivedAsync<byte[]>)e)(this, _data, GetRemoteEndpoint()))).ConfigureAwait(false);

            }
        }

        private async void OnDisconnected()
        {
            if (!isConnected) return;

            this.logger.WarningIf(this, () => $@"pipe {this.pipeName} is disconnected");

            isConnected = false;

            firstConnectEvent.Reset();

            if (DataTransportUnavailable != null)
            {
                //  Esegue la comunicazione dell'evento attraverso l'apposito delegate.
                await Task.WhenAll(Array.ConvertAll(
                         DataTransportUnavailable.GetInvocationList(),
                         e => ((dDataTransportAsync<byte[]>)e)(this))).ConfigureAwait(false);

            }
        }

        private async void OnConnected()
        {
            if (isConnected) return;

            this.logger.InfoIf(this, () => $@"pipe {this.pipeName} is connected");

            isConnected = true;

            firstConnectEvent.Set();

            if (DataTransportAvailable != null)
            {
                //  Esegue la comunicazione dell'evento attraverso l'apposito delegate.
                await Task.WhenAll(Array.ConvertAll(
                         DataTransportAvailable.GetInvocationList(),
                         e => ((dDataTransportAsync<byte[]>)e)(this))).ConfigureAwait(false);

            }
        }

        #endregion

        #region IEnabler
        /// <summary>
        /// NOT USED
        /// </summary>
        public void Initialize()
        {
            // non server 
        }

        /// <summary>
        /// Disable the server
        /// </summary>
        public void Disable()
        {
            if (!isEnabled) return;

            this.logger.InfoIf(this, () => $@"disable pipe {this.pipeName}");

            isEnabled = false;

            threadEvent.Set();

            pipeStream.Dispose();

        }
        /// <summary>
        /// Enable the server
        /// </summary>
        public void Enable()
        {
            if (isEnabled) return;

            this.logger.InfoIf(this, () => $@"enable pipe {this.pipeName}");

            isEnabled = true;
            
            var checkConnectionThread = new Thread(new ThreadStart(CheckConnectionThread));
            checkConnectionThread.Start();

        }
        /// <summary>
        /// Returns the enable status
        /// </summary>
        /// <returns></returns>
        public bool IsEnabled()
        {
            return isEnabled;
        }
        /// <summary>
        /// Disapose the server
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            this.Disable();
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Returns if the stream is available
        /// </summary>
        public bool Available
        {
            get
            {
                return this.IsConnected;
            }
        }
        /// <summary>
        /// Wait for a connected status
        /// </summary>
        /// <param name="connectTimeout"></param>
        /// <returns></returns>
        public bool WaitForConnected(TimeSpan connectTimeout)
        {
            return firstConnectEvent.WaitOne(connectTimeout);
        }
        /// <inheritdoc/>
        public async Task<bool> SendAsync(byte[] data)
        {
            if (!isConnected) return false;

            await pipeStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);

            return false;
        }
        /// <inheritdoc/>
        public Task<bool> SendAsync(byte[] data, DataEndpoint to)
        {
            return SendAsync(data);
        }

        NamedPipeDataEndpoint endpoint;
        /// <inheritdoc/>
        public DataEndpoint GetRemoteEndpoint()
        {
            if (endpoint == null)
                endpoint = new NamedPipeDataEndpoint()
                {
                    name = this.pipeName,
                    server = this.serverName
                };

            return endpoint;
        }

        #endregion

        #region Classe interna
        //  Questa classe serve a generare l'oggetto che contiene le informazioni circa 
        //  l'operazione asincrona di ricezione.
        internal class PipeStateObject
        {
            //  Client socket.
            public PipeStream pipe;
            //  Size of receive buffer.
            public int BufferSize;
            //  Receive buffer.
            public byte[] buffer;

            public PipeStateObject(PipeStream _pipe, int _bufSize)
            {
                pipe = _pipe; //  Serve per riconoscere il giusto socket quando ci sono più client.
                BufferSize = _bufSize;
                buffer = new byte[BufferSize];
            }
        }
        #endregion
    }
}
