using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Dev2.Integration.Tests.MEF.WebTester;
using System.Text.RegularExpressions;

namespace Dev2.Integration.Tests.Helpers
{
    static class TestHelper
    {

        public static readonly string _tblStart = "<table>";
        public static readonly string _tblEnd = "</table>";

        static string _responseData;
        public static string ReturnFragment(string reponseData)
        {
            string FragmentName = "Dev2System.Fragment";
            int datastart = reponseData.IndexOf("<" + FragmentName + ">", 0) + ("<" + FragmentName + ">").Length;
            string DecodedFragment = reponseData.Substring(datastart, reponseData.IndexOf("</" + FragmentName + ">") - datastart);
            string Fragment = DecodedFragment.Replace("&amp;amp;lt;", "<").Replace("&amp;amp;gt;", ">");
            return Fragment;
        }



        public static string PostDataToWebserver(string postandUrl)
        {
            if (postandUrl.Split('?').Count() == 1)
            {
                ExecuteGetWorker(postandUrl);
            }
            else if (postandUrl.Split('?').Count() > 1)
            {
                ExecutePostWorker(postandUrl);
            }
            else
            {
                return _responseData = String.Empty;
            }
            return _responseData;
        }

        public static string ExtractDataBetween(string canidate, string startStr, string endStr)
        {
            string result = string.Empty;

            int start = canidate.IndexOf(startStr);
            if (start >= 0)
            {
                int end = canidate.LastIndexOf(endStr);
                if (end > start)
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

        private static void ExecuteGetWorker(string url)
        {
            GetWorker target = new GetWorker(url);
            target.DoWork();
            _responseData = target.GetResponseData();
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
