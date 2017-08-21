using Microsoft.AspNet.SignalR.Client;

namespace Dev2.Runtime.Interfaces
{
    public interface IHubFactory
    {
        IHubProxy CreateHubProxy(Data.ServiceModel.Connection connection);
    }
}