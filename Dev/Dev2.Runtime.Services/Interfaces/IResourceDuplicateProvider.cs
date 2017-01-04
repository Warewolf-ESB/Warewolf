using System;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceDuplicateProvider
    {
        IResource DuplicateResource(Guid resourceId, string destinationPath, string newName);
        ResourceCatalogResult DuplicateFolder(string sourcePath, string destinationPath, string newName, bool fixRefences);

    }
}