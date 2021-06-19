#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Executor;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Services.Security;
using Warewolf.Data;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class GetOpenAPIServiceHandler : AbstractWebRequestHandler
    {
        static IAuthorizationService _authorizationService;
        static IResourceCatalog _resourceCatalog;

        public GetOpenAPIServiceHandler()
            : this(ResourceCatalog.Instance, ServerAuthorizationService.Instance)
        {
        }

#pragma warning disable S3010 // Used by tests for constructor injection
        public GetOpenAPIServiceHandler(IResourceCatalog catalog, IAuthorizationService auth)
            : base(ResourceCatalog.Instance, TestCatalog.Instance, TestCoverageCatalog.Instance,
                new DefaultEsbChannelFactory(), new SecuritySettings())
        {
            _resourceCatalog = catalog;
            _authorizationService = auth;
        }
#pragma warning restore S3010

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }

            var basePath = ctx.Request.BoundVariables["path"];
            if (!bool.TryParse(ctx.Request.BoundVariables["isPublic"], out bool isPublic))
            {
                isPublic = false;
            }

            WriteReponses(basePath, isPublic, ctx);
        }

        void WriteReponses(string basePath, bool isPublic, ICommunicationContext ctx)
        {
            var webPath = basePath.Replace("\\", "/");
            var searchPath = basePath.Replace("/", "\\").Replace(".api", "");
            var resourceList = ResourceCatalog.Instance.GetResourceList(GlobalConstants.ServerWorkspaceID).Where(
                resource =>
                    resource.GetResourcePath(GlobalConstants.ServerWorkspaceID).Contains(searchPath) &&
                    resource.ResourceType == "WorkflowService").ToList();

            var builder = new StringBuilder();
            var val = ExecutionEnvironmentUtils.GetOpenAPIOutputForServiceList(new List<IWarewolfResource>(resourceList), ctx.Request.Uri.ToString());
            builder.AppendLine(val);
            
            ctx.Send(new StringResponseWriter(builder.ToString(), "application/json"));
        }
    }
}