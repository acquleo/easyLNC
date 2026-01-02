using System;
using System.Threading.Tasks;
using TetSistemi.Base.Interfaces;
using acquleo.Protocol.Transport.Data.Endpoint;

namespace acquleo.Protocol.Transport.Byte.Memory
{

    /// <summary>
    /// 
    /// </summary>
    public class InMemoryByteClient : IDataTransport<byte[]>, IEnabler, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public enum Sides
        {
            /// <summary>
            /// 
            /// </summary>
            Client,
            /// <summary>
            /// 
            /// </summary>
            Server
        }

        readonly Sides side;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="side"></param>
        public InMemoryByteClient(Sides side)
        {
            this.side = side;
        }

        /// <inheritdoc/>
        public bool Available => true;
        /// <inheritdoc/>
        public event dDataReceivedAsync<byte[]> DataReceivedAsync;
        /// <inheritdoc/>
        public event dDataTransportAsync<byte[]> DataTransportAvailable;
        /// <inheritdoc/>
        public event dDataTransportAsync<byte[]> DataTransportUnavailable;
        /// <inheritdoc/>
        public event dDataTransportTraceAsync<byte[]> DataTransportTraceAsync;

        /// <inheritdoc/>
        public DataEndpoint GetRemoteEndpoint()
        {
            return new EmptyDataEndpoint();
        }

        /// <inheritdoc/>
        public Task<bool> SendAsync(byte[] data)
        {
            return SendAsync(data, new EmptyDataEndpoint());
        }

        /// <inheritdoc/>
        public Task<bool> SendAsync(byte[] data, DataEndpoint to)
        {
            InMemoryByte.Instance.Publish(data, side);

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public void Enable()
        {
            InMemoryByte.Instance.SetAs(this, side);
        }

        /// <inheritdoc/>
        public void Disable()
        {
            // non server 
        }

        /// <inheritdoc/>
        public bool IsEnabled()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public async Task Pushdata(byte[] data)
        {
            await Task.WhenAll(Array.ConvertAll(
                    DataReceivedAsync.GetInvocationList(),
                    e => ((dDataReceivedAsync<byte[]>)e)(this, data, new EmptyDataEndpoint()))).ConfigureAwait(false);
        }

        /// <inheritdoc/>
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
            //do nothing
        }
    }
}
