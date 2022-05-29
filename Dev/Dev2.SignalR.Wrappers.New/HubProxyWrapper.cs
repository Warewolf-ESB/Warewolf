/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if NETFRAMEWORK
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
#else
using Microsoft.AspNetCore.SignalR.Client;
#endif
using Newtonsoft.Json.Linq;

namespace Dev2.SignalR.Wrappers.New
{
    public class HubProxyWrapper:IHubProxyWrapper
    {
#if NETFRAMEWORK
        readonly IHubProxy _hubProxy;

        public HubProxyWrapper(IHubProxy hubProxy)
#else
        readonly HubConnection _hubProxy;

        public HubProxyWrapper(HubConnection hubProxy)
#endif
        {
            _hubProxy = hubProxy;
        }

        #region Implementation of IHubProxyWrapper

#if NETFRAMEWORK
        /// <summary>
        /// Executes a method on the server side hub asynchronously.
        /// </summary>
        /// <param name="method">The name of the method.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        public Task Invoke(string method, params object[] args) => _hubProxy.Invoke(method, args);

        /// <summary>
        /// Executes a method on the server side hub asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of result returned from the hub.</typeparam>
        /// <param name="method">The name of the method.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        public Task<T> Invoke<T>(string method, params object[] args) => _hubProxy.Invoke<T>(method, args);

        public object Object() => _hubProxy;

        public IDisposable On<T>(string eventName, Action<T> onData) => ((IHubProxy)Object()).On(eventName, onData);
#else
        /// <summary>
        /// Executes a method on the server side hub asynchronously.
        /// </summary>
        /// <param name="method">The name of the method.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        public Task Invoke(string method, params object[] args) => _hubProxy.InvokeAsync(method, args);

        /// <summary>
        /// Executes a method on the server side hub asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of result returned from the hub.</typeparam>
        /// <param name="method">The name of the method.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        public Task<T> Invoke<T>(string method, params object[] args) => _hubProxy.InvokeAsync<T>(method, args);

        public object Object() => _hubProxy;

        public IDisposable On<T>(string eventName, Action<T> onData) => ((HubConnection)Object()).On(eventName, onData);
#endif

        #endregion
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

#if NETFRAMEWORK
        public StateChangeWrapped(StateChange change)
        {
            OldState = (ConnectionStateWrapped)change.OldState;
            NewState = (ConnectionStateWrapped)change.NewState;
        }
#else
        public StateChangeWrapped(HubConnectionState change)
        {
            if (change == HubConnectionState.Connected)
            {
                OldState = ConnectionStateWrapped.Reconnecting;
                NewState = ConnectionStateWrapped.Connected;
            }
            if (change == HubConnectionState.Disconnected)
            {
                OldState = ConnectionStateWrapped.Connected;
                NewState = ConnectionStateWrapped.Disconnected;
            }
            if (change == HubConnectionState.Reconnecting)
            {
                OldState = ConnectionStateWrapped.Connected;
                NewState = ConnectionStateWrapped.Reconnecting;
            }
            if (change == HubConnectionState.Connecting)
            {
                OldState = ConnectionStateWrapped.Disconnected;
                NewState = ConnectionStateWrapped.Connecting;
            }
        }
#endif

        /// <summary>
        /// Gets the old state of the connection.
        /// </summary>
        public ConnectionStateWrapped OldState { get; private set; }

        /// <summary>
        /// Gets the new state of the connection.
        /// </summary>
        public ConnectionStateWrapped NewState { get; private set; }
    }
}
