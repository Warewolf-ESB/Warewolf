using Dev2.Workspaces;
using System.Collections.Generic;
using Dev2.Studio.Core.AppResources.Enums;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IResourceRepository : IFrameworkRepository<IResourceModel>
    {
        List<IResourceModel> ReloadResource(string resourceName, ResourceType resourceType, IEqualityComparer<IResourceModel> equalityComparer);
        void UpdateWorkspace(IList<IWorkspaceItem> workspaceItems);
        void DeployResource(IResourceModel resource);
        UnlimitedObject DeleteResource(IResourceModel resource);
        bool IsReservedService(string resourceName);
        bool IsWorkflow(string resourceName);
        void Add(IResourceModel resource);
    }
}
