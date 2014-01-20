using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Text;

namespace Dev2.Runtime.WebServer
{
    public class WebServerRequest : ICommunicationRequest
    {
        readonly HttpRequestMessage _request;

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
        public Stream InputStream { get { return ReadInputStream(); } }
        public NameValueCollection QueryString { get; private set; }
        public NameValueCollection BoundVariables { get; private set; }

        void InitializeContentLength()
        {
            ContentLength = (int)(_request.Content.Headers.ContentLength.HasValue ? _request.Content.Headers.ContentLength.Value : 0L);
        }

        void InitializeContentType()
        {
            ContentType = _request.Content.Headers.ContentType == null ? null : _request.Content.Headers.ContentType.MediaType;
        }

        void InitializeQueryString()
        {
            QueryString = new NameValueCollection();
            foreach(var kvp in _request.GetQueryNameValuePairs())
            {
                QueryString.Add(kvp.Key, kvp.Value);
            }
        }

        Stream ReadInputStream()
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