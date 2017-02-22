using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Common.Interfaces
{

    public interface IExplorerDeleteProvider
    {
        IDeletedFileMetadata Delete(IExplorerItemViewModel explorerItemViewModel);
    }

    public interface IExplorerRepository :IExplorerDeleteProvider
    {

        bool Rename(IExplorerItemViewModel vm, string newName);
        Task<bool> Move(IExplorerItemViewModel explorerItemViewModel, IExplorerTreeItem destination);
        Task<IExplorerItem> LoadExplorer(bool reloadCatalogue = false);
        Task<List<string>> LoadExplorerDuplicates();
        IRollbackResult Rollback(Guid resourceId, string version);
        void CreateFolder(string parentPath, string name, Guid id);
        IAdminManager AdminManagerProxy { get; set; }
        IQueryManager QueryManagerProxy { get; set; }
        IExplorerUpdateManager UpdateManagerProxy { get; set; }
        ServerProxyLayer.IVersionManager VersionManager { get; set; }
        ICollection<IVersionInfo> GetVersions(Guid id);
        StringBuilder GetVersion(IVersionInfo versionInfo, Guid resourceId);

        IDeletedFileMetadata HasDependencies(IExplorerItemViewModel explorerItemViewModel, IDependencyGraphGenerator graphGenerator, IExecuteMessage dep);
    }
}
