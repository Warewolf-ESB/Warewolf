using System.Collections.Generic;
using System.Windows;
using Dev2.Providers.Errors;

namespace Dev2.Activities.Adorners
{
    public abstract class AbstractOverlayAdorner : ActivityAdorner
    {
        public delegate void UpdateCompletedEventHandler(object sender, UpdateCompletedEventArgs e);
        public event UpdateCompletedEventHandler UpdateComplete;

        public void OnUpdateComplete(UpdateCompletedEventArgs e)
        {
            if(UpdateComplete != null)
                UpdateComplete(this, e);
        }

        public abstract string HelpText { get; set; }

        public abstract List<IActionableErrorInfo> Errors { get; set; }

        public abstract object Content { get; protected set; }

        protected AbstractOverlayAdorner(UIElement adornedElement) : base(adornedElement) { }

        public abstract void ChangeContent(object content, string getContentAutomationId);

        public abstract void UpdateContentSize();
    }
}