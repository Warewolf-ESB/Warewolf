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
using System.Threading.Tasks;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.Data;
using Warewolf.Data;
using Dev2.Common.Interfaces.Studio.Controller;

namespace Dev2.Studio.Interfaces
{
    public interface IShellViewModelEdit
    {
        void EditSqlServerResource(IDbSource selectedSource);
        void EditSqlServerResource(IDbSource selectedSource, IWorkSurfaceKey key);
        void EditMySqlResource(IDbSource selectedSource);
        void EditMySqlResource(IDbSource selectedSource, IWorkSurfaceKey key);
        void EditPostgreSqlResource(IDbSource selectedSource);
        void EditPostgreSqlResource(IDbSource selectedSource, IWorkSurfaceKey key);
        void EditOracleResource(IDbSource selectedSource);
        void EditOracleResource(IDbSource selectedSource, IWorkSurfaceKey key);
        void EditOdbcResource(IDbSource selectedSource);
        void EditOdbcResource(IDbSource selectedSource, IWorkSurfaceKey key);
        void EditResource(IPluginSource selectedSource);
        void EditResource(IPluginSource selectedSource, IWorkSurfaceKey key);
        void EditResource(IWebServiceSource selectedSource);
        void EditResource(IWebServiceSource selectedSource, IWorkSurfaceKey key);
        void EditResource(IEmailServiceSource selectedSource);
        void EditResource(IEmailServiceSource selectedSource, IWorkSurfaceKey key);
        void EditResource(IExchangeSource selectedSource);
        void EditResource(IExchangeSource selectedSource, IWorkSurfaceKey key);
        void EditResource(IElasticsearchSourceDefinition selectedSource);
        void EditResource(IElasticsearchSourceDefinition selectedSource, IWorkSurfaceKey key);
        void EditResource(IRabbitMQServiceSourceDefinition selectedSource);
        void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IWorkSurfaceKey key);
        void EditResource(IWcfServerSource selectedSource);
        void EditResource(IWcfServerSource selectedSource, IWorkSurfaceKey key);
        void EditResource(IComPluginSource selectedSource);
        void EditResource(IComPluginSource selectedSource, IWorkSurfaceKey key);
    }

    public interface IShellViewModelNew
    {
        void NewServerSource(string resourcePath);
        void NewService(string resourcePath);
        void NewSqlServerSource(string resourcePath);
        void NewMySqlSource(string resourcePath);
        void NewPostgreSqlSource(string resourcePath);
        void NewOracleSource(string resourcePath);
        void NewOdbcSource(string resourcePath);
        void NewPluginSource(string resourcePath);
        void NewWebSource(string resourcePath);
        void NewRedisSource(string resourcePath);
        void NewElasticsearchSource(string resourcePath);
        void NewEmailSource(string resourcePath);
        void NewExchangeSource(string resourcePath);
        void NewRabbitMQSource(string resourcePath);
        void NewSharepointSource(string resourcePath);
        void NewDropboxSource(string resourcePath);
        void NewWcfSource(string resourcePath);
        void NewComPluginSource(string resourcePath);
        void DuplicateResource(IExplorerItemViewModel explorerItemViewModel);
    }

    public interface IShellViewModelOpen
    {
        void OpenResourceAsync(Guid resourceId, IServer server);
        void OpenVersion(Guid resourceId, IVersionInfo versionInfo);
    }

    public interface IShellViewModel : IShellViewModelEdit, IShellViewModelNew, IShellViewModelOpen
    {
        bool ShouldUpdateActiveState { get; set; }
        void ShowPopup(IPopupMessage getDuplicateMessage);
        void SetActiveServer(Guid environmentId);
        void Debug();
        void ShowAboutBox();
        void ShowDependencies(Guid resourceId, IServer server, bool isSource);
        void DeployResources(Guid sourceEnvironmentId, Guid destinationEnvironmentId, IList<Guid> resources, bool deployTests, bool deployTriggers);
        void AddDeploySurface(IEnumerable<IExplorerTreeItem> items);
        void OpenResource(Guid resourceId, Guid environmentId, IServer activeServer);
        void OpenResource(Guid resourceId, Guid environmentId, IServer activeServer, IContextualResourceModel contextualResourceModel);
        void OpenMergeConflictsView(IExplorerItemViewModel currentResource, Guid differenceResourceId, IServer server);
        void OpenMergeConflictsView(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadFromServer);
        void CloseResource(Guid resourceId, Guid environmentId);
        void CloseResource(IContextualResourceModel currentResourceModel, Guid environmentId);
        void UpdateCurrentDataListWithObjectFromJson(string parentObjectName, string json);
        void ViewSwagger(Guid resourceId, IServer server);
        void ViewApisJson(string resourcePath, Uri webServerUri);
        void CreateTest(Guid resourceId);
        void OpenSelectedTest(Guid resourceId, string testName);
        void CloseResourceTestView(Guid resourceId, Guid serverId, Guid environmentId);
        void CloseResourceMergeView(Guid resourceId, Guid serverId, Guid environmentId);
        void BrowserDebug(Guid resourceId, IServer server);
        void StudioDebug(Guid resourceId, IServer server);
        void CopyUrlLink(Guid resourceId, IServer server);
        void NewSchedule(Guid resourceId);
        void NewQueueEvent(Guid resourceId);
        void SetRefreshExplorerState(bool refresh);
        void ResetMainView();
        void OnActiveServerChanged();
        ICommand DeployCommand { get; }
        ICommand MergeCommand { get; }
        ICommand ExitCommand { get; }
        IServer ActiveServer { get; set; }
        IContextualResourceModel DeployResource { get; set; }
        void AddWorkSurfaceContext(IContextualResourceModel resourceModel);
        bool MenuExpanded { get; set; }
        double MenuPanelWidth { get; set; }
        IAuthorizeCommand SaveCommand { get; }
        IAuthorizeCommand DebugCommand { get; }
        IAuthorizeCommand SettingsCommand { get; }
        ICommand SearchCommand { get; }
        ICommand AddWorkflowCommand { get; }
        ICommand RunCoverageCommand { get; }
        ICommand RunAllTestsCommand { get; }
        IAuthorizeCommand SchedulerCommand { get; }
        IAuthorizeCommand QueueEventsCommand { get; }
        IAuthorizeCommand TasksCommand { get; }
        IToolboxViewModel ToolboxViewModel { get; }
        IHelpWindowViewModel HelpViewModel { get; }
        ICommand ShowStartPageCommand { get; }
        IAuthorizeCommand<string> NewServiceCommand { get; }
        IAuthorizeCommand<string> NewPluginSourceCommand { get; }
        IAuthorizeCommand<string> NewSqlServerSourceCommand { get; }
        IAuthorizeCommand<string> NewMySqlSourceCommand { get; }
        IAuthorizeCommand<string> NewPostgreSqlSourceCommand { get; }
        IAuthorizeCommand<string> NewOracleSourceCommand { get; }
        IAuthorizeCommand<string> NewOdbcSourceCommand { get; }
        IAuthorizeCommand<string> NewWebSourceCommand { get; }
        IAuthorizeCommand<string> NewServerSourceCommand { get; }
        IAuthorizeCommand<string> NewEmailSourceCommand { get; }
        IAuthorizeCommand<string> NewExchangeSourceCommand { get; }
        IAuthorizeCommand<string> NewRabbitMQSourceCommand { get; }
        IAuthorizeCommand<string> NewSharepointSourceCommand { get; }
        IAuthorizeCommand<string> NewDropboxSourceCommand { get; }
        IAuthorizeCommand<string> NewWcfSourceCommand { get; }
        IExplorerViewModel ExplorerViewModel { get; set; }
        IPopupController PopupProvider { get; set; }
        IServer LocalhostServer { get; }
        bool ResourceCalled { get; set; }

        void DisplayDialogForNewVersion();
        Task<bool> CheckForNewVersionAsync();
        bool ShowDeleteDialogForFolder(string folderBeingDeleted);
        IWorkflowDesignerViewModel CreateNewDesigner(IContextualResourceModel resourceModel);
        void OpenCurrentVersion(Guid resourceId, Guid environmentId);
        IWorkflowDesignerViewModel GetWorkflowDesigner();
        void OpenMergeDialogView(IExplorerItemViewModel currentResource);
        void UpdateExplorerWorkflowChanges(Guid resourceId);
        IResource CreateResourceFromStreamContent(string resourceContent);
        IResource GetResource(string resourceId);
        List<IServiceInputBase> GetInputsFromWorkflow(Guid resourceId);
    }
}
