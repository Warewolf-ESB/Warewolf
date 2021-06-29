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
using Dev2.Interfaces;
using Dev2.Web;
using System.Net;
using Dev2.Runtime.WebServer.Executor;
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
                            return warewolfErrors.ToXML();
                        }
                    default: 
                    case EmitionTypes.OPENAPI:
                    case EmitionTypes.CoverJson:
                    case EmitionTypes.JSON:
                        {
                            return warewolfErrors.ToJSON();
                        }
                }
            }

            return string.Empty;
        }

        private static (HttpStatusCode statusCode, string tittle, string message) GetResponseMessage(IDSFDataObject dataObject)
        {
            var env = dataObject.Environment;
            var exception = dataObject.ExecutionException;
            var hasException = exception != null;
            if (hasException)
            {
                return (HttpStatusCode.InternalServerError, GlobalConstants.INTERNAL_SERVER_ERROR, message: exception.Message);
            }
            else if (!hasException && env.HasErrors())
            {
                return (HttpStatusCode.BadRequest, GlobalConstants.BAD_REQUEST, message: env.FetchErrors());
            }

            return (HttpStatusCode.NotImplemented, GlobalConstants.NOT_IMPLEMENTED, message: ErrorResource.MethodNotImplemented);
        }

        public static string CreateErrorResponse(EmitionTypes emissionType,  HttpStatusCode statusCode, string title, string message)
        {
            switch (emissionType)
            {
                case EmitionTypes.XML:
                case EmitionTypes.TRX:
                    return new Error
                    {
                        Status = (int)statusCode,
                        Title = title,
                        Message = message
                    }.ToXML();
                default:
                case EmitionTypes.JSON:
                case EmitionTypes.OPENAPI:
                case EmitionTypes.TEST:
                case EmitionTypes.Cover:
                case EmitionTypes.CoverJson:
                    return new Error
                    {
                        Status = (int)statusCode,
                        Title = title,
                        Message = message
                    }.ToJSON();
            }
        }
    }
}
