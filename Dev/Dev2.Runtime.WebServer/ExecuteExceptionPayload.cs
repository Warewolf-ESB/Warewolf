/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Web;
using Newtonsoft.Json;
using System.Net;
using Warewolf.Data.Serializers;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.WebServer
{

    public class ExecuteExceptionPayload
    {
        private ExecuteExceptionPayload()
        {
        }

        public static string Calculate(IDSFDataObject dataObject)
        {
            var (statusCode, title, message) = GetResponseMessage(dataObject);
            var status = (int)statusCode;
            var warewolfErrors = new Error
            {
                Status = status,
                Title = title,
                Message = message
            };
            var notDebug = !dataObject.IsDebug || dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke;
            if (notDebug)
            {

                switch (dataObject.ReturnType)
                {
                    case EmitionTypes.TRX:
                    case EmitionTypes.XML:
                        {
                            return Scrubber.Scrub(warewolfErrors.SerializeToXml(), ScrubType.Xml);
                        }
                    default: 
                    case EmitionTypes.OPENAPI:
                    case EmitionTypes.CoverJson:
                    case EmitionTypes.JSON:
                        {
                            return JsonConvert.SerializeObject(new { Error = warewolfErrors }, Formatting.Indented);
                        }
                }
            }

            return string.Empty;
        }

        public class Error
        {
            public int Status { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }

        }


        private static (HttpStatusCode statusCode, string tittle, string message) GetResponseMessage(IDSFDataObject dataObject)
        {
            var env = dataObject.Environment;
            var exception = dataObject.ExecutionException;
            var hasException = exception != null;
            if (hasException)
            {
                return (HttpStatusCode.InternalServerError, "internal_server_error", message: exception.Message);
            }
            else if (!hasException && env.HasErrors())
            {
                return (HttpStatusCode.BadRequest, "bad_request", message: env.FetchErrors());
            }

            return (HttpStatusCode.NotImplemented, "not_implemented", message: ErrorResource.MethodNotImplemented);
        }
    }
}
