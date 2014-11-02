/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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

using System.Collections;

namespace System.Windows.Controls
{
    /// <summary>
    ///     Provides data for the
    ///     <see cref="E:System.Windows.Controls.AutoCompleteBox.Populated" />
    ///     event.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    public class PopulatedEventArgs : RoutedEventArgs
    {
        /// <summary>
        ///     Gets the list of possible matches added to the drop-down portion of
        ///     the <see cref="T:System.Windows.Controls.AutoCompleteBox" />
        ///     control.
        /// </summary>
        /// <value>
        ///     The list of possible matches added to the
        ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" />.
        /// </value>
        public IEnumerable Data { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:System.Windows.Controls.PopulatedEventArgs" />.
        /// </summary>
        /// <param name="data">
        ///     The list of possible matches added to the
        ///     drop-down portion of the
        ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </param>
        public PopulatedEventArgs(IEnumerable data)
        {
            Data = data;
        }

#if !SILVERLIGHT
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:System.Windows.Controls.PopulatedEventArgs" />.
        /// </summary>
        /// <param name="data">
        ///     The list of possible matches added to the
        ///     drop-down portion of the
        ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </param>
        /// <param name="routedEvent">The routed event identifier for this instance.</param>
        public PopulatedEventArgs(IEnumerable data, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Data = data;
        }
#endif
    }
}