/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Web;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace Dev2.Runtime.WebServer.Extentions
{
    public static class MiscellaneousWebExtensions
    {
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

        public static void CreateWarewolfErrorResponse(this HttpActionContext context, HttpStatusCode statusCode, string title, string message)
        {
           context.Response = CreateWarewolfErrorResponse(context.Request.RequestUri, statusCode, title, message);
        }

        public static HttpResponseMessage CreateWarewolfErrorResponse(this HttpRequestMessage requestMessage, HttpStatusCode statusCode, string tittle, string message) => CreateWarewolfErrorResponse(requestMessage.RequestUri, statusCode, tittle, message);
        public static HttpResponseMessage CreateWarewolfErrorResponse(Uri uri, HttpStatusCode statusCode, string tittle, string message)
        {
            var emitionType = uri.GetEmitionType();
            var calculatedMessage = ExecuteExceptionPayload.CreateErrorResponse(emitionType, statusCode, tittle, message);
            var content = emitionType.GetHttpStringContent(calculatedMessage);
            return new HttpResponseMessage(statusCode)
            {
                Content = content
            };
        }


    }
}
