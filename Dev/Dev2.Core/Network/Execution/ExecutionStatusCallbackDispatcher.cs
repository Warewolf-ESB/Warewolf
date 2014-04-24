using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Dev2.Network.Execution
{
    public class ExecutionStatusCallbackDispatcher : IExecutionStatusCallbackDispatcher, IDisposable
    {
        #region Class Members

        private readonly object _disposeGuard = new object();
        private bool _isDisposed;

        private readonly ConcurrentDictionary<Guid, Action<ExecutionStatusCallbackMessage>> _callbacks = new ConcurrentDictionary<Guid, Action<ExecutionStatusCallbackMessage>>();

        #endregion Class Members

        #region Constructor

        // For testing only!!!
        // ReSharper disable EmptyConstructor
        public ExecutionStatusCallbackDispatcher()
        // ReSharper restore EmptyConstructor
        {
        }

        #endregion Constructor

        #region Singleton Instance

        //
        // Multi-threaded implementation - see http://msdn.microsoft.com/en-us/library/ff650316.aspx
        //
        // This approach ensures that only one instance is created and only when the instance is needed. 
        // Also, the variable is declared to be volatile to ensure that assignment to the instance variable
        // completes before the instance variable can be accessed. Lastly, this approach uses a syncRoot 
        // instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        //
        static volatile ExecutionStatusCallbackDispatcher _instance;
        static readonly object SyncRoot = new Object();

        /// <summary>
        /// Gets the repository instance.
        /// </summary>
        public static ExecutionStatusCallbackDispatcher Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(SyncRoot)
                    {
                        if(_instance == null)
                        {
                            _instance = new ExecutionStatusCallbackDispatcher();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

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
            if(callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            lock(_disposeGuard)
            {
                if(_isDisposed)
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
            lock(_disposeGuard)
            {
                if(_isDisposed)
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
            lock(_disposeGuard)
            {
                if(_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            foreach(KeyValuePair<Guid, Action<ExecutionStatusCallbackMessage>> item in _callbacks)
            {
                Action<ExecutionStatusCallbackMessage> tmpCallback;
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
            if(message == null)
            {
                throw new ArgumentNullException("message");
            }

            lock(_disposeGuard)
            {
                if(_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            Action<ExecutionStatusCallbackMessage> callback;
            if(_callbacks.TryGetValue(message.CallbackID, out callback))
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
            if(message == null)
            {
                throw new ArgumentNullException("message");
            }

            lock(_disposeGuard)
            {
                if(_isDisposed)
                {
                    throw new InvalidOperationException("Channel is disposing.");
                }
            }

            Action<ExecutionStatusCallbackMessage> callback;
            if(_callbacks.TryGetValue(message.CallbackID, out callback))
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
            if(callbackID != Guid.Empty)
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
            lock(_disposeGuard)
            {
                if(_isDisposed)
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
