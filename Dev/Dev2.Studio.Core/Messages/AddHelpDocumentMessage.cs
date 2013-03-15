using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class AddHelpDocumentMessage:IMessage
    {
        public IResourceModel Resource { get; set; }

        public AddHelpDocumentMessage(IResourceModel resource)
        {
            Resource = resource;
        }
    }
}