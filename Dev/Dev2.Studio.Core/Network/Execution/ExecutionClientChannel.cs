using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using Dev2.Composition;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Network.Messaging.Messages;

namespace Dev2.Studio.Core.Network.Execution
{
    public class ExecutionClientChannel : INetworkExecutionChannel, IDisposable
    {
        #region Class Members

        private object _disposeGuard = new object();
        private bool _isDisposed = false;

        private ThreadLocal<object> _threadSpecificContext = new ThreadLocal<object>();
        private ConcurrentDictionary<Guid, object> _registeredCallbackIDs = new ConcurrentDictionary<Guid, object>();
        
        private Guid _serverID;
        private Guid _accountID;
        private TCPDispatchedClient _client;
        private ServerMessaging _serverMessaging;

        private Guid _executionStatusCallbackMessageSubscriptionToken;
        private Guid _networkContextDetachedMessageSubscriptionToken;

        #endregion Class Members

        #region Constructor

        public ExecutionClientChannel(TCPDispatchedClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            _client = client;
            _serverID = client.ServerID;
            _accountID = client.AccountID;

            Initialize();
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        public object Context
        {
            get
            {
                return _threadSpecificContext.Value;
            }
            set
            {
                _threadSpecificContext.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the execution status callback dispatcher.
        /// </summary>
        /// <value>
        /// The execution status callback dispatcher.
        /// </value>
        [Import]
        public IExecutionStatusCallbackDispatcher ExecutionStatusCallbackDispatcher { get; set; }

        /// <summary>
        /// Gets the account ID.
        /// </summary>
        /// <value>
        /// The account ID.
        /// </value>
        public Guid AccountID 
        {
            get
            {
                return _accountID;
            }
        }

        /// <summary>
        /// Gets the server ID.
        /// </summary>
        /// <value>
        /// The server ID.
        /// </value>
        public Guid ServerID 
        {
            get
            {
                return _serverID;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds an execution status callback.
        /// </summary>
        /// <param name="callbackID">The callback ID.</param>
        /// <param name="callback">The callback.</param>
        public bool AddExecutionStatusCallback(Guid callbackID, Action<ExecutionStatusCallbackMessage> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if (ExecutionStatusCallbackDispatcher == null)
            {
                throw new NullReferenceException("ExecutionStatusCallbackDispatcher is null.");
            }

            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposed.");
                }
            }

            bool result = false;

            if (ExecutionStatusCallbackDispatcher.Add(callbackID, callback))
            {
                _registeredCallbackIDs.TryAdd(callbackID, null);

                try
                {
                    ExecutionStatusCallbackMessage message = new ExecutionStatusCallbackMessage(callbackID, ExecutionStatusCallbackMessageType.Add);
                    _serverMessaging.MessageBroker.Send(message, _client);
                    result = true;
                }
                catch
                {
                    ExecutionStatusCallbackDispatcher.Remove(callbackID);
                }
            }
            return result;
        }

        /// <summary>
        /// Removes an execution status callback.
        /// </summary>
        /// <param name="callbackID">The callback ID.</param>
        public bool RemoveExecutionStatusCallback(Guid callbackID)
        {
            if (ExecutionStatusCallbackDispatcher == null)
            {
                throw new NullReferenceException("ExecutionStatusCallbackDispatcher is null.");
            }

            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposed.");
                }
            }

            bool result = false;

            if (ExecutionStatusCallbackDispatcher.Remove(callbackID))
            {
                object tmp;
                _registeredCallbackIDs.TryRemove(callbackID, out tmp);

                ExecutionStatusCallbackMessage message = new ExecutionStatusCallbackMessage(callbackID, ExecutionStatusCallbackMessageType.Remove);
                _serverMessaging.MessageBroker.Send(message, _client);
                result = true;
            }

            return result;
        }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            _serverMessaging = new ServerMessaging();
            _executionStatusCallbackMessageSubscriptionToken = _serverMessaging.MessageAggregator.Subscribe(new Action<ExecutionStatusCallbackMessage, IStudioNetworkChannelContext>(OnExecutionStatusCallbackMessageRecieved));
            _networkContextDetachedMessageSubscriptionToken = _serverMessaging.MessageAggregator.Subscribe(new Action<NetworkContextDetachedMessage, IStudioNetworkChannelContext>(OnNetworkContextDetachedMessageRecieved));

            ImportService.SatisfyImports(this);
        }

        /// <summary>
        /// Handler for recieving execution status callback messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="serverID">The server ID.</param>
        private void OnExecutionStatusCallbackMessageRecieved(ExecutionStatusCallbackMessage message, IStudioNetworkChannelContext context)
        {
            if (AccountID != context.Account || ServerID != context.Server)
            {
                return;
            }

            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    return;
                }
            }

            Context = context;
            ExecutionStatusCallbackDispatcher.Post(message);
        }

        /// <summary>
        /// Handler for recieving network context detached messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="serverID">The server ID.</param>
        private void OnNetworkContextDetachedMessageRecieved(NetworkContextDetachedMessage message, IStudioNetworkChannelContext context)
        {
            if (ServerID != context.Server)
            {
                return;
            }

            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    return;
                }
            }

            Context = context;
            ExecutionStatusCallbackDispatcher.RemoveRange(_registeredCallbackIDs.Select(k => k.Key).ToList());
            Dispose();
        }

        #endregion Private Methods

        #region Tear Down

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    return;
                }

                _threadSpecificContext.Dispose();
                _threadSpecificContext = null;

                _registeredCallbackIDs.Clear();
                _registeredCallbackIDs = null;

                ExecutionStatusCallbackDispatcher = null;
                
                _client = null;

                _serverMessaging.MessageAggregator.Unsubscibe(_executionStatusCallbackMessageSubscriptionToken);
                _serverMessaging.MessageAggregator.Unsubscibe(_networkContextDetachedMessageSubscriptionToken);

                _serverMessaging = null;

                _isDisposed = true;
            }
        }

        #endregion Tear Down
    }
}
