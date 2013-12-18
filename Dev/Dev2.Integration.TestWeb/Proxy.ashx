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
            var extension = context.Request.Headers["ext"];
            //var extension = "xml";
            //var parts = context.Request.RawUrl.Split('?');
            //if(parts.Length > 1)
            //{
            //    extension = parts[1];
            //}
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