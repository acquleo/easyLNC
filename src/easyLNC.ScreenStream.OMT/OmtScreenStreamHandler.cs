using easyLNC.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace easyLNC.ScreenStream.OMT
{
    public class OmtScreenStreamHandler : IScreenStreamHandler
    {
        OmtScreenStream? stream;
        public OmtScreenStreamHandler()
        {

        }

        public void Attach(IScreenCapture screenCapture, out StreamInfo streamInfo)
        {
            if (stream != null)
            {
                stream?.Dispose();
            }

            stream = new OmtScreenStream(screenCapture);

            var url = $@"omt://{IpHelper.GetIPv4OfDefaultGateway()}:{stream.Port}";

            OmtScreenParams omtScreenParams = new OmtScreenParams
            {
                Url = url
            };

            Console.WriteLine($"OMT Stream URL: {url}");

            streamInfo = new StreamInfo
            {
                Type = "OMTv1",
                Params = JsonSerializer.Serialize<OmtScreenParams>(omtScreenParams)
            };

        }

        public void Detach(IScreenCapture screenCapture)
        {
            stream?.Dispose();
        }
    }
}
