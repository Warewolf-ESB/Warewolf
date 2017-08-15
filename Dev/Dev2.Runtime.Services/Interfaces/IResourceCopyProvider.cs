using System;
using Dev2.Common.Interfaces.Data;



namespace Dev2.Runtime.Interfaces
{
    public interface IResourceCopyProvider
    {
        bool CopyResource(Guid resourceID, Guid sourceWorkspaceID, Guid targetWorkspaceID, string userRoles = null);
        bool CopyResource(IResource resource, Guid targetWorkspaceID, string userRoles = null);
    }
}