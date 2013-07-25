using System.Windows;

namespace Dev2.Activities.Adorners
{
    public abstract class AbstractOverlayAdorner : ActivityAdorner
    {
        public abstract string HelpText { get; set; }

        public abstract object Content { get; protected set; }

        protected AbstractOverlayAdorner(UIElement adornedElement) : base(adornedElement) { }

        public abstract void ChangeContent(object content, string getContentAutomationId);
    }
}