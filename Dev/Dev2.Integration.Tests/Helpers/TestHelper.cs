using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Diagnostics;
using Dev2.Integration.Tests.MEF.WebTester;
using Newtonsoft.Json;

namespace Dev2.Integration.Tests.Helpers
{
    public static class TestHelper
    {

        public static readonly string _tblStart = "<table>";
        public static readonly string _tblEnd = "</table>";

        static string _responseData;
        public static string ReturnFragment(string reponseData)
        {
            const string FragmentName = "Dev2System.Fragment";
            int datastart = reponseData.IndexOf("<" + FragmentName + ">", 0, StringComparison.Ordinal) + ("<" + FragmentName + ">").Length;
            string DecodedFragment = reponseData.Substring(datastart, reponseData.IndexOf("</" + FragmentName + ">", StringComparison.Ordinal) - datastart);
            string Fragment = DecodedFragment.Replace("&amp;amp;lt;", "<").Replace("&amp;amp;gt;", ">");
            return Fragment;
        }


        public static string PostDataToWebserver(string postandUrl)
        {
            if(postandUrl.Split('?').Count() == 1)
            {
                ExecuteGetWorker(postandUrl);
            }
            else if(postandUrl.Split('?').Count() > 1)
            {
                ExecutePostWorker(postandUrl);
            }
            else
            {
                return _responseData = String.Empty;
            }

            return _responseData;
        }

        public static string PostDataToWebserver(string postandUrl, out bool wasHTTPS)
        {
            wasHTTPS = false;
            if(postandUrl.Split('?').Count() == 1)
            {
                wasHTTPS = ExecuteGetWorker(postandUrl);
            }
            else if(postandUrl.Split('?').Count() > 1)
            {
                ExecutePostWorker(postandUrl);
            }
            else
            {
                return _responseData = String.Empty;
            }

            return _responseData;
        }


        public static IList<DebugState> FetchRemoteDebugItems(string baseURL, Guid id)
        {
            var myURI = baseURL + "FetchRemoteDebugMessagesService?InvokerID=" + id.ToString();
            WebRequest req = WebRequest.Create(myURI);
            req.Credentials = CredentialCache.DefaultCredentials;
            req.Method = "GET";

            using(var response = req.GetResponse() as HttpWebResponse)
            {
                if(response != null)
                {
                    using(StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var data = reader.ReadToEnd();
                        if(data != null)
                        {
                            return JsonConvert.DeserializeObject<List<DebugState>>(data);
                        }
                    }
                }
            }

            return null;

        }

        public static string PostDataToWebserverAsRemoteAgent(string postandUrl, Guid requestID)
        {
            var len = postandUrl.Split('?').Count();
            if(len == 1)
            {
                string result = string.Empty;

                WebRequest req = WebRequest.Create(postandUrl);
                req.Credentials = CredentialCache.DefaultCredentials;
                req.Method = "GET";

                req.Headers.Add(HttpRequestHeader.From, requestID.ToString()); // Set to remote invoke ID ;)
                req.Headers.Add(HttpRequestHeader.Cookie, GlobalConstants.RemoteServerInvoke);

                using(var response = req.GetResponse() as HttpWebResponse)
                {
                    if(response != null)
                    {
                        // ReSharper disable AssignNullToNotNullAttribute
                        using(StreamReader reader = new StreamReader(response.GetResponseStream()))
                        // ReSharper restore AssignNullToNotNullAttribute
                        {
                            result = reader.ReadToEnd();
                        }
                    }
                }

                return result;
            }

            return string.Empty;
        }

        public static string ExtractDataBetween(string canidate, string startStr, string endStr)
        {
            string result = string.Empty;

            int start = canidate.IndexOf(startStr, StringComparison.Ordinal);
            if(start >= 0)
            {
                int end = canidate.LastIndexOf(endStr, StringComparison.Ordinal);
                if(end > start)
                {
                    result = canidate.Substring(start, ((end + endStr.Length) - start));
                }
            }

            return result;
        }

        public static string CleanUp(string str)
        {
            return str.Replace("\r", "").Replace("\n", "").Replace(" ", "");
        }

        private static bool ExecuteGetWorker(string url)
        {
            GetWorker target = new GetWorker(url);
            target.DoWork();
            _responseData = target.GetResponseData();

            return target.WasHTTPS;
        }

        private static void ExecutePostWorker(string postDataWithURL)
        {
            PostWorker target = new PostWorker(postDataWithURL);
            target.DoWork();
            _responseData = target.GetResponseData();
        }

        public static List<string> BreakHTMLElement(string payload)
        {
            List<string> HTMLElems = new List<string>();
            XElement elements = XElement.Parse(payload);
            if(elements.HasElements)
            {
                foreach(XElement elem in elements.Descendants())
                {
                    if(elem.Name != "script")
                    {
                        if(elem.HasElements)
                        {

                        }
                        else
                        {
                            HTMLElems.Add(elem.ToString());
                        }
                    }
                    else
                    {
                        HTMLElems.Add(elem.ToString());
                    }
                }
            }
            else
            {
                HTMLElems.Add(elements.ToString());
            }

            return HTMLElems;
        }

        public static string RemoveWhiteSpaceBetweenTags(string payload)
        {
            Regex regex = new Regex(@">\s*<");

            payload = regex.Replace(payload, "><");
            return payload;
        }

        public static HttpWebResponse GetResponseFromServer(string path)
        {
            GetWorker target = new GetWorker(path);
            target.DoWork();
            return target.GetResponse();
        }
    }
}
