using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class RemoveNavigationResourceMessage : AbstractResourceMessage
    {
        public RemoveNavigationResourceMessage(IContextualResourceModel resourceModel) : base(resourceModel)
        {
        }
    }
}
