using System;
using System.Configuration;
using System.Net;
using System.Web;

namespace Warewolf.ReleaseWeb.Services
{
    //
    // NOTE: This is class for DEBUG only!
    //
    public class DebugProxy : IHttpHandler
    {
        // - To DEBUG
        //      - Change ReleaseVersion.js invokeProxy() to use DebugProxy.ashx
        //      - Edit/debug ProcessRequest here
        // - When you're finished debugging
        //      - copy/paste ProcessRequest to Proxy.ashx
        //      - Change ReleaseVersion.js invokeProxy() to use Proxy.ashx
        public void ProcessRequest(HttpContext context)
        {
            var parts = context.Request.RawUrl.Split('?');
            var response = "<root>Not Found</root>";
            if(parts.Length > 0)
            {
                try
                {
                    var client = new WebClient();
                    var parameters = parts[1].Split('&');
                    var action = parameters.Length == 0 ? parts[1] : parameters[0];
                    switch(action.ToLower())
                    {
                        case "getlatest":
                            response = client.DownloadString(ConfigurationManager.AppSettings["GetLatest"]);
                            break;
                        case "updateversion":
                            var currVer = context.Request.QueryString["c"];
                            var prevVer = context.Request.QueryString["p"];
                            var userName = context.User.Identity.Name;
                            var updateUrl = string.Format("{0}?PreviousVersion={1}&CurrentVersion={2}&UserName={3}",
                                ConfigurationManager.AppSettings["UpdateVersion"], prevVer, currVer, userName);
                            response = client.DownloadString(updateUrl);
                            break;
                        case "createrelease":
                            response = client.DownloadString(ConfigurationManager.AppSettings["CreateRelease"]);
                            break;
                        case "uploadrelease":
                            response = client.DownloadString(ConfigurationManager.AppSettings["UploadRelease"]);
                            break;
                    }
                }
                catch(Exception ex)
                {
                    response = string.Format("<root>{0}</root>", ex.Message);
                }
            }

            context.Response.ContentType = "text/xml";
            context.Response.Write(response);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}