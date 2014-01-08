using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public abstract class AbstractResourceMessage : IResourceMessage
    {
        public IContextualResourceModel ResourceModel { get; set; }

        protected AbstractResourceMessage(IContextualResourceModel resourceModel)
        {
            ResourceModel = resourceModel;
        }
    }
}
