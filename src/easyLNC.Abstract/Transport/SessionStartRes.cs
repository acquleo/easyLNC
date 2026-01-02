using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using acquleo.Protocol;

namespace easyLNC.Abstract.Transport
{
    public class SessionStartRes
    {
        public string SessionId { get; set; } = string.Empty;

        public List<ScreenInfo> Screens { get; set; } = new List<ScreenInfo>();
    }
}
