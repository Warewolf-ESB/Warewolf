using System;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Warewolf.Studio.ServerProxyLayer
{
    public class SecurityManagerProxy:ISecurityManager
    {
        #region Implementation of ISecurityManager

        /// <summary>
        /// get the permissions for a resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public Permissions GetResourcePermissions(Guid resourceId)
        {
            return Permissions.None;
        }

        /// <summary>
        /// Save the permissions of a resource
        /// </summary>
        /// <param name="resourceId">the resource id</param>
        /// <param name="permissions">the persmissions</param>
        public void SaveResourcePermissions(Guid resourceId, Permissions permissions)
        {
        }

        /// <summary>
        /// Gets the permissions available on a server
        /// </summary>
        /// <returns></returns>
        public Permissions GetServerPermissions()
        {
            return Permissions.None;
        }

        /// <summary>
        /// Set the permissions for a resource
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <param name="permissions">the permissions</param>
        public void SetResourcePermissions(Guid resourceId, Permissions permissions)
        {
        }

        #endregion
    }
}
