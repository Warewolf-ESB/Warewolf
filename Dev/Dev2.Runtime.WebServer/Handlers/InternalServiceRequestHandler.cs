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
using Dev2.Runtime.WebServer.TransferObjects;

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
            var formData = new WebRequestTO();

            var xml = GetPostData(ctx, postDataListID);

            if(!String.IsNullOrEmpty(xml))
            {
                formData.RawRequestPayload = xml;
            }

            formData.ServiceName = serviceName;
            formData.InstanceID = instanceId;
            formData.Bookmark = bookmark;
            formData.WebServerUrl = ctx.Request.Uri.ToString();
            formData.Dev2WebServer = String.Format("{0}://{1}", ctx.Request.Uri.Scheme, ctx.Request.Uri.Authority);

            IResponseWriter responseWriter = CreateForm(formData, serviceName, workspaceID, ctx.FetchHeaders(), PublicFormats);
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

            // we need to adjust for the silly xml structure this system was init built on ;(
            if(string.IsNullOrEmpty(xmlData))
            {
                xmlData = "<DataList></DataList>";
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