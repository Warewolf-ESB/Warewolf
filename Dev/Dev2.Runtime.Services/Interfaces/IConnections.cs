using Dev2.Runtime.Diagnostics;

namespace Dev2.Runtime.Interfaces
{
    public interface IConnections : IHubFactory
    {
        ValidationResult CanConnectToServer(Data.ServiceModel.Connection connection);
    }
}