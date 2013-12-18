<%@ WebHandler Language="C#"  Class="Warewolf.ReleaseWeb.Services.Proxy" %>

using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
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
            var response = "<root>Not Found</root>";
            var parts = context.Request.RawUrl.Split('?');
            var extension = parts.Length > 1 ? parts[1] : context.Request.Headers["extension"];

            if(string.IsNullOrEmpty(extension))
            {
                extension = "xml";
            }
            else
            {
                try
                {
                    var root = context.Request.MapPath("~/Files");

                    var path = Path.Combine(root, "test." + extension);
                    response = File.ReadAllText(path);
                }
                catch(Exception ex)
                {
                    response = string.Format("<root>{0}</root>", ex.Message);
                }
            }

            context.Response.ContentType = "text/" + extension;
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