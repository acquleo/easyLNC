using System;
using System.Runtime.InteropServices;
using System.Timers;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

public class DesktopWatcher : IDisposable
{
    private string _currentDesktop;
    private System.Timers.Timer _timer;

    public event Action<string, SafeHDESK >? DesktopChanged;

    public DesktopWatcher(double pollIntervalMs = 5000)
    {
        _timer = new System.Timers.Timer(pollIntervalMs);
        _timer.Elapsed += Timer_Elapsed;
        _timer.Start();

        _currentDesktop = GetDesktopName(GetCurrentDesktopHandle(out _));
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        var newDesk = GetDesktopName(GetCurrentDesktopHandle(out var hDesk));
        if (newDesk != _currentDesktop)
        {
            Console.WriteLine($@"desktop changed {newDesk} from {_currentDesktop}");

            _currentDesktop = newDesk;

            

            DesktopChanged?.Invoke(_currentDesktop.ToString(), hDesk);
        }
    }
    private static string GetDesktopName(IntPtr hDesk)
    {
        if (hDesk == IntPtr.Zero) return "Unknown";
        int length = 256;
        var sb = new System.Text.StringBuilder(length);
        var name = User32.GetUserObjectInformation<string>(hDesk, User32.UserObjectInformationType.UOI_NAME);
        
        return name;
    }
    private static IntPtr GetCurrentDesktopHandle(out SafeHDESK hDesk)
    {
        hDesk = User32.OpenInputDesktop(0, false, ACCESS_MASK.GENERIC_READ);
        // Open the input desktop (the one receiving input)
        if (hDesk)
        {
            return hDesk.DangerousGetHandle();
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
