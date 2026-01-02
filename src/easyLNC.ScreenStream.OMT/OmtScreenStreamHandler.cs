using easyLNC.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.ScreenStream.OMT
{
    public class OmtScreenStreamHandler : IScreenStreamHandler
    {
        OmtScreenStream? stream;
        public OmtScreenStreamHandler()
        {

        }

        public void Attach(IScreenCapture screenCapture)
        {
            if (stream != null)
            {
                stream?.Dispose();
            }
            stream = new OmtScreenStream(screenCapture);

        }

        public void Detach(IScreenCapture screenCapture)
        {
            stream?.Dispose();
        }
    }
}
