using System;
using System.Net;
using System.Threading.Tasks;
using Dev2.SignalR.Wrappers.New;
using Microsoft.AspNet.SignalR.Client;

namespace Dev2.SignalR.Wrappers.Old
{
    public class HubConnectionWrapperOld : IHubConnectionWrapper
    {
        ConnectionStateWrapped _state;
        HubConnection _wrapped;
        ICredentials _credentials;

        #region Implementation of IHubConnectionWrapper

        public HubConnectionWrapperOld(HubConnection wrapped)
        {
            _wrapped = wrapped;
            _wrapped.Error += exception => Error(exception);
            _wrapped.Closed += delegate { Closed(); };
            _wrapped.StateChanged += change => StateChanged( new StateChangeWrappedOld(change));
        }
        public IHubProxyWrapper CreateHubProxy(string hubName)
        {
            return new HubProxyWrapperOld(_wrapped.CreateHubProxy(hubName));
        }
        public HubConnectionWrapperOld(string uriString):this(new HubConnection(uriString)){}

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
        public ICredentials Credentials
        {
            get
            {
                return _wrapped.Credentials;
            }

            set
            {
                _wrapped.Credentials = value;
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

        #endregion
    }
}