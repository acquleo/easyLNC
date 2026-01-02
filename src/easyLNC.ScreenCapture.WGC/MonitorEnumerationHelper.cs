using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace easyLNC.ScreenCapture.WGC
{
    internal class MonitorEnumerationHelper
    {
        public static IEnumerable<WindowsCaptureScreen> GetMonitors()
        {
            var result = new List<WindowsCaptureScreen>();

            Vanara.PInvoke.User32.EnumDisplayMonitors(IntPtr.Zero, null,
                delegate (IntPtr hMonitor, IntPtr hdc, PRECT lprect, IntPtr lParam)
                {
                    MONITORINFOEX mi = new MONITORINFOEX();
                    mi.cbSize = (uint)Marshal.SizeOf<MONITORINFOEX>();
                    bool success = GetMonitorInfo(hMonitor, ref mi);
                    if (success)
                    {
                        var info = new WindowsCaptureScreen
                        {
                            IsPrimary = (mi.dwFlags & MonitorInfoFlags.MONITORINFOF_PRIMARY) != 0,
                            Height = mi.rcMonitor.bottom - mi.rcMonitor.top,
                            Width = mi.rcMonitor.right - mi.rcMonitor.left,
                            Id = hMonitor.ToString(),
                            Name = mi.szDevice,
                            X = mi.rcMonitor.left,
                            Y = mi.rcMonitor.top,
                        };
                        result.Add(info);
                    }
                    return true;
                }, IntPtr.Zero);
            return result;
        }
    }
}
