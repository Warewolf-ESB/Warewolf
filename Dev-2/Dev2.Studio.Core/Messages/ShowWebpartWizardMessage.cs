using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class ShowWebpartWizardMessage:IMessage
    {
        public IPropertyEditorWizard LayoutObjectViewModel { get; set; }

        public ShowWebpartWizardMessage(IPropertyEditorWizard layoutObjectViewModel)
        {
            LayoutObjectViewModel = layoutObjectViewModel;
        }
    }
}