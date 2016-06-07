using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Common.Interfaces.Versioning;

namespace Dev2.Common.Interfaces
{
    public interface IShellViewModel
    {
        void EditResource(IDbSource selectedSource, IWorkSurfaceKey key = null);

        void EditResource(IPluginSource selectedSource, IWorkSurfaceKey key = null);

        void EditResource(IWebServiceSource selectedSource, IWorkSurfaceKey key = null);

        void EditResource(IEmailServiceSource selectedSource, IWorkSurfaceKey key = null);
        void EditResource(IExchangeSource selectedSource, IWorkSurfaceKey key = null);

        void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IWorkSurfaceKey key = null);

        void EditResource(IWcfServerSource selectedSource, IWorkSurfaceKey key = null);

        void NewResource(string resourceType, string resourcePath);

        string OpenPasteWindow(string current);

        IServer LocalhostServer { get; }
        IServer ActiveServer { get; set; }

        void OpenResource(Guid resourceId, IServer server);
        void OpenResourceAsync(Guid resourceId, IServer server);

        void ShowPopup(IPopupMessage getDuplicateMessage);

        void SetActiveEnvironment(Guid environmentId);

        void SetActiveServer(IServer server);

        void Debug();

        void ShowAboutBox();

        void ShowDependencies(Guid resourceId, IServer server);


        void DeployResources(Guid sourceEnvironmentId, Guid destinationEnvironmentId, IList<Guid> resources);

        void AddDeploySurface(IEnumerable<IExplorerTreeItem> items);

        void OpenVersion(Guid resourceId, IVersionInfo versionInfo);

        void OpenResource(Guid resourceId, Guid environmentId);
        void CloseResource(Guid resourceId, Guid environmentId);

        void UpdateCurrentDataListWithObjectFromJson(string parentObjectName,string json);
    }
}