using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Dev2.Network.Execution
{
    [Export(typeof(IExecutionStatusCallbackDispatcher))]
    public class ExecutionStatusCallbackDispatcher : IExecutionStatusCallbackDispatcher, IDisposable
    {
        #region Class Members

        private object _disposeGuard = new object();
        private bool _isDisposed = false;

        private ConcurrentDictionary<Guid, Action<ExecutionStatusCallbackMessage>> _callbacks;
        private static ExecutionStatusCallbackDispatcher _instance = new ExecutionStatusCallbackDispatcher();

        #endregion Class Members

        #region Constructor

        public ExecutionStatusCallbackDispatcher()
        {
            _callbacks = new ConcurrentDictionary<Guid, Action<ExecutionStatusCallbackMessage>>();
        }

        #endregion Constructor

        #region Static Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ExecutionStatusCallbackDispatcher Instance
        {
            get
            {
                return _instance;
            }
        }

        #endregion Static Properties

        #region Methods

        /// <summary>
        /// Adds the specified callback.
        /// </summary>
        /// <param name="callbackID">The callback ID.</param>
        /// <param name="callback">The callback.</param>
        /// <exception cref="System.ArgumentNullException">callback</exception>
        /// <exception cref="System.InvalidOperationException">Channel is disposing.</exception>
        public bool Add(Guid callbackID, Action<ExecutionStatusCallbackMessage> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            return _callbacks.TryAdd(callbackID, callback);
        }

        /// <summary>
        /// Removes the specified callback.
        /// </summary>
        /// <param name="callbackID">The callback ID.</param>
        /// <exception cref="System.InvalidOperationException">Channel is disposing.</exception>
        public bool Remove(Guid callbackID)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            Action<ExecutionStatusCallbackMessage> tmpCallback;
            return _callbacks.TryRemove(callbackID, out tmpCallback);
        }


        /// <summary>
        /// Removes a range of callback IDs range.
        /// </summary>
        /// <param name="callbackID">The callback ID.</param>
        /// <exception cref="System.InvalidOperationException">Channel is disposing.</exception>
        public void RemoveRange(IList<Guid> callbackID)
        {
            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            Action<ExecutionStatusCallbackMessage> tmpCallback;
            foreach (KeyValuePair<Guid, Action<ExecutionStatusCallbackMessage>> item in _callbacks)
            {
                _callbacks.TryRemove(item.Key, out tmpCallback);   
            }
        }

        /// <summary>
        /// Posts the specified message (Asynchronously).
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentNullException">message</exception>
        /// <exception cref="System.InvalidOperationException">Channel is disposing.</exception>
        public void Post(ExecutionStatusCallbackMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            Action<ExecutionStatusCallbackMessage> callback;
            if (_callbacks.TryGetValue(message.CallbackID, out callback))
            {
                callback.BeginInvoke(message, null, null);
            }
        }

        /// <summary>
        /// Sends the specified message (Synchronously).
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentNullException">message</exception>
        /// <exception cref="System.InvalidOperationException">Channel is disposing.</exception>
        public void Send(ExecutionStatusCallbackMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            lock (_disposeGuard)
            {
                if (_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            Action<ExecutionStatusCallbackMessage> callback;
            if (_callbacks.TryGetValue(message.CallbackID, out callback))
            {
                callback(message);
            }
        }

        /// <summary>
        /// A wrapper for the Post method which will only post a message if the callbackID isn't empty.
        /// </summary>
        /// <param name="callbackID">The callback ID.</param>
        /// <param name="messageType">Type of the message.</param>
        public void Post(Guid callbackID, ExecutionStatusCallbackMessageType messageType)
        {
            if (callbackID != Guid.Empty)
            {
                Post(new ExecutionStatusCallbackMessage(callbackID, messageType));
            }
        }

        #endregion Methods

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

                _isDisposed = true;

                _callbacks.Clear();
            }
        }

        #endregion Tear Down
    }
}
