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
            var xml = GetPostData(ctx, postDataListID);

            if(!String.IsNullOrEmpty(xml))
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

            var responseWriter = CreateForm(requestTO, serviceName, workspaceID, ctx.FetchHeaders(), PublicFormats, ctx.Request.User);
            ctx.Send(responseWriter);

        }
    }
}