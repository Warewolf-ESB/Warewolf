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
    public interface ICommunicationContext
    {
        ICommunicationRequest Request { get; }
        ICommunicationResponse Response { get; }

        void Send(Responses.CommunicationResponseWriter response);
    }

    internal sealed class CommunicationContext : ICommunicationContext
    {
        #region Constants
        private const string DefaultContentType = "text/html; charset=";
        const double SizeCapForDownload = 512000; // 500 KB size limit
        #endregion

        #region Instance Fields
        private ICommunicationRequest _request;
        private ICommunicationResponse _response;

        private IHttpRequest _rawRequest;
        private IHttpResponse _rawResponse;
        private IHttpSession _rawSession;
        #endregion

        #region Public Properties
        public ICommunicationRequest Request { get { return _request; } }
        public ICommunicationResponse Response { get { return _response; } }
        #endregion

        #region Constructor
        internal CommunicationContext(IHttpRequest request, IHttpResponse response, IHttpSession session, UriTemplateMatch match)
        {
            _request = new CommunicationRequest(_rawRequest = request, match);
            _response = new CommunicationResponse(_rawResponse = response);
            _rawSession = session;
        }
        #endregion

        #region Send Handling
        public void Send(Responses.CommunicationResponseWriter response)
        {
            response.Write(this);

            if (String.IsNullOrEmpty(_rawResponse.ContentType))
                _rawResponse.ContentType = DefaultContentType + _rawResponse.Encoding.WebName;

            if (_rawResponse.ContentLength > SizeCapForDownload)
            {
                
                var typeOf = _rawResponse.ContentType;

                bool canForce = false;

                if (typeOf.Equals("text/xml"))
                {
                    _rawResponse.AddHeader("Content-Disposition", "attachment; filename=Output.xml");
                    canForce = true;
                }
                else if (typeOf.Equals("application/json"))
                {
                    _rawResponse.AddHeader("Content-Disposition", "attachment; filename=Output.json");
                    canForce = true;
                }

                if (canForce)
                {
                    _rawResponse.AddHeader("Content-Type", "application/force-download");
                }
                else
                {
                    _rawResponse.AddHeader("Content-Type", _rawResponse.ContentType);
                }
            }
            else
            {
                _rawResponse.AddHeader("Content-Type", _rawResponse.ContentType);
            }
            _rawResponse.AddHeader("Content-Length", _rawResponse.ContentLength.ToString());
            _rawResponse.AddHeader("Server", "Dev2 Server");

            try
            {
                _rawResponse.Send();
            }
            finally
            {
                _request = null;
                _response = null;
                _rawRequest = null;
                _rawResponse = null;
                _rawSession = null;
            }
        }
        #endregion
    }
}
