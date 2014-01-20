using System;
using Dev2.Runtime.WebServer.Responses;
using Unlimited.Framework;

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

            UnlimitedObject formData = null;

            dynamic d = new UnlimitedObject();

            var xml = GetPostData(ctx, postDataListID);

            if(!String.IsNullOrEmpty(xml))
            {
                formData = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(xml);
            }

            d.Service = serviceName;
            d.InstanceId = instanceId;
            d.Bookmark = bookmark;
            d.WebServerUrl = ctx.Request.Uri.ToString();
            d.Dev2WebServer = String.Format("{0}://{1}", ctx.Request.Uri.Scheme, ctx.Request.Uri.Authority);
            if(formData != null)
            {
                d.AddResponse(formData);
            }

            IResponseWriter responseWriter = CreateForm(d, serviceName, workspaceID, ctx.FetchHeaders(), PublicFormats, ctx.Request.User);
            ctx.Send(responseWriter);
        }

    }
}