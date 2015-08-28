using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Dev2.Providers.Logs;
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
//            _wrapped.TraceLevel = TraceLevels.All;
//            _wrapped.TraceWriter = new Dev2LoggingTextWriter();
        }

        public HubConnectionWrapper(string uriString)
            : this(new HubConnection(uriString))
        {
//            _wrapped.TraceLevel = TraceLevels.Events;
//            _wrapped.TraceWriter = new Dev2LoggingTextWriter();
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
            return _wrapped.Start(new ServerSentEventsTransport());
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
