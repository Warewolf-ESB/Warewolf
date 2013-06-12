
using System.Windows;
namespace Dev2.Studio.Core.Messages
{
    public interface IResetLayoutMessage : IMessage
    {
        FrameworkElement Context { get; }
    }
}
