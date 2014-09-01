
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public interface IResetLayoutMessage : IMessage
    {
        FrameworkElement Context { get; }
    }
}
