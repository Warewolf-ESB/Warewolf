/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Newtonsoft.Json;

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

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        protected virtual Service DeserializeService(string args)
        {
            return JsonConvert.DeserializeObject<WebService>(args);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        protected virtual Service DeserializeService(XElement xml, string resourceType)
        {
            return xml == null ? new WebService() : new WebService(xml);
        }

        #endregion

        #region Test

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public WebService Test(string args, Guid workspaceId, Guid dataListId)
        {
            var service = new WebService();
            try
            {
                service = JsonConvert.DeserializeObject<WebService>(args);

                if (string.IsNullOrEmpty(service.RequestResponse))
                {
                    ErrorResultTO errors;
                    ExecuteRequest(service, true, out errors, _webExecute);
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

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
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
            if (service.Headers !=null)
            {
                headers.AddRange(service.Headers.Select(nameValue => nameValue.Name + ":" + SetParameters(service.Method.Parameters, nameValue.Value)).ToList());
            }
            var requestUrl = SetParameters(service.Method.Parameters, service.RequestUrl);
            var requestBody = SetParameters(service.Method.Parameters, service.RequestBody);
            service.RequestResponse = webExecute(service.Source as WebSource, service.RequestMethod, requestUrl, requestBody, throwError, out errors, headers.ToArray());
            if (!String.IsNullOrEmpty(service.JsonPath))
            {
                service.ApplyPath();
            }
        }

        #endregion

        #region SetParameters

        static string SetParameters(IEnumerable<MethodParameter> parameters, string s)
        {
            return parameters.Aggregate(s ?? "", (current, parameter) => current.Replace(DataListUtil.AddBracketsToValueIfNotExist(parameter.Name), parameter.Value));
        }

        #endregion

        public void TestWebService(WebService service)
        {
            if (string.IsNullOrEmpty(service.RequestResponse))
            {
                ErrorResultTO errors;
                ExecuteRequest(service, true, out errors, _webExecute);
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
