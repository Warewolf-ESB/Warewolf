using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceCatalog :
          IResourceWorkspaceProvider
        , IResourceSyncProvider
        , IResourceCopyProvider
        , IResourceRenameProvider
        , IResourceDeleteProvider
        , IResourceLoadProvider
        , IResourceSaveProvider
        , IResourceDuplicateProvider

    {
        ConcurrentDictionary<Guid, List<IResource>> WorkspaceResources { get; }
    }
}