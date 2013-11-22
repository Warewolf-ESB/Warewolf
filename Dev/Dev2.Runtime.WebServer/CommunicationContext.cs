using System;
using System.Collections.Specialized;
using Dev2.Runtime.WebServer.Responses;
using HttpFramework;
using HttpFramework.Sessions;

namespace Dev2.Runtime.WebServer
{
    public interface ICommunicationContext
    {
        ICommunicationRequest Request { get; }
        ICommunicationResponse Response { get; }

        void Send(ResponseWriter response);

        NameValueCollection FetchHeaders();
    }

    internal sealed class CommunicationContext : ICommunicationContext
    {
        private const string DefaultContentType = "text/html; charset=";
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


        /// <summary>
        /// Fetches the headers.
        /// </summary>
        /// <returns></returns>
        public NameValueCollection FetchHeaders()
        {
            return _rawRequest.Headers;
        }

        #region Send Handling
        public void Send(ResponseWriter response)
        {
            response.Write(this);

            if(String.IsNullOrEmpty(_rawResponse.ContentType))
                _rawResponse.ContentType = DefaultContentType + _rawResponse.Encoding.WebName;

            if(_rawResponse.ContentLength > WebServerStartup.SizeCapForDownload)
            {

                var typeOf = _rawResponse.ContentType;

                bool canForce = false;

                if(typeOf.Equals("text/xml"))
                {
                    _rawResponse.AddHeader("Content-Disposition", "attachment; filename=Output.xml");
                    canForce = true;
                }
                else if(typeOf.Equals("application/json"))
                {
                    _rawResponse.AddHeader("Content-Disposition", "attachment; filename=Output.json");
                    canForce = true;
                }

                if(canForce)
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
