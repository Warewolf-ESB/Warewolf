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
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer.TransferObjects;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class WebGetRequestHandler : AbstractWebRequestHandler
    {
        private IResourceCatalog _catalog;
        private ITestCatalog _testCatalog;
        private ITestCoverageCatalog _testCoverageCatalog;

        public WebGetRequestHandler(IResourceCatalog catalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog)
        {
            _catalog = catalog;
            _testCatalog = testCatalog;
            _testCoverageCatalog = testCoverageCatalog;
        }

        public WebGetRequestHandler()
        {

        }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var postDataListId = ctx.GetDataListID();
            if (postDataListId != null)
            {
                _catalog = _catalog ?? ResourceCatalog.Instance;
                _testCatalog = _testCatalog ?? TestCatalog.Instance;
                _testCoverageCatalog = _testCoverageCatalog ?? TestCoverageCatalog.Instance;
                new WebPostRequestHandler(_catalog, _testCatalog, _testCoverageCatalog).ProcessRequest(ctx);
                return;
            }

            var serviceName = ctx.GetServiceName();
            var workspaceId = ctx.GetWorkspaceID();

            var requestTo = new WebRequestTO { ServiceName = serviceName, WebServerUrl = ctx.Request.Uri.ToString(), Dev2WebServer = $"{ctx.Request.Uri.Scheme}://{ctx.Request.Uri.Authority}" };
            var data = SubmittedData.GetPostData(ctx);

            if (!string.IsNullOrEmpty(data))
            {
                requestTo.RawRequestPayload = data;
            }
            var variables = ctx.Request.BoundVariables;
            if (variables != null)
            {
                foreach (string key in variables)
                {
                    requestTo.Variables.Add(key, variables[key]);
                }
            }
            // Execute in its own thread to give proper context ;)
            Thread.CurrentPrincipal = ctx.Request.User;

            var responseWriter = CreateForm(requestTo, serviceName, workspaceId, ctx.FetchHeaders(), ctx.Request.User);
            ctx.Send(responseWriter);
        }
    }
}
