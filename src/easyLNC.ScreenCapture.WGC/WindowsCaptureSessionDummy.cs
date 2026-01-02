using easyLNC.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.ScreenCapture.WGC
{
    internal class WindowsCaptureSessionDummy : IScreenCapture
    {
        Guid id;

        public WindowsCaptureSessionDummy()
        {
            id = Guid.NewGuid();
        }

        public ScreenInfo Screen => throw new NotImplementedException();

        public Guid Id => id;

        public event Action<IScreenCapture, ScreenFrame>? OnNewFrame;

        public bool IsRunning()
        {
            throw new NotImplementedException();
        }
    }
}
