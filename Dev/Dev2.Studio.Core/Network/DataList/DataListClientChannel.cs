using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Network;
using Dev2.Network;
using Dev2.Network.Messages;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;

namespace Dev2.Studio.Core.Network.DataList
{
    public class DataListClientChannel : INetworkDataListChannel, IDisposable
    {
        #region Class Members

        private long _handles;

        private object _disposeGuard = new object();
        private bool _isDisposed = false;

        private ThreadLocal<object> _threadSpecificContext = new ThreadLocal<object>();
        private ConcurrentDictionary<long, SynchronousNetworkMessageToken> _pendingMessages = new ConcurrentDictionary<long, SynchronousNetworkMessageToken>();
        
        private Guid _serverID;
        private Guid _accountID;
        private TCPDispatchedClient _client;
        private ServerMessaging _serverMessaging;

        private Guid _readDataListResultMessageSubscriptionToken;
        private Guid _writeDataListResultMessageSubscriptionToken;
        private Guid _deleteDataListResultMessageSubscriptionToken;
        private Guid _PersistChildChainResultMessageSubscriptionToken;
        private Guid _networkContextDetachedMessageSubscriptionToken;
        private Guid _errorMessageSubscriptionToken;

        #endregion Class Members

        #region Constructor

        public DataListClientChannel(TCPDispatchedClient client)
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
        /// Writes a datalist.
        /// </summary>
        /// <param name="datalistID">The datalist ID.</param>
        /// <param name="datalist">The datalist.</param>
        /// <param name="errors">The errors.</param>
        public bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors)
        {
            if (datalist == null)
            {
                throw new ArgumentNullException("datalist");
            }

            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposed.");
                }
            }

            WriteDataListMessage message = new WriteDataListMessage()
            {
                DatalistID = datalistID,
                Datalist = datalist,
                Errors = errors,
            };

            WriteDataListResultMessage resultMessage = SendSynchronousMessage(message) as WriteDataListResultMessage;

            bool result = false;
            
            if (resultMessage != null)
            {
                result = resultMessage.Result;
                UpdateErrorResultTO(errors, resultMessage.Errors);
            }

            return result;
        }

        /// <summary>
        /// Reads a datalist.
        /// </summary>
        /// <param name="datalistID">The datalist ID.</param>
        /// <param name="errors">The errors.</param>
        public IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposed.");
                }
            }

            ReadDataListMessage message = new ReadDataListMessage()
            {
                DatalistID = datalistID,
                Errors = errors,
            };

            ReadDataListResultMessage resultMessage = SendSynchronousMessage(message) as ReadDataListResultMessage;

            IBinaryDataList result = null;

            if (resultMessage != null)
            {
                result = resultMessage.Datalist;
                UpdateErrorResultTO(errors, resultMessage.Errors);
            }

            return result;
        }

        /// <summary>
        /// Deletes the datalist.
        /// </summary>
        /// <param name="id">The datalist ID.</param>
        /// <param name="onlyIfNotPersisted">if set to <c>true</c> [only if not persisted].</param>
        public void DeleteDataList(Guid id, bool onlyIfNotPersisted)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposed.");
                }
            }

            DeleteDataListMessage message = new DeleteDataListMessage()
            {
                ID = id,
                OnlyIfNotPersisted = onlyIfNotPersisted,
            };

            DeleteDataListResultMessage resultMessage = SendSynchronousMessage(message) as DeleteDataListResultMessage;
        }

        /// <summary>
        /// Persists the chain of child datalists.
        /// </summary>
        /// <param name="id">The datalist ID.</param>
        public bool PersistChildChain(Guid id)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposed.");
                }
            }

            PersistChildChainMessage message = new PersistChildChainMessage()
            {
                ID = id,
            };

            PersistChildChainResultMessage resultMessage = SendSynchronousMessage(message) as PersistChildChainResultMessage;

            bool result = false;

            if (resultMessage != null)
            {
                result = resultMessage.Result;
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
            _readDataListResultMessageSubscriptionToken = _serverMessaging.MessageAggregator.Subscribe(new Action<ReadDataListResultMessage, IStudioNetworkChannelContext>(OnReadDataListResultMessageRecieved));
            _writeDataListResultMessageSubscriptionToken = _serverMessaging.MessageAggregator.Subscribe(new Action<WriteDataListResultMessage, IStudioNetworkChannelContext>(OnWriteDataListResultMessageRecieved));
            _deleteDataListResultMessageSubscriptionToken = _serverMessaging.MessageAggregator.Subscribe(new Action<DeleteDataListResultMessage, IStudioNetworkChannelContext>(OnDeleteDataListResultMessageRecieved));
            _PersistChildChainResultMessageSubscriptionToken = _serverMessaging.MessageAggregator.Subscribe(new Action<PersistChildChainResultMessage, IStudioNetworkChannelContext>(OnPersistChildChainResultMessageRecieved));
            _networkContextDetachedMessageSubscriptionToken = _serverMessaging.MessageAggregator.Subscribe(new Action<NetworkContextDetachedMessage, IStudioNetworkChannelContext>(NetworkContextDetachedMessageRecieved));
            _errorMessageSubscriptionToken = _serverMessaging.MessageAggregator.Subscribe(new Action<ErrorMessage, IStudioNetworkChannelContext>(ErrorMessageRecieved));
        }

        /// <summary>
        /// Handler for recieving read results.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="serverID">The server ID.</param>
        private void OnReadDataListResultMessageRecieved(ReadDataListResultMessage message, IStudioNetworkChannelContext context)
        {
            RecieveSynchronousMessage(message, context);
        }

        /// <summary>
        /// Handler for recieving write results.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="serverID">The server ID.</param>
        private void OnWriteDataListResultMessageRecieved(WriteDataListResultMessage message, IStudioNetworkChannelContext context)
        {
            RecieveSynchronousMessage(message, context);
        }

        /// <summary>
        /// Handler for recieving delete results.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="serverID">The server ID.</param>
        private void OnDeleteDataListResultMessageRecieved(DeleteDataListResultMessage message, IStudioNetworkChannelContext context)
        {
            RecieveSynchronousMessage(message, context);
        }

        /// <summary>
        /// Handler for recieving persist chaild chain results.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="serverID">The server ID.</param>
        private void OnPersistChildChainResultMessageRecieved(PersistChildChainResultMessage message, IStudioNetworkChannelContext context)
        {
            RecieveSynchronousMessage(message, context);
        }

        /// <summary>
        /// Handler for recieving network context detached messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="serverID">The server ID.</param>
        private void NetworkContextDetachedMessageRecieved(NetworkContextDetachedMessage message, IStudioNetworkChannelContext context)
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
            Dispose();
        }

        /// <summary>
        /// Handler for recieving error messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="serverID">The server ID.</param>
        private void ErrorMessageRecieved(ErrorMessage message, IStudioNetworkChannelContext context)
        {
            RecieveSynchronousMessage(message, context);
        }

        /// <summary>
        /// Sends the synchronous message.
        /// </summary>
        /// <typeparam name="T">Message Type</typeparam>
        /// <param name="message">The message.</param>
        /// <exception cref="System.InvalidOperationException">A duplicate message handle has been detected in the DataListChannel.</exception>
        private INetworkMessage SendSynchronousMessage<T>(T message) where T : INetworkMessage, new()
        {
            long handle = Interlocked.Increment(ref _handles);
            SynchronousNetworkMessageToken messageToken = new SynchronousNetworkMessageToken(handle);
            if (!_pendingMessages.TryAdd(handle, messageToken))
            {
                throw new InvalidOperationException("A duplicate message handle has been detected in the DataListChannel.");
            }

            message.Handle = handle;

            try
            {
                _serverMessaging.MessageBroker.Send<T>(message, _client);
            }
            catch
            {
                SynchronousNetworkMessageToken tmpToken;
                if (!_pendingMessages.TryRemove(handle, out tmpToken))
                {
                    tmpToken.SetResponse(null);
                }
            }
            return messageToken.WaitForResponse();
        }

        /// <summary>
        /// Completes the recieve of a synchronous message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void RecieveSynchronousMessage(INetworkMessage message, IStudioNetworkChannelContext context)
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

            if (message is ErrorMessage)
            {              
                //
                // Release all pening messages in the event of an error, this is to prevent the studio from hanging,
                // but should should be expanded to try and be more specific to a handle.
                //
                foreach (KeyValuePair<long, SynchronousNetworkMessageToken> item in _pendingMessages.ToList())
                {
                    SynchronousNetworkMessageToken messageToken;
                    if (_pendingMessages.TryRemove(item.Key, out messageToken))
                    {
                        messageToken.SetResponse(null);
                    }                    
                }
            }
            else
            {
                SynchronousNetworkMessageToken messageToken;
                if (_pendingMessages.TryRemove(message.Handle, out messageToken))
                {
                    messageToken.SetResponse(message);
                }
            }
        }

        /// <summary>
        /// Updates the error result TO.
        /// </summary>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        private void UpdateErrorResultTO(ErrorResultTO destination, ErrorResultTO source)
        {
            if (destination == null || source == null)
            {
                return;
            }

            destination.ClearErrors();
            if (source.HasErrors())
            {
                foreach (string error in source.FetchErrors())
                {
                    destination.AddError(error);
                }
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

                foreach (SynchronousNetworkMessageToken token in _pendingMessages.Values)
                {
                    try
                    {
                        token.Cancel();
                    }
                    catch
                    {
                        //Do nothing because it means that a result has aleady come in for the token and nothing needs to be done
                    }
                }
                
                _pendingMessages.Clear();
                _pendingMessages = null;

                _threadSpecificContext.Dispose();
                _threadSpecificContext = null;

                _client = null;

                _serverMessaging.MessageAggregator.Unsubscibe(_readDataListResultMessageSubscriptionToken);
                _serverMessaging.MessageAggregator.Unsubscibe(_writeDataListResultMessageSubscriptionToken);
                _serverMessaging.MessageAggregator.Unsubscibe(_deleteDataListResultMessageSubscriptionToken);
                _serverMessaging.MessageAggregator.Unsubscibe(_PersistChildChainResultMessageSubscriptionToken);
                _serverMessaging.MessageAggregator.Unsubscibe(_networkContextDetachedMessageSubscriptionToken);

                _serverMessaging = null;

                _isDisposed = true;
            }
        }

        #endregion Tear Down
    }
}
