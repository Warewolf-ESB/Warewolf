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
using Dev2.Runtime.Security;
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
using Dev2.Data.ServiceModel;

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
                                        IHandle<NewTestFromDebugMessage>,
                                        IShellViewModel
    {

        private IEnvironmentModel _activeEnvironment;
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
        private ICommand _exitCommand;
        private AuthorizeCommand _settingsCommand;
        private AuthorizeCommand _schedulerCommand;
        private ICommand _showCommunityPageCommand;
        readonly IAsyncWorker _asyncWorker;
        private ICommand _showStartPageCommand;
        bool _canDebug = true;
        bool _menuExpanded;

        public IPopupController PopupProvider { get; set; }

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

        public bool ShouldUpdateActiveState { get; set; }

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

        public void OnActiveEnvironmentChanged()
        {
            NewSqlServerSourceCommand.UpdateContext(ActiveEnvironment);
            NewMySqlSourceCommand.UpdateContext(ActiveEnvironment);
            NewPostgreSqlSourceCommand.UpdateContext(ActiveEnvironment);
            NewOracleSourceCommand.UpdateContext(ActiveEnvironment);
            NewOdbcSourceCommand.UpdateContext(ActiveEnvironment);
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
                    new AuthorizeCommand(AuthorizationContext.Administrator, param => _worksurfaceContextManager.AddSettingsWorkSurface(), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand SchedulerCommand
        {
            get
            {
                return _schedulerCommand ?? (_schedulerCommand =
                    new AuthorizeCommand(AuthorizationContext.Administrator, param => _worksurfaceContextManager.AddSchedulerWorkSurface(), param => IsActiveEnvironmentConnected()));
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

        public AuthorizeCommand<string> NewSqlServerSourceCommand
        {
            get
            {
                return _newSqlServerSourceCommand ?? (_newSqlServerSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewSqlServerSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewMySqlSourceCommand
        {
            get
            {
                return _newMySqlSourceCommand ?? (_newMySqlSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewMySqlSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewPostgreSqlSourceCommand
        {
            get
            {
                return _newPostgreSqlSourceCommand ?? (_newPostgreSqlSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewPostgreSqlSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewOracleSourceCommand
        {
            get
            {
                return _newOracleSourceCommand ?? (_newOracleSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewOracleSource(@""), param => IsActiveEnvironmentConnected()));
            }
        }

        public AuthorizeCommand<string> NewOdbcSourceCommand
        {
            get
            {
                return _newOdbcSourceCommand ?? (_newOdbcSourceCommand =
                    new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewOdbcSource(@""), param => IsActiveEnvironmentConnected()));
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
            IPopupController popupController = null, IExplorerViewModel explorer = null)
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
            ShouldUpdateActiveState = true;
            EnvironmentRepository = environmentRepository;
            SetActiveEnvironment(_activeServer.EnvironmentID);

            MenuPanelWidth = 60;
            _menuExpanded = false;

            ExplorerViewModel = explorer ?? new ExplorerViewModel(this, CustomContainer.Get<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>(), true);

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
                _worksurfaceContextManager.AddReverseDependencyVisualizerWorkSurface(message.Model);
            }
        }

        public void Handle(SaveAllOpenTabsMessage message)
        {
            Dev2Logger.Debug(message.GetType().Name);
            PersistTabs();
        }


        public void Handle(AddWorkSurfaceMessage message)
        {
            IsNewWorkflowSaved = true;
            Dev2Logger.Info(message.GetType().Name);
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
            _worksurfaceContextManager.ShowDependencies(dependsOnMe, model, ActiveServer);
        }

        public void ShowDependencies(Guid resourceId, IServer server, bool isSource)
        {
            var environmentModel = EnvironmentRepository.Get(server.EnvironmentID);
            if (environmentModel != null)
            {
                if (!isSource)
                    environmentModel.ResourceRepository.LoadResourceFromWorkspace(resourceId, Guid.Empty);
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

        public IServer LocalhostServer => CustomContainer.Get<IServer>();

        public void SetActiveEnvironment(Guid environmentId)
        {
            var environmentModel = EnvironmentRepository.Get(environmentId);
            ActiveEnvironment = environmentModel;
            var server = ExplorerViewModel?.ConnectControlViewModel?.Servers?.FirstOrDefault(a => a.EnvironmentID == environmentId);
            if (server != null)
            {
                SetActiveServer(server);
            }
        }

        public void SetActiveServer(IServer server)
        {
            ActiveServer = server;
            if (ActiveServer.IsConnected && !ActiveEnvironment.IsConnected)
            {
                ActiveEnvironment.Connect();
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
            var environmentModel = EnvironmentRepository.Get(environmentId);
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
            var environmentModel = EnvironmentRepository.Get(environmentId);
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

        public void OpenResource(Guid resourceId, Guid environmentId, IServer activeServer)
        {
            var environmentModel = EnvironmentRepository.Get(environmentId);
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
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessServerSource(contextualResourceModel, workSurfaceKey,environmentModel,activeServer));
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
                new ManagePluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def,
                AsyncWorker);
            var vm = new SourceViewModel<IPluginSource>(EventPublisher, pluginSourceViewModel, PopupProvider, new ManagePluginSourceControl(),ActiveEnvironment);

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
                ActiveEnvironment);
            var vm = new SourceViewModel<IWcfServerSource>(EventPublisher, wcfSourceViewModel, PopupProvider, new ManageWcfSourceControl(), ActiveEnvironment);

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
            var vm = new SourceViewModel<IRabbitMQServiceSourceDefinition>(EventPublisher, viewModel, PopupProvider, new ManageRabbitMQSourceControl(), ActiveEnvironment);

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
                new SharepointServerSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def,
                AsyncWorker,
                null);
            var vm = new SourceViewModel<ISharepointServerSource>(EventPublisher, viewModel, PopupProvider, new SharepointServerSource(), ActiveEnvironment);

            var key = workSurfaceKey;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            return workSurfaceContextViewModel;
        }

        private WorkSurfaceContextViewModel ProcessServerSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey, IEnvironmentModel environmentModel, IServer activeServer)
        {

            var selectedServer = new ServerSource
            {
                ID = contextualResourceModel.ID,
                ResourcePath = contextualResourceModel.GetSavePath()
            };

            var viewModel = new ManageNewServerViewModel(
                new ManageNewServerSourceModel(activeServer.UpdateRepository, activeServer.QueryProxy, environmentModel.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                selectedServer,
                AsyncWorker,
                new ExternalProcessExecutor());
            var vm = new SourceViewModel<IServerSource>(EventPublisher, viewModel, PopupProvider, new ManageServerControl(), environmentModel);

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
            var vm = new SourceViewModel<IOAuthSource>(EventPublisher, oauthSourceViewModel, PopupProvider, new ManageOAuthSourceControl(), ActiveEnvironment);

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
                new ManageExchangeSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def,
                AsyncWorker);
            var vm = new SourceViewModel<IExchangeSource>(EventPublisher, emailSourceViewModel, PopupProvider, new ManageExchangeSourceControl(), ActiveEnvironment);

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
                new ManageComPluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def,
                AsyncWorker);
            var vm = new SourceViewModel<IComPluginSource>(EventPublisher, wcfSourceViewModel, PopupProvider, new ManageComPluginSourceControl(), ActiveEnvironment);

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
                new ManageWebServiceSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def,
                AsyncWorker,
                new ExternalProcessExecutor());
            var vm = new SourceViewModel<IWebServiceSource>(EventPublisher, viewModel, PopupProvider, new ManageWebserviceSourceControl(), ActiveEnvironment);

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
                new ManageEmailSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(),
                def, AsyncWorker);
            var vm = new SourceViewModel<IEmailServiceSource>(EventPublisher, emailSourceViewModel, PopupProvider, new ManageEmailSourceControl(), ActiveEnvironment);
            var key = workSurfaceKey;
            var workSurfaceContextViewModel = _worksurfaceContextManager.EditResource(key, vm);
            return workSurfaceContextViewModel;
        }

        private ManageMySqlSourceViewModel ProcessMySQLDBSource(IDbSource def)
        {
            return new ManageMySqlSourceViewModel(
                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.DisplayName)
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , def
                , AsyncWorker);
        }

        private ManagePostgreSqlSourceViewModel ProcessPostgreSQLDBSource(IDbSource def)
        {
            return new ManagePostgreSqlSourceViewModel(
                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.DisplayName)
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , def
                , AsyncWorker);
        }

        private ManageOracleSourceViewModel ProcessOracleDBSource(IDbSource def)
        {
            return new ManageOracleSourceViewModel(
                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.DisplayName)
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , def
                , AsyncWorker);
        }

        private ManageOdbcSourceViewModel ProcessODBCDBSource(IDbSource def)
        {
            return new ManageOdbcSourceViewModel(
                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository
                , ActiveServer.QueryProxy, ActiveEnvironment.DisplayName)
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , def
                , AsyncWorker);
        }

        private ManageSqlServerSourceViewModel ProcessSQLDBSource(IDbSource def)
        {
            return new ManageSqlServerSourceViewModel(
                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.DisplayName)
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

            var vm = new SourceViewModel<IDbSource>(EventPublisher, dbSourceViewModel, PopupProvider, new ManageDatabaseSourceControl(), ActiveEnvironment);
            var key = workSurfaceKey;
            if (key != null)
            {
                key.EnvironmentID = ActiveEnvironment.ID;
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
            var environmentModel = EnvironmentRepository.Get(environmentId);
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
            var environmentModel = EnvironmentRepository.Get(environmentId);
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

            Uri url;
            Uri.TryCreate(webServerUri, relativeUrl, out url);

            BrowserPopupController.ShowPopup(url.ToString());
        }

        public void CreateNewSchedule(Guid resourceId)
        {
            var environmentModel = EnvironmentRepository.Get(ActiveEnvironment.ID);
            if (environmentModel != null)
            {
                var contextualResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(resourceId);
                _worksurfaceContextManager.CreateNewScheduleWorkSurface(contextualResourceModel);
            }
        }

        public void CreateTest(Guid resourceId)
        {
            var environmentModel = EnvironmentRepository.Get(ActiveEnvironment.ID);
            if (environmentModel != null)
            {
                var contextualResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(resourceId);

                var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ServiceTestsViewer);
                if (contextualResourceModel != null)
                {
                    workSurfaceKey.EnvironmentID = contextualResourceModel.Environment.ID;
                    workSurfaceKey.ResourceID = contextualResourceModel.ID;
                    workSurfaceKey.ServerID = contextualResourceModel.ServerID;

                    _worksurfaceContextManager.ViewTestsForService(contextualResourceModel, workSurfaceKey);
                }
            }
        }

        public void RunAllTests(Guid resourceId)
        {
            var environmentModel = EnvironmentRepository.Get(ActiveEnvironment.ID);
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
            var environmentModel = EnvironmentRepository.Get(environmentId);
            var contextualResourceModel = environmentModel?.ResourceRepository.LoadContextualResourceModel(resourceId);
            if (contextualResourceModel != null)
            {
                var wfscvm = _worksurfaceContextManager.FindWorkSurfaceContextViewModel(contextualResourceModel);
                DeactivateItem(wfscvm, true);
            }
        }

        public async void OpenResourceAsync(Guid resourceId, IServer server)
        {
            var environmentModel = EnvironmentRepository.Get(server.EnvironmentID);
            if (environmentModel != null)
            {
                var contextualResourceModel = await environmentModel.ResourceRepository.LoadContextualResourceModelAsync(resourceId);
                _worksurfaceContextManager.DisplayResourceWizard(contextualResourceModel);
            }
        }

        public void DeployResources(Guid sourceEnvironmentId, Guid destinationEnvironmentId, IList<Guid> resources, bool deployTests)
        {
            var environmentModel = EnvironmentRepository.Get(destinationEnvironmentId);
            var sourceEnvironmentModel = EnvironmentRepository.Get(sourceEnvironmentId);
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

        public void EditResource(IComPluginSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            _worksurfaceContextManager.EditResource(selectedSource, workSurfaceKey);
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
            // ReSharper disable once PossibleInvalidOperationException
            var manageNewServerSourceModel = new ManageNewServerSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name);
            var manageNewServerViewModel = new ManageNewServerViewModel(manageNewServerSourceModel, saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _asyncWorker, new ExternalProcessExecutor()) { SelectedGuid = key.ResourceID.Value };
            var workSurfaceViewModel = new SourceViewModel<IServerSource>(EventPublisher, manageNewServerViewModel, PopupProvider, new ManageServerControl(), ActiveEnvironment);
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

        public bool IsActiveEnvironmentConnected()
        {
            if (ActiveEnvironment == null)

            {
                return false;
            }

            var isActiveEnvironmentConnected = ActiveEnvironment != null && ActiveEnvironment.IsConnected && ActiveEnvironment.CanStudioExecute && ShouldUpdateActiveState;
            if (ActiveEnvironment.IsConnected && ShouldUpdateActiveState)
            {
                if (ToolboxViewModel?.BackedUpTools != null && ToolboxViewModel.BackedUpTools.Count == 0)
                {
                    ToolboxViewModel.BuildToolsList();
                }
            }
            if (ToolboxViewModel != null)
                ToolboxViewModel.IsVisible = isActiveEnvironmentConnected;
            return isActiveEnvironmentConnected;
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
            RefreshActiveEnvironment();
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
                var wfItem = item?.WorkSurfaceViewModel as IWorkflowDesignerViewModel;
                if (wfItem != null)
                {
                    _worksurfaceContextManager.AddWorkspaceItem(wfItem.ResourceModel);
                }
                var studioTestViewModel = item?.WorkSurfaceViewModel as StudioTestViewModel;
                if (studioTestViewModel != null)
                {
                    var serviceTestViewModel = studioTestViewModel.ViewModel as ServiceTestViewModel;
                    EventPublisher.Publish(serviceTestViewModel?.SelectedServiceTest != null
                        ? new DebugOutputMessage(serviceTestViewModel.SelectedServiceTest?.DebugForTest ?? new List<IDebugState>())
                        : new DebugOutputMessage(new List<IDebugState>()));

                    if (serviceTestViewModel != null)
                        serviceTestViewModel.WorkflowDesignerViewModel.IsTestView = true;
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

        public void ResetMainView()
        {
            MainView mainView = MainView.GetInstance();
            mainView.ResetToStartupView();
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
            if (item?.ContextualResourceModel == null) return;
            SetActiveEnvironment(item.Environment);

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
            //            Task t = new Task(() =>
            //            {
            //
            //                lock (_locker)
            //                {
            //                    foreach (var ctx in Items.Where(a => true).ToList())
            //                    {
            //                        if (!ctx.WorkSurfaceViewModel.DisplayName.ToLower().Contains("version") && ctx.IsEnvironmentConnected())
            //                        {
            //                            ctx.Save(true, isStudioShutdown);
            //                        }
            //                    }
            //                }
            //            });
            //            t.Start();

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
            return true;
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
            var fileChooser = fileChooserView.DataContext as FileChooser;
            if (fileChooser != null && fileChooser.Result == MessageBoxResult.OK)
            {
                message.SelectedFiles = fileChooser.GetAttachments();
            }
        }
    }
}
