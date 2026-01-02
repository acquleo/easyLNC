using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using TetSistemi.Base.Logging;
using acquleo.Protocol;
using acquleo.Protocol.Transport.Data.Endpoint;
using static acquleo.Protocol.Transport.Byte.Pipe.NamedPipeStreamServer;

namespace tet.Protocol.Transport.Byte.Pipe
{
    internal class NamedPipeStreamServerConn : IDataTransport<byte[]>,
        IDisposable
    {
        readonly IApplicationLog logger;
        readonly NamedPipeServerStream pipeStream;
        readonly AsyncCallback m_pfnCallBack;
        readonly PipeStateObject stateObj;
        readonly string pipeName;
        public event Action<NamedPipeStreamServerConn> OnDisconnected;
        public NamedPipeStreamServerConn(string pipeName,NamedPipeServerStream pipeStream) {
            this.logger = SingletonFactoryProvider.Provider.GetApplicationLogFactory().GetBuilder().WithObject(this).Build();
            this.pipeName = pipeName;
            this.pipeStream = pipeStream;
            this.m_pfnCallBack = new AsyncCallback(OnDataReceived);
            this.stateObj = new PipeStateObject(pipeStream,this.BufferSize);
#pragma warning disable S3869 // "SafeHandle.DangerousGetHandle" should not be called
            this.Handle= (int)this.pipeStream.SafePipeHandle.DangerousGetHandle();
#pragma warning restore S3869 // "SafeHandle.DangerousGetHandle" should not be called
            WaitForData();
        }
        public int Handle
        {
            get;
            private set;
        }
        public int BufferSize { get; private set; } = 1024;
        public bool Available => pipeStream.IsConnected;

        public event dDataReceivedAsync<byte[]> DataReceivedAsync;
        public event dDataTransportAsync<byte[]> DataTransportAvailable;
        public event dDataTransportAsync<byte[]> DataTransportUnavailable;
        public event dDataTransportTraceAsync<byte[]> DataTransportTraceAsync;

        private void WaitForData()
        {
            pipeStream.BeginRead(stateObj.buffer, 0, stateObj.BufferSize, m_pfnCallBack, stateObj);
        }

        private async void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                PipeStateObject theSSO = (PipeStateObject)asyn.AsyncState;
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
                    this.OnDisconnect();
                }


            }
            catch (Exception ex)
            {
                this.OnDisconnect();
            }

        }

        void OnDisconnect()
        {
            this.OnDisconnected?.Invoke(this);
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

        public void Dispose()
        {
            this.pipeStream.Dispose();
        }

        public DataEndpoint GetRemoteEndpoint()
        {
            return new NamedPipeDataEndpoint()
            {
                name = this.pipeName
            };
        }

        public async Task<bool> SendAsync(byte[] data)
        {
            try
            {
                await this.pipeStream.WriteAsync(data, 0, data.Length);
                return true;
            }
            catch(Exception ex)
            {
                this.OnDisconnect();
                return false;
            }
        }

        public Task<bool> SendAsync(byte[] data, DataEndpoint to)
        {
            return this.SendAsync(data);
        }

        public void Enable()
        {
            
        }

        public void Disable()
        {
            
        }
    }
}
