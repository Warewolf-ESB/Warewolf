
namespace Dev2.Studio.Core.Messages
{
    public interface ITabMessage : IMessage
    {
        object Context { get; set; }
    }
}
