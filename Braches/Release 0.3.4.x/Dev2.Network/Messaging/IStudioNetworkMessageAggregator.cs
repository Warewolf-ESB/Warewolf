using System;
using System.Network;
using Dev2.Network.Messaging;

namespace Dev2.Network
{
    public interface IStudioNetworkMessageAggregator
    {
        void Publish(INetworkMessage message, bool async = true);
        void Publish(INetworkMessage message, IStudioNetworkChannelContext context, bool async = true);
        Guid Subscribe<T>(Action<T, IStudioNetworkChannelContext> callback) where T : INetworkMessage, new();
        Guid Subscribe<T>(Action<T, INetworkOperator> callback) where T : INetworkMessage, new();
        bool Unsubscibe(Guid subscriptionToken);
    }
}
