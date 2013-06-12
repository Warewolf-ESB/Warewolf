
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public interface ITabMessage : IMessage
    {
        object Context { get; set; }
    }
}
