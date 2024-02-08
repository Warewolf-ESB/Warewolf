using Microsoft.AspNetCore.SignalR.Client;

namespace Dev2.Runtime.Interfaces
{
    public interface IHubFactory
    {
        HubConnection GetHubConnection(Data.ServiceModel.Connection connection);
    }
}