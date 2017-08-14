using System;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;



namespace Dev2.Runtime.ResourceCatalogImpl
{
    internal class ResourceCopyProvider: IResourceCopyProvider
    {
        private readonly IResourceCatalog _resourceCatalog;

        public ResourceCopyProvider(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }
        #region Implementation of IResourceCopyProvider

        public bool CopyResource(Guid resourceID, Guid sourceWorkspaceID, Guid targetWorkspaceID, string userRoles = null)
        {
            var resource = _resourceCatalog.GetResource(sourceWorkspaceID, resourceID);
            return CopyResource(resource, targetWorkspaceID, userRoles);
        }

        public bool CopyResource(IResource resource, Guid targetWorkspaceID, string userRoles = null)
        {
            if (resource != null)
            {
                var copy = new Resource(resource);
                var globalResource = _resourceCatalog.GetResource(Guid.Empty, resource.ResourceID);
                if (globalResource != null)
                {

                    copy.VersionInfo = globalResource.VersionInfo;
                }
                var contents = _resourceCatalog.GetResourceContents(resource);
                var saveResult = ((ResourceCatalog)_resourceCatalog).SaveImpl(targetWorkspaceID, copy, contents);
                return saveResult.Status == ExecStatus.Success;
            }
            return false;
        }

        #endregion
    }
}