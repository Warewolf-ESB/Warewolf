#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Text;
using System.Web;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Decision;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Web;
using Newtonsoft.Json;
using Warewolf.Data;

namespace Dev2.Runtime.WebServer
{
    public interface IExecutionDto
    {
        DataListFormat DataListFormat { get; set; }
        Guid DataListIdGuid { get; set; }
        IDSFDataObject DataObject { get; set; }
        ErrorResultTO ErrorResultTO { get; set; }
        string PayLoad { get; set; }
        EsbExecuteRequest Request { get; set; }
        IWarewolfResource Resource { get; set; }
        Dev2JsonSerializer Serializer { get; set; }
        string ServiceName { get; set; }
        WebRequestTO WebRequestTO { get; set; }
        Guid WorkspaceID { get; set; }
    }

    public class ExecutionDto : IExecutionDto
    {
        public WebRequestTO WebRequestTO { get; set; }
        public string ServiceName { get; set; }
        public string PayLoad { get; set; }
        public IDSFDataObject DataObject { get; set; }
        public EsbExecuteRequest Request { get; set; }
        public Guid DataListIdGuid { get; set; }
        public Guid WorkspaceID { get; set; }
        public IWarewolfResource Resource { get; set; }
        public DataListFormat DataListFormat { get; set; }
        public Dev2JsonSerializer Serializer { get; set; }
        public ErrorResultTO ErrorResultTO { get; set; }
    }

    public class ExecutionDtoExtensions
    {
        readonly IExecutionDto _executionDto;

        public ExecutionDtoExtensions(IExecutionDto executionDto)
        {
            _executionDto = executionDto;
        }

        public class ResponseData
        {
            private ResponseData(string content)
            {
                Content = content;
            }

            private ResponseData(string content, string contentType)
                : this(content)
            {
                ContentType = contentType;
            }

            private ResponseData(HttpException exception, string content)
                : this(content)
            {
                Exception = exception;
            }
            public string Content { get; }
            public string ContentType { get; }
            public HttpException Exception { get; }

            public static ResponseData FromExecutionDto(IExecutionDto executionDto, string contentType)
            {
                return new ResponseData(executionDto.PayLoad, contentType);
            }

            public static ResponseData FromException(HttpException exception, string content)
            {
                return new ResponseData(exception, content);
            }

            public IResponseWriter ToResponseWriter(IStringResponseWriterFactory stringResponseWriterFactory)
            {
                if (Exception != null)
                {
                    return new ExceptionResponseWriter(HttpStatusCode.InternalServerError, Content);
                }

                return stringResponseWriterFactory.New(Content, ContentType);
            }
        }

        public IResponseWriter CreateResponseWriter(IStringResponseWriterFactory stringResponseWriterFactory)
        {
            var responseData = CreateResponse();
            return responseData.ToResponseWriter(stringResponseWriterFactory);
        }
        public ResponseData CreateResponse()
        {
            var dataObject = _executionDto.DataObject;
            var esbExecuteRequest = _executionDto.Request;
            var executionDlid = _executionDto.DataListIdGuid;
            var workspaceGuid = _executionDto.WorkspaceID;
            var serviceName = _executionDto.ServiceName;
            var resource = _executionDto.Resource;
            var formatter = _executionDto.DataListFormat;
            var webRequest = _executionDto.WebRequestTO;
            var serializer = _executionDto.Serializer;
            var allErrors = _executionDto.ErrorResultTO;
            bool wasInternalService = esbExecuteRequest?.WasInternalService ?? false;

            if (!wasInternalService)
            {
                dataObject.DataListID = executionDlid;
                dataObject.WorkspaceID = workspaceGuid;
                dataObject.ServiceName = serviceName;
                if (dataObject.ExecutionException is null)
                {
                    _executionDto.PayLoad = GetExecutePayload(dataObject, resource, webRequest, ref formatter);
                }
                else
                {
                    var content = GetExecuteExceptionPayload(dataObject);
                    return ResponseData.FromException(new HttpException((int)HttpStatusCode.InternalServerError, "internal server error"), content);
                }
            }
            else
            {
                // internal service request we need to return data for it from the request object
                _executionDto.PayLoad = string.Empty;
                var msg = serializer.Deserialize<ExecuteMessage>(esbExecuteRequest.ExecuteResult);

                if (msg != null)
                {
                    _executionDto.PayLoad = msg.Message.ToString();
                }

                // out fail safe to return different types of data from services
                if (string.IsNullOrEmpty(_executionDto.PayLoad))
                {
                    _executionDto.PayLoad = esbExecuteRequest.ExecuteResult.ToString();
                }
            }

            if (dataObject.Environment.HasErrors())
            {
                Dev2Logger.Error(GlobalConstants.ExecutionLoggingResultStartTag + (_executionDto.PayLoad ?? "").Replace(Environment.NewLine, string.Empty) + GlobalConstants.ExecutionLoggingResultEndTag, dataObject.ExecutionID.ToString());
            }
            else
            {
                Dev2Logger.Debug(GlobalConstants.ExecutionLoggingResultStartTag + (_executionDto.PayLoad ?? "").Replace(Environment.NewLine, string.Empty) + GlobalConstants.ExecutionLoggingResultEndTag, dataObject.ExecutionID.ToString());
            }

            if (!dataObject.Environment.HasErrors() && wasInternalService)
            {
                TryGetFormatter(ref formatter);
            }

            Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);

            CleanUp(_executionDto);
            return ResponseData.FromExecutionDto(_executionDto, formatter.ContentType);
        }

        void TryGetFormatter(ref DataListFormat formatter)
        {
            if (_executionDto.PayLoad.IsJSON())
            {
                formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
            }
            else
            {
                if (_executionDto.PayLoad.IsXml())
                {
                    formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
                }
            }
        }


        string GetExecutePayload(IDSFDataObject dataObject, IWarewolfResource resource, WebRequestTO webRequest, ref DataListFormat formatter)
        {
            var notDebug = !dataObject.IsDebug || dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke;
            if (notDebug && resource?.DataList != null)
            {
                switch (dataObject.ReturnType)
                {
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



        string GetExecuteExceptionPayload(IDSFDataObject dataObject)
        {
            var notDebug = !dataObject.IsDebug || dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke;
            if (notDebug)
            {
                switch (dataObject.ReturnType)
                {
                    case EmitionTypes.XML:
                    {
                        return $"<Error>{dataObject.ExecutionException.Message}</Error>";
                    }
                    default:
                    case EmitionTypes.SWAGGER:
                    case EmitionTypes.JSON:
                    {
                        return JsonConvert.SerializeObject(new {Message = dataObject.ExecutionException.Message});
                    }
                }
            }

            return string.Empty;
        }

        private void CleanUp(IExecutionDto dto)
        {
            dto.DataObject = null;
            dto.ErrorResultTO.ClearErrors();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(3, GCCollectionMode.Forced, false);
        }
    }
}