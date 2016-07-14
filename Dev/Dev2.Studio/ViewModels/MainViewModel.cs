/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Factory;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Security;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Scheduler;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.ViewModels;
using Dev2.Views.Dialogs;
using Dev2.Workspaces;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Dev2.Studio.Core;
// ReSharper disable CatchAllClause
// ReSharper disable InconsistentNaming
// ReSharper disable NonLocalizedString

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels
{
    public class MainViewModel : BaseConductor<WorkSurfaceContextViewModel>, IMainViewModel,
                                        IHandle<DeleteResourcesMessage>,
                                        IHandle<DeleteFolderMessage>,
                                        IHandle<ShowDependenciesMessage>,
                                        IHandle<AddWorkSurfaceMessage>,
                                        IHandle<RemoveResourceAndCloseTabMessage>,
                                        IHandle<SaveAllOpenTabsMessage>,
                                        IHandle<ShowReverseDependencyVisualizer>,
                                        IHandle<FileChooserMessage>,
                                        IShellViewModel
    {

        private IEnvironmentModel _activeEnvironment;
        private WorkSurfaceContextViewModel _previousActive;
        private bool _disposed;

        private AuthorizeCommand<string> _newServiceCommand;
        private AuthorizeCommand<string> _newPluginSourceCommand;
        private AuthorizeCommand<string> _newDatabaseSourceCommand;
        private AuthorizeCommand<string> _newWebSourceCommand;
        private AuthorizeCommand<string> _newServerSourceCommand;
        private AuthorizeCommand<string> _newEmailSourceCommand;
        private AuthorizeCommand<string> _newExchangeSourceCommand;
        private AuthorizeCommand<string> _newRabbitMQSourceCommand;
        private AuthorizeCommand<string> _newSharepointSourceCommand;
        private AuthorizeCommand<string> _newDropboxSourceCommand;
        private AuthorizeCommand<string> _newWcfSourceCommand;
        private ICommand _deployCommand;
        private ICommand _exitCommand;
        private AuthorizeCommand _settingsCommand;
        private AuthorizeCommand _schedulerCommand;
        private ICommand _showCommunityPageCommand;
        readonly IAsyncWorker _asyncWorker;
        private ICommand _showStartPageCommand;
        bool _canDebug = true;
        bool _menuExpanded;

        public Common.Interfaces.Studio.Controller.IPopupController PopupProvider { get; set; }

        private IEnvironmentRepository EnvironmentRepository { get; }


        public bool CloseCurrent { get; private set; }

        public IExplorerViewModel ExplorerViewModel
        {
            get { return _explorerViewModel; }
            set
            {
                if (_explorerViewModel == value) return;
                _explorerViewModel = value;
                NotifyOfPropertyChange(() => ExplorerViewModel);
            }
        }

        public IServer ActiveServer
        {
            get { return _activeServer; }
            set
            {
                if (value.EnvironmentID != _activeServer.EnvironmentID)
                {
                    _activeServer = value;
                    ExplorerViewModel.ConnectControlViewModel.SelectedConnection = value;
                    NotifyOfPropertyChange(() => ActiveServer);
                }
            }
        }

        public IEnvironmentModel ActiveEnvironment
        {
            get { return _activeEnvironment; }
            set
            {
                if (!Equals(value, _activeEnvironment))
                {
                    _activeEnvironment = value;
                    if (EnvironmentRepository != null)
                    {
                        EnvironmentRepository.ActiveEnvironment = value;
                    }
                    OnActiveEnvironmentChanged();
                    NotifyOfPropertyChange(() => ActiveEnvironment);

                }
            }
        }



        public IBrowserPopupController BrowserPopupController { get; }



        void OnActiveEnvironmentChanged()
        {
            NewDatabaseSourceCommand.UpdateContext(ActiveEnvironment);
            NewServiceCommand.UpdateContext(ActiveEnvironment);
            NewPluginSourceCommand.UpdateContext(ActiveEnvironment);
            NewWebSourceCommand.UpdateContext(ActiveEnvironment);
            NewWcfSourceCommand.UpdateContext(ActiveEnvironment);
            NewServerSourceCommand.UpdateContext(ActiveEnvironment);
            NewSharepointSourceCommand.UpdateContext(ActiveEnvironment);
            NewRabbitMQSourceCommand.UpdateContext(ActiveEnvironment);
            NewDropboxSourceCommand.UpdateContext(ActiveEnvironment);
            NewEmailSourceCommand.UpdateContext(ActiveEnvironment);
            NewExchangeSourceCommand.UpdateContext(ActiveEnvironment);
            SettingsCommand.UpdateContext(ActiveEnvironment);
            SchedulerCommand.UpdateContext(ActiveEnvironment);
            DebugCommand.UpdateContext(ActiveEnvironment);
            SaveCommand.UpdateContext(ActiveEnvironment);
        }

        public AuthorizeCommand SaveCommand
        {
            get
            {
                if (ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                if (ActiveItem.WorkSurfaceKey.WorkSurfaceContext != WorkSurfaceContext.Workflow)
                {
                    var vm = ActiveItem.WorkSurfaceViewModel as IStudioTab;
                    if (vm != null)
                    {
                        return new AuthorizeCommand(AuthorizationContext.Any, o => vm.DoDeactivate(false), o => vm.IsDirty);
                    }
                }
                return ActiveItem.SaveCommand;
            }
        }

        public AuthorizeCommand DebugCommand
        {
            get
            {
                if (ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                return ActiveItem.DebugCommand;
            }
        }

        public AuthorizeCommand QuickDebugCommand
        {
            get
            {
                if (ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                return ActiveItem.QuickDebugCommand;
            }
        }

        public AuthorizeCommand QuickViewInBrowserCommand
        {
            get
            {
                if (ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                return ActiveItem.QuickViewInBrowserCommand;
            }
        }
        public AuthorizeCommand ViewInBrowserCommand
        {
            get
            {
                if (ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                return ActiveItem.ViewInBrowserCommand;
            }
        }

        public ICommand ShowStartPageCommand
        {
            get
            {
                return _showStartPageCommand ?? (_showStartPageCommand = new DelegateCommand(param => ShowStartPage()));
            }
        }

        public ICommand ShowCommunityPageCommand
        {
            get { return _showCommunityPageCommand ?? (_showCommunityPageCommand = new DelegateCommand(param => ShowCommunityPage())); }
        }

        public AuthorizeCommand SettingsCommand
        {
            get
            {
                return _settingsCommand ?? (_settingsCommand =
                    new AuthorizeCommand(AuthorizationContext.Administrator, param => WorksurfaceContextManager.AddSettingsWorkSurface(), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand SchedulerCommand
        {
            get
            {
                return _schedulerCommand ?? (_schedulerCommand =
                    new AuthorizeCommand(AuthorizationContext.Administrator, param => WorksurfaceContextManager.AddSchedulerWorkSurface(), param => IsActiveEnvironmentConnected()));
            }
        }




        public AuthorizeCommand<string> NewServiceCommand
        {
            get
            {
                return _newServiceCommand ?? (_newServiceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewService(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewPluginSourceCommand
        {
            get
            {
                return _newPluginSourceCommand ?? (_newPluginSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewPluginSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewDatabaseSourceCommand
        {
            get
            {
                return _newDatabaseSourceCommand ?? (_newDatabaseSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewDatabaseSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewWebSourceCommand
        {
            get
            {
                return _newWebSourceCommand ?? (_newWebSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewWebSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewServerSourceCommand
        {
            get
            {
                return _newServerSourceCommand ?? (_newServerSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewServerSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewEmailSourceCommand
        {
            get
            {
                return _newEmailSourceCommand ?? (_newEmailSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewEmailSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }


        public AuthorizeCommand<string> NewExchangeSourceCommand
        {
            get
            {
                return _newExchangeSourceCommand ?? (_newExchangeSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewExchangeSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewRabbitMQSourceCommand
        {
            get
            {
                return _newRabbitMQSourceCommand ?? (_newRabbitMQSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewRabbitMQSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewSharepointSourceCommand
        {
            get
            {
                return _newSharepointSourceCommand ?? (_newSharepointSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewSharepointSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewDropboxSourceCommand
        {
            get
            {
                return _newDropboxSourceCommand ?? (_newDropboxSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewDropboxSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewWcfSourceCommand
        {
            get
            {
                return _newWcfSourceCommand ?? (_newWcfSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewWcfSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                return _exitCommand ??
                       (_exitCommand =
                        new RelayCommand(param =>
                                         Application.Current.Shutdown(), param => true));
            }
        }

        public ICommand DeployCommand
        {
            get
            {
                return _deployCommand ??
                       (_deployCommand = new RelayCommand(param => AddDeploySurface(new List<IExplorerTreeItem>())));
            }
        }




        public IVersionChecker Version { get; }

        [ExcludeFromCodeCoverage]
        public MainViewModel()
            : this(EventPublishers.Aggregator, new AsyncWorker(), Core.EnvironmentRepository.Instance, new VersionChecker())
        {            
        }

        public MainViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IEnvironmentRepository environmentRepository,
            IVersionChecker versionChecker, bool createDesigners = true, IBrowserPopupController browserPopupController = null,
            Common.Interfaces.Studio.Controller.IPopupController popupController = null, IExplorerViewModel explorer = null)
            : base(eventPublisher)
        {
            if (environmentRepository == null)
            {
                throw new ArgumentNullException(nameof(environmentRepository));
            }

            if (versionChecker == null)
            {
                throw new ArgumentNullException(nameof(versionChecker));
            }
            Version = versionChecker;
            VerifyArgument.IsNotNull(@"asyncWorker", asyncWorker);
            _asyncWorker = asyncWorker;
            _worksurfaceContextManager = new WorksurfaceContextManager(createDesigners, this);
            BrowserPopupController = browserPopupController ?? new ExternalBrowserPopupController();
            PopupProvider = popupController ?? new PopupController();
            _activeServer = LocalhostServer;

            EnvironmentRepository = environmentRepository;
            SetActiveEnvironment(_activeServer.EnvironmentID);

            MenuPanelWidth = 60;
            _menuExpanded = false;

            ExplorerViewModel = explorer ?? new ExplorerViewModel(this, CustomContainer.Get<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>());
            
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            AddWorkspaceItems();
            ShowStartPage();
            DisplayName = @"Warewolf" + $" ({ClaimsPrincipal.Current.Identity.Name})".ToUpperInvariant();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor

        }
        
        public void Handle(ShowReverseDependencyVisualizer message)
        {
            Dev2Logger.Debug(message.GetType().Name);
            if (message.Model != null)
            {
                WorksurfaceContextManager.AddReverseDependencyVisualizerWorkSurface(message.Model);
            }
        }

        public void Handle(SaveAllOpenTabsMessage message)
        {
            Dev2Logger.Debug(message.GetType().Name);
            PersistTabs();
        }


        public void Handle(AddWorkSurfaceMessage message)
        {
            Dev2Logger.Info(message.GetType().Name);
            WorksurfaceContextManager.AddWorkSurface(message.WorkSurfaceObject);

            if (message.ShowDebugWindowOnLoad)
            {
                if (ActiveItem != null && _canDebug)
                {
                    ActiveItem.DebugCommand.Execute(null);
                }
            }
        }

        public void Handle(DeleteResourcesMessage message)
        {
            Dev2Logger.Info(message.GetType().Name);
            DeleteResources(message.ResourceModels, message.FolderName, message.ShowDialog, message.ActionToDoOnDelete);
        }

        public void Handle(DeleteFolderMessage message)
        {
            Dev2Logger.Info(message.GetType().Name);
            if (ShowDeleteDialogForFolder(message.FolderName))
            {
                message.ActionToDoOnDelete?.Invoke();
            }
        }


        public void SetActiveEnvironment(IEnvironmentModel activeEnvironment)
        {
            ActiveEnvironment = activeEnvironment;
            EnvironmentRepository.ActiveEnvironment = ActiveEnvironment;
            SetActiveEnvironment(activeEnvironment.ID);
            ActiveEnvironment.AuthorizationServiceSet += (sender, args) => OnActiveEnvironmentChanged();
        }

        public void Handle(ShowDependenciesMessage message)
        {
            Dev2Logger.Info(message.GetType().Name);
            var model = message.ResourceModel;
            var dependsOnMe = message.ShowDependentOnMe;
            WorksurfaceContextManager.ShowDependencies(dependsOnMe, model, ActiveServer);
        }

        public void ShowDependencies(Guid resourceId, IServer server)
        {
            var environmentModel = EnvironmentRepository.Get(server.EnvironmentID);
            if (environmentModel != null)
            {
                environmentModel.ResourceRepository.LoadResourceFromWorkspace(resourceId, Guid.Empty);
                var resource = environmentModel.ResourceRepository.FindSingle(model => model.ID == resourceId, true);
                var contextualResourceModel = new ResourceModel(environmentModel, EventPublisher);
                contextualResourceModel.Update(resource);
                contextualResourceModel.ID = resourceId;
                WorksurfaceContextManager.ShowDependencies(true, contextualResourceModel, server);
            }
        }

        public void Handle(RemoveResourceAndCloseTabMessage message)
        {
            _worksurfaceContextManager.Handle(message);
        }

        public IContextualResourceModel DeployResource { get; set; }
        public void RefreshActiveEnvironment()
        {
            if (ActiveItem?.Environment != null)
            {
               SetActiveEnvironment(ActiveItem.Environment);
            }
        }

        public void ShowAboutBox()
        {
            var splashViewModel = new SplashViewModel(ActiveServer, new ExternalProcessExecutor());

            SplashPage splashPage = new SplashPage { DataContext = splashViewModel };
            ISplashView splashView = splashPage;
            splashViewModel.ShowServerVersion();
            // Show it 
            splashView.Show(true);
        }

        // ReSharper disable once InconsistentNaming

        public string OpenPasteWindow(string current)
        {
            var pasteView = new ManageWebservicePasteView();
            return pasteView.ShowView(current);
        }

        public IServer LocalhostServer => CustomContainer.Get<IServer>();

        public void SetActiveEnvironment(Guid environmentId)
        {
            var environmentModel = EnvironmentRepository.Get(environmentId);
            ActiveEnvironment = environmentModel != null && (environmentModel.IsConnected || environmentModel.IsLocalHost) ? environmentModel : EnvironmentRepository.Get(Guid.Empty);
            var server = ExplorerViewModel?.ConnectControlViewModel?.Servers?.FirstOrDefault(a => a.EnvironmentID == environmentId);
            if (server != null)
            {
                SetActiveServer(server);
            }
        }

        public void SetActiveServer(IServer server)
        {
            if (server.IsConnected)
            {
                ActiveServer = server;
            }
        }

        public void Debug()
        {
            ActiveItem.DebugCommand.Execute(null);
        }

        public void OpenResource(Guid resourceId, IServer server)
        {
            OpenResource(resourceId,server.EnvironmentID);
        }
        public void OpenResource(Guid resourceId, Guid environmentId)
        {
            var environmentModel = EnvironmentRepository.Get(environmentId);
            if (environmentModel != null)
            {
                var contextualResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(resourceId);
                WorksurfaceContextManager.DisplayResourceWizard(contextualResourceModel);
            }
        }

        public void CloseResource(Guid resourceId, Guid environmentId)
        {
            var environmentModel = EnvironmentRepository.Get(environmentId);
            var contextualResourceModel = environmentModel?.ResourceRepository.LoadContextualResourceModel(resourceId);
            if (contextualResourceModel != null)
            {
                var wfscvm = WorksurfaceContextManager.FindWorkSurfaceContextViewModel(contextualResourceModel);
                DeactivateItem(wfscvm, true);
            }
        }

        public async void OpenResourceAsync(Guid resourceId, IServer server)
        {
            var environmentModel = EnvironmentRepository.Get(server.EnvironmentID);
            if (environmentModel != null)
            {
                var contextualResourceModel = await environmentModel.ResourceRepository.LoadContextualResourceModelAsync(resourceId);
                var wfscvm = WorksurfaceContextManager.FindWorkSurfaceContextViewModel(contextualResourceModel);
                DeactivateItem(wfscvm, true);
            }
        }

        public void DeployResources(Guid sourceEnvironmentId, Guid destinationEnvironmentId, IList<Guid> resources)
        {
            var environmentModel = EnvironmentRepository.Get(destinationEnvironmentId);
            var sourceEnvironmentModel = EnvironmentRepository.Get(sourceEnvironmentId);
            var dto = new DeployDto { ResourceModels = resources.Select(a => sourceEnvironmentModel.ResourceRepository.LoadContextualResourceModel(a) as IResourceModel).ToList() };
            environmentModel.ResourceRepository.DeployResources(sourceEnvironmentModel, environmentModel, dto);
            ExplorerViewModel.RefreshEnvironment(destinationEnvironmentId);
        }

        public void ShowPopup(IPopupMessage popupMessage)
        {
            PopupProvider.Show(popupMessage.Description, popupMessage.Header, popupMessage.Buttons, MessageBoxImage.Error, @"", false, true, false, false);
        }

        public void EditServer(IServerSource selectedServer)
        {
            _worksurfaceContextManager.EditServer(selectedServer);
        }

        public void EditResource(IDbSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            _worksurfaceContextManager.EditResource(selectedSource, workSurfaceKey);
        }

        public void EditResource(IPluginSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            _worksurfaceContextManager.EditResource(selectedSource, workSurfaceKey);
        }

        public void EditResource(IWebServiceSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            _worksurfaceContextManager.EditResource(selectedSource, workSurfaceKey);
        }

        public void EditResource(IEmailServiceSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            _worksurfaceContextManager.EditResource(selectedSource, workSurfaceKey);
        }

        public void EditResource(IExchangeSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            _worksurfaceContextManager.EditResource(selectedSource, workSurfaceKey);
        }

        public void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            _worksurfaceContextManager.EditResource(selectedSource, workSurfaceKey);
        }

        public void EditResource(IWcfServerSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            _worksurfaceContextManager.EditResource(selectedSource, workSurfaceKey);
        }

        public void NewService(string resourcePath)
        {
            _worksurfaceContextManager.NewService(resourcePath);
        }

        public void NewServerSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = WorksurfaceContextManager.GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.ServerSourceNewHeaderLabel);
            var key = (WorkSurfaceKey)WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ServerSource);
            key.ServerID = ActiveServer.ServerID;
            // ReSharper disable once PossibleInvalidOperationException
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IServerSource>(EventPublisher, new ManageNewServerViewModel(new ManageNewServerSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _asyncWorker, new ExternalProcessExecutor()) { SelectedGuid = key.ResourceID.Value }, PopupProvider, new ManageServerControl()));
            WorksurfaceContextManager.AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewDatabaseSource(string resourcePath)
        {
            _worksurfaceContextManager.NewDatabaseSource(resourcePath);
        }

        public void NewWebSource(string resourcePath)
        {
            _worksurfaceContextManager.NewWebSource(resourcePath);
        }

        public void NewPluginSource(string resourcePath)
        {
            _worksurfaceContextManager.NewPluginSource(resourcePath);
        }

        public void NewWcfSource(string resourcePath)
        {
            _worksurfaceContextManager.NewWcfSource(resourcePath);
        }

        public void NewDropboxSource(string resourcePath)
        {
            _worksurfaceContextManager.NewDropboxSource(resourcePath);
        }

        public void NewRabbitMQSource(string resourcePath)
        {
            _worksurfaceContextManager.NewRabbitMQSource(resourcePath);
        }

        public void NewSharepointSource(string resourcePath)
        {
            _worksurfaceContextManager.NewSharepointSource(resourcePath);
        }

        public void AddDeploySurface(IEnumerable<IExplorerTreeItem> items)
        {
            _worksurfaceContextManager.AddDeploySurface(items);
        }

        public void OpenVersion(Guid resourceId, IVersionInfo versionInfo)
        {
            _worksurfaceContextManager.OpenVersion(resourceId, versionInfo);
        }

        public async void ShowStartPage()
        {
            WorksurfaceContextManager.ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.StartPage);
            WorkSurfaceContextViewModel workSurfaceContextViewModel = Items.FirstOrDefault(c => c.WorkSurfaceViewModel.DisplayName == "Start Page" && c.WorkSurfaceViewModel.GetType() == typeof(HelpViewModel));
            if (workSurfaceContextViewModel != null)
            {
                await ((HelpViewModel)workSurfaceContextViewModel.WorkSurfaceViewModel).LoadBrowserUri(Version.CommunityPageUri);
            }
        }

        public void ShowCommunityPage()
        {
            BrowserPopupController.ShowPopup(StringResources.Uri_Community_HomePage);
        }
      
        public bool IsActiveEnvironmentConnected()
        {
            if (ActiveEnvironment == null)
            {
                return false;
            }
            
            return ActiveEnvironment != null && ActiveEnvironment.IsConnected && ActiveEnvironment.CanStudioExecute;
        }

        public void NewEmailSource(string resourcePath)
        {
            _worksurfaceContextManager.NewEmailSource(resourcePath);
        }

        public void NewExchangeSource(string resourcePath)
        {
            _worksurfaceContextManager.NewExchangeSource(resourcePath);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    OnDeactivate(true);
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }


        protected override void ChangeActiveItem(WorkSurfaceContextViewModel newItem, bool closePrevious)
        {
            if (_previousActive != null)
            {
                if (newItem?.DataListViewModel != null)
                {
                    string errors;
                    newItem.DataListViewModel.ClearCollections();
                    newItem.DataListViewModel.CreateListsOfIDataListItemModelToBindTo(out errors);
                }
            }
            base.ChangeActiveItem(newItem, closePrevious);
            RefreshActiveEnvironment();
        }


        public override void DeactivateItem(WorkSurfaceContextViewModel item, bool close)
        {
            if (item == null)
            {
                return;
            }

            bool success = true;
            if (close)
            {
                success = WorksurfaceContextManager.CloseWorkSurfaceContext(item, null);
            }

            if (success)
            {
                if (_previousActive != item && Items.Contains(_previousActive))
                {
                    ActivateItem(_previousActive);
                }

                base.DeactivateItem(item, close);
                item.Dispose();
                CloseCurrent = true;
            }
            else
            {
                CloseCurrent = false;
            }
        }


        // Process saving tabs and such when exiting ;)
        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                PersistTabs();
            }

            base.OnDeactivate(close);
        }

        protected override void OnActivationProcessed(WorkSurfaceContextViewModel item, bool success)
        {
            if (success)
            {
                var wfItem = item?.WorkSurfaceViewModel as IWorkflowDesignerViewModel;
                if (wfItem != null)
                {
                    WorksurfaceContextManager.AddWorkspaceItem(wfItem.ResourceModel);
                }
                NotifyOfPropertyChange(() => SaveCommand);
                NotifyOfPropertyChange(() => DebugCommand);
                NotifyOfPropertyChange(() => QuickDebugCommand);
                NotifyOfPropertyChange(() => QuickViewInBrowserCommand);
                NotifyOfPropertyChange(() => ViewInBrowserCommand);
                if (MenuViewModel != null)
                {
                    MenuViewModel.SaveCommand = SaveCommand;
                    MenuViewModel.ExecuteServiceCommand = DebugCommand;
                }
            }
            base.OnActivationProcessed(item, success);
        }

        public ICommand SaveAllCommand => new DelegateCommand(SaveAll);

        void SaveAll(object obj)
        {
            for (int index = Items.Count - 1; index >= 0; index--)
            {
                var workSurfaceContextViewModel = Items[index];
                ActivateItem(workSurfaceContextViewModel);
                var workSurfaceContext = workSurfaceContextViewModel.WorkSurfaceKey.WorkSurfaceContext;
                if (workSurfaceContext == WorkSurfaceContext.Workflow)
                {
                    if (workSurfaceContextViewModel.CanSave())
                    {
                        workSurfaceContextViewModel.Save();
                    }
                }
                else
                {
                    var vm = workSurfaceContextViewModel.WorkSurfaceViewModel;
                    var viewModel = vm as IStudioTab;
                    viewModel?.DoDeactivate(true);
                }
            }
        }
        
        public void UpdateCurrentDataListWithObjectFromJson(string parentObjectName,string json)
        {
            ActiveItem?.DataListViewModel?.GenerateComplexObjectFromJson(parentObjectName, json);
        }

        public override void ActivateItem(WorkSurfaceContextViewModel item)
        {
            _previousActive = ActiveItem;
            if (_previousActive?.DebugOutputViewModel != null)
            {
                _previousActive.DebugOutputViewModel.PropertyChanged -= DebugOutputViewModelOnPropertyChanged;
            }
            base.ActivateItem(item);
            if (item?.ContextualResourceModel == null) return;
            if (item.DebugOutputViewModel != null)
            {
                item.DebugOutputViewModel.PropertyChanged += DebugOutputViewModelOnPropertyChanged;
            }
            SetActiveEnvironment(item.Environment);            
        }

        void DebugOutputViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "IsProcessing")
            {
                if (MenuViewModel != null)
                {
                    if (ActiveItem.DebugOutputViewModel != null)
                    {
                        MenuViewModel.IsProcessing = ActiveItem.DebugOutputViewModel.IsProcessing;
                    }
                }
            }
        }

        private bool ConfirmDeleteAfterDependencies(ICollection<IContextualResourceModel> models)
        {
            if (!models.Any(model => model.Environment.ResourceRepository.HasDependencies(model)))
            {
                return true;
            }

            if (models.Count >= 1)
            {
                var model = models.FirstOrDefault();
                if (model != null)
                {
                    var result = PopupProvider.Show(string.Format(StringResources.DialogBody_HasDependencies, model.ResourceName, model.ResourceType.GetDescription()),
                                                    string.Format(StringResources.DialogTitle_HasDependencies, model.ResourceType.GetDescription()),
                                                    MessageBoxButton.OK, MessageBoxImage.Error, "", true, true, false, false);

                    if (result != MessageBoxResult.OK)
                    {
                        WorksurfaceContextManager.ShowDependencies(false, model, ActiveServer);
                    }
                }
                return false;
            }
            return true;
        }

        private bool ConfirmDelete(ICollection<IContextualResourceModel> models, string folderName)
        {
            bool confirmDeleteAfterDependencies = ConfirmDeleteAfterDependencies(models);
            if (confirmDeleteAfterDependencies)
            {
                if (models.Count > 1)
                {
                    var contextualResourceModel = models.FirstOrDefault();
                    if (contextualResourceModel != null)
                    {
                        var folderBeingDeleted = folderName;
                        return ShowDeleteDialogForFolder(folderBeingDeleted);
                    }
                }
                if (models.Count == 1)
                {
                    var contextualResourceModel = models.FirstOrDefault();
                    if (contextualResourceModel != null)
                    {
                        var deletionName = folderName;
                        var description = "";
                        if (string.IsNullOrEmpty(deletionName))
                        {
                            deletionName = contextualResourceModel.ResourceName;
                            description = contextualResourceModel.ResourceType.GetDescription();
                        }

                        var shouldDelete = PopupProvider.Show(string.Format(StringResources.DialogBody_ConfirmDelete, deletionName, description),
                                                              StringResources.DialogTitle_ConfirmDelete,
                                                              MessageBoxButton.YesNo, MessageBoxImage.Information, @"", false, false, true, false) == MessageBoxResult.Yes;

                        return shouldDelete;
                    }
                }
            }
            return false;
        }

        public bool ShowDeleteDialogForFolder(string folderBeingDeleted)
        {
            var popupResult = PopupProvider.Show(string.Format(StringResources.DialogBody_ConfirmFolderDelete, folderBeingDeleted),
                               StringResources.DialogTitle_ConfirmDelete,
                               MessageBoxButton.YesNo, MessageBoxImage.Information, @"", false, false, true, false);

            var confirmDelete = popupResult == MessageBoxResult.Yes;

            return confirmDelete;
        }

        public void DeleteResources(ICollection<IContextualResourceModel> models, string folderName, bool showConfirm = true, System.Action actionToDoOnDelete = null)
        {
            if (models == null || showConfirm && !ConfirmDelete(models, folderName))
            {
                return;
            }

            foreach (var contextualModel in models)
            {
                if (contextualModel == null)
                {
                    continue;
                }

                WorksurfaceContextManager.DeleteContext(contextualModel);

                actionToDoOnDelete?.Invoke();
            }
        }



        public double MenuPanelWidth { get; set; }

        private void SaveWorkspaceItems()
        {
            _getWorkspaceItemRepository().Write();
        }

        readonly Func<IWorkspaceItemRepository> _getWorkspaceItemRepository = () => WorkspaceItemRepository.Instance;

        protected virtual void AddWorkspaceItems()
        {
            if (EnvironmentRepository == null) return;

            HashSet<IWorkspaceItem> workspaceItemsToRemove = new HashSet<IWorkspaceItem>();
            // ReSharper disable ForCanBeConvertedToForeach
            for (int i = 0; i < _getWorkspaceItemRepository().WorkspaceItems.Count; i++)
            // ReSharper restore ForCanBeConvertedToForeach
            {
                //
                // Get the environment for the workspace item
                //
                IWorkspaceItem item = _getWorkspaceItemRepository().WorkspaceItems[i];
                Dev2Logger.Info($"Start Proccessing WorkspaceItem: {item.ServiceName}");
                IEnvironmentModel environment = EnvironmentRepository.All().Where(env => env.IsConnected).TakeWhile(env => env.Connection != null).FirstOrDefault(env => env.ID == item.EnvironmentID);

                if (environment?.ResourceRepository == null)
                {
                    Dev2Logger.Info(@"Environment Not Found");
                    if (environment != null && item.EnvironmentID == environment.ID)
                    {
                        workspaceItemsToRemove.Add(item);
                    }
                }
                if (environment != null)
                {
                    Dev2Logger.Info($"Proccessing WorkspaceItem: {item.ServiceName} for Environment: {environment.DisplayName}");
                    if (environment.ResourceRepository != null)
                    {
                        environment.ResourceRepository.LoadResourceFromWorkspace(item.ID, item.WorkspaceID);
                        var resource = environment.ResourceRepository?.All().FirstOrDefault(rm =>
                        {
                            var sameEnv = true;
                            if (item.EnvironmentID != Guid.Empty)
                            {
                                sameEnv = item.EnvironmentID == environment.ID;
                            }
                            return rm.ID == item.ID && sameEnv;
                        }) as IContextualResourceModel;

                        if (resource == null)
                        {
                            workspaceItemsToRemove.Add(item);
                        }
                        else
                        {
                            Dev2Logger.Info($"Got Resource Model: {resource.DisplayName} ");
                            var fetchResourceDefinition = environment.ResourceRepository.FetchResourceDefinition(environment, item.WorkspaceID, resource.ID, false);
                            resource.WorkflowXaml = fetchResourceDefinition.Message;
                            resource.IsWorkflowSaved = item.IsWorkflowSaved;
                            resource.OnResourceSaved += model => _getWorkspaceItemRepository().UpdateWorkspaceItemIsWorkflowSaved(model);
                            WorksurfaceContextManager.AddWorkSurfaceContextImpl(resource, true);
                        }
                    }
                }
                else
                {
                    workspaceItemsToRemove.Add(item);
                }
            }

            foreach (IWorkspaceItem workspaceItem in workspaceItemsToRemove)
            {
                _getWorkspaceItemRepository().WorkspaceItems.Remove(workspaceItem);
            }
        }

        public bool IsWorkFlowOpened(IContextualResourceModel resource)
        {
            return _worksurfaceContextManager.IsWorkFlowOpened(resource);
        }

        public void UpdateWorkflowLink(IContextualResourceModel resource, string newPath, string oldPath)
        {
            _worksurfaceContextManager.UpdateWorkflowLink(resource, newPath, oldPath);
        }

        public void AddWorkSurfaceContext(IContextualResourceModel resourceModel)
        {
            _worksurfaceContextManager.AddWorkSurfaceContext(resourceModel);
        }

        /// <summary>
        ///     Saves all open tabs locally and writes the open tabs the to collection of workspace items
        /// </summary>
        public void PersistTabs(bool isStudioShutdown = false)
        {
            if (isStudioShutdown)
                SaveAndShutdown(true);
            else
            {
                SaveOnBackgroundTask(false);
            }
        }
        private readonly object _locker = new object();
        void SaveOnBackgroundTask(bool isStudioShutdown)
        {

            SaveWorkspaceItems();
            Task t = new Task(() =>
            {

                lock (_locker)
                {
                    foreach (var ctx in Items.Where(a => true).ToList())
                    {
                        if (!ctx.WorkSurfaceViewModel.DisplayName.ToLower().Contains("version") && ctx.IsEnvironmentConnected())
                        {
                            ctx.Save(true, isStudioShutdown);
                        }
                    }
                }
            });
            t.Start();

        }

        void SaveAndShutdown(bool isStudioShutdown)
        {
            SaveWorkspaceItems();
            foreach (var ctx in Items)
            {
                if (ctx.IsEnvironmentConnected())
                {
                    ctx.Save(true, isStudioShutdown);
                }
            }
        }

        public bool OnStudioClosing()
        {
            List<WorkSurfaceContextViewModel> workSurfaceContextViewModels = Items.ToList();
            foreach (WorkSurfaceContextViewModel workSurfaceContextViewModel in workSurfaceContextViewModels)
            {
                var vm = workSurfaceContextViewModel.WorkSurfaceViewModel;
                if (vm != null)
                {
                    if (vm.WorkSurfaceContext == WorkSurfaceContext.Settings)
                    {
                        var settingsViewModel = vm as SettingsViewModel;
                        if (settingsViewModel != null && settingsViewModel.IsDirty)
                        {
                            ActivateItem(workSurfaceContextViewModel);
                            bool remove = settingsViewModel.DoDeactivate(true);
                            if (!remove)
                            {
                                return false;
                            }
                        }
                    }
                    else if (vm.WorkSurfaceContext == WorkSurfaceContext.Scheduler)
                    {
                        var schedulerViewModel = vm as SchedulerViewModel;
                        if (schedulerViewModel?.SelectedTask != null && schedulerViewModel.SelectedTask.IsDirty)
                        {
                            ActivateItem(workSurfaceContextViewModel);
                            bool remove = schedulerViewModel.DoDeactivate(true);
                            if (!remove)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            CloseRemoteConnections();
            return true;
        }

        private void CloseRemoteConnections()
        {
            var connected = EnvironmentRepository.All().Where(a => a.IsConnected);
            foreach (var environmentModel in connected)
            {
                environmentModel.Disconnect();
            }
        }

        IMenuViewModel _menuViewModel;
        IServer _activeServer;
        private IExplorerViewModel _explorerViewModel;
        private readonly WorksurfaceContextManager _worksurfaceContextManager;

        public bool IsDownloading()
        {
            return false;
        }        

        public async Task<bool> CheckForNewVersion()
        {
            var hasNewVersion = await Version.GetNewerVersionAsync();
            return hasNewVersion;
        }

        public async void DisplayDialogForNewVersion()
        {
            var hasNewVersion = await CheckForNewVersion();
            if (hasNewVersion)
            {
                var dialog = new WebLatestVersionDialog();
                dialog.ShowDialog();
            }
        }


        public bool MenuExpanded
        {
            get
            {
                return _menuExpanded;
            }
            set
            {
                _menuExpanded = value;
                NotifyOfPropertyChange(() => MenuExpanded);
            }
        }
        public IMenuViewModel MenuViewModel => _menuViewModel ?? (_menuViewModel = new MenuViewModel(this));

        public IToolboxViewModel ToolboxViewModel
        {
            get
            {
                var toolboxViewModel = CustomContainer.Get<IToolboxViewModel>();
                return toolboxViewModel;
            }
        }
        public IHelpWindowViewModel HelpViewModel
        {
            get
            {
                var helpViewModel = CustomContainer.Get<IHelpWindowViewModel>();
                return helpViewModel;
            }
        }
        public WorksurfaceContextManager WorksurfaceContextManager
        {
            get
            {
                return _worksurfaceContextManager;
            }
        }
        public WorkSurfaceContextViewModel PreviousActive
        {
            set
            {
                _previousActive = value;
            }
            get
            {
                return _previousActive;
            }
        }
        public IAsyncWorker AsyncWorker => _asyncWorker;
        public bool CanDebug
        {
            set
            {
                _canDebug = value;
            }
            get
            {
                return _canDebug;
            }
        }
        public Func<IWorkspaceItemRepository> GETWorkspaceItemRepository
        {
            get
            {
                return _getWorkspaceItemRepository;
            }
        }

        public void Handle(FileChooserMessage message)
        {
            var emailAttachmentView = new ManageEmailAttachmentView();

            emailAttachmentView.ShowView(message.SelectedFiles.ToList());
            var emailAttachmentVm = emailAttachmentView.DataContext as EmailAttachmentVm;
            if (emailAttachmentVm != null && emailAttachmentVm.Result == MessageBoxResult.OK)
            {
                message.SelectedFiles = emailAttachmentVm.GetAttachments();
            }
        }

    }
}
