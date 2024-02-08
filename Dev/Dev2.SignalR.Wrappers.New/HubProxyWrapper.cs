﻿///*
//*  Warewolf - Once bitten, there's no going back
//*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
//*  Licensed under GNU Affero General Public License 3.0 or later.
//*  Some rights reserved.
//*  Visit our website for more information <http://warewolf.io/>
//*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
//*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
//*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Client;

using Newtonsoft.Json.Linq;

namespace Dev2.SignalR.Wrappers.New
{
    public class HubProxyWrapper : IHubProxyWrapper
    {
        readonly Microsoft.AspNetCore.SignalR.Client.HubConnection _hubConnection;
        readonly IHubProxy _hubProxy;

        public HubProxyWrapper(Microsoft.AspNetCore.SignalR.Client.HubConnection connection)
        {
            _hubConnection = connection;
        }

        public HubProxyWrapper(IHubProxy hubProxy)
        {
            _hubProxy = hubProxy;
        }

        public ISubscriptionWrapper Subscribe(string sendmemo)
        {
            var s = new Subscription();
            _hubConnection.On<IList<JToken>>(sendmemo, s.OnReceived);
            return new SubscriptionWrapper(s);
        }

        public ISubscriptionWrapper LegacySubscribe(string sendmemo)
        {
            var s = _hubProxy.Subscribe(sendmemo);
            return new LegacySubscriptionWrapper(s);
        }


        #region Implementation of IHubProxyWrapper

        /// <summary>
        /// Executes a method on the server side hub asynchronously.
        /// </summary>
        /// <param name="method">The name of the method.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        public Task Invoke(string method, params object[] args) => _hubConnection != null ? _hubConnection.InvokeCoreAsync(method, args) : _hubProxy.Invoke(method, args);

        /// <summary>
        /// Executes a method on the server side hub asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of result returned from the hub.</typeparam>
        /// <param name="method">The name of the method.</param>
        /// <param name="args">The arguments</param>
        /// <returns>A task that represents when invocation returned.</returns>
        public Task<T> Invoke<T>(string method, params object[] args) => _hubConnection != null ? _hubConnection.InvokeCoreAsync<T>(method, args) : _hubProxy.Invoke<T>(method, args);

        public object Object() => _hubConnection != null ? (object)_hubConnection : _hubProxy;

        public IDisposable On<T>(string eventName, Action<T> onData) => _hubConnection != null ? ((Microsoft.AspNetCore.SignalR.Client.HubConnection)Object()).On(eventName, onData) : ((IHubProxy)Object()).On(eventName, onData);

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
            Received?.Invoke(obj);
        }

        public event Action<IList<JToken>> Received;
        public Subscription Wrapped { get; private set; }
    }
    public class LegacySubscriptionWrapper : ISubscriptionWrapper
    {
        public LegacySubscriptionWrapper(Microsoft.AspNet.SignalR.Client.Hubs.Subscription s)
        {
            Wrapped = s;
            Wrapped.Received += WrappedReceived;
        }

        void WrappedReceived(IList<JToken> obj)
        {
            Received?.Invoke(obj);
        }

        public event Action<IList<JToken>> Received;
        public Microsoft.AspNet.SignalR.Client.Hubs.Subscription Wrapped { get; private set; }
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

        public StateChangeWrapped(HubConnectionState oldState, HubConnectionState newState)
        {
            OldState = oldState.ToConnectionStateWrapped();
            NewState = newState.ToConnectionStateWrapped();
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

    public class Subscription
    {
        public event Action<IList<JToken>> Received;

        internal void OnReceived(IList<JToken> data)
        {
            if (this.Received != null)
            {
                this.Received(data);
            }
        }
    }
}
