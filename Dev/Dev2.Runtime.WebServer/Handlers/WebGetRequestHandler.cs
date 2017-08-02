/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Runtime.WebServer.Handlers
{
    public class WebGetRequestHandler : AbstractWebRequestHandler
    {
        private IResourceCatalog _catalog;
        private ITestCatalog _testCatalog;

        public WebGetRequestHandler(IResourceCatalog catalog, ITestCatalog testCatalog)
        {
            _catalog = catalog;
            _testCatalog = testCatalog;
        }

        public WebGetRequestHandler()
        {

        }

        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var postDataListID = GetDataListID(ctx);
            if (postDataListID != null)
            {
                _catalog = _catalog ?? ResourceCatalog.Instance;
                _testCatalog = _testCatalog ?? TestCatalog.Instance;
                new WebPostRequestHandler(_catalog, _testCatalog).ProcessRequest(ctx);
                return;
            }

            var serviceName = GetServiceName(ctx);
            var workspaceID = GetWorkspaceID(ctx);

            var requestTO = new WebRequestTO { ServiceName = serviceName, WebServerUrl = ctx.Request.Uri.ToString(), Dev2WebServer = $"{ctx.Request.Uri.Scheme}://{ctx.Request.Uri.Authority}" };
            var data = GetPostData(ctx);

            if (!string.IsNullOrEmpty(data))
            {
                requestTO.RawRequestPayload = data;
            }
            var variables = ctx.Request.BoundVariables;
            if (variables != null)
            {
                foreach (string key in variables)
                {
                    requestTO.Variables.Add(key, variables[key]);
                }
            }
            // Execute in its own thread to give proper context ;)
            Thread.CurrentPrincipal = ctx.Request.User;

            var responseWriter = CreateForm(requestTO, serviceName, workspaceID, ctx.FetchHeaders(), ctx.Request.User);
            ctx.Send(responseWriter);
        }
    }
}
