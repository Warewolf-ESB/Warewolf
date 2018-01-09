using System;
using System.Net;
using System.Threading.Tasks;

namespace Dev2.SignalR.Wrappers
{
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
    }

    public interface IStateChangeWrapped
    {
         ConnectionStateWrapped OldState { get;  }        
         ConnectionStateWrapped NewState { get; }
    }
}