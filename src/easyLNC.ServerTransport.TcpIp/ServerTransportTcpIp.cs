using acquleo.Protocol;
using acquleo.Protocol.Enveloper.Byte;
using acquleo.Protocol.Serializer;
using acquleo.Protocol.Transport.Byte.TcpIp;
using easyLNC.Abstract;
using easyLNC.Abstract.Transport;
using TetSistemi.Base.Class;

namespace easyLNC.ServerTransport.TcpIp
{
    public class ServerTransportTcpIpParams
    {
        public int Port { get; set; }
        public ServerTransportTcpIpParams(int port)
        {
            Port = port;
        }
    }

    public class ServerTransportTcpIp : IServerTransportHandler
    {
        public event Action<IServerTransportHandler, IClient>? OnConnected;
        public event Action<IServerTransportHandler, IClient>? OnDisconnected;
        public event Func<IClient, SessionStartReq, SessionStartRes>? OnSessionStart;
        public event Func<IClient, SessionEndReq, SessionEndRes>? OnSessionEnd;
        public event Func<IClient, ScreenCaptureStartReq, ScreenCaptureStartRes>? OnScreenCaptureStart;
        public event Func<IClient, ScreenCaptureEndReq, ScreenCaptureEndRes>? OnScreenCaptureEnd;
        public event Action<IClient, MouseEnterScreen>? OnMouseEnterScreen;
        public event Action<IClient, MouseLeaveScreen>? OnMouseLeaveScreen;
        public event Action<IClient, MouseMove>? OnMouseMove;
        public event Action<IClient, MouseButtonAction>? OnMouseButtonAction;
        public event Action<IClient, MouseWheel>? OnMouseWheel;
        public event Action<IClient, VirtualKeyDown>? OnVirtualKeyDown;
        public event Action<IClient, VirtualKeyUp>? OnVirtualKeyUp;
        public event Action<IClient, SessionKeepAlive>? OnKeepAlive;

        TcpServer tcpServer;
        MessageEndpoint<byte[], EmptyMessageEnvelope> endpoint;

        public ServerTransportTcpIp(ServerTransportTcpIpParams param)
        {
            tcpServer = new TcpServer(param.Port);
            XmlByteMessageSerializer xmlByteMessageSerializer = new XmlByteMessageSerializer(new XmlMessageSerializerInfo());
            ByteArrayMessageEnveloper byteArrayMessageEnveloper = new ByteArrayMessageEnveloper(xmlByteMessageSerializer);
            endpoint = new MessageEndpoint<byte[], EmptyMessageEnvelope>(tcpServer, byteArrayMessageEnveloper);
            endpoint.MessageReceivedAsync += Endpoint_MessageReceivedAsync;
            tcpServer.Enable();
        }

        private Task Endpoint_MessageReceivedAsync(IMessageEndpoint<EmptyMessageEnvelope> endpoint, EmptyMessageEnvelope msg, DataEndpoint sender)
        {
            if(msg.Payload.Is<SessionStartReq>())
            {
                this.OnSessionStart?.Invoke(null, msg.Payload.As<SessionStartReq>());
            }
            if (msg.Payload.Is<SessionEndReq>())
            {
                this.OnSessionEnd?.Invoke(null, msg.Payload.As<SessionEndReq>());
            }
            if (msg.Payload.Is<ScreenCaptureStartReq>())
            {
                this.OnScreenCaptureStart?.Invoke(null, msg.Payload.As<ScreenCaptureStartReq>());
            }
            if (msg.Payload.Is<ScreenCaptureEndReq>())
            {
                this.OnScreenCaptureEnd?.Invoke(null, msg.Payload.As<ScreenCaptureEndReq>());
            }
            if (msg.Payload.Is<MouseEnterScreen>())
            {
                this.OnMouseEnterScreen?.Invoke(null, msg.Payload.As<MouseEnterScreen>());
            }
            if (msg.Payload.Is<MouseLeaveScreen>())
            {
                this.OnMouseLeaveScreen?.Invoke(null, msg.Payload.As<MouseLeaveScreen>());
            }
            if (msg.Payload.Is<MouseMove>())
            {
                this.OnMouseMove?.Invoke(null, msg.Payload.As<MouseMove>());
            }
            if (msg.Payload.Is<SessionStartReq>())
            {
                this.OnSessionStart?.Invoke(null, msg.Payload.As<SessionStartReq>());
            }
            if (msg.Payload.Is<SessionStartReq>())
            {
                this.OnSessionStart?.Invoke(null, msg.Payload.As<SessionStartReq>());
            }
            if (msg.Payload.Is<SessionStartReq>())
            {
                this.OnSessionStart?.Invoke(null, msg.Payload.As<SessionStartReq>());
            }
            if (msg.Payload.Is<SessionStartReq>())
            {
                this.OnSessionStart?.Invoke(null, msg.Payload.As<SessionStartReq>());
            }
            if (msg.Payload.Is<SessionStartReq>())
            {
                this.OnSessionStart?.Invoke(null, msg.Payload.As<SessionStartReq>());
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IClient GetClientById(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IClient> GetConnectedClients()
        {
            throw new NotImplementedException();
        }
    }
}
