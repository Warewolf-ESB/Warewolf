
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
using System.Collections.Specialized;
using System.Net.Http;
using Dev2.Runtime.WebServer.Responses;

namespace Dev2.Runtime.WebServer
{
    public class WebServerContext : ICommunicationContext, IDisposable
    {
        readonly HttpRequestMessage _request;

        public WebServerContext(HttpRequestMessage request, NameValueCollection requestPaths)
        {
            _request = request;
            ResponseMessage = request.CreateResponse();
            Request = new WebServerRequest(request, requestPaths);
            Response = new WebServerResponse(ResponseMessage);
        } 
        
        public WebServerContext(HttpRequestMessage request)
        {
            _request = request;
            ResponseMessage = request.CreateResponse();
            Request = new WebServerRequest(request);
            Response = new WebServerResponse(ResponseMessage);
        }

        public HttpResponseMessage ResponseMessage { get; private set; }

        public ICommunicationRequest Request { get; private set; }
        public ICommunicationResponse Response { get; private set; }

        public void Send(IResponseWriter response)
        {
            VerifyArgument.IsNotNull("response", response);
            response.Write(this);
        }

        public NameValueCollection FetchHeaders()
        {
            var result = new NameValueCollection();
            foreach(var header in _request.Headers)
            {
                result.Add(header.Key, string.Join("; ", header.Value));
            }
            return result;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if(Request != null && Request.InputStream != null)
                {
                    Request.InputStream.Close();
                    Request.InputStream.Dispose();
                }
                if(Response.Response != null)
                {
                    ResponseMessage.Dispose();
                    Response.Response.Dispose();
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // best effort to clean up ;)
            }
        }

        #endregion
    }
}
