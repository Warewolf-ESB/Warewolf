
using System;
using System.Net;
using System.Threading.Tasks;
using Dev2.SignalR.Wrappers.New;
using Microsoft.AspNet.SignalR.Client.Old;

namespace Dev2.SignalR.Wrappers.Old
{
    public class HubConnectionWrapperOld : IHubConnectionWrapper
    {
        ConnectionStateWrapped _state;
       HubConnection _wrapped;
        ICredentials _credentials;

        #region Implementation of IHubConnectionWrapper

        public HubConnectionWrapperOld(string uri)
        {
     

            _wrapped = new HubConnection(uri);
            _wrapped.Error += exception => {
                                               if(Error != null)
                                               {
                                                   Error(exception);
                                               }
            };
            _wrapped.Closed += delegate {
                                            if(Closed != null)
                                            {
                                                Closed();
                                            }
            };
            _wrapped.StateChanged += change => {
                                                   if(StateChanged != null)
                                                   {
                                                       StateChanged(new StateChangeWrappedOld(change));
                                                   }
            };
        }
        public IHubProxyWrapper CreateHubProxy(string hubName)
        {
            return new HubProxyWrapperOld(_wrapped.CreateHubProxy(hubName));
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