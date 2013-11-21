using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Dev2.Common;
using HttpFramework;

namespace Unlimited.Applications.WebServer
{
    public interface ICommunicationRequest
    {
        string Method { get; }
        Uri Uri { get; }

        int ContentLength { get; }
        string ContentType { get; }
        Encoding ContentEncoding { get; }
        Stream InputStream { get; }

        NameValueCollection QueryString { get; }
        NameValueCollection BoundVariables { get; }
    }

    internal sealed class CommunicationRequest : ICommunicationRequest
    {
        private IHttpRequest _request;
        private UriTemplateMatch _match;
        private NameValueCollection _boundVariables;
        private string _contentType;
        private Encoding _contentEncoding;
        private bool _resetBody;
        readonly NameValueCollection _queryString;

        public string Method { get { return _request.Method; } }
        public Uri Uri { get { return _request.Uri; } }
        public int ContentLength { get { return _request.ContentLength; } }
        public string ContentType { get { return _contentType; } }
        public Encoding ContentEncoding { get { return _contentEncoding; } }
        public Stream InputStream { get { if (!_resetBody) { _resetBody = true; _request.Body.Position = 0L; } return _request.Body; } }
        public NameValueCollection QueryString { get { return _queryString; } }
        public NameValueCollection BoundVariables { get { return _boundVariables ?? (_boundVariables = _match.BoundVariables); } }

        internal CommunicationRequest(IHttpRequest request, UriTemplateMatch match)
        {
            _request = request;
            _match = match;

            _contentType = _request.Headers["content-type"];

            string rawContentEncoding = _request.Headers["content-encoding"];

            if (!String.IsNullOrEmpty(rawContentEncoding))
            {
                try
                {
                    _contentEncoding = Encoding.GetEncoding(rawContentEncoding);
                }
                catch(Exception ex)
                {
                    ServerLogger.LogError(ex);
                    _contentEncoding = null;
                }
            }

            if (_contentEncoding == null) _contentEncoding = Encoding.UTF8;

            _queryString = new NameValueCollection();
            if(request.QueryString != HttpInput.Empty)
            {
                foreach(HttpInputItem item in request.QueryString)
                {
                    _queryString.Add(item.Name, item.Value);
                }
            }
        }
    }
}
