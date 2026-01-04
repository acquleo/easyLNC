using easyLNC.Abstract;
using easyLNC.Abstract.Transport;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace easyLNC.ScreenControl.WinV1
{
    public class KeyboardMouseInputWinV1 : IScreenControlHandler
    {

        private readonly ConcurrentQueue<Action> _inputActions = new();
        private readonly AutoResetEvent _inputReadySignal = new(false);
        private volatile bool _inputBlocked;
        private Thread? _inputProcessingThread;
        private readonly IScreenInfoHandler screenInfoHandler;

        [Flags]
        private enum ShiftState : byte
        {
            None = 0,
            ShiftPressed = 1 << 0,
            CtrlPressed = 1 << 1,
            AltPressed = 1 << 2,
            HankakuPressed = 1 << 3,
            Reserved1 = 1 << 4,
            Reserved2 = 1 << 5,
        }
        public enum ButtonAction
        {
            Down,
            Up
        }

        /// <summary>
        ///  Specifies constants that define which mouse button was pressed.
        /// </summary>
        [Flags]
        public enum MouseButtons
        {
            /// <summary>
            ///  The left mouse button was pressed.
            /// </summary>
            Left = 0x00100000,

            /// <summary>
            ///  No mouse button was pressed.
            /// </summary>
            None = 0x00000000,

            /// <summary>
            ///  The right mouse button was pressed.
            /// </summary>
            Right = 0x00200000,

            /// <summary>
            ///  The middle mouse button was pressed.
            /// </summary>
            Middle = 0x00400000,

            XButton1 = 0x00800000,

            XButton2 = 0x01000000,
        }

        public KeyboardMouseInputWinV1(IScreenInfoHandler screenInfoHandler)
        {
            this.screenInfoHandler = screenInfoHandler;
            this.Init(CancellationToken.None);
        }

        public void Init(CancellationToken cancellationToken)
        {
            StartInputProcessingThread(cancellationToken);
        }

        public void SendKeyDown(string key)
        {
            TryOnInputDesktop(() =>
            {
                try
                {
                    try
                    {
                        if (!ConvertJavaScriptKeyToVirtualKey(key, out var vk))
                        {
                            return;
                        }
                        ;


                        var input = CreateKeyboardInput(vk.Value, true);
                        var sent = SendInput(1, [input], Marshal.SizeOf<INPUT>());


                    }
                    catch (Exception ex)
                    {

                    }
                }
                catch (Exception ex)
                {

                }
            });
        }

        public void SendKeyUp(string key)
        {
            TryOnInputDesktop(() =>
            {
                try
                {
                    if (!ConvertJavaScriptKeyToVirtualKey(key, out var vk))
                    {
                        return;
                    }
                    ;


                    var input = CreateKeyboardInput(vk.Value, false);
                    var sent = SendInput(1, [input], Marshal.SizeOf<INPUT>());


                }
                catch (Exception ex)
                {

                }
            });
        }
        static IEnumerable<Enum> GetFlags(Enum input)
        {
            foreach (Enum value in Enum.GetValues(input.GetType()))
                if (input.HasFlag(value))
                    yield return value;
        }

        public void VirtualKeyDown(VirtualKeyDown msg, ScreenInfo screen)
        {
            Console.WriteLine("SendKeyDown: " + msg.VirtualKey.ToString());
            TryOnInputDesktop(() =>
            {
                try
                {
                    try
                    {
                        List<INPUT> inputs = new List<INPUT>();
                        inputs.Add(CreateKeyboardInput((VK)msg.VirtualKey, true));

                        var sent = SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<INPUT>());


                    }
                    catch (Exception ex)
                    {

                    }
                }
                catch (Exception ex)
                {

                }
            });
        }

        public void VirtualKeyUp(VirtualKeyUp msg, ScreenInfo screen)
        {
            Console.WriteLine("SendKeyUp: " + msg.VirtualKey.ToString());
            TryOnInputDesktop(() =>
            {
                try
                {
                    List<INPUT> inputs = new List<INPUT>();
                    inputs.Add(CreateKeyboardInput((VK)msg.VirtualKey, false));

                    var sent = SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<INPUT>());


                }
                catch (Exception ex)
                {

                }
            });

        }
        public void MouseButtonAction(MouseButtonAction msg, ScreenInfo viewer)
        {
            TryOnInputDesktop(() =>
            {
                try
                {
                    MOUSEEVENTF mouseEvent;
                    switch ((MouseButtons)msg.Button)
                    {
                        case MouseButtons.Left:
                            switch ((ButtonAction)msg.Action)
                            {
                                case ButtonAction.Down:
                                    mouseEvent = MOUSEEVENTF.MOUSEEVENTF_LEFTDOWN;
                                    break;
                                case ButtonAction.Up:
                                    mouseEvent = MOUSEEVENTF.MOUSEEVENTF_LEFTUP;
                                    break;
                                default:
                                    return;
                            }
                            break;
                        case MouseButtons.Middle:
                            switch ((ButtonAction)msg.Action)
                            {
                                case ButtonAction.Down:
                                    mouseEvent = MOUSEEVENTF.MOUSEEVENTF_MIDDLEDOWN;
                                    break;
                                case ButtonAction.Up:
                                    mouseEvent = MOUSEEVENTF.MOUSEEVENTF_MIDDLEUP;
                                    break;
                                default:
                                    return;
                            }
                            break;
                        case MouseButtons.Right:
                            switch ((ButtonAction)msg.Action)
                            {
                                case ButtonAction.Down:
                                    mouseEvent = MOUSEEVENTF.MOUSEEVENTF_RIGHTDOWN;
                                    break;
                                case ButtonAction.Up:
                                    mouseEvent = MOUSEEVENTF.MOUSEEVENTF_RIGHTUP;
                                    break;
                                default:
                                    return;
                            }
                            break;
                        default:
                            return;
                    }
                    var xyPercent = GetAbsolutePercentFromRelativePercent(msg.X, msg.Y, viewer);
                    // Coordinates must be normalized.  The bottom-right coordinate is mapped to 65535.
                    var normalizedX = xyPercent.Item1 * 65535D;
                    var normalizedY = xyPercent.Item2 * 65535D;
                    var mi = new MOUSEINPUT()
                    {
                        dwFlags = MOUSEEVENTF.MOUSEEVENTF_ABSOLUTE | mouseEvent | MOUSEEVENTF.MOUSEEVENTF_VIRTUALDESK,
                        dx = (int)normalizedX,
                        dy = (int)normalizedY,
                        time = 0,
                        mouseData = 0,
                        dwExtraInfo = GetMessageExtraInfo()
                    };
                    var input = new INPUT() { type = INPUTTYPE.INPUT_MOUSE, mi = mi };
                    var sent = SendInput(1, [input], Marshal.SizeOf<INPUT>());
                    if (sent == 0)
                    {

                    }
                }
                catch (Exception ex)
                {

                }
            });
        }

        public void MouseMove(MouseMove message, ScreenInfo viewer)
        {
            TryOnInputDesktop(() =>
            {
                try
                {
                    if (!SwitchToInputDesktop())
                    {

                    }

                    var xyPercent = GetAbsolutePercentFromRelativePercent(message.AbsoluteX, message.AbsoluteY, viewer);
                    // Coordinates must be normalized.  The bottom-right coordinate is mapped to 65535.
                    var normalizedX = xyPercent.Item1 * 65535D;
                    var normalizedY = xyPercent.Item2 * 65535D;
                    var mi = new MOUSEINPUT() { dwFlags = MOUSEEVENTF.MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF.MOUSEEVENTF_MOVE | MOUSEEVENTF.MOUSEEVENTF_VIRTUALDESK, dx = (int)normalizedX, dy = (int)normalizedY, time = 0, mouseData = 0, dwExtraInfo = GetMessageExtraInfo() };
                    var input = new INPUT() { type = INPUTTYPE.INPUT_MOUSE, mi = mi };
                    var sent = SendInput(1, [input], Marshal.SizeOf<INPUT>());

                }
                catch (Exception ex)
                {

                }
            });
        }

        public void SendMouseMoveRelative(double x, double y, ScreenInfo viewer)
        {
            TryOnInputDesktop(() =>
            {
                try
                {
                    if (!SwitchToInputDesktop())
                    {

                    }

                    // Coordinates must be normalized.  The bottom-right coordinate is mapped to 65535.
                    var mi = new MOUSEINPUT() { dwFlags = MOUSEEVENTF.MOUSEEVENTF_MOVE, dx = (int)x, dy = (int)y, time = 0, mouseData = 0, dwExtraInfo = GetMessageExtraInfo() };
                    var input = new INPUT() { type = INPUTTYPE.INPUT_MOUSE, mi = mi };
                    var sent = SendInput(1, [input], Marshal.SizeOf<INPUT>());

                }
                catch (Exception ex)
                {

                }
            });
        }

        public void MouseWheel(MouseWheel mouseWheel, ScreenInfo viewer)
        {
            TryOnInputDesktop(() =>
            {
                try
                {
                    if (mouseWheel.Delta < 0)
                    {
                        mouseWheel.Delta = -120;
                    }
                    else if (mouseWheel.Delta > 0)
                    {
                        mouseWheel.Delta = 120;
                    }
                    var input = new INPUT() { type = INPUTTYPE.INPUT_MOUSE, mi = new 
                        MOUSEINPUT() 
                    { dwFlags = MOUSEEVENTF.MOUSEEVENTF_WHEEL, dx = 0, dy = 0, time = 0, mouseData = mouseWheel.Delta, dwExtraInfo = GetMessageExtraInfo() } };
                    var sent = SendInput(1, [input], Marshal.SizeOf<INPUT>());

                }
                catch (Exception ex)
                {

                }
            });
        }

        public void SendText(string transferText)
        {
            TryOnInputDesktop(() =>
            {
                try
                {
                    foreach (var character in transferText)
                    {
                        var keyCode = Convert.ToUInt16(character);

                        var keyDown = CreateKeyboardInput(keyCode, true);
                        var result = SendInput(1, [keyDown], Marshal.SizeOf<INPUT>());
                        if (result != 1)
                        {

                            break;
                        }

                        var keyUp = CreateKeyboardInput(keyCode, false);
                        result = SendInput(1, [keyUp], Marshal.SizeOf<INPUT>());
                        if (result != 1)
                        {

                            break;
                        }

                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {

                }
            });
        }

        public void SetKeyStatesUp()
        {
            TryOnInputDesktop(() =>
            {
                foreach (VK key in Enum.GetValues(typeof(VK)))
                {
                    try
                    {
                        // Skip mouse buttons and toggleable keys.
                        switch (key)
                        {
                            case VK.VK_LBUTTON:
                            case VK.VK_RBUTTON:
                            case VK.VK_MBUTTON:
                            case VK.VK_NUMLOCK:
                            case VK.VK_CAPITAL:
                            case VK.VK_SCROLL:
                                continue;
                            default:
                                break;
                        }
                        var (isPressed, isToggled) = GetKeyPressState(key);
                        if (isPressed || isToggled)
                        {
                            var input = CreateKeyboardInput(key, false);
                            var sent = SendInput(1, [input], Marshal.SizeOf<INPUT>());

                            Thread.Sleep(1);
                        }
                    }
                    catch { }
                }
            });
        }

        public void ToggleBlockInput(bool toggleOn)
        {
            TryOnInputDesktop(() =>
            {
                _inputBlocked = toggleOn;
                var result = BlockInput(toggleOn);
            });
        }

        private static INPUT CreateKeyboardInput(
            VK virtualKey,
            bool isPressed)
        {
            KEYEVENTF flags = 0;

            if (IsExtendedKey(virtualKey))
            {
                flags |= KEYEVENTF.KEYEVENTF_EXTENDEDKEY;
            }

            if (!isPressed)
            {
                flags |= KEYEVENTF.KEYEVENTF_KEYUP;
            }

            return new INPUT()
            {
                type = INPUTTYPE.INPUT_KEYBOARD,
                ki = new KEYBDINPUT()
                {
                    wVk = (ushort)virtualKey,
                    wScan = (ushort)MapVirtualKeyEx((uint)virtualKey, MAPVK.MAPVK_VK_TO_VSC_EX, GetKeyboardLayout((uint)Environment.CurrentManagedThreadId)),
                    dwExtraInfo = GetMessageExtraInfo(),

                    dwFlags = flags,
                    time = 0
                }
            };
        }

        private static INPUT CreateKeyboardInput(ushort unicodeKey, bool isPressed)
        {
            var flags = KEYEVENTF.KEYEVENTF_UNICODE;
            if (!isPressed)
            {
                flags |= KEYEVENTF.KEYEVENTF_KEYUP;
            }

            return new INPUT()
            {
                type = INPUTTYPE.INPUT_KEYBOARD,
                ki = new KEYBDINPUT()
                {
                    wVk = 0,
                    wScan = unicodeKey,
                    dwFlags = flags,
                    dwExtraInfo = GetMessageExtraInfo()
                }
            };
        }

        public Rectangle GetVirtualScreenBounds()
        {
            var displays = this.screenInfoHandler.GetScreens();
            var lowestX = 0;
            var highestX = 0;
            var lowestY = 0;
            var highestY = 0;

            foreach (var display in displays)
            {
                lowestX = Math.Min(display.Area.Left, lowestX);
                highestX = Math.Max(display.Area.Right, highestX);
                lowestY = Math.Min(display.Area.Top, lowestY);
                highestY = Math.Max(display.Area.Bottom, highestY);
            }

            return new Rectangle(lowestX, lowestY, highestX - lowestX, highestY - lowestY);
        }

        private Tuple<double, double> GetAbsolutePercentFromRelativePercent(double percentX, double percentY, ScreenInfo capturer)
        {
            var absoluteX = capturer.Area.Width * percentX + capturer.Area.Left - GetVirtualScreenBounds().Left;
            var absoluteY = capturer.Area.Height * percentY + capturer.Area.Top - GetVirtualScreenBounds().Top;
            return new Tuple<double, double>(absoluteX / GetVirtualScreenBounds().Width, absoluteY / GetVirtualScreenBounds().Height);
        }

        private static (bool Pressed, bool Toggled) GetKeyPressState(VK vkey)
        {
            // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getkeystate#return-value
            var state = User32.GetKeyState((int)vkey);
            var pressed = state < 0;
            var toggled = (state & 1) != 0;
            return (pressed, toggled);
        }

        private static bool IsExtendedKey(VK virtualKey)
        {
            return virtualKey switch
            {
                VK.VK_SHIFT or
                VK.VK_CONTROL or
                VK.VK_MENU or
                VK.VK_RCONTROL or
                VK.VK_RMENU or
                VK.VK_INSERT or
                VK.VK_DELETE or
                VK.VK_HOME or
                VK.VK_END or
                VK.VK_PRIOR or
                VK.VK_NEXT or
                VK.VK_LEFT or
                VK.VK_RIGHT or
                VK.VK_UP or
                VK.VK_DOWN or
                VK.VK_NUMLOCK or
                VK.VK_CANCEL or
                VK.VK_DIVIDE or
                VK.VK_SNAPSHOT or
                VK.VK_RETURN => true,
                _ => false
            };
        }

        private bool ConvertJavaScriptKeyToVirtualKey(string key, [NotNullWhen(true)] out VK? result)
        {
            result = key switch
            {
                " " => VK.VK_SPACE,
                "Down" or "ArrowDown" => VK.VK_DOWN,
                "Up" or "ArrowUp" => VK.VK_UP,
                "Left" or "ArrowLeft" => VK.VK_LEFT,
                "Right" or "ArrowRight" => VK.VK_RIGHT,
                "Enter" => VK.VK_RETURN,
                "Esc" or "Escape" => VK.VK_ESCAPE,
                "Alt" => VK.VK_MENU,
                "Control" => VK.VK_CONTROL,
                "Shift" => VK.VK_SHIFT,
                "PAUSE" => VK.VK_PAUSE,
                "BREAK" => VK.VK_PAUSE,
                "Backspace" => VK.VK_BACK,
                "Tab" => VK.VK_TAB,
                "CapsLock" => VK.VK_CAPITAL,
                "Delete" => VK.VK_DELETE,
                "Home" => VK.VK_HOME,
                "End" => VK.VK_END,
                "PageUp" => VK.VK_PRIOR,
                "PageDown" => VK.VK_NEXT,
                "NumLock" => VK.VK_NUMLOCK,
                "Insert" => VK.VK_INSERT,
                "ScrollLock" => VK.VK_SCROLL,
                "F1" => VK.VK_F1,
                "F2" => VK.VK_F2,
                "F3" => VK.VK_F3,
                "F4" => VK.VK_F4,
                "F5" => VK.VK_F5,
                "F6" => VK.VK_F6,
                "F7" => VK.VK_F7,
                "F8" => VK.VK_F8,
                "F9" => VK.VK_F9,
                "F10" => VK.VK_F10,
                "F11" => VK.VK_F11,
                "F12" => VK.VK_F12,
                "Meta" => VK.VK_LWIN,
                "ContextMenu" => VK.VK_MENU,
                _ => key.Length == 1 ?
                        (VK)User32.VkKeyScanW(Convert.ToChar(key)) :
                        null
            };

            if (result is null)
            {
                return false;
            }
            return true;
        }

        private void ProcessQueue(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    _inputReadySignal.WaitOne();
                    while (_inputActions.TryDequeue(out var action))
                    {
                        action();
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {

                }
            }


        }
        private void StartInputProcessingThread(CancellationToken cancelToken)
        {
            // After BlockInput is enabled, only simulated input coming from the same thread
            // will work.  So we have to start a new thread that runs continuously and
            // processes a queue of input events.
            _inputProcessingThread = new Thread(() =>
            {

                if (_inputBlocked && !BlockInput(true))
                {

                }
                ProcessQueue(cancelToken);
            });

            _inputProcessingThread.SetApartmentState(ApartmentState.MTA);
            _inputProcessingThread.Start();
        }

        private void TryOnInputDesktop(Action inputAction)
        {
            _inputActions.Enqueue(() =>
            {
                try
                {
                    var switchResult = SwitchToInputDesktop();

                    // Try to perform the dequeued action whether or not the switch was successful.
                    inputAction();

                    if (!switchResult)
                    {

                        // Thread likely has hooks in current desktop.  SendKeys will create one with no way to unhook it.
                        // Start a new thread for processing input.

                        CancellationTokenSource cancellationToken = new CancellationTokenSource();
                        StartInputProcessingThread(cancellationToken.Token);
                        return;
                    }

                }
                catch (Exception ex)
                {

                }
            });
            _inputReadySignal.Set();
        }
        [StructLayout(LayoutKind.Explicit)]
        private struct ShortHelper(short value)
        {
            [FieldOffset(0)]
            public short Value = value;
            [FieldOffset(0)]
            public byte Low;
            [FieldOffset(1)]
            public byte High;
        }

        public static bool SwitchToInputDesktop()
        {
            try
            {
                var inputDesktop = OpenInputDesktop();

                try
                {
                    if (inputDesktop == nint.Zero)
                    {
                        return false;
                    }

                    return SetThreadDesktop(inputDesktop);
                }
                finally
                {
                    CloseDesktop(inputDesktop);
                }
            }
            catch
            {
                return false;
            }
        }

        public static nint OpenInputDesktop()
        {
            return User32.OpenInputDesktop(CreateDesktopFlags.DF_ALLOWOTHERACCOUNTHOOK, true, ACCESS_MASK.GENERIC_ALL).DangerousGetHandle();
        }

        public void MouseEnter(MouseEnterScreen mouseEnterScreenMessage, ScreenInfo screen)
        {
            //nothing to do
        }

        public void MouseLeave(MouseLeaveScreen mouseLeaveScreenMessage, ScreenInfo screen)
        {
            //nothing to do
        }

    }
}
