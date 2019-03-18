#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Collections.ObjectModel;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Studio.Interfaces;
using Dev2;
using Dev2.Instrumentation;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerItemViewModelCommandController
    {
        readonly IShellViewModel _shellViewModel;
        readonly IPopupController _popupController;

        public ExplorerItemViewModelCommandController(IShellViewModel shellViewModel, IPopupController popupController)
        {
            _shellViewModel = shellViewModel;
            _popupController = popupController;
        }

        public void RollbackCommand(IExplorerRepository explorerRepository, IExplorerTreeItem parent, Guid resourceId, string versionNumber)
        {
            var output = explorerRepository.Rollback(resourceId, versionNumber);
            parent.AreVersionsVisible = false;
            parent.ResourceName = output.DisplayName;
            if (parent.Server != null)
            {
                _shellViewModel.CloseResource(resourceId, parent.Server.EnvironmentID);
                _shellViewModel.OpenCurrentVersion(resourceId, parent.Server.EnvironmentID);
            }
            parent.AreVersionsVisible = true;
        }

        internal void OpenCommand(ExplorerItemViewModel item, IServer server)
        {
            Dev2Logger.Info("Open resource: " + item.ResourceName + " - ResourceId: " + item.ResourceId, "Warewolf Info");

            var applicationTracker = CustomContainer.Get<IApplicationTracker>();
            if (applicationTracker != null)
            {
                if (item.ResourceName == "Shared Resources Server")
                {
                    applicationTracker.TrackEvent(Resources.Languages.TrackEventExplorer.EventCategory,
                                                  Resources.Languages.TrackEventExplorer.SharedResourcesServer);
                }
                if (item.ResourceName == "Hello World")
                {
                    applicationTracker.TrackEvent(Resources.Languages.TrackEventWorkflowTabs.EventCategory,
                                                       Resources.Languages.TrackEventWorkflowTabs.HelloWorld);
                }
            }

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

        private void OpenVersionCommand(Guid resourceId, IVersionInfo versionInfo)
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

        public void RunAllTestsCommand(string ResourcePath, Guid resourceId)
        {
            _shellViewModel.RunAllTests(ResourcePath, resourceId);
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

        static void SetActiveStates(IShellViewModel shellViewModel, IServer server)
        {
            shellViewModel.SetActiveServer(server.EnvironmentID);
        }

        public void ShowDependenciesCommand(Guid resourceId, IServer server,bool isSource)
        {
            _shellViewModel.ShowDependencies(resourceId, server, isSource);
        }

        public void DeleteVersionCommand(IExplorerRepository explorerRepository, ExplorerItemViewModel explorerItemViewModel, IExplorerTreeItem parent, string resourceName)
        {
            if (_popupController.ShowDeleteVersionMessage(resourceName) == MessageBoxResult.Yes)
            {
                explorerRepository.TryDelete(explorerItemViewModel);
                var parentChildren = new ObservableCollection<IExplorerItemViewModel>(parent.Children);

                var index = 0;
                for (var i = 0; i < parentChildren.Count; i++)
                {
                    if (parentChildren[i].ResourceName == resourceName)
                    {
                        index = i;
                        break;
                    }
                }

                parentChildren.RemoveAt(index);
                parent.Children = new ObservableCollection<IExplorerItemViewModel>(parentChildren);
                if (parent.ChildrenCount == 0)
                {
                    parent.AreVersionsVisible = true;
                }
                if (parentChildren.Count == 0)
                {
                    parent.AreVersionsVisible = false;
                    parent.IsMergeVisible = false;
                }
                _shellViewModel.UpdateExplorerWorkflowChanges(explorerItemViewModel.ResourceId);
            }
        }

        public void DuplicateResource(IExplorerItemViewModel explorerItemViewModel) => _shellViewModel.DuplicateResource(explorerItemViewModel);

        public void TryDeleteCommand(IExplorerTreeItem parent, IExplorerRepository explorerRepository, ExplorerItemViewModel explorerItemViewModel, IPopupController popupController, IServer server)
        {
            try
            {
                DeleteIfAllowed(parent, explorerRepository, explorerItemViewModel, popupController, server);
            }
            catch (Exception ex)
            {
                explorerItemViewModel.ShowErrorMessage(ex.Message, @"Delete not allowed");
            }
        }

        void DeleteIfAllowed(IExplorerTreeItem parent, IExplorerRepository explorerRepository, ExplorerItemViewModel explorerItemViewModel, IPopupController popupController, IServer server)
        {
            if (explorerItemViewModel.IsResourceVersion)
            {
                DeleteVersionCommand(explorerRepository, explorerItemViewModel, parent, explorerItemViewModel.ResourceName);
            }
            else
            {
                DeleteCommand(parent, explorerRepository, explorerItemViewModel, popupController, server);
            }
        }

        void DeleteCommand(IExplorerTreeItem parent, IExplorerRepository explorerRepository, ExplorerItemViewModel explorerItemViewModel, IPopupController popupController, IServer server)
        {
            var messageBoxResult = popupController.Show(popupController.GetDeleteConfirmation(explorerItemViewModel.ResourceName));
            if (server != null && messageBoxResult == MessageBoxResult.Yes)
            {
                _shellViewModel.CloseResource(explorerItemViewModel.ResourceId, server.EnvironmentID);
                var deletedFileMetadata = explorerRepository.TryDelete(explorerItemViewModel);
                if (deletedFileMetadata.IsDeleted)
                {
                    if (explorerItemViewModel.ResourceType == @"ServerSource" || explorerItemViewModel.IsServer)
                    {
                        server.UpdateRepository.FireServerSaved(explorerItemViewModel.ResourceId, true);
                    }
                    parent?.RemoveChild(explorerItemViewModel);
                }
            }
        }

        public ExplorerItemViewModel CreateChild(string name, Guid id, IServer server, ExplorerItemViewModel explorerItem, Action<IExplorerItemViewModel> selectAction)
        {            
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

        public void MergeCommand(IExplorerItemViewModel explorerItemViewModel, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.OpenMergeDialogView(explorerItemViewModel);
        }

        public void CreateTest(Guid resourceId)
        {
            _shellViewModel.CreateTest(resourceId);
        }
    }
}