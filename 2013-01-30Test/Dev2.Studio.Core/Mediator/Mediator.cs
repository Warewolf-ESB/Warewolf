using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace Dev2.Studio.Core {
	/// <summary>
	/// Provides loosely-coupled messaging between
	/// various colleagues.  All references to objects
	/// are stored weakly, to prevent memory leaks.
	/// </summary>
    public class Mediator
    {
        #region Fields

        internal static readonly ThreadLocal<MessageToActionsMap> _messageToCallbacksMapThreadLocal = new ThreadLocal<MessageToActionsMap>();
        private static readonly ThreadLocal<Dictionary<MediatorMessages, List<DispatchedActionTO>>> _dispatchedCallbacksThreadLocal = new ThreadLocal<Dictionary<MediatorMessages, List<DispatchedActionTO>>>();
        private static readonly Action<DispatchedActionInvoker> _invokeDispatched = OnInvokeDispatched;
        
        #endregion // Fields

        #region Constructor
        //static Mediator()
        //{
        //    _messageToCallbacksMap = new MessageToActionsMap();
        //}
        #endregion // Constructor

        #region Properties

        private static Dictionary<MediatorMessages, List<DispatchedActionTO>> _dispatchedCallbacks
        {
            get
            {
                if (!_dispatchedCallbacksThreadLocal.IsValueCreated)
                {
                    _dispatchedCallbacksThreadLocal.Value = new Dictionary<MediatorMessages, List<DispatchedActionTO>>();
                }

                return _dispatchedCallbacksThreadLocal.Value;
            }
        }

        private static MessageToActionsMap _messageToCallbacksMap
        {
            get
            {
                if (!_messageToCallbacksMapThreadLocal.IsValueCreated)
                {
                    _messageToCallbacksMapThreadLocal.Value = new MessageToActionsMap();
                }

                return _messageToCallbacksMapThreadLocal.Value;
            }
        }

        #endregion Properties

        #region Methods
        /// <summary>
        /// Registers to receieve a message that has been dispatched asynchronously.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static object RegisterToReceiveDispatchedMessage(MediatorMessages message, object key, Action<object> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var to = new DispatchedActionTO(message, key, callback);
            List<DispatchedActionTO> existing;
            if (!_dispatchedCallbacks.TryGetValue(message, out existing)) _dispatchedCallbacks.Add(message, existing = new List<DispatchedActionTO>());
            existing.Add(to);
            return key;
        }

        public static string RegisterToReceiveMessage(MediatorMessages message, Action<object> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            return _messageToCallbacksMap.AddAction(message, callback);
        }

        public static void SendMessage(MediatorMessages message, object parameter)
        {
            // Debug statements do not belong in product code ;)
            //Debug.WriteLine("sending message {0}", new object[] { ((MediatorMessages)message).ToString() });

            List<Action<object>> actions =
                _messageToCallbacksMap.GetActions(message);

            if (actions != null)
                actions.ForEach(action => action(parameter));


            List<DispatchedActionTO> existing;

            if (_dispatchedCallbacks.TryGetValue(message, out existing))
            {
                DispatchedActionTO[] pending = existing.ToArray();
                DispatchedActionInvoker invoker = new DispatchedActionInvoker(pending, parameter, 10);
                Dispatcher.CurrentDispatcher.BeginInvoke(_invokeDispatched, DispatcherPriority.ApplicationIdle, invoker);
            }
        }

        private static void OnInvokeDispatched(DispatchedActionInvoker invoker)
        {
            if (invoker.Next())
                Dispatcher.CurrentDispatcher.BeginInvoke(_invokeDispatched, DispatcherPriority.ApplicationIdle, invoker);
        }

        public static void DeRegisterAllActionsForMessage(MediatorMessages message)
        {
            _dispatchedCallbacks.Remove(message);
            _messageToCallbacksMap.RemoveAllActionsForMessage(message);
        }

        public static void DeRegister(MediatorMessages message, object key)
        {
            if (key == null) throw new ArgumentNullException("key");

            List<DispatchedActionTO> existing;

            if (_dispatchedCallbacks.TryGetValue(message, out existing))
            {
                for (int i = existing.Count - 1; i >= 0; i--)
                    if (existing[i].Key == key)
                        existing.RemoveAt(i);
            }
        }

        public static void SuspendRegistration(MediatorMessages message, object key)
        {
            if (key == null) throw new ArgumentNullException("key");
            List<DispatchedActionTO> existing;

            if (_dispatchedCallbacks.TryGetValue(message, out existing))
            {
                for (int i = existing.Count - 1; i >= 0; i--)
                    if (existing[i].Key == key)
                        existing[i].Suspended = true;
            }
        }

        public static void ReinstateRegistration(MediatorMessages message, object key)
        {
            if (key == null) throw new ArgumentNullException("key");

            List<DispatchedActionTO> existing;

            if (_dispatchedCallbacks.TryGetValue(message, out existing))
            {
                for (int i = existing.Count - 1; i >= 0; i--)
                    if (existing[i].Key == key)
                        existing[i].Suspended = false;
            }
        }

        public static void DeRegister(MediatorMessages message, string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            _messageToCallbacksMap.RemoveAction(message, key);
        }

        //public static void SuspendRegistration(MediatorMessages message, string key)
        //{
        //    if (key == null)
        //        throw new ArgumentNullException("key");

        //    _messageToCallbacksMap.SuspendAction(message, key);
        //}

        //public static void ReinstateRegistration(MediatorMessages message, string key)
        //{
        //    if (key == null)
        //        throw new ArgumentNullException("key");

        //    _messageToCallbacksMap.ReinstateAction(message, key);
        //}
        #endregion

        private sealed class DispatchedActionInvoker
        {
            private DispatchedActionTO[] _actions;
            private object _argument;
            private int _index;
            private int _length;
            private int _batchSize;

            public DispatchedActionInvoker(DispatchedActionTO[] actions, object argument, int batchSize)
            {
                _argument = argument;
                _index = 0;
                _length = (_actions = actions).Length;
                _batchSize = batchSize;
            }

            public bool Next()
            {
                int end = Math.Min(_length, _index + _batchSize);

                for (; _index < end; _index++)
                {
                    if (_actions[_index].Suspended) continue;
                    _actions[_index].Callback(_argument);
                }

                return _index < _length;
            }
        }

        private sealed class DispatchedActionTO
        {
            private MediatorMessages _message;
            private object _key;
            private Action<object> _callback;
            private bool _suspended;

            public object Key { get { return _key; } }
            public bool Suspended { get { return _suspended; } set { _suspended = value; } }
            public Action<object> Callback { get { return _callback; } }

            public DispatchedActionTO(MediatorMessages message, object key, Action<object> callback)
            {
                _message = message;
                _key = key;
                _callback = callback;
            }
        }
    }
}