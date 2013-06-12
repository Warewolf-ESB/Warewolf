using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Messages;

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
