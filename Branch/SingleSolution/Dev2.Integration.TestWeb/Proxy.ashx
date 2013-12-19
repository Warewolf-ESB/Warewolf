<%@ WebHandler Language="C#"  Class="Warewolf.ReleaseWeb.Services.Proxy" %>

using System;
using System.IO;
using System.Web;

namespace Warewolf.ReleaseWeb.Services
{
    /// <summary>
    ///   Summary description for Proxy1
    /// </summary>
    public class Proxy : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string response = "<root>Not Found</root>";
            string[] parts = context.Request.RawUrl.Split('?');
            string extension = parts.Length > 1 ? parts[1] : context.Request.Headers["extension"];

            string contentType = context.Request.Headers["Content-Type"];
            if (!string.IsNullOrEmpty(contentType))
            {
                switch (contentType)
                {
                    case "application/json":
                        {
                            extension = "json";
                            break;
                        }
                    case "application/xml":
                        {
                            extension = "xml";
                            break;
                        }
                }
            }

            if (string.IsNullOrEmpty(extension))
            {
                extension = "xml";
            }
            else
            {
                try
                {
                    string root = context.Request.MapPath("~/Files");

                    string path = Path.Combine(root, "test." + extension);
                    response = File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    response = string.Format("<root>{0}</root>", ex.Message);
                }
            }

            context.Response.ContentType = "text/" + extension;
            context.Response.Write(response);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}