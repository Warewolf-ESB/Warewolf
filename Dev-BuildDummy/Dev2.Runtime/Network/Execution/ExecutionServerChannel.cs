using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;
using System.Threading;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Network.Messaging.Messages;

namespace Dev2.DynamicServices.Network.Execution
{
    public class ExecutionServerChannel : INetworkExecutionChannel, IDisposable
    {
        #region Class Members

        private object _disposeGuard = new object();
        private bool _isDisposed = false;
        
        ThreadLocal<object> _threadSpecificContext = new ThreadLocal<object>();
        private object _contextGuard = new object();
        private ConcurrentDictionary<Guid, List<Guid>> _registeredCallbackIDsIndexedByAccountID = new ConcurrentDictionary<Guid, List<Guid>>();
        private ConcurrentDictionary<Guid, StudioNetworkSession> _networkContextsIndexedByCallbackID = new ConcurrentDictionary<Guid, StudioNetworkSession>();

        private Guid _executionStatusCallbackMessageSubscriptionToken;
        private Guid _networkContextDetachedMessageSubscriptionToken;

        private INetworkMessageBroker _messageBroker;
        private IServerNetworkMessageAggregator<StudioNetworkSession> _messageAggregator;
        private IExecutionStatusCallbackDispatcher _executionStatusCallbackDispatcher;

        #endregion Class Members

        #region Constructor

        public ExecutionServerChannel(INetworkMessageBroker messageBroker, IServerNetworkMessageAggregator<StudioNetworkSession> messageAggregator, IExecutionStatusCallbackDispatcher executionStatusCallbackDispatcher)
        {
            if (messageBroker == null)
            {
                throw new ArgumentNullException("messageBroker");
            }

            if (messageAggregator == null)
            {
                throw new ArgumentNullException("messageAggregator");
            }

            if (executionStatusCallbackDispatcher == null)
            {
                throw new ArgumentNullException("executionStatusCallbackDispatcher");
            }

            _messageBroker = messageBroker;
            _messageAggregator = messageAggregator;
            _executionStatusCallbackDispatcher = executionStatusCallbackDispatcher;

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
        /// Gets the server context.
        /// </summary>
        /// <value>
        /// The server context.
        /// </value>
        private IServerNetworkChannelContext<StudioNetworkSession> ServerContext
        {
            get
            {
                return Context as IServerNetworkChannelContext<StudioNetworkSession>;
            }
        }

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
                return Guid.Empty;
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
                return Guid.Empty;
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

            if (ServerContext == null)
            {
                throw new NullReferenceException("No Context attached to channel");
            }

            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            bool result = false;

            if (_executionStatusCallbackDispatcher.Add(callbackID, callback))
            {
                lock (_contextGuard)
                {
                    List<Guid> callBackIDs;
                    if (!_registeredCallbackIDsIndexedByAccountID.TryGetValue(ServerContext.NetworkContext.AccountID, out callBackIDs))
                    {
                        callBackIDs = new List<Guid>();
                        _registeredCallbackIDsIndexedByAccountID.TryAdd(ServerContext.NetworkContext.AccountID, callBackIDs);
                    }
                    callBackIDs.Add(callbackID);

                    _networkContextsIndexedByCallbackID.TryAdd(callbackID, ServerContext.NetworkContext);
                }

                result = true;
            }

            return result;
        }

        /// <summary>
        /// Removes an execution status callback.
        /// </summary>
        /// <param name="callbackID">The callback ID.</param>
        public bool RemoveExecutionStatusCallback(Guid callbackID)
        {
            if (ServerContext == null)
            {
                throw new NullReferenceException("No Context attached to channel");
            }

            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            bool result = false;

            if (_executionStatusCallbackDispatcher.Remove(callbackID))
            {
                lock (_contextGuard)
                {
                    List<Guid> callBackIDs;
                    if (_registeredCallbackIDsIndexedByAccountID.TryRemove(ServerContext.NetworkContext.AccountID, out callBackIDs))
                    {
                        callBackIDs.Remove(callbackID);
                    }

                    StudioNetworkSession tmp;
                    _networkContextsIndexedByCallbackID.TryRemove(callbackID, out tmp);
                }

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
            _executionStatusCallbackMessageSubscriptionToken = _messageAggregator.Subscribe(new Action<ExecutionStatusCallbackMessage, IServerNetworkChannelContext<StudioNetworkSession>>(ExecutionStatusCallbackMessageRecieved));
            _networkContextDetachedMessageSubscriptionToken = _messageAggregator.Subscribe(new Action<NetworkContextDetachedMessage, IServerNetworkChannelContext<StudioNetworkSession>>(NetworkContextDetachedMessageRecieved));
        }

        /// <summary>
        /// Handler for recieving execution status callback messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="server">The server.</param>
        /// <param name="context">The context.</param>
        private void ExecutionStatusCallbackMessageRecieved(ExecutionStatusCallbackMessage message, IServerNetworkChannelContext<StudioNetworkSession> context)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    return;
                }
            }
            
            Context = context;

            if (message.MessageType == ExecutionStatusCallbackMessageType.Add)
            {
                AddExecutionStatusCallback(message.CallbackID, new Action<ExecutionStatusCallbackMessage>(m =>
                {
                    StudioNetworkSession studioNetworkSession;
                    if (_networkContextsIndexedByCallbackID.TryGetValue(m.CallbackID, out studioNetworkSession))
                    {
                        _messageBroker.Send(m, studioNetworkSession);
                    }
                }));
            }
            else if (message.MessageType == ExecutionStatusCallbackMessageType.Remove)
            {
                RemoveExecutionStatusCallback(message.CallbackID);
            }
        }

        /// <summary>
        /// Handler for recieving network context detached messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="server">The server.</param>
        /// <param name="context">The context.</param>
        private void NetworkContextDetachedMessageRecieved(NetworkContextDetachedMessage message, IServerNetworkChannelContext<StudioNetworkSession> context)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    return;
                }
            }

            Context = context;

            List<Guid> safeCallBackIDs = new List<Guid>();
            lock (_contextGuard)
            {
                List<Guid> callBackIDs;
                if (_registeredCallbackIDsIndexedByAccountID.TryGetValue(ServerContext.NetworkContext.AccountID, out callBackIDs))
                {
                    safeCallBackIDs.AddRange(callBackIDs);
                }
            }

            foreach (Guid item in safeCallBackIDs)
            {
                RemoveExecutionStatusCallback(item);
            }
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

                _registeredCallbackIDsIndexedByAccountID.Clear();
                _registeredCallbackIDsIndexedByAccountID = null;

                _networkContextsIndexedByCallbackID.Clear();
                _networkContextsIndexedByCallbackID = null;

                _messageAggregator.Unsubscibe(_executionStatusCallbackMessageSubscriptionToken);
                _messageAggregator.Unsubscibe(_networkContextDetachedMessageSubscriptionToken);

                _messageBroker = null;
                _messageAggregator = null;
                _executionStatusCallbackDispatcher = null;

                _isDisposed = true;
            }
        }

        #endregion Tear Down
    }
}
