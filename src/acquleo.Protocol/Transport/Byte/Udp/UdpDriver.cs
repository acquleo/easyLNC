// ------------------------------------------------------------------------
//Società: T&TSistemi s.r.l.
//Anno: 2008 
//Progetto: AMIL5
//Autore: Acquisti Leonardo
//Nome modulo software: TetSistemi.Commons.dll
//Data ultima modifica: $LastChangedDate: 2020-01-09 14:21:54 +0100 (Thu, 09 Jan 2020) $
//Versione: $Rev: 706 $
// ------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using acquleo.Protocol.Exceptions;
using acquleo.Protocol.Transport.Data.Endpoint;

namespace acquleo.Protocol.Transport.Byte.Udp
{
    /// <summary>
    /// Implementa un Driver Udp
    /// </summary>
    public class UdpDriver : IDataTransport<byte[]>, IDisposable
    {
        #region Private Field

        private System.Net.Sockets.Socket log_socket;
        Thread readthread;
        private EndPoint localEndPoint;
        private IPEndPoint remote;
        private EndPoint remoteEndPoint;
        bool isRunning;        
        readonly int inputBufferSize = 0;

        #endregion

        #region Constructor

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="_localPort">Porta locale di ricezione</param>
        /// <param name="_remotePort">Porta remota di ricezione</param>
        /// <param name="_remoteIpAddress">Indirizzo Ip remoto</param>
        /// <param name="_inputBufferSize">Lunghezza in bytes del buffer di ricezione</param>
        public UdpDriver(int _localPort, int _remotePort, string _remoteIpAddress, int _inputBufferSize)
        {
            IPEndPoint local = new IPEndPoint(IPAddress.Any, _localPort);
            this.localEndPoint = (EndPoint)local;
            this.remote = new IPEndPoint(IPAddress.Parse(_remoteIpAddress), _remotePort);
            this.remoteEndPoint = (EndPoint)remote;
            inputBufferSize = _inputBufferSize;
        }

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="_localPort">Porta locale di ricezione</param>
        /// <param name="_remotePort">Porta remota di ricezione</param>
        /// <param name="_remoteIpAddress">Indirizzo Ip remoto</param>
        public UdpDriver(int _localPort, int _remotePort, string _remoteIpAddress)
        {
            IPEndPoint local = new IPEndPoint(IPAddress.Any, _localPort);
            this.localEndPoint = (EndPoint)local;
            this.remote = new IPEndPoint(IPAddress.Parse(_remoteIpAddress), _remotePort);
            this.remoteEndPoint = (EndPoint)remote;
        }

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="_localPort">Porta locale di ricezione</param>
        /// <param name="localIpAddress">Indirizzo Ip locale</param>
        /// <param name="_remotePort">Porta remota di ricezione</param>
        /// <param name="_remoteIpAddress">Indirizzo Ip remoto</param>
        public UdpDriver(int _localPort, string localIpAddress, int _remotePort, string _remoteIpAddress)
        {
            IPEndPoint local = new IPEndPoint(IPAddress.Parse(localIpAddress), _localPort);
            this.localEndPoint = (EndPoint)local;
            this.remote = new IPEndPoint(IPAddress.Parse(_remoteIpAddress), _remotePort);
            this.remoteEndPoint = (EndPoint)remote;
        }

        #endregion

        #region Properties

        /// <summary>
        /// EndPoint locale
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get
            {
                return localEndPoint;
            }
        }

        /// <summary>
        /// EndPoint remoto
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get
            {
                return remoteEndPoint;
            }
        }

        /// <summary>
        /// Indica se il Driver è in esecuzione
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }

        /// <summary>
        /// Ritorna la lunghezza in bytes del buffer di ricezione
        /// </summary>
        public int InputBufferSize
        {
            get
            {
                return inputBufferSize;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Avvia il driver Udp
        /// </summary>
        public void Start()
        {
            if (!isRunning)
            {
                isRunning = true;

                this.log_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                this.log_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
                if (inputBufferSize != 0)
                {
                    this.log_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, inputBufferSize);
                }
                this.log_socket.Bind(localEndPoint);

                //start listening thread
                this.readthread = new Thread(new ThreadStart(this.WaitRead));
                this.readthread.Start();


            }
        }

        /// <summary>
        /// ferma il driver Udp
        /// </summary>
        public void Stop()
        {
            if (isRunning)
            {
                isRunning = false;

                this.readthread.Interrupt();
                this.readthread.Join();
                this.log_socket.Shutdown(SocketShutdown.Both);
                this.log_socket.Close();

            }
        }

        /// <summary>
        /// Modifica la porta di ricezione locale
        /// </summary>
        /// <param name="_localaddress">Indirizzo ip locale.</param>
        /// <param name="_localPort">Porta locale.</param>
        public void ChangeLocalAddress(string _localaddress, int _localPort)
        {
            bool lastisrunning = this.isRunning;
            if (lastisrunning) Stop();

            //restart listening with new port            
            IPEndPoint local = new IPEndPoint(IPAddress.Parse(_localaddress), _localPort);
            this.localEndPoint = (EndPoint)local;

            if (lastisrunning) Start();

        }

        /// <summary>
        /// Modifica la porta di ricezione locale
        /// </summary>
        /// <param name="_localPort">Porta locale.</param>
        public void ChangeLocalPort(int _localPort)
        {
            bool lastisrunning = this.isRunning;
            if (lastisrunning) Stop();

            //restart listening with new port            
            IPEndPoint local = new IPEndPoint(IPAddress.Any, _localPort);
            this.localEndPoint = (EndPoint)local;

            if (lastisrunning) Start();

        }

        /// <summary>
        /// Modifica l'indirizzo Ip Remoto
        /// </summary>
        /// <param name="_remotePort">Porta remota.</param>
        /// <param name="_remoteIpAddress">Indirizzo ip remoto.</param>
        public void ChangeRemoteAddress(int _remotePort, string _remoteIpAddress)
        {
            this.remote = new IPEndPoint(IPAddress.Parse(_remoteIpAddress), _remotePort);
            this.remoteEndPoint = (EndPoint)remote;
        }


        /// <summary>
        /// Invia un array di bytes sul socket
        /// </summary>
        /// <param name="msg">Array di bytes</param>
        /// <param name="endpoint">Endpoint di destinazione</param>
        /// <returns>Ritorna il numero di bytes inviati</returns>
        public int SendDataTo(byte[] msg, IPEndPoint endpoint)
        {
            int dataSend = this.log_socket.SendTo(msg, msg.Length, SocketFlags.None, endpoint);


            return dataSend;
        }

        #endregion

        #region Private Methods


        private void WaitRead()
        {
            try
            {
                byte[] DATA = new byte[short.MaxValue];
                //setting socket connection                
                do
                {
                    if (this.log_socket.Available > 0)
                    {
                        //read data from buffer
                        EndPoint ipEndPoint = new IPEndPoint(IPAddress.None, 0);
                        int dataRcvSize = 0;
                        try
                        {
                            dataRcvSize = this.log_socket.ReceiveFrom(DATA, 0, DATA.Length, SocketFlags.None, ref ipEndPoint);
                        }
                        catch (SocketException ex1)
                        {
                            Console.WriteLine(ex1.Message);
                        }
                        //process data
                        byte[] dataRcv = new byte[dataRcvSize];
                        Array.Copy(DATA, 0, dataRcv, 0, dataRcvSize);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        FireOnDataReceived(dataRcv, (IPEndPoint)ipEndPoint);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                              //clear memory data                        
                        for (int i = 0; i < DATA.Length; i++)
                        {
                            DATA[i] = 0;
                        }
                    }
                    else
                        Thread.Sleep(1);
                }
                while (isRunning);
            }
            catch (ThreadInterruptedException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private async Task FireOnDataReceived(byte[] dataRcv, IPEndPoint endpoint)
        {
            if (DataReceivedAsync != null)
            {
                //  Esegue la comunicazione dell'evento attraverso l'apposito delegate.
                await Task.WhenAll(Array.ConvertAll(
                         DataReceivedAsync.GetInvocationList(),
                         e => ((dDataReceivedAsync<byte[]>)e)(this, dataRcv, new IpDataEndpoint()
                         {
                             address = endpoint.Address.ToString(),
                             port = endpoint.Port,
                         }))).ConfigureAwait(false);

            }
        }

        #endregion

        #region IStream Members

        /// <inheritdoc />
        public Task<bool> SendAsync(byte[] data)
        {
            return SendAsync(data, GetRemoteEndpoint());
        }
        /// <inheritdoc />
        public async Task<bool> SendAsync(byte[] data, DataEndpoint to)
        {
            if (to == null)
            {
                to = GetRemoteEndpoint();
            }

            if (!(to is IpDataEndpoint)) throw new ArgumentProtocolException("invalid data endpoint type");

            IpDataEndpoint ipendpoint = (IpDataEndpoint)to;
            var target = new IPEndPoint(IPAddress.Parse(ipendpoint.address), ipendpoint.port);


            int dataSend = await this.log_socket.SendToAsync(new ArraySegment<byte>(data), SocketFlags.None, target).ConfigureAwait(false);

            return dataSend > 0;
        }

        IpDataEndpoint endpoint;
        /// <inheritdoc />
        public DataEndpoint GetRemoteEndpoint()
        {
            if (endpoint == null)
                endpoint = new IpDataEndpoint()
                {
                    address = this.remote.Address.ToString(),
                    port = this.remote.Port
                };

            return endpoint;
        }

        #endregion

        #region IDisposable Members

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
            Stop();
        }

        public void Enable()
        {
            this.Start();
        }

        public void Disable()
        {
            this.Stop();
        }

        #endregion

        #region IStream Members
        /// <inheritdoc />
        public bool Available
        {
            get
            {
                return this.IsRunning;
            }
        }
        /// <inheritdoc />
        public event dDataReceivedAsync<byte[]> DataReceivedAsync;
        /// <inheritdoc />
        public event dDataTransportAsync<byte[]> DataTransportAvailable;
        /// <inheritdoc />
        public event dDataTransportAsync<byte[]> DataTransportUnavailable;
        /// <inheritdoc />
        public event dDataTransportTraceAsync<byte[]> DataTransportTraceAsync;

        #endregion
    }
}
