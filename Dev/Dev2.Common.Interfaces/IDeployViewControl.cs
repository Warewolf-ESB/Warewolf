using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface IDeployViewControl:IView
    {
        void SelectSourceServer(string sourceServerName);

        void SelectDestinationServer(string destinationServer);

        void SelectSourceResource(string resourceName);

        void Deploy();

        void SelectAllDependencies();

        void EnterDestinationFilter(string filterTerm);

        void ClearDestinationFilter();

        void EnterSourceFilter(string filterTerm);

        void ClearSourceFilter();

        string GetCurrentValidationMessage();

        bool IsSourceResourceIsVisible(string resourceName);

        bool IsSourceResourceSelected(string resourceName);

        bool IsDestinationResourceIsVisible(string resourceName);

        bool IsDestinationResourceSelected(string resourceName);
    }
}