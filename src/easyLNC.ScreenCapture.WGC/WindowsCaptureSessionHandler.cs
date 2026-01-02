using easyLNC.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.ScreenCapture.WGC
{

    public class WindowsCaptureSessionHandler : IScreenCaptureHandler
    {
        Guid id = Guid.NewGuid();

        public event Action<IScreenCaptureHandler, IScreenCapture>? OnBegin;
        public event Action<IScreenCaptureHandler, IScreenCapture>? OnEnd;
        public event Action<IScreenCaptureHandler, IScreenCapture>? OnStarted;
        public event Action<IScreenCaptureHandler, IScreenCapture>? OnStopped;


        readonly private SharpDX.Direct3D11.Device device;
        readonly object lockObj = new object();

        List<IScreenCapture> screenCaptures = new List<IScreenCapture>();

        public WindowsCaptureSessionHandler()
        {
            this.device = easyLNC.Helper.Direct3D11.Direct3D11Helper.CreateDevice();
        }
        
        Dictionary<string, WindowsCaptureSessionInfo> sessions = new Dictionary<string, WindowsCaptureSessionInfo>();

        
        public bool Begin(ScreenInfo screen, [NotNullWhen(true)] out IScreenCapture screenCapture)
        {
            try
            {
                WindowsCaptureSession session;
                lock (lockObj)
                {
                    if (sessions.ContainsKey(screen.Name))
                    {
                        screenCapture = sessions[screen.Name].Session;
                        return true;
                    }
                    session = new WindowsCaptureSession(screen, device);
                    WindowsCaptureSessionInfo sessionInfo = new WindowsCaptureSessionInfo(session);
                    sessions[screen.Name] = sessionInfo;

                }

                session.StartCapture();

                OnStarted?.Invoke(this, session);

                screenCapture = session;
                return true;
            }
            finally
            {

                sessions[screen.Name].AddReference();

                OnBegin?.Invoke(this, sessions[screen.Name].Session);
            }
        }

        public void End(IScreenCapture screenCapture)
        {
            WindowsCaptureSessionInfo? sessionInfo;
            lock (lockObj)
            {
                if (!sessions.ContainsKey(screenCapture.Screen.Name))
                {
                    return;
                }

                sessionInfo = sessions[screenCapture.Screen.Name];
            }

            sessionInfo.RemoveReference();

            OnEnd?.Invoke(this, screenCapture);

            if (sessionInfo.References==0)
            {
                sessions.Remove(screenCapture.Screen.Name);
                sessionInfo.Session.Dispose();

                OnStopped?.Invoke(this, sessionInfo.Session);
            }
        }

        public IEnumerable<ScreenInfo> GetScreens()
        {
            return MonitorEnumerationHelper.GetMonitors();
        }

        public IEnumerable<IScreenCapture> Get()
        {
            lock(lockObj)
            {
                return sessions.Values.Select(s => s.Session).ToList();
            }
        }
    }

    internal class WindowsCaptureSessionInfo
    {
        int references = 0;
        public int References => references;
        public WindowsCaptureSession Session { get; set; }
        public WindowsCaptureSessionInfo(WindowsCaptureSession session)
        {
            Session = session;
        }

        public void AddReference()
        {
            Interlocked.Increment(ref references);
        }

        public void RemoveReference()
        {
            if (references > 0)
                Interlocked.Decrement(ref references);
        }
    }
}
