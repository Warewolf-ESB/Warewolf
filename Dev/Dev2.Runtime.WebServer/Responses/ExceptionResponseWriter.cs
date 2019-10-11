/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Net;
using System.Net.Http;

namespace Dev2.Runtime.WebServer.Responses
{
    public class ExceptionResponseWriter : IResponseWriter
    {
        readonly HttpStatusCode _statusCode;
        readonly string _message;
        public ExceptionResponseWriter(HttpStatusCode statusCode, string message)
        {
            _statusCode = statusCode;
            _message = message;
        }
        public new void Write(IResponseMessageContext context)
        {
            context.ResponseMessage.StatusCode = _statusCode;
            context.ResponseMessage.Content = new StringContent(_message);
        }
    }
}