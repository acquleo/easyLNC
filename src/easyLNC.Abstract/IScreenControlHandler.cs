using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface IScreenControlHandler
    {
        /// <summary>
        /// Handles the event when the mouse pointer enters the specified screen.
        /// </summary>
        /// <param name="screen">The screen that the mouse pointer has entered. Cannot be null.</param>
        void MouseEnter(ScreenInfo screen);
        /// <summary>
        /// Handles the event when the mouse pointer leaves the specified screen.
        /// </summary>
        /// <param name="screen">The screen from which the mouse pointer has departed. Cannot be null.</param>
        void MauseLeave(ScreenInfo screen);
        
    }
}
