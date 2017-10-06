/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Factory;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.Security;
using Dev2.Security;
using Dev2.Services.Events;
using Dev2.Settings;
using Dev2.Settings.Scheduler;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.AppResources.Browsers;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Workspaces;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Threading;
using Dev2.ViewModels;
using Dev2.Workspaces;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Network;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.Views;
using IPopupController = Dev2.Common.Interfaces.Studio.Controller.IPopupController;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using System.IO;

namespace Dev2.Studio.ViewModels
{
    public class ShellViewModel : BaseConductor<WorkSurfaceContextViewModel>,
                                        IHandle<DeleteResourcesMessage>,
                                        IHandle<DeleteFolderMessage>,
                                        IHandle<ShowDependenciesMessage>,
                                        IHandle<AddWorkSurfaceMessage>,
                                        IHandle<RemoveResourceAndCloseTabMessage>,
                                        IHandle<SaveAllOpenTabsMessage>,
                                        IHandle<ShowReverseDependencyVisualizer>,
                                        IHandle<FileChooserMessage>,
                                        IHandle<NewTestFromDebugMessage>,
                                        IShellViewModel
    {

        private WorkSurfaceContextViewModel _previousActive;
        private bool _disposed;

        private AuthorizeCommand<string> _newServiceCommand;
        private AuthorizeCommand<string> _newPluginSourceCommand;
        private AuthorizeCommand<string> _newSqlServerSourceCommand;
        private AuthorizeCommand<string> _newMySqlSourceCommand;
        private AuthorizeCommand<string> _newPostgreSqlSourceCommand;
        private AuthorizeCommand<string> _newOracleSourceCommand;
        private AuthorizeCommand<string> _newOdbcSourceCommand;
        private AuthorizeCommand<string> _newWebSourceCommand;
        private AuthorizeCommand<string> _newServerSourceCommand;
        private AuthorizeCommand<string> _newEmailSourceCommand;
        private AuthorizeCommand<string> _newExchangeSourceCommand;
        private AuthorizeCommand<string> _newRabbitMQSourceCommand;
        private AuthorizeCommand<string> _newSharepointSourceCommand;
        private AuthorizeCommand<string> _newDropboxSourceCommand;
        private AuthorizeCommand<string> _newWcfSourceCommand;
        private ICommand _deployCommand;
        private ICommand _mergeCommand;
        private ICommand _exitCommand;
        private AuthorizeCommand _settingsCommand;
        private AuthorizeCommand _schedulerCommand;
        private ICommand _showCommunityPageCommand;
        readonly IAsyncWorker _asyncWorker;
        private readonly IViewFactory _factory;
        private ICommand _showStartPageCommand;
        bool _canDebug = true;
        bool _menuExpanded;

        public IPopupController PopupProvider { get; set; }

        private IServerRepository ServerRepository { get; }


        public bool CloseCurrent { get; private set; }

        public IExplorerViewModel ExplorerViewModel
        {
            get { return _explorerViewModel; }
            set
            {
                if (_explorerViewModel == value)
                {
                    return;
                }

                _explorerViewModel = value;
                NotifyOfPropertyChange(() => ExplorerViewModel);
            }
        }

        public bool ShouldUpdateActiveState { get; set; }

        public IServer ActiveServer
        {
            get { return _activeServer; }
            set
            {
                if (!Equals(value, _activeServer))
                {
                    _activeServer = value;
                    if (ServerRepository != null)
                    {
                        ServerRepository.ActiveServer = value;
                    }
                    SetEnvironmentIsSelected();
                    OnActiveServerChanged();
                    NotifyOfPropertyChange(() => ActiveServer);
                }
            }
        }

        private void SetEnvironmentIsSelected()
        {
            var environmentViewModels = ExplorerViewModel?.Environments;
            if (environmentViewModels != null)
            {
                foreach (var environment in environmentViewModels)
                {
                    environment.IsSelected = false;
                }
                var environmentViewModel =
                    environmentViewModels.FirstOrDefault(model => model.ResourceId == _activeServer.EnvironmentID);
                if (environmentViewModel != null)
                {
                    environmentViewModel.IsSelected = true;
                }
            }
        }

        internal void LoadWorkflow(string e)
        {
            if (!File.Exists(e)) { return; }
            ActiveServer.ResourceRepository.Load();
            string fileName = string.Empty;
            fileName = Path.GetFileNameWithoutExtension(e);
            var singleResource = ActiveServer.ResourceRepository.FindSingle(p => p.ResourceName == fileName);
            OpenResource(singleResource.ID, ActiveServer.EnvironmentID, ActiveServer);
        }



        public IBrowserPopupController BrowserPopupController { get; }

        public void OnActiveServerChanged()
        {
            NewSqlServerSourceCommand.UpdateContext(ActiveServer);
            NewMySqlSourceCommand.UpdateContext(ActiveServer);
            NewPostgreSqlSourceCommand.UpdateContext(ActiveServer);
            NewOracleSourceCommand.UpdateContext(ActiveServer);
            NewOdbcSourceCommand.UpdateContext(ActiveServer);
            NewServiceCommand.UpdateContext(ActiveServer);
            NewPluginSourceCommand.UpdateContext(ActiveServer);
            NewWebSourceCommand.UpdateContext(ActiveServer);
            NewWcfSourceCommand.UpdateContext(ActiveServer);
            NewServerSourceCommand.UpdateContext(ActiveServer);
            NewSharepointSourceCommand.UpdateContext(ActiveServer);
            NewRabbitMQSourceCommand.UpdateContext(ActiveServer);
            NewDropboxSourceCommand.UpdateContext(ActiveServer);
            NewEmailSourceCommand.UpdateContext(ActiveServer);
            NewExchangeSourceCommand.UpdateContext(ActiveServer);
            SettingsCommand.UpdateContext(ActiveServer);
            SchedulerCommand.UpdateContext(ActiveServer);
            DebugCommand.UpdateContext(ActiveServer);
            SaveCommand.UpdateContext(ActiveServer);
        }

        public IAuthorizeCommand SaveCommand
        {
            get
            {
                if (ActiveItem == null)
                {
                    return new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false);
                }
                if (ActiveItem.WorkSurfaceKey.WorkSurfaceContext != WorkSurfaceContext.Workflow)
                {
                    if (ActiveItem.WorkSurfaceViewModel is IStudioTab vm)
                    {
                        return new AuthorizeCommand(AuthorizationContext.Any, o => vm.DoDeactivate(false), o => vm.IsDirty);
                    }
                }
                return ActiveItem.SaveCommand;
            }
        }

        public IAuthorizeCommand DebugCommand
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

        public IAuthorizeCommand QuickDebugCommand
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

        public IAuthorizeCommand QuickViewInBrowserCommand
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
        public IAuthorizeCommand ViewInBrowserCommand
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

        public IAuthorizeCommand SettingsCommand
        {
            get
            {
                return _settingsCommand ?? (_settingsCommand =
                    new AuthorizeCommand(AuthorizationContext.Administrator, param => _worksurfaceContextManager.AddSettingsWorkSurface(), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand SchedulerCommand
        {
            get
            {
                return _schedulerCommand ?? (_schedulerCommand =
                    new AuthorizeCommand(AuthorizationContext.Administrator, param => _worksurfaceContextManager.AddSchedulerWorkSurface(), param => IsActiveServerConnected()));
            }
        }




        public IAuthorizeCommand<string> NewServiceCommand
        {
            get
            {
                return _newServiceCommand ?? (_newServiceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewService(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewPluginSourceCommand
        {
            get
            {
                return _newPluginSourceCommand ?? (_newPluginSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewPluginSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewSqlServerSourceCommand
        {
            get
            {
                return _newSqlServerSourceCommand ?? (_newSqlServerSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewSqlServerSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewMySqlSourceCommand
        {
            get
            {
                return _newMySqlSourceCommand ?? (_newMySqlSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewMySqlSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewPostgreSqlSourceCommand
        {
            get
            {
                return _newPostgreSqlSourceCommand ?? (_newPostgreSqlSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewPostgreSqlSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewOracleSourceCommand
        {
            get
            {
                return _newOracleSourceCommand ?? (_newOracleSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewOracleSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewOdbcSourceCommand
        {
            get
            {
                return _newOdbcSourceCommand ?? (_newOdbcSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewOdbcSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewWebSourceCommand
        {
            get
            {
                return _newWebSourceCommand ?? (_newWebSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewWebSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewServerSourceCommand
        {
            get
            {
                return _newServerSourceCommand ?? (_newServerSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewServerSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewEmailSourceCommand
        {
            get
            {
                return _newEmailSourceCommand ?? (_newEmailSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewEmailSource(@""), param => IsActiveServerConnected()));
            }
        }


        public IAuthorizeCommand<string> NewExchangeSourceCommand
        {
            get
            {
                return _newExchangeSourceCommand ?? (_newExchangeSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewExchangeSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewRabbitMQSourceCommand
        {
            get
            {
                return _newRabbitMQSourceCommand ?? (_newRabbitMQSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewRabbitMQSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewSharepointSourceCommand
        {
            get
            {
                return _newSharepointSourceCommand ?? (_newSharepointSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewSharepointSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewDropboxSourceCommand
        {
            get
            {
                return _newDropboxSourceCommand ?? (_newDropboxSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewDropboxSource(@""), param => IsActiveServerConnected()));
            }
        }

        public IAuthorizeCommand<string> NewWcfSourceCommand
        {
            get
            {
                return _newWcfSourceCommand ?? (_newWcfSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewWcfSource(@""), param => IsActiveServerConnected()));
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

        public ICommand MergeCommand
        {
            get
            {
                return _mergeCommand ??
                       (_mergeCommand = new RelayCommand(param =>
                       {
                           // OPEN WINDOW TO SELECT RESOURCE TO MERGE WITH

                           var resourceId = Guid.Parse("ea916fa6-76ca-4243-841c-74fa18dd8c14");
                           OpenMergeConflictsView(ActiveItem as IExplorerItemViewModel, resourceId, ActiveServer);
                       }));
            }
        }

        public IVersionChecker Version { get; }

        [ExcludeFromCodeCoverage]
        public ShellViewModel()
            : this(EventPublishers.Aggregator, new AsyncWorker(), CustomContainer.Get<IServerRepository>(), new VersionChecker(), new ViewFactory())
        {
        }

        public ShellViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IServerRepository serverRepository,
            IVersionChecker versionChecker, IViewFactory factory, bool createDesigners = true, IBrowserPopupController browserPopupController = null,
            IPopupController popupController = null, IExplorerViewModel explorer = null)
            : base(eventPublisher)
        {
            Version = versionChecker ?? throw new ArgumentNullException(nameof(versionChecker));
            VerifyArgument.IsNotNull(@"asyncWorker", asyncWorker);
            _asyncWorker = asyncWorker;
            _factory = factory;
            _worksurfaceContextManager = new WorksurfaceContextManager(createDesigners, this);
            BrowserPopupController = browserPopupController ?? new ExternalBrowserPopupController();
            PopupProvider = popupController ?? new PopupController();
            ServerRepository = serverRepository ?? throw new ArgumentNullException(nameof(serverRepository));
            _activeServer = LocalhostServer;
            ServerRepository.ActiveServer = _activeServer;
            ShouldUpdateActiveState = true;
            SetActiveServer(_activeServer.EnvironmentID);

            MenuPanelWidth = 60;
            _menuExpanded = false;

            ExplorerViewModel = explorer ?? new ExplorerViewModel(this, CustomContainer.Get<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>(), true);


            AddWorkspaceItems();
            ShowStartPage();
            DisplayName = @"Warewolf" + $" ({ClaimsPrincipal.Current.Identity.Name})".ToUpperInvariant();


        }

        public void Handle(ShowReverseDependencyVisualizer message)
        {
            Dev2Logger.Debug(message.GetType().Name, "Warewolf Debug");
            if (message.Model != null)
            {
                _worksurfaceContextManager.AddReverseDependencyVisualizerWorkSurface(message.Model);
            }
        }

        public void Handle(SaveAllOpenTabsMessage message)
        {
            Dev2Logger.Debug(message.GetType().Name, "Warewolf Debug");
            PersistTabs();
        }


        public void Handle(AddWorkSurfaceMessage message)
        {
            IsNewWorkflowSaved = true;
            Dev2Logger.Info(message.GetType().Name, "Warewolf Info");
            _worksurfaceContextManager.AddWorkSurface(message.WorkSurfaceObject);
            if (message.ShowDebugWindowOnLoad)
            {
                if (ActiveItem != null && _canDebug)
                {
                    ActiveItem.DebugCommand.Execute(null);
                }
            }
        }

        public bool IsNewWorkflowSaved { get; set; }

        public void Handle(DeleteResourcesMessage message)
        {
            Dev2Logger.Info(message.GetType().Name, "Warewolf Info");
            DeleteResources(message.ResourceModels, message.FolderName, message.ShowDialog, message.ActionToDoOnDelete);
        }

        public void Handle(DeleteFolderMessage message)
        {
            Dev2Logger.Info(message.GetType().Name, "Warewolf Info");
            if (ShowDeleteDialogForFolder(message.FolderName))
            {
                message.ActionToDoOnDelete?.Invoke();
            }
        }



        public void Handle(ShowDependenciesMessage message)
        {
            Dev2Logger.Info(message.GetType().Name, "Warewolf Info");
            var model = message.ResourceModel;
            var dependsOnMe = message.ShowDependentOnMe;
            _worksurfaceContextManager.ShowDependencies(dependsOnMe, model, ActiveServer);
        }

        public void ShowDependencies(Guid resourceId, IServer server, bool isSource)
        {
            var environmentModel = ServerRepository.Get(server.EnvironmentID);
            if (environmentModel != null)
            {
                if (!isSource)
                {
                    environmentModel.ResourceRepository.LoadResourceFromWorkspace(resourceId, Guid.Empty);
                }

                if (server.IsConnected)
                {
                    if (isSource)
                    {
                        var resource = environmentModel.ResourceRepository.LoadContextualResourceModel(resourceId);
                        var contextualResourceModel = new ResourceModel(environmentModel, EventPublisher);
                        contextualResourceModel.Update(resource);
                        contextualResourceModel.ID = resourceId;
                        _worksurfaceContextManager.ShowDependencies(true, contextualResourceModel, server);
                    }
                    else
                    {
                        var resource = environmentModel.ResourceRepository.FindSingle(model => model.ID == resourceId, true);
                        var contextualResourceModel = new ResourceModel(environmentModel, EventPublisher);
                        contextualResourceModel.Update(resource);
                        contextualResourceModel.ID = resourceId;
                        _worksurfaceContextManager.ShowDependencies(true, contextualResourceModel, server);
                    }

                }
            }
        }

        public void Handle(RemoveResourceAndCloseTabMessage message)
        {
            _worksurfaceContextManager.Handle(message);
        }

        public void Handle(NewTestFromDebugMessage message)
        {
            _worksurfaceContextManager.Handle(message);
        }

        public IContextualResourceModel DeployResource { get; set; }
        public void RefreshActiveServer()
        {
            if (ActiveItem?.Environment != null)
            {
                SetActiveServer(ActiveItem.Environment);
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

        public IServer LocalhostServer => ServerRepository.Source;
        public bool ResourceCalled { get; set; }

        public void SetActiveServer(Guid environmentId)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            if (environmentModel != null)
            {
                ActiveServer = environmentModel;
            }
            var server = ExplorerViewModel?.ConnectControlViewModel?.Servers?.FirstOrDefault(a => a.EnvironmentID == environmentId);
            if (server != null)
            {
                SetActiveServer(server);
            }
        }

        public void SetActiveServer(IServer server)
        {
            ActiveServer = server;
            if (!ActiveServer.IsConnected)
            {
                ActiveServer.Connect();
            }
        }

        public void Debug()
        {
            ActiveItem.DebugCommand.Execute(null);
        }

        public void StudioDebug(Guid resourceId, IServer server)
        {
            DebugStudio(resourceId, server.EnvironmentID);
        }
        public void DebugStudio(Guid resourceId, Guid environmentId)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            var contextualResourceModel = environmentModel?.ResourceRepository.LoadContextualResourceModel(resourceId);
            if (contextualResourceModel != null)
            {
                _worksurfaceContextManager.DisplayResourceWizard(contextualResourceModel);
                QuickDebugCommand.Execute(contextualResourceModel);
            }
        }

        public void NewSchedule(Guid resourceId)
        {
            CreateNewSchedule(resourceId);
        }

        public void BrowserDebug(Guid resourceId, IServer server)
        {
            OpenBrowser(resourceId, server.EnvironmentID);
        }
        public void OpenBrowser(Guid resourceId, Guid environmentId)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            var contextualResourceModel = environmentModel?.ResourceRepository.LoadContextualResourceModel(resourceId);
            if (contextualResourceModel != null)
            {
                _worksurfaceContextManager.DisplayResourceWizard(contextualResourceModel);
                QuickViewInBrowserCommand.Execute(contextualResourceModel);
            }
        }

        public void SetRefreshExplorerState(bool refresh)
        {
            ExplorerViewModel.IsRefreshing = refresh;
        }

        public void OpenMergeDialogView(IExplorerItemViewModel currentResource)
        {
            var mergeServiceViewModel = new MergeServiceViewModel(this, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), currentResource, new MergeSelectionView());
            var result = mergeServiceViewModel.ShowMergeDialog();
            if (result == MessageBoxResult.OK)
            {
                var differentResource = mergeServiceViewModel.SelectedMergeItem;

                OpenMergeConflictsView(currentResource, differentResource.ResourceId, differentResource.Server);
            }
        }


        public void OpenMergeConflictsView(IExplorerItemViewModel currentResource, Guid differenceResourceId, IServer server)
        {

            var localHost = ((ExplorerItemViewModel)currentResource).Server;
            if (localHost != null)
            {

                var currentResourceModel = localHost.ResourceRepository.LoadContextualResourceModel(currentResource.ResourceId);
                //var currentResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(currentResourceId);
                var differenceResourceModel = server.ResourceRepository.LoadContextualResourceModel(differenceResourceId);

                var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.MergeConflicts);
                if (currentResourceModel != null && differenceResourceModel != null)
                {
                    workSurfaceKey.EnvironmentID = currentResourceModel.Environment.EnvironmentID;
                    workSurfaceKey.ResourceID = currentResourceModel.ID;
                    workSurfaceKey.ServerID = currentResourceModel.ServerID;

                    _worksurfaceContextManager.ViewMergeConflictsService(currentResourceModel, differenceResourceModel, true, workSurfaceKey);
                }
            }
        }

        public void OpenMergeConflictsView(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadFromServer)
        {
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.MergeConflicts);
            if (currentResourceModel != null && differenceResourceModel != null)
            {
                workSurfaceKey.EnvironmentID = currentResourceModel.Environment.EnvironmentID;
                workSurfaceKey.ResourceID = currentResourceModel.ID;
                workSurfaceKey.ServerID = currentResourceModel.ServerID;

                _worksurfaceContextManager.ViewMergeConflictsService(currentResourceModel, differenceResourceModel, loadFromServer, workSurfaceKey);
            }
        }


        public void OpenCurrentVersion(Guid resourceId, Guid environmentId)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            var contextualResourceModel = environmentModel?.ResourceRepository.LoadContextualResourceModel(resourceId);

            if (contextualResourceModel != null)
            {
                _worksurfaceContextManager.AddWorkSurfaceContext(contextualResourceModel);
            }
        }

        public void OpenResource(Guid resourceId, Guid environmentId, IServer activeServer)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            environmentModel?.ResourceRepository?.UpdateServer(activeServer);
            var contextualResourceModel = environmentModel?.ResourceRepository.LoadContextualResourceModel(resourceId);

            if (contextualResourceModel != null)
            {
                var workSurfaceKey = new WorkSurfaceKey { EnvironmentID = environmentId, ResourceID = resourceId, ServerID = contextualResourceModel.ServerID };
                switch (contextualResourceModel.ServerResourceType)
                {
                    case "SqlDatabase":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.SqlServerSource;
                        ProcessDBSource(ProcessSQLDBSource(CreateDbSource(contextualResourceModel, WorkSurfaceContext.SqlServerSource)), workSurfaceKey);
                        break;
                    case "ODBC":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.OdbcSource;
                        ProcessDBSource(ProcessODBCDBSource(CreateDbSource(contextualResourceModel, WorkSurfaceContext.OdbcSource)), workSurfaceKey);
                        break;
                    case "Oracle":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.OracleSource;
                        ProcessDBSource(ProcessOracleDBSource(CreateDbSource(contextualResourceModel, WorkSurfaceContext.OracleSource)), workSurfaceKey);
                        break;
                    case "PostgreSQL":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.PostgreSqlSource;
                        ProcessDBSource(ProcessPostgreSQLDBSource(CreateDbSource(contextualResourceModel, WorkSurfaceContext.PostgreSqlSource)), workSurfaceKey);
                        break;
                    case "MySqlDatabase":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.MySqlSource;
                        ProcessDBSource(ProcessMySQLDBSource(CreateDbSource(contextualResourceModel, WorkSurfaceContext.MySqlSource)), workSurfaceKey);
                        break;
                    case "EmailSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.EmailSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessEmailSource(contextualResourceModel, workSurfaceKey));
                        break;
                    case "WebSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.EmailSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessWebSource(contextualResourceModel, workSurfaceKey));
                        break;
                    case "ComPluginSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.ComPluginSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessComPluginSource(contextualResourceModel, workSurfaceKey));
                        break;
                    case "ExchangeSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.Exchange;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessExchangeSource(contextualResourceModel, workSurfaceKey));
                        break;
                    case "OauthSource":
                    case "DropBoxSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.OAuthSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessDropBoxSource(contextualResourceModel, workSurfaceKey));
                        break;
                    case "Server":
                    case "Dev2Server":
                    case "ServerSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.ServerSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessServerSource(contextualResourceModel, workSurfaceKey, environmentModel, activeServer));
                        break;
                    case "SharepointServerSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.SharepointServerSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessSharepointServerSource(contextualResourceModel, workSurfaceKey));
                        break;
                    case "RabbitMQSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.RabbitMQSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessRabbitMQSource(contextualResourceModel, workSurfaceKey));
                        break;
                    case "WcfSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.WcfSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessWcfSource(contextualResourceModel, workSurfaceKey));
                        break;
                    case "PluginSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.PluginSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessPluginSource(contextualResourceModel, workSurfaceKey));
                        break;
                    default:
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.Workflow;
                        _worksurfaceContextManager.DisplayResourceWizard(contextualResourceModel);
                        break;
                }
            }
        }

        private WorkSurfaceContextViewModel ProcessPluginSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {

            var def = new PluginSourceDefinition
            {
                Id = contextualResourceModel.ID,
                Path = contextualResourceModel.GetSavePath()
            };

            var pluginSourceViewModel = new ManagePluginSourceViewModel(
                new ManagePluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def,
                AsyncWorker);
            var vm = new SourceViewModel<IPluginSource>(EventPublisher, pluginSourceViewModel, PopupProvider, new ManagePluginSourceControl(), ActiveServer);

            var key = workSurfaceKey;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            return workSurfaceContextViewModel;
        }

        private WorkSurfaceContextViewModel ProcessWcfSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {

            var def = new WcfServiceSourceDefinition
            {
                Id = contextualResourceModel.ID,
                Path = contextualResourceModel.GetSavePath()
            };

            var wcfSourceViewModel = new ManageWcfSourceViewModel(
                new ManageWcfSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def,
                AsyncWorker,
                ActiveServer);
            var vm = new SourceViewModel<IWcfServerSource>(EventPublisher, wcfSourceViewModel, PopupProvider, new ManageWcfSourceControl(), ActiveServer);

            var key = workSurfaceKey;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            return workSurfaceContextViewModel;
        }

        private WorkSurfaceContextViewModel ProcessRabbitMQSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new RabbitMQServiceSourceDefinition
            {
                ResourceID = contextualResourceModel.ID,
                ResourcePath = contextualResourceModel.GetSavePath()
            };

            var viewModel = new ManageRabbitMQSourceViewModel(
                new ManageRabbitMQSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, this),
                def,
                AsyncWorker);
            var vm = new SourceViewModel<IRabbitMQServiceSourceDefinition>(EventPublisher, viewModel, PopupProvider, new ManageRabbitMQSourceControl(), ActiveServer);

            var key = workSurfaceKey;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            return workSurfaceContextViewModel;
        }

        private WorkSurfaceContextViewModel ProcessSharepointServerSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new SharePointServiceSourceDefinition
            {
                Id = contextualResourceModel.ID,
                Path = contextualResourceModel.GetSavePath()
            };

            var viewModel = new SharepointServerSourceViewModel(
                new SharepointServerSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def,
                AsyncWorker,
                null);
            var vm = new SourceViewModel<ISharepointServerSource>(EventPublisher, viewModel, PopupProvider, new SharepointServerSource(), ActiveServer);

            var key = workSurfaceKey;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            return workSurfaceContextViewModel;
        }

        private WorkSurfaceContextViewModel ProcessServerSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey, IServer server, IServer activeServer)
        {

            var selectedServer = new ServerSource
            {
                ID = contextualResourceModel.ID,
                ResourcePath = contextualResourceModel.GetSavePath()
            };

            var viewModel = new ManageNewServerViewModel(
                new ManageNewServerSourceModel(activeServer.UpdateRepository, activeServer.QueryProxy, server.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                selectedServer,
                AsyncWorker,
                new ExternalProcessExecutor());
            var vm = new SourceViewModel<IServerSource>(EventPublisher, viewModel, PopupProvider, new ManageServerControl(), server);

            var key = workSurfaceKey;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            return workSurfaceContextViewModel;
        }

        private WorkSurfaceContextViewModel ProcessDropBoxSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var db = new DropBoxSource
            {
                ResourceID = contextualResourceModel.ID,
                ResourcePath = contextualResourceModel.GetSavePath()
            };

            var oauthSourceViewModel = new ManageOAuthSourceViewModel(
                new ManageOAuthSourceModel(ActiveServer.UpdateRepository,
                ActiveServer.QueryProxy, ""),
                db,
                AsyncWorker);
            var vm = new SourceViewModel<IOAuthSource>(EventPublisher, oauthSourceViewModel, PopupProvider, new ManageOAuthSourceControl(), ActiveServer);

            var key = workSurfaceKey;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            return workSurfaceContextViewModel;
        }

        private WorkSurfaceContextViewModel ProcessExchangeSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new ExchangeSourceDefinition
            {
                ResourceID = contextualResourceModel.ID,
                Path = contextualResourceModel.GetSavePath()
            };

            var emailSourceViewModel = new ManageExchangeSourceViewModel(
                new ManageExchangeSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def,
                AsyncWorker);
            var vm = new SourceViewModel<IExchangeSource>(EventPublisher, emailSourceViewModel, PopupProvider, new ManageExchangeSourceControl(), ActiveServer);

            var key = workSurfaceKey;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            return workSurfaceContextViewModel;
        }

        private WorkSurfaceContextViewModel ProcessComPluginSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new ComPluginSourceDefinition
            {
                Id = contextualResourceModel.ID,
                ResourcePath = contextualResourceModel.GetSavePath()
            };

            var wcfSourceViewModel = new ManageComPluginSourceViewModel(
                new ManageComPluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def,
                AsyncWorker);
            var vm = new SourceViewModel<IComPluginSource>(EventPublisher, wcfSourceViewModel, PopupProvider, new ManageComPluginSourceControl(), ActiveServer);

            var key = workSurfaceKey;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            return workSurfaceContextViewModel;
        }

        private WorkSurfaceContextViewModel ProcessWebSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new WebServiceSourceDefinition
            {
                Id = contextualResourceModel.ID,
                Path = contextualResourceModel.GetSavePath()
            };
            var viewModel = new ManageWebserviceSourceViewModel(
                new ManageWebServiceSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def,
                AsyncWorker,
                new ExternalProcessExecutor());
            var vm = new SourceViewModel<IWebServiceSource>(EventPublisher, viewModel, PopupProvider, new ManageWebserviceSourceControl(), ActiveServer);

            var key = workSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            return workSurfaceContextViewModel;
        }

        private WorkSurfaceContextViewModel ProcessEmailSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new EmailServiceSourceDefinition
            {
                Id = contextualResourceModel.ID,
                Path = contextualResourceModel.GetSavePath()
            };

            var emailSourceViewModel = new ManageEmailSourceViewModel(
                new ManageEmailSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def, AsyncWorker);
            var vm = new SourceViewModel<IEmailServiceSource>(EventPublisher, emailSourceViewModel, PopupProvider, new ManageEmailSourceControl(), ActiveServer);
            var key = workSurfaceKey;
            var workSurfaceContextViewModel = _worksurfaceContextManager.EditResource(key, vm);
            return workSurfaceContextViewModel;
        }

        private ManageMySqlSourceViewModel ProcessMySQLDBSource(IDbSource def)
        {
            return new ManageMySqlSourceViewModel(
                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName)
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , def
                , AsyncWorker);
        }

        private ManagePostgreSqlSourceViewModel ProcessPostgreSQLDBSource(IDbSource def)
        {
            return new ManagePostgreSqlSourceViewModel(
                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName)
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , def
                , AsyncWorker);
        }

        private ManageOracleSourceViewModel ProcessOracleDBSource(IDbSource def)
        {
            return new ManageOracleSourceViewModel(
                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName)
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , def
                , AsyncWorker);
        }

        private ManageOdbcSourceViewModel ProcessODBCDBSource(IDbSource def)
        {
            return new ManageOdbcSourceViewModel(
                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository
                , ActiveServer.QueryProxy, ActiveServer.DisplayName)
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , def
                , AsyncWorker);
        }

        private ManageSqlServerSourceViewModel ProcessSQLDBSource(IDbSource def)
        {
            return new ManageSqlServerSourceViewModel(
                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName)
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , def
                , AsyncWorker);
        }

        private IDbSource CreateDbSource(IContextualResourceModel contextualResourceModel, WorkSurfaceContext workSurfaceContext)
        {
            var def = new DbSourceDefinition { Id = contextualResourceModel.ID, Path = contextualResourceModel.GetSavePath(), Type = ToenSourceType(workSurfaceContext) };
            return def;
        }

        private void ProcessDBSource(DatabaseSourceViewModelBase dbSourceViewModel, WorkSurfaceKey workSurfaceKey)
        {
            var vm = new SourceViewModel<IDbSource>(EventPublisher, dbSourceViewModel, PopupProvider, new ManageDatabaseSourceControl(), ActiveServer);
            var key = workSurfaceKey;
            if (key != null)
            {
                key.EnvironmentID = ActiveServer.EnvironmentID;
            }
            var workSurfaceContextViewModel = _worksurfaceContextManager.EditResource(key, vm);
            _worksurfaceContextManager.DisplayResourceWizard(workSurfaceContextViewModel);

        }

        private enSourceType ToenSourceType(WorkSurfaceContext sqlServerSource)
        {
            switch (sqlServerSource)
            {
                case WorkSurfaceContext.SqlServerSource:
                    return enSourceType.SqlDatabase;
                case WorkSurfaceContext.MySqlSource:
                    return enSourceType.MySqlDatabase;
                case WorkSurfaceContext.PostgreSqlSource:
                    return enSourceType.PostgreSQL;
                case WorkSurfaceContext.OracleSource:
                    return enSourceType.Oracle;
                case WorkSurfaceContext.OdbcSource:
                    return enSourceType.ODBC;
                default:
                    return enSourceType.Unknown;
            }
        }

        public void CopyUrlLink(Guid resourceId, IServer server)
        {
            GetCopyUrlLink(resourceId, server.EnvironmentID);
        }

        private void GetCopyUrlLink(Guid resourceId, Guid environmentId)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            if (environmentModel != null)
            {
                var contextualResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(resourceId);

                var workflowUri = WebServer.GetWorkflowUri(contextualResourceModel, "", UrlType.Json, false);
                if (workflowUri != null)
                {
                    Clipboard.SetText(workflowUri.ToString());
                }
            }
        }

        public void ViewSwagger(Guid resourceId, IServer server)
        {
            ViewSwagger(resourceId, server.EnvironmentID);
        }

        private void ViewSwagger(Guid resourceId, Guid environmentId)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            if (environmentModel != null)
            {
                var contextualResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(resourceId);

                var workflowUri = WebServer.GetWorkflowUri(contextualResourceModel, "", UrlType.API);
                if (workflowUri != null)
                {
                    BrowserPopupController.ShowPopup(workflowUri.ToString());
                }
            }
        }

        public void ViewApisJson(string resourcePath, Uri webServerUri)
        {
            var relativeUrl = "";

            if (!string.IsNullOrWhiteSpace(resourcePath))
            {
                relativeUrl = "/secure/" + resourcePath + "/apis.json";
            }
            else
            {
                relativeUrl += "/secure/apis.json";
            }

            Uri.TryCreate(webServerUri, relativeUrl, out Uri url);

            BrowserPopupController.ShowPopup(url.ToString());
        }

        public void CreateNewSchedule(Guid resourceId)
        {
            var environmentModel = ServerRepository.Get(ActiveServer.EnvironmentID);
            if (environmentModel != null)
            {
                var contextualResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(resourceId);
                _worksurfaceContextManager.CreateNewScheduleWorkSurface(contextualResourceModel);
            }
        }

        public void CreateTest(Guid resourceId)
        {
            var environmentModel = ServerRepository.Get(ActiveServer.EnvironmentID);
            if (environmentModel != null)
            {
                var contextualResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(resourceId);

                var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ServiceTestsViewer);
                if (contextualResourceModel != null)
                {
                    workSurfaceKey.EnvironmentID = contextualResourceModel.Environment.EnvironmentID;
                    workSurfaceKey.ResourceID = contextualResourceModel.ID;
                    workSurfaceKey.ServerID = contextualResourceModel.ServerID;

                    _worksurfaceContextManager.ViewTestsForService(contextualResourceModel, workSurfaceKey);
                }
            }
        }

        public void RunAllTests(Guid resourceId)
        {
            var environmentModel = ServerRepository.Get(ActiveServer.EnvironmentID);
            var contextualResourceModel = environmentModel?.ResourceRepository.LoadContextualResourceModel(resourceId);

            if (contextualResourceModel != null)
            {
                _worksurfaceContextManager.RunAllTestsForService(contextualResourceModel);
            }
        }

        public void CloseResourceTestView(Guid resourceId, Guid serverId, Guid environmentId)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ServiceTestsViewer, resourceId, serverId, environmentId);
            var testViewModelForResource = FindWorkSurfaceContextViewModel(key);
            if (testViewModelForResource != null)
            {
                DeactivateItem(testViewModelForResource, true);
            }

        }

        private WorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(WorkSurfaceKey key)
        {
            return Items.FirstOrDefault(c => WorkSurfaceKeyEqualityComparerWithContextKey.Current.Equals(key, c.WorkSurfaceKey));
        }

        public void CloseResource(Guid resourceId, Guid environmentId)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            var contextualResourceModel = environmentModel?.ResourceRepository.LoadContextualResourceModel(resourceId);
            if (contextualResourceModel != null)
            {
                var wfscvm = _worksurfaceContextManager.FindWorkSurfaceContextViewModel(contextualResourceModel);
                DeactivateItem(wfscvm, true);
            }
        }

        public async void OpenResourceAsync(Guid resourceId, IServer server)
        {
            var environmentModel = ServerRepository.Get(server.EnvironmentID);
            if (environmentModel != null && environmentModel.ResourceRepository != null)
            {
                var contextualResourceModel = await environmentModel.ResourceRepository.LoadContextualResourceModelAsync(resourceId);
                _worksurfaceContextManager.DisplayResourceWizard(contextualResourceModel);
            }
        }

        public void DeployResources(Guid sourceEnvironmentId, Guid destinationEnvironmentId, IList<Guid> resources, bool deployTests)
        {
            var environmentModel = ServerRepository.Get(destinationEnvironmentId);
            var sourceEnvironmentModel = ServerRepository.Get(sourceEnvironmentId);
            var dto = new DeployDto { ResourceModels = resources.Select(a => sourceEnvironmentModel.ResourceRepository.LoadContextualResourceModel(a) as IResourceModel).ToList(), DeployTests = deployTests };
            environmentModel.ResourceRepository.DeployResources(sourceEnvironmentModel, environmentModel, dto);
            ServerAuthorizationService.Instance.GetResourcePermissions(dto.ResourceModels.First().ID);
            ExplorerViewModel.RefreshEnvironment(destinationEnvironmentId);

        }

        public void ShowPopup(IPopupMessage popupMessage)
        {
            PopupProvider.Show(popupMessage.Description, popupMessage.Header, popupMessage.Buttons, MessageBoxImage.Error, @"", false, true, false, false, false, false);
        }

        public void EditSqlServerResource(IDbSource selectedSourceDefinition, IWorkSurfaceKey workSurfaceKey = null)
        {
            workSurfaceKey = _worksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.SqlServerSource, selectedSourceDefinition.Id);
            ProcessDBSource(ProcessSQLDBSource(selectedSourceDefinition), workSurfaceKey as WorkSurfaceKey);
        }

        public void EditMySqlResource(IDbSource selectedSourceDefinition, IWorkSurfaceKey workSurfaceKey = null)
        {
            workSurfaceKey = _worksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.MySqlSource, selectedSourceDefinition.Id);
            ProcessDBSource(ProcessMySQLDBSource(selectedSourceDefinition), workSurfaceKey as WorkSurfaceKey);
        }

        public void EditPostgreSqlResource(IDbSource selectedSourceDefinition, IWorkSurfaceKey workSurfaceKey = null)
        {
            workSurfaceKey = _worksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.PostgreSqlSource, selectedSourceDefinition.Id);
            ProcessDBSource(ProcessPostgreSQLDBSource(selectedSourceDefinition), workSurfaceKey as WorkSurfaceKey);
        }

        public void EditOracleResource(IDbSource selectedSourceDefinition, IWorkSurfaceKey workSurfaceKey = null)
        {
            workSurfaceKey = _worksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.OracleSource, selectedSourceDefinition.Id);
            ProcessDBSource(ProcessOracleDBSource(selectedSourceDefinition), workSurfaceKey as WorkSurfaceKey);
        }

        public void EditOdbcResource(IDbSource selectedSourceDefinition, IWorkSurfaceKey workSurfaceKey = null)
        {
            workSurfaceKey = _worksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.OdbcSource, selectedSourceDefinition.Id);
            ProcessDBSource(ProcessODBCDBSource(selectedSourceDefinition), workSurfaceKey as WorkSurfaceKey);
        }

        public void EditResource(IPluginSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var view = _factory.GetViewGivenServerResourceType("PluginSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, workSurfaceKey);
        }

        public void EditResource(IWebServiceSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var view = _factory.GetViewGivenServerResourceType("WebSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, workSurfaceKey);
        }

        public void EditResource(IEmailServiceSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var view = _factory.GetViewGivenServerResourceType("EmailSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, workSurfaceKey);
        }

        public void EditResource(IExchangeSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var view = _factory.GetViewGivenServerResourceType("ExchangeSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, workSurfaceKey);
        }

        public void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var view = _factory.GetViewGivenServerResourceType("RabbitMQSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, workSurfaceKey);
        }

        public void EditResource(IWcfServerSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var view = _factory.GetViewGivenServerResourceType("WcfSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, workSurfaceKey);
        }

        public void EditResource(IComPluginSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var view = _factory.GetViewGivenServerResourceType("ComPluginSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, workSurfaceKey);
        }

        public void NewService(string resourcePath)
        {
            _worksurfaceContextManager.NewService(resourcePath);
        }

        public void NewServerSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = _worksurfaceContextManager.GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.ServerSourceNewHeaderLabel);
            var key = (WorkSurfaceKey)WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ServerSource);
            key.ServerID = ActiveServer.ServerID;

            var manageNewServerSourceModel = new ManageNewServerSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name);
            var manageNewServerViewModel = new ManageNewServerViewModel(manageNewServerSourceModel, saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _asyncWorker, new ExternalProcessExecutor()) { SelectedGuid = key.ResourceID.Value };
            var workSurfaceViewModel = new SourceViewModel<IServerSource>(EventPublisher, manageNewServerViewModel, PopupProvider, new ManageServerControl(), ActiveServer);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, workSurfaceViewModel);
            _worksurfaceContextManager.AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewSqlServerSource(string resourcePath)
        {
            _worksurfaceContextManager.NewSqlServerSource(resourcePath);
        }

        public void NewMySqlSource(string resourcePath)
        {
            _worksurfaceContextManager.NewMySqlSource(resourcePath);
        }

        public void NewPostgreSqlSource(string resourcePath)
        {
            _worksurfaceContextManager.NewPostgreSqlSource(resourcePath);
        }

        public void NewOracleSource(string resourcePath)
        {
            _worksurfaceContextManager.NewOracleSource(resourcePath);
        }

        public void NewOdbcSource(string resourcePath)
        {
            _worksurfaceContextManager.NewOdbcSource(resourcePath);
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

        public void NewComPluginSource(string resourcePath)
        {
            _worksurfaceContextManager.NewComPluginSource(resourcePath);
        }

        private void ShowServerDisconnectedPopup()
        {
            var controller = CustomContainer.Get<IPopupController>();
            controller?.Show(string.Format(Warewolf.Studio.Resources.Languages.Core.ServerDisconnected, ActiveServer.DisplayName.Replace("(Connected)", "")) + Environment.NewLine +
                             Warewolf.Studio.Resources.Languages.Core.ServerReconnectForActions, Warewolf.Studio.Resources.Languages.Core.ServerDisconnectedHeader, MessageBoxButton.OK,
                MessageBoxImage.Error, "", false, true, false, false, false, false);
        }

        public void DuplicateResource(IExplorerItemViewModel explorerItemViewModel)
        {
            if (!ActiveServer.IsConnected)
            {
                ShowServerDisconnectedPopup();
            }
            else
            {
                _worksurfaceContextManager.DuplicateResource(explorerItemViewModel);
            }
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
            WorkSurfaceContextViewModel workSurfaceContextViewModel = Items.FirstOrDefault(c => c.WorkSurfaceViewModel.DisplayName == "Start Page" && c.WorkSurfaceViewModel.GetType() == typeof(HelpViewModel));
            if (workSurfaceContextViewModel == null)
            {
                var helpViewModel = _worksurfaceContextManager.ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.StartPage);
                if (helpViewModel != null)
                {
                    await helpViewModel.LoadBrowserUri(Version.CommunityPageUri);
                }
            }
            else
            {
                ActivateItem(workSurfaceContextViewModel);
            }
        }

        public void ShowCommunityPage()
        {
            BrowserPopupController.ShowPopup(StringResources.Uri_Community_HomePage);
        }

        public bool IsActiveServerConnected()
        {
            if (ActiveServer == null)

            {
                return false;
            }

            var isActiveServerConnected = ActiveServer != null && ActiveServer.IsConnected && ActiveServer.CanStudioExecute && ShouldUpdateActiveState;
            if (ActiveServer.IsConnected && ShouldUpdateActiveState)
            {
                if (ToolboxViewModel?.BackedUpTools != null && ToolboxViewModel.BackedUpTools.Count == 0)
                {
                    ToolboxViewModel.BuildToolsList();
                }
            }
            if (ToolboxViewModel != null)
            {
                ToolboxViewModel.IsVisible = isActiveServerConnected;
            }

            return isActiveServerConnected;
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
            base.ChangeActiveItem(newItem, closePrevious);
            RefreshActiveServer();
        }

        public void BaseDeactivateItem(WorkSurfaceContextViewModel item, bool close)
        {
            base.DeactivateItem(item, close);
        }
        public bool DontPrompt { get; set; }
        public override void DeactivateItem(WorkSurfaceContextViewModel item, bool close)
        {
            if (item == null)
            {
                return;
            }

            bool success = true;
            if (close)
            {
                success = _worksurfaceContextManager.CloseWorkSurfaceContext(item, null, DontPrompt);
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
                if (item?.WorkSurfaceViewModel is IWorkflowDesignerViewModel wfItem)
                {
                    _worksurfaceContextManager.AddWorkspaceItem(wfItem.ResourceModel);
                }
                if (item?.WorkSurfaceViewModel is StudioTestViewModel studioTestViewModel)
                {
                    var serviceTestViewModel = studioTestViewModel.ViewModel as ServiceTestViewModel;
                    EventPublisher.Publish(serviceTestViewModel?.SelectedServiceTest != null
                        ? new DebugOutputMessage(serviceTestViewModel.SelectedServiceTest?.DebugForTest ?? new List<IDebugState>())
                        : new DebugOutputMessage(new List<IDebugState>()));

                    if (serviceTestViewModel != null)
                    {
                        serviceTestViewModel.WorkflowDesignerViewModel.IsTestView = true;
                    }
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
            ContinueShutDown = true;
            for (int index = Items.Count - 1; index >= 0; index--)
            {
                var workSurfaceContextViewModel = Items[index];

                var workSurfaceContext = workSurfaceContextViewModel.WorkSurfaceKey.WorkSurfaceContext;
                if (workSurfaceContext == WorkSurfaceContext.Help)
                {
                    continue;
                }
                DeactivateItem(workSurfaceContextViewModel, true);
                if (!CloseCurrent)
                {
                    ContinueShutDown = false;
                    break;
                }
            }
        }

        public bool ContinueShutDown;

        public void ResetMainView()
        {
            ShellView shellView = ShellView.GetInstance();
            shellView.ResetToStartupView();
        }

        public void UpdateCurrentDataListWithObjectFromJson(string parentObjectName, string json)
        {
            ActiveItem?.DataListViewModel?.GenerateComplexObjectFromJson(parentObjectName, json);
        }

        public override void ActivateItem(WorkSurfaceContextViewModel item)
        {
            _previousActive = ActiveItem;
            base.ActivateItem(item);
            ActiveItemChanged?.Invoke(item);
            if (item?.ContextualResourceModel == null)
            {
                return;
            }

            SetActiveServer(item.Environment);

        }

        public Action<WorkSurfaceContextViewModel> ActiveItemChanged;

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
                                                    MessageBoxButton.OK, MessageBoxImage.Error, "", true, true, false, false, true, true);

                    if (result != MessageBoxResult.OK)
                    {
                        _worksurfaceContextManager.ShowDependencies(false, model, ActiveServer);
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
                                                              MessageBoxButton.YesNo, MessageBoxImage.Information, @"", false, false, true, false, false, false) == MessageBoxResult.Yes;

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
                               MessageBoxButton.YesNo, MessageBoxImage.Information, @"", false, false, true, false, false, false);

            var confirmDelete = popupResult == MessageBoxResult.Yes;

            return confirmDelete;
        }

        public IWorkflowDesignerViewModel CreateNewDesigner(IContextualResourceModel resourceModel)
        {
            var workflow = new WorkflowDesignerViewModel(resourceModel) { IsTestView = true };
            return workflow;
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

                _worksurfaceContextManager.DeleteContext(contextualModel);

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
            if (ServerRepository == null)
            {
                return;
            }

            HashSet<IWorkspaceItem> workspaceItemsToRemove = new HashSet<IWorkspaceItem>();

            for (int i = 0; i < _getWorkspaceItemRepository().WorkspaceItems.Count; i++)

            {
                //
                // Get the environment for the workspace item
                //
                IWorkspaceItem item = _getWorkspaceItemRepository().WorkspaceItems[i];
                Dev2Logger.Info($"Start Proccessing WorkspaceItem: {item.ServiceName}", "Warewolf Info");
                IServer environment = ServerRepository.All().Where(env => env.IsConnected).TakeWhile(env => env.Connection != null).FirstOrDefault(env => env.EnvironmentID == item.EnvironmentID);

                if (environment?.ResourceRepository == null)
                {
                    Dev2Logger.Info(@"Environment Not Found", "Warewolf Info");
                    if (environment != null && item.EnvironmentID == environment.EnvironmentID)
                    {
                        workspaceItemsToRemove.Add(item);
                    }
                }
                if (environment != null)
                {
                    Dev2Logger.Info($"Proccessing WorkspaceItem: {item.ServiceName} for Environment: {environment.DisplayName}", "Warewolf Info");
                    if (environment.ResourceRepository != null)
                    {
                        environment.ResourceRepository.LoadResourceFromWorkspace(item.ID, item.WorkspaceID);
                        var resource = environment.ResourceRepository?.All().FirstOrDefault(rm =>
                        {
                            var sameEnv = true;
                            if (item.EnvironmentID != Guid.Empty)
                            {
                                sameEnv = item.EnvironmentID == environment.EnvironmentID;
                            }
                            return rm.ID == item.ID && sameEnv;
                        }) as IContextualResourceModel;

                        if (resource == null)
                        {
                            workspaceItemsToRemove.Add(item);
                        }
                        else
                        {
                            Dev2Logger.Info($"Got Resource Model: {resource.DisplayName} ", "Warewolf Info");
                            var fetchResourceDefinition = environment.ResourceRepository.FetchResourceDefinition(environment, item.WorkspaceID, resource.ID, false);
                            resource.WorkflowXaml = fetchResourceDefinition.Message;
                            resource.IsWorkflowSaved = item.IsWorkflowSaved;
                            resource.OnResourceSaved += model => _getWorkspaceItemRepository().UpdateWorkspaceItemIsWorkflowSaved(model);
                            _worksurfaceContextManager.AddWorkSurfaceContextImpl(resource, true);
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
            {
                SaveAndShutdown(true);
            }
            else
            {
                SaveOnBackgroundTask(false);
            }
        }
        private readonly object _locker = new object();
        void SaveOnBackgroundTask(bool isStudioShutdown)
        {
            SaveWorkspaceItems();
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

        public MessageBoxResult ShowUnsavedWorkDialog()
        {
            var popupResult = PopupProvider.Show(StringResources.Unsaved_Changes, StringResources.CloseHeader,
                               MessageBoxButton.YesNoCancel, MessageBoxImage.Information, @"", false, false, true, false, false, false);

            return popupResult;
        }

        private bool CallSaveDialog(bool closeStudio)
        {
            var result = ShowUnsavedWorkDialog();
            // CANCEL - DON'T CLOSE THE STUDIO
            // NO - DON'T SAVE ANYTHING AND CLOSE THE STUDIO
            // YES - CALL THE SAVE ALL COMMAND AND CLOSE THE STUDIO
            if (result == MessageBoxResult.Cancel)
            {
                closeStudio = false;
            }
            else if (result == MessageBoxResult.Yes)
            {
                closeStudio = true;
                SaveAllCommand.Execute(null);
                if (!ContinueShutDown)
                {
                    closeStudio = false;
                }
            }

            return closeStudio;
        }

        public bool OnStudioClosing()
        {
            var closeStudio = true;
            List<WorkSurfaceContextViewModel> workSurfaceContextViewModels = Items.ToList();
            foreach (WorkSurfaceContextViewModel workSurfaceContextViewModel in workSurfaceContextViewModels)
            {
                var vm = workSurfaceContextViewModel.WorkSurfaceViewModel;
                if (vm != null)
                {
                    if (vm.WorkSurfaceContext == WorkSurfaceContext.Help)
                    {
                        continue;
                    }
                    if (vm.WorkSurfaceContext == WorkSurfaceContext.Workflow)
                    {
                        if (vm is WorkflowDesignerViewModel workflowDesignerViewModel)
                        {
                            if (workflowDesignerViewModel.ResourceModel is IContextualResourceModel resourceModel && !resourceModel.IsWorkflowSaved)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                    }
                    else if (vm.WorkSurfaceContext == WorkSurfaceContext.Settings)
                    {
                        if (vm is SettingsViewModel settingsViewModel && settingsViewModel.IsDirty)
                        {
                            closeStudio = CallSaveDialog(closeStudio);
                            break;
                        }
                    }
                    else if (vm.WorkSurfaceContext == WorkSurfaceContext.Scheduler)
                    {
                        var schedulerViewModel = vm as SchedulerViewModel;
                        if (schedulerViewModel?.SelectedTask != null && schedulerViewModel.SelectedTask.IsDirty)
                        {
                            closeStudio = CallSaveDialog(closeStudio);
                            break;
                        }
                    }
                    else if (vm.GetType().Name == "SourceViewModel`1")
                    {
                        if (vm is SourceViewModel<IServerSource> serverSourceModel)
                        {
                            if (serverSourceModel.IsDirty || serverSourceModel.ViewModel.HasChanged)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                        if (vm is SourceViewModel<IPluginSource> pluginSourceModel)
                        {
                            if (pluginSourceModel.IsDirty || pluginSourceModel.ViewModel.HasChanged)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                        if (vm is SourceViewModel<IWcfServerSource> wcfServerSourceModel)
                        {
                            if (wcfServerSourceModel.IsDirty || wcfServerSourceModel.ViewModel.HasChanged)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                        if (vm is SourceViewModel<IRabbitMQServiceSourceDefinition> rabbitMqServiceSourceModel)
                        {
                            if (rabbitMqServiceSourceModel.IsDirty || rabbitMqServiceSourceModel.ViewModel.HasChanged)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                        if (vm is SourceViewModel<ISharepointServerSource> sharepointServerSourceModel)
                        {
                            if (sharepointServerSourceModel.IsDirty || sharepointServerSourceModel.ViewModel.HasChanged)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                        if (vm is SourceViewModel<IOAuthSource> oAuthSourceModel)
                        {
                            if (oAuthSourceModel.IsDirty || oAuthSourceModel.ViewModel.HasChanged)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                        if (vm is SourceViewModel<IExchangeSource> exchangeSourceModel)
                        {
                            if (exchangeSourceModel.IsDirty || exchangeSourceModel.ViewModel.HasChanged)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                        if (vm is SourceViewModel<IComPluginSource> comPluginSourceModel)
                        {
                            if (comPluginSourceModel.IsDirty || comPluginSourceModel.ViewModel.HasChanged)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                        if (vm is SourceViewModel<IWebServiceSource> webServiceSourceModel)
                        {
                            if (webServiceSourceModel.IsDirty || webServiceSourceModel.ViewModel.HasChanged)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                        if (vm is SourceViewModel<IEmailServiceSource> emailServiceSourceModel)
                        {
                            if (emailServiceSourceModel.IsDirty || emailServiceSourceModel.ViewModel.HasChanged)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                        if (vm is SourceViewModel<IDbSource> dbSourceModel)
                        {
                            if (dbSourceModel.IsDirty || dbSourceModel.ViewModel.HasChanged)
                            {
                                closeStudio = CallSaveDialog(closeStudio);
                                break;
                            }
                        }
                    }
                }
            }
            return closeStudio;
        }

        IMenuViewModel _menuViewModel;
        IServer _activeServer;
        private IExplorerViewModel _explorerViewModel;
        private IWorksurfaceContextManager _worksurfaceContextManager;

        public IWorksurfaceContextManager WorksurfaceContextManager
        {
            get
            {
                return _worksurfaceContextManager;
            }
            set
            {
                _worksurfaceContextManager = value;
            }
        }

        public IWorkflowDesignerViewModel GetWorkflowDesigner()
        {
            var workflowDesignerViewModel = ActiveItem?.WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            return workflowDesignerViewModel;
        }

        public bool IsDownloading()
        {
            return false;
        }

        public async Task<bool> CheckForNewVersion()
        {
            var hasNewVersion = await Version.GetNewerVersionAsync();
            return hasNewVersion;
        }

        public void DisplayDialogForNewVersion()
        {
            BrowserPopupController.ShowPopup(Warewolf.Studio.Resources.Languages.Core.WarewolfLatestDownloadUrl);
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
            var fileChooserView = new FileChooserView();
            var selectedFiles = message.SelectedFiles ?? new List<string>();
            if (!string.IsNullOrEmpty(message.Filter))
            {
                fileChooserView.ShowView(selectedFiles.ToList(), message.Filter);
            }
            else
            {
                fileChooserView.ShowView(selectedFiles.ToList());
            }
            if (fileChooserView.DataContext is FileChooser fileChooser && fileChooser.Result == MessageBoxResult.OK)
            {
                message.SelectedFiles = fileChooser.GetAttachments();
            }
        }
    }
}
