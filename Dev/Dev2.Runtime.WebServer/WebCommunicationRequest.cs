using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dev2.Common;
using Unlimited.Applications.WebServer;

namespace Dev2.Runtime.WebServer
{
    public class WebCommunicationRequest : ICommunicationRequest
    {
        readonly HttpRequestMessage _request;

        public WebCommunicationRequest(HttpRequestMessage request, NameValueCollection boundVariables)
        {
            VerifyArgument.IsNotNull("request", request);
            VerifyArgument.IsNotNull("boundVariables", boundVariables);
            _request = request;

            Method = _request.Method.ToString();
            Uri = _request.RequestUri;
            BoundVariables = boundVariables;

            InitializeContentLength();
            InitializeContentType();
            InitializeEncoding();
            InitializeQueryString();
        }

        public string Method { get; private set; }
        public Uri Uri { get; private set; }
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
            ContentType = _request.Content.Headers.ContentType.MediaType;
        }

        void InitializeEncoding()
        {
            var encoding = _request.Content.Headers.ContentEncoding.FirstOrDefault();
            if(!string.IsNullOrEmpty(encoding))
            {
                try
                {
                    ContentEncoding = Encoding.GetEncoding(encoding);
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError(ex);
                    encoding = null;
                }
            }
            if(encoding == null)
            {
                ContentEncoding = Encoding.UTF8;
            }
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