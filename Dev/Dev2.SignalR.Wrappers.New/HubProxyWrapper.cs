using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;

namespace Dev2.SignalR.Wrappers.New
{
    public class HubProxyWrapper:IHubProxyWrapper
    {
        readonly IHubProxy _hubProxy;

        public HubProxyWrapper(IHubProxy hubProxy)
        {
            _hubProxy = hubProxy; 
        }

        public ISubscriptionWrapper Subscribe(string sendmemo)
        {
            Subscription s = _hubProxy.Subscribe(sendmemo);
            return new SubscriptionWrapper(s);
        }

        #region Implementation of IHubProxyWrapper

        /// <summary>
        /// Executes a method on the server side hub asynchronously.
        /// </summary>
        /// <param name="method">The name of the method.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        public Task Invoke(string method, params object[] args)
        {
            return _hubProxy.Invoke(method,args);
        }

        /// <summary>
        /// Executes a method on the server side hub asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of result returned from the hub.</typeparam>
        /// <param name="method">The name of the method.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        public Task<T> Invoke<T>(string method, params object[] args)
        {
            return _hubProxy.Invoke<T>(method, args);
        }

        public object Object()
        {
            return _hubProxy;
        }

        public  IDisposable On<T>( string eventName, Action<T> onData)
        {
            return ((IHubProxy)Object()).On(eventName, onData);
        }

        #endregion
    }
    public class SubscriptionWrapper : ISubscriptionWrapper
    {
        public SubscriptionWrapper(Subscription s)
        {
            Wrapped = s;
            Wrapped.Received += WrappedReceived;
        }

        void WrappedReceived(IList<JToken> obj)
        {
            if (Received != null)
            {
                Received(obj);
            }
        }
        public event Action<IList<JToken>> Received;
        public Subscription Wrapped { get; private set; }
    }

    public class StateChangeWrapped : IStateChangeWrapped
    {
        /// <summary>
        /// Creates a new stance of <see cref="StateChange"/>.
        /// </summary>
        /// <param name="oldState">The old state of the connection.</param>
        /// <param name="newState">The new state of the connection.</param>
        public StateChangeWrapped(ConnectionStateWrapped oldState, ConnectionStateWrapped newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public StateChangeWrapped(StateChange change)
        {

            OldState = (ConnectionStateWrapped)change.OldState;
            NewState = (ConnectionStateWrapped)change.NewState;
        }

        /// <summary>
        /// Gets the old state of the connection.
        /// </summary>
        public ConnectionStateWrapped OldState { get; private set; }

        /// <summary>
        /// Gets the new state of the connection.
        /// </summary>
        public ConnectionStateWrapped NewState { get; private set; }
    }
    public class HttpClientExceptionWrapped : HttpClientException { }


}
