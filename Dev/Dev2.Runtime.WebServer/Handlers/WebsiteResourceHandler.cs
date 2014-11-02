/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
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
            string uriString = ctx.Request.Uri.OriginalString;

            if (uriString.IndexOf("wwwroot", StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                // http://127.0.0.1:1234/services/"/themes/system/js/json2.js"
                new WebGetRequestHandler().ProcessRequest(ctx);
                return;
            }

            string website = GetWebsite(ctx);
            string path = GetPath(ctx);
            string extension = Path.GetExtension(uriString);

            if (string.IsNullOrEmpty(extension))
            {
                //
                // REST request e.g. http://localhost:1234/wwwroot/sources/server
                //
                const string ContentToken = "getParameterByName(\"content\")";

                string layoutFilePath = string.Format("{0}\\webs\\{1}\\layout.htm", Location, website);
                string contentPath = string.Format("\"/{0}/views/{1}.htm\"", website, path);

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
            if (!string.IsNullOrEmpty(path) && path.IndexOf('/') == -1)
            {
                uriString = uriString.Replace(path, "");
            }

            IResponseWriter result = GetFileFromPath(new Uri(uriString));

            ctx.Send(result);
        }

        private IResponseWriter GetFileFromPath(Uri uri)
        {
            string filePath = string.Format("{0}\\Webs{1}\\{2}", Location,
                Path.GetDirectoryName(uri.LocalPath),
                Path.GetFileName(uri.LocalPath));
            return GetFileFromPath(filePath);
        }

        private static IResponseWriter GetFileFromPath(string filePath)
        {
            string supportedFileExtensions = ConfigurationManager.AppSettings["SupportedFileExtensions"];
            string extension = Path.GetExtension(filePath);
            string ext = string.IsNullOrEmpty(extension) ? "" : extension;
            IEnumerable<string> isSupportedExtensionList = supportedFileExtensions.Split(new[] {','})
                .ToList()
                .Where(
                    supportedExtension =>
                        supportedExtension.Trim().Equals(ext, StringComparison.InvariantCultureIgnoreCase));

            if (string.IsNullOrEmpty(supportedFileExtensions) || !isSupportedExtensionList.Any())
            {
                return new StatusResponseWriter(HttpStatusCode.NotFound);
            }

            if (File.Exists(filePath))
            {
                string contentType;
                switch (ext.ToLower())
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