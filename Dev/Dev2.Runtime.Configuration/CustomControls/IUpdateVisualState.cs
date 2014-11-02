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
    ///     The IUpdateVisualState interface is used to provide the
    ///     InteractionHelper with access to the type's UpdateVisualState method.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic",
        Justification = "This is not an exception class.")]
    internal interface IUpdateVisualState
    {
        /// <summary>
        ///     Update the visual state of the control.
        /// </summary>
        /// <param name="useTransitions">
        ///     A value indicating whether to automatically generate transitions to
        ///     the new state, or instantly transition to the new state.
        /// </param>
        void UpdateVisualState(bool useTransitions);
    }
}