
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.DynamicServices.Objects.Base;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Hosting
{
    // PBI 953 - 2013.05.16 - TWR - Created
    public interface IResourceCatalog
    {
        int WorkspaceCount { get; }

        int GetResourceCount(Guid workspaceID);

        void RemoveWorkspace(Guid workspaceID);

        IResource GetResource(Guid workspaceID, string resourceName, ResourceType resourceType = ResourceType.Unknown, string version = null);

        IResource GetResource(Guid workspaceID, Guid resourceID);

        /// <summary>
        /// Gets the contents of the resource with the given name.
        /// </summary>
        /// <param name="workspaceID">The workspace ID to be queried.</param>
        /// <param name="resourceID">The resource ID to be queried.</param>
        /// <returns>
        /// The resource's contents or <code>string.Empty</code> if not found.
        /// </returns>
        StringBuilder GetResourceContents(Guid workspaceID, Guid resourceID);

        /// <summary>
        /// Gets the resource's contents.
        /// </summary>
        /// <param name="resource">The resource to be queried.</param>
        /// <returns>
        /// The resource's contents or <code>string.Empty</code> if not found.
        /// </returns>
        StringBuilder GetResourceContents(IResource resource);

        /// <summary>
        /// Gets the contents of the resource with the given guids.
        /// </summary>
        /// <param name="workspaceID">The workspace ID to be queried.</param>
        /// <param name="guidCsv">The guids to be queried.</param>
        /// <param name="type">The type string: WorkflowService, Service, Source, ReservedService or *, to be queried.</param>
        /// <returns>The resource's contents or <code>string.Empty</code> if not found.</returns>
        /// <exception cref="System.ArgumentNullException">type</exception>
        StringBuilder GetPayload(Guid workspaceID, string guidCsv, string type);

        /// <summary>
        /// Gets the contents of the resources with the given source type.
        /// </summary>
        /// <param name="workspaceID">The workspace ID to be queried.</param>
        /// <param name="sourceType">The type of the source to be queried.</param>
        /// <returns>The resource's contents or <code>string.Empty</code> if not found.</returns>
        StringBuilder GetPayload(Guid workspaceID, enSourceType sourceType);

        /// <summary>
        /// Gets the contents of the resource with the given name and type (WorkflowService, Service, Source, ReservedService or *).
        /// </summary>
        /// <param name="workspaceID">The workspace ID to be queried.</param>
        /// <param name="resourceName">The name of the resource to be queried.</param>
        /// <param name="type">The type string: WorkflowService, Service, Source, ReservedService or *, to be queried.</param>
        /// <param name="userRoles">The user roles to be queried.</param>
        /// <param name="useContains"><code>true</code> if matching resource name's should contain the given <paramref name="resourceName"/>;
        /// <code>false</code> if resource name's must exactly match the given <paramref name="resourceName"/>.</param>
        /// <returns>The resource's contents or <code>string.Empty</code> if not found.</returns>
        /// <exception cref="System.Runtime.Serialization.InvalidDataContractException">ResourceName or Type is missing from the request</exception>
        StringBuilder GetPayload(Guid workspaceID, string resourceName, string type, string userRoles, bool useContains = true);

        /// <summary>
        /// Loads the workspace.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        void LoadWorkspace(Guid workspaceID);

        /// <summary>
        /// Loads the workspace via builder.
        /// </summary>
        /// <param name="workspacePath">The workspace path.</param>
        /// <param name="folders">The folders.</param>
        /// <returns></returns>
        IList<IResource> LoadWorkspaceViaBuilder(string workspacePath, params string[] folders);

        bool CopyResource(Guid resourceID, Guid sourceWorkspaceID, Guid targetWorkspaceID, string userRoles = null);

        bool CopyResource(IResource resource, Guid targetWorkspaceID, string userRoles = null);

        ResourceCatalogResult SaveResource(Guid workspaceID, StringBuilder resourceXml, string userRoles = null,string reason ="",string user ="");

        ResourceCatalogResult SaveResource(Guid workspaceID, IResource resource, string userRoles = null, string reason ="",string user ="");

        ResourceCatalogResult DeleteResource(Guid workspaceID, string resourceName, string type, string userRoles = null, bool deleteVersions = true);

       // bool RollbackResource(Guid workspaceID, Guid resourceID, Version fromVersion, Version toVersion);

        void SyncTo(string sourceWorkspacePath, string targetWorkspacePath, bool overwrite = true, bool delete = true, IList<string> filesToIgnore = null);

        List<TServiceType> GetDynamicObjects<TServiceType>(Guid workspaceID, string resourceName, bool useContains = false)
            where TServiceType : DynamicServiceObjectBase;

        List<DynamicServiceObjectBase> GetDynamicObjects(IResource resource);

        List<DynamicServiceObjectBase> GetDynamicObjects(Guid workspaceID);

        List<DynamicServiceObjectBase> GetDynamicObjects(IEnumerable<IResource> resources);

        List<Guid> GetDependants(Guid workspaceID, Guid? resourceId);

        List<ResourceForTree> GetDependentsAsResourceForTrees(Guid workspaceID, Guid resourceId);

        IList<IResource> GetResourceList(Guid workspaceId);

        ResourceCatalogResult RenameResource(Guid workspaceID, Guid? resourceID, string newName);

        ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory);

        ResourceCatalogResult RenameCategory(Guid workspaceID, string oldCategory, string newCategory, List<IResource> resourcesToUpdate);

        T GetResource<T>(Guid workspaceID, Guid serviceID) where T : Resource, new();

        T GetResource<T>(Guid workspaceID, string resourceName) where T : Resource, new();
    }
}
