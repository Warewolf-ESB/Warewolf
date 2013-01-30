using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;

namespace Dev2.Network
{
    public interface IServerNetworkChannelContext<ContextT> : INetworkChannelContext<TCPServer<ContextT>> where ContextT : NetworkContext, new()
    {
        ContextT NetworkContext { get; }
    }
}
