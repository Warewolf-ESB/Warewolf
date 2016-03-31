using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Versioning;
using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IShellViewModel
    {
        IServer LocalhostServer { get; }
        
        IServer ActiveServer { get; set; }

        void EditResource(IDbSource selectedSource, IWorkSurfaceKey key = null);

        void EditResource(IPluginSource selectedSource, IWorkSurfaceKey key = null);

        void EditResource(IWebServiceSource selectedSource, IWorkSurfaceKey key = null);

        void EditResource(IDatabaseService selectedSource, IWorkSurfaceKey key = null);

        void EditResource(IEmailServiceSource selectedSource, IWorkSurfaceKey key = null);

        void EditResource(IRabbitMQServiceSourceDefinition selectedSource, IWorkSurfaceKey key = null);

        void NewResource(string resourceType, string resourcePath);

        string OpenPasteWindow(string current);

        void OpenResource(Guid resourceId, Guid environmentId);

        void OpenResource(Guid resourceId, IServer server);

        void OpenResourceAsync(Guid resourceId, IServer server);

        void ShowPopup(IPopupMessage getDuplicateMessage);

        void SetActiveEnvironment(Guid environmentId);

        void SetActiveServer(IServer server);

        void Debug();

        void ShowAboutBox();

        void ShowDependencies(Guid resourceId, IServer server);

        void ShowDependencies(Guid resourceId, bool dependsOnMe);

        void DeployResources(Guid sourceEnvironmentId, Guid destinationEnvironmentId, IList<Guid> resources);

        void AddDeploySurface(IEnumerable<IExplorerTreeItem> items);

        void OpenVersion(Guid resourceId, IVersionInfo versionInfo);

        void CloseResource(Guid resourceId, Guid environmentId);
    }
}