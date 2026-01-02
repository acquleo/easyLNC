using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using tet.Protocol.Transport.Byte.Pipe;
using TetSistemi.Base.Interfaces;
using TetSistemi.Base.Logging;
using acquleo.Protocol.Transport.Data.Endpoint;

namespace acquleo.Protocol.Transport.Byte.Pipe
{

    /// <summary>
    /// Server NamedPipe
    /// </summary>
    public class NamedPipeStreamServer : IDataTransport<byte[]>, IEnabler,
        IDisposable
    {
        #region Private fields
        readonly IApplicationLog logger;
        readonly string pipeName;
        PipeStateObject stateObj;
        readonly AsyncCallback m_connCallBack;
        bool isEnabled;
        bool shutdown;
        readonly List<NamedPipeStreamServerConn> streams = new List<NamedPipeStreamServerConn>();
        readonly object m_lock = new object();
        NamedPipeServerStream waitConnection = null;
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Pipe Name</param>
        public NamedPipeStreamServer(string name)
        {
            

            this.logger = SingletonFactoryProvider.Provider.GetApplicationLogFactory().GetBuilder().WithObject(this).Build();
            this.pipeName = name;
            this.m_connCallBack = new AsyncCallback(OnConnectReceived);
        }
        #endregion

        #region IConnectionStatus
        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected
        {
            get
            {
                lock(m_lock)
                {
                    return streams.Exists(s=>s.Available);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int BufferSize { get; set; } = 1024;
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


        private void Listen()
        {
            if (!shutdown)
            {
                this.logger.InfoIf(this, () => $@"start listen named pipe {this.pipeName}");

                try
                {
                    PipeSecurity pipeSecurity = new PipeSecurity();
                    pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                        PipeAccessRights.FullControl, AccessControlType.Allow));
                    this.waitConnection = NamedPipeServerStreamConstructors.New(pipeName, PipeDirection.InOut, 10,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 1024, 1024, pipeSecurity);

                    stateObj = new PipeStateObject(this.waitConnection, this.BufferSize);


                    this.WaitForConnection(this.waitConnection);
                 }
                    catch (Exception ex)
                    {
                        this.logger.ErrorIf(this, () => $@"error listen named pipe {this.pipeName}", () => ex);
                    }
        }
        }


        private void OnConnectReceived(IAsyncResult asyn)
        {
            try
            {
                NamedPipeServerStream stream = ((PipeStateObject)asyn.AsyncState).pipe;
                stream.EndWaitForConnection(asyn);
                this.waitConnection = null;

                if (!shutdown)
                {
                    NamedPipeStreamServerConn instance = new NamedPipeStreamServerConn(this.pipeName,stream);
                    instance.DataReceivedAsync += Instance_DataReceivedAsync;
                    instance.OnDisconnected += Instance_OnDisconnected;

                    this.logger.InfoIf(this, () => $@"named pipe {this.pipeName} connected {instance.Handle}");

                    lock (m_lock)
                    {
                        streams.Add(instance);
                    }

                }

                this.Listen();
            }
            catch (Exception ex)
            {
                this.logger.ErrorIf(this, () => $@"start listen named pipe {this.pipeName}", ()=>ex);
            }

        }

        private void Instance_OnDisconnected(NamedPipeStreamServerConn instance)
        {
            lock(m_lock)
            {
                this.logger.WarningIf(this, () => $@"named pipe {this.pipeName} disconnected {instance.Handle}");

                streams.Remove(instance);

                instance.DataReceivedAsync -= Instance_DataReceivedAsync;
                instance.OnDisconnected -= Instance_OnDisconnected;
                
                instance.Dispose();
            }
        }

        private async Task Instance_DataReceivedAsync(IDataTransport<byte[]> endpoint, byte[] data, DataEndpoint sender)
        {
            if (DataReceivedAsync != null)
            {
                //  Esegue la comunicazione dell'evento attraverso l'apposito delegate.
                await Task.WhenAll(Array.ConvertAll(
                         DataReceivedAsync.GetInvocationList(),
                         e => ((dDataReceivedAsync<byte[]>)e)(endpoint, data, GetRemoteEndpoint()))).ConfigureAwait(false);

            }
        }

        private void WaitForConnection(NamedPipeServerStream pipeStream)
        {
            pipeStream.BeginWaitForConnection(m_connCallBack, stateObj);
        }

        #endregion

        #region IEnabler

        /// <inheritdoc/>
        public void Disable()
        {
            if (isEnabled)
            {
                this.logger.InfoIf(this, () => $@"named pipe {this.pipeName} disable");

                shutdown = true;

                this.waitConnection?.Dispose();

                foreach(var stream in streams)
                {
                    stream.Dispose();
                }

                isEnabled = false;

            }
        }

        /// <inheritdoc/>
        public void Enable()
        {
            if (!isEnabled)
            {
                shutdown = false;

                this.logger.InfoIf(this, () => $@"named pipe {this.pipeName} enable");

                this.Listen();

                isEnabled = true;
            }


        }

        /// <inheritdoc/>
        public void Initialize()
        {
            // non server 
        }

        /// <inheritdoc/>
        public bool IsEnabled()
        {
            return isEnabled;
        }
        #endregion

        #region IStream
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

        ///<inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<bool> SendAsync(byte[] data)
        {
            List<NamedPipeStreamServerConn> lists;
            lock (m_lock)
            {
                lists = streams.ToList();
            }

            foreach (var stream in lists)
            {
                await SendAsync(data, stream);
            }

            return true;
        }
        async Task<bool> SendAsync(byte[] data, NamedPipeStreamServerConn stream)
        {
            if (!stream.Available) return false;

            await stream.SendAsync(data).ConfigureAwait(false);

            return true;
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
                    name = this.pipeName
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
            public NamedPipeServerStream pipe;
            //  Size of receive buffer.
            public int BufferSize;
            //  Receive buffer.
            public byte[] buffer;

            public PipeStateObject(NamedPipeServerStream _pipe, int _bufSize)
            {
                pipe = _pipe; //  Serve per riconoscere il giusto socket quando ci sono più client.
                BufferSize = _bufSize;
                buffer = new byte[BufferSize];
            }
        }
        #endregion
    }
}
