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
using System.Collections.Generic;
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
using Dev2.Webs;
using Dev2.Common.Wrappers;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Common.Common;
using Dev2.Instrumentation;
using Dev2.Triggers;
using Dev2.Dialogs;
using Dev2.Studio.Enums;
using Warewolf.Data;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Warewolf.Core;

namespace Dev2.Studio.ViewModels
{
    public class ShellViewModel : BaseConductor<IWorkSurfaceContextViewModel>,
        IHandle<DeleteResourcesMessage>,
        IHandle<AddWorkSurfaceMessage>,
        IHandle<RemoveResourceAndCloseTabMessage>,
        IHandle<SaveAllOpenTabsMessage>,
        IHandle<ShowReverseDependencyVisualizer>,
        IHandle<FileChooserMessage>,
        IHandle<NewTestFromDebugMessage>,
        IShellViewModel
    {
        IWorkSurfaceContextViewModel _previousActive;
        bool _disposed;
        private AuthorizeCommand<string> _newServiceCommand;
        private AuthorizeCommand<string> _newPluginSourceCommand;
        private AuthorizeCommand<string> _newSqlServerSourceCommand;
        private AuthorizeCommand<string> _newMySqlSourceCommand;
        private AuthorizeCommand<string> _newPostgreSqlSourceCommand;
        private AuthorizeCommand<string> _newOracleSourceCommand;
        private AuthorizeCommand<string> _newOdbcSourceCommand;
        private AuthorizeCommand<string> _newWebSourceCommand;
        private AuthorizeCommand<string> _newRedisSourceCommand;
        private AuthorizeCommand<string> _newElasticsearchSourceCommand;
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

        //TODO: Remove once the Tasks command is the only command in use.
        private AuthorizeCommand _schedulerCommand;
        private AuthorizeCommand _queueEventsCommand;
        private AuthorizeCommand _tasksCommand;
        private ICommand _searchCommand;
        private ICommand _showCommunityPageCommand;
        private ICommand _addWorkflowCommand;
        readonly IAsyncWorker _asyncWorker;
        readonly IViewFactory _factory;
        readonly IFile _file;
        readonly Common.Interfaces.Wrappers.IFilePath _filePath;
        private ICommand _showStartPageCommand;
        IContextualResourceModel _contextualResourceModel;
        bool _canDebug = true;
        bool _menuExpanded;
        readonly IApplicationTracker _applicationTracker;
        public IPopupController PopupProvider { get; set; }
        IServerRepository ServerRepository { get; }
        public bool CloseCurrent { get; private set; }

        public IExplorerViewModel ExplorerViewModel
        {
            get => _explorerViewModel;
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
            get => _activeServer;
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

        void SetEnvironmentIsSelected()
        {
            var environmentViewModels = ExplorerViewModel?.Environments;
            if (environmentViewModels != null)
            {
                foreach (var environment in environmentViewModels)
                {
                    environment.IsSelected = false;
                }

                var environmentViewModel = environmentViewModels.FirstOrDefault(model => model.ResourceId == _activeServer.EnvironmentID);
                if (environmentViewModel != null)
                {
                    environmentViewModel.IsSelected = true;
                    ExplorerViewModel.SelectedEnvironment = environmentViewModel;
                }
            }
        }

        internal async Task<bool> LoadWorkflowAsync(string clickedResource)
        {
            _contextualResourceModel = null;
            if (!File.Exists(clickedResource) || clickedResource.Contains(EnvironmentVariables.VersionsPath))
            {
                return false;
            }

            ActiveServer.ResourceRepository.ReLoadResources();
            var fileName = string.Empty;
            fileName = Path.GetFileNameWithoutExtension(clickedResource);
            var singleResource = ActiveServer.ResourceRepository.FindSingle(p => p.ResourceName == fileName);
            var serverRepo = CustomContainer.Get<IServerRepository>();
            if (singleResource != null && clickedResource.Contains(EnvironmentVariables.ResourcePath))
            {
                OpenResource(singleResource.ID, ActiveServer.EnvironmentID, ActiveServer);
            }
            else
            {
                if (singleResource == null)
                {
                    _contextualResourceModel = await ResourceExtensionHelper.HandleResourceNotInResourceFolderAsync(clickedResource, PopupProvider, this, _file, _filePath, serverRepo);
                    if (_contextualResourceModel != null && (_contextualResourceModel.ResourceType == ResourceType.WorkflowService || _contextualResourceModel.ResourceType == ResourceType.Service))
                    {
                        SaveDialogHelper.ShowNewWorkflowSaveDialog(_contextualResourceModel, false, clickedResource);
                    }
                }

                if (singleResource != null && !clickedResource.Contains(EnvironmentVariables.ResourcePath))
                {
                    _contextualResourceModel = await ResourceExtensionHelper.HandleResourceInResourceFolderAndOtherDir(clickedResource, PopupProvider, this, _file, _filePath, serverRepo);
                }
            }

            return true;
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
            TasksCommand.UpdateContext(ActiveServer);
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

                if (ActiveItem.WorkSurfaceKey.WorkSurfaceContext != WorkSurfaceContext.Workflow && ActiveItem.WorkSurfaceViewModel is IStudioTab vm)
                {
                    return new AuthorizeCommand(AuthorizationContext.Any, o => vm.DoDeactivate(false), o => vm.IsDirty);
                }

                return ActiveItem.SaveCommand;
            }
        }

        public IAuthorizeCommand DebugCommand
        {
            get => ActiveItem == null ? new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false) : ActiveItem.DebugCommand;
        }

        public IAuthorizeCommand QuickDebugCommand
        {
            get => ActiveItem == null ? new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false) : ActiveItem.QuickDebugCommand;
        }

        public IAuthorizeCommand QuickViewInBrowserCommand
        {
            get => ActiveItem == null ? new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false) : ActiveItem.QuickViewInBrowserCommand;
        }

        public IAuthorizeCommand ViewInBrowserCommand
        {
            get => ActiveItem == null ? new AuthorizeCommand(AuthorizationContext.None, p => { }, param => false) : ActiveItem.ViewInBrowserCommand;
        }

        public ICommand ShowStartPageCommand
        {
            get => _showStartPageCommand ?? (_showStartPageCommand = new DelegateCommand(param => ShowStartPageAsync()));
        }

        public ICommand ShowCommunityPageCommand
        {
            get => _showCommunityPageCommand ?? (_showCommunityPageCommand = new DelegateCommand(param => ShowCommunityPage()));
        }

        public IAuthorizeCommand SettingsCommand
        {
            get
            {
                return _settingsCommand ?? (_settingsCommand =
                    new AuthorizeCommand(AuthorizationContext.Administrator, param => _worksurfaceContextManager.AddSettingsWorkSurface(), param => IsActiveServerConnected()));
            }
        }

        public ICommand SearchCommand
        {
            get { return _searchCommand ?? (_searchCommand = new DelegateCommand(param => ShowSearchWindow())); }
        }

        IResourcePickerDialog _currentResourcePicker;

        public ICommand AddWorkflowCommand
        {
            get => _addWorkflowCommand ?? (_addWorkflowCommand = new DelegateCommand(param => OpenResourcePicker(param)));
        }

        private void OpenResourcePicker(object item)
        {
            if (_currentResourcePicker is null)
            {
                return;
            }

            if (_currentResourcePicker.ShowDialog(ServerRepository.ActiveServer))
            {
                var optionView = item as Warewolf.UI.OptionView;
                var selectedResource = _currentResourcePicker.SelectedResource;

                if (optionView.DataContext is Warewolf.Options.OptionWorkflow optionWorkflow)
                {
                    optionWorkflow.Workflow = new WorkflowWithInputs
                    {
                        Name = selectedResource.ResourcePath,
                        Value = selectedResource.ResourceId,
                        Inputs = GetInputsFromWorkflow(selectedResource.ResourceId)
                    };
                }
            }
        }

        public List<IServiceInputBase> GetInputsFromWorkflow(Guid resourceId)
        {
            var inputs = new List<IServiceInputBase>();
            var contextualResourceModel = ServerRepository.ActiveServer.ResourceRepository.LoadContextualResourceModel(resourceId);
            var dataList = new DataListModel();
            var dataListConversionUtils = new DataListConversionUtils();
            dataList.Create(contextualResourceModel.DataList, contextualResourceModel.DataList);
            var inputList = dataListConversionUtils.GetInputs(dataList);
            inputs = inputList.Select(sca =>
            {
                var serviceTestInput = new ServiceInput(sca.DisplayValue, "");
                return serviceTestInput.As<IServiceInputBase>();
            }).ToList();
            return inputs;
        }

        public OptomizedObservableCollection<IDataListItem> GetOutputsFromWorkflow(Guid resourceId)
        {
            var contextualResourceModel = ServerRepository.ActiveServer.ResourceRepository.LoadContextualResourceModel(resourceId);
            var dataList = new DataListModel();
            var dataListConversionUtils = new DataListConversionUtils();
            dataList.Create(contextualResourceModel.DataList, contextualResourceModel.DataList);
            var outputs = dataListConversionUtils.GetOutputs(dataList);
            return outputs;
        }

        IResourcePickerDialog CreateResourcePickerDialog()
        {
            if (_currentResourcePicker == null)
            {
                var environmentViewModels = ExplorerViewModel?.Environments;
                if (environmentViewModels != null)
                {
                    var environmentViewModel = environmentViewModels.FirstOrDefault(model => model.ResourceId == _activeServer.EnvironmentID);
                    var res = new ResourcePickerDialog(enDsfActivityType.All, environmentViewModel);
                    ResourcePickerDialog.CreateAsync(enDsfActivityType.Workflow, environmentViewModel).ContinueWith(a => _currentResourcePicker = a.Result);
                    return res;
                }
            }

            return _currentResourcePicker;
        }

        public IResource GetResource(string resourceId)
        {
            try
            {
                var explorerItem = ExplorerViewModel.Environments[0].AsList().First(o => o.ResourceId == Guid.Parse(resourceId));

                IResource resource = new Resource
                {
                    ResourceID = explorerItem.ResourceId,
                    ResourceName = explorerItem.ResourceName
                };

                return resource;
            }
            finally
            {
                Dev2Logger.Error($"Could not find resource for - {resourceId}", GlobalConstants.WarewolfError);
            }

            return null;
        }

        private ICommand _runCoverageCommand;
        public ICommand RunCoverageCommand => _runCoverageCommand ?? (_runCoverageCommand = new DelegateCommand(RunCoverage));

        private void RunCoverage(object explorerObj)
        {
            var resourcePath = "";
            var resourceId = Guid.Empty;
            switch (explorerObj)
            {
                case ExplorerItemViewModel explorerItem:
                    resourcePath = explorerItem.ResourcePath;
                    resourceId = explorerItem.ResourceId;
                    break;
                case EnvironmentViewModel environmentViewModel:
                    resourcePath = environmentViewModel.ResourcePath;
                    resourceId = environmentViewModel.ResourceId;
                    break;
                case WorkflowDesignerViewModel workflowDesignerViewModel:
                    resourcePath = workflowDesignerViewModel.ResourceModel.GetSavePath() + workflowDesignerViewModel.ResourceModel.DisplayName;
                    resourceId = workflowDesignerViewModel.ResourceModel.ID;
                    break;
            }

            RunCoverage(resourcePath, resourceId);
        }

        public IAuthorizeCommand SchedulerCommand
        {
            get => _schedulerCommand ?? (_schedulerCommand = new AuthorizeCommand(AuthorizationContext.Administrator, param => _worksurfaceContextManager.AddSchedulerWorkSurface(), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand QueueEventsCommand
        {
            get => _queueEventsCommand ?? (_queueEventsCommand = new AuthorizeCommand(AuthorizationContext.Administrator, param => _worksurfaceContextManager.AddQueuesWorkSurface(), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand TasksCommand
        {
            get => _tasksCommand ?? (_tasksCommand = new AuthorizeCommand(AuthorizationContext.Administrator, param => _worksurfaceContextManager.AddTriggersWorkSurface(), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewServiceCommand
        {
            get => _newServiceCommand ?? (_newServiceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewService(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewPluginSourceCommand
        {
            get => _newPluginSourceCommand ?? (_newPluginSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewPluginSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewSqlServerSourceCommand
        {
            get => _newSqlServerSourceCommand ?? (_newSqlServerSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewSqlServerSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewMySqlSourceCommand
        {
            get => _newMySqlSourceCommand ?? (_newMySqlSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewMySqlSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewPostgreSqlSourceCommand
        {
            get => _newPostgreSqlSourceCommand ?? (_newPostgreSqlSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewPostgreSqlSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewOracleSourceCommand
        {
            get => _newOracleSourceCommand ?? (_newOracleSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewOracleSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewOdbcSourceCommand
        {
            get => _newOdbcSourceCommand ?? (_newOdbcSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewOdbcSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewWebSourceCommand
        {
            get => _newWebSourceCommand ?? (_newWebSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewWebSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewRedisSourceCommand
        {
            get => _newRedisSourceCommand ?? (_newRedisSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewRedisSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewElasticsearchSourceCommand
        {
            get => _newElasticsearchSourceCommand ?? (_newElasticsearchSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewElasticsearchSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewServerSourceCommand
        {
            get => _newServerSourceCommand ?? (_newServerSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewServerSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewEmailSourceCommand
        {
            get => _newEmailSourceCommand ?? (_newEmailSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewEmailSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewExchangeSourceCommand
        {
            get => _newExchangeSourceCommand ?? (_newExchangeSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewExchangeSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewRabbitMQSourceCommand
        {
            get => _newRabbitMQSourceCommand ?? (_newRabbitMQSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewRabbitMQSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewSharepointSourceCommand
        {
            get => _newSharepointSourceCommand ?? (_newSharepointSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewSharepointSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewDropboxSourceCommand
        {
            get => _newDropboxSourceCommand ?? (_newDropboxSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewDropboxSource(@""), param => IsActiveServerConnected()));
        }

        public IAuthorizeCommand<string> NewWcfSourceCommand
        {
            get => _newWcfSourceCommand ?? (_newWcfSourceCommand = new AuthorizeCommand<string>(AuthorizationContext.Contribute, param => NewWcfSource(@""), param => IsActiveServerConnected()));
        }

        public ICommand ExitCommand
        {
            get => _exitCommand ?? (_exitCommand = new RelayCommand(param => Application.Current.Shutdown(), param => true));
        }

        public ICommand DeployCommand
        {
            get => _deployCommand ?? (_deployCommand = new RelayCommand(param => AddDeploySurface(new List<IExplorerTreeItem>())));
        }

        public ICommand MergeCommand
        {
            get => _mergeCommand ?? (_mergeCommand = new RelayCommand(param =>
            {
                // OPEN WINDOW TO SELECT RESOURCE TO MERGE WITH
                var resourceId = Guid.Parse("ea916fa6-76ca-4243-841c-74fa18dd8c14");
                OpenMergeConflictsView(ActiveItem as IExplorerItemViewModel, resourceId, ActiveServer);
            }));
        }

        public IVersionChecker Version { get; }

        public ShellViewModel()
            : this(EventPublishers.Aggregator, new AsyncWorker(), CustomContainer.Get<IServerRepository>(), new VersionChecker(), new ViewFactory())
        {
        }

        public ShellViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IServerRepository serverRepository,
            IVersionChecker versionChecker, IViewFactory factory)
            : this(eventPublisher, asyncWorker, serverRepository, versionChecker, factory, true, null, null, null, null)
        {
        }

        public ShellViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IServerRepository serverRepository,
            IVersionChecker versionChecker, IViewFactory factory, bool createDesigners)
            : this(eventPublisher, asyncWorker, serverRepository, versionChecker, factory, createDesigners, null, null, null, null)
        {
        }

        public ShellViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IServerRepository serverRepository,
            IVersionChecker versionChecker, IViewFactory factory, bool createDesigners, IBrowserPopupController browserPopupController)
            : this(eventPublisher, asyncWorker, serverRepository, versionChecker, factory, createDesigners, browserPopupController, null, null, null)
        {
        }

        public ShellViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IServerRepository serverRepository,
            IVersionChecker versionChecker, IViewFactory factory, bool createDesigners, IBrowserPopupController browserPopupController, IPopupController popupController)
            : this(eventPublisher, asyncWorker, serverRepository, versionChecker, factory, createDesigners, browserPopupController, popupController, null, null)
        {
        }

        public ShellViewModel(IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IServerRepository serverRepository,
            IVersionChecker versionChecker, IViewFactory factory, bool createDesigners, IBrowserPopupController browserPopupController,
            IPopupController popupController, IExplorerViewModel explorer, IResourcePickerDialog currentResourcePicker)
            : base(eventPublisher)
        {
            _file = new FileWrapper();
            _filePath = new FilePathWrapper();
            Version = versionChecker ?? throw new ArgumentNullException(nameof(versionChecker));
            VerifyArgument.IsNotNull(@"asyncWorker", asyncWorker);
            _asyncWorker = asyncWorker;
            _factory = factory;
            _worksurfaceContextManager = new WorksurfaceContextManager(createDesigners, this);
            BrowserPopupController = browserPopupController ?? new ExternalBrowserPopupController();
            if (popupController == null)
            {
                popupController = CustomContainer.Get<IPopupController>();
            }

            PopupProvider = popupController;
            ServerRepository = serverRepository ?? throw new ArgumentNullException(nameof(serverRepository));
            _activeServer = LocalhostServer;
            ServerRepository.ActiveServer = _activeServer;
            ShouldUpdateActiveState = true;
            SetActiveServer(_activeServer.EnvironmentID);

            MenuPanelWidth = 60;
            _menuExpanded = false;

            ExplorerViewModel = explorer ?? new ExplorerViewModel(this, CustomContainer.Get<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>(), true);

            AddWorkspaceItems(popupController);
            ShowStartPageAsync();
            DisplayName = @"Warewolf" + $" ({ClaimsPrincipal.Current.Identity.Name})".ToUpperInvariant();
            _applicationTracker = CustomContainer.Get<IApplicationTracker>();

            _currentResourcePicker = currentResourcePicker;
            if (_currentResourcePicker == null)
            {
                _currentResourcePicker = CreateResourcePickerDialog();
            }
        }

        public void Handle(ShowReverseDependencyVisualizer message)
        {
            Dev2Logger.Debug(message.GetType().Name, "Warewolf Debug");
            if (message.Model != null)
            {
                _worksurfaceContextManager.TryShowDependencies(message.Model);
            }
        }

        public void Handle(SaveAllOpenTabsMessage message)
        {
            Dev2Logger.Debug(message.GetType().Name, GlobalConstants.WarewolfDebug);
            PersistTabs();
        }

        public void Handle(AddWorkSurfaceMessage message)
        {
            IsNewWorkflowSaved = true;
            Dev2Logger.Info(message.GetType().Name, GlobalConstants.WarewolfInfo);
            _worksurfaceContextManager.AddWorkSurface(message.WorkSurfaceObject);
            if (message.ShowDebugWindowOnLoad && ActiveItem != null && _canDebug)
            {
                ActiveItem.DebugCommand.Execute(null);
            }
        }

        public bool IsNewWorkflowSaved { get; set; }

        public void Handle(DeleteResourcesMessage message)
        {
            Dev2Logger.Info(message.GetType().Name, GlobalConstants.WarewolfInfo);
            DeleteResources(message.ResourceModels, message.FolderName, message.ShowDialog, message.ActionToDoOnDelete);
        }

        public void ShowDependencies(Guid resourceId, IServer server, bool isSource)
        {
            var environmentModel = ServerRepository.Get(server.EnvironmentID);
            if (environmentModel == null)
            {
                return;
            }

            if (!isSource)
            {
                environmentModel.ResourceRepository.LoadResourceFromWorkspace(resourceId, Guid.Empty);
            }

            if (server.IsConnected)
            {
                ResourceModel contextualResourceModel;
                if (isSource)
                {
                    var resource = environmentModel.ResourceRepository.LoadContextualResourceModel(resourceId) as IResourceModel;
                    contextualResourceModel = new ResourceModel(environmentModel, EventPublisher);
                    contextualResourceModel.Update(resource);
                }
                else
                {
                    var resource = environmentModel.ResourceRepository.FindSingle(model => model.ID == resourceId, true);
                    contextualResourceModel = new ResourceModel(environmentModel, EventPublisher);
                    contextualResourceModel.Update(resource);
                }

                contextualResourceModel.ID = resourceId;
                _worksurfaceContextManager.ShowDependencies(true, contextualResourceModel, server);
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
            var splashPage = new SplashPage {DataContext = splashViewModel};
            ISplashView splashView = splashPage;
            splashViewModel.ShowServerStudioVersion();
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

        public void NewQueueEvent(Guid resourceId)
        {
            CreateNewQueueEvent(resourceId);
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
            var mergeServiceViewModel = new MergeServiceViewModel(this, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), currentResource, new MergeSelectionView(), ActiveServer);
            var result = mergeServiceViewModel.ShowMergeDialog();
            if (result == MessageBoxResult.OK)
            {
                var selectedMergeItem = mergeServiceViewModel.SelectedMergeItem;
                var server = selectedMergeItem?.Server;

                if (selectedMergeItem is VersionViewModel differenceVersion)
                {
                    var differenceResourceModel = differenceVersion.VersionInfo.ToContextualResourceModel(server, differenceVersion.ResourceId);
                    var currentResourceModel = ActiveServer?.ResourceRepository.LoadContextualResourceModel(differenceVersion.ResourceId);

                    BuildAndViewCurrentVersion(currentResourceModel, differenceResourceModel, differenceVersion);
                }
                else if (currentResource is VersionViewModel currentVersion)
                {
                    var currentResourceModel = currentVersion.VersionInfo.ToContextualResourceModel(currentResource.Server, currentVersion.ResourceId);
                    var differenceResourceModel = ActiveServer?.ResourceRepository.LoadContextualResourceModel(currentResource.Parent.ResourceId);

                    BuildAndViewCurrentVersion(currentResourceModel, differenceResourceModel, currentVersion);
                }
                else
                {
                    if (selectedMergeItem != null)
                    {
                        OpenMergeConflictsView(currentResource, selectedMergeItem.ResourceId, server);
                    }
                }
            }
        }

        private void BuildAndViewCurrentVersion(IContextualResourceModel resourceModel, IContextualResourceModel otherResourceModel, VersionViewModel version)
        {
            if (otherResourceModel != null && resourceModel != null)
            {
                resourceModel.ResourceName = otherResourceModel.ResourceName;
                if (resourceModel.IsVersionResource)
                {
                    resourceModel.VersionInfo = version.VersionInfo;
                }

                if (otherResourceModel.IsVersionResource)
                {
                    otherResourceModel.VersionInfo = version.VersionInfo;
                }

                var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.MergeConflicts);
                workSurfaceKey.EnvironmentID = otherResourceModel.Environment.EnvironmentID;
                workSurfaceKey.ResourceID = otherResourceModel.ID;
                workSurfaceKey.ServerID = otherResourceModel.ServerID;
                _worksurfaceContextManager.ViewMergeConflictsService(resourceModel, otherResourceModel, true, workSurfaceKey);
            }
        }

        public void OpenMergeConflictsView(IExplorerItemViewModel currentResource, Guid differenceResourceId, IServer server)
        {
            if (currentResource is ExplorerItemViewModel normalExplorer && normalExplorer.Server != null)
            {
                var currentResourceModel = normalExplorer.Server.ResourceRepository.LoadContextualResourceModel(currentResource.ResourceId);
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

        public void OpenResource(Guid resourceId, Guid environmentId, IServer activeServer, IContextualResourceModel contextualResourceModel)
        {
            _contextualResourceModel = contextualResourceModel;
            OpenResource(resourceId, environmentId, activeServer);
        }

        public void OpenResource(Guid resourceId, Guid environmentId, IServer activeServer)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            environmentModel?.ResourceRepository?.UpdateServer(activeServer);
            if (_contextualResourceModel == null)
            {
                _contextualResourceModel = environmentModel?.ResourceRepository.LoadContextualResourceModel(resourceId);
            }

            if (_contextualResourceModel != null)
            {
                var workSurfaceKey = new WorkSurfaceKey {EnvironmentID = environmentId, ResourceID = resourceId, ServerID = _contextualResourceModel.ServerID};
                switch (_contextualResourceModel.ServerResourceType)
                {
                    case "SqlDatabase":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.SqlServerSource;
                        ProcessDBSource(ProcessSQLDBSource(CreateDbSource(_contextualResourceModel, WorkSurfaceContext.SqlServerSource)), workSurfaceKey);
                        break;
                    case "ODBC":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.OdbcSource;
                        ProcessDBSource(ProcessODBCDBSource(CreateDbSource(_contextualResourceModel, WorkSurfaceContext.OdbcSource)), workSurfaceKey);
                        break;
                    case "Oracle":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.OracleSource;
                        ProcessDBSource(ProcessOracleDBSource(CreateDbSource(_contextualResourceModel, WorkSurfaceContext.OracleSource)), workSurfaceKey);
                        break;
                    case "SqliteDatabase":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.SqliteSource;
                        ProcessDBSource(ProcessSqliteSource(CreateDbSource(_contextualResourceModel, WorkSurfaceContext.SqliteSource)), workSurfaceKey);
                        break;
                    case "PostgreSQL":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.PostgreSqlSource;
                        ProcessDBSource(ProcessPostgreSQLDBSource(CreateDbSource(_contextualResourceModel, WorkSurfaceContext.PostgreSqlSource)), workSurfaceKey);
                        break;
                    case "MySqlDatabase":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.MySqlSource;
                        ProcessDBSource(ProcessMySQLDBSource(CreateDbSource(_contextualResourceModel, WorkSurfaceContext.MySqlSource)), workSurfaceKey);
                        break;
                    case "EmailSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.EmailSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessEmailSource(_contextualResourceModel, workSurfaceKey));
                        break;
                    case "WebSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.WebSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessWebSource(_contextualResourceModel, workSurfaceKey));
                        break;
                    case "RedisSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.RedisSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessRedisSource(_contextualResourceModel, workSurfaceKey));
                        break;
                    case "ElasticsearchSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.ElasticsearchSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessElasticsearchSource(_contextualResourceModel, workSurfaceKey));
                        break;
                    case "ComPluginSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.ComPluginSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessComPluginSource(_contextualResourceModel, workSurfaceKey));
                        break;
                    case "ExchangeSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.Exchange;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessExchangeSource(_contextualResourceModel, workSurfaceKey));
                        break;
                    case "OauthSource":
                    case "DropBoxSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.OAuthSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessDropBoxSource(_contextualResourceModel, workSurfaceKey));
                        break;
                    case "Server":
                    case "Dev2Server":
                    case "ServerSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.ServerSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessServerSource(_contextualResourceModel, workSurfaceKey, environmentModel, activeServer));
                        break;
                    case "SharepointServerSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.SharepointServerSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessSharepointServerSource(_contextualResourceModel, workSurfaceKey));
                        break;
                    case "RabbitMQSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.RabbitMQSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessRabbitMQSource(_contextualResourceModel, workSurfaceKey));
                        break;
                    case "WcfSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.WcfSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessWcfSource(_contextualResourceModel, workSurfaceKey));
                        break;
                    case "PluginSource":
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.PluginSource;
                        _worksurfaceContextManager.DisplayResourceWizard(ProcessPluginSource(_contextualResourceModel, workSurfaceKey));
                        break;
                    default:
                        workSurfaceKey.WorkSurfaceContext = WorkSurfaceContext.Workflow;
                        _worksurfaceContextManager.DisplayResourceWizard(_contextualResourceModel);
                        break;
                }

                _contextualResourceModel = null;
            }
        }

        WorkSurfaceContextViewModel ProcessPluginSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new PluginSourceDefinition {Id = contextualResourceModel.ID, Path = contextualResourceModel.GetSavePath()};

            var pluginSourceViewModel = new ManagePluginSourceViewModel(
                new ManagePluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker);
            var vm = new SourceViewModel<IPluginSource>(EventPublisher, pluginSourceViewModel, PopupProvider, new ManagePluginSourceControl(), ActiveServer);

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        WorkSurfaceContextViewModel ProcessWcfSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new WcfServiceSourceDefinition {Id = contextualResourceModel.ID, Path = contextualResourceModel.GetSavePath()};

            var wcfSourceViewModel = new ManageWcfSourceViewModel(
                new ManageWcfSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker, ActiveServer);
            var vm = new SourceViewModel<IWcfServerSource>(EventPublisher, wcfSourceViewModel, PopupProvider, new ManageWcfSourceControl(), ActiveServer);

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        WorkSurfaceContextViewModel ProcessRabbitMQSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new RabbitMQServiceSourceDefinition {ResourceID = contextualResourceModel.ID, ResourcePath = contextualResourceModel.GetSavePath()};

            var viewModel = new ManageRabbitMQSourceViewModel(
                new ManageRabbitMQSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, this), def, AsyncWorker);
            var vm = new SourceViewModel<IRabbitMQServiceSourceDefinition>(EventPublisher, viewModel, PopupProvider, new ManageRabbitMQSourceControl(), ActiveServer);

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        WorkSurfaceContextViewModel ProcessSharepointServerSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new SharePointServiceSourceDefinition {Id = contextualResourceModel.ID, Path = contextualResourceModel.GetSavePath()};

            var viewModel = new SharepointServerSourceViewModel(
                new SharepointServerSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker, null);
            var vm = new SourceViewModel<ISharepointServerSource>(EventPublisher, viewModel, PopupProvider, new SharepointServerSource(), ActiveServer);

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        WorkSurfaceContextViewModel ProcessServerSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey, IServer server, IServer activeServer)
        {
            var selectedServer = new ServerSource {ID = contextualResourceModel.ID, ResourcePath = contextualResourceModel.GetSavePath()};

            var viewModel = new ManageNewServerViewModel(
                new ManageNewServerSourceModel(activeServer.UpdateRepository, activeServer.QueryProxy, server.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedServer, AsyncWorker, new ExternalProcessExecutor());
            var vm = new SourceViewModel<IServerSource>(EventPublisher, viewModel, PopupProvider, new ManageServerControl(), server);

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        WorkSurfaceContextViewModel ProcessDropBoxSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var db = new DropBoxSource {ResourceID = contextualResourceModel.ID, ResourcePath = contextualResourceModel.GetSavePath()};

            var oauthSourceViewModel = new ManageOAuthSourceViewModel(
                new ManageOAuthSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), db, AsyncWorker);
            var vm = new SourceViewModel<IOAuthSource>(EventPublisher, oauthSourceViewModel, PopupProvider, new ManageOAuthSourceControl(), ActiveServer);

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        WorkSurfaceContextViewModel ProcessExchangeSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new ExchangeSourceDefinition {ResourceID = contextualResourceModel.ID, Path = contextualResourceModel.GetSavePath()};

            var emailSourceViewModel = new ManageExchangeSourceViewModel(
                new ManageExchangeSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker);
            var vm = new SourceViewModel<IExchangeSource>(EventPublisher, emailSourceViewModel, PopupProvider, new ManageExchangeSourceControl(), ActiveServer);

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        WorkSurfaceContextViewModel ProcessComPluginSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new ComPluginSourceDefinition {Id = contextualResourceModel.ID, ResourcePath = contextualResourceModel.GetSavePath()};

            var wcfSourceViewModel = new ManageComPluginSourceViewModel(
                new ManageComPluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker);
            var vm = new SourceViewModel<IComPluginSource>(EventPublisher, wcfSourceViewModel, PopupProvider, new ManageComPluginSourceControl(), ActiveServer);

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        WorkSurfaceContextViewModel ProcessWebSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new WebServiceSourceDefinition {Id = contextualResourceModel.ID, Path = contextualResourceModel.GetSavePath()};

            var viewModel = new ManageWebserviceSourceViewModel(
                new ManageWebServiceSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker, new ExternalProcessExecutor());
            var vm = new SourceViewModel<IWebServiceSource>(EventPublisher, viewModel, PopupProvider, new ManageWebserviceSourceControl(), ActiveServer);

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        WorkSurfaceContextViewModel ProcessEmailSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new EmailServiceSourceDefinition {Id = contextualResourceModel.ID, Path = contextualResourceModel.GetSavePath()};

            var emailSourceViewModel = new ManageEmailSourceViewModel(
                new ManageEmailSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker);
            var vm = new SourceViewModel<IEmailServiceSource>(EventPublisher, emailSourceViewModel, PopupProvider, new ManageEmailSourceControl(), ActiveServer);

            var workSurfaceContextViewModel = _worksurfaceContextManager.EditResource(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        WorkSurfaceContextViewModel ProcessRedisSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new RedisSourceDefinition {Id = contextualResourceModel.ID, Path = contextualResourceModel.GetSavePath()};

            var viewModel = new RedisSourceViewModel(
                new RedisSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker, new ExternalProcessExecutor());
            var vm = new SourceViewModel<IRedisServiceSource>(EventPublisher, viewModel, PopupProvider, new RedisSourceControl(), ActiveServer);

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        WorkSurfaceContextViewModel ProcessElasticsearchSource(IContextualResourceModel contextualResourceModel, WorkSurfaceKey workSurfaceKey)
        {
            var def = new ElasticsearchSourceDefinition() {Id = contextualResourceModel.ID, Path = contextualResourceModel.GetSavePath()};

            var viewModel = new ElasticsearchSourceViewModel(
                new ElasticsearchSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
                new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker, new ExternalProcessExecutor(), ActiveServer);
            var vm = new SourceViewModel<IElasticsearchSourceDefinition>(EventPublisher, viewModel, PopupProvider, new ElasticsearchSourceControl(), ActiveServer);

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            return workSurfaceContextViewModel;
        }

        ManageMySqlSourceViewModel ProcessMySQLDBSource(IDbSource def) => new ManageMySqlSourceViewModel(
            new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
            new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker);

        ManagePostgreSqlSourceViewModel ProcessPostgreSQLDBSource(IDbSource def) => new ManagePostgreSqlSourceViewModel(
            new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
            new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker);

        ManageOracleSourceViewModel ProcessOracleDBSource(IDbSource def) => new ManageOracleSourceViewModel(
            new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
            new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker);

        ManageOdbcSourceViewModel ProcessODBCDBSource(IDbSource def) => new ManageOdbcSourceViewModel(
            new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
            new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker);

        ManageSqliteSourceViewModel ProcessSqliteSource(IDbSource def) => new ManageSqliteSourceViewModel(
            new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
            new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker);

        ManageSqlServerSourceViewModel ProcessSQLDBSource(IDbSource def) => new ManageSqlServerSourceViewModel(
            new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.DisplayName),
            new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), def, AsyncWorker);

        private static IDbSource CreateDbSource(IContextualResourceModel contextualResourceModel, WorkSurfaceContext workSurfaceContext)
        {
            var def = new DbSourceDefinition {Id = contextualResourceModel.ID, Path = contextualResourceModel.GetSavePath(), Type = ToenSourceType(workSurfaceContext)};
            return def;
        }

        void ProcessDBSource(DatabaseSourceViewModelBase dbSourceViewModel, IWorkSurfaceKey workSurfaceKey)
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

        private static enSourceType ToenSourceType(WorkSurfaceContext sqlServerSource)
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
                case WorkSurfaceContext.SqliteSource:
                    return enSourceType.SQLiteDatabase;
                default:
                    return enSourceType.Unknown;
            }
        }

        public void CopyUrlLink(Guid resourceId, IServer server)
        {
            GetCopyUrlLink(resourceId, server.EnvironmentID);
        }

        void GetCopyUrlLink(Guid resourceId, Guid environmentId)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            var contextualResourceModel = environmentModel?.ResourceRepository?.LoadContextualResourceModel(resourceId);
            var workflowUri = contextualResourceModel.GetWorkflowUri("", UrlType.Json, false);
            if (workflowUri != null)
            {
                Clipboard.SetText(workflowUri.ToString());
            }
        }

        public void ViewSwagger(Guid resourceId, IServer server)
        {
            ViewSwagger(resourceId, server.EnvironmentID);
        }

        void ViewSwagger(Guid resourceId, Guid environmentId)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            var contextualResourceModel = environmentModel?.ResourceRepository?.LoadContextualResourceModel(resourceId);
            var workflowUri = contextualResourceModel.GetWorkflowUri("", UrlType.API);
            if (workflowUri != null)
            {
                BrowserPopupController.ShowPopup(workflowUri.ToString());
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
            var contextualResourceModel = environmentModel?.ResourceRepository?.LoadContextualResourceModel(resourceId);
            _worksurfaceContextManager.TryCreateNewScheduleWorkSurface(contextualResourceModel);
        }

        public void CreateNewQueueEvent(Guid resourceId)
        {
            var environmentModel = ServerRepository.Get(ActiveServer.EnvironmentID);
            var contextualResourceModel = environmentModel?.ResourceRepository?.LoadContextualResourceModel(resourceId);
            _worksurfaceContextManager.TryCreateNewQueueEventWorkSurface(contextualResourceModel);
        }

        public void CreateTest(Guid resourceId)
        {
            var environmentModel = ServerRepository.Get(ActiveServer.EnvironmentID);
            var contextualResourceModel = environmentModel?.ResourceRepository?.LoadContextualResourceModel(resourceId);
            if (contextualResourceModel != null)
            {
                var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ServiceTestsViewer);
                workSurfaceKey.EnvironmentID = contextualResourceModel.Environment.EnvironmentID;
                workSurfaceKey.ResourceID = contextualResourceModel.ID;
                workSurfaceKey.ServerID = contextualResourceModel.ServerID;
                _worksurfaceContextManager.ViewTestsForService(contextualResourceModel, workSurfaceKey);
            }
        }

        public void OpenSelectedTest(Guid resourceId, string testName)
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

                    var loadTests = environmentModel.ResourceRepository.LoadResourceTests(resourceId);
                    var selectedTest = loadTests.FirstOrDefault(model => model.TestName.ToLower().Contains(testName.ToLower()));

                    if (selectedTest != null)
                    {
                        var workflow = new WorkflowDesignerViewModel(contextualResourceModel);
                        var testViewModel = new ServiceTestViewModel(contextualResourceModel, new AsyncWorker(), EventPublisher, new ExternalProcessExecutor(), workflow, CustomContainer.Get<IPopupController>());

                        var serviceTestModel = testViewModel.ToServiceTestModel(selectedTest);
                        _worksurfaceContextManager.ViewSelectedTestForService(contextualResourceModel, serviceTestModel, testViewModel, workSurfaceKey);
                        testViewModel.SelectedServiceTest = serviceTestModel;
                    }
                }
            }
        }

        public void RunCoverage(string resourcePath, Guid resourceId)
        {
            RunCoverage(resourcePath, resourceId, new ExternalProcessExecutor());
        }

        private void RunCoverage(string resourcePath, Guid resourceId, IExternalProcessExecutor processExecutor)
        {
            var environmentModel = ServerRepository.Get(ActiveServer.EnvironmentID);
            var contextualResourceModel = environmentModel?.ResourceRepository?.LoadContextualResourceModel(resourceId);

            if (contextualResourceModel != null)
            {
                _worksurfaceContextManager.RunAllTestCoverageForService(contextualResourceModel);
            }
            else
            {
                var secureResourcePath = environmentModel?.Connection.WebServerUri + "secure/" + resourcePath;
                _worksurfaceContextManager.RunAllTestCoverageForFolder(secureResourcePath, processExecutor);
            }
        }

        private ICommand _runAllTestsCommand;
        public ICommand RunAllTestsCommand => _runAllTestsCommand ?? (_runAllTestsCommand = new DelegateCommand(RunAllTests));

        private void RunAllTests(object explorerObj)
        {
            var resourcePath = "";
            var resourceId = Guid.Empty;
            switch (explorerObj)
            {
                case ExplorerItemViewModel explorerItem:
                    resourcePath = explorerItem.ResourcePath;
                    resourceId = explorerItem.ResourceId;
                    break;
                case EnvironmentViewModel environmentViewModel:
                    resourcePath = environmentViewModel.ResourcePath;
                    resourceId = environmentViewModel.ResourceId;
                    break;
                case WorkflowDesignerViewModel workflowDesignerViewModel:
                    resourcePath = workflowDesignerViewModel.ResourceModel.GetSavePath() + workflowDesignerViewModel.ResourceModel.DisplayName;
                    resourceId = workflowDesignerViewModel.ResourceModel.ID;
                    break;
            }

            RunAllTests(resourcePath, resourceId, new ExternalProcessExecutor());
        }

        public void RunAllTests(string resourcePath, Guid resourceId, IExternalProcessExecutor processExecutor)
        {
            var environmentModel = ServerRepository.Get(ActiveServer.EnvironmentID);
            var contextualResourceModel = environmentModel?.ResourceRepository?.LoadContextualResourceModel(resourceId);

            if (contextualResourceModel != null)
            {
                _worksurfaceContextManager.RunAllTestsForService(contextualResourceModel);
            }
            else
            {
                var secureResourcePath = environmentModel?.Connection.WebServerUri + "secure/" + resourcePath;
                _worksurfaceContextManager.RunAllTestsForFolder(secureResourcePath, processExecutor);
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

        public void CloseResourceMergeView(Guid resourceId, Guid serverId, Guid environmentId)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.MergeConflicts, resourceId, serverId, environmentId);
            var mergeViewModelForResource = FindWorkSurfaceContextViewModel(key);
            if (mergeViewModelForResource != null)
            {
                DeactivateItem(mergeViewModelForResource, true);
            }
        }

        IWorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(IWorkSurfaceKey key) => Items.FirstOrDefault(c => WorkSurfaceKeyEqualityComparerWithContextKey.Current.Equals(key, c.WorkSurfaceKey));

        public void CloseResource(IContextualResourceModel currentResourceModel, Guid environmentId)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            Close(currentResourceModel);
        }

        public void CloseResource(Guid resourceId, Guid environmentId)
        {
            var environmentModel = ServerRepository.Get(environmentId);
            var contextualResourceModel = environmentModel?.ResourceRepository?.LoadContextualResourceModel(resourceId);
            Close(contextualResourceModel);
        }

        private void Close(IContextualResourceModel contextualResourceModel)
        {
            if (contextualResourceModel != null)
            {
                var wfscvm = _worksurfaceContextManager.FindWorkSurfaceContextViewModel(contextualResourceModel);
                DeactivateItem(wfscvm, true);
            }
        }

        public async void OpenResourceAsync(Guid resourceId, IServer server)
        {
            var environmentModel = ServerRepository.Get(server.EnvironmentID);
            var contextualResourceModel = await environmentModel?.ResourceRepository?.LoadContextualResourceModelAsync(resourceId);
            _worksurfaceContextManager.DisplayResourceWizard(contextualResourceModel);
        }

        public void DeployResources(Guid sourceEnvironmentId, Guid destinationEnvironmentId, IList<Guid> resources, bool deployTests)
        {
            var environmentModel = ServerRepository.Get(destinationEnvironmentId);
            var sourceEnvironmentModel = ServerRepository.Get(sourceEnvironmentId);
            var dto = new DeployDto {ResourceModels = resources.Select(a => sourceEnvironmentModel.ResourceRepository.LoadContextualResourceModel(a) as IResourceModel).ToList(), DeployTests = deployTests};
            environmentModel?.ResourceRepository?.DeployResources(sourceEnvironmentModel, environmentModel, dto);
            ServerAuthorizationService.Instance.GetResourcePermissions(dto.ResourceModels.First().ID);
            ExplorerViewModel.RefreshEnvironment(destinationEnvironmentId);
        }

        public void ShowPopup(IPopupMessage getDuplicateMessage) => PopupProvider.Show(getDuplicateMessage.Description, getDuplicateMessage.Header, getDuplicateMessage.Buttons, MessageBoxImage.Error, "", false, true, false, false, false, false);

        public void EditSqlServerResource(IDbSource selectedSource) => EditSqlServerResource(selectedSource, null);

        public void EditSqlServerResource(IDbSource selectedSource, IWorkSurfaceKey key)
        {
            key = _worksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(key, WorkSurfaceContext.SqlServerSource, selectedSource.Id);
            ProcessDBSource(ProcessSQLDBSource(selectedSource), key);
        }

        public void EditMySqlResource(IDbSource selectedSource) => EditMySqlResource(selectedSource, null);

        public void EditMySqlResource(IDbSource selectedSource, IWorkSurfaceKey key)
        {
            key = _worksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(key, WorkSurfaceContext.MySqlSource, selectedSource.Id);
            ProcessDBSource(ProcessMySQLDBSource(selectedSource), key);
        }

        public void EditPostgreSqlResource(IDbSource selectedSource) => EditPostgreSqlResource(selectedSource, null);

        public void EditPostgreSqlResource(IDbSource selectedSource, IWorkSurfaceKey key)
        {
            key = _worksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(key, WorkSurfaceContext.PostgreSqlSource, selectedSource.Id);
            ProcessDBSource(ProcessPostgreSQLDBSource(selectedSource), key);
        }

        public void EditOracleResource(IDbSource selectedSource) => EditOracleResource(selectedSource, null);

        public void EditOracleResource(IDbSource selectedSource, IWorkSurfaceKey key)
        {
            key = _worksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(key, WorkSurfaceContext.OracleSource, selectedSource.Id);
            ProcessDBSource(ProcessOracleDBSource(selectedSource), key);
        }

        public void EditOdbcResource(IDbSource selectedSource) => EditOdbcResource(selectedSource, null);

        public void EditOdbcResource(IDbSource selectedSource, IWorkSurfaceKey key)
        {
            key = _worksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(key, WorkSurfaceContext.OdbcSource, selectedSource.Id);
            ProcessDBSource(ProcessODBCDBSource(selectedSource), key);
        }

        public void EditSqliteResource(IDbSource selectedSource) => EditSqliteResource(selectedSource, null);

        public void EditSqliteResource(IDbSource selectedSource, IWorkSurfaceKey key)
        {
            key = _worksurfaceContextManager.TryGetOrCreateWorkSurfaceKey(key, WorkSurfaceContext.SqliteSource, selectedSource.Id);
            ProcessDBSource(ProcessSqliteSource(selectedSource), key);
        }

        public void EditResource(IPluginSource selectedSource) => EditResource(selectedSource, null);

        public void EditResource(IPluginSource selectedSource, IWorkSurfaceKey key)
        {
            var view = _factory.GetViewGivenServerResourceType("PluginSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, key);
        }

        public void EditResource(IWebServiceSource selectedSource) => EditResource(selectedSource, null);

        public void EditResource(IWebServiceSource selectedSource, IWorkSurfaceKey key)
        {
            var view = _factory.GetViewGivenServerResourceType("WebSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, key);
        }

        public void EditResource(IEmailServiceSource selectedSource) => EditResource(selectedSource, null);

        public void EditResource(IEmailServiceSource selectedSource, IWorkSurfaceKey key)
        {
            var view = _factory.GetViewGivenServerResourceType("EmailSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, key);
        }

        public void EditResource(IExchangeSource selectedSource) => EditResource(selectedSource, null);

        public void EditResource(IExchangeSource selectedSource, IWorkSurfaceKey key)
        {
            var view = _factory.GetViewGivenServerResourceType("ExchangeSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, key);
        }

        public void EditResource(IElasticsearchSourceDefinition selectedSource, IWorkSurfaceKey key)
        {
            var view = _factory.GetViewGivenServerResourceType("ElasticsearchSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, key);
        }

        public void EditResource(IElasticsearchSourceDefinition selectedSource) => EditResource(selectedSource, null);
        public void EditResource(IRabbitMQServiceSourceDefinition selectedSource) => EditResource(selectedSource, null);

        public void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IWorkSurfaceKey key)
        {
            var view = _factory.GetViewGivenServerResourceType("RabbitMQSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, key);
        }

        public void EditResource(IWcfServerSource selectedSource) => EditResource(selectedSource, null);

        public void EditResource(IWcfServerSource selectedSource, IWorkSurfaceKey key)
        {
            var view = _factory.GetViewGivenServerResourceType("WcfSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, key);
        }

        public void EditResource(IComPluginSource selectedSource) => EditResource(selectedSource, null);

        public void EditResource(IComPluginSource selectedSource, IWorkSurfaceKey key)
        {
            var view = _factory.GetViewGivenServerResourceType("ComPluginSource");
            _worksurfaceContextManager.EditResource(selectedSource, view, key);
        }

        public void NewService(string resourcePath)
        {
            _worksurfaceContextManager.NewService(resourcePath);

            _applicationTracker?.TrackEvent(Warewolf.Studio.Resources.Languages.TrackEventMenu.EventCategory,
                Warewolf.Studio.Resources.Languages.TrackEventMenu.NewService);
        }

        public void NewServerSource(string resourcePath)
        {
            var saveViewModel = _worksurfaceContextManager.GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.ServerSourceNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ServerSource);
            key.ServerID = ActiveServer.ServerID;

            var manageNewServerSourceModel = new ManageNewServerSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name);
            var manageNewServerViewModel = new ManageNewServerViewModel(manageNewServerSourceModel, saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _asyncWorker, new ExternalProcessExecutor()) {SelectedGuid = key.ResourceID.Value};
            var workSurfaceViewModel = new SourceViewModel<IServerSource>(EventPublisher, manageNewServerViewModel, PopupProvider, new ManageServerControl(), ActiveServer);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, workSurfaceViewModel);
            _worksurfaceContextManager.AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewSqlServerSource(string resourcePath) => _worksurfaceContextManager.NewSqlServerSource(resourcePath);

        public void NewMySqlSource(string resourcePath) => _worksurfaceContextManager.NewMySqlSource(resourcePath);

        public void NewPostgreSqlSource(string resourcePath) => _worksurfaceContextManager.NewPostgreSqlSource(resourcePath);

        public void NewOracleSource(string resourcePath) => _worksurfaceContextManager.NewOracleSource(resourcePath);

        public void NewOdbcSource(string resourcePath) => _worksurfaceContextManager.NewOdbcSource(resourcePath);

        public void NewWebSource(string resourcePath) => _worksurfaceContextManager.NewWebSource(resourcePath);

        public void NewRedisSource(string resourcePath) => _worksurfaceContextManager.NewRedisSource(resourcePath);

        public void NewPluginSource(string resourcePath) => _worksurfaceContextManager.NewPluginSource(resourcePath);

        public void NewWcfSource(string resourcePath) => _worksurfaceContextManager.NewWcfSource(resourcePath);

        public void NewComPluginSource(string resourcePath) => _worksurfaceContextManager.NewComPluginSource(resourcePath);

        void ShowServerDisconnectedPopup()
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

        public void NewDropboxSource(string resourcePath) => _worksurfaceContextManager.NewDropboxSource(resourcePath);

        public void NewRabbitMQSource(string resourcePath) => _worksurfaceContextManager.NewRabbitMQSource(resourcePath);

        public void NewElasticsearchSource(string resourcePath) => _worksurfaceContextManager.NewElasticsearchSource(resourcePath);
        public void NewSharepointSource(string resourcePath) => _worksurfaceContextManager.NewSharepointSource(resourcePath);

        public void AddDeploySurface(IEnumerable<IExplorerTreeItem> items)
        {
            _applicationTracker?.TrackEvent(Warewolf.Studio.Resources.Languages.TrackEventMenu.EventCategory,
                Warewolf.Studio.Resources.Languages.TrackEventMenu.Deploy);

            _worksurfaceContextManager.AddDeploySurface(items);
        }

        public void OpenVersion(Guid resourceId, IVersionInfo versionInfo) => _worksurfaceContextManager.OpenVersion(resourceId, versionInfo);

        public async void ShowStartPageAsync()
        {
            _applicationTracker?.TrackEvent(Warewolf.Studio.Resources.Languages.TrackEventMenu.EventCategory, Warewolf.Studio.Resources.Languages.TrackEventMenu.StartPage);
            var workSurfaceContextViewModel = Items.FirstOrDefault(c => c.WorkSurfaceViewModel.DisplayName == "Start Page" && c.WorkSurfaceViewModel.GetType() == typeof(HelpViewModel));
            if (workSurfaceContextViewModel == null)
            {
                var helpViewModel = _worksurfaceContextManager.ActivateOrCreateUniqueWorkSurface<HelpViewModel>(WorkSurfaceContext.StartPage);
                await helpViewModel?.LoadBrowserUri(Version.CommunityPageUri);
            }
            else
            {
                ActivateItem(workSurfaceContextViewModel);
            }
        }

        public void ShowSearchWindow()
        {
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.SearchViewer);
            workSurfaceKey.EnvironmentID = ActiveServer.EnvironmentID;
            workSurfaceKey.ResourceID = Guid.Empty;
            workSurfaceKey.ServerID = ActiveServer.ServerID;
            _worksurfaceContextManager.SearchView(workSurfaceKey);
        }

        public void ShowCommunityPage() => BrowserPopupController.ShowPopup(StringResources.Uri_Community_HomePage);

        public bool IsActiveServerConnected()
        {
            if (ActiveServer == null)
            {
                return false;
            }

            var isActiveServerConnected = ActiveServer != null && ActiveServer.IsConnected && ActiveServer.CanStudioExecute && ShouldUpdateActiveState;
            if (ActiveServer.IsConnected && ShouldUpdateActiveState && ToolboxViewModel?.BackedUpTools != null && ToolboxViewModel.BackedUpTools.Count == 0)
            {
                ToolboxViewModel.BuildToolsList();
            }

            if (ToolboxViewModel != null)
            {
                ToolboxViewModel.IsVisible = isActiveServerConnected;
            }

            return isActiveServerConnected;
        }

        public void NewEmailSource(string resourcePath) => _worksurfaceContextManager.NewEmailSource(resourcePath);

        public void NewExchangeSource(string resourcePath) => _worksurfaceContextManager.NewExchangeSource(resourcePath);

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

        protected override void ChangeActiveItem(IWorkSurfaceContextViewModel newItem, bool closePrevious)
        {
            base.ChangeActiveItem(newItem, closePrevious);
            RefreshActiveServer();
        }

        public void BaseDeactivateItem(IWorkSurfaceContextViewModel item, bool close) => base.DeactivateItem(item, close);
        public bool DontPrompt { get; set; }

        public override void DeactivateItem(IWorkSurfaceContextViewModel item, bool close)
        {
            if (item == null)
            {
                return;
            }

            var success = true;
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

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                PersistTabs();
            }

            base.OnDeactivate(close);
        }

        protected override void OnActivationProcessed(IWorkSurfaceContextViewModel item, bool success)
        {
            if (success)
            {
                if (item?.WorkSurfaceViewModel is IWorkflowDesignerViewModel wfItem)
                {
                    _worksurfaceContextManager.AddWorkspaceItem(wfItem.ResourceModel);
                }

                if (item?.WorkSurfaceViewModel is StudioTestViewModel studioTestViewModel)
                {
                    var serviceTestViewModel = studioTestViewModel.ViewModel;
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

        public ICommand SaveAllAndCloseCommand => new DelegateCommand(SaveAllAndClose);

        public ICommand SaveAllCommand => new DelegateCommand(SaveAll);

        void SaveAll(object obj)
        {
            for (int index = Items.Count - 1; index >= 0; index--)
            {
                var workSurfaceContextViewModel = Items[index];
                var workSurfaceContext = workSurfaceContextViewModel.WorkSurfaceKey.WorkSurfaceContext;
                if (workSurfaceContext != WorkSurfaceContext.Help && workSurfaceContextViewModel.CanSave())
                {
                    workSurfaceContextViewModel.Save();
                }
            }
        }


        void SaveAllAndClose(object obj)
        {
            _continueShutDown = true;
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
                    _continueShutDown = false;
                    break;
                }
            }
        }

        bool _continueShutDown;

        public void ResetMainView()
        {
            var shellView = ShellView.GetInstance();
            shellView?.ResetToStartupView();
        }

        public void UpdateCurrentDataListWithObjectFromJson(string parentObjectName, string json)
        {
            ActiveItem?.DataListViewModel?.GenerateComplexObjectFromJson(parentObjectName, json);
        }

        public override void ActivateItem(IWorkSurfaceContextViewModel item)
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

        internal Action<IWorkSurfaceContextViewModel> ActiveItemChanged;

        bool ConfirmDeleteAfterDependencies(ICollection<IContextualResourceModel> models)
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

        bool ConfirmDelete(ICollection<IContextualResourceModel> models, string folderName)
        {
            var confirmDeleteAfterDependencies = ConfirmDeleteAfterDependencies(models);
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
                        return ShouldDelete(folderName, contextualResourceModel);
                    }
                }
            }

            return false;
        }

        private bool ShouldDelete(string folderName, IContextualResourceModel contextualResourceModel)
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
            var workflow = new WorkflowDesignerViewModel(resourceModel) {IsTestView = true};
            return workflow;
        }

        public void DeleteResources(ICollection<IContextualResourceModel> models, string folderName) => DeleteResources(models, folderName, true, null);

        public void DeleteResources(ICollection<IContextualResourceModel> models, string folderName, bool showConfirm) => DeleteResources(models, folderName, showConfirm, null);

        public void DeleteResources(ICollection<IContextualResourceModel> models, string folderName, bool showConfirm, System.Action actionToDoOnDelete)
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

        void SaveWorkspaceItems()
        {
            _getWorkspaceItemRepository().Write();
        }

        readonly Func<IWorkspaceItemRepository> _getWorkspaceItemRepository = () => WorkspaceItemRepository.Instance;

        protected virtual void AddWorkspaceItems(IPopupController popupController)
        {
            if (ServerRepository == null)
            {
                return;
            }

            var workspaceItemsToRemove = new HashSet<IWorkspaceItem>();
            for (int i = 0; i < _getWorkspaceItemRepository().WorkspaceItems.Count; i++)
            {
                var item = _getWorkspaceItemRepository().WorkspaceItems[i];
                Dev2Logger.Info($"Start Proccessing WorkspaceItem: {item.ServiceName}", GlobalConstants.WarewolfInfo);
                var environment = ServerRepository.All().Where(env => env.IsConnected).TakeWhile(env => env.Connection != null).FirstOrDefault(env => env.EnvironmentID == item.EnvironmentID);

                if (environment?.ResourceRepository == null)
                {
                    Dev2Logger.Info(@"Environment Not Found", GlobalConstants.WarewolfInfo);
                    if (environment != null && item.EnvironmentID == environment.EnvironmentID)
                    {
                        workspaceItemsToRemove.Add(item);
                    }
                }

                if (environment != null)
                {
                    Dev2Logger.Info($"Proccessing WorkspaceItem: {item.ServiceName} for Environment: {environment.DisplayName}", GlobalConstants.WarewolfInfo);
                    if (environment.ResourceRepository != null)
                    {
                        environment.ResourceRepository.LoadResourceFromWorkspace(item.ID, item.WorkspaceID);
                        var resource = environment.ResourceRepository?.All().FirstOrDefault(rm => { return EnvironmentContainsResourceModel(rm, item, environment); }) as IContextualResourceModel;
                        AddResourcesAsWorkSurfaceItem(workspaceItemsToRemove, item, environment, resource, popupController);
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

        private void AddResourcesAsWorkSurfaceItem(HashSet<IWorkspaceItem> workspaceItemsToRemove, IWorkspaceItem item, IServer environment, IContextualResourceModel resource, IPopupController popupController)
        {
            if (resource == null)
            {
                workspaceItemsToRemove.Add(item);
            }
            else
            {
                Dev2Logger.Info($"Got Resource Model: {resource.DisplayName} ", GlobalConstants.WarewolfInfo);
                var fetchResourceDefinition = environment.ResourceRepository.FetchResourceDefinition(environment, item.WorkspaceID, resource.ID, false);
                resource.WorkflowXaml = fetchResourceDefinition.Message;
                resource.IsWorkflowSaved = item.IsWorkflowSaved;
                resource.OnResourceSaved += model => _getWorkspaceItemRepository().UpdateWorkspaceItemIsWorkflowSaved(model);
                _worksurfaceContextManager.AddWorkSurfaceContextImpl(resource, true, popupController, _asyncWorker);
            }
        }

        private static bool EnvironmentContainsResourceModel(IResourceModel rm, IWorkspaceItem item, IServer environment)
        {
            var sameEnv = true;
            if (item.EnvironmentID != Guid.Empty)
            {
                sameEnv = item.EnvironmentID == environment.EnvironmentID;
            }

            return rm.ID == item.ID && sameEnv;
        }

        public bool IsWorkFlowOpened(IContextualResourceModel resource) => _worksurfaceContextManager.IsWorkFlowOpened(resource);

        public void AddWorkSurfaceContext(IContextualResourceModel resourceModel)
        {
            _worksurfaceContextManager.AddWorkSurfaceContext(resourceModel);
        }

        public void PersistTabs() => PersistTabs(false);

        public void PersistTabs(bool isStudioShutdown) => SaveAndShutdown(isStudioShutdown);

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

        bool CallSaveDialog(bool closeStudio)
        {
            var result = ShowUnsavedWorkDialog();
            // CANCEL - DON'T CLOSE THE STUDIO
            // NO - DON'T SAVE ANYTHING AND CLOSE THE STUDIO
            // YES - CALL THE SAVE ALL COMMAND AND CLOSE THE STUDIO
            if (result == MessageBoxResult.Cancel)
            {
                closeStudio = false;
            }
            else
            {
                if (result == MessageBoxResult.Yes)
                {
                    closeStudio = true;
                    SaveAllAndCloseCommand.Execute(null);
                    if (!_continueShutDown)
                    {
                        closeStudio = false;
                    }
                }
            }

            return closeStudio;
        }

        public bool OnStudioClosing()
        {
            var closeStudio = true;
            var workSurfaceContextViewModels = Items.ToList();
            foreach (IWorkSurfaceContextViewModel workSurfaceContextViewModel in workSurfaceContextViewModels)
            {
                var vm = workSurfaceContextViewModel.WorkSurfaceViewModel;
                if (vm == null || vm.WorkSurfaceContext == WorkSurfaceContext.Help)
                {
                    continue;
                }

                if (vm.WorkSurfaceContext == WorkSurfaceContext.Workflow)
                {
                    if (vm is WorkflowDesignerViewModel workflowDesignerViewModel && workflowDesignerViewModel.ResourceModel is IContextualResourceModel resourceModel && !resourceModel.IsWorkflowSaved)
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
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
                else if (vm.WorkSurfaceContext == WorkSurfaceContext.Triggers)
                {
                    if (vm is TriggersViewModel tasksViewModel && tasksViewModel.IsDirty)
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }
                }
                else
                {
                    if (vm.GetType().Name != "SourceViewModel`1")
                    {
                        continue;
                    }

                    // TODO: refactor to common interface possibly extracted from SourceViewModel
                    if (vm is SourceViewModel<IServerSource> serverSourceModel && (serverSourceModel.IsDirty || serverSourceModel.ViewModel.HasChanged))
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }

                    if (vm is SourceViewModel<IPluginSource> pluginSourceModel && (pluginSourceModel.IsDirty || pluginSourceModel.ViewModel.HasChanged))
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }

                    if (vm is SourceViewModel<IWcfServerSource> wcfServerSourceModel && (wcfServerSourceModel.IsDirty || wcfServerSourceModel.ViewModel.HasChanged))
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }

                    if (vm is SourceViewModel<IRabbitMQServiceSourceDefinition> rabbitMqServiceSourceModel && (rabbitMqServiceSourceModel.IsDirty || rabbitMqServiceSourceModel.ViewModel.HasChanged))
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }

                    if (vm is SourceViewModel<ISharepointServerSource> sharepointServerSourceModel && (sharepointServerSourceModel.IsDirty || sharepointServerSourceModel.ViewModel.HasChanged))
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }

                    if (vm is SourceViewModel<IOAuthSource> oAuthSourceModel && (oAuthSourceModel.IsDirty || oAuthSourceModel.ViewModel.HasChanged))
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }

                    if (vm is SourceViewModel<IExchangeSource> exchangeSourceModel && (exchangeSourceModel.IsDirty || exchangeSourceModel.ViewModel.HasChanged))
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }

                    if (vm is SourceViewModel<IComPluginSource> comPluginSourceModel && (comPluginSourceModel.IsDirty || comPluginSourceModel.ViewModel.HasChanged))
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }

                    if (vm is SourceViewModel<IWebServiceSource> webServiceSourceModel && (webServiceSourceModel.IsDirty || webServiceSourceModel.ViewModel.HasChanged))
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }

                    if (vm is SourceViewModel<IEmailServiceSource> emailServiceSourceModel && (emailServiceSourceModel.IsDirty || emailServiceSourceModel.ViewModel.HasChanged))
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }

                    if (vm is SourceViewModel<IDbSource> dbSourceModel && (dbSourceModel.IsDirty || dbSourceModel.ViewModel.HasChanged))
                    {
                        closeStudio = CallSaveDialog(closeStudio);
                        break;
                    }
                }
            }

            return closeStudio;
        }

        IMenuViewModel _menuViewModel;
        IServer _activeServer;
        IExplorerViewModel _explorerViewModel;
        IWorksurfaceContextManager _worksurfaceContextManager;

        public IWorksurfaceContextManager WorksurfaceContextManager
        {
            get => _worksurfaceContextManager;
            set { _worksurfaceContextManager = value; }
        }

        public IWorkflowDesignerViewModel GetWorkflowDesigner()
        {
            var workflowDesignerViewModel = ActiveItem?.WorkSurfaceViewModel as IWorkflowDesignerViewModel;
            return workflowDesignerViewModel;
        }

        public static bool IsDownloading() => false;

        public async Task<bool> CheckForNewVersionAsync()
        {
            var hasNewVersion = await Version.GetNewerVersionAsync();
            return hasNewVersion;
        }

        public void DisplayDialogForNewVersion() => BrowserPopupController.ShowPopup(Warewolf.Studio.Resources.Languages.Core.WarewolfLatestDownloadUrl);

        public bool MenuExpanded
        {
            get => _menuExpanded;
            set
            {
                _menuExpanded = value;
                NotifyOfPropertyChange(() => MenuExpanded);
            }
        }

        public IMenuViewModel MenuViewModel => _menuViewModel ?? (_menuViewModel = new MenuViewModel(this));

        public IToolboxViewModel ToolboxViewModel => CustomContainer.Get<IToolboxViewModel>();

        public IHelpWindowViewModel HelpViewModel => CustomContainer.Get<IHelpWindowViewModel>();

        public IWorkSurfaceContextViewModel PreviousActive
        {
            get => _previousActive;
            set { _previousActive = value; }
        }

        public IAsyncWorker AsyncWorker => _asyncWorker;

        public bool CanDebug
        {
            get => _canDebug;
            set { _canDebug = value; }
        }

        public Func<IWorkspaceItemRepository> GETWorkspaceItemRepository => _getWorkspaceItemRepository;

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

        public void UpdateExplorerWorkflowChanges(Guid resourceId)
        {
            if (ActiveServer.ResourceRepository.FindSingle(c => c.ID == resourceId, true) is IContextualResourceModel resource)
            {
                var key = WorkSurfaceKeyFactory.CreateKey(resource);
                var currentContext = FindWorkSurfaceContextViewModel(key);
                var vm = currentContext?.WorkSurfaceViewModel as IWorkflowDesignerViewModel;
                if (vm != null)
                {
                    vm.CanMerge = true;
                }
            }
        }

        public IResource CreateResourceFromStreamContent(string resourceContent) => new Resource(resourceContent.ToStringBuilder().ToXElement());
    }
}