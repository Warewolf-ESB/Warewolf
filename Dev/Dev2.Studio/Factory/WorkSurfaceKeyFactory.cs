/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces;
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
        public static IWorkSurfaceKey CreateKey(WorkSurfaceContext context)
        {
            return new WorkSurfaceKey
            {
                WorkSurfaceContext = context,
                ResourceID = Guid.NewGuid(),
                ServerID = Guid.Empty
            };
        }

        /// <summary>
        /// Create a key which are unique to the entire studio
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="environemt"></param>
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2/28/2013</date>
        public static IWorkSurfaceKey CreateEnvKey(WorkSurfaceContext context, Guid environemt)
        {
            return new WorkSurfaceKey
            {
                WorkSurfaceContext = context,
                ResourceID = environemt,
                ServerID = environemt
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
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static WorkSurfaceKey CreateKey(IDebugState debugState)
        {
            var origin = debugState.WorkspaceID;
            if (origin != Guid.Empty)
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
    }
}
