using System;
using System.Windows.Controls.Primitives;

namespace Dev2.Activities.Adorners
{

    /// <summary>
    /// Used to indicate when an overlay is closed by pressing the done button
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/08/01</date>
    public class UpdateCompletedEventArgs : EventArgs
    {

        /// <summary>
        /// Gets a value indicating whether this <see cref="UpdateCompletedEventArgs"/> should trigger validation
        /// </summary>
        /// <value>
        ///   <c>true</c> if validate; otherwise, <c>false</c>.
        /// </value>
        /// <author>Jurie.smit</author>
        /// <date>2013/08/01</date>
        public bool Validate { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="validate">if set to <c>true</c> the handler needs to validate the content when handling the events</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/08/01</date>
        public UpdateCompletedEventArgs(bool validate)
        {
            Validate = validate;
        }
    }
}