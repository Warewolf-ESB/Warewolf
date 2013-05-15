using System;
using Dev2.Network.Messaging;

namespace Dev2.Network
{
    public interface IServerNetworkMessageAggregator<ContextT> where ContextT : NetworkContext, new()
    {
        void Publish(INetworkMessage message, bool async = true);
        void Publish(INetworkMessage message, IServerNetworkChannelContext<ContextT> context, bool async = true);
        Guid Subscribe<T>(Action<T, IServerNetworkChannelContext<ContextT>> callback) where T : INetworkMessage, new();
        Guid Subscribe<T>(Action<T, NetworkContext> callback) where T : INetworkMessage, new();
        bool Unsubscibe(Guid subscriptionToken);
    }
}
