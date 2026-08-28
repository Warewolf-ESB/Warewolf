#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Warewolf.Resource.Errors;

namespace Dev2.Activities
{
    public class WebRequestInvoker : IWebRequestInvoker
    {
        public string ExecuteRequest(string method, string url, List<Tuple<string, string>> headers) => ExecuteRequest(method, url, null, headers);

        public string ExecuteRequest(string method, string url, List<Tuple<string, string>> headers, int timeoutMilliseconds) => ExecuteRequest(timeoutMilliseconds, method, url, null, headers);
        public string ExecuteRequest(string method, string url, string data) => ExecuteRequest(method, url, data, null, null);

        public string ExecuteRequest(string method, string url, string data, List<Tuple<string, string>> headers) => ExecuteRequest(method, url, data, headers, null);

#pragma warning disable S1541 // Methods and properties should not be too complex
        public string ExecuteRequest(string method, string url, string data, List<Tuple<string, string>> headers, Action<string> asyncCallback)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            using (var webClient = new WebClient())
            {
                webClient.Credentials = CredentialCache.DefaultCredentials;
                webClient.Encoding = Encoding.UTF8;
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        webClient.Headers.Add(header.Item1, header.Item2);
                    }
                }

                var uri = new Uri(url.Contains("http://") || url.Contains("https://") ? url : "http://" + url);

                // The verb is normalised because it arrives from user-authored workflow data
                // (the X6 `webrequest_method` field), not from a constrained UI control.
                var verb = (method ?? string.Empty).Trim().ToUpperInvariant();
                // WebClient.UploadString throws ArgumentNullException on a null body, and the
                // callers that only supply a verb + URL pass data: null. An absent body is a
                // legitimate request, so it is sent as empty rather than crashing the workflow.
                var payload = data ?? string.Empty;

                switch (verb)
                {
                    case "GET":
                        if (asyncCallback == null)
                        {
                            return webClient.DownloadString(uri);
                        }
                        webClient.DownloadStringCompleted += (sender, args) => asyncCallback?.Invoke(args.Result);
                        webClient.DownloadStringAsync(uri, null);
                        break;
                    case "POST":
                    case "PUT":
                    case "DELETE":
                    case "PATCH":
                        if (asyncCallback == null)
                        {
                            return webClient.UploadString(uri, verb, payload);
                        }
                        webClient.UploadStringCompleted += (sender, args) => asyncCallback?.Invoke(args.Result);
                        webClient.UploadStringAsync(uri, verb, payload);
                        break;
                    default:
                        Dev2Logger.Warn("Unsupported Web Request method: " + method, GlobalConstants.WarewolfInfo);
                        throw new ArgumentException("Unsupported Web Request method '" + method + "'. Supported methods are GET, POST, PUT, DELETE and PATCH.");
                }
            }
            return string.Empty;
        }

        public string ExecuteRequest(int timeoutMilliseconds, string method, string url, string data) => ExecuteRequest(timeoutMilliseconds, method, url, data, null, null);

        public string ExecuteRequest(int timeoutMilliseconds, string method, string url, string data, List<Tuple<string, string>> headers) => ExecuteRequest(timeoutMilliseconds, method, url, data, headers, null);

#pragma warning disable S1541 // Methods and properties should not be too complex
        public string ExecuteRequest(int timeoutMilliseconds, string method, string url, string data, List<Tuple<string, string>> headers, Action<string> asyncCallback)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            using (var webClient = new WebClientWithTimeout(timeoutMilliseconds))
            {
                webClient.Credentials = CredentialCache.DefaultCredentials;

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        webClient.Headers.Add(header.Item1, header.Item2);
                    }
                }

                var uri = new Uri(url.Contains("http://") || url.Contains("https://") ? url : "http://" + url);

                // The verb is normalised because it arrives from user-authored workflow data
                // (the X6 `webrequest_method` field), not from a constrained UI control.
                var verb = (method ?? string.Empty).Trim().ToUpperInvariant();
                // WebClient.UploadString throws ArgumentNullException on a null body, and the
                // callers that only supply a verb + URL pass data: null. An absent body is a
                // legitimate request, so it is sent as empty rather than crashing the workflow.
                var payload = data ?? string.Empty;

                switch (verb)
                {
                    case "GET":
                        if (asyncCallback == null)
                        {
                            return webClient.DownloadString(uri);
                        }
                        webClient.DownloadStringCompleted += (sender, args) => asyncCallback?.Invoke(args.Result);
                        webClient.DownloadStringAsync(uri, null);
                        break;
                    case "POST":
                    case "PUT":
                    case "DELETE":
                    case "PATCH":
                        if (asyncCallback == null)
                        {
                            return webClient.UploadString(uri, verb, payload);
                        }
                        webClient.UploadStringCompleted += (sender, args) => asyncCallback?.Invoke(args.Result);
                        webClient.UploadStringAsync(uri, verb, payload);
                        break;
                    default:
                        Dev2Logger.Warn("Unsupported Web Request method: " + method, GlobalConstants.WarewolfInfo);
                        throw new ArgumentException("Unsupported Web Request method '" + method + "'. Supported methods are GET, POST, PUT, DELETE and PATCH.");
                }
            }
            return string.Empty;
        }

        class WebClientWithTimeout : WebClient
        {
            readonly int _timeoutMilliseconds;
            public WebClientWithTimeout(int timeoutMilliseconds)
            {
                _timeoutMilliseconds = timeoutMilliseconds;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var webRequest = base.GetWebRequest(address);
                if (webRequest == null)
                {
                    throw new Exception(ErrorResource.WebRequestError);
                }
                webRequest.Timeout = _timeoutMilliseconds;
                return webRequest;
            }
        }
    }
}
