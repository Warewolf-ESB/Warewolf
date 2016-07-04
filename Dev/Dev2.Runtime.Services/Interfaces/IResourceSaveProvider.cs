using System;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceSaveProvider
    {
        ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string userRoles = null, string reason = "", string user = "");
        ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, string userRoles = null, string reason = "", string user = "");
    }
}