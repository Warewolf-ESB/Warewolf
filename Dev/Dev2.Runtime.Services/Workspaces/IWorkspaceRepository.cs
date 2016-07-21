/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Principal;

namespace Dev2.Workspaces
{
    /// <summary>
    /// Defines the requirments for an <see cref="IWorkspace"/> repository.
    /// </summary>
    public interface IWorkspaceRepository
    {
        /// <summary>
        /// Gets the number of items in the repository.
        /// </summary>
        int Count
        {
            get;
        }

        Guid GetWorkspaceID(WindowsIdentity identity);
    }
}
