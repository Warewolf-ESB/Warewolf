using System;
using System.Threading;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime.WebServer.Responses;
using Unlimited.Framework;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class InternalServiceRequestHandler : AbstractWebRequestHandler
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

            StringResponseWriter responseWriter = CreateForm(d, serviceName, workspaceID, ctx.FetchHeaders(), PublicFormats);
            ctx.Send(responseWriter);
        }

        public string ProcessRequest(string payload, Guid workspaceID, Guid dataListID, string connectionId)
        {
            var channel = new EsbServicesEndpoint();
            IDSFDataObject dataObject = new DsfDataObject(payload, dataListID);
            dataObject.ClientID = Guid.Parse(connectionId);
            // we need to assign new ThreadID to request coming from here, becasue it is a fixed connection and will not change ID on its own ;)
            if(!dataObject.Errors.HasErrors())
            {

                Guid dlID = Guid.Empty;
                ErrorResultTO errors;
                var t = new Thread(() =>
                {
                    dlID = channel.ExecuteRequest(dataObject, workspaceID, out errors);
                });

                t.Start();

                t.Join();

                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                string result = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML),
                                                     enTranslationDepth.Data, out errors);

                // we really need to clean up chaps ;)
                compiler.ForceDeleteDataListByID(dlID);

                return result;
            }
            return dataObject.Errors.MakeUserReady();
        }
    }
}