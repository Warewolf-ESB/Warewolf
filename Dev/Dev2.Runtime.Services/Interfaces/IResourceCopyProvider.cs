using System;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Runtime.Interfaces
{
    public interface IResourceCopyProvider
    {
        bool CopyResource(Guid resourceID, Guid sourceWorkspaceID, Guid targetWorkspaceID);
        bool CopyResource(Guid resourceID, Guid sourceWorkspaceID, Guid targetWorkspaceID, string userRoles);
        bool CopyResource(IResource resource, Guid targetWorkspaceID);
        bool CopyResource(IResource resource, Guid targetWorkspaceID, string userRoles);
    }
}