using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dev2.Activities.Adorners
{
    public abstract class AbstractOptionsAdorner : ActivityAdorner
    {
        public delegate void SelectionChangedEventHandler(object sender, ButtonSelectionChangedEventArgs e);
        public event SelectionChangedEventHandler SelectionChanged;

        protected AbstractOptionsAdorner(UIElement adornedElement) : base(adornedElement)
        {
        }
        
        /// <summary>
        /// Raises the <see cref="Dev2.Activities.Adorners.OptionsAdorner.SelectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ButtonSelectionChangedEventArgs"/> instance containing the event data.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public void OnSelectionChanged(ButtonSelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        public abstract void AddButton(ButtonBase button, bool attachEvents);
        public abstract void RemoveButton(ButtonBase button);
        public abstract void SelectButton(ButtonBase button);
        public abstract void ResetSelection();
    }
}