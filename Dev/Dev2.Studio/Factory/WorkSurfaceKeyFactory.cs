#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;


namespace Dev2.Factory
{
    public static class WorkSurfaceKeyFactory
    {
        public static IWorkSurfaceKey CreateKey(WorkSurfaceContext context) => new WorkSurfaceKey
        {
            WorkSurfaceContext = context,
            ResourceID = Guid.NewGuid(),
            ServerID = Guid.Empty
        };
        
        public static IWorkSurfaceKey CreateEnvKey(WorkSurfaceContext context, Guid environemt) => new WorkSurfaceKey
        {
            WorkSurfaceContext = context,
            ResourceID = environemt,
            ServerID = environemt
        };
        

        public static WorkSurfaceKey CreateKey(WorkSurfaceContext context, Guid resourceID, Guid serverID) => CreateKey(context, resourceID, serverID, null);
        public static WorkSurfaceKey CreateKey(WorkSurfaceContext context, Guid resourceID, Guid serverID, Guid? environmentID) => new WorkSurfaceKey
        {
            WorkSurfaceContext = context,
            ResourceID = resourceID,
            ServerID = serverID,
            EnvironmentID = environmentID
        };
        
        public static IWorkSurfaceKey CreateKey(IContextualResourceModel resourceModel)
        {
            var context = resourceModel.ResourceType.ToWorkSurfaceContext();
            return new WorkSurfaceKey
            {
                WorkSurfaceContext = context,
                ResourceID = resourceModel.ID,
                ServerID = resourceModel.ServerID,
                EnvironmentID = resourceModel.Environment.EnvironmentID
            };
        }

    
        public static WorkSurfaceKey CreateKey(IDebugState debugState)
        {
            var origin = debugState.WorkspaceID;
            if (origin != Guid.Empty)
            {
                var serverRepository = CustomContainer.Get<IServerRepository>();
                var server = serverRepository.FindSingle(model => model.Connection.WorkspaceID == origin);
                var environmentID = server.EnvironmentID;
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
