
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
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace HttpFramework
{
    /// <summary>
    /// Contains a connection to a browser/client.
    /// </summary>
    public interface IHttpClientContext
    {
        /// <summary>
        /// Using SSL or other encryption method.
        /// </summary>
        [Obsolete("Use IsSecured instead.")]
        bool Secured { get; }

        /// <summary>
        /// Using SSL or other encryption method.
        /// </summary>
        bool IsSecured { get; }

        /// <summary>
        /// Gets the client's security certificate.
        /// </summary>
        ClientCertificate ClientCertificate { get; }

        /// <summary>
        /// Disconnect from client
        /// </summary>
        /// <param name="error">error to report in the <see cref="Disconnected"/> event.</param>
        void Disconnect(SocketError error);

        /// <summary>
        /// Send a response.
        /// </summary>
        /// <param name="httpVersion">Either <see cref="HttpHelper.HTTP10"/> or <see cref="HttpHelper.HTTP11"/></param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="reason">reason for the status code.</param>
        /// <param name="body">HTML body contents, can be null or empty.</param>
        /// <param name="contentType">A content type to return the body as, i.e. 'text/html' or 'text/plain', defaults to 'text/html' if null or empty</param>
        /// <exception cref="ArgumentException">If <paramref name="httpVersion"/> is invalid.</exception>
        void Respond(string httpVersion, HttpStatusCode statusCode, string reason, string body, string contentType);

        /// <summary>
        /// Send a response.
        /// </summary>
        /// <param name="httpVersion">Either <see cref="HttpHelper.HTTP10"/> or <see cref="HttpHelper.HTTP11"/></param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="reason">reason for the status code.</param>
        void Respond(string httpVersion, HttpStatusCode statusCode, string reason);

        /// <summary>
        /// Send a response.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        void Respond(string body);

        /// <summary>
        /// send a whole buffer
        /// </summary>
        /// <param name="buffer">buffer to send</param>
        /// <exception cref="ArgumentNullException"></exception>
        void Send(byte[] buffer);

        /// <summary>
        /// Send data using the stream
        /// </summary>
        /// <param name="buffer">Contains data to send</param>
        /// <param name="offset">Start position in buffer</param>
        /// <param name="size">number of bytes to send</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void Send(byte[] buffer, int offset, int size);

        /// <summary>
        /// Closes the streams and disposes of the unmanaged resources
        /// </summary>
        void Close();

        /// <summary>
        /// Set this to false to keep the connection alive after the response
        /// has been sent, even if ConnectionType.Close is used
        /// </summary>
        bool EndWhenDone { get; set; }

        /// <summary>
        /// The context have been disconnected.
        /// </summary>
        /// <remarks>
        /// Event can be used to clean up a context, or to reuse it.
        /// </remarks>
        event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        /// A request have been received in the context.
        /// </summary>
        event EventHandler<RequestEventArgs> RequestReceived;
    }

    /// <summary>
    /// A <see cref="IHttpClientContext"/> have been disconnected.
    /// </summary>
    public class DisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets reason to why client disconnected.
        /// </summary>
        public SocketError Error { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectedEventArgs"/> class.
        /// </summary>
        /// <param name="error">Reason to disconnection.</param>
        public DisconnectedEventArgs(SocketError error)
        {
            Check.Require(error, "error");

            Error = error;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RequestEventArgs : EventArgs
    {
        /// <summary>
        /// Gets received request.
        /// </summary>
        public IHttpRequest Request { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestEventArgs"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        public RequestEventArgs(IHttpRequest request)
        {
            Check.Require(request, "request");

            Request = request;
        }
    }

    /// <summary>
    /// Client X.509 certificate, X.509 chain, and any SSL policy errors encountered
    /// during the SSL stream creation
    /// </summary>
    public class ClientCertificate
    {
        /// <summary>
        /// Client security certificate
        /// </summary>
        public readonly X509Certificate Certificate;

        /// <summary>
        /// Client security certificate chain
        /// </summary>
        public readonly X509Chain Chain;

        /// <summary>
        /// Any SSL policy errors encountered during the SSL stream creation
        /// </summary>
        public readonly SslPolicyErrors SslPolicyErrors;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="certificate">Certificate</param>
        /// <param name="chain">Certificate chain</param>
        /// <param name="sslPolicyErrors">SSL policy errors</param>
        public ClientCertificate(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Certificate = certificate;
            Chain = chain;
            SslPolicyErrors = sslPolicyErrors;
        }
    }
}
