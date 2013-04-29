using System;
using Dev2.Diagnostics;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Factory
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
        /// <returns></returns>
        /// <author>Jurie.smit</author>
        /// <date>2/28/2013</date>
        public static WorkSurfaceKey CreateKey(WorkSurfaceContext context, Guid resourceID, Guid serverID)
        {
            return new WorkSurfaceKey
            {
                WorkSurfaceContext = context,
                ResourceID = resourceID,
                ServerID = serverID
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
                    ServerID = resourceModel.ServerID
                };
        }

        public static WorkSurfaceKey CreateKey(IDebugState debugState)
        {
            return new WorkSurfaceKey
                {
                    WorkSurfaceContext = WorkSurfaceContext.Workflow,
                    ResourceID = debugState.ResourceID,
                    ServerID = debugState.ServerID
                };
        }
    }
}
