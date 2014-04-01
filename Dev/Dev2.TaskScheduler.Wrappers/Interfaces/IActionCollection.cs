using System;
using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers.Interfaces
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
        bool ContainsType(Type actionType);

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents the actions in this collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents the actions in this collection.
        /// </returns>
        string ToString();
    }
}