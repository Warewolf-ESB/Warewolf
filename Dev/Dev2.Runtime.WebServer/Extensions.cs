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
using Microsoft.AspNetCore.Mvc.WebApiCompatShim;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Principal;
using System.Text;
//using System.Web.Http.Controllers;

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
                          ? new StringContent(message, System.Text.Encoding.UTF8, "application/xml")
                          : new StringContent(message, System.Text.Encoding.UTF8, "application/json");
        }

        //public static void CreateWarewolfErrorResponse(this HttpActionContext context, WarewolfErrorResponseArgs errorResponseArgs)
        //{
        //    context.Response = CreateWarewolfErrorResponse(context.Request.RequestUri, errorResponseArgs);
        //}

        public static void CreateWarewolfErrorResponse(this Microsoft.AspNetCore.Http.HttpContext context, WarewolfErrorResponseArgs errorResponseArgs)
        {
            var errorResponse = CreateWarewolfErrorResponse(context.Request.ToUri(), errorResponseArgs);
            var errorContent = errorResponse.Content.ReadAsStringAsync().Result;
            //context.GetHttpRequestMessage().CreateErrorResponse(errorResponse.StatusCode, errorContent);
            context.GetHttpRequestMessage().CreateResponse(errorResponse.StatusCode, errorContent, new JsonMediaTypeFormatter());

            //context.Response = errorResponse;
        }

        //public static void CreateWarewolfErrorResponse(this Microsoft.AspNetCore.Mvc.ActionContext actionContext, WarewolfErrorResponseArgs errorResponseArgs)
        //{
        //    var context = actionContext.HttpContext;
        //    var errorResponse = CreateWarewolfErrorResponse(context.Request.ToUri(), errorResponseArgs);
        //    var errorContent = errorResponse.Content.ReadAsStringAsync().Result;
        //    context.GetHttpRequestMessage().CreateErrorResponse(errorResponse.StatusCode, errorContent);

        //    //context.Response = errorResponse;
        //}



        public static HttpResponseMessage CreateWarewolfErrorResponse(this HttpRequestMessage requestMessage, WarewolfErrorResponseArgs errorResponseArgs) => CreateWarewolfErrorResponse(requestMessage.RequestUri, errorResponseArgs);
        public static HttpResponseMessage CreateWarewolfErrorResponse(Uri uri, WarewolfErrorResponseArgs errorResponseArgs) => CreateWarewolfErrorResponse(uri.GetEmitionType(), errorResponseArgs.StatusCode, errorResponseArgs.Title, errorResponseArgs.Message);
        public static HttpResponseMessage CreateWarewolfErrorResponse(EmitionTypes emitionType, HttpStatusCode statusCode, string tittle, string message)
        {
            var calculatedMessage = ExecuteExceptionPayload.CreateErrorResponse(emitionType, statusCode, tittle, message);
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