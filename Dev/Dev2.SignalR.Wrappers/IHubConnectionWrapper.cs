using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace Dev2.SignalR.Wrappers
{
    public interface IConnectedHubProxy
    {
        IHubConnectionWrapper Connection { get; set; }
        IHubProxyWrapper Proxy { get; set; }
    }

    public interface IHubConnectionWrapper 
    {        
        IHubProxyWrapper CreateHubProxy(string hubName);
        event Action<Exception> Error;
        event Action Closed;
        event Action<IStateChangeWrapped> StateChanged;
        ConnectionStateWrapped State { get;  }
        ICredentials Credentials { get;  }

        Task Start();
        void Stop(TimeSpan timeSpan);
        Task EnsureConnected(TimeSpan timeout);
    }

    public interface IStateChangeWrapped
    {
         ConnectionStateWrapped OldState { get;  }        
         ConnectionStateWrapped NewState { get; }
    }
}