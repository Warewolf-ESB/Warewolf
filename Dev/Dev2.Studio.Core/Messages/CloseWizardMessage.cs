using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class CloseWizardMessage:IMessage
    {
        public SimpleBaseViewModel ResourceWizardViewModel { get; set; }

        public CloseWizardMessage(SimpleBaseViewModel resourceWizardViewModel)
        {
            ResourceWizardViewModel = resourceWizardViewModel;
        }
    }
}