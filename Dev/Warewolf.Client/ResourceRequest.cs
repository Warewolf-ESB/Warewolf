/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using System.Threading.Tasks;
using Dev2.Communication;
using Dev2.SignalR.Wrappers;
using Warewolf.Esb;

namespace Warewolf.Client
{
    public class ResourceRequest<T> : IExecutableRequest<T>, ICatalogRequest where T : class, new()
    {
        private readonly Guid _workspaceId;
        private readonly Guid _resourceId;

        public ResourceRequest(Guid workspaceId, Guid resourceId)
        {
            _workspaceId = workspaceId;
            _resourceId = resourceId;
        }

        public IEsbRequest Build()
        {
            var servicePayload = new EsbExecuteRequest
            {
                ServiceName = nameof(Service.GetResourceById)
            };
            servicePayload.AddArgument(Service.GetResourceById.WorkspaceId, new StringBuilder(_workspaceId.ToString()));
            servicePayload.AddArgument(Service.GetResourceById.ResourceId, new StringBuilder(_resourceId.ToString()));
            return servicePayload;
        }

        public Task<T> Execute(IHubProxyWrapper hubProxy)
        {
            return hubProxy.ExecReq2<T>(this);
        }

        public Task<T> Execute(IConnectedHubProxyWrapper hubProxy, int maxRetries)
        {
            return hubProxy.ExecReq3<T>(this, maxRetries);
        }
    }
}
