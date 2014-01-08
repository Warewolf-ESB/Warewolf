using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class ResetLayoutMessage : IResetLayoutMessage
    {
        public ResetLayoutMessage(FrameworkElement context)
        {
            Context = context;
        }

        public FrameworkElement Context { get; private set; }
    }
}
