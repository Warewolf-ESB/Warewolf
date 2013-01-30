using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;
using Dev2.Network.Messages;

namespace Dev2.Network
{
    public interface IStudioNetworkMessageAggregator
    {
        void Publish(INetworkMessage message, bool async = true);
        void Publish(INetworkMessage message, IStudioNetworkChannelContext context, bool async = true);
        Guid Subscribe<T>(Action<T, IStudioNetworkChannelContext> callback) where T : INetworkMessage, new();
        bool Unsubscibe(Guid subscriptionToken);
    }
}
