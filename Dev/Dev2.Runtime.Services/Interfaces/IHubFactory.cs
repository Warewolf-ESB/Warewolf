using Dev2.SignalR.Wrappers;
using Microsoft.AspNetCore.SignalR.Client;

namespace Dev2.Runtime.Interfaces
{
    public interface IHubFactory
    {
        //IHubProxy CreateHubProxy(Data.ServiceModel.Connection connection);
        HubConnection GetHubConnection(Data.ServiceModel.Connection connection);
        HubConnection GetTestHubConnection(Data.ServiceModel.Connection connection);
    }
}