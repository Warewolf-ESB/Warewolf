using System;
using Dev2.Network;

namespace Dev2.Runtime.Configuration
{
    public interface ISettingsProvider
    {
        ILoggingSettings Logging { get; }
        ISecuritySettings Security { get; }
        IBackupSettings Backup { get; }

        void Start<TContext>(IServerNetworkMessageAggregator<TContext> aggregator, INetworkMessageBroker messageBroker)
            where TContext : NetworkContext, new();

        void Stop<TContext>(IServerNetworkMessageAggregator<TContext> aggregator)
            where TContext : NetworkContext, new();
    }
}
