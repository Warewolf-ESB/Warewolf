using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common;
using Dev2.Common.DependencyVisualization;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Studio.ServerProxyLayer;
using Dev2.Studio.Core;

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
            ExplorerItems = explorerItems;
            return explorerItems;
        }
        public Task<List<string>> LoadExplorerDuplicates()
        {
            var explorerItems = QueryManagerProxy.LoadDuplicates();
            return explorerItems;
        }

        public IAdminManager AdminManagerProxy { get; set; }
        public IExplorerItem ExplorerItems { get; set; }
        public Dev2.Common.Interfaces.ServerProxyLayer.IVersionManager VersionManager { get; set; }
        public IQueryManager QueryManagerProxy { get; set; }
        public IExplorerUpdateManager UpdateManagerProxy { get; set; }

        public IExplorerItem FindItemByID(Guid id)
        {
            var explorerItemModels = ExplorerItems.Descendants().ToList();
            return id == Guid.Empty ? null : explorerItemModels.Find(item => item.ResourceId == id);
        }

        public bool Rename(IExplorerItemViewModel vm, string newName)
        {
            if (vm.ResourceType == "Folder")
                UpdateManagerProxy.RenameFolder(vm.ResourcePath, vm.ResourcePath.Replace(vm.ResourceName,newName));
            else
                UpdateManagerProxy.Rename(vm.ResourceId, newName);
            return true;
        }

        public async Task<bool> Move(IExplorerItemViewModel explorerItemViewModel, IExplorerTreeItem destination)
        {
            await UpdateManagerProxy.MoveItem(explorerItemViewModel.ResourceId, destination.ResourcePath, explorerItemViewModel.ResourcePath);
            return true;
        }

        public IDeletedFileMetadata Delete(IExplorerItemViewModel explorerItemViewModel)
        {
            try
            {
                if (explorerItemViewModel != null)
                {
                    var graphGenerator = new DependencyGraphGenerator();
                    if (explorerItemViewModel.ResourceType != "Version" && explorerItemViewModel.ResourceType != "Folder")
                    {
                        var dep = QueryManagerProxy.FetchDependants(explorerItemViewModel.ResourceId);
                        var deleteFileMeta = HasDependencies(explorerItemViewModel, graphGenerator, dep);
                        if (deleteFileMeta.IsDeleted)
                        {
                            UpdateManagerProxy.DeleteResource(explorerItemViewModel.ResourceId);
                        }
                        return deleteFileMeta;
                    }
                    if (explorerItemViewModel.ResourceType == "Version")
                    {
                        VersionManager.DeleteVersion(explorerItemViewModel.ResourceId, explorerItemViewModel.VersionNumber,explorerItemViewModel.ResourcePath);
                    }
                    else if (explorerItemViewModel.ResourceType == "Folder")
                    {
                        var explorerItemViewModels = explorerItemViewModel.AsList();
                        // ReSharper disable once LoopCanBeConvertedToQuery
                        var deleteFileMetaData = new DeletedFileMetadata
                        {
                            IsDeleted = true,
                            ShowDependencies = false
                        };
                        foreach (IExplorerItemViewModel itemViewModel in explorerItemViewModels)
                        {
                            if (itemViewModel.ResourceType != "Folder")
                            {
                                var dependants = QueryManagerProxy.FetchDependants(itemViewModel.ResourceId);
                                if (dependants != null)
                                {
                                    var deletedFileMetadata = HasDependencies(itemViewModel, graphGenerator, dependants);
                                    
                                    if (!deletedFileMetadata.IsDeleted)
                                    {
                                        deleteFileMetaData.IsDeleted = false;
                                        deleteFileMetaData.ShowDependencies = true;
                                        deleteFileMetaData.ResourceId = itemViewModel.ResourceId;
                                    }
                                }
                            }
                        }
                        if (deleteFileMetaData.IsDeleted)
                        {
                            if (!string.IsNullOrWhiteSpace(explorerItemViewModel.ResourcePath))
                            {
                                UpdateManagerProxy.DeleteFolder(explorerItemViewModel.ResourcePath);
                            }
                        }
                        return deleteFileMetaData;
                    }
                }
                return new DeletedFileMetadata
                {
                    IsDeleted = true
                };
            }
            catch (Exception)
            {
                return new DeletedFileMetadata
                {
                    IsDeleted = false
                };
            }
        }

        private IDeletedFileMetadata HasDependencies(IExplorerItemViewModel explorerItemViewModel, DependencyGraphGenerator graphGenerator, IExecuteMessage dep)
        {
            var graph = graphGenerator.BuildGraph(dep.Message, "", 1000, 1000, 1);
            _popupController = CustomContainer.Get<IPopupController>();
            RemoveDeletedNodes(graph);
            if (graph.Nodes.Count > 1)
            {
                var result = _popupController.Show(string.Format(StringResources.Delete_Error, explorerItemViewModel.ResourceName),
                    StringResources.Delete_Error_Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, "false", true, true, false, false);
                if (result == MessageBoxResult.OK)
                {
                    return new DeletedFileMetadata
                    {
                        IsDeleted = false,
                        ResourceId = explorerItemViewModel.ResourceId,
                        ShowDependencies = false
                    };
                }
                explorerItemViewModel.ShowDependencies();
                return new DeletedFileMetadata
                {
                    IsDeleted = false,
                    ResourceId = explorerItemViewModel.ResourceId,
                    ShowDependencies = true
                };
            }
            return new DeletedFileMetadata
            {
                IsDeleted = true,
                ResourceId = explorerItemViewModel.ResourceId,
                ShowDependencies = false
            };
        }

        private void RemoveDeletedNodes(Graph graph)
        {
            var nodes = graph.Nodes.Select(node => node.ID).ToList();
            IList<Node> nodesToRemove = new List<Node>();
            foreach (var nod in nodes)
            {
                var findNode = FindItemByID(new Guid(nod));
                if(findNode == null)
                    nodesToRemove.Add(graph.Nodes.FirstOrDefault(node => node.ID == nod));
            }

            foreach(var node in graph.Nodes.ToList().Where(node => nodesToRemove.Any(p => p.ID == node.ID)))
                graph.Nodes.Remove(node);
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

    public static class StudioServerProxyHelper
    {
        public static IEnumerable<IExplorerItem> Descendants(this IExplorerItem root)
        {
            var nodes = new Stack<IExplorerItem>(new[] { root });
            while (nodes.Any())
            {
                IExplorerItem node = nodes.Pop();
                yield return node;
                if (node.Children != null)
                {
                    foreach (var n in node.Children) nodes.Push(n);
                }
            }
        }
    }
}
