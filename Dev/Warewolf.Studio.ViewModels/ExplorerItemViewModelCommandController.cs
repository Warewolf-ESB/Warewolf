using System;
using System.Linq;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Studio.Core.Interfaces;
// ReSharper disable NonLocalizedString
// ReSharper disable InconsistentNaming

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

        internal void OpenCommand(ExplorerItemViewModel item, IServer server)
        {
            Dev2Logger.Info("Open resource: " + item.ResourceName + " - ResourceId: " + item.ResourceId);
            if (item.IsFolder)
            {
                item.IsExpanded = !item.IsExpanded;
            }
            else if (item.IsResourceVersion)
            {
                OpenVersionCommand(item.Parent.ResourceId, item.VersionInfo);
            }
            else
            {
                SetActiveStates(_shellViewModel, server);
                _shellViewModel.OpenResource(item.ResourceId,server.EnvironmentID, server);
            }
        }

        public void OpenVersionCommand(Guid resourceId, IVersionInfo versionInfo)
        {
            _shellViewModel.OpenVersion(resourceId, versionInfo);
        }

        public void NewServiceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewService(resourcePath);
        }

        public void DebugStudioCommand(Guid resourceId, IServer server)
        {
            _shellViewModel.StudioDebug(resourceId, server);
        }

        public void DebugBrowserCommand(Guid resourceId, IServer server)
        {
            _shellViewModel.BrowserDebug(resourceId, server);
        }

        public void ScheduleCommand(Guid resourceId)
        {
            _shellViewModel.NewSchedule(resourceId);
        }

        public void RunAllTestsCommand(Guid resourceId)
        {
            _shellViewModel.RunAllTests(resourceId);
        }

        public void CopyUrlCommand(Guid resourceId, IServer server)
        {
            _shellViewModel.CopyUrlLink(resourceId, server);
        }

        public void NewDropboxSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewDropboxSource(resourcePath);
        }
        public void NewSharepointSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewSharepointSource(resourcePath);
        }
        public void NewRabbitMQSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewRabbitMQSource(resourcePath);
        }
        public void NewExchangeSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewExchangeSource(resourcePath);
        }
        public void NewEmailSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewEmailSource(resourcePath);
        }
        public void NewWebSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewWebSource(resourcePath);
        }
        public void NewPluginSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewPluginSource(resourcePath);
        }
        public void NewComPluginSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewComPluginSource(resourcePath);
        }
        public void NewWcfSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewWcfSource(resourcePath);
        }
        public void NewSqlServerSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewSqlServerSource(resourcePath);
        }
        public void NewMySqlSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewMySqlSource(resourcePath);
        }
        public void NewPostgreSqlSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewPostgreSqlSource(resourcePath);
        }
        public void NewOracleSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewOracleSource(resourcePath);
        }
        public void NewOdbcSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewOdbcSource(resourcePath);
        }

        public void NewServerSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewServerSource(resourcePath);
        }

        private static void SetActiveStates(IShellViewModel shellViewModel, IServer server)
        {
            shellViewModel.SetActiveEnvironment(server.EnvironmentID);
            shellViewModel.SetActiveServer(server);
        }

        public void ShowDependenciesCommand(Guid resourceId, IServer server,bool isSource)
        {
            _shellViewModel.ShowDependencies(resourceId, server, isSource);
        }

        public void DeleteVersionCommand(IExplorerRepository explorerRepository, ExplorerItemViewModel explorerItemViewModel, IExplorerTreeItem parent, string resourceName)
        {
            if (_popupController.ShowDeleteVersionMessage(resourceName) == MessageBoxResult.Yes)
            {
                explorerRepository.Delete(explorerItemViewModel);
                parent?.RemoveChild(parent.Children.First(a => a.ResourceName == resourceName));
            }
        }
        public void DuplicateResource(IExplorerItemViewModel explorerItemViewModel)
        {
            _shellViewModel.DuplicateResource(explorerItemViewModel);
        }

        public void DeleteCommand(IEnvironmentModel environmentModel, IExplorerTreeItem parent, IExplorerRepository explorerRepository, ExplorerItemViewModel explorerItemViewModel, IPopupController popupController, IServer server)
        {
            try
            {
                if (explorerItemViewModel.IsResourceVersion)
                {
                    DeleteVersionCommand(explorerRepository, explorerItemViewModel, parent, explorerItemViewModel.ResourceName);
                }
                else
                {
                    var messageBoxResult = popupController.Show(popupController.GetDeleteConfirmation(explorerItemViewModel.ResourceName));
                    if (environmentModel != null && messageBoxResult == MessageBoxResult.Yes)
                    {
                        _shellViewModel.CloseResource(explorerItemViewModel.ResourceId, environmentModel.ID);
                        var deletedFileMetadata = explorerRepository.Delete(explorerItemViewModel);
                        if (deletedFileMetadata.IsDeleted)
                        {
                            if (explorerItemViewModel.ResourceType == @"ServerSource" || explorerItemViewModel.IsServer)
                            {
                                server.UpdateRepository.FireServerSaved(explorerItemViewModel.ResourceId);
                            }
                            parent?.RemoveChild(explorerItemViewModel);
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                explorerItemViewModel.ShowErrorMessage(ex.Message, @"Delete not allowed");
            }
        }

        public ExplorerItemViewModel CreateChild(string name, Guid id, IServer server, ExplorerItemViewModel explorerItem, Action<IExplorerItemViewModel> selectAction)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var child = new ExplorerItemViewModel(server, explorerItem, selectAction, _shellViewModel, _popupController)
            {
                ResourcePath = explorerItem.ResourcePath + "\\" + name,
                IsSelected = true,
                IsRenaming = true,
                CanDelete = true,
                IsFolder = true,
                IsNewFolder = true,
                ResourceId = id,
                ResourceType = @"Folder",
                AllowResourceCheck = explorerItem.AllowResourceCheck,
                IsResourceChecked = explorerItem.IsResourceChecked,
                ShowContextMenu = explorerItem.ShowContextMenu
            };
            
           var permissions = server.GetPermissions(explorerItem.ResourceId);
            child.SetPermissions(permissions);

            child.ResourceName = name;
            child.IsRenaming = true;
            child.IsSaveDialog = explorerItem.IsSaveDialog;

            return child;
        }

        internal void ViewApisJsonCommand(string resourcePath, Uri webServerUri)
        {
            _shellViewModel.ViewApisJson(resourcePath, webServerUri);
        }

        public void ViewSwaggerCommand(Guid resourceId, IServer server)
        {
            _shellViewModel.ViewSwagger(resourceId, server);
        }

        public void CreateTest(Guid resourceId)
        {
            _shellViewModel.CreateTest(resourceId);
        }
    }
}