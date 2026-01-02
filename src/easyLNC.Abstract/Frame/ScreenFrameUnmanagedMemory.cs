using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract.Frame
{
    public class ScreenFrameUnmanagedMemory : ScreenFrame
    {
        public int DataLength { get; set; }
        public nint Data { get; set; }
    }
}
