using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.Abstract.Transport
{
    public partial class MouseButtonAction :BaseSessionReq
    {
        public int ScreenIndex { get; set; }
        public int Button { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Action { get; set; }

    }
}
