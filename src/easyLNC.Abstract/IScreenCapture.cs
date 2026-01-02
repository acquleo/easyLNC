using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface IScreenCapture
    {
        Guid Id { get; }
        event Action<IScreenCapture, ScreenFrame>? OnNewFrame;
        ScreenInfo Screen { get; }
        bool IsRunning();
    }
}
