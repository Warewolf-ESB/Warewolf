using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace Dev2.SignalR.Wrappers.New
{
    public class HubProxyWrapper:IHubProxyWrapper
    {
        readonly IHubProxy _hubProxy;

        public HubProxyWrapper(IHubProxy hubProxy)
        {
            _hubProxy = hubProxy; 
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

        /// <summary>
        /// Executes a method on the server side hub asynchronously with progress updates.
        /// </summary>
        /// <param name="method">The name of the method.</param>
        /// <param name="onProgress">The callback to invoke when progress updates are received.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        public Task Invoke<T>(string method, Action<T> onProgress, params object[] args)
        {
            return _hubProxy.Invoke(method,onProgress, args);
        }

        /// <summary>
        /// Executes a method on the server side hub asynchronously with progress updates.
        /// </summary>
        /// <typeparam name="TResult">The type of result returned from the hub.</typeparam>
        /// <typeparam name="TProgress">The type of progress update value.</typeparam>
        /// <param name="method">The name of the method.</param>
        /// <param name="onProgress">The callback to invoke when progress updates are received.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        public Task<TResult> Invoke<TResult, TProgress>(string method, Action<TProgress> onProgress, params object[] args)
        {
            return _hubProxy.Invoke<TResult, TProgress>(method, onProgress, args);
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


    public class StateChangeWrapped : IStateChangeWrapped
    {
        private StateChange change;

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
