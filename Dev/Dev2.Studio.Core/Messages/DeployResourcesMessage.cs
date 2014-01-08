using Dev2.Studio.Core.ViewModels.Base;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class DeployResourcesMessage : IMessage
    {
        public SimpleBaseViewModel ViewModel { get; set; }

        public DeployResourcesMessage(SimpleBaseViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}