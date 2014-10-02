
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
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace HttpFramework
{
    /// <summary>
    /// New implementation of the HTTP listener.
    /// </summary>
    /// <remarks>
    /// Use the <c>Create</c> methods to create a default listener.
    /// </remarks>
    public class HttpListener : HttpListenerBase
    {
        /// <summary>
        /// A client have been accepted, but not handled, by the listener.
        /// </summary>
        public event EventHandler<ClientAcceptedEventArgs> Accepted = delegate{};


        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListener"/> class.
        /// </summary>
        /// <param name="address">IP Address to accept connections on</param>
        /// <param name="port">TCP Port to listen on, default HTTP port is 80.</param>
        /// <param name="factory">Factory used to create <see cref="IHttpClientContext"/>es.</param>
        /// <exception cref="ArgumentNullException"><c>address</c> is null.</exception>
        /// <exception cref="ArgumentException">Port must be a positive number.</exception>
        public HttpListener(IPAddress address, int port, IHttpContextFactory factory)
            : base(address, port, factory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListener"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="certificate">The certificate.</param>
        public HttpListener(IPAddress address, int port, IHttpContextFactory factory, X509Certificate certificate)
            : base(address, port, factory, certificate, SslProtocols.Default, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListener"/> class.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="certificate">The certificate.</param>
        /// <param name="clientCertCallback">Callback to validate SSL client certificates.</param>
        /// <param name="protocol">The protocol.</param>
        /// <param name="requireClientCerts">True if client SSL certificates are required, otherwise false.</param>
        public HttpListener(IPAddress address, int port, IHttpContextFactory factory, X509Certificate certificate,
            RemoteCertificateValidationCallback clientCertCallback, SslProtocols protocol, bool requireClientCerts)
            : base(address, port, factory, certificate, protocol, requireClientCerts)
        {
        }

        /// <summary>
        /// Creates a new <see cref="HttpListener"/> instance with default factories.
        /// </summary>
        /// <param name="address">Address that the listener should accept connections on.</param>
        /// <param name="port">Port that listener should accept connections on.</param>
		/// <returns>Created HTTP listener.</returns>
        public static HttpListener Create(IPAddress address, int port)
        {
            RequestParserFactory requestFactory = new RequestParserFactory();
            HttpContextFactory factory = new HttpContextFactory(NullLogWriter.Instance, 16384, requestFactory);
            return new HttpListener(address, port, factory);
        }

        /// <summary>
        /// Creates a new <see cref="HttpListener"/> instance with default factories.
        /// </summary>
        /// <param name="address">Address that the listener should accept connections on.</param>
        /// <param name="port">Port that listener should accept connections on.</param>
        /// <param name="certificate">Certificate to use</param>
		/// <returns>Created HTTP listener.</returns>
        public static HttpListener Create(IPAddress address, int port, X509Certificate certificate)
        {
            RequestParserFactory requestFactory = new RequestParserFactory();
            HttpContextFactory factory = new HttpContextFactory(NullLogWriter.Instance, 16384, requestFactory);
            return new HttpListener(address, port, factory, certificate);
        }

        /// <summary>
        /// Creates a new <see cref="HttpListener"/> instance with default factories.
        /// </summary>
        /// <param name="address">Address that the listener should accept connections on.</param>
        /// <param name="port">Port that listener should accept connections on.</param>
        /// <param name="certificate">Certificate to use</param>
        /// <param name="clientCertCallback">Callback to validate SSL client certificates.</param>
        /// <param name="protocol">which HTTPS protocol to use, default is TLS.</param>
        /// <param name="requireClientCerts">True if client SSL certificates are required, otherwise false.</param>
        /// <returns>Created HTTP listener.</returns>
        public static HttpListener Create(IPAddress address, int port, X509Certificate certificate,
            RemoteCertificateValidationCallback clientCertCallback, SslProtocols protocol, bool requireClientCerts)
        {
            RequestParserFactory requestFactory = new RequestParserFactory();
            HttpContextFactory factory = new HttpContextFactory(NullLogWriter.Instance, 16384, requestFactory);
            return new HttpListener(address, port, factory, certificate, clientCertCallback, protocol, requireClientCerts);
        }

        /// <summary>
        /// Can be used to create filtering of new connections.
        /// </summary>
        /// <param name="socket">Accepted socket</param>
        /// <returns>
        /// true if connection can be accepted; otherwise false.
        /// </returns>
        protected override bool OnAcceptingSocket(Socket socket)
        {
            ClientAcceptedEventArgs args = new ClientAcceptedEventArgs(socket);
            Accepted(this, args);
            return !args.Revoked;
        }
    }
}
