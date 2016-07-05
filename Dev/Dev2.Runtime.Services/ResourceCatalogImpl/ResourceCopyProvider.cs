using System;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;

// ReSharper disable PrivateMembersMustHaveComments
// ReSharper disable PublicMembersMustHaveComments
namespace Dev2.Runtime.ResourceCatalogImpl
{
    public class ResourceCopyProvider: IResourceCopyProvider
    {
        #region Implementation of IResourceCopyProvider

        public bool CopyResource(Guid resourceID, Guid sourceWorkspaceID, Guid targetWorkspaceID, string userRoles = null)
        {
            var resource = ResourceCatalog.Instance.GetResource(sourceWorkspaceID, resourceID);
            return CopyResource(resource, targetWorkspaceID, userRoles);
        }

        public bool CopyResource(IResource resource, Guid targetWorkspaceID, string userRoles = null)
        {
            if (resource != null)
            {
                var copy = new Resource(resource);
                var globalResource = ResourceCatalog.Instance.GetResource(Guid.Empty, resource.ResourceID);
                if (globalResource != null)
                {

                    copy.VersionInfo = globalResource.VersionInfo;
                }
                var contents = ResourceCatalog.Instance.GetResourceContents(resource);
                var saveResult = ResourceCatalog.Instance.SaveImpl(targetWorkspaceID, copy, contents);
                return saveResult.Status == ExecStatus.Success;
            }
            return false;
        }

        #endregion
    }
}