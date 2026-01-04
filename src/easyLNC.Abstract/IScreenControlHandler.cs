using easyLNC.Abstract.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface IScreenControlHandler
    {
        void MouseEnter(MouseEnterScreen mouseEnterScreen, ScreenInfo screen);
        void MouseLeave(MouseLeaveScreen mouseLeaveScreen, ScreenInfo screen);
        void MouseMove(MouseMove mouseMove, ScreenInfo screen);
        void MouseButtonAction(MouseButtonAction mouseButtonAction, ScreenInfo screen);
        void MouseWheel(MouseWheel mouseWheel, ScreenInfo screen);
        void VirtualKeyDown(VirtualKeyDown sendVirtualKeyDown, ScreenInfo screen);
        void VirtualKeyUp(VirtualKeyUp sendVirtualKeyUp, ScreenInfo screen);

    }
}
