using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Network.Channels
{
    public class ExecutionClientChannel : INetworkExecutionChannel, IDisposable
    {
        volatile bool _isDisposed;

        readonly ThreadLocal<object> _threadSpecificContext = new ThreadLocal<object>();
        readonly ConcurrentDictionary<Guid, object> _callbacks = new ConcurrentDictionary<Guid, object>();
        readonly IEnvironmentConnection _connection;
        readonly Guid _executionStatusCallbackSubscription;
        readonly Guid _networkContextDetachedSubscription;

        #region Constructor

        public ExecutionClientChannel(IEnvironmentConnection connection)
        {
            if(connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            _connection = connection;
            _executionStatusCallbackSubscription = _connection.MessageAggregator.Subscribe(new Action<ExecutionStatusCallbackMessage, IStudioNetworkChannelContext>(OnExecutionStatusCallbackMessageRecieved));
            _networkContextDetachedSubscription = _connection.MessageAggregator.Subscribe(new Action<NetworkContextDetachedMessage, IStudioNetworkChannelContext>(OnNetworkContextDetachedMessageRecieved));
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

        #endregion

        #region Methods

        /// <summary>
        /// Adds an execution status callback.
        /// </summary>
        /// <param name="callbackID">The callback ID.</param>
        /// <param name="callback">The callback.</param>
        public bool AddExecutionStatusCallback(Guid callbackID, Action<ExecutionStatusCallbackMessage> callback)
        {
            if(callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if(_isDisposed)
            {
                return false;
            }

            var result = false;

            if(ExecutionStatusCallbackDispatcher.Instance.Add(callbackID, callback))
            {
                _callbacks.TryAdd(callbackID, null);

                try
                {
                    var message = new ExecutionStatusCallbackMessage(callbackID, ExecutionStatusCallbackMessageType.Add);
                    _connection.SendNetworkMessage(message);
                    result = true;
                }
                catch
                {
                    ExecutionStatusCallbackDispatcher.Instance.Remove(callbackID);
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
            if(_isDisposed)
            {
                return false;
            }

            var result = false;

            if(ExecutionStatusCallbackDispatcher.Instance.Remove(callbackID))
            {
                object tmp;
                _callbacks.TryRemove(callbackID, out tmp);

                var message = new ExecutionStatusCallbackMessage(callbackID, ExecutionStatusCallbackMessageType.Remove);
                _connection.SendNetworkMessage(message);
                result = true;
            }

            return result;
        }
         /// <summary>
        /// Adds an execution status callback.
        /// </summary>
        /// <param name="callbackID">The callback ID.</param>
        /// <param name="callback">The callback.</param>
        public bool AddNetworkContextDetachedMessageCallback(Guid callbackID, Action<NetworkContextDetachedMessage> callback)
        {
            if(callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if(_isDisposed)
            {
                return false;
            }

            var result = false;

             try
             {
                 _callbacks.TryAdd(callbackID, null);
                 var message = new NetworkContextDetachedMessage();
                 _connection.SendNetworkMessage(message);
                 result = true;
             }
             catch
             {
             }
             return result;
        }

        #endregion Methods

        #region Private Methods

        void OnExecutionStatusCallbackMessageRecieved(ExecutionStatusCallbackMessage message, IStudioNetworkChannelContext context)
        {
            if(_connection.WorkspaceID != context.Account || _connection.ServerID != context.Server)
            {
                return;
            }

            if(_isDisposed)
            {
                return;
            }

            Context = context;
            ExecutionStatusCallbackDispatcher.Instance.Post(message);
        }

        void OnNetworkContextDetachedMessageRecieved(NetworkContextDetachedMessage message, IStudioNetworkChannelContext context)
        {
            if(_connection.ServerID != context.Server)
            {
                return;
            }
            if(_isDisposed)
            {
                return;
            }

            Context = context;
            ExecutionStatusCallbackDispatcher.Instance.RemoveRange(_callbacks.Select(k => k.Key).ToList());
            Dispose();
        }

        #endregion Private Methods



        #region Implementation of IDisposable

        ~ExecutionClientChannel()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.
                    _threadSpecificContext.Dispose();
                    _callbacks.Clear();
                    _connection.MessageAggregator.Unsubscibe(_executionStatusCallbackSubscription);
                    _connection.MessageAggregator.Unsubscibe(_networkContextDetachedSubscription);
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                _isDisposed = true;
            }
        }

        #endregion

    }
}
