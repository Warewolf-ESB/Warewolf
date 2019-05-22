/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Core.Tests.Utils;
using Dev2.Network;

namespace TestBase
{
    public class TestHelper
    {
        public static readonly string TblStart = "<table>";
        public static readonly string TblEnd = "</table>";

        static string _responseData;
        public static string ReturnFragment(string reponseData)
        {
            const string FragmentName = "Dev2System.Fragment";
            var datastart = reponseData.IndexOf("<" + FragmentName + ">", 0, StringComparison.Ordinal) + ("<" + FragmentName + ">").Length;
            var decodedFragment = reponseData.Substring(datastart, reponseData.IndexOf("</" + FragmentName + ">", StringComparison.Ordinal) - datastart);
            var fragment = decodedFragment.Replace("&amp;amp;lt;", "<").Replace("&amp;amp;gt;", ">");
            return fragment;
        }

        public static string PostDataToWebserver(string postandUrl)
        {
            if (postandUrl.Split('?').Length == 1)
            {
                ExecuteGetWorker(postandUrl);
            }
            else if (postandUrl.Split('?').Length > 1)
            {
                ExecutePostWorker(postandUrl);
            }
            else
            {
                return _responseData = String.Empty;
            }

            return _responseData;
        }

        public static string ExecuteServiceOnLocalhostUsingProxy(string serviceName, Dictionary<string, string> payloadArguments)
        {
            var fact = new CommunicationControllerFactory();
            var comm = fact.CreateController(serviceName);
            var prx = new ServerProxy("http://localhost:3142", CredentialCache.DefaultNetworkCredentials, AsyncWorkerTests.CreateSynchronousAsyncWorkerObject());
            prx.Connect(Guid.NewGuid());
            foreach (var payloadArgument in payloadArguments)
            {
                comm.AddPayloadArgument(payloadArgument.Key, payloadArgument.Value);
            }
            if (comm != null)
            {
                var messageToExecute = comm.ExecuteCommand<ExecuteMessage>(prx, Guid.Empty);
                if (messageToExecute != null)
                {
                    var responseMessage = messageToExecute.Message;
                    if (responseMessage != null)
                    {
                        var actual = responseMessage.ToString();
                        return actual;
                    }
                    return "Error: response message empty!";
                }
                return "Error: message to send to localhost server could not be generated.";
            }
            return "Error: localhost server controller could not be created.";
        }

        public static string PostDataToWebserver(string postandUrl, out bool wasHttps)
        {
            wasHttps = false;
            if (postandUrl.Split('?').Length == 1)
            {
                wasHttps = ExecuteGetWorker(postandUrl);
            }
            else if (postandUrl.Split('?').Length > 1)
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
            var req = WebRequest.Create(myUri);
            req.Credentials = CredentialCache.DefaultCredentials;
            req.Method = "GET";

            using (var response = req.GetResponse() as HttpWebResponse)
            {
                if (response != null)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var data = reader.ReadToEnd();

                        if (data != null)
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
            var len = postandUrl.Split('?').Length;
            if (len == 1)
            {
                var result = string.Empty;

                var req = WebRequest.Create(postandUrl);
                req.Credentials = CredentialCache.DefaultCredentials;
                req.Method = "GET";

                req.Headers.Add(HttpRequestHeader.From, requestID.ToString()); // Set to remote invoke ID ;)
                req.Headers.Add(HttpRequestHeader.Cookie, GlobalConstants.RemoteServerInvoke);

                using (var response = req.GetResponse() as HttpWebResponse)
                {
                    if (response != null)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
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
            var result = string.Empty;

            var start = canidate.IndexOf(startStr, StringComparison.Ordinal);
            if (start >= 0)
            {
                var end = canidate.LastIndexOf(endStr, StringComparison.Ordinal);
                if (end > start)
                {
                    result = canidate.Substring(start, end + endStr.Length - start);
                }
            }

            return result;
        }

        public static string CleanUp(string str)
        {
            return str.Replace("\r", "").Replace("\n", "").Replace(" ", "");
        }

        static bool ExecuteGetWorker(string url)
        {
            var target = new GetWorker(url);
            target.DoWork();
            _responseData = target.GetResponseData();

            return target.WasHTTPS;
        }

        static void ExecutePostWorker(string postDataWithUrl)
        {
            var target = new PostWorker(postDataWithUrl);
            target.DoWork();
            _responseData = target.GetResponseData();
        }

        public static List<string> BreakHtmlElement(string payload)
        {
            var htmlElems = new List<string>();
            var elements = XElement.Parse(payload);
            if (elements.HasElements)
            {
                foreach (XElement elem in elements.Descendants())
                {
                    if (elem.Name != "script")
                    {
                        if (elem.HasElements)
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
            var regex = new Regex(@">\s*<");

            payload = regex.Replace(payload, "><");
            return payload;
        }

        public static HttpWebResponse GetResponseFromServer(string path)
        {
            var target = new GetWorker(path);
            target.DoWork();
            return target.GetResponse();
        }
    }
}
