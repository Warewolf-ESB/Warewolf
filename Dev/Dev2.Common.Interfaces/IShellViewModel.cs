using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.Versioning;
// ReSharper disable InconsistentNaming

namespace Dev2.Common.Interfaces
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
        IServer LocalhostServer { get; }
        IServer ActiveServer { get; set; }
        bool ShouldUpdateActiveState { get; set; }
        void ShowPopup(IPopupMessage getDuplicateMessage);
        void SetActiveEnvironment(Guid environmentId);
        void SetActiveServer(IServer server);
        void Debug();
        void ShowAboutBox();
        void ShowDependencies(Guid resourceId, IServer server, bool isSource);
        void DeployResources(Guid sourceEnvironmentId, Guid destinationEnvironmentId, IList<Guid> resources, bool deployTests);
        void AddDeploySurface(IEnumerable<IExplorerTreeItem> items);
        void OpenResource(Guid resourceId, Guid environmentId, IServer activeServer);
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
        void OnActiveEnvironmentChanged();
    }
}