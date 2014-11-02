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

using System.Diagnostics.CodeAnalysis;

namespace System.Windows.Controls
{
    /// <summary>
    ///     Represents the method that will handle the
    ///     <see cref="E:System.Windows.Controls.AutoCompleteBox.Populated" />
    ///     event of a <see cref="T:System.Windows.Controls.AutoCompleteBox" />
    ///     control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///     A
    ///     <see cref="T:System.Windows.Controls.PopulatedEventArgs" /> that
    ///     contains the event data.
    /// </param>
    /// <QualityBand>Stable</QualityBand>
    [SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances",
        Justification = "There is no generic RoutedEventHandler.")]
    public delegate void PopulatedEventHandler(object sender, PopulatedEventArgs e);
}