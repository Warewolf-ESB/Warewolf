using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceSaveProvider
    {
        ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string savedPath, string reason = "", string user = "");
        ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, string savedPath, string reason = "", string user = "");
        Action<IResource> ResourceSaved { get; set; }
        Action<Guid, IList<ICompileMessageTO>> SendResourceMessages { get; set; }

        ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, StringBuilder contents, string savedPath, string reason = "", string user = "");

        string SetResourceFilePath(Guid workspaceID, IResource resource, ref string savedPath);
    }
}