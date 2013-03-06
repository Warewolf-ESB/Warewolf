using System;
using Dev2.Network;
using Dev2.Runtime.Configuration.Settings;

namespace Dev2.Runtime.Configuration
{
    public class SettingsProvider : ISettingsProvider
    {
        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile SettingsProvider _instance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static SettingsProvider Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
                        {
                            _instance = new SettingsProvider();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        /// <summary>
        /// Do NOT instantiate directly - use static <see cref="Instance"/> property instead; use for testing only!
        /// </summary>
        public SettingsProvider()
        {
            Logging = new LoggingSettings();
            Security = new SecuritySettings();
            Backup = new BackupSettings();
        }

        public Guid SubscriptionToken { get; private set; }

        public ILoggingSettings Logging { get; private set; }

        public ISecuritySettings Security { get; private set; }

        public IBackupSettings Backup { get; private set; }

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
            SubscriptionToken = aggregator.Subscribe((ISettingsMessage message, NetworkContext context) =>
            {
                var result = message.Execute();
                messageBroker.Send(result, context);
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