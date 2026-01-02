using easyLNC.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.ScreenCapture.WGC
{
    public class WindowsCaptureScreen : ScreenInfo
    {
        public string Id { get; internal set; }=string.Empty;
        public string Name { get; internal set; }=string.Empty;
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int X { get; internal set; }
        public int Y { get; internal set; }
        public bool IsPrimary { get; internal set; }

    }
}
