using System.Windows;

namespace Dev2.Activities.Adorners
{
    public abstract class AbstractOverlayAdorner : ActivityAdorner
    {
        public delegate void UpdateCompletedEventHandler(object sender, UpdateCompletedEventArgs e);
        public event UpdateCompletedEventHandler UpdateComplete;

        /// <summary>
        /// Raises the <see cref="E:UpdateComplete" /> event.
        /// </summary>
        /// <param name="e">The <see cref="UpdateCompletedEventArgs"/> instance containing the event data.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/08/01</date>
        public void OnUpdateComplete(UpdateCompletedEventArgs e)
        {
            if(UpdateComplete != null)
                UpdateComplete(this, e);
        }

        public abstract string HelpText { get; set; }

        public abstract object Content { get; protected set; }

        protected AbstractOverlayAdorner(UIElement adornedElement) : base(adornedElement) { }

        public abstract void ChangeContent(object content, string getContentAutomationId);

        public abstract void UpdateContentSize();
    }
}