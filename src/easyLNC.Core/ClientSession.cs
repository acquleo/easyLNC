using easyLNC.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Core
{
    internal class ClientSession : IClientSession
    {
        DateTime lastKeepAlive = DateTime.UtcNow;
        Guid sessionId;
        List<IScreenCapture> screenCaptures = new List<IScreenCapture>();
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

        public void AddScreenCapture(IScreenCapture screenCapture)
        {
            screenCaptures.Add(screenCapture);
        }

        public void RemoveScreenCapture(IScreenCapture screenCapture)
        {
            screenCaptures.Remove(screenCapture);
        }

        public bool GetScreenCaptureById(string id, out IScreenCapture? screenCapture)
        {
            screenCapture = screenCaptures.FirstOrDefault(sc => sc.Id.ToString() == id);
            return screenCapture != null;
        }

        public IEnumerable<IScreenCapture> GetScreenCaptures()
        {
            return screenCaptures.AsEnumerable();
        }
    }
}
