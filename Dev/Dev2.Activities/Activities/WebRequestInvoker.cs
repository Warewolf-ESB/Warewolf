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
using System.Net;
using System.Text;
using Warewolf.Resource.Errors;

namespace Dev2.Activities
{
    public class WebRequestInvoker : IWebRequestInvoker
    {
        public string ExecuteRequest(string method, string url, List<Tuple<string, string>> headers)
        {
            return ExecuteRequest(method, url, null, headers);
        }

        public string ExecuteRequest(string method, string url, List<Tuple<string, string>> headers, int timeoutMilliseconds)
        {
            return ExecuteRequest(timeoutMilliseconds, method, url, null, headers);
        }

        public string ExecuteRequest(string method, string url, string data, List<Tuple<string, string>> headers = null, Action<string> asyncCallback = null)
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
                        webClient.DownloadStringCompleted += (sender, args) => asyncCallback(args.Result);
                        webClient.DownloadStringAsync(uri, null);
                        break;
                    case "POST":
                        if (asyncCallback == null)
                        {
                            return webClient.UploadString(uri, data);
                        }
                        webClient.UploadStringCompleted += (sender, args) => asyncCallback(args.Result);
                        webClient.UploadStringAsync(uri, data);
                        break;
                }
            }
            return string.Empty;
        }

        // TODO: factor out the guts of this and the default timout method above with a private method taking a WebClient object
        public string ExecuteRequest(int timeoutMilliseconds, string method, string url, string data, List<Tuple<string, string>> headers = null, Action<string> asyncCallback = null)
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
                        webClient.DownloadStringCompleted += (sender, args) => asyncCallback(args.Result);
                        webClient.DownloadStringAsync(uri, null);
                        break;
                    case "POST":
                        if (asyncCallback == null)
                        {
                            return webClient.UploadString(uri, data);
                        }
                        webClient.UploadStringCompleted += (sender, args) => asyncCallback(args.Result);
                        webClient.UploadStringAsync(uri, data);
                        break;
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
                if(webRequest == null)
                {
                    throw new Exception(ErrorResource.WebRequestError);
                }
                webRequest.Timeout = _timeoutMilliseconds;
                return webRequest;
            }
        }
    }
}
