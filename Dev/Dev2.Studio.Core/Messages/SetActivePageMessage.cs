using Dev2.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class SetActivePageMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SetActivePageMessage(ILayoutObjectViewModel layoutObjectViewModel)
        {
            LayoutObjectViewModel = layoutObjectViewModel;
        }

        public ILayoutObjectViewModel LayoutObjectViewModel { get; set; }
    }
}