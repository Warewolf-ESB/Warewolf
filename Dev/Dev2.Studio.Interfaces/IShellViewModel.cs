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
using Dev2.Common.Interfaces.Versioning;



namespace Dev2.Studio.Interfaces
{
    public interface IShellViewModelEdit
    {
        void EditSqlServerResource(IDbSource selectedSource, IWorkSurfaceKey key = null);
        void EditMySqlResource(IDbSource selectedSource, IWorkSurfaceKey key = null);
        void EditPostgreSqlResource(IDbSource selectedSource, IWorkSurfaceKey key = null);
        void EditOracleResource(IDbSource selectedSource, IWorkSurfaceKey key = null);
        void EditOdbcResource(IDbSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IPluginSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IWebServiceSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IEmailServiceSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IExchangeSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IWcfServerSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IComPluginSource selectedSource, IWorkSurfaceKey key = null);
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
        void DeployResources(Guid sourceEnvironmentId, Guid destinationEnvironmentId, IList<Guid> resources, bool deployTests);
        void AddDeploySurface(IEnumerable<IExplorerTreeItem> items);
        void OpenResource(Guid resourceId, Guid environmentId, IServer activeServer);
        void OpenMergeConflictsView(IExplorerItemViewModel currentResource, Guid differenceResourceId, IServer server);
       void OpenMergeConflictsView(IContextualResourceModel currentResourceModel, IContextualResourceModel differenceResourceModel, bool loadFromServer);
        void CloseResource(Guid resourceId, Guid environmentId);
        void UpdateCurrentDataListWithObjectFromJson(string parentObjectName,string json);
        void ViewSwagger(Guid resourceId, IServer server);
        void ViewApisJson(string resourcePath, Uri webServerUri);
        void CreateTest(Guid resourceId);
        void RunAllTests(Guid resourceId);
        void CloseResourceTestView(Guid resourceId, Guid serverId, Guid environmentId);
        void BrowserDebug(Guid resourceId, IServer server);
        void StudioDebug(Guid resourceId, IServer server);
        void CopyUrlLink(Guid resourceId, IServer server);
        void NewSchedule(Guid resourceId);
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
        IAuthorizeCommand SchedulerCommand { get; }
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
        IServer LocalhostServer { get; }
        bool ResourceCalled { get; set; }

        void DisplayDialogForNewVersion();
        Task<bool> CheckForNewVersion();
        bool ShowDeleteDialogForFolder(string folderBeingDeleted);
        IWorkflowDesignerViewModel CreateNewDesigner(IContextualResourceModel resourceModel);
        void OpenCurrentVersion(Guid resourceId, Guid environmentId);
        IWorkflowDesignerViewModel GetWorkflowDesigner();
        void OpenMergeDialogView(IExplorerItemViewModel currentResource);
    }
}