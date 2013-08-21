using System;
using System.Windows.Controls.Primitives;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// Used to indicate which of the adorner options are currently selected
    /// Fired when the selection changes
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public class ButtonSelectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the currently selected option.
        /// </summary>
        /// <value>
        /// The selected option.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public ButtonBase SelectedOption { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonSelectionChangedEventArgs"/> class.
        /// </summary>
        /// <param name="selectedOption">The selected option.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public ButtonSelectionChangedEventArgs(ButtonBase selectedOption)
        {
            SelectedOption = selectedOption;
        }
    }
}