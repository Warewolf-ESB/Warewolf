using System.Windows;

namespace Dev2.Studio.Interfaces.Messages
{
    public interface IResetLayoutMessage : IMessage
    {
        FrameworkElement Context { get; }
    }
}
