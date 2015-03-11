/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    /// <summary>
    ///     Defines the requirements for a dispatcher of <see cref="IDebugState" /> messages.
    /// </summary>
    public interface IDebugDispatcher
    {
        /// <summary>
        ///     Gets the number of writers.
        /// </summary>
        int Count { get; }

        bool IsQueueEmpty { get; }

        /// <summary>
        ///     Adds the specified writer to the dispatcher.
        /// </summary>
        /// <param name="workspaceId">The ID of the workspace to which the writer belongs.</param>
        /// <param name="writer">The writer to be added.</param>
        void Add(Guid workspaceId, IDebugWriter writer);

        /// <summary>
        ///     Removes the specified workspace from the dispatcher.
        /// </summary>
        /// <param name="workspaceId">The ID of workspace to be removed.</param>
        void Remove(Guid workspaceId);

        /// <summary>
        ///     Gets the writer for the given workspace ID.
        /// </summary>
        /// <param name="workspaceId">The workspace ID to be queried.</param>
        /// <returns>The <see cref="IDebugWriter" /> with the specified ID, or <code>null</code> if not found.</returns>
        IDebugWriter Get(Guid workspaceId);

        /// <summary>
        ///     Writes the given state to any registered <see cref="IDebugWriter" />'s.
        ///     <remarks>
        ///         This must implement the one-way (fire and forget) message exchange pattern.
        ///     </remarks>
        /// </summary>
        /// <param name="debugState">The state to be written.</param>
        /// <param name="isRemoteInvoke"><code>true</code> if this is a remote invoke; <code>false</code> otherwise.</param>
        /// <param name="remoteInvokerId">The remote invoker ID.</param>
        /// <param name="parentInstanceId">The parent instance ID.</param>
        /// <param name="remoteDebugItems">The remote debug items.</param>
        // BUG 9706 - 2013.06.22 - TWR : added remote invoke parameters
        void Write(IDebugState debugState, bool isRemoteInvoke = false, string remoteInvokerId = null,
            string parentInstanceId = null, IList<IDebugState> remoteDebugItems = null);
    }
}