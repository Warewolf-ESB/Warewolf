using Dev2.Studio.Core.ViewModels.Base;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class CloseWizardMessage : IMessage
    {
        public SimpleBaseViewModel ResourceWizardViewModel { get; set; }

        public CloseWizardMessage(SimpleBaseViewModel resourceWizardViewModel)
        {
            ResourceWizardViewModel = resourceWizardViewModel;
        }
    }
}