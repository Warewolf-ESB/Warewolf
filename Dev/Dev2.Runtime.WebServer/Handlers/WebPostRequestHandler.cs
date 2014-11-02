/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Threading;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class WebPostRequestHandler : AbstractWebRequestHandler
    {
        public override void ProcessRequest(ICommunicationContext ctx)
        {
            string serviceName = GetServiceName(ctx);
            string instanceId = GetInstanceID(ctx);
            string bookmark = GetBookmark(ctx);
            string postDataListID = GetDataListID(ctx);
            string workspaceID = GetWorkspaceID(ctx);

            var requestTO = new WebRequestTO();
            string xml = GetPostData(ctx, postDataListID);

            if (!String.IsNullOrEmpty(xml))
            {
                requestTO.RawRequestPayload = xml;
            }

            requestTO.ServiceName = serviceName;
            requestTO.InstanceID = instanceId;
            requestTO.Bookmark = bookmark;
            requestTO.WebServerUrl = ctx.Request.Uri.ToString();
            requestTO.Dev2WebServer = String.Format("{0}://{1}", ctx.Request.Uri.Scheme, ctx.Request.Uri.Authority);


            // Execute in its own thread to give proper context ;)
            Thread.CurrentPrincipal = ctx.Request.User;

            IResponseWriter responseWriter = CreateForm(requestTO, serviceName, workspaceID, ctx.FetchHeaders(),
                PublicFormats, ctx.Request.User);
            ctx.Send(responseWriter);
        }
    }
}