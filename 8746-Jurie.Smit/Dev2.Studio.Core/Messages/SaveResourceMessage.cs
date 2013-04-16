using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class SaveResourceMessage:IMessage
    {
        public IContextualResourceModel Resource { get; set; }

        public SaveResourceMessage(IContextualResourceModel resource)
        {
            Resource = resource;
        }
    }
}