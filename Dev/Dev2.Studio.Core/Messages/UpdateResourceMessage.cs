using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class UpdateResourceMessage : AbstractResourceMessage
    {
        public UpdateResourceMessage(IContextualResourceModel contextualResourceModel)
            : base(contextualResourceModel)
        {
        }
    }
}
