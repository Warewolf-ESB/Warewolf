using System;
using Dev2.Common.Interfaces.Security;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    /// <summary>
    /// Manages resource and server permissions 
    /// </summary>
    public interface ISecurityManager
    {
        /// <summary>
        /// get the permissions for a resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        Permissions GetResourcePermissions(Guid resourceId);
        /// <summary>
        /// Save the permissions of a resource
        /// </summary>
        /// <param name="resourceId">the resource id</param>
        /// <param name="permissions">the persmissions</param>
        void SaveResourcePermissions(Guid resourceId, Permissions permissions);
        /// <summary>
        /// Gets the permissions available on a server
        /// </summary>
        /// <returns></returns>
        Permissions GetServerPermissions();

        /// <summary>
        /// Set the permissions for a resource
        /// </summary>
        /// <param name="resourceId">the resource</param>
        /// <param name="permissions">the permissions</param>
        void SetResourcePermissions(Guid resourceId, Permissions permissions);
    }
}