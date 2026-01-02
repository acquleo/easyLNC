using easyLNC.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Screen.InfoHandler
{
    public class ScreenInfoHandler : IScreenInfoHandler
    {
        public IEnumerable<ScreenInfo> GetScreens()
        {
            return MonitorEnumerationHelper.GetMonitors();
        }
    }
}
