using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class SaveResourceMessage:IMessage
    {
        public IContextualResourceModel Resource { get; set; }
        public bool IsLocalSave { get; set; }
        public bool AddToTabManager { get; set; }

        public SaveResourceMessage(IContextualResourceModel resource, bool isLocalSave, bool addToTabManager = true)
        {
            Resource = resource;
            IsLocalSave = isLocalSave;
            AddToTabManager = addToTabManager;
        }
    }
}