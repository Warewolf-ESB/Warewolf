using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using HttpFramework;
using HttpFramework.Sessions;

namespace Unlimited.Applications.WebServer
{
    public interface ICommunicationResponse
    {
        HttpStatusCode Status { get; set; }
        string Reason { get; set; }
        string ContentType { get; set; }
        long ContentLength { get; set; }
        Encoding Encoding { get; set; }
        Stream OutputStream { get; }

        void AddHeader(string name, string value);
    }

    internal class CommunicationResponse : ICommunicationResponse
    {
        private IHttpResponse _response;
        private bool _resetBody;

        public HttpStatusCode Status { get { return _response.Status; } set { _response.Status = value; } }
        public string Reason { get { return _response.Reason; } set { _response.Reason = value; } }
        public string ContentType { get { return _response.ContentType; } set { _response.ContentType = value; } }
        public long ContentLength { get { return _response.ContentLength; } set { _response.ContentLength = value; } }
        public Encoding Encoding { get { return _response.Encoding; } set { _response.Encoding = value; } }
        public Stream OutputStream { get { if (!_resetBody) { _resetBody = true; _response.Body.Position = 0L; } return _response.Body; } }

        public CommunicationResponse(IHttpResponse response)
        {
            _response = response;
        }

        public void AddHeader(string name, string value)
        {
            if (String.Equals(name, "content-type", StringComparison.OrdinalIgnoreCase))
                _response.ContentType = value;
            else if (String.Equals(name, "content-length", StringComparison.OrdinalIgnoreCase))
                _response.ContentLength = Int64.Parse(value);
            else
            {
                _response.AddHeader(name, value);
            }
        }
    }
}
