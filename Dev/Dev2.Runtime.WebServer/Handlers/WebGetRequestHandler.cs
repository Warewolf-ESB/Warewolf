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
using Dev2.Runtime.WebServer.TransferObjects;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class WebGetRequestHandler : AbstractWebRequestHandler
    {
        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var postDataListID = GetDataListID(ctx);
            if(postDataListID != null)
            {
                new WebPostRequestHandler().ProcessRequest(ctx);
                return;
            }

            var serviceName = GetServiceName(ctx);
            var workspaceID = GetWorkspaceID(ctx);

            var requestTO = new WebRequestTO { ServiceName = serviceName, WebServerUrl = ctx.Request.Uri.ToString(), Dev2WebServer = $"{ctx.Request.Uri.Scheme}://{ctx.Request.Uri.Authority}" };
            var data = GetPostData(ctx);

            if(!string.IsNullOrEmpty(data))
            {
                requestTO.RawRequestPayload = data;
            }
            var variables = ctx.Request.BoundVariables;
            if(variables != null)
            {
                foreach(string key in variables)
                {
                    requestTO.Variables.Add(key, variables[key]);
                }
            }
            // Execute in its own thread to give proper context ;)
            Thread.CurrentPrincipal = ctx.Request.User;

            var responseWriter = CreateForm(requestTO, serviceName, workspaceID, ctx.FetchHeaders(),ctx.Request.User);
            ctx.Send(responseWriter);
        }
    }
}
