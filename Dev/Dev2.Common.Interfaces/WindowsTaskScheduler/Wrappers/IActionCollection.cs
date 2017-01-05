/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface IActionCollection : IEnumerable<IAction>, IDisposable, IWrappedObject<ActionCollection>
    {
        /// <summary>
        ///     Gets the number of actions in the collection.
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     Adds an action to the task.
        /// </summary>
        /// <param name="action">
        ///     A derived <see cref="Microsoft.Win32.TaskScheduler.Action" /> class.
        /// </param>
        /// <returns>
        ///     The bound <see cref="Microsoft.Win32.TaskScheduler.Action" /> that was added to the collection.
        /// </returns>
        IAction Add(IAction action);


        /// <summary>
        ///     Determines whether the specified action type is contained in this collection.
        /// </summary>
        /// <param name="actionType">Type of the action.</param>
        /// <returns>
        ///     <c>true</c> if the specified action type is contained in this collection; otherwise, <c>false</c>.
        /// </returns>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        bool ContainsType(Type actionType);

    }
}