using System;
using System.Collections.Generic;
using System.Net;

namespace Dev2.Activities
{
    public class WebRequestInvoker : IWebRequestInvoker
    {
        #region Implementation of IWebRequestInvoker

        public string ExecuteRequest(string method, string url)
        {
            return ExecuteRequest(method, url, new List<Tuple<string, string>>());
        }

        public string ExecuteRequest(string method, string url, List<Tuple<string, string>> headers)
        {
            using (var webClient = new WebClient())
            {
                webClient.Credentials = CredentialCache.DefaultCredentials;

                foreach (var header in headers)
                {
                    webClient.Headers.Add(header.Item1, header.Item2);
                }

                if (method == "GET")
                {
                    var pUrl = url.Contains("http://") || url.Contains("https://") ? url : "http://" + url;
                    return webClient.DownloadString(pUrl);
                }
            }
            return "";
        }
        #endregion
    }
}