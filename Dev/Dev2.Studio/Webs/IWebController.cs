using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Webs
{
    public interface IWebController
    {
        void DisplayDialogue(IContextualResourceModel resourceModel, bool includeArgs);
        void CloseWizard();
    }
}
