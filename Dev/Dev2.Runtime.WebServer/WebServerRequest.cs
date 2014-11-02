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
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Runtime.WebServer
{
    public class WebServerRequest : ICommunicationRequest
    {
        private readonly HttpRequestMessage _request;

        public WebServerRequest(HttpRequestMessage request, NameValueCollection boundVariables)
        {
            VerifyArgument.IsNotNull("request", request);
            VerifyArgument.IsNotNull("boundVariables", boundVariables);
            _request = request;

            Method = _request.Method.ToString();
            Uri = _request.RequestUri;
            BoundVariables = boundVariables;
            ContentEncoding = _request.Content.GetContentEncoding();

            InitializeContentLength();
            InitializeContentType();
            InitializeQueryString();
        }

        public string Method { get; private set; }
        public Uri Uri { get; private set; }
        public IPrincipal User { get; set; }
        public int ContentLength { get; private set; }
        public string ContentType { get; private set; }
        public Encoding ContentEncoding { get; private set; }

        public Stream InputStream
        {
            get { return ReadInputStream(); }
        }

        public NameValueCollection QueryString { get; private set; }
        public NameValueCollection BoundVariables { get; private set; }

        private void InitializeContentLength()
        {
            ContentLength =
                (int)
                    (_request.Content.Headers.ContentLength.HasValue ? _request.Content.Headers.ContentLength.Value : 0L);
        }

        private void InitializeContentType()
        {
            ContentType = _request.Content.Headers.ContentType == null
                ? null
                : _request.Content.Headers.ContentType.MediaType;
        }

        private void InitializeQueryString()
        {
            QueryString = new NameValueCollection();
            foreach (var kvp in _request.GetQueryNameValuePairs())
            {
                QueryString.Add(kvp.Key, kvp.Value);
            }
        }

        private Stream ReadInputStream()
        {
            if (_request.Content == null)
            {
                return null;
            }
            Task<Stream> task = _request.Content.ReadAsStreamAsync();
            task.Wait();
            return task.Result;
        }
    }
}