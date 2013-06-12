using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class DeployResourcesMessage:IMessage
    {
        public SimpleBaseViewModel ViewModel { get; set; }

        public DeployResourcesMessage(SimpleBaseViewModel viewModel)
        {
            ViewModel = viewModel;
        }
    }
}