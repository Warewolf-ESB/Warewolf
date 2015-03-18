using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface IDeployViewControl:IView
    {
        void SelectSourceServer(string sourceServerName);

        void SelectDestinationServer(string destinationServer);

        void SelectSourceResource(string resourceName);

        void Deploy();
    }
}