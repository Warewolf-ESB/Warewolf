using Dev2.Studio.Core.Interfaces;

namespace Dev2.Webs
{
    public interface IWebController
    {
        void DisplayDialogue(IContextualResourceModel resourceModel, bool includeArgs);
        void CloseWizard();
    }
}
