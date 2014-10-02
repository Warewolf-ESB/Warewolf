
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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Factory
{
    /// <summary>
    /// Used to generate unique keys for every work surface
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/28/2013</date>
    public static class WorkSurfaceKeyFactory
    {
        /// <summary>
        /// Create a key which are unique to the entire studio
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2/28/2013</date>
        public static WorkSurfaceKey CreateKey(WorkSurfaceContext context)
        {
            return new WorkSurfaceKey
                {
                    WorkSurfaceContext = context,
                    ResourceID = Guid.Empty,
                    ServerID = Guid.Empty
                };
        }

        /// <summary>
        /// Creates a key for a worksurface that identifies a unique resource
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="resourceID">The resource ID.</param>
        /// <param name="serverID">The server ID.</param>
        /// <param name="environmentID">The environment ID.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2/28/2013</date>
        public static WorkSurfaceKey CreateKey(WorkSurfaceContext context, Guid resourceID, Guid serverID, Guid? environmentID = null)
        {
            return new WorkSurfaceKey
            {
                WorkSurfaceContext = context,
                ResourceID = resourceID,
                ServerID = serverID,
                EnvironmentID = environmentID
            };
        }

        /// <summary>
        /// Creates a key used for worksurfaces unique to a specific server
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="serverID">The server ID.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2/28/2013</date>
        public static WorkSurfaceKey CreateKey(WorkSurfaceContext context, Guid serverID)
        {
            return new WorkSurfaceKey
            {
                WorkSurfaceContext = context,
                ResourceID = Guid.Empty,
                ServerID = serverID
            };
        }

        /// <summary>
        /// Creates the for a specific Contextual Resource
        /// </summary>
        /// <param name="resourceModel">The resource model.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>3/4/2013</date>
        public static WorkSurfaceKey CreateKey(IContextualResourceModel resourceModel)
        {
            var context = resourceModel.ResourceType.ToWorkSurfaceContext();
            return new WorkSurfaceKey
                {
                    WorkSurfaceContext = context,
                    ResourceID = resourceModel.ID,
                    ServerID = resourceModel.ServerID,
                    EnvironmentID = resourceModel.Environment.ID
                };
        }

        public static WorkSurfaceKey CreateKey(IDebugState debugState)
        {
            var origin = debugState.WorkspaceID;
            if(origin != Guid.Empty)
            {
                IEnvironmentModel environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.Connection.WorkspaceID == origin);
                Guid environmentID = environmentModel.ID;
                return new WorkSurfaceKey
                {
                    WorkSurfaceContext = WorkSurfaceContext.Workflow,
                    ResourceID = debugState.OriginatingResourceID,
                    ServerID = debugState.ServerID,
                    EnvironmentID = environmentID
                };
            }
            return new WorkSurfaceKey
                {
                    WorkSurfaceContext = WorkSurfaceContext.Workflow,
                    ResourceID = debugState.OriginatingResourceID,
                    ServerID = debugState.ServerID,
                };
        }

        /// <summary>
        /// Creates a key used for worksurfaces unique to a specific server
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="resource">The resource.</param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2/28/2013</date>
        public static WorkSurfaceKey CreateKey(WorkSurfaceContext context, IContextualResourceModel resource)
        {
            return new WorkSurfaceKey
            {
                WorkSurfaceContext = context,
                ResourceID = resource.ID,
                ServerID = resource.ServerID
            };
        }

    }
}
