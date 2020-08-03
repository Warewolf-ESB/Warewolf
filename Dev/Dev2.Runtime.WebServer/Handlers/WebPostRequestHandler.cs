#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class WebPostRequestHandler : AbstractWebRequestHandler
    {
        public WebPostRequestHandler(IResourceCatalog catalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog, IEsbChannelFactory esbChannelFactory)
            : base(catalog, testCatalog, testCoverageCatalog, esbChannelFactory, new SecuritySettings())
        {
        }

        public WebPostRequestHandler()
            : this(ResourceCatalog.Instance, TestCatalog.Instance, TestCoverageCatalog.Instance, new DefaultEsbChannelFactory())
        {
        }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var serviceName = ctx.GetServiceName();
            var instanceId = ctx.GetInstanceID();
            var bookmark = ctx.GetBookmark();
            var workspaceId = ctx.GetWorkspaceID();
            var requestTo = new WebRequestTO();
            var xml = SubmittedData.GetPostData(ctx);

            if (!string.IsNullOrEmpty(xml))
            {
                requestTo.RawRequestPayload = xml;
            }

            requestTo.ServiceName = serviceName;
            requestTo.InstanceID = instanceId;
            requestTo.Bookmark = bookmark;
            requestTo.WebServerUrl = ctx.Request.Uri.ToString();
            requestTo.Dev2WebServer = $"{ctx.Request.Uri.Scheme}://{ctx.Request.Uri.Authority}";

            var variables = ctx.Request.BoundVariables;
            if (variables != null)
            {
                foreach (string key in variables)
                {
                    requestTo.Variables.Add(key, variables[key]);
                }
            }
            Thread.CurrentPrincipal = ctx.Request.User;
            var responseWriter = CreateForm(requestTo, serviceName, workspaceId, ctx.FetchHeaders(), ctx.Request.User);
            ctx.Send(responseWriter);
        }
    }
}
