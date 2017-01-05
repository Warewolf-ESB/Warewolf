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
// ReSharper disable UnusedMemberInSuper.Global

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    /// <summary>
    ///     Defines the requirements for a dispatcher of <see cref="IDebugState" /> messages.
    /// </summary>
    public interface IDebugDispatcher
    {


        /// <summary>
        ///     Removes the specified workspace from the dispatcher.
        /// </summary>
        /// <param name="workspaceId">The ID of workspace to be removed.</param>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        void Remove(Guid workspaceId);

        /// <summary>
        ///     Gets the writer for the given workspace ID.
        /// </summary>
        /// <param name="workspaceId">The workspace ID to be queried.</param>
        /// <returns>The <see cref="IDebugWriter" /> with the specified ID, or <code>null</code> if not found.</returns>
        IDebugWriter Get(Guid workspaceId);
        
        void Write(IDebugState debugState,bool isTestExecution = false,string testName="", bool isRemoteInvoke = false, string remoteInvokerId = null,
            string parentInstanceId = null, IList<IDebugState> remoteDebugItems = null);
    }
}