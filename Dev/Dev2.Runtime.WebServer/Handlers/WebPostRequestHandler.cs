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
using System.Threading;
using Dev2.Runtime.WebServer.TransferObjects;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class WebPostRequestHandler : AbstractWebRequestHandler
    {
        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var serviceName = GetServiceName(ctx);
            var instanceId = GetInstanceID(ctx);
            var bookmark = GetBookmark(ctx);
            var postDataListID = GetDataListID(ctx);
            var workspaceID = GetWorkspaceID(ctx);

            var requestTO = new WebRequestTO();
            var xml = GetPostData(ctx);

            if(!String.IsNullOrEmpty(xml))
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
            // Execute in its own thread to give proper context ;)
            Thread.CurrentPrincipal = ctx.Request.User;
           // requestTO.Variables.Add(ctx.Request.BoundVariables);
            var responseWriter = CreateForm(requestTO, serviceName, workspaceID, ctx.FetchHeaders(), ctx.Request.User);
            ctx.Send(responseWriter);

        }
    }
}
