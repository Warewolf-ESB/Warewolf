using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class UpdateResourceDesignerMessage : IMessage
    {
        public IContextualResourceModel ResourceModel { get; set; }
        public UpdateResourceDesignerMessage(IContextualResourceModel resourceModel)
        {
            ResourceModel = resourceModel;
        }
    }
}
