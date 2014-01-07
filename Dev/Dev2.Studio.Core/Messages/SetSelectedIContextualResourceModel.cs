using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class SetSelectedIContextualResourceModel : IMessage
    {
        public SetSelectedIContextualResourceModel(IContextualResourceModel selectedResource, bool didDoubleClickOccur)
        {
            DidDoubleClickOccur = didDoubleClickOccur;
            SelectedResource = selectedResource;
        }

        public IContextualResourceModel SelectedResource { get; set; }

        public bool DidDoubleClickOccur { get; set; }
    }
}
