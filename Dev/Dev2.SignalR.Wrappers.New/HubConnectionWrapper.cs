/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace Dev2.SignalR.Wrappers.New
{
    public class ConnectedHubProxy : IConnectedHubProxy
    {
        public IHubConnectionWrapper Connection { get; set; }
        public IHubProxyWrapper Proxy { get; set; }
    }
    
    public class HubConnectionWrapper : IHubConnectionWrapper, IDisposable
    {
        readonly HubConnection _wrapped;
        private readonly ManualResetEvent _connectNotify = new ManualResetEvent(false);

        #region Implementation of IHubConnectionWrapper

        HubConnectionWrapper(HubConnection wrapped)
        {
            _wrapped = wrapped;
            _wrapped.DeadlockErrorTimeout = TimeSpan.FromSeconds(30);
            _wrapped.StateChanged += change =>
            {
                switch (change.NewState)
                {
                    case ConnectionState.Connected:
                        _connectNotify.Set();
                        break;
                    case ConnectionState.Connecting:
                    case ConnectionState.Reconnecting:
                    case ConnectionState.Disconnected:
                        _connectNotify.Reset();
                        break;
                    default:
                        throw new Exception("unknown ConnectionState");
                }
            };
        }

        public HubConnectionWrapper(string uriString)
            : this(new HubConnection(uriString))
        {
        }

        public Task EnsureConnected(TimeSpan timeout)
        { /// when this function returns we should have waited for a connect or reconnect to have completed
            var startTask = Task.Factory.StartNew(() =>
            {
                if (State != ConnectionStateWrapped.Connected)
                {
                    _connectNotify.WaitOne(timeout);
                }
            });

            var timerTask = Task.Delay(timeout);
            return Task.WhenAny(startTask, timerTask).ContinueWith((task) =>
            {
                if (task == timerTask || State != ConnectionStateWrapped.Connected)
                {
                    throw new Exception("unexpected timeout while connecting to warewolf server");
                }

                return task.Result;
            });
        }


        public IHubProxyWrapper CreateHubProxy(string hubName) => new HubProxyWrapper(_wrapped.CreateHubProxy(hubName));

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
                _wrapped.StateChanged += change => value?.Invoke(new StateChangeWrapped(change));
            }
            remove
            {
                throw new NotImplementedException();
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

        public void Dispose()
        {
            _connectNotify.Dispose();
        }
    }
}
