
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
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Data.ServiceModel;

namespace Dev2.Services
{
    /// <summary>
    /// Resource Services - Was for ResourceHub, NOW to give fixed interfacing from Studio via an EsbServiceCatalog 
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// Deletes the resource.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IExecuteMessage DeleteResource(Guid workspaceID, string resourceName, string type);

        /// <summary>
        /// Finds the dependencies.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="dependsOnMe">if set to <c>true</c> [depends configuration memory].</param>
        /// <returns></returns>
        IExecuteMessage FindDependencies(Guid workspaceID, string resourceName, bool dependsOnMe);

        /// <summary>
        /// Fetches the resource definition.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceID">The resource unique identifier.</param>
        /// <returns></returns>
        IExecuteMessage FetchResourceDefinition(Guid workspaceID, Guid resourceID);

        /// <summary>
        /// Finds the resource.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns></returns>
        IList<SerializableResource> FindResource(Guid workspaceID, string resourceName, string resourceType);

        /// <summary>
        /// Finds the resources by unique identifier.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="guidCsv">The unique identifier CSV.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns></returns>
        IList<SerializableResource> FindResourcesByID(Guid workspaceID, string guidCsv, string resourceType);

        /// <summary>
        /// Finds the type of the sources by.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="typeOf">The type of.</param>
        /// <returns></returns>
        IEnumerable<T> FindSourcesByType<T>(Guid workspaceID, string typeOf);

        /// <summary>
        /// Gets the dependancies configuration list.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceNames">The resource names.</param>
        /// <param name="dependsOnMe">if set to <c>true</c> [depends configuration memory].</param>
        /// <returns></returns>
        IList<string> GetDependanciesOnList(Guid workspaceID, List<string> resourceNames, bool dependsOnMe);

        /// <summary>
        /// Reloads the resource.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceID">The resource unique identifier.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns></returns>
        IExecuteMessage ReloadResource(Guid workspaceID, string resourceID, string resourceType);

        /// <summary>
        /// Renames the resource category.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="oldCategory">The old category.</param>
        /// <param name="newCategory">The new category.</param>
        /// <param name="resourceType">Type of the resource.</param>
        /// <returns></returns>
        IExecuteMessage RenameResourceCategory(Guid workspaceID, string oldCategory, string newCategory, string resourceType);

        /// <summary>
        /// Renames the resource category.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceID">The resource unique identifier.</param>
        /// <param name="newName">The new name.</param>
        /// <returns></returns>
        IExecuteMessage RenameResource(Guid workspaceID, Guid resourceID, string newName);

        /// <summary>
        /// Renames the resource category.
        /// </summary>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="resourceXml">The resource XML.</param>
        /// <returns></returns>
        IExecuteMessage SaveResource(Guid workspaceID, string resourceXml);
    }
}
