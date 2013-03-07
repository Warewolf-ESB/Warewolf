using System;
using Dev2.Network;
using Dev2.Network.Messaging.Messages;

namespace Dev2.Runtime.Configuration
{
    public interface ISettingsProvider
    {
        Guid SubscriptionToken { get; }

        ILoggingSettings Logging { get; }
        ISecuritySettings Security { get; }
        IBackupSettings Backup { get; }

        ISettingsMessage ProcessMessage(ISettingsMessage request);

        void Start<TContext>(IServerNetworkMessageAggregator<TContext> aggregator, INetworkMessageBroker messageBroker)
            where TContext : NetworkContext, new();

        void Stop<TContext>(IServerNetworkMessageAggregator<TContext> aggregator)
            where TContext : NetworkContext, new();
    }
}
