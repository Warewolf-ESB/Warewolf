/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Studio.Interfaces
{

    public interface IExplorerDeleteProvider
    {
        IDeletedFileMetadata TryDelete(IExplorerItemViewModel explorerItemViewModel);
    }

    public interface IExplorerRepository :IExplorerDeleteProvider
    {

        bool Rename(IExplorerItemViewModel vm, string newName);
        Task<bool> Move(IExplorerItemViewModel explorerItemViewModel, IExplorerTreeItem destination);
        Task<IExplorerItem> LoadExplorer();
        Task<IExplorerItem> LoadExplorer(bool reloadCatalogue);
        Task<List<string>> LoadExplorerDuplicates();
        IRollbackResult Rollback(Guid resourceId, string version);
        void CreateFolder(string parentPath, string name, Guid id);
        IAdminManager AdminManagerProxy { get; set; }
        IQueryManager QueryManagerProxy { get; set; }
        IExplorerUpdateManager UpdateManagerProxy { get; set; }
        Common.Interfaces.ServerProxyLayer.IVersionManager VersionManager { get; set; }
        ICollection<IVersionInfo> GetVersions(Guid id);
        StringBuilder GetVersion(IVersionInfo versionInfo, Guid resourceId);

        IDeletedFileMetadata HasDependencies(IExplorerItemViewModel explorerItemViewModel, IDependencyGraphGenerator graphGenerator, IExecuteMessage dep);
    }
}
