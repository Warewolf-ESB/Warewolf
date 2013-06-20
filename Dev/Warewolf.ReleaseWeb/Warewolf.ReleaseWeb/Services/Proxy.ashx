<%@ WebHandler Language="C#"  Class="Warewolf.ReleaseWeb.Services.Proxy" %>

using System;
using System.Configuration;
using System.Net;
using System.Web;

namespace Warewolf.ReleaseWeb.Services
{
    /// <summary>
    /// Summary description for Proxy1
    /// </summary>
    public class Proxy : IHttpHandler
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
                        case "create":
                            var currVer = context.Request.QueryString["c"];
                            var prevVer = context.Request.QueryString["p"];
                            //var userName = context.User.Identity.Name;
                            var updateUrl = string.Format("{0}?PreviousVersion={1}&CurrentVersion={2}",
                                ConfigurationManager.AppSettings["Create"], prevVer, currVer);
                            response = client.DownloadString(updateUrl);
                            break;
                        case "upload":
                            response = client.DownloadString(ConfigurationManager.AppSettings["Upload"]);
                            break;
                        case "rollback":
                            response = client.DownloadString(ConfigurationManager.AppSettings["Rollback"]);
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