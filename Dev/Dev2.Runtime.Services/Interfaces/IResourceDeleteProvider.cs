using System;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceDeleteProvider
    {
        ResourceCatalogResult DeleteResource(Guid workspaceID, string resourceName, string type, bool deleteVersions = true);
        ResourceCatalogResult DeleteResource(Guid workspaceID, Guid resourceID, string type, bool deleteVersions = true);
        
    }
}