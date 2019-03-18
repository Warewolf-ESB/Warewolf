#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Controller;
using Dev2.Studio.Interfaces;



namespace Dev2.Studio.Core
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

        public async Task<IExplorerItem> LoadExplorer() => await LoadExplorer(false).ConfigureAwait(false);

        public async Task<IExplorerItem> LoadExplorer(bool reloadCatalogue)
        {
            var explorerItems = await QueryManagerProxy.Load(reloadCatalogue).ConfigureAwait(true);
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
            {
                UpdateManagerProxy.RenameFolder(vm.ResourcePath, vm.ResourcePath?.Replace(vm.ResourceName, newName));
            }
            else
            {
                UpdateManagerProxy.Rename(vm.ResourceId, newName);
            }

            return true;
        }

        public async Task<bool> Move(IExplorerItemViewModel explorerItemViewModel, IExplorerTreeItem destination)
        {
            var res = await UpdateManagerProxy.MoveItem(explorerItemViewModel.ResourceId, destination.ResourcePath, explorerItemViewModel.ResourcePath).ConfigureAwait(true);
            if (res.Status == ExecStatus.Success)
            {
                return true;
            }
            return false;
        }

        public IDeletedFileMetadata TryDelete(IExplorerItemViewModel explorerItemViewModel)
        {

            var explorerDeleteProvider = new ExplorerDeleteProvider(this);
            var deletedFileMetadata = explorerDeleteProvider.TryDelete(explorerItemViewModel);
            return deletedFileMetadata;
        }

        public IDeletedFileMetadata HasDependencies(IExplorerItemViewModel explorerItemViewModel, IDependencyGraphGenerator graphGenerator, IExecuteMessage dep)
        {
            var graph = graphGenerator.BuildGraph(dep.Message, "", 1000, 1000, 1);
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

        IDeletedFileMetadata BuildMetadata(Guid resourceId, bool isDeleted, bool showDependecnies, bool applyToAll, bool deleteAnyway) => new DeletedFileMetadata
        {
            IsDeleted = isDeleted,
            ResourceId = resourceId,
            ShowDependencies = showDependecnies,
            ApplyToAll = applyToAll,
            DeleteAnyway = deleteAnyway
        };

        public StringBuilder GetVersion(IVersionInfo versionInfo, Guid resourceId) => VersionManager.GetVersion(versionInfo, resourceId);

        public ICollection<IVersionInfo> GetVersions(Guid id) => new List<IVersionInfo>(VersionManager.GetVersions(id).Select(a => a.VersionInfo));

        public IRollbackResult Rollback(Guid resourceId, string version) => VersionManager.RollbackTo(resourceId, version);

        public void CreateFolder(string parentPath, string name, Guid id)
        {
            UpdateManagerProxy.AddFolder(parentPath, name, id);
        }
    }
}
