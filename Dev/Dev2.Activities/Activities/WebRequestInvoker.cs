#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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

                switch (method)
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
                        if (asyncCallback == null)
                        {
                            return webClient.UploadString(uri, data);
                        }
                        webClient.UploadStringCompleted += (sender, args) => asyncCallback?.Invoke(args.Result);
                        webClient.UploadStringAsync(uri, data);
                        break;
                    default:
                        Dev2Logger.Info("No Web method for the Web Request Property Name: " + method, GlobalConstants.WarewolfInfo);
                        break;
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

                switch (method)
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
                        if (asyncCallback == null)
                        {
                            return webClient.UploadString(uri, data);
                        }
                        webClient.UploadStringCompleted += (sender, args) => asyncCallback?.Invoke(args.Result);
                        webClient.UploadStringAsync(uri, data);
                        break;
                    default:
                        return string.Empty;
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
