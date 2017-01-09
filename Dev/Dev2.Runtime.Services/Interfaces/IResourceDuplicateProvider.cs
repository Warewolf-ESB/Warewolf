using System;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceDuplicateProvider
    {
        ResourceCatalogDuplicateResult DuplicateResource(Guid resourceId, string destinationPath, string newName);
        ResourceCatalogDuplicateResult DuplicateFolder(string sourcePath, string destinationPath, string newName, bool fixRefences);

    }
}