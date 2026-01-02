using easyLNC.Abstract;
using System.Collections.Concurrent;

namespace easyLNC.Core
{
    public class CoreEasyLNC : ISessionHandler
    {
        IScreenCaptureHandler screenCaptureHandler;
        IServerTransportHandler serverTransportHandler;
        IScreenControlHandler screenControlHandler;
        IScreenInfoHandler screenInfoHandler;
        IScreenStreamHandler screenStreamHandler;
        ConcurrentDictionary<string,ClientSession> clientSessions = new ConcurrentDictionary<string, ClientSession>();
        public CoreEasyLNC(IScreenCaptureHandler screenCaptureHandler,
            IServerTransportHandler serverTransportHandler,
            IScreenControlHandler screenControlHandler,
            IScreenInfoHandler screenInfoHandler,
            IScreenStreamHandler screenStreamHandler)
        {
            this.screenCaptureHandler = screenCaptureHandler;
            this.serverTransportHandler = serverTransportHandler;
            this.screenControlHandler = screenControlHandler;
            this.screenInfoHandler = screenInfoHandler;
            this.screenStreamHandler = screenStreamHandler;

            this.serverTransportHandler.OnSessionStart += ServerTransportHandler_OnSessionStart;
            this.serverTransportHandler.OnSessionEnd += ServerTransportHandler_OnSessionEnd;
            this.serverTransportHandler.OnConnected += ServerTransportHandler_OnConnected;
            this.serverTransportHandler.OnDisconnected += ServerTransportHandler_OnDisconnected;
            this.serverTransportHandler.OnScreenCaptureStart += ServerTransportHandler_OnScreenCaptureStart;
            this.serverTransportHandler.OnScreenCaptureEnd += ServerTransportHandler_OnScreenCaptureEnd;
            this.serverTransportHandler.OnMouseEnterScreen += ServerTransportHandler_OnMouseEnterScreen;
            this.serverTransportHandler.OnMouseLeaveScreen += ServerTransportHandler_OnMouseLeaveScreen;
            this.serverTransportHandler.OnMouseMove += ServerTransportHandler_OnMouseMove;
        }

        private void ServerTransportHandler_OnMouseMove(IClient arg1, Abstract.Transport.MouseMoveMessage arg2)
        {
            throw new NotImplementedException();
        }

        private void ServerTransportHandler_OnMouseLeaveScreen(IClient arg1, Abstract.Transport.MouseLeaveScreenMessage arg2)
        {
            if (!clientSessions.TryGetValue(arg2.SessionId, out var session))
            {
                return;
            }

            if (!screenInfoHandler.GetScreen(arg2.ScreenIndex, out var screenInfo))
            {
                return;
            }

            screenControlHandler.MouseLeave(screenInfo);
        }

        private void ServerTransportHandler_OnMouseEnterScreen(IClient arg1, Abstract.Transport.MouseEnterScreenMessage arg2)
        {
            if (!clientSessions.TryGetValue(arg2.SessionId, out var session))
            {
                return;
            }

            if(!screenInfoHandler.GetScreen(arg2.ScreenIndex, out var screenInfo))
            {
                return;
            }

            screenControlHandler.MouseEnter(screenInfo);
        }

        private Abstract.Transport.ScreenCaptureEndRes ServerTransportHandler_OnScreenCaptureEnd(IClient arg1, Abstract.Transport.ScreenCaptureEndReq arg2)
        {
            if (!clientSessions.TryGetValue(arg2.SessionId, out var session))
            {
                // Session not found, return empty response
                return new Abstract.Transport.ScreenCaptureEndRes
                {
                };
            }

            if(!session.GetScreenCaptureById(arg2.CaptureId, out var screenCapture))
            {
                // Screen capture not found, return empty response
                return new Abstract.Transport.ScreenCaptureEndRes
                {
                };
            }

            if(screenCapture != null)
            {
                screenCaptureHandler.End(screenCapture);

                session.RemoveScreenCapture(screenCapture);
            }

            Abstract.Transport.ScreenCaptureEndRes response = new Abstract.Transport.ScreenCaptureEndRes
            {
            };
            return response;
        }

        private Abstract.Transport.ScreenCaptureStartRes ServerTransportHandler_OnScreenCaptureStart(IClient arg1, Abstract.Transport.ScreenCaptureStartReq arg2)
        {
            List<Abstract.Transport.ScreenStreamInfo> screenStreamInfos = new List<Abstract.Transport.ScreenStreamInfo>();

            if (!clientSessions.TryGetValue(arg2.SessionId, out var session))
            {
                // Session not found, return empty response
                return new Abstract.Transport.ScreenCaptureStartRes
                {
                    ScreenStreamInfos = screenStreamInfos
                };
            }

            foreach (var screein in arg2.ScreenList)
            {
                if (!this.screenInfoHandler.GetScreen(screein, out var screenInfo))
                {
                    continue;
                }

                if(!this.screenCaptureHandler.Begin(screenInfo, out var streamCapture))
                {
                    continue;
                }

                this.screenStreamHandler.Attach(streamCapture, out var streamInfo);

                session.AddScreenCapture(streamCapture);

                screenStreamInfos.Add(new Abstract.Transport.ScreenStreamInfo
                {
                    CaptureId = streamCapture.Id.ToString(),
                    MonitorIndex = screenInfo.Index,
                    StreamType = streamInfo.Type,
                    StreamParams = streamInfo.Params
                });

            }

            Abstract.Transport.ScreenCaptureStartRes response = new Abstract.Transport.ScreenCaptureStartRes
            {
                ScreenStreamInfos = screenStreamInfos
            };
            return response;
        }

        private void ServerTransportHandler_OnDisconnected(IServerTransportHandler arg1, IClient arg2)
        {

        }

        private void ServerTransportHandler_OnConnected(IServerTransportHandler arg1, IClient arg2)
        {
        }

        private Abstract.Transport.SessionEndRes ServerTransportHandler_OnSessionEnd(IClient arg1, Abstract.Transport.SessionEndReq arg2)
        {
            if(clientSessions.TryRemove(arg2.SessionId, out var session))
            {
                foreach(var screenCapture in session.GetScreenCaptures())
                {
                    screenCaptureHandler.End(screenCapture);
                }
            }
            Abstract.Transport.SessionEndRes response = new Abstract.Transport.SessionEndRes
            {
                 
            };
            return response;
        }

        private Abstract.Transport.SessionStartRes ServerTransportHandler_OnSessionStart(IClient arg1, Abstract.Transport.SessionStartReq arg2)
        {
            ClientSession session = new ClientSession();

            clientSessions.TryAdd(session.Id, session);

            Abstract.Transport.SessionStartRes response = new Abstract.Transport.SessionStartRes
            {
                SessionId = session.Id,
                Screens = screenInfoHandler.GetScreens().ToList()
            };



            return response;
        }

        public IEnumerable<IClientSession> GetSessions()
        {
            throw new NotImplementedException();
        }

        public IClientSession GetSessionById(string id)
        {
            throw new NotImplementedException();
        }
    }
}
