
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
using System.Web;

namespace Dev2.Integration.TestWeb
{
    public abstract class AbstractHttpHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string response;

            string extension = null;
            var contentType = context.Request.Headers["Content-Type"];
            switch(contentType)
            {
                case "application/json":
                    extension = "json";
                    break;
                case "application/xml":
                    extension = "xml";
                    break;
                default:
                    var urlParts = context.Request.RawUrl.Split('?');
                    if(urlParts.Length == 2)
                    {
                        var queryParts = urlParts[1].Split('&');
                        if(queryParts.Length == 1)
                        {
                            extension = queryParts[0];
                        }
                    }
                    if(string.IsNullOrEmpty(extension))
                    {
                        extension = context.Request.Headers["extension"];
                        if(string.IsNullOrEmpty(extension))
                        {
                            extension = context.Request.Params["extension"];
                            if(string.IsNullOrEmpty(extension))
                            {
                                extension = "xml";
                            }
                        }
                    }
                    break;
            }

            try
            {
                response = GetResponse(context, extension);
            }
            catch(Exception ex)
            {
                response = string.Format("<root>{0}</root>", ex.Message);
            }

            context.Response.ContentType = "text/" + extension;
            context.Response.Write(response);
        }

        protected abstract string GetResponse(HttpContext context, string extension);

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
