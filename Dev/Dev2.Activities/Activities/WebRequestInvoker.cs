
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Net;

namespace Dev2.Activities
{
    public class WebRequestInvoker : IWebRequestInvoker
    {
        public string ExecuteRequest(string method, string url)
        {
            return ExecuteRequest(method, url, new List<Tuple<string, string>>());
        }

        public string ExecuteRequest(string method, string url, List<Tuple<string, string>> headers)
        {
            return ExecuteRequest(method, url, null, headers);
        }

        public string ExecuteRequest(string method, string url, string data, List<Tuple<string, string>> headers = null, Action<string> asyncCallback = null)
        {
            using(var webClient = new WebClient())
            {
                webClient.Credentials = CredentialCache.DefaultCredentials;

                if(headers != null)
                {
                    foreach(var header in headers)
                    {
                        webClient.Headers.Add(header.Item1, header.Item2);
                    }
                }

                var uri = new Uri(url.Contains("http://") || url.Contains("https://") ? url : "http://" + url);

                switch(method)
                {
                    case "GET":
                        if(asyncCallback == null)
                        {
                            return webClient.DownloadString(uri);
                        }
                        webClient.DownloadStringCompleted += (sender, args) => asyncCallback(args.Result);
                        webClient.DownloadStringAsync(uri, null);
                        break;
                    case "POST":
                        if(asyncCallback == null)
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

    }
}
