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
        void EditResource(IDbSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IPluginSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IWebServiceSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IEmailServiceSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IExchangeSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IWcfServerSource selectedSource, IWorkSurfaceKey key = null);
    }

    public interface IShellViewModelNew
    {
        void NewServerSource(string resourcePath);
        void NewService(string resourcePath);
        void NewDatabaseSource(string resourcePath);
        void NewPluginSource(string resourcePath);
        void NewWebSource(string resourcePath);
        void NewEmailSource(string resourcePath);
        void NewExchangeSource(string resourcePath);
        void NewRabbitMQSource(string resourcePath);
        void NewSharepointSource(string resourcePath);
        void NewDropboxSource(string resourcePath);
        void NewWcfSource(string resourcePath);
        void NewComPluginSource(string resourcePath);
    }

    public interface IShellViewModelOpen
    {
        string OpenPasteWindow(string current);
        void OpenResource(Guid resourceId, IServer server);
        void OpenResourceAsync(Guid resourceId, IServer server);
        void OpenVersion(Guid resourceId, IVersionInfo versionInfo);
    }

    public interface IShellViewModel : IShellViewModelEdit, IShellViewModelNew, IShellViewModelOpen
    {
        IServer LocalhostServer { get; }
        IServer ActiveServer { get; set; }
        void ShowPopup(IPopupMessage getDuplicateMessage);
        void SetActiveEnvironment(Guid environmentId);
        void SetActiveServer(IServer server);
        void Debug();
        void ShowAboutBox();
        void ShowDependencies(Guid resourceId, IServer server);
        void DeployResources(Guid sourceEnvironmentId, Guid destinationEnvironmentId, IList<Guid> resources);
        void AddDeploySurface(IEnumerable<IExplorerTreeItem> items);
        void OpenResource(Guid resourceId, Guid environmentId);
        void CloseResource(Guid resourceId, Guid environmentId);
        void UpdateCurrentDataListWithObjectFromJson(string parentObjectName,string json);
    }
}