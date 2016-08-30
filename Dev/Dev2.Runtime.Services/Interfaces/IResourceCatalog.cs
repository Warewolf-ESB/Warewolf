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
       
    }
}