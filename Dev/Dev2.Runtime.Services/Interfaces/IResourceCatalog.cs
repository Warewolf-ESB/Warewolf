#pragma warning disable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Services.Security;
using Warewolf.Data;
using Warewolf.ResourceManagement;
using Warewolf.Services;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceCatalog :
          IResourceWorkspaceProvider
        , IResourceSyncProvider
        , IResourceRenameProvider
        , IResourceDeleteProvider
        , IResourceLoadProvider
        , IResourceSaveProvider
        , IResourceDuplicateProvider

    {
        void AddToActivityCache(IResource resource);
        ConcurrentDictionary<Guid, List<IResource>> WorkspaceResources { get; }
        IDev2Activity Parse(Guid workspaceID, Guid resourceID);
        IDev2Activity Parse(Guid workspaceID, Guid resourceID, string executionId);
        IDev2Activity Parse(Guid workspaceID, Guid resourceID, string executionId, IResource resourceOverride);
        void CleanUpOldVersionControlStructure();
        IResourceActivityCache GetResourceActivityCache(Guid workspaceID);
        void RemoveFromResourceActivityCache(Guid workspaceID, IResource resource);
        string SetResourceFilePath(Guid workspaceID, IResource resource, ref string savedPath);
        ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents);
        ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath);
        ResourceCatalogResult SaveImpl(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath, string reason);
        IList<IResource> LoadExamplesViaBuilder(string releasePath);
        void LoadServerActivityCache();
        void Reload();
        DynamicService GetService(Guid workspaceID, Guid resourceID, string resourceName);
        int GetLatestVersionNumberForResource(Guid resourceId);
        IContextualResourceCatalog NewContextualResourceCatalog(IAuthorizationService authService, Guid workspaceId);
        IWarewolfWorkflow GetWorkflow(Guid resourceId);
    }

    public interface IResourceCatalogFactory
    {
        IResourceCatalog New();
    }
}