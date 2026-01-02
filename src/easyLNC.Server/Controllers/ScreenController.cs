using easyLNC.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScreenController : ControllerBase
    {
        IScreenInfoHandler screenInfoHandler;
        public ScreenController(IScreenInfoHandler screenInfoHandler)
        {
            this.screenInfoHandler = screenInfoHandler;
        }

        [HttpGet(Name = "GetScreens")]
        public IEnumerable<ScreenInfo> Get()
        {
            return screenInfoHandler.GetScreens();
        }
    }
}