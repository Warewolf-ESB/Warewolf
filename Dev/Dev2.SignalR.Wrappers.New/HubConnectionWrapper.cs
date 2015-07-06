using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace Dev2.SignalR.Wrappers.New
{
    public class HubConnectionWrapper : IHubConnectionWrapper
    {
        ConnectionStateWrapped _state;
        HubConnection _wrapped;

        #region Implementation of IHubConnectionWrapper

        public HubConnectionWrapper(HubConnection wrapped)
        {
            _wrapped = wrapped;
            _wrapped.Error += exception => Error(exception);
            _wrapped.Closed += delegate { Closed(); };
            _wrapped.StateChanged += change => StateChanged(new StateChangeWrapped(change));
        }

        public HubConnectionWrapper(string uriString)
            : this(new HubConnection(uriString))
        {

        }

        public IHubProxyWrapper CreateHubProxy(string hubName)
        {
           return new HubProxyWrapper(_wrapped.CreateHubProxy(hubName));
        }

        public event Action<Exception> Error;
        public event Action Closed;
        public event Action<IStateChangeWrapped> StateChanged;
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
