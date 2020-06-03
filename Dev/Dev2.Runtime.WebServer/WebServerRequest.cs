/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using System.Text;

namespace Dev2.Runtime.WebServer
{
    public class WebServerRequest : ICommunicationRequest
    {
        private readonly HttpRequestMessage _request;

        public WebServerRequest(HttpRequestMessage request, NameValueCollection boundVariables)
        {
            VerifyArgument.IsNotNull(nameof(request), request);
            VerifyArgument.IsNotNull(nameof(boundVariables), boundVariables);
            _request = request;

            Method = _request.Method.ToString();
            Uri = _request.RequestUri;
            BoundVariables = boundVariables;
            ContentEncoding = _request.Content.GetContentEncoding();

            InitializeContentLength();
            InitializeContentType();
            InitializeQueryString();
        } 
        
        public WebServerRequest(HttpRequestMessage request)
        {
            VerifyArgument.IsNotNull(nameof(request), request);
            _request = request;

            Method = _request.Method.ToString();
            Uri = _request.RequestUri;
            ContentEncoding = _request.Content.GetContentEncoding();
            Headers = request.Headers;

            InitializeContentLength();
            InitializeContentType();
            InitializeQueryString();
        }

        public string Method { get; }
        public Uri Uri { get; }
        public IPrincipal User { get; set; }
        public int ContentLength { get; private set; }
        public string ContentType { get; private set; }
        public Encoding ContentEncoding { get; }
        public Stream InputStream => ReadInputStream();
        public NameValueCollection QueryString { get; private set; }
        public NameValueCollection BoundVariables { get; }
        public HttpRequestHeaders Headers { get; }
        public bool IsTokenAuthentication => !string.IsNullOrEmpty(Headers?.Authorization?.Parameter);

        private void InitializeContentLength()
        {
            ContentLength = (int)(_request.Content.Headers.ContentLength ?? 0L);
        }

        private void InitializeContentType()
        {
            ContentType = _request.Content.Headers.ContentType?.MediaType;
        }

        private void InitializeQueryString()
        {
            QueryString = new NameValueCollection();
            foreach(var kvp in _request.GetQueryNameValuePairs())
            {
                QueryString.Add(kvp.Key, kvp.Value);
            }
        }

        private Stream ReadInputStream()
        {
            if(_request.Content == null)
            {
                return null;
            }
            var task = _request.Content.ReadAsStreamAsync();
            task.Wait();
            return task.Result;
        }
    }
}
