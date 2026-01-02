using easyLNC.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Screen.InfoHandler
{
    public class ScreenInfoHandler : IScreenInfoHandler
    {
        List<ScreenInfo> screenInfos = new List<ScreenInfo>();
        public bool GetScreen(int index, [NotNullWhen(true)] out ScreenInfo? screen)
        {
            screen = screenInfos.FirstOrDefault(s => s.Index == index);
            return screen != null;
        }

        public IEnumerable<ScreenInfo> GetScreens()
        {
            if(screenInfos.Count == 0)
            {
                screenInfos = MonitorEnumerationHelper.GetMonitors().ToList();
            }

            return screenInfos;
        }
    }
}
