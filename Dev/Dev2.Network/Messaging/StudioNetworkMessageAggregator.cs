using Dev2.Network.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Network;

namespace Dev2.Network.Messaging
{
    [Export(typeof(IStudioNetworkMessageAggregator))]
    public class StudioNetworkMessageAggregator : IStudioNetworkMessageAggregator, IDisposable
    {
        #region Class Members

        private object _subscriptionLock = new object();
        private Dictionary<Guid, Delegate> _subscriptions;
        private Dictionary<Type, Dictionary<Delegate, int>> _callbacksIndexedByType;

        #endregion Class Members

        #region Constructors

        public StudioNetworkMessageAggregator()
        {
            _subscriptions = new Dictionary<Guid, Delegate>();
            _callbacksIndexedByType = new Dictionary<Type, Dictionary<Delegate, int>>();
        }

        #endregion Constructors

        #region INetworkMessageAggregator

        /// <summary>
        /// Publishes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="async">if set to <c>true</c> [async].</param>
        public void Publish(INetworkMessage message, bool async = true) 
        {
            Publish(message, null, async);
        }

        /// <summary>
        /// Publishes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="context">The context.</param>
        /// <param name="async">if set to <c>true</c> [async].</param>
        public void Publish(INetworkMessage message, IStudioNetworkChannelContext context, bool async = true) 
        {
            if (message == null)
            {
                return;
            }

            //
            // Get delegates to invoke by type
            //
            Type messageType = message.GetType();
            Type callbackType = typeof(Action<,>).MakeGenericType(new Type[] { messageType, typeof(IStudioNetworkChannelContext) });
            Type callbackTypeSimple = typeof(Action<,>).MakeGenericType(new Type[] { messageType, typeof(INetworkOperator) });
            Dictionary<Delegate, int> delegates;
            object[] parameters = new object[] { message, context };
            object[] parametersSimple = new object[] { message, (context != null) ? context.NetworkOperator : null };
            
            if (_callbacksIndexedByType.TryGetValue(messageType, out delegates))
            {
                foreach (KeyValuePair<Delegate, int> callbackAndCount in delegates.ToList())
                {
                    if (callbackAndCount.Key.GetType() == callbackType)
                    {
                        Delegate callback = callbackAndCount.Key;
                        if (async)
                        {
                            Action a = new Action(() =>
                            {
                                callback.DynamicInvoke(parameters);
                            });
                            a.BeginInvoke(null, null);
                        }
                        else
                        {
                            callback.DynamicInvoke(parameters);
                        }
                    }
                    else if (callbackAndCount.Key.GetType() == callbackTypeSimple)
                    {
                        Delegate callback = callbackAndCount.Key;
                        if (async)
                        {
                            Action a = () =>
                            {
                                callback.DynamicInvoke(parametersSimple);
                            };
                            a.BeginInvoke(null, null);
                        }
                        else
                        {
                            callback.DynamicInvoke(parametersSimple);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Subscribes to recieve messages of a certain type.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        public Guid Subscribe<T>(Action<T, IStudioNetworkChannelContext> callback) where T : INetworkMessage, new()
        {
            if (callback == null)
            {
                return Guid.Empty;
            }

            Guid subscriptionToken = Guid.NewGuid();

            lock (_subscriptionLock)
            {
                //
                // Register subscription
                //
                _subscriptions.Add(subscriptionToken, callback);

                //
                // Ensure the callback is registered with the type index
                //
                Type messageType = typeof(T);
                Dictionary<Delegate, int> delegates;
                
                if (!_callbacksIndexedByType.TryGetValue(messageType, out delegates))
                {
                    delegates = new Dictionary<Delegate, int>();
                    _callbacksIndexedByType.Add(messageType, delegates);
                }

                int count;
                if (!delegates.TryGetValue(callback, out count))
                {
                    delegates.Add(callback, 1);
                }
                else
                {
                    delegates[callback]++;
                }
            }

            return subscriptionToken;
        }

        /// <summary>
        /// Subscribes to recieve messages of a certain type.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        public Guid Subscribe<T>(Action<T, INetworkOperator> callback) where T : INetworkMessage, new()
        {
            if (callback == null)
            {
                return Guid.Empty;
            }

            Guid subscriptionToken = Guid.NewGuid();

            lock (_subscriptionLock)
            {
                //
                // Register subscription
                //
                _subscriptions.Add(subscriptionToken, callback);

                //
                // Ensure the callback is registered with the type index
                //
                Type messageType = typeof(T);
                Dictionary<Delegate, int> delegates;

                if (!_callbacksIndexedByType.TryGetValue(messageType, out delegates))
                {
                    delegates = new Dictionary<Delegate, int>();
                    _callbacksIndexedByType.Add(messageType, delegates);
                }

                int count;
                if (!delegates.TryGetValue(callback, out count))
                {
                    delegates.Add(callback, 1);
                }
                else
                {
                    delegates[callback]++;
                }
            }

            return subscriptionToken;
        }

        /// <summary>
        /// Unsubscibes from recieving messages of a certain type.
        /// </summary>
        /// <param name="subscriptionToken">The subscription token.</param>
        public bool Unsubscibe(Guid subscriptionToken)
        {
            if (subscriptionToken == null)
            {
                return false;
            }

            bool result = false;

            lock (_subscriptionLock)
            {
                //
                // Get callback
                //
                Delegate callback;
                if (_subscriptions.TryGetValue(subscriptionToken, out callback))
                {
                    //
                    // Get message type from callback
                    //
                    Type messageType = callback.GetType().GetGenericArguments().FirstOrDefault();
                    if (messageType != null)
                    {
                        //
                        // Remove from delegate type index
                        //
                        Dictionary<Delegate, int> delegates;
                        if (_callbacksIndexedByType.TryGetValue(messageType, out delegates))
                        {
                            int count;
                            if (delegates.TryGetValue(callback, out count))
                            {
                                if (count <= 1)
                                {
                                    delegates.Remove(callback);
                                }
                                else
                                {
                                    delegates[callback]--;
                                }
                            }
                        }

                        //
                        // Remove subscription
                        //
                        result = _subscriptions.Remove(subscriptionToken);
                    }
                }
            }

            return result;
        }

        #endregion INetworkMessageAggregator

        #region Tear Down

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _subscriptions.Clear();
            _callbacksIndexedByType.Clear();
        }

        #endregion Tear Down
    }
}
