using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TetSistemi.Base.Interfaces;
using TetSistemi.Base.Logging;
using acquleo.Protocol.Transport.Data.Endpoint;

namespace acquleo.Protocol.Transport.Byte.TcpIp
{
    /// <summary>
    /// implements a TCP server arg
    /// </summary>
    public class TcpEventArgs : EventArgs
    {
        #region Variable Declaration
        private readonly DataEndpoint mintSocket;
        private readonly byte[] mData;
        #endregion

        #region Constructor
        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="data">Array di byte.</param>
        /// <param name="socketkey">Socket key</param>
        public TcpEventArgs(byte[] data, DataEndpoint socketkey)
        {
            mData = data;
            mintSocket = socketkey;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Proprietà Data.
        /// </summary>
        public byte[] Data
        {
            get
            {
                return mData;
            }
        }

        /// <summary>
        /// Proprietà SocketKey.
        /// </summary>
        public DataEndpoint SocketKey
        {
            get
            {
                return mintSocket;
            }
        }

        #endregion
    }

    /// <summary>
    /// implements a TCP server
    /// </summary>
    public class TcpServer : IDataTransport<byte[]>, IEnabler, IDisposable
    {

        #region Variable Declaration
        private int mintConnections = 0;
        private Socket listener;
        private readonly int defaultBackLogSize = 10000;
        readonly Dictionary<DataEndpoint, IDataTransport<byte[]>> connectionsMap = new Dictionary<DataEndpoint, IDataTransport<byte[]>>();
        readonly object connectionsMapSync = new object();
        readonly IApplicationLog logger;

        #endregion

        #region Constructor
        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="port">server port</param>
        public TcpServer(int port)
        {
            this.logger = SingletonFactoryProvider.Provider.GetApplicationLogFactory().GetBuilder().WithObject(this).Build();
            this.LocalPort = port;
        }


        #endregion

        #region Properties

        /// <summary>
        /// Porta di ricezione TCP
        /// </summary>
		public int LocalPort { get; private set; } = 0;

        /// <summary>
        /// Dimensione del buffer di ricezione
        /// </summary>
        public int InputBuffer { get; set; } = 1000000;

        /// <summary>
        /// Connessioni Correnti
        /// </summary>
        public List<IDataTransport<byte[]>> ActiveConnections
        {
            get
            {
                lock (connectionsMapSync)
                {
                    List<IDataTransport<byte[]>> list = new List<IDataTransport<byte[]>>();
                    foreach (var el in connectionsMap.Values)
                    {
                        list.Add(el);
                    }
                    return list;
                }
            }
        }

        /// <summary>
        /// Numero di Connessioni Correnti
        /// </summary>
		public int ActiveConnectionsCount
        {
            get { return mintConnections; }
        }

        /// <summary>
        /// Indirizzo sul quale viene effettuato il Bind
        /// </summary>
		public IPAddress LocalIpAddress { get; set; } = IPAddress.Any;

        /// <summary>
        /// 
        /// </summary>
        public bool Available => true;

        #endregion

        #region Events.

        /// <summary>
        /// Delega l'evento BeginConnect
        /// </summary>
        /// <param name="sender">Oggetto TcpServerConnection</param>
        /// <param name="e">Args</param>
        public delegate void ConnectEventHandler(TcpServerConnection sender, TcpEventArgs e);

        /// <summary>
        /// Implementa l'evento di connessione
        /// </summary>
		public event ConnectEventHandler BeginConnect;

        /// <summary>
        /// Delega l'evento BeginConnect
        /// </summary>
        /// <param name="sender">Oggetto TcpServerConnection</param>
        /// <param name="e">Args</param>
		public delegate void DisconnectEventHandler(object sender, TcpEventArgs e);
        /// <summary>
        /// Implementa l'evento di disconnessione
        /// </summary>
		public event DisconnectEventHandler BeginDisconnect;
        /// <inheritdoc/>
        public event dDataReceivedAsync<byte[]> DataReceivedAsync;
        /// <inheritdoc/>
        public event dDataTransportAsync<byte[]> DataTransportAvailable;
        /// <inheritdoc/>
        public event dDataTransportAsync<byte[]> DataTransportUnavailable;
        /// <inheritdoc/>
        public event dDataTransportTraceAsync<byte[]> DataTransportTraceAsync;
        bool enabled;
        #endregion

        #region Public Method
        /// <summary>Abilita il Server</summary>
        public void Enable()
        {
            if (enabled) return;
            if (LocalPort < 1)
            {
                this.logger.Log(LogLevels.Error, this, $@"Invalid server listening port: {LocalPort}");

                throw new InvalidOperationException($@"Invalid port. {LocalPort}");
            }

            try
            {
                // Initialize socket objects.
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(this.LocalIpAddress, LocalPort));
                listener.Listen(defaultBackLogSize);

                listener.BeginAccept(new AsyncCallback(BeginAccept), listener);
                enabled = true;

                this.logger.Log(LogLevels.Info, this, $@"Start listening on port: {LocalPort}");
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevels.Error, this, $@"Server listening on port: {LocalPort} exception {ex.ToString()}");
                throw;
            }
        }

        /// <summary>Disabilita il Server</summary>
        public void Disable()
        {
            if (!enabled) return;
            try
            {
                if (listener != null)
                {
                    enabled = false;
                    listener.Close();

                    Disconnect();  // Diconnette tutti i Clients

                    this.logger.Log(LogLevels.Info, this, $@"Server STOP listening on port: {LocalPort}");
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevels.Error, this, $@"Server listening on port: {LocalPort} exception {ex.ToString()}");
                throw;
            }
        }

        /// <summary>Disconnette un Client</summary>
        /// <param name="connectionId">Identificativo della connessione</param>
        private void Disconnect(DataEndpoint connectionId)
        {
            if (!connectionsMap.ContainsKey(connectionId)) return;
            if (connectionsMap[connectionId] != null && ((TcpServerConnection)connectionsMap[connectionId]).Connected)
            {
                ((TcpServerConnection)connectionsMap[connectionId]).DisconnectSocket(false);

                this.logger.Log(LogLevels.Info, this, $@"Disconnected socket server: {LocalPort} connection {connectionId.ToString()}");
            }
        }

        /// <summary>Disconnette tutti i Clients</summary>
        public void Disconnect()
        {
            List<DataEndpoint> strings = new List<DataEndpoint>();
            lock (connectionsMapSync)
            {
                foreach (var el in connectionsMap.Keys)
                {
                    strings.Add(el);
                }
            }
            foreach (DataEndpoint key in strings) { Disconnect(key); }
        }

        #endregion

        #region Private Method

        /// <summary>Evento Connessione Avvenuta</summary>
        private void BeginAccept(IAsyncResult ar)
        {
            Socket s_listener = ar.AsyncState as Socket;

            if (s_listener == null || s_listener.Handle.ToInt32() == -1) return;

            try
            {

                Socket client = s_listener.EndAccept(ar);

                // search for empty client node.
                if (client.RemoteEndPoint == null) return;

                TcpServerConnection conn = new TcpServerConnection(client, InputBuffer);
                DataEndpoint key = conn.GetRemoteEndpoint();

                if (key == null) return;

                if (!connectionsMap.ContainsKey(key))
                {
                    lock (connectionsMapSync)
                    {
                        conn.OnDisconnect += new TcpServerConnection.DisconnectEventHandler(OnDisconnectSocket);
                        conn.DataReceivedAsync += Conn_DataReceivedAsync;
                        conn.DataTransportAvailable += Conn_DataTransportAvailable;
                        conn.DataTransportTraceAsync += Conn_DataTransportTraceAsync;
                        conn.DataTransportUnavailable += Conn_DataTransportUnavailable;

                        connectionsMap.Add(key, conn);
                        mintConnections++;
                    }

                    this.logger.Log(LogLevels.Info, this, $@"OnDisconnectSocket connection added from connection list: {LocalPort} socket {key} num connections: {mintConnections}");

                    if (BeginConnect != null) BeginConnect(conn, new TcpEventArgs(null, key));

                    conn.BeginReceive();
                }

            }
            catch (ObjectDisposedException ex)
            {
                this.logger.Log(LogLevels.Error, this, $@"Server listening on port: {LocalPort} exception {ex.ToString()}");
            }
            catch (Exception e)
            {
                this.logger.Log(LogLevels.Error, this, $@"Server listening on port: {LocalPort} exception {e.ToString()}");
            }

            try
            {
                s_listener.BeginAccept(new AsyncCallback(BeginAccept), s_listener);

            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevels.Error, this, $@"Server listening on port: {LocalPort} exception {ex.ToString()}");
            }

        }

        private async Task Conn_DataTransportUnavailable(IDataTransport<byte[]> endpoint)
        {
            if (DataTransportUnavailable != null)
            {
                await Task.WhenAll(Array.ConvertAll(
                     DataTransportUnavailable.GetInvocationList(),
                     e => ((dDataTransportAsync<byte[]>)e)(endpoint))).ConfigureAwait(false);
            }
        }

        private async Task Conn_DataTransportTraceAsync(IDataTransport<byte[]> endpoint, DataDirection direction, byte[] data, DataEndpoint sender)
        {
            if (DataTransportTraceAsync != null)
            {
                await Task.WhenAll(Array.ConvertAll(
                     DataTransportTraceAsync.GetInvocationList(),
                     e => ((dDataTransportTraceAsync<byte[]>)e)(endpoint, direction, data, sender))).ConfigureAwait(false);
            }
        }

        private async Task Conn_DataTransportAvailable(IDataTransport<byte[]> endpoint)
        {
            if (DataTransportAvailable != null)
            {
                await Task.WhenAll(Array.ConvertAll(
                     DataTransportAvailable.GetInvocationList(),
                     e => ((dDataTransportAsync<byte[]>)e)(endpoint))).ConfigureAwait(false);
            }
        }

        private async Task Conn_DataReceivedAsync(IDataTransport<byte[]> endpoint, byte[] data, DataEndpoint sender)
        {
            if (DataReceivedAsync != null)
            {
                await Task.WhenAll(Array.ConvertAll(
                     DataReceivedAsync.GetInvocationList(),
                     e => ((dDataReceivedAsync<byte[]>)e)(endpoint, data, sender))).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// Client Disconnesso
        /// </summary>
        /// <param name="sender">Client</param>
        /// <param name="e">Parametri</param>
        /// <param name="DisconnectByServer">Indica se il socket è stato disconnesso dall'implementatore</param>
        private void OnDisconnectSocket(object sender, TcpEventArgs e, bool DisconnectByServer)
        {
            if (connectionsMap.ContainsKey(e.SocketKey))
            {
                lock (connectionsMapSync)
                {
                    connectionsMap.Remove(e.SocketKey);
                    --mintConnections;
                }

                this.logger.Log(LogLevels.Info, this, $@"OnDisconnectSocket connection removed from connection list: {LocalPort} socket {e.SocketKey}");

                if (!DisconnectByServer && BeginDisconnect != null)
                {
                    BeginDisconnect(sender, e);
                }
            }
        }
        /// <inheritdoc/>
        public async Task<bool> SendAsync(byte[] data)
        {
            var conns = this.ActiveConnections;
            foreach (var conn in conns)
            {
                await conn.SendAsync(data).ConfigureAwait(false);
            }
            return true;
        }
        /// <inheritdoc/>
        public async Task<bool> SendAsync(byte[] data, DataEndpoint to)
        {
            if (to == null) return await SendAsync(data).ConfigureAwait(false);

            if (!connectionsMap.ContainsKey(to)) return false;

            return await connectionsMap[to].SendAsync(data).ConfigureAwait(false);
        }

        readonly EmptyDataEndpoint empty = new EmptyDataEndpoint();
        /// <inheritdoc />
        public DataEndpoint GetRemoteEndpoint()
        {
            return empty;
        }

        /// <summary>
        /// 
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

        /// <inheritdoc />
        public bool IsEnabled()
        {
            return enabled;
        }
        #endregion
    }
}