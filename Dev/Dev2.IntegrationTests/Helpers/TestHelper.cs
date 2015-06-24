
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Communication;
using Dev2.Diagnostics.Debug;
using Dev2.Integration.Tests.MEF.WebTester;

namespace Dev2.Integration.Tests.Helpers
{
    public static class TestHelper
    {

        public static readonly string TblStart = "<table>";
        public static readonly string TblEnd = "</table>";

        static string _responseData;
        public static string ReturnFragment(string reponseData)
        {
            const string FragmentName = "Dev2System.Fragment";
            int datastart = reponseData.IndexOf("<" + FragmentName + ">", 0, StringComparison.Ordinal) + ("<" + FragmentName + ">").Length;
            string decodedFragment = reponseData.Substring(datastart, reponseData.IndexOf("</" + FragmentName + ">", StringComparison.Ordinal) - datastart);
            string fragment = decodedFragment.Replace("&amp;amp;lt;", "<").Replace("&amp;amp;gt;", ">");
            return fragment;
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

        public static string PostDataToWebserver(string postandUrl, out bool wasHttps)
        {
            wasHttps = false;
            if(postandUrl.Split('?').Count() == 1)
            {
                wasHttps = ExecuteGetWorker(postandUrl);
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


        public static IList<IDebugState> FetchRemoteDebugItems(string baseUrl, Guid id)
        {
            var myUri = baseUrl + "FetchRemoteDebugMessagesService?InvokerID=" + id.ToString();
            WebRequest req = WebRequest.Create(myUri);
            req.Credentials = CredentialCache.DefaultCredentials;
            req.Method = "GET";

            using(var response = req.GetResponse() as HttpWebResponse)
            {
                if(response != null)
                {
                    // ReSharper disable AssignNullToNotNullAttribute
                    using(StreamReader reader = new StreamReader(response.GetResponseStream()))
                    // ReSharper restore AssignNullToNotNullAttribute
                    {

                        var data = reader.ReadToEnd();
                        // ReSharper disable ConditionIsAlwaysTrueOrFalse
                        if(data != null)
                        // ReSharper restore ConditionIsAlwaysTrueOrFalse
                        {
                            var serializer = new Dev2JsonSerializer();
                            return serializer.Deserialize<List<IDebugState>>(data);
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

        private static void ExecutePostWorker(string postDataWithUrl)
        {
            PostWorker target = new PostWorker(postDataWithUrl);
            target.DoWork();
            _responseData = target.GetResponseData();
        }

        public static List<string> BreakHtmlElement(string payload)
        {
            List<string> htmlElems = new List<string>();
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
                            htmlElems.Add(elem.ToString());
                        }
                    }
                    else
                    {
                        htmlElems.Add(elem.ToString());
                    }
                }
            }
            else
            {
                htmlElems.Add(elements.ToString());
            }

            return htmlElems;
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
