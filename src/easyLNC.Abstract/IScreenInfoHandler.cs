using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface IScreenInfoHandler
    {
        /// <summary>
        /// Retrieves a collection of all available screens connected to the system.
        /// </summary>
        IEnumerable<ScreenInfo> GetScreens();
    }
}
