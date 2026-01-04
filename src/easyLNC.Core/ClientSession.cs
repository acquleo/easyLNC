using easyLNC.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Core
{
    

    internal class ClientSession : IClientSession
    {
        DateTime lastKeepAlive = DateTime.UtcNow;
        Guid sessionId;
        List<ClientScreenCaptureInfo> screenCaptures = new List<ClientScreenCaptureInfo>();
        public ClientSession() { 
            sessionId = Guid.NewGuid();
        }
        
        public string Id => sessionId.ToString();

        public void KeepAlive()
        {
            lastKeepAlive = DateTime.UtcNow;
        }

        public bool IsExpired()
        {
            return (DateTime.UtcNow - lastKeepAlive).TotalSeconds > 30;
        }

        public bool GetScreenCaptureInfo(int screenIndex, [NotNullWhen(true)] out ClientScreenCaptureInfo? screenCaptureInfo)
        {
            var capture = screenCaptures.FirstOrDefault(sc => sc.ScreenCapture.Screen.Index == screenIndex);
            screenCaptureInfo = capture != null ? new ClientScreenCaptureInfo(capture.ScreenCapture, capture.StreamInfo) : null;
            return screenCaptureInfo != null;
        }

        public void AddScreenCapture(IScreenCapture screenCapture, StreamInfo streamInfo)
        {
            screenCaptures.Add(new ClientScreenCaptureInfo(screenCapture, streamInfo));
        }

        public void RemoveScreenCapture(ClientScreenCaptureInfo screenCaptureInfo)
        {
            screenCaptures.Remove(screenCaptureInfo);
        }


        public IEnumerable<ClientScreenCaptureInfo> GetScreenCaptures()
        {
            return screenCaptures.AsEnumerable();
        }
    }
}
