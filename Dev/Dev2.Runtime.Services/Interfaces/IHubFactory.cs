#if !NETFRAMEWORK
using Dev2.SignalR.Wrappers;
#endif
using Microsoft.AspNetCore.SignalR.Client;

namespace Dev2.Runtime.Interfaces
{
    public interface IHubFactory
    {
#if NETFRAMEWORK
        HubConnection GetHubConnection(Data.ServiceModel.Connection connection);
#else
        HubConnection GetTestHubConnection(Data.ServiceModel.Connection connection);
#endif
    }
}