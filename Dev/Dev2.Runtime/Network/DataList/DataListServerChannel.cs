using System;
using System.Threading;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Network;
using Dev2.Network;
using Dev2.Network.Messaging.Messages;

namespace Dev2.DynamicServices.Network.DataList
{
    public class DataListServerChannel : INetworkDataListChannel, IDisposable
    {
        #region Class Members

        private object _disposeGuard = new object();
        private bool _isDisposed = false;

        ThreadLocal<object> _threadSpecificContext = new ThreadLocal<object>();

        private Guid _readDataListMessageSubscriptionToken;
        private Guid _writeDataListMessageSubscriptionToken;
        private Guid _deleteDataListMessageSubscriptionToken;
        private Guid _PersistChildChainMessageSubscriptionToken;
        private Guid _networkContextDetachedMessageSubscriptionToken;

        private INetworkMessageBroker _messageBroker;
        private IServerNetworkMessageAggregator<StudioNetworkSession> _messageAggregator;
        private IDataListServer _datalistServer;

        #endregion Class Members

        #region Constructor

        public DataListServerChannel(INetworkMessageBroker messageBroker, IServerNetworkMessageAggregator<StudioNetworkSession> messageAggregator, IDataListServer datalistServer)
        {
            if (messageBroker == null)
            {
                throw new ArgumentNullException("messageBroker");
            }

            if (messageAggregator == null)
            {
                throw new ArgumentNullException("messageAggregator");
            }

            if (datalistServer == null)
            {
                throw new ArgumentNullException("datalistServer");
            }

            _messageBroker = messageBroker;
            _messageAggregator = messageAggregator;
            _datalistServer = datalistServer;
            ServerID = Guid.Empty;

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
        #endregion Properties

        #region Methods

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
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            return _datalistServer.WriteDataList(datalistID, datalist, out errors);
        }

        public IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            return _datalistServer.ReadDatalist(datalistID, out errors);
        }

        public void DeleteDataList(Guid id, bool onlyIfNotPersisted)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            _datalistServer.DeleteDataList(id, onlyIfNotPersisted);
        }

        public Guid ServerID { get; private set; }

        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            _readDataListMessageSubscriptionToken = _messageAggregator.Subscribe(new Action<ReadDataListMessage, IServerNetworkChannelContext<StudioNetworkSession>>(OnReadDataListMessageRecieved));
            _writeDataListMessageSubscriptionToken = _messageAggregator.Subscribe(new Action<WriteDataListMessage, IServerNetworkChannelContext<StudioNetworkSession>>(OnWriteDataListMessageRecieved));
            _deleteDataListMessageSubscriptionToken = _messageAggregator.Subscribe(new Action<DeleteDataListMessage, IServerNetworkChannelContext<StudioNetworkSession>>(OnDeleteDataListMessageRecieved));
            _networkContextDetachedMessageSubscriptionToken = _messageAggregator.Subscribe(new Action<NetworkContextDetachedMessage, IServerNetworkChannelContext<StudioNetworkSession>>(NetworkContextDetachedMessageRecieved));
        }

        /// <summary>
        /// Handler for recieving execution read messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        private void OnReadDataListMessageRecieved(ReadDataListMessage message, IServerNetworkChannelContext<StudioNetworkSession> context)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    return;
                }
            }

            Context = context;

            ReadDataListResultMessage resultMessage;

            try
            {
                ErrorResultTO errors = new ErrorResultTO();
                IBinaryDataList dataList = ReadDatalist(message.DatalistID, errors);

                resultMessage = new ReadDataListResultMessage()
                {
                    Handle = message.Handle,
                    Datalist = dataList,
                    Errors = errors,
                };
            }
            catch (Exception e)
            {
                ErrorResultTO errors = new ErrorResultTO();
                errors.AddError(e.Message);

                resultMessage = new ReadDataListResultMessage()
                {
                    Handle = message.Handle,
                    Datalist = Dev2BinaryDataListFactory.CreateDataList(),
                    Errors = errors,
                };
            }
            
            _messageBroker.Send(resultMessage, context.NetworkContext);
        }

        /// <summary>
        /// Handler for recieving write messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="server">The server.</param>
        /// <param name="context">The context.</param>
        private void OnWriteDataListMessageRecieved(WriteDataListMessage message, IServerNetworkChannelContext<StudioNetworkSession> context)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    return;
                }
            }

            Context = context;

            WriteDataListResultMessage resultMessage;

            try
            {
                ErrorResultTO errors = new ErrorResultTO();
                bool result = WriteDataList(message.DatalistID, message.Datalist , errors);

                if (errors == null)
                {
                    errors = new ErrorResultTO();
                }

                resultMessage = new WriteDataListResultMessage()
                {
                    Handle = message.Handle,
                    Result = result,
                    Errors = errors,
                };
            }
            catch (Exception e)
            {
                ErrorResultTO errors = new ErrorResultTO();
                errors.AddError(e.Message);

                resultMessage = new WriteDataListResultMessage()
                {
                    Handle = message.Handle,
                    Result = false,
                    Errors = errors,
                };
            }

            _messageBroker.Send(resultMessage, context.NetworkContext);
        }

        /// <summary>
        /// Handler for recieving delete messages
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="server">The server.</param>
        /// <param name="context">The context.</param>
        private void OnDeleteDataListMessageRecieved(DeleteDataListMessage message, IServerNetworkChannelContext<StudioNetworkSession> context)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    return;
                }
            }

            Context = context;

            DeleteDataListResultMessage resultMessage;

            try
            {
                ErrorResultTO errors = new ErrorResultTO();
                DeleteDataList(message.ID, message.OnlyIfNotPersisted);

                if (errors == null)
                {
                    errors = new ErrorResultTO();
                }

                resultMessage = new DeleteDataListResultMessage()
                {
                    Handle = message.Handle,
                    Errors = errors,
                };
            }
            catch (Exception e)
            {
                ErrorResultTO errors = new ErrorResultTO();
                errors.AddError(e.Message);

                resultMessage = new DeleteDataListResultMessage()
                {
                    Handle = message.Handle,
                    Errors = errors,
                };
            }

            _messageBroker.Send(resultMessage, context.NetworkContext);
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
            // At the moment no resources are stored on this channel per client, if there were here would be the place to release them for when a client disconnects
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

                _messageAggregator.Unsubscibe(_readDataListMessageSubscriptionToken);
                _messageAggregator.Unsubscibe(_writeDataListMessageSubscriptionToken);
                _messageAggregator.Unsubscibe(_deleteDataListMessageSubscriptionToken);
                _messageAggregator.Unsubscibe(_PersistChildChainMessageSubscriptionToken);
                _messageAggregator.Unsubscibe(_networkContextDetachedMessageSubscriptionToken);

                _messageBroker = null;
                _messageAggregator = null;
                _datalistServer = null;

                _isDisposed = true;
            }
        }

        #endregion Tear Down
    }
}
