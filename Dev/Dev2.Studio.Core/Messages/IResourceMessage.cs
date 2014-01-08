using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public interface IResourceMessage : IMessage
    {
        IContextualResourceModel ResourceModel { get; set; }
    }
}
