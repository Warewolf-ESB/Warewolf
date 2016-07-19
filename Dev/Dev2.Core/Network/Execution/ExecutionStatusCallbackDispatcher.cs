/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using Warewolf.Resource.Errors;

namespace Dev2.Network.Execution
{
    public class ExecutionStatusCallbackDispatcher : IExecutionStatusCallbackDispatcher, IDisposable
    {
        #region Class Members

        private readonly ConcurrentDictionary<Guid, Action<ExecutionStatusCallbackMessage>> _callbacks =
            new ConcurrentDictionary<Guid, Action<ExecutionStatusCallbackMessage>>();

        private readonly object _disposeGuard = new object();
        private bool _isDisposed;

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
        private static volatile ExecutionStatusCallbackDispatcher _instance;
        private static readonly object SyncRoot = new Object();

        /// <summary>
        ///     Gets the repository instance.
        /// </summary>
        public static ExecutionStatusCallbackDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
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
        ///     Posts the specified message (Asynchronously).
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
                    throw new InvalidOperationException(ErrorResource.ChannelDisposing);
                }
            }

            Action<ExecutionStatusCallbackMessage> callback;
            if (_callbacks.TryGetValue(message.CallbackID, out callback))
            {
                callback.BeginInvoke(message, null, null);
            }
        }

        #endregion Methods

        #region Tear Down

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
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