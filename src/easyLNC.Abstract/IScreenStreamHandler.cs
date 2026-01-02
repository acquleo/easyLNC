using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface IScreenStreamHandler
    {
        public void Attach(IScreenCapture screenCapture);
        public void Detach(IScreenCapture screenCapture);
    }
}
