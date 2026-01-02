using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface ISession
    {
        IScreenCaptureHandler GetScreenCaptureHandler();
        IEnumerable<IScreenCapture> GetScreenCaptures();

    }
}
