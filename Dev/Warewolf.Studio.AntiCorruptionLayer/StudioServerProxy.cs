﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Controller;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Studio.ServerProxyLayer;

namespace Warewolf.Studio.AntiCorruptionLayer
{

    public class StudioServerProxy : IExplorerRepository
    {
        IPopupController _popupController;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public StudioServerProxy(ICommunicationControllerFactory controllerFactory,IEnvironmentConnection environmentConnection)
        {
            if (controllerFactory == null)
            {
                throw new ArgumentNullException("controllerFactory");
            }
            if (environmentConnection == null)
            {
                throw new ArgumentNullException("environmentConnection");
            }
            QueryManagerProxy = new QueryManagerProxy(controllerFactory, environmentConnection);
            UpdateManagerProxy = new ExplorerUpdateManagerProxy(controllerFactory,environmentConnection);
            VersionManager = new VersionManagerProxy(environmentConnection, controllerFactory); //todo:swap
            AdminManagerProxy = new AdminManagerProxy(controllerFactory, environmentConnection); //todo:swap
            
        }

        public async Task<IExplorerItem> LoadExplorer()
        {
            var explorerItems = await QueryManagerProxy.Load();
            ExplorerItems = explorerItems;
            return explorerItems;
        }
        public AdminManagerProxy AdminManagerProxy { get; set; }
        public IExplorerItem ExplorerItems { get; set; }
        public Dev2.Common.Interfaces.ServerProxyLayer.IVersionManager VersionManager { get; set; }
        public IQueryManager QueryManagerProxy { get; set; }
        public ExplorerUpdateManagerProxy UpdateManagerProxy { get; set; }

        public IExplorerItem FindItem(Func<IExplorerItem, bool> searchCriteria)
        {
            var explorerItemModels = ExplorerItems.Descendants().ToList();
            return searchCriteria == null ? null : explorerItemModels.FirstOrDefault(searchCriteria);
        }

        public IExplorerItem FindItemByID(Guid id)
        {
            var explorerItemModels = ExplorerItems.Descendants().ToList();
            return id == Guid.Empty ? null : explorerItemModels.Find(item => item.ResourceId == id);
        }

        public bool Rename(IExplorerItemViewModel vm, string newName)
        {
            if (vm.ResourceType == ResourceType.Folder)
                UpdateManagerProxy.RenameFolder(vm.ResourcePath, newName, vm.ResourceId);
            else
                UpdateManagerProxy.Rename(vm.ResourceId, newName);
            return true;            
        }

        public async Task<bool> Move(IExplorerItemViewModel explorerItemViewModel, IExplorerTreeItem destination)
        {
                await UpdateManagerProxy.MoveItem(explorerItemViewModel.ResourceId,destination.ResourcePath,explorerItemViewModel.ResourcePath);
                return true;
        }

        public bool Delete(IExplorerItemViewModel explorerItemViewModel)
        {
            if (explorerItemViewModel != null)
            {
                var dep = QueryManagerProxy.FetchDependants(explorerItemViewModel.ResourceId);
                var graphGenerator = new DependencyGraphGenerator();
                if (explorerItemViewModel.ResourceType != ResourceType.Version && explorerItemViewModel.ResourceType != ResourceType.Folder)
                {
                    var graph = graphGenerator.BuildGraph(dep.Message, "", 1000, 1000, 1);
                    _popupController = CustomContainer.Get<IPopupController>();

                    if (graph.Nodes.Count > 1)
                    {
                        _popupController.Show(string.Format(StringResources.Delete_Error, explorerItemViewModel.ResourceName),
                                              StringResources.Delete_Error_Title,
                                              MessageBoxButton.OK, MessageBoxImage.Error, "false", true, true, false, false);
                        return false;
                    }
                }
                if (explorerItemViewModel.ResourceType == ResourceType.Version)
                {
                    VersionManager.DeleteVersion(explorerItemViewModel.ResourceId, explorerItemViewModel.VersionNumber);
                }
                else if (explorerItemViewModel.ResourceType == ResourceType.Folder)
                {
                    if (!string.IsNullOrWhiteSpace(explorerItemViewModel.ResourcePath))
                    {
                        UpdateManagerProxy.DeleteFolder(explorerItemViewModel.ResourcePath);
                    }
                }
                else
                {
                    UpdateManagerProxy.DeleteResource(explorerItemViewModel.ResourceId);
                }
            }
            return true;
        }



        public ICollection<IVersionInfo> GetVersions(Guid id)
        {
            return new List<IVersionInfo>(VersionManager.GetVersions(id).Select(a => a.VersionInfo));
        }

        public IRollbackResult Rollback(Guid resourceId, string version)
        {
           return  VersionManager.RollbackTo(resourceId,version);
        }

        public void CreateFolder(string parentPath, string name, Guid id)
        {
            UpdateManagerProxy.AddFolder(parentPath, name, id);
        }
//
//        #endregion
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
                if(node.Children != null)
                {
                    foreach (var n in node.Children) nodes.Push(n);
                }
            }
        }

        public static IEnumerable<IExplorerItemViewModel> Descendants(this IExplorerItemViewModel root)
        {
            var nodes = new Stack<IExplorerItemViewModel>(new[] { root });
            while (nodes.Any())
            {
                IExplorerItemViewModel node = nodes.Pop();
                yield return node;
                if (node.Children != null)
                {
                    foreach (var n in node.Children) nodes.Push(n);
                }
            }
        }
    }
}
