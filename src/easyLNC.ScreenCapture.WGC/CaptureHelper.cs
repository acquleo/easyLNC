using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using WinRT;

namespace easyLNC.ScreenCapture.WGC
{
    public static class CaptureHelper
    {
        //static readonly Guid GraphicsCaptureItemGuid = new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");

        [ComImport]
        [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        interface IInitializeWithWindow
        {
            void Initialize(
                IntPtr hwnd);
        }


        [ComImport]
        [Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        interface IGraphicsCaptureItemInterop
        {
            IntPtr CreateForWindow(
                [In] IntPtr window,
                [In] ref Guid iid);

            IntPtr CreateForMonitor(
                [In] IntPtr monitor,
                [In] ref Guid iid);
        }
        static readonly Guid GraphicsCaptureItemGuid = new Guid("79C3F95B-31F7-4EC2-A464-632EF5D30760");

        public static void SetWindow(this GraphicsCapturePicker picker, IntPtr hwnd)
        {
            var interop = picker.As<IInitializeWithWindow>();
            interop.Initialize(hwnd);
        }

        public static GraphicsCaptureItem CreateItemForMonitor(IntPtr hmon)
        {
            var interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
            var temp = typeof(GraphicsCaptureItem);
            var itemPointer = interop.CreateForMonitor(hmon, GraphicsCaptureItemGuid);
            var item = GraphicsCaptureItem.FromAbi(itemPointer);
            //var item = Marshal.GetObjectForIUnknown(itemPointer) as GraphicsCaptureItem;
            Marshal.Release(itemPointer);

            return item;

        }
    }
}
