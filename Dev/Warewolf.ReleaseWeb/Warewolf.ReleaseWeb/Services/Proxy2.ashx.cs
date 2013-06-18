using System;
using System.Configuration;
using System.Net;
using System.Web;

namespace Warewolf.ReleaseWeb.Services
{
    /// <summary>
    /// Summary description for Proxy2
    /// </summary>
    public class Proxy2 : IHttpHandler
    {

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
                            var userName = "tbc"; //context.User.Identity.Name;
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