#pragma warning disable
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

        public void QueueEventCommand(Guid resourceId)
        {
            _shellViewModel.NewQueueEvent(resourceId);
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
        public void NewWcfSourceCommand(string resourcePath, IServer server)
        {
            SetActiveStates(_shellViewModel, server);
            _shellViewModel.NewWcfSource(resourcePath);
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