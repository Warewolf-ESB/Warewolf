using System;
using Dev2.Runtime.WebServer.Responses;
using Unlimited.Framework;

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

            dynamic d = new UnlimitedObject();
            d.Service = serviceName;

            d.WebServerUrl = ctx.Request.Uri.ToString();
            d.Dev2WebServer = String.Format("{0}://{1}", ctx.Request.Uri.Scheme, ctx.Request.Uri.Authority);

            var data = GetPostData(ctx, Guid.Empty.ToString());

            if(!String.IsNullOrEmpty(data))
            {
                d.PostData = data;
                d.Add(new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(data));
            }

            ResponseWriter responseWriter = CreateForm(d, serviceName, workspaceID, ctx.FetchHeaders(), PublicFormats);
            ctx.Send(responseWriter);
        }

    }
}