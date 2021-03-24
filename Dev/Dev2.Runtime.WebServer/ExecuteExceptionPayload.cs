/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Interfaces;
using Dev2.Web;
using Newtonsoft.Json;
using System.Net;

namespace Dev2.Runtime.WebServer
{

    public class ExecuteExceptionPayload
    {
        private ExecuteExceptionPayload()
        {
        }

        public static string Calculate(IDSFDataObject dataObject)
        {
            var (statusCode, message) = GetResponseMessage(dataObject);
            var notDebug = !dataObject.IsDebug || dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke;
            if (notDebug)
            {
                switch (dataObject.ReturnType)
                {
                    case EmitionTypes.TRX:
                    case EmitionTypes.XML:
                        {
                            return $"<ExecutionError>" +
                                "<MessageId>" +statusCode+ "</MessageId>" +
                                "<Message>" + message + "</Message>"+
                                "</ExecutionError>";   
                        }
                    default: 
                    case EmitionTypes.OPENAPI:
                    case EmitionTypes.CoverJson:
                    case EmitionTypes.JSON:
                        {
                            return JsonConvert.SerializeObject(new { ExecutionError = new { MessageId = statusCode.ToString(), Message = message } });
                        }
                }
            }

            return string.Empty;
        }

        private static (HttpStatusCode statusCode, string message) GetResponseMessage(IDSFDataObject dataObject)
        {
            var env = dataObject.Environment;
            var exception = dataObject.ExecutionException;
            var hasException = exception != null;
            if (hasException)
            {
                return (HttpStatusCode.InternalServerError, message: exception.Message);
            }
            else if (!hasException && env.HasErrors())
            {
                return (HttpStatusCode.BadRequest, message: env.FetchErrors());
            }

            return (HttpStatusCode.NotImplemented, message: "Please consult system Administrator.");
        }
    }
}
