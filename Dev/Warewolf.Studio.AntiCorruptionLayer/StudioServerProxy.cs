using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Studio.ServerProxyLayer;
using Dev2.Studio.Core;

// ReSharper disable LoopCanBeConvertedToQuery

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public class StudioServerProxy : IExplorerRepository
    {
        IPopupController _popupController;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public StudioServerProxy(ICommunicationControllerFactory controllerFactory, IEnvironmentConnection environmentConnection)
        {
            if (controllerFactory == null)
            {
                throw new ArgumentNullException(nameof(controllerFactory));
            }
            if (environmentConnection == null)
            {
                throw new ArgumentNullException(nameof(environmentConnection));
            }
            QueryManagerProxy = new QueryManagerProxy(controllerFactory, environmentConnection);
            UpdateManagerProxy = new ExplorerUpdateManagerProxy(controllerFactory, environmentConnection);
            VersionManager = new VersionManagerProxy(controllerFactory, environmentConnection);
            AdminManagerProxy = new AdminManagerProxy(controllerFactory, environmentConnection);
        }

        public async Task<IExplorerItem> LoadExplorer(bool reloadCatalogue = false)
        {
            var explorerItems = await QueryManagerProxy.Load(reloadCatalogue);
            return explorerItems;
        }
        public Task<List<string>> LoadExplorerDuplicates()
        {
            var explorerItems = QueryManagerProxy.LoadDuplicates();
            return explorerItems;
        }

        public IAdminManager AdminManagerProxy { get; set; }
        public Dev2.Common.Interfaces.ServerProxyLayer.IVersionManager VersionManager { get; set; }
        public IQueryManager QueryManagerProxy { get; set; }
        public IExplorerUpdateManager UpdateManagerProxy { get; set; }
      

        public bool Rename(IExplorerItemViewModel vm, string newName)
        {
            if (vm == null)
            {
                throw new ArgumentNullException(nameof(vm));
            }
            if (vm.ResourceType == "Folder")
                UpdateManagerProxy.RenameFolder(vm.ResourcePath, vm.ResourcePath?.Replace(vm.ResourceName, newName));
            else
                UpdateManagerProxy.Rename(vm.ResourceId, newName);
            return true;
        }

        public async Task<bool> Move(IExplorerItemViewModel explorerItemViewModel, IExplorerTreeItem destination)
        {
            var res = await UpdateManagerProxy.MoveItem(explorerItemViewModel.ResourceId, destination.ResourcePath, explorerItemViewModel.ResourcePath);
            if (res.Status == ExecStatus.Success)
            {
                return true;
            }
            return false;
        }

        public IDeletedFileMetadata Delete(IExplorerItemViewModel explorerItemViewModel)
        {

            var explorerDeleteProvider = new ExplorerDeleteProvider(this);
            var deletedFileMetadata = explorerDeleteProvider.Delete(explorerItemViewModel);
            return deletedFileMetadata;
        }

        public IDeletedFileMetadata HasDependencies(IExplorerItemViewModel explorerItemViewModel, IDependencyGraphGenerator graphGenerator, IExecuteMessage dep)
        {
            IGraph graph = graphGenerator.BuildGraph(dep.Message, "", 1000, 1000, 1);
            _popupController = CustomContainer.Get<IPopupController>();
            if (graph.Nodes.Count > 1)
            {
                var result = _popupController.Show(string.Format(StringResources.Delete_Error, explorerItemViewModel.ResourceName),
                    string.Format(StringResources.Delete_Error_Title, explorerItemViewModel.ResourceName),
                    MessageBoxButton.OK, MessageBoxImage.Warning, "false", true, false, true, false, true, true);

                if (_popupController.DeleteAnyway)
                {
                    return new DeletedFileMetadata
                    {
                        IsDeleted = false,
                        ResourceId = explorerItemViewModel.ResourceId,
                        ShowDependencies = false,
                        ApplyToAll = _popupController.ApplyToAll,
                        DeleteAnyway = _popupController.DeleteAnyway
                    };
                }

                if (result == MessageBoxResult.OK)
                {
                    return BuildMetadata(explorerItemViewModel.ResourceId, false, false, _popupController.ApplyToAll, _popupController.DeleteAnyway);
                }
                explorerItemViewModel.ShowDependencies();
                return BuildMetadata(explorerItemViewModel.ResourceId, false, true, _popupController.ApplyToAll, _popupController.DeleteAnyway);
              
            }
            return new DeletedFileMetadata
            {
                IsDeleted = true,
                ResourceId = explorerItemViewModel.ResourceId,
                ShowDependencies = false
            };
        }

        private IDeletedFileMetadata BuildMetadata(Guid resourceId, bool isDeleted, bool showDependecnies, bool applyToAll, bool deleteAnyway)
        {
            return new DeletedFileMetadata
            {
                IsDeleted = isDeleted,
                ResourceId = resourceId,
                ShowDependencies = showDependecnies,
                ApplyToAll = applyToAll,
                DeleteAnyway = deleteAnyway
            };
        }
        
        public StringBuilder GetVersion(IVersionInfo versionInfo, Guid resourceId)
        {
            return VersionManager.GetVersion(versionInfo, resourceId);
        }

        public ICollection<IVersionInfo> GetVersions(Guid id)
        {
            return new List<IVersionInfo>(VersionManager.GetVersions(id).Select(a => a.VersionInfo));
        }

        public IRollbackResult Rollback(Guid resourceId, string version)
        {
            return VersionManager.RollbackTo(resourceId, version);
        }

        public void CreateFolder(string parentPath, string name, Guid id)
        {
            UpdateManagerProxy.AddFolder(parentPath, name, id);
        }
    }
}
