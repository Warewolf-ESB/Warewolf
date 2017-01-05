/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.


namespace System.Windows.Controls
{
    /// <summary>
    /// Provides data for the
    /// <see cref="E:System.Windows.Controls.AutoCompleteBox.Populating" />
    /// event.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    public class PopulatingEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the text that is used to determine which items to display in
        /// the <see cref="T:System.Windows.Controls.AutoCompleteBox" />
        /// control.
        /// </summary>
        /// <value>The text that is used to determine which items to display in
        /// the <see cref="T:System.Windows.Controls.AutoCompleteBox" />.</value>
        public string Parameter { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the
        /// <see cref="E:System.Windows.Controls.AutoCompleteBox.Populating" />
        /// event should be canceled.
        /// </summary>
        /// <value>True to cancel the event, otherwise false. The default is
        /// false.</value>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:System.Windows.Controls.PopulatingEventArgs" />.
        /// </summary>
        /// <param name="parameter">The value of the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.SearchText" />
        /// property, which is used to filter items for the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.</param>
        public PopulatingEventArgs(string parameter)
        {
            Parameter = parameter;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:System.Windows.Controls.PopulatingEventArgs" />.
        /// </summary>
        /// <param name="parameter">The value of the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.SearchText" />
        /// property, which is used to filter items for the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.</param>
        /// <param name="routedEvent">The routed event identifier for this instance.</param>
        public PopulatingEventArgs(string parameter, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Parameter = parameter;
        }
#endif
    }
}
