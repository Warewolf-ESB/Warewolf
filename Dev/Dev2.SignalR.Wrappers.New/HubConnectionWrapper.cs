/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace Dev2.SignalR.Wrappers.New
{
    public class HubConnectionWrapper : IHubConnectionWrapper
    {
        readonly HubConnection _wrapped;

        #region Implementation of IHubConnectionWrapper

        private HubConnectionWrapper(HubConnection wrapped)
        {
            _wrapped = wrapped;
            _wrapped.DeadlockErrorTimeout = TimeSpan.FromSeconds(30);
        }

        public HubConnectionWrapper(string uriString)
            : this(new HubConnection(uriString))
        {
        }

        public IHubProxyWrapper CreateHubProxy(string hubName)
        {
           return new HubProxyWrapper(_wrapped.CreateHubProxy(hubName));
        }

        public event Action<Exception> Error
        {
            add
            {
                _wrapped.Error += value;
            }
            remove
            {
                _wrapped.Error -= value;
            }
        }

        public event Action Closed
        {
            add
            {
                _wrapped.Closed += value;
            }
            remove
            {
                _wrapped.Closed -= value;
            }
        }

        public event Action<IStateChangeWrapped> StateChanged
        {
            add
            {
                _wrapped.StateChanged += change => value(new StateChangeWrapped(change));
            }
            remove
            {
            }
        }

        public ConnectionStateWrapped State => (ConnectionStateWrapped)_wrapped.State;

        public Task Start()
        {
            var serverSentEventsTransport = new ServerSentEventsTransport();
            return _wrapped.Start(serverSentEventsTransport);
        }

        public void Stop(TimeSpan timeSpan)
        {
            _wrapped.Stop(timeSpan);
        }

        public ICredentials Credentials
        {
            get
            {
                return _wrapped.Credentials;
            }

            set
            {
                _wrapped.Credentials=value;
            }
        }

        #endregion
    }
}
