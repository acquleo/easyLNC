using easyLNC.Abstract.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface IServerTransportHandler : IDisposable
    {
        event Action<IServerTransportHandler, IClient>? OnConnected;
        event Action<IServerTransportHandler, IClient>? OnDisconnected;

        event Func<IClient, SessionStartReq, SessionStartRes>? OnSessionStart;
        event Func<IClient, SessionEndReq, SessionEndRes>? OnSessionEnd;
        event Func<IClient, ScreenCaptureStartReq, ScreenCaptureStartRes>? OnScreenCaptureStart;
        event Func<IClient, ScreenCaptureEndReq, ScreenCaptureEndRes>? OnScreenCaptureEnd;
        event Action<IClient, MouseEnterScreen>? OnMouseEnterScreen;
        event Action<IClient, MouseLeaveScreen>? OnMouseLeaveScreen;
        event Action<IClient, MouseMove>? OnMouseMove;
        event Action<IClient, MouseButtonAction>? OnMouseButtonAction;
        event Action<IClient, MouseWheel>? OnMouseWheel;
        event Action<IClient, VirtualKeyDown>? OnVirtualKeyDown;
        event Action<IClient, VirtualKeyUp>? OnVirtualKeyUp;
        event Action<IClient, SessionKeepAlive>? OnKeepAlive;

        IEnumerable<IClient> GetConnectedClients();
        IClient GetClientById(string id);


    }
}
