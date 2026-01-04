using easyLNC.Abstract;
using easyLNC.Abstract.Transport;

namespace easyLNC.Server.Dummy
{
    public class DummyServerTransportHandler : IServerTransportHandler
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
        public event Action<IClient, KeepAlive>? OnKeepAlive;

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
