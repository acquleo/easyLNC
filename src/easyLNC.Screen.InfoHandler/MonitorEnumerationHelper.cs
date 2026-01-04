using easyLNC.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace easyLNC.Screen.InfoHandler
{
    internal class MonitorEnumerationHelper
    {
        public static IEnumerable<ScreenInfo> GetMonitors()
        {
            var result = new List<ScreenInfo>();
            int index = 0;
            Vanara.PInvoke.User32.EnumDisplayMonitors(IntPtr.Zero, null,
                delegate (IntPtr hMonitor, IntPtr hdc, PRECT lprect, IntPtr lParam)
                {
                    MONITORINFOEX mi = new MONITORINFOEX();
                    mi.cbSize = (uint)Marshal.SizeOf<MONITORINFOEX>();
                    bool success = GetMonitorInfo(hMonitor, ref mi);
                    if (success)
                    {
                        var info = new ScreenInfo
                        {
                            Index = index++,
                            Handle = (int)hMonitor,
                            IsPrimary = (mi.dwFlags & MonitorInfoFlags.MONITORINFOF_PRIMARY) != 0,
                            Area = new System.Drawing.Rectangle(
                                mi.rcMonitor.left,
                                mi.rcMonitor.top,
                                mi.rcMonitor.right - mi.rcMonitor.left,
                                mi.rcMonitor.bottom - mi.rcMonitor.top),
                            Name = mi.szDevice,
                        };
                        result.Add(info);
                    }
                    return true;
                }, IntPtr.Zero);
            return result;
        }
    }
}
