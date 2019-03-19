#pragma warning disable
using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceRenameProvider
    {
        ResourceCatalogResult RenameResource(Guid workspaceID, Guid? resourceID, string newName, string resourcePath);
        ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory);
        ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory, List<IResource> resourcesToUpdate);
    }
}