using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.SaveDialog;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Data.ServiceModel;
using Dev2.Factory;
using Dev2.Instrumentation;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Settings;
using Dev2.Settings.Scheduler;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Core.ViewModels;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.DependencyVisualization;
using Dev2.Threading;
using Dev2.Utils;
using Dev2.ViewModels;
using Infragistics.Windows.DockManager.Events;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
// ReSharper disable InconsistentNaming
// ReSharper disable NonLocalizedString
// ReSharper disable CheckNamespace

namespace Dev2.Studio.ViewModels
{
    public interface IWorksurfaceContextManager
    {
        void Handle(RemoveResourceAndCloseTabMessage message);
        void Handle(NewTestFromDebugMessage message, IWorkSurfaceKey workSurfaceKey = null);
        void EditServer(IServerSource selectedServer, IServer activeServer);
        void EditSqlServerResource(IDbSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditMySqlResource(IDbSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditPostgreSqlResource(IDbSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditOracleResource(IDbSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditOdbcResource(IDbSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditResource(IPluginSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditResource(IComPluginSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditResource(IWebServiceSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditResource(IEmailServiceSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditResource(IExchangeSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditResource(IWcfServerSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void NewService(string resourcePath);
        void NewSqlServerSource(string resourcePath);
        void NewMySqlSource(string resourcePath);
        void NewPostgreSqlSource(string resourcePath);
        void NewOracleSource(string resourcePath);
        void NewOdbcSource(string resourcePath);
        bool DuplicateResource(IExplorerItemViewModel explorerItemViewModel);
        void NewWebSource(string resourcePath);
        void NewPluginSource(string resourcePath);
        void NewComPluginSource(string resourcePath);
        void NewWcfSource(string resourcePath);
        void NewDropboxSource(string resourcePath);
        void NewRabbitMQSource(string resourcePath);
        void NewSharepointSource(string resourcePath);
        void AddDeploySurface(IEnumerable<IExplorerTreeItem> items);
        void OpenVersion(Guid resourceId, IVersionInfo versionInfo);
        void NewEmailSource(string resourcePath);
        void NewExchangeSource(string resourcePath);
        bool IsWorkFlowOpened(IContextualResourceModel resource);
        void AddWorkSurfaceContext(IContextualResourceModel resourceModel);
        void ShowDependencies(bool dependsOnMe, IContextualResourceModel model, IServer server);
        void DisplayResourceWizard(IContextualResourceModel resourceModel);
        void DisplayResourceWizard(IWorkSurfaceContextViewModel contextViewModel);
        void EditSqlServerSource(IContextualResourceModel resourceModel);
        void EditMySqlSource(IContextualResourceModel resourceModel);
        void EditPostgreSqlSource(IContextualResourceModel resourceModel);
        void EditOracleSource(IContextualResourceModel resourceModel);
        void EditOdbcSource(IContextualResourceModel resourceModel);
        void EditPluginSource(IContextualResourceModel resourceModel);
        void EditComPluginSource(IContextualResourceModel resourceModel);
        void EditWebSource(IContextualResourceModel resourceModel);
        void EditSharePointSource(IContextualResourceModel resourceModel);
        void EditEmailSource(IContextualResourceModel resourceModel);
        void EditExchangeSource(IContextualResourceModel resourceModel);
        void EditDropBoxSource(IContextualResourceModel resourceModel);
        void EditRabbitMQSource(IContextualResourceModel resourceModel);
        void EditWcfSource(IContextualResourceModel resourceModel);
        void EditServer(IContextualResourceModel resourceModel);
        void EditResource(IOAuthSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        void EditResource(ISharepointServerSource selectedSource, IWorkSurfaceKey workSurfaceKey = null);
        Task<IRequestServiceNameViewModel> GetSaveViewModel(string resourcePath, string header, IExplorerItemViewModel explorerItemViewModel = null);
        void AddReverseDependencyVisualizerWorkSurface(IContextualResourceModel resource);
        void AddSettingsWorkSurface();
        void AddSchedulerWorkSurface();
        void CreateNewScheduleWorkSurface(IContextualResourceModel resourceModel);

        void AddWorkspaceItem(IContextualResourceModel model);
        void DeleteContext(IContextualResourceModel model);
        T ActivateOrCreateUniqueWorkSurface<T>(WorkSurfaceContext context) where T : IWorkSurfaceViewModel;
        WorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(IContextualResourceModel resource);
        void AddWorkSurfaceContextImpl(IContextualResourceModel resourceModel, bool isLoadingWorkspace);
        void AddAndActivateWorkSurface(WorkSurfaceContextViewModel context);
        void AddWorkSurface(IWorkSurfaceObject obj);
        bool CloseWorkSurfaceContext(WorkSurfaceContextViewModel context, PaneClosingEventArgs e, bool dontPrompt = false);
        void ViewTestsForService(IContextualResourceModel resourceModel, IWorkSurfaceKey workSurfaceKey = null);
        void RunAllTestsForService(IContextualResourceModel resourceModel);
        WorkSurfaceContextViewModel EditResource<T>(IWorkSurfaceKey workSurfaceKey, SourceViewModel<T> viewModel) where T : IEquatable<T>;

        IWorkSurfaceKey TryGetOrCreateWorkSurfaceKey(IWorkSurfaceKey workSurfaceKey, WorkSurfaceContext workSurfaceContext, Guid resourceID);
    }

    public class WorksurfaceContextManager : IWorksurfaceContextManager
    {
        private readonly MainViewModel _mainViewModel;
        private readonly bool _createDesigners;
        private readonly Func<IContextualResourceModel, bool, IWorkSurfaceContextViewModel> _getWorkSurfaceContextViewModel = (resourceModel, createDesigner) => WorkSurfaceContextFactory.CreateResourceViewModel(resourceModel, createDesigner);

        public WorksurfaceContextManager(bool createDesigners, MainViewModel mainViewModel)
        {
            _createDesigners = createDesigners;
            _mainViewModel = mainViewModel;
        }

        public void Handle(RemoveResourceAndCloseTabMessage message)
        {
            Dev2Logger.Debug(message.GetType().Name);
            if (message.ResourceToRemove == null)
            {
                return;
            }

            var wfscvm = FindWorkSurfaceContextViewModel(message.ResourceToRemove);
            if (message.RemoveFromWorkspace)
            {
                _mainViewModel.DeactivateItem(wfscvm, true);
            }
            else
            {
                _mainViewModel.BaseDeactivateItem(wfscvm, true);
            }

            _mainViewModel.PreviousActive = null;

        }

        public void Handle(NewTestFromDebugMessage message, IWorkSurfaceKey workSurfaceKey = null)
        {
            Dev2Logger.Debug(message.GetType().Name);
            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.ServiceTestsViewer, message.ResourceID);
            var found = FindWorkSurfaceContextViewModel(workSurfaceKey as WorkSurfaceKey);
            if (found != null)
            {
                var vm = found.WorkSurfaceViewModel;
                if (vm != null)
                {
                    var studioTestViewModel = vm as StudioTestViewModel;
                    var serviceTestViewModel = studioTestViewModel?.ViewModel;
                    serviceTestViewModel?.PrepopulateTestsUsingDebug(message.RootItems);
                }
                AddAndActivateWorkSurface(found);
            }
            else
            {
                var workflow = new WorkflowDesignerViewModel(message.ResourceModel);
                var testViewModel = new ServiceTestViewModel(message.ResourceModel, new AsyncWorker(), _mainViewModel.EventPublisher, new ExternalProcessExecutor(), workflow, message);
                var vm = new StudioTestViewModel(_mainViewModel.EventPublisher, testViewModel, _mainViewModel.PopupProvider, new ServiceTestView());
                var key = workSurfaceKey as WorkSurfaceKey;
                var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
                AddAndActivateWorkSurface(workSurfaceContextViewModel);
            }
        }

        IServer ActiveServer => _mainViewModel.ActiveServer;

        public void EditServer(IServerSource selectedServer, IServer activeServer)
        {
            var environmentModel = EnvironmentRepository.Instance.Get(activeServer.EnvironmentID);
            var viewModel = new ManageNewServerViewModel(new ManageNewServerSourceModel(activeServer.UpdateRepository, activeServer.QueryProxy, activeServer.DisplayName), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedServer, _mainViewModel.AsyncWorker, new ExternalProcessExecutor());
            var vm = new SourceViewModel<IServerSource>(_mainViewModel.EventPublisher, viewModel, _mainViewModel.PopupProvider, new ManageServerControl(), environmentModel);

            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ServerSource);
            workSurfaceKey.EnvironmentID = activeServer.EnvironmentID;
            workSurfaceKey.ResourceID = selectedServer.ID;
            workSurfaceKey.ServerID = activeServer.ServerID;
            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);

            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public WorkSurfaceContextViewModel EditResource<T>(IWorkSurfaceKey workSurfaceKey, SourceViewModel<T> viewModel) where T : IEquatable<T>
        {
            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, viewModel);
            OpeningWorkflowsHelper.AddWorkflow(key);
            return workSurfaceContextViewModel;
        }

        public void EditSqlServerResource(IDbSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var dbSourceViewModel = new ManageSqlServerSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name)
                    , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                    , selectedSource, _mainViewModel.AsyncWorker);
            var vm = new SourceViewModel<IDbSource>(_mainViewModel.EventPublisher, dbSourceViewModel, _mainViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveEnvironment);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.SqlServerSource, selectedSource.Id);

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditMySqlResource(IDbSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var dbSourceViewModel = new ManageMySqlSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, "")
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , selectedSource, _mainViewModel.AsyncWorker);
            var vm = new SourceViewModel<IDbSource>(_mainViewModel.EventPublisher, dbSourceViewModel, _mainViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveEnvironment);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.MySqlSource, selectedSource.Id);

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditPostgreSqlResource(IDbSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var dbSourceViewModel = new ManagePostgreSqlSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _mainViewModel.AsyncWorker);
            var vm = new SourceViewModel<IDbSource>(_mainViewModel.EventPublisher, dbSourceViewModel, _mainViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveEnvironment);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.PostgreSqlSource, selectedSource.Id);

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditOracleResource(IDbSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var dbSourceViewModel = new ManageOracleSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, "Oracle"), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _mainViewModel.AsyncWorker);
            var vm = new SourceViewModel<IDbSource>(_mainViewModel.EventPublisher, dbSourceViewModel, _mainViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveEnvironment);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.OracleSource, selectedSource.Id);

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditOdbcResource(IDbSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var dbSourceViewModel = new ManageOdbcSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _mainViewModel.AsyncWorker);
            var vm = new SourceViewModel<IDbSource>(_mainViewModel.EventPublisher, dbSourceViewModel, _mainViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveEnvironment);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.OdbcSource, selectedSource.Id);

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public IWorkSurfaceKey TryGetOrCreateWorkSurfaceKey(IWorkSurfaceKey workSurfaceKey, WorkSurfaceContext workSurfaceContext, Guid resourceID)
        {
            if (workSurfaceKey == null)
            {
                workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(workSurfaceContext);
                workSurfaceKey.EnvironmentID = ActiveServer.EnvironmentID;
                workSurfaceKey.ResourceID = resourceID;
                workSurfaceKey.ServerID = ActiveServer.ServerID;
            }
            return workSurfaceKey;
        }

        public void ViewTestsForService(IContextualResourceModel resourceModel, IWorkSurfaceKey workSurfaceKey = null)
        {
            var workflow = new WorkflowDesignerViewModel(resourceModel);
            var testViewModel = new ServiceTestViewModel(resourceModel, new AsyncWorker(), _mainViewModel.EventPublisher, new ExternalProcessExecutor(), workflow);
            var vm = new StudioTestViewModel(_mainViewModel.EventPublisher, testViewModel, _mainViewModel.PopupProvider, new ServiceTestView());
            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.ServiceTestsViewer, resourceModel.ID);
            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void RunAllTestsForService(IContextualResourceModel resourceModel)
        {
            var workflow = new WorkflowDesignerViewModel(resourceModel);
            using (var testViewModel = new ServiceTestViewModel(resourceModel, new AsyncWorker(), _mainViewModel.EventPublisher, new ExternalProcessExecutor(), workflow))
            {
                testViewModel.RunAllTestsInBrowserCommand.Execute(null);
            }
        }
        
        public void EditResource(IPluginSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var pluginSourceViewModel = new ManagePluginSourceViewModel(new ManagePluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _mainViewModel.AsyncWorker);
            var vm = new SourceViewModel<IPluginSource>(_mainViewModel.EventPublisher, pluginSourceViewModel, _mainViewModel.PopupProvider, new ManagePluginSourceControl(), ActiveEnvironment);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.PluginSource, selectedSource.Id);


            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }
        public void EditResource(IComPluginSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var wcfSourceViewModel = new ManageComPluginSourceViewModel(new ManageComPluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _mainViewModel.AsyncWorker);
            var vm = new SourceViewModel<IComPluginSource>(_mainViewModel.EventPublisher, wcfSourceViewModel, _mainViewModel.PopupProvider, new ManageComPluginSourceControl(), ActiveEnvironment);
            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.ComPluginSource, selectedSource.Id);
            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }
        public void EditResource(IWebServiceSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var viewModel = new ManageWebserviceSourceViewModel(new ManageWebServiceSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _mainViewModel.AsyncWorker, new ExternalProcessExecutor());
            var vm = new SourceViewModel<IWebServiceSource>(_mainViewModel.EventPublisher, viewModel, _mainViewModel.PopupProvider, new ManageWebserviceSourceControl(), ActiveEnvironment);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.WebSource, selectedSource.Id);

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IEmailServiceSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var emailSourceViewModel = new ManageEmailSourceViewModel(new ManageEmailSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _mainViewModel.AsyncWorker);
            var vm = new SourceViewModel<IEmailServiceSource>(_mainViewModel.EventPublisher, emailSourceViewModel, _mainViewModel.PopupProvider, new ManageEmailSourceControl(), ActiveEnvironment);


            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.EmailSource, selectedSource.Id);

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IExchangeSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var emailSourceViewModel = new ManageExchangeSourceViewModel(new ManageExchangeSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _mainViewModel.AsyncWorker);
            var vm = new SourceViewModel<IExchangeSource>(_mainViewModel.EventPublisher, emailSourceViewModel, _mainViewModel.PopupProvider, new ManageExchangeSourceControl(), ActiveEnvironment);


            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.Exchange, selectedSource.ResourceID);


            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var viewModel = new ManageRabbitMQSourceViewModel(new ManageRabbitMQSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, _mainViewModel), selectedSource, _mainViewModel.AsyncWorker);
            var vm = new SourceViewModel<IRabbitMQServiceSourceDefinition>(_mainViewModel.EventPublisher, viewModel, _mainViewModel.PopupProvider, new ManageRabbitMQSourceControl(), ActiveEnvironment);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.RabbitMQSource, selectedSource.ResourceID);

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IWcfServerSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var wcfSourceViewModel = new ManageWcfSourceViewModel(new ManageWcfSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _mainViewModel.AsyncWorker, ActiveEnvironment);
            var vm = new SourceViewModel<IWcfServerSource>(_mainViewModel.EventPublisher, wcfSourceViewModel, _mainViewModel.PopupProvider, new ManageWcfSourceControl(), ActiveEnvironment);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.WcfSource, selectedSource.ResourceID);

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }



        IEnvironmentModel ActiveEnvironment => _mainViewModel.ActiveEnvironment;

        public void NewService(string resourcePath)
        {
            TempSave(ActiveEnvironment, resourcePath);
        }

        public void NewSqlServerSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.SqlServerSourceNewHeaderLabel);
            var key = (WorkSurfaceKey)WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.SqlServerSource);
            key.ServerID = ActiveServer.ServerID;
            // ReSharper disable once PossibleInvalidOperationException
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IDbSource>(_mainViewModel.EventPublisher, new ManageSqlServerSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _mainViewModel.AsyncWorker)
            {
                SelectedGuid = key.ResourceID.Value
            }
            , _mainViewModel.PopupProvider
                , new ManageDatabaseSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }
        public void NewMySqlSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.MySqlSourceNewHeaderLabel);
            var key = (WorkSurfaceKey)WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.MySqlSource);
            key.ServerID = ActiveServer.ServerID;
            // ReSharper disable once PossibleInvalidOperationException
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IDbSource>(_mainViewModel.EventPublisher, new ManageMySqlSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _mainViewModel.AsyncWorker)
            {
                SelectedGuid = key.ResourceID.Value
            }, _mainViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }
        public void NewPostgreSqlSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.PostgreSqlSourceNewHeaderLabel);
            var key = (WorkSurfaceKey)WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.PostgreSqlSource);
            key.ServerID = ActiveServer.ServerID;
            // ReSharper disable once PossibleInvalidOperationException
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key,
                new SourceViewModel<IDbSource>(_mainViewModel.EventPublisher, new ManagePostgreSqlSourceViewModel(
                                                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name)
                                                , saveViewModel
                                                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                                                , _mainViewModel.AsyncWorker)
                {
                    SelectedGuid = key.ResourceID.Value
                }
                , _mainViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }
        public void NewOracleSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.OracleSourceNewHeaderLabel);
            var key = (WorkSurfaceKey)WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.OracleSource);
            key.ServerID = ActiveServer.ServerID;
            // ReSharper disable once PossibleInvalidOperationException
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IDbSource>(_mainViewModel.EventPublisher, new ManageOracleSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name)
                , saveViewModel
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , _mainViewModel.AsyncWorker)
            {
                SelectedGuid = key.ResourceID.Value
            }, _mainViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }
        public void NewOdbcSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.OdbcSourceNewHeaderLabel);
            var key = (WorkSurfaceKey)WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.OdbcSource);
            key.ServerID = ActiveServer.ServerID;
            // ReSharper disable once PossibleInvalidOperationException
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IDbSource>(_mainViewModel.EventPublisher, new ManageOdbcSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name)
                , saveViewModel
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , _mainViewModel.AsyncWorker)
            { SelectedGuid = key.ResourceID.Value }, _mainViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }
        public bool DuplicateResource(IExplorerItemViewModel explorerItemViewModel)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(string.Empty, explorerItemViewModel.ResourceName, explorerItemViewModel);
            var messageBoxResult = saveViewModel.Result.ShowSaveDialog();
            return messageBoxResult == MessageBoxResult.OK;
        }

        public void NewWebSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.WebserviceNewHeaderLabel);
            var key = (WorkSurfaceKey)WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.WebSource);
            key.ServerID = ActiveServer.ServerID;
            // ReSharper disable once PossibleInvalidOperationException
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IWebServiceSource>(_mainViewModel.EventPublisher, new ManageWebserviceSourceViewModel(new ManageWebServiceSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _mainViewModel.AsyncWorker, new ExternalProcessExecutor()) { SelectedGuid = key.ResourceID.Value }, _mainViewModel.PopupProvider, new ManageWebserviceSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewPluginSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.PluginSourceNewHeaderLabel);
            var key = (WorkSurfaceKey)WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.PluginSource);
            key.ServerID = ActiveServer.ServerID;
            // ReSharper disable once PossibleInvalidOperationException
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IPluginSource>(_mainViewModel.EventPublisher, new ManagePluginSourceViewModel(new ManagePluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name),
                saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _mainViewModel.AsyncWorker)
            { SelectedGuid = key.ResourceID.Value }, _mainViewModel.PopupProvider, new ManagePluginSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewComPluginSource(string resourcePath)
        {

            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.ComPluginSourceNewHeaderLabel);
            var key = (WorkSurfaceKey)WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ComPluginSource);
            key.ServerID = ActiveServer.ServerID;
            // ReSharper disable once PossibleInvalidOperationException
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IComPluginSource>(_mainViewModel.EventPublisher, new ManageComPluginSourceViewModel(new ManageComPluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name),
                saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _mainViewModel.AsyncWorker)
            { SelectedGuid = key.ResourceID.Value }, _mainViewModel.PopupProvider, new ManageComPluginSourceControl(), ActiveEnvironment));

            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewWcfSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.WcfServiceNewHeaderLabel);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.WcfSource) as WorkSurfaceKey, new SourceViewModel<IWcfServerSource>(_mainViewModel.EventPublisher, new ManageWcfSourceViewModel(new ManageWcfSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _mainViewModel.AsyncWorker, ActiveEnvironment), _mainViewModel.PopupProvider, new ManageWcfSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewDropboxSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.DropboxSourceNewHeaderLabel);
            var key = (WorkSurfaceKey)WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.OAuthSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IOAuthSource>(_mainViewModel.EventPublisher, new ManageOAuthSourceViewModel(new ManageOAuthSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name), saveViewModel), _mainViewModel.PopupProvider, new ManageOAuthSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewRabbitMQSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.RabbitMQSourceNewHeaderLabel);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.RabbitMQSource) as WorkSurfaceKey, new SourceViewModel<IRabbitMQServiceSourceDefinition>(_mainViewModel.EventPublisher, new ManageRabbitMQSourceViewModel(new ManageRabbitMQSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, _mainViewModel), saveViewModel), _mainViewModel.PopupProvider, new ManageRabbitMQSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewSharepointSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.SharePointServiceNewHeaderLabel);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.SharepointServerSource) as WorkSurfaceKey, new SourceViewModel<ISharepointServerSource>(_mainViewModel.EventPublisher, new SharepointServerSourceViewModel(new SharepointServerSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _mainViewModel.AsyncWorker, ActiveEnvironment), _mainViewModel.PopupProvider, new SharepointServerSource(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void AddDeploySurface(IEnumerable<IExplorerTreeItem> items)
        {
            var dep = ActivateOrCreateUniqueWorkSurface<DeployWorksurfaceViewModel>(WorkSurfaceContext.DeployViewer);
            if (dep != null)
            {

                dep.ViewModel.Source.Preselected = new ObservableCollection<IExplorerTreeItem>(items);

            }
        }

        public void OpenVersion(Guid resourceId, IVersionInfo versionInfo)
        {
            var workflowXaml = ActiveServer?.ProxyLayer?.GetVersion(versionInfo, resourceId);
            if (workflowXaml != null)
            {
                IResourceModel resourceModel =
                    ActiveEnvironment.ResourceRepository.LoadContextualResourceModel(resourceId);

                var xamlElement = XElement.Parse(workflowXaml.ToString());
                var dataList = xamlElement.Element(@"DataList");
                var dataListString = string.Empty;
                if (dataList != null)
                {
                    dataListString = dataList.ToString();
                }
                var action = xamlElement.Element(@"Action");
                var xamlString = string.Empty;
                var xaml = action?.Element(@"XamlDefinition");
                if (xaml != null)
                {
                    xamlString = xaml.Value;
                }

                var resource = new Resource(workflowXaml.ToXElement());
                if (resource.VersionInfo != null)
                {
                    versionInfo.User = resource.VersionInfo.User;
                }

                var resourceVersion = new ResourceModel(ActiveEnvironment, EventPublishers.Aggregator)
                {
                    ResourceType = resourceModel.ResourceType,
                    ResourceName = $"{resource.ResourceName} (v.{versionInfo.VersionNumber})",
                    WorkflowXaml = new StringBuilder(xamlString),
                    UserPermissions = Permissions.View,
                    DataList = dataListString,
                    IsVersionResource = true,
                    ID = Guid.NewGuid()
                };

                DisplayResourceWizard(resourceVersion);
            }
        }

        public void NewEmailSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.EmailSourceNewHeaderLabel);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.EmailSource) as WorkSurfaceKey, new SourceViewModel<IEmailServiceSource>(_mainViewModel.EventPublisher, new ManageEmailSourceViewModel(new ManageEmailSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()), _mainViewModel.PopupProvider, new ManageEmailSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);

        }

        public void NewExchangeSource(string resourcePath)
        {
            Task<IRequestServiceNameViewModel> saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.EmailSourceNewHeaderLabel);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Exchange) as WorkSurfaceKey, new SourceViewModel<IExchangeSource>(_mainViewModel.EventPublisher, new ManageExchangeSourceViewModel(new ManageExchangeSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveEnvironment.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()), _mainViewModel.PopupProvider, new ManageExchangeSourceControl(), ActiveEnvironment));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public bool IsWorkFlowOpened(IContextualResourceModel resource)
        {
            return FindWorkSurfaceContextViewModel(resource) != null;
        }

        public void AddWorkSurfaceContext(IContextualResourceModel resourceModel)
        {
            AddWorkSurfaceContextImpl(resourceModel, false);
        }

        public void ShowDependencies(bool dependsOnMe, IContextualResourceModel model, IServer server)
        {
            var vm = new DependencyVisualiserViewModel(new DependencyVisualiserView(), server, dependsOnMe)
            {
                ResourceType = @"DependencyViewer",
                ResourceModel = model
            };

            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.DependencyVisualiser);
            workSurfaceKey.EnvironmentID = model.Environment.ID;
            workSurfaceKey.ResourceID = model.ID;
            workSurfaceKey.ServerID = model.ServerID;

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        private void TempSave(IEnvironmentModel activeEnvironment, string resourcePath)
        {
            string newWorflowName = NewWorkflowNames.Instance.GetNext();

            IContextualResourceModel tempResource = ResourceModelFactory.CreateResourceModel(activeEnvironment, @"WorkflowService",
                newWorflowName);
            tempResource.Category = string.IsNullOrEmpty(resourcePath) ? @"Unassigned\" + newWorflowName : resourcePath + @"\" + newWorflowName;
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;

            AddAndActivateWorkSurface(WorkSurfaceContextFactory.CreateResourceViewModel(tempResource));
            AddWorkspaceItem(tempResource);
        }

        private bool ShowRemovePopup(IWorkflowDesignerViewModel workflowVm)
        {
            var result = _mainViewModel.PopupProvider.Show(string.Format(StringResources.DialogBody_NotSaved, workflowVm.ResourceModel.ResourceName),
                $"Save {workflowVm.ResourceModel.ResourceName}?",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Information, @"", false, false, true, false, false, false);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    workflowVm.ResourceModel.Commit();
                    Dev2Logger.Info(@"Publish message of type - " + typeof(SaveResourceMessage));
                    _mainViewModel.EventPublisher.Publish(new SaveResourceMessage(workflowVm.ResourceModel, false, false));
                    return !workflowVm.WorkflowName.ToLower().StartsWith(@"unsaved");
                case MessageBoxResult.No:
                    var model = workflowVm.ResourceModel;
                    try
                    {
                        if (workflowVm.EnvironmentModel.ResourceRepository.DoesResourceExistInRepo(model) && workflowVm.ResourceModel.IsNewWorkflow)
                        {
                            _mainViewModel.DeleteResources(new List<IContextualResourceModel> { model }, @"", false);
                        }
                        else
                        {
                            model.Rollback();
                        }
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Info(@"Exception: " + e.Message);
                    }

                    NewWorkflowNames.Instance.Remove(workflowVm.ResourceModel.ResourceName);
                    return true;
                default:
                    return false;
            }
        }


        public void DisplayResourceWizard(IWorkSurfaceContextViewModel contextViewModel)
        {
            AddAndActivateWorkSurface(contextViewModel as WorkSurfaceContextViewModel);
        }


        public void DisplayResourceWizard(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null)
            {
                return;
            }

            if (resourceModel.ServerResourceType == ResourceType.WorkflowService.ToString())
            {
                _mainViewModel.PersistTabs();
                AddWorkSurfaceContext(resourceModel);
                return;
            }
            switch (resourceModel.ServerResourceType)
            {
                case "SqlDatabase":
                    EditSqlServerSource(resourceModel);
                    break;
                case "ODBC":
                    EditOdbcSource(resourceModel);
                    break;
                case "Oracle":
                    EditOracleSource(resourceModel);
                    break;
                case "PostgreSQL":
                    EditPostgreSqlSource(resourceModel);
                    break;
                case "MySqlDatabase":
                    EditMySqlSource(resourceModel);
                    break;
                case "WebSource":
                    EditWebSource(resourceModel);
                    break;
                case "PluginSource":
                    EditPluginSource(resourceModel);
                    break;
                case "ComPluginSource":
                    EditComPluginSource(resourceModel);
                    break;
                case "WcfSource":
                    EditWcfSource(resourceModel);
                    break;
                case "EmailSource":
                    EditEmailSource(resourceModel);
                    break;
                case "ExchangeSource":
                    EditExchangeSource(resourceModel);
                    break;
                case "DropBoxSource":
                    EditDropBoxSource(resourceModel);
                    break;
                case "SharepointServerSource":
                    EditSharePointSource(resourceModel);
                    break;
                case "OauthSource":
                    EditDropBoxSource(resourceModel);
                    break;
                case "RabbitMQSource":
                    EditRabbitMQSource(resourceModel);
                    break;
                case "Server":
                case "Dev2Server":
                case "ServerSource":
                    EditServer(resourceModel);
                    break;
                default:
                    AddWorkSurfaceContext(resourceModel);
                    break;
            }
        }

        public void EditSqlServerSource(IContextualResourceModel resourceModel)
        {
            var def = new DbSourceDefinition
            {
                Id = resourceModel.ID,
                Path = resourceModel.GetSavePath(),
                Name = resourceModel.ResourceName,
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.SqlServerSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditSqlServerResource(def, workSurfaceKey);
        }

        public void EditMySqlSource(IContextualResourceModel resourceModel)
        {
            var db = new DbSource(resourceModel.WorkflowXaml.ToXElement());
            var def = new DbSourceDefinition
            {
                AuthenticationType = db.AuthenticationType,
                DbName = db.DatabaseName,
                Id = db.ResourceID,
                Path = resourceModel.GetSavePath(),
                Name = db.ResourceName,
                Password = db.Password,
                ServerName = db.Server,
                Type = db.ServerType,
                UserName = db.UserID
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.MySqlSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditMySqlResource(def, workSurfaceKey);
        }

        public void EditPostgreSqlSource(IContextualResourceModel resourceModel)
        {
            var db = new DbSource(resourceModel.WorkflowXaml.ToXElement());
            var def = new DbSourceDefinition
            {
                AuthenticationType = db.AuthenticationType,
                DbName = db.DatabaseName,
                Id = db.ResourceID,
                Path = resourceModel.GetSavePath(),
                Name = db.ResourceName,
                Password = db.Password,
                ServerName = db.Server,
                Type = db.ServerType,
                UserName = db.UserID
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.PostgreSqlSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditPostgreSqlResource(def, workSurfaceKey);
        }

        public void EditOracleSource(IContextualResourceModel resourceModel)
        {
            var db = new DbSource(resourceModel.WorkflowXaml.ToXElement());
            var def = new DbSourceDefinition
            {
                AuthenticationType = db.AuthenticationType,
                DbName = db.DatabaseName,
                Id = db.ResourceID,
                Path = resourceModel.GetSavePath(),
                Name = db.ResourceName,
                Password = db.Password,
                ServerName = db.Server,
                Type = db.ServerType,
                UserName = db.UserID
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.OracleSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditOracleResource(def, workSurfaceKey);
        }

        public void EditOdbcSource(IContextualResourceModel resourceModel)
        {
            var db = new DbSource(resourceModel.WorkflowXaml.ToXElement());
            var def = new DbSourceDefinition
            {
                AuthenticationType = db.AuthenticationType,
                DbName = db.DatabaseName,
                Id = db.ResourceID,
                Path = resourceModel.GetSavePath(),
                Name = db.ResourceName,
                Password = db.Password,
                ServerName = db.Server,
                Type = db.ServerType,
                UserName = db.UserID
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.OdbcSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditOdbcResource(def, workSurfaceKey);
        }

        public void EditPluginSource(IContextualResourceModel resourceModel)
        {
            var db = new PluginSource(resourceModel.WorkflowXaml.ToXElement());
            var def = new PluginSourceDefinition
            {
                SelectedDll = new DllListing { FullName = db.AssemblyLocation, Name = db.AssemblyName, Children = new Collection<IFileListing>(), IsDirectory = false },
                Id = db.ResourceID,
                Path = resourceModel.GetSavePath(),
                Name = db.ResourceName,
                ConfigFilePath = db.ConfigFilePath
            };
            if (db.AssemblyLocation.EndsWith(".dll"))
            {
                def.FileSystemAssemblyName = db.AssemblyLocation;
                def.GACAssemblyName = string.Empty;
            }
            else if (db.AssemblyLocation.StartsWith("GAC:"))
            {
                def.GACAssemblyName = db.AssemblyLocation;
                def.FileSystemAssemblyName = string.Empty;
            }
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.PluginSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, workSurfaceKey);
        }

        public void EditComPluginSource(IContextualResourceModel resourceModel)
        {
            var db = new ComPluginSource(resourceModel.WorkflowXaml.ToXElement());
            var def = new ComPluginSourceDefinition
            {
                SelectedDll = new DllListing { Name = db.ComName, ClsId = db.ClsId, Is32Bit = db.Is32Bit, Children = new Collection<IFileListing>(), IsDirectory = false },
                Id = db.ResourceID,
                ResourcePath = resourceModel.GetSavePath(),
                ClsId = db.ClsId,
                Is32Bit = db.Is32Bit,
                ResourceName = db.ResourceName
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ComPluginSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, workSurfaceKey);
        }

        public void EditWebSource(IContextualResourceModel resourceModel)
        {
            var db = new WebSource(resourceModel.WorkflowXaml.ToXElement());
            var def = new WebServiceSourceDefinition()
            {
                AuthenticationType = db.AuthenticationType,
                DefaultQuery = db.DefaultQuery,
                Id = db.ResourceID,
                Name = db.ResourceName,
                Password = db.Password,
                HostName = db.Address,
                Path = resourceModel.GetSavePath(),
                UserName = db.UserName
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.WebSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, workSurfaceKey);
        }

        public void EditSharePointSource(IContextualResourceModel resourceModel)
        {
            var db = new SharepointSource(resourceModel.WorkflowXaml.ToXElement());
            var def = new SharePointServiceSourceDefinition
            {
                AuthenticationType = db.AuthenticationType,
                Server = db.Server,
                Path = resourceModel.GetSavePath(),
                Id = db.ResourceID,
                Name = db.ResourceName,
                Password = db.Password,
                UserName = db.UserName
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.SharepointServerSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, workSurfaceKey);
        }

        public void EditEmailSource(IContextualResourceModel resourceModel)
        {
            var db = new EmailSource(resourceModel.WorkflowXaml.ToXElement());

            var def = new EmailServiceSourceDefinition
            {
                Id = db.ResourceID,
                HostName = db.Host,
                Password = db.Password,
                UserName = db.UserName,
                Path = resourceModel.GetSavePath(),
                Port = db.Port,
                Timeout = db.Timeout,
                ResourceName = db.ResourceName,
                EnableSsl = db.EnableSsl
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.EmailSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, workSurfaceKey);
        }

        public void EditExchangeSource(IContextualResourceModel resourceModel)
        {
            var db = new ExchangeSource(resourceModel.WorkflowXaml.ToXElement());

            var def = new ExchangeSourceDefinition()
            {
                AutoDiscoverUrl = db.AutoDiscoverUrl,
                Id = db.ResourceID,
                Password = db.Password,
                UserName = db.UserName,
                Path = resourceModel.GetSavePath(),
                Timeout = db.Timeout,
                ResourceName = db.ResourceName,
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Exchange);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, workSurfaceKey);
        }

        public void EditDropBoxSource(IContextualResourceModel resourceModel)
        {
            var db = new DropBoxSource(resourceModel.WorkflowXaml.ToXElement());

            var def = new DropBoxSource
            {
                AccessToken = db.AccessToken,
                ResourceID = db.ResourceID,
                ResourcePath = resourceModel.GetSavePath(),
                AppKey = db.AppKey,
                ResourceName = db.ResourceName,
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.OAuthSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, workSurfaceKey);
        }

        public void EditRabbitMQSource(IContextualResourceModel resourceModel)
        {
            var source = new RabbitMQSource(resourceModel.WorkflowXaml.ToXElement());
            var def = new RabbitMQServiceSourceDefinition
            {
                ResourceID = source.ResourceID,
                ResourceName = source.ResourceName,
                HostName = source.HostName,
                Port = source.Port,
                UserName = source.UserName,
                ResourcePath = resourceModel.GetSavePath(),
                Password = source.Password,
                VirtualHost = source.VirtualHost
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.RabbitMQSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, workSurfaceKey);
        }

        public void EditWcfSource(IContextualResourceModel resourceModel)
        {
            var wcfsource = new WcfSource(resourceModel.WorkflowXaml.ToXElement());

            var def = new WcfServiceSourceDefinition
            {
                Id = wcfsource.Id,
                Name = wcfsource.ResourceName,
                Path = resourceModel.GetSavePath(),
                ResourceName = wcfsource.Name,
                EndpointUrl = wcfsource.EndpointUrl
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.WcfSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.ID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, workSurfaceKey);
        }

        public void EditServer(IContextualResourceModel resourceModel)
        {
            var connection = new Connection(resourceModel.WorkflowXaml.ToXElement());
            string address = null;
            Uri uri;
            if (Uri.TryCreate(connection.Address, UriKind.RelativeOrAbsolute, out uri))
            {
                address = uri.Host;
            }

            var selectedServer = new ServerSource
            {
                Address = connection.Address,
                ID = connection.ResourceID,
                AuthenticationType = connection.AuthenticationType,
                UserName = connection.UserName,
                Password = connection.Password,
                ResourcePath = resourceModel.GetSavePath(),
                ServerName = address,
                Name = connection.ResourceName
            };

            

            EditServer(selectedServer, ActiveServer);
        }

        public void EditResource(IOAuthSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var oauthSourceViewModel = new ManageOAuthSourceViewModel(new ManageOAuthSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), selectedSource, _mainViewModel.AsyncWorker);
            var vm = new SourceViewModel<IOAuthSource>(_mainViewModel.EventPublisher, oauthSourceViewModel, _mainViewModel.PopupProvider, new ManageOAuthSourceControl(), ActiveEnvironment);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.OAuthSource, selectedSource.ResourceID);

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(ISharepointServerSource selectedSource, IWorkSurfaceKey workSurfaceKey = null)
        {
            var viewModel = new SharepointServerSourceViewModel(new SharepointServerSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _mainViewModel.AsyncWorker, null);
            var vm = new SourceViewModel<ISharepointServerSource>(_mainViewModel.EventPublisher, viewModel, _mainViewModel.PopupProvider, new SharepointServerSource(), ActiveEnvironment);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.SharepointServerSource, selectedSource.Id);

            var key = workSurfaceKey as WorkSurfaceKey;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, vm);
            OpeningWorkflowsHelper.AddWorkflow(key);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public async Task<IRequestServiceNameViewModel> GetSaveViewModel(string resourcePath, string header, IExplorerItemViewModel explorerItemViewModel = null)
        {
            var environmentViewModel = _mainViewModel.ExplorerViewModel?.Environments.FirstOrDefault(model => model.Server.EnvironmentID == ActiveServer.EnvironmentID);
            return await RequestServiceNameViewModel.CreateAsync(environmentViewModel, resourcePath, header, explorerItemViewModel);
        }

        public void AddReverseDependencyVisualizerWorkSurface(IContextualResourceModel resource)
        {
            if (resource == null) return;
            ShowDependencies(true, resource, ActiveServer);
        }

        public void AddSettingsWorkSurface()
        {
            ActivateOrCreateUniqueWorkSurface<SettingsViewModel>(WorkSurfaceContext.Settings);
        }

        public void AddSchedulerWorkSurface()
        {
            ActivateOrCreateUniqueWorkSurface<SchedulerViewModel>(WorkSurfaceContext.Scheduler);
        }

        public void CreateNewScheduleWorkSurface(IContextualResourceModel resourceModel)
        {
            if (resourceModel != null)
            {
                WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateEnvKey(WorkSurfaceContext.Scheduler, ActiveServer.EnvironmentID) as WorkSurfaceKey;
                var workSurfaceContextViewModel = FindWorkSurfaceContextViewModel(key);
                if (workSurfaceContextViewModel != null)
                {
                    var workSurfaceViewModel = workSurfaceContextViewModel.WorkSurfaceViewModel;
                    if (workSurfaceViewModel != null)
                    {
                        var findWorkSurfaceContextViewModel = (SchedulerViewModel)workSurfaceViewModel;

                        if (findWorkSurfaceContextViewModel.IsDirty)
                        {
                            _mainViewModel.PopupProvider.Show(Warewolf.Studio.Resources.Languages.Core.SchedulerUnsavedTaskMessage,
                                Warewolf.Studio.Resources.Languages.Core.SchedulerUnsavedTaskHeader,
                                MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false, false, false);
                            ActivateAndReturnWorkSurfaceIfPresent(key);
                        }
                        else
                        {
                            ActivateAndReturnWorkSurfaceIfPresent(key);
                            findWorkSurfaceContextViewModel.CreateNewTask();
                            findWorkSurfaceContextViewModel.UpdateScheduleWithResourceDetails(resourceModel.Category, resourceModel.ID, resourceModel.ResourceName);
                        }
                    }
                }
                else
                {
                    var schedulerViewModel = ActivateOrCreateUniqueWorkSurface<SchedulerViewModel>(WorkSurfaceContext.Scheduler);
                    schedulerViewModel.CreateNewTask();
                    schedulerViewModel.UpdateScheduleWithResourceDetails(resourceModel.Category, resourceModel.ID, resourceModel.ResourceName);
                }
            }
        }

        public void AddWorkspaceItem(IContextualResourceModel model)
        {
            _mainViewModel.GETWorkspaceItemRepository().AddWorkspaceItem(model);
        }

        private void RemoveWorkspaceItem(IDesignerViewModel viewModel)
        {
            _mainViewModel.GETWorkspaceItemRepository().Remove(viewModel.ResourceModel);
        }

        public void DeleteContext(IContextualResourceModel model)
        {
            var context = FindWorkSurfaceContextViewModel(model);
            if (context == null)
            {
                return;
            }

            context.DeleteRequested = true;
            _mainViewModel.DeactivateItem(context, true);
        }

        private T CreateAndActivateUniqueWorkSurface<T>(WorkSurfaceContext context)
            where T : IWorkSurfaceViewModel
        {
            T vmr;
            WorkSurfaceContextViewModel ctx = WorkSurfaceContextFactory.Create(context, out vmr);
            AddAndActivateWorkSurface(ctx);
            return vmr;
        }

        public T ActivateOrCreateUniqueWorkSurface<T>(WorkSurfaceContext context)
            where T : IWorkSurfaceViewModel
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateEnvKey(context, ActiveServer.EnvironmentID) as WorkSurfaceKey;
            var exists = ActivateAndReturnWorkSurfaceIfPresent(key);

            if (exists == null)
            {
                if (typeof(T) == typeof(SettingsViewModel))
                {
                    Tracker.TrackEvent(TrackerEventGroup.Settings, TrackerEventName.Opened);
                }

                return CreateAndActivateUniqueWorkSurface<T>(context);
            }
            try
            {
                return (T)exists.WorkSurfaceViewModel;
            }
            catch
            {
                return default(T);
            }
        }

        private bool ActivateWorkSurfaceIfPresent(IContextualResourceModel resource)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(resource);
            WorkSurfaceContextViewModel currentContext = FindWorkSurfaceContextViewModel(key);

            if (currentContext != null)
            {
                _mainViewModel.ActivateItem(currentContext);
                return true;
            }
            return false;
        }

        private WorkSurfaceContextViewModel ActivateAndReturnWorkSurfaceIfPresent(WorkSurfaceKey key)
        {
            WorkSurfaceContextViewModel currentContext = FindWorkSurfaceContextViewModel(key);

            if (currentContext != null)
            {
                _mainViewModel.ActivateItem(currentContext);
                return currentContext;
            }
            return null;
        }

        private WorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(WorkSurfaceKey key)
        {
            return _mainViewModel.Items.FirstOrDefault(c => WorkSurfaceKeyEqualityComparerWithContextKey.Current.Equals(key, c.WorkSurfaceKey));
        }

        public WorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(IContextualResourceModel resource)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(resource);
            return FindWorkSurfaceContextViewModel(key);
        }

        public void AddWorkSurfaceContextImpl(IContextualResourceModel resourceModel, bool isLoadingWorkspace)
        {
            if (resourceModel == null)
            {
                return;
            }

            var exists = IsInOpeningState(resourceModel) || ActivateWorkSurfaceIfPresent(resourceModel);
            if (exists)
            {
                return;
            }

            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(resourceModel);

            _mainViewModel.CanDebug = false;

            if (!isLoadingWorkspace)
            {
                OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
                resourceModel.IsWorkflowSaved = true;
            }

            AddWorkspaceItem(resourceModel);
            var workSurfaceContextViewModel = _getWorkSurfaceContextViewModel(resourceModel, _createDesigners) as WorkSurfaceContextViewModel;
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
            OpeningWorkflowsHelper.RemoveWorkflow(workSurfaceKey);
            _mainViewModel.CanDebug = true;
        }

        private bool IsInOpeningState(IContextualResourceModel resource)
        {
            WorkSurfaceKey key = WorkSurfaceKeyFactory.CreateKey(resource);
            return OpeningWorkflowsHelper.FetchOpeningKeys().Any(c => WorkSurfaceKeyEqualityComparer.Current.Equals(key, c));
        }

        public void AddAndActivateWorkSurface(WorkSurfaceContextViewModel context)
        {
            if (context != null)
            {
                var found = FindWorkSurfaceContextViewModel(context.WorkSurfaceKey);
                if (found == null)
                {
                    found = context;
                    _mainViewModel.Items.Add(context);
                }
                _mainViewModel.ActivateItem(found);
            }
        }

        public void AddWorkSurface(IWorkSurfaceObject obj)
        {
            TypeSwitch.Do(obj, TypeSwitch.Case<IContextualResourceModel>(AddWorkSurfaceContext));
        }

        public bool CloseWorkSurfaceContext(WorkSurfaceContextViewModel context, PaneClosingEventArgs e, bool dontPrompt = false)
        {
            bool remove = true;
            if (context != null)
            {
                if (!context.DeleteRequested)
                {
                    var vm = context.WorkSurfaceViewModel;
                    if (vm != null)
                    {
                        if (vm.WorkSurfaceContext == WorkSurfaceContext.Workflow)
                        {
                            return CloseWorkflow(context, e, dontPrompt, vm, ref remove) && remove;
                        }
                        if (vm.WorkSurfaceContext == WorkSurfaceContext.Settings)
                        {
                            return CloseSettings(vm, true);
                        }
                        if (vm.WorkSurfaceContext == WorkSurfaceContext.Scheduler)
                        {
                            return RemoveScheduler(vm, true);
                        }
                    }
                    var tab = vm as IStudioTab;
                    if (tab != null)
                    {
                        remove = tab.DoDeactivate(true);
                        if (remove)
                        {
                            tab.Dispose();
                            tab.CloseView();
                        }
                    }
                }
            }

            return remove;
        }

        private static bool RemoveScheduler(IWorkSurfaceViewModel vm, bool remove)
        {
            var schedulerViewModel = vm as SchedulerViewModel;
            if (schedulerViewModel != null)
            {
                remove = schedulerViewModel.DoDeactivate(true);
                if (remove)
                {
                    schedulerViewModel.Dispose();
                }
            }
            return remove;
        }

        private static bool CloseSettings(IWorkSurfaceViewModel vm, bool remove)
        {
            var settingsViewModel = vm as SettingsViewModel;
            if (settingsViewModel != null)
            {
                remove = settingsViewModel.DoDeactivate(true);
                if (remove)
                {
                    settingsViewModel.Dispose();
                }
            }
            return remove;
        }

        private bool CloseWorkflow(WorkSurfaceContextViewModel context, PaneClosingEventArgs e, bool dontPrompt, IWorkSurfaceViewModel vm, ref bool remove)
        {
            var workflowVm = vm as IWorkflowDesignerViewModel;
            IContextualResourceModel resource = workflowVm?.ResourceModel;
            if (resource != null)
            {
                remove = !resource.IsAuthorized(AuthorizationContext.Contribute) || resource.IsWorkflowSaved;
                var connection = workflowVm.ResourceModel.Environment.Connection;
                if (connection != null && !connection.IsConnected)
                {
                    var result = _mainViewModel.PopupProvider.Show(string.Format(StringResources.DialogBody_DisconnectedItemNotSaved, workflowVm.ResourceModel.ResourceName),
                        $"Save not allowed {workflowVm.ResourceModel.ResourceName}?",
                        MessageBoxButton.OKCancel, MessageBoxImage.Information, "", false, false, true, false, false, false);

                    switch (result)
                    {
                        case MessageBoxResult.OK:
                            remove = true;
                            break;
                        case MessageBoxResult.Cancel:
                            return false;
                        default:
                            return false;
                    }
                }
                if (dontPrompt)
                    remove = true;
                if (!remove)
                {
                    remove = ShowRemovePopup(workflowVm);
                }

                if (remove)
                {
                    if (resource.IsNewWorkflow)
                    {
                        NewWorkflowNames.Instance.Remove(resource.ResourceName);
                    }
                    RemoveWorkspaceItem(workflowVm);
                    _mainViewModel.Items.Remove(context);
                    workflowVm.Dispose();
                    if (_mainViewModel.PreviousActive != null && _mainViewModel.PreviousActive.WorkSurfaceViewModel == vm)
                        _mainViewModel.PreviousActive = null;
                    if (e != null)
                    {
                        e.Cancel = true;
                    }
                }
                else if (e != null)
                {
                    e.Handled = true;
                    e.Cancel = false;
                }
            }
            return true;
        }
    }
}