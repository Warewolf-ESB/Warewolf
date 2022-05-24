#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Web;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Web.Http.Controllers;
#if !NETFRAMEWORK
using Microsoft.AspNetCore.Http.Extensions;
using System.Web.Http.Controllers;
using Microsoft.AspNetCore.Http;
#endif

namespace Dev2.Runtime.WebServer
{
    public static class Extensions
    {
        public static bool IsAuthenticated(this IPrincipal user)
        {
            if (user == null)
            {
                Dev2Logger.Debug("Null User", GlobalConstants.WarewolfDebug);
            }

            return user != null && user.Identity.IsAuthenticated;
        }

        public static Encoding GetContentEncoding(this HttpContent content)
        {
            var encoding = content == null ? String.Empty : content.Headers.ContentEncoding.FirstOrDefault();
            if (!String.IsNullOrEmpty(encoding))
            {
                try
                {
                    return Encoding.GetEncoding(encoding);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error("Dev2.Runtime.WebServer.Extensions", ex, GlobalConstants.WarewolfError);
                }
            }
            return Encoding.UTF8;
        }

        public static EmitionTypes GetEmitionType(this Uri uri)
        {
            var isXml = uri.AbsolutePath.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase);
            var isTrx = uri.AbsolutePath.EndsWith(".trx", StringComparison.InvariantCultureIgnoreCase);

            if (isXml || isTrx)
            {
                return EmitionTypes.XML;
            }
            return EmitionTypes.JSON;
        }

        public static StringContent GetHttpStringContent(this EmitionTypes emitionType, string message)
        {
            return emitionType == EmitionTypes.XML || emitionType == EmitionTypes.TRX
                          ? new StringContent(message, Encoding.UTF8, "application/xml")
                          : new StringContent(message, Encoding.UTF8, "application/json");
        }

#if NETFRAMEWORK
        public static HttpResponseMessage CreateWarewolfErrorResponse(this HttpRequestMessage requestMessage, WarewolfErrorResponseArgs errorResponseArgs) => CreateWarewolfErrorResponse(requestMessage.RequestUri, errorResponseArgs);
#else
        public static HttpResponseMessage CreateWarewolfErrorResponse(this HttpRequest requestMessage, WarewolfErrorResponseArgs errorResponseArgs) => CreateWarewolfErrorResponse(new Uri(requestMessage.GetDisplayUrl()), errorResponseArgs);
#endif
        public static HttpResponseMessage CreateWarewolfErrorResponse(this HttpActionContext requestMessage, WarewolfErrorResponseArgs errorResponseArgs) => requestMessage.Response = CreateWarewolfErrorResponse(requestMessage.Request.RequestUri, errorResponseArgs);
        public static HttpResponseMessage CreateWarewolfErrorResponse(Uri uri, WarewolfErrorResponseArgs errorResponseArgs) => CreateWarewolfErrorResponse(uri.GetEmitionType(), errorResponseArgs.StatusCode, errorResponseArgs.Title, errorResponseArgs.Message);
        public static HttpResponseMessage CreateWarewolfErrorResponse(EmitionTypes emitionType, HttpStatusCode statusCode, string title, string message)
        {
            var calculatedMessage = ExecuteExceptionPayload.CreateErrorResponse(emitionType, statusCode, title, message);
            var content = emitionType.GetHttpStringContent(calculatedMessage);
            return new HttpResponseMessage(statusCode)
            {
                Content = content
            };
        }
    }

    public class WarewolfErrorResponseArgs
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}