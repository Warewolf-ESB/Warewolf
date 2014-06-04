using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using Dev2.Runtime.WebServer.Responses;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class WebsiteResourceHandler : AbstractWebRequestHandler
    {
        public override void ProcessRequest(ICommunicationContext ctx)
        {
            var uriString = ctx.Request.Uri.OriginalString;

            if(uriString.IndexOf("wwwroot", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                // http://127.0.0.1:1234/services/"/themes/system/js/json2.js"
                new WebGetRequestHandler().ProcessRequest(ctx);
                return;
            }

            var website = GetWebsite(ctx);
            var path = GetPath(ctx);
            var extension = Path.GetExtension(uriString);

            if(string.IsNullOrEmpty(extension))
            {
                //
                // REST request e.g. http://localhost:1234/wwwroot/sources/server
                //
                const string ContentToken = "getParameterByName(\"content\")";

                var layoutFilePath = string.Format("{0}\\webs\\{1}\\layout.htm", Location, website);
                var contentPath = string.Format("\"/{0}/views/{1}.htm\"", website, path);

                ctx.Send(new DynamicFileResponseWriter(layoutFilePath, ContentToken, contentPath));
                return;
            }

            // Should get url's with the following signatures
            //
            // http://localhost:1234/wwwroot/sources/Scripts/jquery-1.7.1.js
            // http://localhost:1234/wwwroot/sources/Content/Site.css
            // http://localhost:1234/wwwroot/sources/images/error.png
            // http://localhost:1234/wwwroot/sources/Views/Dialogs/SaveDialog.htm
            // http://localhost:1234/wwwroot/views/sources/server.htm
            //
            // We support only 1 level below the Views folder 
            // If path is a string without a backslash then we are processing the following request
            //       http://localhost:1234/wwwroot/views/sources/server.htm
            // If path is a string with a backslash then we are processing the following request
            //       http://localhost:1234/wwwroot/sources/Views/Dialogs/SaveDialog.htm
            //
            if(!string.IsNullOrEmpty(path) && path.IndexOf('/') == -1)
            {
                uriString = uriString.Replace(path, "");
            }

            var result = GetFileFromPath(new Uri(uriString));

            ctx.Send(result);
        }

        IResponseWriter GetFileFromPath(Uri uri)
        {
            var filePath = string.Format("{0}\\Webs{1}\\{2}", Location,
                Path.GetDirectoryName(uri.LocalPath),
                Path.GetFileName(uri.LocalPath));
            return GetFileFromPath(filePath);
        }

        static IResponseWriter GetFileFromPath(string filePath)
        {
            var supportedFileExtensions = ConfigurationManager.AppSettings["SupportedFileExtensions"];
            var extension = Path.GetExtension(filePath);
            var ext = string.IsNullOrEmpty(extension) ? "" : extension;
            var isSupportedExtensionList = supportedFileExtensions.Split(new[] { ',' })
                .ToList()
                .Where(supportedExtension => supportedExtension.Trim().Equals(ext, StringComparison.InvariantCultureIgnoreCase));

            if(string.IsNullOrEmpty(supportedFileExtensions) || !isSupportedExtensionList.Any())
            {
                return new StatusResponseWriter(HttpStatusCode.NotFound);
            }

            if(File.Exists(filePath))
            {
                string contentType;
                switch(ext.ToLower())
                {
                    case ".js":
                        contentType = "text/javascript";
                        break;

                    case ".css":
                        contentType = "text/css";
                        break;

                    case ".ico":
                        contentType = "image/x-icon";
                        break;

                    case ".bm":
                    case ".bmp":
                        contentType = "image/bmp";
                        break;

                    case ".gif":
                        contentType = "image/gif";
                        break;

                    case ".jpeg":
                    case ".jpg":
                        contentType = "image/jpg";
                        break;

                    case ".tiff":
                        contentType = "image/tiff";
                        break;

                    case ".png":
                        contentType = "image/png";
                        break;

                    case ".htm":
                    case ".html":
                        contentType = "text/html";
                        break;

                    default:
                        return new StatusResponseWriter(HttpStatusCode.NotFound);
                }
                return new StaticFileResponseWriter(filePath, contentType);
            }
            return new StatusResponseWriter(HttpStatusCode.NotFound);
        }

    }
}