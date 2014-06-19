using Dev2.Models;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class DeployResourcesMessage : IMessage
    {
        public IExplorerItemModel ViewModel { get; set; }

        public DeployResourcesMessage(IExplorerItemModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}