/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Net.Http;

namespace Dev2.Runtime.WebServer.Responses
{
    public interface IResponseMessageContext
    {
        HttpResponseMessage ResponseMessage { get; }
    }

    public interface IResponseWriter
    {
        void Write(IResponseMessageContext context);
    }

    public interface IStringResponseWriterFactory
    {
        IResponseWriter New(string executePayload, string contentType);
    }

    public class StringResponseWriterFactory : IStringResponseWriterFactory
    {
        public IResponseWriter New(string executePayload, string contentType)
        {
            return new StringResponseWriter(executePayload, contentType);
        }
    }
}
