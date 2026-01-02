using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace easyLNC.Abstract
{
    public interface IClientSessionTransportHandler
    {
        event Action<IClientSessionTransportHandler, IClientSessionTransport> OnConnected;
        event Action<IClientSessionTransportHandler, IClientSessionTransport> OnDisconnected;

        IEnumerable<IClientSessionTransport> GetClients();
        IClientSessionTransport GetClientById(string id);


    }
}
