
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Old;
using Microsoft.AspNet.SignalR.Client.Old.Transports;

namespace Dev2.SignalR.Wrappers.Old
{
    public class HubConnectionWrapperOld : IHubConnectionWrapper
    {
        readonly HubConnection _wrapped;

        #region Implementation of IHubConnectionWrapper

        public HubConnectionWrapperOld(string uri)
        {
            _wrapped = new HubConnection(uri);            
        }
        public IHubProxyWrapper CreateHubProxy(string hubName)
        {
            return new HubProxyWrapperOld(_wrapped.CreateHubProxy(hubName));
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
                _wrapped.StateChanged += change => value(new StateChangeWrappedOld(change));
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
            return _wrapped.Start(new LongPollingTransport());
        }

        public void Stop(TimeSpan timeSpan)
        {
            _wrapped.Stop(timeSpan);
        }

       

        #endregion
    }
}