#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Runtime;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data.Decision;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Web;

namespace Dev2.Runtime.WebServer
{
    class ExecutionDto
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

    static class ExecutionDtoExtentions
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
            var executePayload = "";
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
                    executePayload = GetExecutePayload(dataObject, resource, webRequest, ref formatter);
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
                Dev2Logger.Error(GlobalConstants.ExecutionLoggingResultStartTag + (executePayload ?? "").Replace(Environment.NewLine,string.Empty) + GlobalConstants.ExecutionLoggingResultEndTag, dataObject.ExecutionID.ToString());
            }
            else
            {
                Dev2Logger.Debug(GlobalConstants.ExecutionLoggingResultStartTag + (executePayload ?? "").Replace(Environment.NewLine, string.Empty) + GlobalConstants.ExecutionLoggingResultEndTag, dataObject.ExecutionID.ToString());
            }
            if (!dataObject.Environment.HasErrors() && esbExecuteRequest.WasInternalService)
            {
                TryGetFormatter(executePayload, ref formatter);
            }
            Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
            
            CleanUp(dto);
            return new StringResponseWriter(executePayload, formatter.ContentType);
        }

        static void TryGetFormatter(string executePayload, ref DataListFormat formatter)
        {
            if (executePayload.IsJSON())
            {
                formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
            }
            else
            {
                if (executePayload.IsXml())
                {
                    formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
                }
            }
        }

        static string GetExecutePayload(IDSFDataObject dataObject, IResource resource, WebRequestTO webRequest, ref DataListFormat formatter)
        {
            var notDebug = !dataObject.IsDebug || dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke;
            if (notDebug && resource?.DataList != null)
            {
                switch (dataObject.ReturnType) {
                    case EmitionTypes.XML:
                    {
                        return ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment(dataObject, resource.DataList.ToString(), 0);
                    }
                    case EmitionTypes.SWAGGER:
                    {
                        formatter = DataListFormat.CreateFormat("SWAGGER", EmitionTypes.SWAGGER, "application/json");
                        return ExecutionEnvironmentUtils.GetSwaggerOutputForService(resource, resource.DataList.ToString(), webRequest.WebServerUrl);
                    }
                    default:
                    case EmitionTypes.JSON:
                    {
                        formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                        return ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObject, resource.DataList.ToString(), 0);
                    }
                }
            }
            return string.Empty;
        }

        private static void CleanUp(ExecutionDto dto)
        {
            dto.DataObject = null;
            dto.ErrorResultTO.ClearErrors();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(3,GCCollectionMode.Forced,false);
        }

        static string SetupErrors(IDSFDataObject dataObject, ErrorResultTO allErrors)
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
