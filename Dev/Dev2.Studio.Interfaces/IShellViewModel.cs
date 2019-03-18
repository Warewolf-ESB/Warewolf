#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using Dev2.Common.Interfaces.Data;

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
        void RunAllTests(string ResourcePath, Guid resourceId);
        void CloseResourceTestView(Guid resourceId, Guid serverId, Guid environmentId);
        void CloseResourceMergeView(Guid resourceId, Guid serverId, Guid environmentId);
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
        ICommand SearchCommand { get; }
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
        Task<bool> CheckForNewVersionAsync();
        bool ShowDeleteDialogForFolder(string folderBeingDeleted);
        IWorkflowDesignerViewModel CreateNewDesigner(IContextualResourceModel resourceModel);
        void OpenCurrentVersion(Guid resourceId, Guid environmentId);
        IWorkflowDesignerViewModel GetWorkflowDesigner();
        void OpenMergeDialogView(IExplorerItemViewModel currentResource);
        void UpdateExplorerWorkflowChanges(Guid resourceId);
        IResource CreateResourceFromStreamContent(string resourceContent);
    }
}