using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    // PBI 1220 - 2013.05.20 - TWR - Created
    public class WebServices : Services
    {
        static readonly WebExecuteString DefaultWebExecute = WebSources.Execute;
        readonly WebExecuteString _webExecute = DefaultWebExecute;

        #region CTOR

        public WebServices()
        {
        }

        public WebServices(IResourceCatalog resourceCatalog, WebExecuteString webExecute)
            : this(resourceCatalog, webExecute, ServerAuthorizationService.Instance)
        {
        }

        public WebServices(IResourceCatalog resourceCatalog, WebExecuteString webExecute, IAuthorizationService authorizationService)
            : base(resourceCatalog, authorizationService)
        {
            VerifyArgument.IsNotNull("webExecute", webExecute);
            _webExecute = webExecute;
        }

        #endregion

        #region DeserializeService

        protected override Service DeserializeService(string args)
        {
            return JsonConvert.DeserializeObject<WebService>(args);
        }

        protected override Service DeserializeService(XElement xml, ResourceType resourceType)
        {
            return xml == null ? new WebService() : new WebService(xml);
        }

        #endregion

        #region Test

        public WebService Test(string args, Guid workspaceID, Guid dataListID)
        {
            var service = new WebService();
            try
            {
                service = JsonConvert.DeserializeObject<WebService>(args);

                if(string.IsNullOrEmpty(service.RequestResponse))
                {
                    ErrorResultTO errors;
                    ExecuteRequest(service, true, out errors, _webExecute);
                    ((WebSource)service.Source).DisposeClient();
                }

                var preTestRSData = service.Recordsets;
                service.RequestMessage = string.Empty;
                service.JsonPathResult = string.Empty;

                if(service.RequestResponse.IsJSON() && String.IsNullOrEmpty(service.JsonPath))
                {
                    service.ApplyPath();
                    // we need to timeout this request after 10 seconds due to nasty pathing issues ;)
                    Thread jsonMapTaskThread = null;
                    var jsonMapTask = new Task(() =>
                    {
                        jsonMapTaskThread = Thread.CurrentThread;
                        service.Recordsets = FetchRecordset(service, true);
                    });

                    jsonMapTask.Start();
                    jsonMapTask.Wait(10000);

                    if(!jsonMapTask.IsCompleted)
                    {
                        if(jsonMapTaskThread != null)
                        {
                            jsonMapTaskThread.Abort();
                        }

                        service.Recordsets = preTestRSData;
                        service.RequestMessage = GlobalConstants.WebServiceTimeoutMessage;
                    }
                }
                else
                {
                    service.Recordsets = FetchRecordset(service, true);
                }

            }
            catch(Exception ex)
            {
                RaiseError(ex);
                if(service.Recordsets.Count > 0)
                {
                    service.Recordsets[0].HasErrors = true;
                    service.Recordsets[0].ErrorMessage = ex.Message;
                }
                service.RequestResponse = ex.Message;
            }

            return service;
        }

        public WebService ApplyPath(string args, Guid workspaceID, Guid dataListID)
        {
            var service = new WebService();
            try
            {
                service = JsonConvert.DeserializeObject<WebService>(args);
                service.ApplyPath();
                var oldResult = service.RequestResponse;
                service.RequestResponse = service.JsonPathResult;
                service.Recordsets = FetchRecordset(service, true);
                service.RequestResponse = oldResult;
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                if(service.Recordsets.Count > 0)
                {
                    service.Recordsets[0].HasErrors = true;
                    service.Recordsets[0].ErrorMessage = ex.Message;
                }

                service.JsonPathResult = ex.Message;
            }

            return service;
        }

        #endregion
        
        #region ExecuteRequest

        public static void ExecuteRequest(WebService service, bool throwError, out ErrorResultTO errors)
        {
            ExecuteRequest(service, throwError, out errors, DefaultWebExecute);
        }

        public static void ExecuteRequest(WebService service, bool throwError, out ErrorResultTO errors, WebExecuteString webExecute)
        {
            var headers = string.IsNullOrEmpty(service.RequestHeaders)
                              ? new string[0]
                              : service.RequestHeaders.Split(new[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var requestUrl = SetParameters(service.Method.Parameters, service.RequestUrl);
            var requestBody = SetParameters(service.Method.Parameters, service.RequestBody);
            service.RequestResponse = webExecute(service.Source as WebSource, service.RequestMethod, requestUrl, requestBody, throwError, out errors, headers);
            if(!String.IsNullOrEmpty(service.JsonPath))
            {
                service.ApplyPath();
            }
        }

        #endregion

        #region SetParameters

        static string SetParameters(IEnumerable<MethodParameter> parameters, string s)
        {
            return parameters.Aggregate(s ?? "", (current, parameter) => current.Replace("[[" + parameter.Name + "]]", parameter.Value));
        }

        #endregion

    }

    
}