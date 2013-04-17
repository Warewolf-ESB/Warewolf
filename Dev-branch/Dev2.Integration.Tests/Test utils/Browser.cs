using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

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
                return request.GetResponse().ReadAllContent().ToString();
            }
            catch (WebException httpex)
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
            if (data == null)
            {
                request.ContentLength = 0;
            }
            else
            {
                using (var requestStream = request.GetRequestStream())
                using (var writer = new StreamWriter(requestStream, new UTF8Encoding(false)))
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
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result;
            }
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
