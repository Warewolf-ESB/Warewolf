using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;

namespace Dev2.Network
{
    public interface IStudioNetworkChannelContext : INetworkChannelContext<Guid>
    {
        Guid Account { get; }
        INetworkOperator NetworkOperator { get; }
    }
}
