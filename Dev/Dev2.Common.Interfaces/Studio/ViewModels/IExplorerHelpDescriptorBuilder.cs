using Dev2.Common.Interfaces.Help;

namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IExplorerHelpDescriptorBuilder
    {
        IHelpDescriptor Build(IExplorerItemViewModel model,ExplorerEventContext ctx );
    }
}