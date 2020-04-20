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
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Data.ServiceModel;
using Dev2.Factory;
using Dev2.Instrumentation;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Settings;
using Dev2.Triggers.Scheduler;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Utils;
using Dev2.Studio.Factory;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Studio.ViewModels.DependencyVisualization;
using Dev2.Studio.ViewModels.Workflow;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.DependencyVisualization;
using Dev2.Threading;
using Dev2.Utils;
using Dev2.ViewModels;
using Infragistics.Windows.DockManager.Events;
using Microsoft.Practices.Prism.Mvvm;
using ServiceStack.Net30.Collections.Concurrent;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Dev2.ViewModels.Merge;
using Dev2.Views.Merge;
using Dev2.ViewModels.Search;
using Dev2.Views.Search;
using Dev2.Triggers;
using Warewolf.Data;

namespace Dev2.Studio.ViewModels
{
    public interface IWorksurfaceContextManager
    {
        void Handle(RemoveResourceAndCloseTabMessage message);
        void Handle(NewTestFromDebugMessage message);
        void Handle(NewTestFromDebugMessage message, IWorkSurfaceKey workSurfaceKey);
        void EditServer(IServerSource selectedServer, IServer activeServer, IView view);
        void EditSqlServerResource(IDbSource selectedSource, IView view);
        void EditSqlServerResource(IDbSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditMySqlResource(IDbSource selectedSource, IView view);
        void EditMySqlResource(IDbSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditPostgreSqlResource(IDbSource selectedSource, IView view);
        void EditPostgreSqlResource(IDbSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditOracleResource(IDbSource selectedSource, IView view);
        void EditOracleResource(IDbSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditOdbcResource(IDbSource selectedSource, IView view);
        void EditOdbcResource(IDbSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
		void EditResource(IPluginSource selectedSource, IView view);
        void EditResource(IPluginSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditResource(IComPluginSource selectedSource, IView view);
        void EditResource(IComPluginSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditResource(IWebServiceSource selectedSource, IView view);
        void EditResource(IWebServiceSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditResource(IEmailServiceSource selectedSource, IView view);
        void EditResource(IEmailServiceSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditResource(IExchangeSource selectedSource, IView view);
        void EditResource(IExchangeSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IView view);
        void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditResource(IElasticsearchSourceDefinition selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditResource(IWcfServerSource selectedSource, IView view);
        void EditResource(IWcfServerSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void NewService(string resourcePath);
        void NewSqlServerSource(string resourcePath);
        void NewMySqlSource(string resourcePath);
        void NewPostgreSqlSource(string resourcePath);
        void NewOracleSource(string resourcePath);
        void NewOdbcSource(string resourcePath);
		bool DuplicateResource(IExplorerItemViewModel explorerItemViewModel);
        void NewWebSource(string resourcePath);
        void NewRedisSource(string resourcePath);
        void NewElasticsearchSource(string resourcePath);
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
        void EditSqlServerSource(IContextualResourceModel resourceModel, IView view);
        void EditMySqlSource(IContextualResourceModel resourceModel, IView view);
        void EditPostgreSqlSource(IContextualResourceModel resourceModel, IView view);
        void EditOracleSource(IContextualResourceModel resourceModel, IView view);
        void EditOdbcSource(IContextualResourceModel resourceModel, IView view);
		void EditPluginSource(IContextualResourceModel resourceModel, IView view);
        void EditComPluginSource(IContextualResourceModel resourceModel, IView view);
        void EditWebSource(IContextualResourceModel resourceModel, IView view);
        void EditSharePointSource(IContextualResourceModel resourceModel, IView view);
        void EditEmailSource(IContextualResourceModel resourceModel, IView view);
        void EditExchangeSource(IContextualResourceModel resourceModel, IView view);
        void EditDropBoxSource(IContextualResourceModel resourceModel, IView view);
        void EditRabbitMQSource(IContextualResourceModel resourceModel, IView view);
        void EditWcfSource(IContextualResourceModel resourceModel, IView view);
        void EditServer(IContextualResourceModel resourceModel, IView view);
        void EditResource(IOAuthSource selectedSource, IView view);
        void EditResource(IOAuthSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        void EditResource(ISharepointServerSource selectedSource, IView view);
        void EditResource(ISharepointServerSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey);
        Task<IRequestServiceNameViewModel> GetSaveViewModel(string resourcePath, string header);
        Task<IRequestServiceNameViewModel> GetSaveViewModel(string resourcePath, string header, IExplorerItemViewModel explorerItemViewModel);
        void TryShowDependencies(IContextualResourceModel resource);
        void AddSettingsWorkSurface();
        void AddSchedulerWorkSurface();
        TriggersViewModel AddTriggersWorkSurface();
        void AddQueuesWorkSurface();
        void TryCreateNewScheduleWorkSurface(IContextualResourceModel resourceModel);
        void TryCreateNewQueueEventWorkSurface(IContextualResourceModel resourceModel);

        void AddWorkspaceItem(IContextualResourceModel model);
        void DeleteContext(IContextualResourceModel model);
        T ActivateOrCreateUniqueWorkSurface<T>(WorkSurfaceContext context) where T : IWorkSurfaceViewModel;
        IWorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(IContextualResourceModel resource);
        void AddWorkSurfaceContextImpl(IContextualResourceModel resourceModel, bool isLoadingWorkspace);
        void AddAndActivateWorkSurface(IWorkSurfaceContextViewModel context);
        void AddWorkSurface(IWorkSurfaceObject obj);
        bool CloseWorkSurfaceContext(IWorkSurfaceContextViewModel context, PaneClosingEventArgs e);
        void ViewTestsForService(IContextualResourceModel resourceModel);
        bool CloseWorkSurfaceContext(IWorkSurfaceContextViewModel context, PaneClosingEventArgs e, bool dontPrompt);
        void ViewMergeConflictsService(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadFromServer);
        void SearchView(IWorkSurfaceKey workSurfaceKey);
        void ViewMergeConflictsService(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadFromServer, IWorkSurfaceKey workSurfaceKey);
        void ViewTestsForService(IContextualResourceModel resourceModel, IWorkSurfaceKey workSurfaceKey);
        void ViewSelectedTestForService(IContextualResourceModel resourceModel, IServiceTestModel selectedServiceTest, ServiceTestViewModel testViewModel, IWorkSurfaceKey workSurfaceKey);
        void RunAllTestsForService(IContextualResourceModel resourceModel);
        void RunAllTestsForFolder(string ResourcePath, IExternalProcessExecutor ProcessExecutor);
        void RunAllTestCoverageForService(IContextualResourceModel contextualResourceModel);
        void RunAllTestCoverageForFolder(string secureResourcePath, IExternalProcessExecutor processExecutor);
        WorkSurfaceContextViewModel EditResource<T>(IWorkSurfaceKey workSurfaceKey, SourceViewModel<T> viewModel) where T : IEquatable<T>;

        IWorkSurfaceKey TryGetOrCreateWorkSurfaceKey(IWorkSurfaceKey workSurfaceKey, WorkSurfaceContext workSurfaceContext, Guid resourceID);
    }

    public class WorksurfaceContextManager : IWorksurfaceContextManager
    {
        readonly IServerRepository _repository;
        readonly IViewFactory _factory;

        public IServerRepository ServerRepo => _repository ?? CustomContainer.Get<IServerRepository>();
        public IViewFactory ViewFactoryProvider => _factory ?? new ViewFactory();
        private readonly ShellViewModel _shellViewModel;
        private readonly bool _createDesigners;
        private readonly Func<IContextualResourceModel, bool, IWorkSurfaceContextViewModel> _getWorkSurfaceContextViewModel = (resourceModel, createDesigner) => WorkSurfaceContextFactory.CreateResourceViewModel(resourceModel, createDesigner);
        private readonly IApplicationTracker _applicationTracker;
        public WorksurfaceContextManager(bool createDesigners, ShellViewModel shellViewModel)
        {
            _createDesigners = createDesigners;
            _shellViewModel = shellViewModel;
            _applicationTracker = CustomContainer.Get<IApplicationTracker>();
            SetUpEditHandlers();
        }

        public WorksurfaceContextManager(bool createDesigners, ShellViewModel shellViewModel, IServerRepository repository, IViewFactory factory)
            : this(createDesigners, shellViewModel)
        {
            _repository = repository;
            _factory = factory;
        }



        public void Handle(RemoveResourceAndCloseTabMessage message)
        {
            Dev2Logger.Debug(message.GetType().Name, "Warewolf Debug");
            if (message.ResourceToRemove == null)
            {
                return;
            }

            var wfscvm = FindWorkSurfaceContextViewModel(message.ResourceToRemove);
            if (message.RemoveFromWorkspace)
            {
                _shellViewModel.DeactivateItem(wfscvm, true);
            }
            else
            {
                _shellViewModel.BaseDeactivateItem(wfscvm, true);
            }

            _shellViewModel.PreviousActive = null;

        }

        public void Handle(NewTestFromDebugMessage message) => Handle(message, null);
        public void Handle(NewTestFromDebugMessage message, IWorkSurfaceKey workSurfaceKey)
        {
            Dev2Logger.Debug(message.GetType().Name, "Warewolf Debug");
            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.ServiceTestsViewer, message.ResourceID);
            var found = FindWorkSurfaceContextViewModel(workSurfaceKey);
            if (found != null)
            {
                var vm = found.WorkSurfaceViewModel;
                if (vm != null && vm is IStudioTestWorkSurfaceViewModel testVm)
                {
                    var serviceTestViewModel = testVm?.ViewModel;
                    serviceTestViewModel?.PrepopulateTestsUsingDebug(message.RootItems);
                }
                AddAndActivateWorkSurface(found);
            }
            else
            {
                var workflow = new WorkflowDesignerViewModel(message.ResourceModel);
                var testViewModel = new ServiceTestViewModel(message.ResourceModel, new AsyncWorker(), _shellViewModel.EventPublisher, new ExternalProcessExecutor(), workflow, message);
                var vm = new StudioTestViewModel(_shellViewModel.EventPublisher, testViewModel, _shellViewModel.PopupProvider, new ServiceTestView());
                var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
                AddAndActivateWorkSurface(workSurfaceContextViewModel);
            }
        }


        public void EditServer(IServerSource selectedServer, IServer activeServer, IView view)
        {
            var environmentModel = ServerRepo.Get(activeServer.EnvironmentID);
            var viewModel = new ManageNewServerViewModel(new ManageNewServerSourceModel(activeServer.UpdateRepository, activeServer.QueryProxy, activeServer.DisplayName), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedServer, _shellViewModel.AsyncWorker, new ExternalProcessExecutor());
            var vm = new SourceViewModel<IServerSource>(_shellViewModel.EventPublisher, viewModel, _shellViewModel.PopupProvider, view, environmentModel);

            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ServerSource);
            workSurfaceKey.EnvironmentID = activeServer.EnvironmentID;
            workSurfaceKey.ResourceID = selectedServer.ID;
            workSurfaceKey.ServerID = activeServer.ServerID;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);

            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public WorkSurfaceContextViewModel EditResource<T>(IWorkSurfaceKey workSurfaceKey, SourceViewModel<T> viewModel) where T : IEquatable<T>
        {
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, viewModel);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            return workSurfaceContextViewModel;
        }

        public void EditSqlServerResource(IDbSource selectedSource, IView view) => EditSqlServerResource(selectedSource, view, null);
        public void EditSqlServerResource(IDbSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var dbSourceViewModel = new ManageSqlServerSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name)
                    , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                    , selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IDbSource>(_shellViewModel.EventPublisher, dbSourceViewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.SqlServerSource, selectedSource.Id);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditMySqlResource(IDbSource selectedSource, IView view) => EditMySqlResource(selectedSource, view, null);
        public void EditMySqlResource(IDbSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var dbSourceViewModel = new ManageMySqlSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, "")
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IDbSource>(_shellViewModel.EventPublisher, dbSourceViewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.MySqlSource, selectedSource.Id);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditPostgreSqlResource(IDbSource selectedSource, IView view) => EditPostgreSqlResource(selectedSource, view, null);
        public void EditPostgreSqlResource(IDbSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var dbSourceViewModel = new ManagePostgreSqlSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IDbSource>(_shellViewModel.EventPublisher, dbSourceViewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.PostgreSqlSource, selectedSource.Id);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditOracleResource(IDbSource selectedSource, IView view) => EditOracleResource(selectedSource, view, null);
        public void EditOracleResource(IDbSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var dbSourceViewModel = new ManageOracleSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, "Oracle"), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IDbSource>(_shellViewModel.EventPublisher, dbSourceViewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.OracleSource, selectedSource.Id);
            
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditOdbcResource(IDbSource selectedSource, IView view) => EditOdbcResource(selectedSource, view, null);
        public void EditOdbcResource(IDbSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var dbSourceViewModel = new ManageOdbcSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IDbSource>(_shellViewModel.EventPublisher, dbSourceViewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.OdbcSource, selectedSource.Id);
            
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
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

        public void ViewMergeConflictsService(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadFromServer) => ViewMergeConflictsService(currentResourceModel, differenceResourceModel, loadFromServer, null);

        public void ViewMergeConflictsService(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadFromServer, IWorkSurfaceKey workSurfaceKey)
        {
            var mergeViewModel = new MergeWorkflowViewModel(currentResourceModel, differenceResourceModel, loadFromServer);//if this is merge between two server versions then we pass false
            var vm = new MergeViewModel(_shellViewModel.EventPublisher, mergeViewModel, _shellViewModel.PopupProvider, new MergeWorkflowView());
            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.MergeConflicts, currentResourceModel.ID);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void SearchView(IWorkSurfaceKey workSurfaceKey)
        {
            var vm = new SearchModel(_shellViewModel.EventPublisher, new SearchViewModel(_shellViewModel, CustomContainer.Get<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>()), new SearchView());
            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.SearchViewer, Guid.Empty);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void ViewTestsForService(IContextualResourceModel resourceModel) => ViewTestsForService(resourceModel, null);
        public void ViewTestsForService(IContextualResourceModel resourceModel, IWorkSurfaceKey workSurfaceKey)
        {
            var workflow = new WorkflowDesignerViewModel(resourceModel);
            var testViewModel = new ServiceTestViewModel(resourceModel, new AsyncWorker(), _shellViewModel.EventPublisher, new ExternalProcessExecutor(), workflow);
            var vm = new StudioTestViewModel(_shellViewModel.EventPublisher, testViewModel, _shellViewModel.PopupProvider, new ServiceTestView());
            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.ServiceTestsViewer, resourceModel.ID);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void ViewSelectedTestForService(IContextualResourceModel resourceModel, IServiceTestModel selectedServiceTest, ServiceTestViewModel testViewModel, IWorkSurfaceKey workSurfaceKey)
        {
            var workflow = new WorkflowDesignerViewModel(resourceModel);
            var vm = new StudioTestViewModel(_shellViewModel.EventPublisher, testViewModel, _shellViewModel.PopupProvider, new ServiceTestView());
            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.ServiceTestsViewer, resourceModel.ID);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
            testViewModel.SelectedServiceTest = selectedServiceTest;
        }

        public void RunAllTestsForService(IContextualResourceModel resourceModel)
        {
            var workflow = new WorkflowDesignerViewModel(resourceModel);
            using (var testViewModel = new ServiceTestViewModel(resourceModel, new AsyncWorker(), _shellViewModel.EventPublisher, new ExternalProcessExecutor(), workflow))
            {
                testViewModel.RunAllTestsInBrowserCommand.Execute(null);
            }
        }

        public void RunAllTestCoverageForService(IContextualResourceModel resourceModel)
        {
            var workflow = new WorkflowDesignerViewModel(resourceModel);
            using (var testViewModel = new ServiceTestViewModel(resourceModel, new AsyncWorker(), _shellViewModel.EventPublisher, new ExternalProcessExecutor(), workflow))
            {
                testViewModel.RunAllTestCoverageInBrowserCommand.Execute(null);
            }
        }

        public void RunAllTestsForFolder(string ResourcePath, IExternalProcessExecutor ProcessExecutor)
        {
            var ServiceTestCommandHandler = new ServiceTestCommandHandlerModel();
            var resourceTestsPath = ResourcePath + "/.tests";
            ServiceTestCommandHandler.RunAllTestsInBrowser(false, resourceTestsPath, ProcessExecutor);
        }

        public void RunAllTestCoverageForFolder(string ResourcePath, IExternalProcessExecutor ProcessExecutor)
        {
            var ServiceTestCommandHandler = new ServiceTestCommandHandlerModel();
            var resourceTestsPath = ResourcePath + "/.coverage";
            ServiceTestCommandHandler.RunAllTestCoverageInBrowser(false, resourceTestsPath, ProcessExecutor);
        }

        public void EditResource(IPluginSource selectedSource, IView view) => EditResource(selectedSource, view, null);
        public void EditResource(IPluginSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var pluginSourceViewModel = new ManagePluginSourceViewModel(new ManagePluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IPluginSource>(_shellViewModel.EventPublisher, pluginSourceViewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.PluginSource, selectedSource.Id);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IComPluginSource selectedSource, IView view) => EditResource(selectedSource, view, null);
        public void EditResource(IComPluginSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var wcfSourceViewModel = new ManageComPluginSourceViewModel(new ManageComPluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IComPluginSource>(_shellViewModel.EventPublisher, wcfSourceViewModel, _shellViewModel.PopupProvider, view, ActiveServer);
            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.ComPluginSource, selectedSource.Id);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IWebServiceSource selectedSource, IView view) => EditResource(selectedSource, view, null);
        public void EditResource(IWebServiceSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var viewModel = new ManageWebserviceSourceViewModel(new ManageWebServiceSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _shellViewModel.AsyncWorker, new ExternalProcessExecutor());
            var vm = new SourceViewModel<IWebServiceSource>(_shellViewModel.EventPublisher, viewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.WebSource, selectedSource.Id);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IRedisServiceSource selectedSource, IView view) => EditResource(selectedSource, view, null);
        public void EditResource(IRedisServiceSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var viewModel = new RedisSourceViewModel(new RedisSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _shellViewModel.AsyncWorker, new ExternalProcessExecutor());
            var vm = new SourceViewModel<IRedisServiceSource>(_shellViewModel.EventPublisher, viewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.RedisSource, selectedSource.Id);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IEmailServiceSource selectedSource, IView view) => EditResource(selectedSource, view, null);
        public void EditResource(IEmailServiceSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var emailSourceViewModel = new ManageEmailSourceViewModel(new ManageEmailSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IEmailServiceSource>(_shellViewModel.EventPublisher, emailSourceViewModel, _shellViewModel.PopupProvider, view, ActiveServer);


            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.EmailSource, selectedSource.Id);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IExchangeSource selectedSource, IView view) => EditResource(selectedSource, view, null);
        public void EditResource(IExchangeSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var emailSourceViewModel = new ManageExchangeSourceViewModel(new ManageExchangeSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IExchangeSource>(_shellViewModel.EventPublisher, emailSourceViewModel, _shellViewModel.PopupProvider, view, ActiveServer);


            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.Exchange, selectedSource.ResourceID);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IView view) => EditResource(selectedSource, view, null);
        public void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var viewModel = new ManageRabbitMQSourceViewModel(new ManageRabbitMQSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, _shellViewModel), selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IRabbitMQServiceSourceDefinition>(_shellViewModel.EventPublisher, viewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.RabbitMQSource, selectedSource.ResourceID);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IElasticsearchSourceDefinition selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var viewModel = new ElasticsearchSourceViewModel(new ElasticsearchSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, _shellViewModel), selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IElasticsearchSourceDefinition>(_shellViewModel.EventPublisher, viewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.RabbitMQSource, selectedSource.Id);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(IWcfServerSource selectedSource, IView view) => EditResource(selectedSource, view, null);
        public void EditResource(IWcfServerSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var wcfSourceViewModel = new ManageWcfSourceViewModel(new ManageWcfSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _shellViewModel.AsyncWorker, ActiveServer);
            var vm = new SourceViewModel<IWcfServerSource>(_shellViewModel.EventPublisher, wcfSourceViewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.WcfSource, selectedSource.ResourceID);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }



        IServer ActiveServer => _shellViewModel.ActiveServer;

        public void NewService(string resourcePath)
        {
            TempSave(ActiveServer, resourcePath);
        }

        public void NewSqlServerSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.SqlServerSourceNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.SqlServerSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IDbSource>(_shellViewModel.EventPublisher, new ManageSqlServerSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _shellViewModel.AsyncWorker)
            {
                SelectedGuid = key.ResourceID.Value
            }
            , _shellViewModel.PopupProvider
                , new ManageDatabaseSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewMySqlSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.MySqlSourceNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.MySqlSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IDbSource>(_shellViewModel.EventPublisher, new ManageMySqlSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _shellViewModel.AsyncWorker)
            {
                SelectedGuid = key.ResourceID.Value
            }, _shellViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewPostgreSqlSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.PostgreSqlSourceNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.PostgreSqlSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key,
                new SourceViewModel<IDbSource>(_shellViewModel.EventPublisher, new ManagePostgreSqlSourceViewModel(
                                                new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name)
                                                , saveViewModel
                                                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                                                , _shellViewModel.AsyncWorker)
                {
                    SelectedGuid = key.ResourceID.Value
                }
                , _shellViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewOracleSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.OracleSourceNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.OracleSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IDbSource>(_shellViewModel.EventPublisher, new ManageOracleSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name)
                , saveViewModel
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , _shellViewModel.AsyncWorker)
            {
                SelectedGuid = key.ResourceID.Value
            }, _shellViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewOdbcSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.OdbcSourceNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.OdbcSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IDbSource>(_shellViewModel.EventPublisher, new ManageOdbcSourceViewModel(new ManageDatabaseSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name)
                , saveViewModel
                , new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()
                , _shellViewModel.AsyncWorker)
            { SelectedGuid = key.ResourceID.Value }, _shellViewModel.PopupProvider, new ManageDatabaseSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }
		public bool DuplicateResource(IExplorerItemViewModel explorerItemViewModel)
        {
            var saveViewModel = GetSaveViewModel(string.Empty, explorerItemViewModel.ResourceName, explorerItemViewModel);
            var messageBoxResult = saveViewModel.Result.ShowSaveDialog();
            return messageBoxResult == MessageBoxResult.OK;
        }

        public void NewWebSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.WebserviceNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.WebSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IWebServiceSource>(_shellViewModel.EventPublisher, new ManageWebserviceSourceViewModel(new ManageWebServiceSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _shellViewModel.AsyncWorker, new ExternalProcessExecutor()) { SelectedGuid = key.ResourceID.Value }, _shellViewModel.PopupProvider, new ManageWebserviceSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewRedisSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.RedisNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.RedisSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IRedisServiceSource>(_shellViewModel.EventPublisher, new RedisSourceViewModel(new RedisSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _shellViewModel.AsyncWorker, new ExternalProcessExecutor()) { SelectedGuid = key.ResourceID.Value }, _shellViewModel.PopupProvider, new RedisSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewElasticsearchSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.ElasticsearchNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ElasticsearchSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IElasticsearchSourceDefinition>(_shellViewModel.EventPublisher, new ElasticsearchSourceViewModel(new ElasticsearchSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _shellViewModel.AsyncWorker, new ExternalProcessExecutor()) { SelectedGuid = key.ResourceID.Value }, _shellViewModel.PopupProvider, new ElasticsearchSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewPluginSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.PluginSourceNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.PluginSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IPluginSource>(_shellViewModel.EventPublisher, new ManagePluginSourceViewModel(new ManagePluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name),
                saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _shellViewModel.AsyncWorker)
            { SelectedGuid = key.ResourceID.Value }, _shellViewModel.PopupProvider, new ManagePluginSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewComPluginSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.ComPluginSourceNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.ComPluginSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IComPluginSource>(_shellViewModel.EventPublisher, new ManageComPluginSourceViewModel(new ManageComPluginSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name),
                saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _shellViewModel.AsyncWorker)
            { SelectedGuid = key.ResourceID.Value }, _shellViewModel.PopupProvider, new ManageComPluginSourceControl(), ActiveServer));

            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewWcfSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.WcfServiceNewHeaderLabel);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.WcfSource), new SourceViewModel<IWcfServerSource>(_shellViewModel.EventPublisher, new ManageWcfSourceViewModel(new ManageWcfSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _shellViewModel.AsyncWorker, ActiveServer), _shellViewModel.PopupProvider, new ManageWcfSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewDropboxSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.DropboxSourceNewHeaderLabel);
            var key = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.OAuthSource);
            key.ServerID = ActiveServer.ServerID;

            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(key, new SourceViewModel<IOAuthSource>(_shellViewModel.EventPublisher, new ManageOAuthSourceViewModel(new ManageOAuthSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name), saveViewModel), _shellViewModel.PopupProvider, new ManageOAuthSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewRabbitMQSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.RabbitMQSourceNewHeaderLabel);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.RabbitMQSource), new SourceViewModel<IRabbitMQServiceSourceDefinition>(_shellViewModel.EventPublisher, new ManageRabbitMQSourceViewModel(new ManageRabbitMQSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, _shellViewModel), saveViewModel), _shellViewModel.PopupProvider, new ManageRabbitMQSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void NewSharepointSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.SharePointServiceNewHeaderLabel);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.SharepointServerSource), new SourceViewModel<ISharepointServerSource>(_shellViewModel.EventPublisher, new SharepointServerSourceViewModel(new SharepointServerSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), _shellViewModel.AsyncWorker, ActiveServer), _shellViewModel.PopupProvider, new SharepointServerSource(), ActiveServer));
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
                    ActiveServer.ResourceRepository.LoadContextualResourceModel(resourceId);

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

                var resourceVersion = new ResourceModel(ActiveServer, EventPublishers.Aggregator)
                {
                    ResourceType = resourceModel.ResourceType,
                    ResourceName = $"{resource.ResourceName} (v.{versionInfo.VersionNumber})",
                    WorkflowXaml = new StringBuilder(xamlString),
                    UserPermissions = Permissions.View,
                    DataList = dataListString,
                    IsVersionResource = true,
                    ID = Guid.NewGuid(),
                    OriginalId = resourceId
                };

                DisplayResourceWizard(resourceVersion);
            }
        }

        public void NewEmailSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.EmailSourceNewHeaderLabel);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.EmailSource), new SourceViewModel<IEmailServiceSource>(_shellViewModel.EventPublisher, new ManageEmailSourceViewModel(new ManageEmailSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()), _shellViewModel.PopupProvider, new ManageEmailSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);

        }

        public void NewExchangeSource(string resourcePath)
        {
            var saveViewModel = GetSaveViewModel(resourcePath, Warewolf.Studio.Resources.Languages.Core.EmailSourceNewHeaderLabel);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.Exchange), new SourceViewModel<IExchangeSource>(_shellViewModel.EventPublisher, new ManageExchangeSourceViewModel(new ManageExchangeSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ActiveServer.Name), saveViewModel, new Microsoft.Practices.Prism.PubSubEvents.EventAggregator()), _shellViewModel.PopupProvider, new ManageExchangeSourceControl(), ActiveServer));
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public bool IsWorkFlowOpened(IContextualResourceModel resource) => FindWorkSurfaceContextViewModel(resource) != null;

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
            workSurfaceKey.EnvironmentID = model.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = model.ID;
            workSurfaceKey.ServerID = model.ServerID;
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        void TempSave(IServer activeEnvironment, string resourcePath)
        {
            var newWorflowName = NewWorkflowNames.Instance.GetNext();

            var tempResource = ResourceModelFactory.CreateResourceModel(activeEnvironment, @"WorkflowService",
                newWorflowName);
            if (string.IsNullOrEmpty(tempResource.Category))
            {
                tempResource.Category = string.IsNullOrEmpty(resourcePath) ? @"Unassigned\" + newWorflowName : resourcePath + @"\" + newWorflowName;
            }
            tempResource.ResourceName = newWorflowName;
            tempResource.DisplayName = newWorflowName;
            tempResource.IsNewWorkflow = true;

            AddAndActivateWorkSurface(WorkSurfaceContextFactory.CreateResourceViewModel(tempResource));
            AddWorkspaceItem(tempResource);
        }

        bool ShowRemovePopup(IWorkflowDesignerViewModel workflowVm)
        {
            var result = _shellViewModel.PopupProvider.Show(string.Format(StringResources.DialogBody_NotSaved, workflowVm.ResourceModel.ResourceName),
                $"Save {workflowVm.ResourceModel.ResourceName}?",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Information, @"", false, false, true, false, false, false);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    workflowVm.ResourceModel.Commit();
                    Dev2Logger.Info(@"Publish message of type - " + typeof(SaveResourceMessage), "Warewolf Info");
                    _shellViewModel.EventPublisher.Publish(new SaveResourceMessage(workflowVm.ResourceModel, false, false));
                    return !workflowVm.WorkflowName.ToLower().StartsWith(@"unsaved");
                case MessageBoxResult.No:
                    var model = workflowVm.ResourceModel;
                    try
                    {
                        if (workflowVm.Server.ResourceRepository.DoesResourceExistInRepo(model) && workflowVm.ResourceModel.IsNewWorkflow)
                        {
                            _shellViewModel.DeleteResources(new List<IContextualResourceModel> { model }, @"", false);
                        }
                        else
                        {
                            model.Rollback();
                        }
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Info(@"Exception: " + e.Message, "Warewolf Info");
                    }

                    NewWorkflowNames.Instance.Remove(workflowVm.ResourceModel.ResourceName);
                    return true;
                default:
                    return false;
            }
        }


        public void DisplayResourceWizard(IWorkSurfaceContextViewModel contextViewModel)
        {
            AddAndActivateWorkSurface(contextViewModel);
        }


        public void DisplayResourceWizard(IContextualResourceModel resourceModel)
        {
            if (resourceModel == null)
            {
                return;
            }

            if (resourceModel.ServerResourceType == ResourceType.WorkflowService.ToString())
            {
                _shellViewModel.PersistTabs();
                AddWorkSurfaceContext(resourceModel);
                return;
            }

            if (_editHandler.TryGetValue(resourceModel.ServerResourceType, out Action<IContextualResourceModel, IView> editAction))
            {
                editAction.Invoke(resourceModel, resourceModel.GetView(() => ViewFactoryProvider.GetViewGivenServerResourceType(resourceModel.ServerResourceType)));
            }
            else
            {
                AddWorkSurfaceContext(resourceModel);
            }
        }

        readonly ConcurrentDictionary<string, Action<IContextualResourceModel, IView>> _editHandler = new ConcurrentDictionary<string, Action<IContextualResourceModel, IView>>();
        void SetUpEditHandlers()
        {
            _editHandler.TryAdd("Server", EditServer);
            _editHandler.TryAdd("Dev2Server", EditServer);
            _editHandler.TryAdd("ServerSource", EditServer);
            _editHandler.TryAdd("RabbitMQSource", EditRabbitMQSource);
            _editHandler.TryAdd("OauthSource", EditDropBoxSource);
            _editHandler.TryAdd("SharepointServerSource", EditSharePointSource);
            _editHandler.TryAdd("DropBoxSource", EditDropBoxSource);
            _editHandler.TryAdd("ExchangeSource", EditExchangeSource);
            _editHandler.TryAdd("EmailSource", EditEmailSource);
            _editHandler.TryAdd("WcfSource", EditWcfSource);
            _editHandler.TryAdd("ComPluginSource", EditComPluginSource);
            _editHandler.TryAdd("PluginSource", EditPluginSource);
            _editHandler.TryAdd("WebSource", EditWebSource);
            _editHandler.TryAdd("MySqlDatabase", EditMySqlSource);
            _editHandler.TryAdd("PostgreSQL", EditPostgreSqlSource);
            _editHandler.TryAdd("Oracle", EditOracleSource);
            _editHandler.TryAdd("ODBC", EditOdbcSource);
            _editHandler.TryAdd("SqlDatabase", EditSqlServerSource);

		}
		public void EditSqlServerSource(IContextualResourceModel resourceModel, IView view)
        {
            var def = new DbSourceDefinition
            {
                Id = resourceModel.ID,
                Path = resourceModel.GetSavePath(),
                Name = resourceModel.ResourceName,
            };
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.SqlServerSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditSqlServerResource(def, view, workSurfaceKey);
        }

        public void EditMySqlSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditMySqlResource(def, view, workSurfaceKey);
        }

        public void EditPostgreSqlSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditPostgreSqlResource(def, view, workSurfaceKey);
        }

        public void EditOracleSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditOracleResource(def, view, workSurfaceKey);
        }

        public void EditOdbcSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditOdbcResource(def, view, workSurfaceKey);
        }

        public void EditPluginSource(IContextualResourceModel resourceModel, IView view)
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
            else
            {
                if (db.AssemblyLocation.StartsWith("GAC:"))
                {
                    def.GACAssemblyName = db.AssemblyLocation;
                    def.FileSystemAssemblyName = string.Empty;
                }
            }
            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(WorkSurfaceContext.PluginSource);
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, view, workSurfaceKey);
        }

        public void EditComPluginSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, view, workSurfaceKey);
        }

        public void EditWebSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, view, workSurfaceKey);
        }

        public void EditSharePointSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, view, workSurfaceKey);
        }

        public void EditEmailSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, view, workSurfaceKey);
        }

        public void EditExchangeSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, view, workSurfaceKey);
        }

        public void EditDropBoxSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, view, workSurfaceKey);
        }

        public void EditRabbitMQSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, view, workSurfaceKey);
        }

        public void EditWcfSource(IContextualResourceModel resourceModel, IView view)
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
            workSurfaceKey.EnvironmentID = resourceModel.Environment.EnvironmentID;
            workSurfaceKey.ResourceID = resourceModel.ID;
            workSurfaceKey.ServerID = resourceModel.ServerID;
            EditResource(def, view, workSurfaceKey);
        }

        public void EditServer(IContextualResourceModel resourceModel, IView view)
        {
            var connection = new Connection(resourceModel.WorkflowXaml.ToXElement());
            string address = null;
            if (Uri.TryCreate(connection.Address, UriKind.RelativeOrAbsolute, out Uri uri))
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
            EditServer(selectedServer, ActiveServer, view);
        }

        public void EditResource(IOAuthSource selectedSource, IView view) => EditResource(selectedSource, view, null);
        public void EditResource(IOAuthSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var oauthSourceViewModel = new ManageOAuthSourceViewModel(new ManageOAuthSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), selectedSource, _shellViewModel.AsyncWorker);
            var vm = new SourceViewModel<IOAuthSource>(_shellViewModel.EventPublisher, oauthSourceViewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.OAuthSource, selectedSource.ResourceID);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public void EditResource(ISharepointServerSource selectedSource, IView view) => EditResource(selectedSource, view, null);
        public void EditResource(ISharepointServerSource selectedSource, IView view, IWorkSurfaceKey workSurfaceKey)
        {
            var viewModel = new SharepointServerSourceViewModel(new SharepointServerSourceModel(ActiveServer.UpdateRepository, ActiveServer.QueryProxy, ""), new Microsoft.Practices.Prism.PubSubEvents.EventAggregator(), selectedSource, _shellViewModel.AsyncWorker, null);
            var vm = new SourceViewModel<ISharepointServerSource>(_shellViewModel.EventPublisher, viewModel, _shellViewModel.PopupProvider, view, ActiveServer);

            workSurfaceKey = TryGetOrCreateWorkSurfaceKey(workSurfaceKey, WorkSurfaceContext.SharepointServerSource, selectedSource.Id);
            var workSurfaceContextViewModel = new WorkSurfaceContextViewModel(workSurfaceKey, vm);
            OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
        }

        public async Task<IRequestServiceNameViewModel> GetSaveViewModel(string resourcePath, string header) => await GetSaveViewModel(resourcePath, header, null);
        public async Task<IRequestServiceNameViewModel> GetSaveViewModel(string resourcePath, string header, IExplorerItemViewModel explorerItemViewModel)
        {
            var environmentViewModel = _shellViewModel.ExplorerViewModel?.Environments.FirstOrDefault(model => model.Server.EnvironmentID == ActiveServer.EnvironmentID);
            return await RequestServiceNameViewModel.CreateAsync(environmentViewModel, resourcePath, header, explorerItemViewModel);
        }

        public void AddSettingsWorkSurface()
        {
            if (_applicationTracker != null)
            {
                _applicationTracker.TrackEvent(Warewolf.Studio.Resources.Languages.TrackEventMenu.EventCategory,
                                                Warewolf.Studio.Resources.Languages.TrackEventMenu.Settings);
            }
            ActivateOrCreateUniqueWorkSurface<SettingsViewModel>(WorkSurfaceContext.Settings);
        }

        public void AddSchedulerWorkSurface()
        {
            if (_applicationTracker != null)
            {
                _applicationTracker.TrackEvent(Warewolf.Studio.Resources.Languages.TrackEventMenu.EventCategory,
                                                Warewolf.Studio.Resources.Languages.TrackEventMenu.Task);
            }
            ActivateOrCreateUniqueWorkSurface<SchedulerViewModel>(WorkSurfaceContext.Scheduler);
        }

        public void AddQueuesWorkSurface()
        {
            var vm = AddTriggersWorkSurface();
            vm.IsEventsSelected = true;
        }
        public TriggersViewModel AddTriggersWorkSurface()
        {
            if (_applicationTracker != null)
            {
                _applicationTracker.TrackEvent(Warewolf.Studio.Resources.Languages.TrackEventMenu.EventCategory,
                                                Warewolf.Studio.Resources.Languages.TrackEventMenu.Task);
            }
            return ActivateOrCreateUniqueWorkSurface<TriggersViewModel>(WorkSurfaceContext.Triggers);
        }

        public void TryShowDependencies(IContextualResourceModel resource)
        {
            if (resource != null)
            {
                ShowDependencies(true, resource, ActiveServer);
            }
        }
        public void TryCreateNewQueueEventWorkSurface(IContextualResourceModel resourceModel)
        {
            if (resourceModel != null)
            {
                //TODO: Open Tasks QueueEvent Tab with workflow populated
                AddTriggersWorkSurface();
            }
        }

        //TODO: Remove or update?
        public void TryCreateNewScheduleWorkSurface(IContextualResourceModel resourceModel)
        {
            if (resourceModel != null)
            {
                CreateNewScheduleWorkSurface(resourceModel);
            }
        }
        //TODO: Remove or update?
        void CreateNewScheduleWorkSurface(IContextualResourceModel resourceModel)
        {
            var key = WorkSurfaceKeyFactory.CreateEnvKey(WorkSurfaceContext.Scheduler, ActiveServer.EnvironmentID);
            var workSurfaceContextViewModel = FindWorkSurfaceContextViewModel(key);
            if (workSurfaceContextViewModel != null)
            {
                var workSurfaceViewModel = workSurfaceContextViewModel.WorkSurfaceViewModel;
                if (workSurfaceViewModel != null && workSurfaceViewModel is SchedulerViewModel findWorkSurfaceContextViewModel)
                {
                    if (findWorkSurfaceContextViewModel.IsDirty)
                    {
                        _shellViewModel.PopupProvider.Show(Warewolf.Studio.Resources.Languages.Core.SchedulerUnsavedTaskMessage,
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

        public void AddWorkspaceItem(IContextualResourceModel model)
        {
            _shellViewModel.GETWorkspaceItemRepository().AddWorkspaceItem(model);
        }

        void RemoveWorkspaceItem(IDesignerViewModel viewModel)
        {
            _shellViewModel.GETWorkspaceItemRepository().Remove(viewModel.ResourceModel);
        }

        public void DeleteContext(IContextualResourceModel model)
        {
            var context = FindWorkSurfaceContextViewModel(model);
            if (context == null)
            {
                return;
            }

            context.DeleteRequested = true;
            _shellViewModel.DeactivateItem(context, true);
        }

        T CreateAndActivateUniqueWorkSurface<T>(WorkSurfaceContext context)
            where T : IWorkSurfaceViewModel
        {
            var ctx = WorkSurfaceContextFactory.Create(context, out T vmr);
            AddAndActivateWorkSurface(ctx);
            return vmr;
        }

        public T ActivateOrCreateUniqueWorkSurface<T>(WorkSurfaceContext context)
            where T : IWorkSurfaceViewModel
        {
            var key = WorkSurfaceKeyFactory.CreateEnvKey(context, ActiveServer.EnvironmentID);
            var exists = ActivateAndReturnWorkSurfaceIfPresent(key);

            if (exists == null)
            {   
                return CreateAndActivateUniqueWorkSurface<T>(context);
            }
            try
            {
                return (T)exists.WorkSurfaceViewModel;
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        bool ActivateWorkSurfaceIfPresent(IContextualResourceModel resource)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(resource);
            var currentContext = FindWorkSurfaceContextViewModel(key);

            if (currentContext != null)
            {
                _shellViewModel.ActivateItem(currentContext);
                return true;
            }
            return false;
        }

        IWorkSurfaceContextViewModel ActivateAndReturnWorkSurfaceIfPresent(IWorkSurfaceKey key)
        {
            var currentContext = FindWorkSurfaceContextViewModel(key);

            if (currentContext != null)
            {
                _shellViewModel.ActivateItem(currentContext);
                return currentContext;
            }
            return null;
        }

        IWorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(IWorkSurfaceKey key) => _shellViewModel.Items.FirstOrDefault(c => WorkSurfaceKeyEqualityComparerWithContextKey.Current.Equals(key, c.WorkSurfaceKey));

        public IWorkSurfaceContextViewModel FindWorkSurfaceContextViewModel(IContextualResourceModel resource)
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
                if (resourceModel.IsNotWarewolfPath)
                {
                    _shellViewModel.CloseResource(resourceModel, resourceModel.Environment.EnvironmentID);
                }
                else
                {
                    return;
                }
            }

            var workSurfaceKey = WorkSurfaceKeyFactory.CreateKey(resourceModel);

            _shellViewModel.CanDebug = false;

            if (!isLoadingWorkspace)
            {
                OpeningWorkflowsHelper.AddWorkflow(workSurfaceKey);
                resourceModel.IsWorkflowSaved = true;
            }

            AddWorkspaceItem(resourceModel);
            var workSurfaceContextViewModel = _getWorkSurfaceContextViewModel(resourceModel, _createDesigners);
            AddAndActivateWorkSurface(workSurfaceContextViewModel);
            OpeningWorkflowsHelper.RemoveWorkflow(workSurfaceKey);
            _shellViewModel.CanDebug = true;
        }

        bool IsInOpeningState(IContextualResourceModel resource)
        {
            var key = WorkSurfaceKeyFactory.CreateKey(resource);
            return OpeningWorkflowsHelper.FetchOpeningKeys().Any(c => WorkSurfaceKeyEqualityComparer.Current.Equals(key, c));
        }

        public void AddAndActivateWorkSurface(IWorkSurfaceContextViewModel context)
        {
            if (context != null)
            {
                var found = FindWorkSurfaceContextViewModel(context.WorkSurfaceKey);
                if (found == null)
                {
                    found = context;
                    _shellViewModel.Items.Add(context);
                }
                _shellViewModel.ActivateItem(found);
            }
        }

        public void AddWorkSurface(IWorkSurfaceObject obj)
        {
            TypeSwitch.Do(obj, TypeSwitch.Case<IContextualResourceModel>(AddWorkSurfaceContext));
        }

        public bool CloseWorkSurfaceContext(IWorkSurfaceContextViewModel context, PaneClosingEventArgs e) => CloseWorkSurfaceContext(context, e, false);
        public bool CloseWorkSurfaceContext(IWorkSurfaceContextViewModel context, PaneClosingEventArgs e, bool dontPrompt)
        {
            var remove = true;
            if (context != null && !context.DeleteRequested)
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
                    if (vm.WorkSurfaceContext == WorkSurfaceContext.Triggers)
                    {
                        return CloseTriggers(vm, true);
                    }
                }
                if (vm is IStudioTab tab)
                {
                    remove = tab.DoDeactivate(true);
                    if (remove)
                    {
                        tab.Dispose();
                        tab.CloseView();
                    }
                }
            }


            return remove;
        }

        static bool CloseTriggers(IWorkSurfaceViewModel vm, bool remove)
        {
            if (vm is TriggersViewModel tasksViewModel)
            {
                remove = tasksViewModel.DoDeactivate(true);
                if (remove)
                {
                    tasksViewModel.Dispose();
                }
            }
            return remove;
        }

        static bool CloseSettings(IWorkSurfaceViewModel vm, bool remove)
        {
            if (vm is SettingsViewModel settingsViewModel)
            {
                remove = settingsViewModel.DoDeactivate(true);
                if (remove)
                {
                    settingsViewModel.Dispose();
                }
            }
            return remove;
        }

        bool CloseWorkflow(IWorkSurfaceContextViewModel context, PaneClosingEventArgs e, bool dontPrompt, IWorkSurfaceViewModel vm, ref bool remove)
        {
            var workflowVm = vm as IWorkflowDesignerViewModel;
            var resource = workflowVm?.ResourceModel;
            if (resource != null)
            {
                remove = !resource.IsAuthorized(AuthorizationContext.Contribute) || resource.IsWorkflowSaved;
                var connection = workflowVm.ResourceModel.Environment.Connection;
                if (connection != null && !connection.IsConnected)
                {
                    var result = _shellViewModel.PopupProvider.Show(string.Format(StringResources.DialogBody_DisconnectedItemNotSaved, workflowVm.ResourceModel.ResourceName),
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
                {
                    remove = true;
                }

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
                    _shellViewModel.Items.Remove(context);
                    workflowVm.Dispose();
                    if (_shellViewModel.PreviousActive != null && _shellViewModel.PreviousActive.WorkSurfaceViewModel == vm)
                    {
                        _shellViewModel.PreviousActive = null;
                    }

                    if (e != null)
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    if (e != null)
                    {
                        e.Handled = true;
                        e.Cancel = false;
                    }
                }
            }
            return true;
        }
    }
}