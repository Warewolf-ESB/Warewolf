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
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;

namespace Dev2.Common.Interfaces.Infrastructure
{
    public interface IExplorerResourceRepository
    {

        IExplorerItem Load(Guid workSpaceId, bool reload = false);
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        IExplorerItem Load(string type, Guid workSpaceId);
        IExplorerRepositoryResult RenameItem(IExplorerItem itemToRename, string newName, Guid workSpaceId);
        IExplorerRepositoryResult RenameFolder(string path, string newName, Guid workSpaceId);
        IExplorerRepositoryResult DeleteItem(IExplorerItem itemToRename, Guid workSpaceId);
        IExplorerRepositoryResult AddItem(IExplorerItem itemToRename, Guid workSpaceId);
        IExplorerRepositoryResult MoveItem(IExplorerItem itemToMove, string newPath, Guid empty);
    }

    public interface IExplorerRepositoryResult
    {
        ExecStatus Status { get; set; }

        string Message { get; set; }
    }
}
