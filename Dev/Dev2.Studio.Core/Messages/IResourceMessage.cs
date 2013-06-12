using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public interface IResourceMessage : IMessage
    {
        IResourceModel ResourceModel { get; set; }
    }
}
