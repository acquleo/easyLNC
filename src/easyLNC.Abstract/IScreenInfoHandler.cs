using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="screen"></param>
        /// <returns></returns>
        bool GetScreen(int index, [NotNullWhen(true)] out ScreenInfo? screen);
    }
}
