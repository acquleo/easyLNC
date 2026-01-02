using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface IClientSession
    {
        public string Id { get; }
        IEnumerable<IScreenCapture> GetScreenCaptures();

    }
}
