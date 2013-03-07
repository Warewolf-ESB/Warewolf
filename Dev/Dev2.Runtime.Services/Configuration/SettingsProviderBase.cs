using System;
using Dev2.Network;
using Dev2.Network.Messaging.Messages;
using Dev2.Runtime.Configuration.Settings;

namespace Dev2.Runtime.Configuration
{
    public abstract class SettingsProviderBase : ISettingsProvider
    {
        protected SettingsProviderBase()
        {
            Logging = new LoggingSettings();
            Security = new SecuritySettings();
            Backup = new BackupSettings();
        }

        public Guid SubscriptionToken { get; private set; }

        public ILoggingSettings Logging { get; private set; }
        public ISecuritySettings Security { get; private set; }
        public IBackupSettings Backup { get; private set; }

        public abstract ISettingsMessage ProcessMessage(ISettingsMessage request);

        #region Start

        public void Start<TContext>(IServerNetworkMessageAggregator<TContext> aggregator, INetworkMessageBroker messageBroker)
            where TContext : NetworkContext, new()
        {
            if(aggregator == null)
            {
                throw new ArgumentNullException("aggregator");
            }
            if(messageBroker == null)
            {
                throw new ArgumentNullException("messageBroker");
            }
            SubscriptionToken = aggregator.Subscribe((ISettingsMessage request, NetworkContext context) =>
            {
                try
                {
                    var result = ProcessMessage(request);
                    result.Handle = request.Handle;
                    messageBroker.Send(result, context);
                }
                catch(Exception ex)
                {
                    var error = new ErrorMessage(request.Handle, ex.Message);
                    messageBroker.Send(error, context);
                }
            });
        }

        #endregion

        #region Stop

        public void Stop<TContext>(IServerNetworkMessageAggregator<TContext> aggregator)
            where TContext : NetworkContext, new()
        {
            if(aggregator == null)
            {
                throw new ArgumentNullException("aggregator");
            }

            aggregator.Unsubscibe(SubscriptionToken);
        }

        #endregion
    }
}