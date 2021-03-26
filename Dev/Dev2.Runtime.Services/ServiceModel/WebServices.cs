#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Newtonsoft.Json;
using Warewolf.Data.Options;

namespace Dev2.Runtime.ServiceModel
{
    // PBI 1220 - 2013.05.20 - TWR - Created
    public interface IWebServices
    {
        void TestWebService(WebService service);
    }

    public class WebServices : Services, IWebServices
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


        protected virtual Service DeserializeService(string args) => JsonConvert.DeserializeObject<WebService>(args);

        protected virtual Service DeserializeService(XElement xml, string resourceType)
        {
            return xml == null ? new WebService() : new WebService(xml);
        }

        #endregion

        #region Test

    
        public WebService Test(string args, Guid workspaceId, Guid dataListId)
        {
            var service = new WebService();
            try
            {
                service = JsonConvert.DeserializeObject<WebService>(args);

                if (string.IsNullOrEmpty(service.RequestResponse))
                {
                    ExecuteRequest(service, true, out ErrorResultTO errors, _webExecute);
                    ((WebSource)service.Source).DisposeClient();
                }

                var preTestRsData = service.Recordsets;
                service.RequestMessage = string.Empty;
                service.JsonPathResult = string.Empty;

                if (service.RequestResponse.IsJSON() && String.IsNullOrEmpty(service.JsonPath))
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

                    if (!jsonMapTask.IsCompleted)
                    {
                        jsonMapTaskThread?.Abort();

                        service.Recordsets = preTestRsData;
                        service.RequestMessage = GlobalConstants.WebServiceTimeoutMessage;
                    }

                    jsonMapTask.Dispose();
                }
                else
                {
                    service.Recordsets = FetchRecordset(service, true);
                }

            }
            catch (Exception ex)
            {
                RaiseError(ex);
                if (service.Recordsets.Count > 0)
                {
                    service.Recordsets[0].HasErrors = true;
                    service.Recordsets[0].ErrorMessage = ex.Message;
                }
                service.RequestResponse = ex.Message;
            }

            return service;
        }

    
        public WebService ApplyPath(string args, Guid workspaceId, Guid dataListId)
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
            catch (Exception ex)
            {
                RaiseError(ex);
                if (service.Recordsets.Count > 0)
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
            var headers = new List<string>();
            var evaluatedHeaders = new List<INameValue>();
            if (service.Headers !=null)
            {
                evaluatedHeaders = service.Headers.Select(o => new NameValue(SetParameters(service.Method.Parameters, o.Name), SetParameters(service.Method.Parameters, o.Value)) as INameValue).ToList();
                headers.AddRange(ToHeaderStringList(evaluatedHeaders));
            }

            var requestUrl = SetParameters(service.Method.Parameters, service.RequestUrl);
            
            var requestBody = string.Empty;
            var IsClassicRequestBody = !service.IsManualChecked && !service.IsFormDataChecked;
            if (service.IsManualChecked || IsClassicRequestBody)
            {
                requestBody = SetParameters(service.Method.Parameters, service.RequestBody);
            }

            var formDataParameters = new List<IFormDataParameters>();
            if (service.IsFormDataChecked && service.FormDataParameters != null)
            {

                var headersHelper = new WebRequestHeadersHelper(service.Headers, evaluatedHeaders);
                var evaluated = headersHelper.CalculateFormDataContentType();
                headers = ToHeaderStringList(evaluated.ToList());

                formDataParameters.AddRange(service.FormDataParameters.Select(o =>
                {
                    if (o is TextParameter textParam)
                    {
                        textParam.Key = SetParameters(service.Method.Parameters, textParam.Key);
                        textParam.Value = SetParameters(service.Method.Parameters, textParam.Value);
                        return textParam;
                    }
                    else if (o is FileParameter fileParam)
                    {
                        fileParam.Key = SetParameters(service.Method.Parameters, fileParam.Key);
                        fileParam.FileName = SetParameters(service.Method.Parameters, fileParam.FileName);
                        fileParam.FileBase64 = SetParameters(service.Method.Parameters, fileParam.FileBase64);
                        return fileParam;
                    }
                    return o;
                }).ToList());

            }
            var webExecuteStringArgs = new WebExecuteStringArgs
            {
                IsManualChecked = service.IsManualChecked,
                IsFormDataChecked = service.IsFormDataChecked,
                FormDataParameters = service.FormDataParameters,
                WebRequestFactory = null
            };
            var webResponse = webExecute?.Invoke(service.Source as WebSource, service.RequestMethod, requestUrl, requestBody, throwError, out errors, headers.ToArray(), webExecuteStringArgs);

            service.RequestResponse = Scrubber.Scrub(webResponse);

            if (!String.IsNullOrEmpty(service.JsonPath))
            {
                service.ApplyPath();
            }
            errors = new ErrorResultTO();
        }

        private static List<string> ToHeaderStringList(List<INameValue> headers)
        {
            return headers.Select(o => o.Name + ":" + o.Value).ToList();
        }

        #endregion

        #region SetParameters

        static string SetParameters(IEnumerable<MethodParameter> parameters, string s) => parameters.Aggregate(s ?? "", (current, parameter) => current.Replace(DataListUtil.AddBracketsToValueIfNotExist(parameter.Name), parameter.Value));

        #endregion

        public void TestWebService(WebService service)
        {
            if (string.IsNullOrEmpty(service.RequestResponse))
            {
                ExecuteRequest(service, true, out ErrorResultTO errors, _webExecute);
                ((WebSource)service.Source).DisposeClient();
            }

            var preTestRsData = service.Recordsets;
            service.RequestMessage = string.Empty;
            service.JsonPathResult = string.Empty;

            if (service.RequestResponse.IsJSON() && String.IsNullOrEmpty(service.JsonPath))
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

                if (!jsonMapTask.IsCompleted)
                {
                    jsonMapTaskThread?.Abort();

                    service.Recordsets = preTestRsData;
                    service.RequestMessage = GlobalConstants.WebServiceTimeoutMessage;
                }

                jsonMapTask.Dispose();
            }
            else
            {
                service.Recordsets = FetchRecordset(service, true);
            }
        }

    }


}
