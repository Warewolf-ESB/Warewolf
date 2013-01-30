using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;
using Dev2.Network.Messages;

namespace Dev2.Network
{
    public interface IServerNetworkMessageAggregator<ContextT> where ContextT : NetworkContext, new()
    {
        void Publish(INetworkMessage message, bool async = true);
        void Publish(INetworkMessage message, IServerNetworkChannelContext<ContextT> context, bool async = true);
        Guid Subscribe<T>(Action<T, IServerNetworkChannelContext<ContextT>> callback) where T : INetworkMessage, new();
        bool Unsubscibe(Guid subscriptionToken);
    }
}
