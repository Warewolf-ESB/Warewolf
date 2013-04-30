using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class AddWorkflowDesignerMessage : IMessage
    {
        public IResourceModel Resource { get; set; }

        public AddWorkflowDesignerMessage(IResourceModel resource)
        {
            Resource = resource;
        }
    }
}