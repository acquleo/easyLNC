using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface IClientSessionTransport
    {
        string Id { get; }
        event Action<IClientSessionTransport?> OnDisconnected;
    }
}
