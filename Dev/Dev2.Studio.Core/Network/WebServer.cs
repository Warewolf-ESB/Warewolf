/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Threading;
using Dev2.Controller;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Dev2.Studio.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Studio.Core.Network
{
    public enum UrlType
    {
        Xml,
        Json,
        API,
        Tests
    }

    public static class WebServer
    {
        public static void Send(IContextualResourceModel resourceModel, string payload, IAsyncWorker asyncWorker)
        {
            if (resourceModel?.Environment == null || !resourceModel.Environment.IsConnected)
            {
                return;
            }
            
            asyncWorker.Start(() =>
            {
                var controller = new CommunicationController
                {
                    ServiceName = string.IsNullOrEmpty(resourceModel.Category) ? resourceModel.ResourceName : resourceModel.Category,
                    ServicePayload =
                    {
                        ResourceID = resourceModel.ID
                    },
                };
                controller.AddPayloadArgument("DebugPayload", payload);
                controller.ExecuteCommand<string>(resourceModel.Environment.Connection, resourceModel.Environment.Connection.WorkspaceID);
            }, () => { });
        }
        
        public static void OpenInBrowser(IContextualResourceModel resourceModel, string xmlData)
        {
            var url = GetWorkflowUri(resourceModel, xmlData, UrlType.Xml);
            if (url != null)
            {
                var parameter = "\"" + url + "\"";
                Process.Start(parameter);
            }
        }

        [ExcludeFromCodeCoverage]
        public static void SendErrorOpenInBrowser(IEnumerable<string> exceptionList, string description, string url)
        {
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => true;
            const string PayloadFormat = "\"header\":{0},\"description\":{1},\"type\":3,\"category\":27";
            var headerVal = JsonConvert.SerializeObject(string.Join(Environment.NewLine, exceptionList));
            var serDescription = JsonConvert.SerializeObject(description);
            var postData = "{" + string.Format(PayloadFormat, headerVal, serDescription) + "}";
            //make sure to use TLS 1.2 first before trying other version
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.ServicePoint.ConnectionLimit = 1;
            request.Method = "POST";
            var byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "text/plain";
            request.ContentLength = byteArray.Length;
            request.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            var response = request.GetResponse();
            dataStream = response.GetResponseStream();
            if (dataStream != null)
            {
                var reader = new StreamReader(dataStream);
                var responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                if (JsonConvert.DeserializeObject(responseFromServer) is JObject responseObj)
                {
                    var urlToOpen = ((dynamic)responseObj).data.url;

                    var urlValue = urlToOpen.ToString();
                    if (!string.IsNullOrEmpty(urlValue))
                    {
                        var executor = new ExternalProcessExecutor();
                        executor.OpenInBrowser(new Uri(urlValue));
                    }
                }
            }
        }

        public static Uri GetWorkflowUri(IContextualResourceModel resourceModel, string xmlData, UrlType urlType) => GetWorkflowUri(resourceModel, xmlData, urlType, true);

        public static Uri GetWorkflowUri(IContextualResourceModel resourceModel, string xmlData, UrlType urlType, bool addworkflowId)
        {
            if (resourceModel?.Environment?.Connection == null || !resourceModel.Environment.IsConnected)
            {
                return null;
            }
            var environmentConnection = resourceModel.Environment.Connection;

            var urlExtension = "xml";
            switch (urlType)
            {
                case UrlType.Xml:
                    break;

                case UrlType.Json:
                    urlExtension = "json";
                    break;
    
                    case UrlType.API:
                    urlExtension = "api";
                    break;
                case UrlType.Tests:
                    urlExtension = "tests";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("urlType");
            }

            var category = resourceModel.Category;
            if (string.IsNullOrEmpty(category))
            {
                category = resourceModel.ResourceName;
            }
            var relativeUrl = $"/secure/{category}.{urlExtension}";
            if (urlType != UrlType.API && urlType != UrlType.Tests)
            {
                relativeUrl += "?"+xmlData;
                if (addworkflowId)
                {
                    relativeUrl += "&wid=" + environmentConnection.WorkspaceID;
                }
            }
            Uri.TryCreate(environmentConnection.WebServerUri, relativeUrl, out Uri url);
            return url;
        }

        public static Uri GetInternalServiceUri(string serviceName, IEnvironmentConnection connection)
        {
            if (connection == null || !connection.IsConnected)
            {
                return null;
            }

            var relativeUrl = string.Format("/internal/{0}", serviceName);
            Uri.TryCreate(connection.WebServerUri, relativeUrl, out Uri url);
            return url;
        }
    }
}