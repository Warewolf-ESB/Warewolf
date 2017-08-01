using System;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data.Decision;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Web;
using FluentAssertions.Common;

namespace Dev2.Runtime.WebServer
{
    internal class ExecutionDto
    {
        public WebRequestTO WebRequestTO { get; set; }
        public string ServiceName { get; set; }
        public string PayLoad { get; set; }
        public IDSFDataObject DataObject { get; set; }
        public EsbExecuteRequest Request { get; set; }
        public Guid DataListIdGuid { get; set; }
        public Guid WorkspaceID { get; set; }
        public IResource Resource { get; set; }
        public DataListFormat DataListFormat { get; set; }
        public Dev2JsonSerializer Serializer { get; set; }
        public ErrorResultTO ErrorResultTO { get; set; }
    }

    internal static class ExecutionDtoExtentions
    {
        public static IResponseWriter CreateResponseWriter(this ExecutionDto dto)
        {
            var dataObject = dto.DataObject;
            var esbExecuteRequest = dto.Request;
            var executionDlid = dto.DataListIdGuid;
            var workspaceGuid = dto.WorkspaceID;
            var serviceName = dto.ServiceName;
            var resource = dto.Resource;
            var formatter = dto.DataListFormat;
            var executePayload = dto.PayLoad;
            var webRequest = dto.WebRequestTO;
            var serializer = dto.Serializer;
            var allErrors = dto.ErrorResultTO;
            if (!dataObject.Environment.HasErrors())
            {
                if (!esbExecuteRequest.WasInternalService)
                {
                    dataObject.DataListID = executionDlid;
                    dataObject.WorkspaceID = workspaceGuid;
                    dataObject.ServiceName = serviceName;

                    if (!dataObject.IsDebug || dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke)
                    {
                        if (resource?.DataList != null)
                        {
                            if (dataObject.ReturnType == EmitionTypes.JSON)
                            {
                                formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                                executePayload = ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObject,
                                    resource.DataList.ToString(), 0);
                            }
                            else if (dataObject.ReturnType == EmitionTypes.XML)
                            {
                                executePayload = ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment(dataObject,
                                    resource.DataList.ToString(), 0);
                            }
                            else if (dataObject.ReturnType == EmitionTypes.SWAGGER)
                            {
                                formatter = DataListFormat.CreateFormat("SWAGGER", EmitionTypes.SWAGGER, "application/json");
                                executePayload = ExecutionEnvironmentUtils.GetSwaggerOutputForService(resource,
                                    resource.DataList.ToString(), webRequest.WebServerUrl);
                            }
                        }
                    }
                    else
                    {
                        executePayload = string.Empty;
                    }
                }
                else
                {
                    // internal service request we need to return data for it from the request object ;)

                    executePayload = string.Empty;
                    var msg = serializer.Deserialize<ExecuteMessage>(esbExecuteRequest.ExecuteResult);

                    if (msg != null)
                    {
                        executePayload = msg.Message.ToString();
                    }

                    // out fail safe to return different types of data from services ;)
                    if (string.IsNullOrEmpty(executePayload))
                    {
                        executePayload = esbExecuteRequest.ExecuteResult.ToString();
                    }
                }
            }
            else
            {
                executePayload = SetupErrors(dataObject, allErrors);
            }

            if (dataObject.Environment.HasErrors())
            {
                Dev2Logger.Error(GlobalConstants.ExecutionLoggingResultStartTag + (executePayload ?? "").RemoveNewLines() + GlobalConstants.ExecutionLoggingResultEndTag, dataObject.ExecutionID.ToString());
            }
            else
            {
                Dev2Logger.Debug(GlobalConstants.ExecutionLoggingResultStartTag + (executePayload ?? "").RemoveNewLines() + GlobalConstants.ExecutionLoggingResultEndTag, dataObject.ExecutionID.ToString());
            }
            if (!dataObject.Environment.HasErrors() && esbExecuteRequest.WasInternalService)
            {
                if (executePayload.IsJSON())
                {
                    formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                }
                else if (executePayload.IsXml())
                {
                    formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
                }
            }
            Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
            dataObject.Environment = null;
            return new StringResponseWriter(executePayload, formatter.ContentType);
        }

        private static string SetupErrors(IDSFDataObject dataObject, ErrorResultTO allErrors)
        {
            string executePayload;
            if (dataObject.ReturnType == EmitionTypes.XML)
            {
                executePayload =
                    "<FatalError> <Message> An internal error occurred while executing the service request </Message>";
                executePayload += allErrors.MakeDataListReady();
                executePayload += "</FatalError>";
            }
            else
            {
                executePayload =
                    "{ \"FatalError\": \"An internal error occurred while executing the service request\",";
                executePayload += allErrors.MakeDataListReady(false);
                executePayload += "}";
            }
            return executePayload;
        }
    }
}
