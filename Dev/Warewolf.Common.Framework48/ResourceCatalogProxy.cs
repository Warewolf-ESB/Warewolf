﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Studio.Interfaces;

namespace Warewolf.Common
{
    public interface IResourceCatalogProxyFactory
    {
        IResourceCatalogProxy New(IEnvironmentConnection environmentConnection);
    }

    public class ResourceCatalogProxyFactory : IResourceCatalogProxyFactory
    {
        public IResourceCatalogProxy New(IEnvironmentConnection environmentConnection)
        {
            return new ResourceCatalogProxy(environmentConnection);
        }
    }

    public interface IResourceCatalogProxy
    {
        T GetResourceById<T>(Guid workspaceId, Guid resourceId) where T : class;
        ExecuteMessage ResumeWorkflowExecution(string resource, string environment, string startActivityId, string versionNumber,string currentUserPrincipal);
    }
    public class ResourceCatalogProxy : IResourceCatalogProxy
    {
        readonly IEnvironmentConnection _environmentConnection;
        public ResourceCatalogProxy(IEnvironmentConnection environmentConnection)
        {
            _environmentConnection = environmentConnection;
        }

        public T GetResourceById<T>(Guid workspaceId, Guid resourceId) where T: class
        {
            var communicationController = new CommunicationController
            {
                ServiceName = nameof(Service.GetResourceById)
            };
            communicationController.AddPayloadArgument(Service.GetResourceById.WorkspaceId, workspaceId.ToString());
            communicationController.AddPayloadArgument(Service.GetResourceById.ResourceId, resourceId.ToString());
            var result = communicationController.ExecuteCommand<T>(_environmentConnection, workspaceId);

            return result;
        }
        public ExecuteMessage ResumeWorkflowExecution(string resourceId, string environment, string startActivityId, string versionNumber,string currentUserPrincipal)
        {
            var comController = new CommunicationController {ServiceName = "WorkflowResume"};
            comController.AddPayloadArgument("resourceID", resourceId);
            comController.AddPayloadArgument("environment", environment);
            comController.AddPayloadArgument("startActivityId", startActivityId);
            comController.AddPayloadArgument("versionNumber", versionNumber);
            comController.AddPayloadArgument("currentuserprincipal", currentUserPrincipal);

            var result =  comController.ExecuteCommand<ExecuteMessage>(_environmentConnection, GlobalConstants.ServerWorkspaceID);
            return result;
        }
    }
}
