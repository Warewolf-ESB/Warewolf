using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class UpdateResourceMessage : AbstractResourceMessage
    {
        public UpdateResourceMessage(IResourceModel contextualResourceModel) : base(contextualResourceModel)
        {
        }
    }
}
