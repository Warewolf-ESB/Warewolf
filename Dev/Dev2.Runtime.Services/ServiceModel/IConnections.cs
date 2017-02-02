using Dev2.Runtime.Diagnostics;
using Microsoft.AspNet.SignalR.Client;

namespace Dev2.Runtime.ServiceModel
{
    public interface IConnections
    {
        ValidationResult CanConnectToServer(Dev2.Data.ServiceModel.Connection connection);

        IHubProxy CreateHubProxy(Dev2.Data.ServiceModel.Connection connection);
    }
}