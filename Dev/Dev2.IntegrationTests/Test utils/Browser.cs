
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
using System.IO;
using System.Net;
using System.Text;

namespace Dev2.Integration.Tests.Utils
{
    public class Browser
    {
        public static string ExecuteGet(string url)
        {
            var request = WebRequest.Create(url);
            request.Method = "GET";
            request.ContentLength = 0;
            try
            {
                return request.GetResponse().ReadAllContent();
            }
            catch(WebException httpex)
            { return httpex.Message; }
        }

        public static Action CancelableGet(string url)
        {
            var request = WebRequest.Create(url);
            request.Method = "GET";
            request.ContentLength = 0;
            var asynctask = request.BeginGetResponse(r => { }, null);
            return () => asynctask.AsyncWaitHandle.Close();
        }

        public static HttpWebResponse ExecutePost(string url, string data = null)
        {
            var request = WebRequest.Create(url);
            request.Method = "POST";
            if(data == null)
            {
                request.ContentLength = 0;
            }
            else
            {
                using(var requestStream = request.GetRequestStream())
                using(var writer = new StreamWriter(requestStream, new UTF8Encoding(false)))
                {
                    writer.Write(data);
                }
                request.ContentType = "text/plain;charset=UTF-8";
            }
            return (HttpWebResponse)request.GetResponse();
        }

        public void ExecuteStuff(string url)
        {
            ExecuteGet(url);
        }
    }

    public static class WebResponseHelpers
    {
        public static string ReadAllContent(this WebResponse response)
        {
            if(response != null)
            {
                // ReSharper disable AssignNullToNotNullAttribute
                using(var streamReader = new StreamReader(response.GetResponseStream()))
                // ReSharper restore AssignNullToNotNullAttribute
                {
                    var result = streamReader.ReadToEnd();
                    return result;
                }
            }

            return string.Empty;
        }
    }

    public static class ObjectExtensions
    {
        public static T OfType<T>(this object o)
        {
            return (T)o;
        }
    }
}
