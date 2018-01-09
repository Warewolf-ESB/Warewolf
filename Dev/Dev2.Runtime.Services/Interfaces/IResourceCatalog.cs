using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

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
        void CleanUpOldVersionControlStructure();
    }
}