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
            this.serverTransportHandler.OnMouseButtonAction += ServerTransportHandler_OnMouseButtonAction;
        }

        private void ServerTransportHandler_OnMouseButtonAction(IClient arg1, Abstract.Transport.MouseButtonAction arg2)
        {
            if (!clientSessions.TryGetValue(arg2.SessionId, out var session))
            {
                return;
            }
            if (!screenInfoHandler.GetScreen(arg2.ScreenIndex, out var screenInfo))
            {
                return;
            }
            screenControlHandler.MouseButtonAction(arg2, screenInfo);
        }

        private void ServerTransportHandler_OnMouseMove(IClient arg1, Abstract.Transport.MouseMove arg2)
        {
            if (!clientSessions.TryGetValue(arg2.SessionId, out var session))
            {
                return;
            }
            if(!screenInfoHandler.GetScreen(arg2.ScreenIndex, out var screenInfo))
            {
                return;
            }
            screenControlHandler.MouseMove(arg2, screenInfo);
        }

        private void ServerTransportHandler_OnMouseLeaveScreen(IClient arg1, Abstract.Transport.MouseLeaveScreen arg2)
        {
            if (!clientSessions.TryGetValue(arg2.SessionId, out var session))
            {
                return;
            }

            if (!screenInfoHandler.GetScreen(arg2.ScreenIndex, out var screenInfo))
            {
                return;
            }

            screenControlHandler.MouseLeave(arg2, screenInfo);
        }

        private void ServerTransportHandler_OnMouseEnterScreen(IClient arg1, Abstract.Transport.MouseEnterScreen arg2)
        {
            if (!clientSessions.TryGetValue(arg2.SessionId, out var session))
            {
                return;
            }

            if(!screenInfoHandler.GetScreen(arg2.ScreenIndex, out var screenInfo))
            {
                return;
            }

            screenControlHandler.MouseEnter(arg2, screenInfo);
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

            if(!session.GetScreenCaptureInfo(arg2.ScreenIndex, out var existingInfo))
            {
                // Screen capture not found, return empty response
                return new Abstract.Transport.ScreenCaptureEndRes
                {
                };
            }

            if(existingInfo != null)
            {
                screenCaptureHandler.End(existingInfo.ScreenCapture);

                session.RemoveScreenCapture(existingInfo);
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

            foreach (var screen in arg2.ScreenList)
            {
                if (!this.screenInfoHandler.GetScreen(screen, out var screenInfo))
                {
                    continue;
                }

                if(session.GetScreenCaptureInfo(screenInfo.Index, out var existingCapture))
                {
                    // Screen capture already exists for this screen, skip
                    screenStreamInfos.Add(new Abstract.Transport.ScreenStreamInfo
                    {
                        MonitorIndex = screenInfo.Index,
                        StreamType = existingCapture.StreamInfo.Type,
                        StreamParams = existingCapture.StreamInfo.Params
                    });
                    continue;
                }

                if(!this.screenCaptureHandler.Begin(screenInfo, out var streamCapture))
                {
                    continue;
                }

                this.screenStreamHandler.Attach(streamCapture, out var streamInfo);

                session.AddScreenCapture(streamCapture, streamInfo);

                screenStreamInfos.Add(new Abstract.Transport.ScreenStreamInfo
                {
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
                    screenCaptureHandler.End(screenCapture.ScreenCapture);
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
