using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public class ScreenInfo
    {
        public int Index { get; set; }
        public int Handle { get; set; }
        public string Name { get; set; }=string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsPrimary { get; set; }

    }
}
