using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace Dev2.SignalR.Wrappers.New
{
    public class HubConnectionWrapper : IHubConnectionWrapper
    {
        readonly HubConnection _wrapped;

        #region Implementation of IHubConnectionWrapper

        private HubConnectionWrapper(HubConnection wrapped)
        {
            _wrapped = wrapped;
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
        public ConnectionStateWrapped State
        {
            get
            {
                return (ConnectionStateWrapped)_wrapped.State;
            }
    
        }

        public Task Start()
        {
            return _wrapped.Start();
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
