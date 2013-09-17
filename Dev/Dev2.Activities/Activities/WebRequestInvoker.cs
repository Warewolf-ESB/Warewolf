using System.Net;

namespace Dev2.Activities
{
    public class WebRequestInvoker : IWebRequestInvoker
    {
        #region Implementation of IWebRequestInvoker

        public virtual string ExecuteRequest(string method, string url)
        {
            using(var webClient = new WebClient())
            {
                if(method == "GET")
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