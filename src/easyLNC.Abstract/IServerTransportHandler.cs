using easyLNC.Abstract.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface IServerTransportHandler
    {
        event Action<IServerTransportHandler, IClient> OnConnected;
        event Action<IServerTransportHandler, IClient> OnDisconnected;

        event Func<IClient, SessionStartReq, SessionStartRes> OnSessionStart;
        event Func<IClient, SessionEndReq, SessionEndRes> OnSessionEnd;
        event Func<IClient, ScreenCaptureStartReq, ScreenCaptureStartRes> OnScreenCaptureStart;
        event Func<IClient, ScreenCaptureEndReq, ScreenCaptureEndRes> OnScreenCaptureEnd;
        event Action<IClient, MouseEnterScreenMessage> OnMouseEnterScreen;
        event Action<IClient, MouseLeaveScreenMessage> OnMouseLeaveScreen;
        event Action<IClient, MouseMoveMessage> OnMouseMove;

        IEnumerable<IClient> GetConnectedClients();
        IClient GetClientById(string id);


    }
}
