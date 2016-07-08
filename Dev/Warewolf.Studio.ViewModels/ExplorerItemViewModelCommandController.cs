using System;
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerItemViewModelCommandController
    {
        private static IShellViewModel _shellViewModel;
        static IPopupController _popupController;

        public ExplorerItemViewModelCommandController(IShellViewModel shellViewModel, IPopupController popupController)
        {
            _shellViewModel = shellViewModel;
            _popupController = popupController;
        }
        public void RollbackCommand(IExplorerRepository explorerRepository, IExplorerTreeItem parent, Guid resourceId, string versionNumber)
        {
            var output = explorerRepository.Rollback(resourceId, versionNumber);
            parent.AreVersionsVisible = true;
            parent.ResourceName = output.DisplayName;
        }

        internal void OpenCommand(Guid resourceId, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.OpenResource(resourceId, server);
        }

        public void OpenVersionCommand(Guid resourceId, IVersionInfo versionInfo)
        {
            _shellViewModel.OpenVersion(resourceId, versionInfo);
        }

        public void NewCommand(string type, string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewResource(type, resourcePath);
        }

        private static void SetActiveStates(IShellViewModel shellViewModel, IServer server)
        {
            shellViewModel.SetActiveEnvironment(server.EnvironmentID);
            shellViewModel.SetActiveServer(server);
        }

        public void ShowDependenciesCommand(Guid resourceId, IServer server)
        {
            _shellViewModel.ShowDependencies(resourceId, server);
        }

        public void DeleteVersionCommand(IExplorerRepository explorerRepository, ExplorerItemViewModel explorerItemViewModel, IExplorerTreeItem parent, string resourceName)
        {
            if (_popupController.ShowDeleteVersionMessage(resourceName) == MessageBoxResult.Yes)
            {
                explorerRepository.Delete(explorerItemViewModel);
                parent?.RemoveChild(parent.Children.First(a => a.ResourceName == resourceName));
            }
        }

        public IDeletedFileMetadata DeleteCommand(Guid resourceId, Guid id, IExplorerTreeItem parent, IExplorerRepository explorerRepository, ExplorerItemViewModel explorerItemViewModel)
        {
            _shellViewModel.CloseResource(resourceId, id);
            // Remove the item from the parent for studio change to show, then do the delete from the server.
            parent?.RemoveChild(explorerItemViewModel);
            //This Delete process is quite long and should happen after the studio change so that the user caqn continue without the studio hanging
            return explorerRepository.Delete(explorerItemViewModel);
        }

        internal void CreateFolderCommand(IExplorerRepository explorerRepository, string resourcePath, string name, Guid id)
        {
            explorerRepository.CreateFolder(resourcePath, name, id);
        }

        public ExplorerItemViewModel CreateChild(string name, Guid id, IServer server, ExplorerItemViewModel explorerItem, Action<IExplorerItemViewModel> selectAction)
        {
            var child = new ExplorerItemViewModel(server, explorerItem, selectAction, _shellViewModel, _popupController)
            {
                ResourceName = name,
                ResourceId = id,
                ResourceType = "Folder",
                AllowResourceCheck = explorerItem.AllowResourceCheck,
                IsResourceChecked = explorerItem.IsResourceChecked,
                CanCreateFolder = explorerItem.CanCreateFolder,
                CanCreateSource = explorerItem.CanCreateSource,
                CanShowVersions = explorerItem.CanShowVersions,
                CanRename = explorerItem.CanRename,
                CanDeploy = explorerItem.CanDeploy,
                CanShowDependencies = explorerItem.CanShowDependencies,
                ResourcePath = explorerItem.ResourcePath + "\\" + name,
                CanCreateWorkflowService = explorerItem.CanCreateWorkflowService,
                ShowContextMenu = explorerItem.ShowContextMenu,
                IsSelected = true,
                IsRenaming = true,
                IsFolder = true
            };
            return child;
        }
    }
}