using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TetSistemi.Base.Logging;
using acquleo.Protocol.Transport.Data.Endpoint;

namespace acquleo.Protocol.Transport.Byte.TcpIp
{

    /// <summary>
    /// Implementa una connessione TCP/IP lato Server
    /// </summary>
	public class TcpServerConnection : IDataTransport<byte[]>
    {

        #region Events

        /// <summary>
        /// Avvenuta ricezione dati: evento da concatenare all'evento OnReceive  
        /// </summary>
        public event dDataReceivedAsync<byte[]> DataReceivedAsync;
        /// <inheritdoc/>
        public delegate void DisconnectEventHandler(object sender, TcpEventArgs e, bool DisconnectByServer);
        /// <inheritdoc/>
        public event DisconnectEventHandler OnDisconnect;
        /// <inheritdoc/>
        public event dDataTransportAsync<byte[]> DataTransportAvailable;
        /// <inheritdoc/>
        public event dDataTransportAsync<byte[]> DataTransportUnavailable;
        /// <inheritdoc/>
        public event dDataTransportTraceAsync<byte[]> DataTransportTraceAsync;

        #endregion

        #region Variable Declaration
        private Socket msockClient;
        private int mintID;
        readonly private int tcpConnectionId;
        readonly IApplicationLog logger;
        readonly private byte[] mbytBuffer; // packet data buffer.
        readonly string remoteEndPointString;
        readonly IpDataEndpoint endpoint;
        #endregion

        #region Proprietà
        /// <summary>
        /// Ritorna l'identificativo univoco della connessione
        /// </summary>
        public int ConnectionId
        {
            get { return mintID; }
        }

        /// <summary>
        /// Ritorna l'identificativo univoco della connessione
        /// </summary>
        public int Handle
        {
            get { return tcpConnectionId; }
        }

        /// <summary>
        /// Client Connesso
        /// </summary>
		public bool Connected
        {
            get
            {
                if (msockClient == null) return false;
                return msockClient.Connected;
            }
        }

        readonly EndPoint remoteEndPoint;
        /// <summary>
        /// Client Remoto
        /// </summary>
		public EndPoint RemoteEndPoint
        {
            get { return remoteEndPoint; }
        }
        #endregion

        #region Constructor

        /// <summary>Associa la classe TcpServerConnection a un Soket esistente</summary>
        /// <param name="client">Socket Connesso</param>
        /// <param name="inputBuffer">Buffer di ingresso.</param>
        public TcpServerConnection(Socket client, int inputBuffer)
        {
            this.logger = SingletonFactoryProvider.Provider.GetApplicationLogFactory().GetBuilder().WithObject(this).Build();
            this.msockClient = client;
            this.remoteEndPoint = client.RemoteEndPoint;
            this.remoteEndPointString = remoteEndPoint.ToString();
            var data = remoteEndPointString.Split(':');
            this.endpoint = new IpDataEndpoint() { address = data[0], port = int.Parse(data[1]) };

            this.msockClient.SendTimeout = 10000;
            this.msockClient.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

            this.tcpConnectionId = client.Handle.ToInt32();

            // Inizializzazione

            mbytBuffer = new byte[inputBuffer];

        }

        #endregion

        #region Public Method
        /// <summary>
        /// Metodo per la ricezione dei dati
        /// </summary>
        public void BeginReceive()
        {
            try
            {
                if (msockClient == null) return;
                // Inizio a ricevere Dati
                msockClient.BeginReceive(mbytBuffer, 0, mbytBuffer.Length, SocketFlags.None, new AsyncCallback(BeginReceive), msockClient);
            }
            catch (SocketException ex)
            {
                this.logger.Log(LogLevels.Error, this, $@"BeginReceive socket: {this.remoteEndPointString} exception: {ex.ToString()}");
                DisconnectSocket(false);
            }
        }

        /// <inheritdoc />
        public Task<bool> SendAsync(byte[] byte_data, DataEndpoint to)
        {
            return SendAsync(byte_data);
        }

        /// <inheritdoc />
        public async Task<bool> SendAsync(byte[] byte_data)
        {
            try
            {
                if (msockClient != null && msockClient.Connected)
                {
                    var result = await msockClient.SendAsync(new ArraySegment<byte>(byte_data), SocketFlags.None).ConfigureAwait(false);

                    await FireOnTrace(DataDirection.Out, byte_data);

                    return result > 0;
                }
                return false;
            }
            catch (SocketException)
            {
                DisconnectSocket(false);
                return false;
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevels.Error, this, $@"SendAsync socket: {this.remoteEndPointString} exception: {ex.ToString()}");
                return false;
            }
        }

        /// <summary>Disconnette tutti i Sockets</summary>
        public void DisconnectSocket(bool disconnectByServer)
        {
            if (msockClient == null) return;

            TcpEventArgs args = new TcpEventArgs(null, (IpDataEndpoint)GetRemoteEndpoint());

            if (msockClient.Connected)
            {
                mintID = -1; // Prevents BeginReceive from double disconnecting.
                msockClient.Shutdown(SocketShutdown.Both);
                msockClient.Close();
                msockClient = null;

            }

            // Evento disconnessione
            if (OnDisconnect != null) OnDisconnect(this, args, disconnectByServer);
        }
        #endregion

        #region Private Method

        private async Task FireOnTrace(DataDirection direction, byte[] data)
        {
            if (DataTransportTraceAsync != null)
            {
                await Task.WhenAll(Array.ConvertAll(
                     DataTransportTraceAsync.GetInvocationList(),
                     e => ((dDataTransportTraceAsync<byte[]>)e)(this, direction, data, this.GetRemoteEndpoint()))).ConfigureAwait(false);
            }
        }

        private async void BeginReceive(IAsyncResult ar)
        {
            Socket client = ar.AsyncState as Socket;

            if (client == null) return;
            if (mintID < 0) return;

            // Ci sono dati?
            try
            {
                int nBytesRec = client.EndReceive(ar);
                if (nBytesRec > 0)
                {

                    byte[] dataread = new byte[nBytesRec];
                    Array.Copy(mbytBuffer, dataread, nBytesRec);

                    await FireOnTrace(DataDirection.In, dataread);

                    // Invio i dati ricevuti
                    if (DataReceivedAsync != null)
                    {
                        await Task.WhenAll(Array.ConvertAll(
                          DataReceivedAsync.GetInvocationList(),
                          e => ((dDataReceivedAsync<byte[]>)e)(this, dataread, GetRemoteEndpoint()))).ConfigureAwait(false);

                    }

                    // Se la connessione è UP mi rimetto in ricezione
                    if (client.Connected) client.BeginReceive(mbytBuffer, 0, mbytBuffer.Length, SocketFlags.None, new AsyncCallback(BeginReceive), client);
                }
                else
                {
                    // Se non ci sono dati probabilmente la connessione è stata chiusa
                    DisconnectSocket(false);
                }
            }
            catch (SocketException ex)
            {
                this.logger.Log(LogLevels.Error, this, $@"BeginReceive socket: {this.remoteEndPointString} exception: {ex.ToString()}");
                DisconnectSocket(false);
            }
            catch (Exception ex)
            {
                this.logger.Log(LogLevels.Error, this, $@"BeginReceive socket: {this.remoteEndPointString} exception: {ex.ToString()}");
                
                throw;
            }
        }
        /// <inheritdoc />
        public DataEndpoint GetRemoteEndpoint()
        {
            return endpoint;
        }

        public void Enable()
        {
            
        }

        public void Disable()
        {
            
        }
        #endregion

        #region IStream Members
        /// <summary>
        /// Returns if the stream is available
        /// </summary>
        public bool Available
        {
            get
            {
                return Connected;
            }
        }

        #endregion
    }
}