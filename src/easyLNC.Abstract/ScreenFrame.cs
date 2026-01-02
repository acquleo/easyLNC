using easyLNC.Abstract.Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public abstract class ScreenFrame
    {
        public FrameTypes Type { get; set; }
        public PixelFormats Format { get; set; }
        public int Stride { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public TimeSpan Timestamp { get; set; } = TimeSpan.Zero;
    }
}
