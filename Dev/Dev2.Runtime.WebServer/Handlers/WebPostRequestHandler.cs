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
using System.Threading;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer.TransferObjects;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class WebPostRequestHandler : AbstractWebRequestHandler
    {
        public WebPostRequestHandler(IResourceCatalog catalog, ITestCatalog testCatalog)
            : base(catalog, testCatalog)
        {

        }

        public WebPostRequestHandler()
        {
            
        }
        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var serviceName = ctx.GetServiceName();
            var instanceId = ctx.GetInstanceID();
            var bookmark = ctx.GetBookmark();
            var workspaceID = ctx.GetWorkspaceID();
            var requestTO = new WebRequestTO();
            var xml = SubmittedData.GetPostData(ctx);

            if (!String.IsNullOrEmpty(xml))
            {
                requestTO.RawRequestPayload = xml;
            }

            requestTO.ServiceName = serviceName;
            requestTO.InstanceID = instanceId;
            requestTO.Bookmark = bookmark;
            requestTO.WebServerUrl = ctx.Request.Uri.ToString();
            requestTO.Dev2WebServer = String.Format("{0}://{1}", ctx.Request.Uri.Scheme, ctx.Request.Uri.Authority);

            var variables = ctx.Request.BoundVariables;
            if (variables != null)
            {
                foreach (string key in variables)
                {
                    requestTO.Variables.Add(key, variables[key]);
                }
            }
            Thread.CurrentPrincipal = ctx.Request.User;
            var responseWriter = CreateForm(requestTO, serviceName, workspaceID, ctx.FetchHeaders(), ctx.Request.User);
            ctx.Send(responseWriter);

        }
    }
}
