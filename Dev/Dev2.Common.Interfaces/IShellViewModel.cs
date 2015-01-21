using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Studio.ViewModels;

namespace Dev2.Common.Interfaces
{
    public interface IShellViewModel
    {
        void AddService(IResource resource);

        void DeployService(IExplorerItemViewModel resourceToDeploy);

        void UpdateHelpDescriptor(IHelpDescriptor helpDescriptor);
        void NewResource(ResourceType? type);
    }
}