using System;
using Dev2.Runtime.WebServer.Responses;
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

            var requestTO = new WebRequestTO { ServiceName = serviceName, WebServerUrl = ctx.Request.Uri.ToString(), Dev2WebServer = String.Format("{0}://{1}", ctx.Request.Uri.Scheme, ctx.Request.Uri.Authority) };
            var data = GetPostData(ctx, Guid.Empty.ToString());

            if(!String.IsNullOrEmpty(data))
            {
                requestTO.RawRequestPayload = data;
            }

            IResponseWriter responseWriter = CreateForm(requestTO, serviceName, workspaceID, ctx.FetchHeaders(), PublicFormats);
            ctx.Send(responseWriter);
        }

    }
}