using System;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Control;
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

        public StringBuilder ProcessRequest(EsbExecuteRequest request, Guid workspaceID, Guid dataListID, string connectionId)
        {
            var channel = new EsbServicesEndpoint();
            var xmlData = string.Empty;
            if(request.Args != null && request.Args.ContainsKey("DebugPayload"))
            {
                xmlData = request.Args["DebugPayload"].ToString();
                xmlData = xmlData.Replace("<DataList>", "<XmlData>").Replace("</DataList>", "</XmlData>");
            }
            IDSFDataObject dataObject = new DsfDataObject(xmlData, dataListID);
            dataObject.ServiceName = request.ServiceName;
            dataObject.ClientID = Guid.Parse(connectionId);
            dataObject.ExecutingUser = ExecutingUser;
            // we need to assign new ThreadID to request coming from here, because it is a fixed connection and will not change ID on its own ;)
            if(!dataObject.Errors.HasErrors())
            {
                Guid dlID = Guid.Empty;
                ErrorResultTO errors;
                var princple = Thread.CurrentPrincipal;

                var t = new Thread(() =>
                {
                    Thread.CurrentPrincipal = princple;
                    dlID = channel.ExecuteRequest(dataObject, request, workspaceID, out errors);
                });

                t.Start();

                t.Join();

                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

                if(request.ExecuteResult.Length > 0)
                {
                    return request.ExecuteResult;
                }

                // return the datalist ;)
                if(dataObject.IsDebugMode())
                {
                    compiler.ForceDeleteDataListByID(dlID);
                    return new StringBuilder("Completed Debug");
                }
                var result = new StringBuilder(compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out errors));
                compiler.ForceDeleteDataListByID(dlID);
                return result;
            }

            ExecuteMessage msg = new ExecuteMessage { HasError = true };
            msg.SetMessage(dataObject.Errors.MakeDisplayReady());

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(msg);
        }

        public IPrincipal ExecutingUser { get; set; }
    }
}