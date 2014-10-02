
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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HttpFramework.Exceptions;
using HttpFramework.Parser;

namespace HttpFramework
{
    /// <summary>
    /// Contains a connection to a browser/client.
    /// </summary>
    /// <remarks>
    /// Remember to <see cref="Start"/> after you have hooked the <see cref="RequestReceived"/> event.
    /// </remarks>
    /// TODO: Maybe this class should be broken up into HttpClientChannel and HttpClientContext?
    public class HttpClientContext : IHttpClientContext ,IDisposable
    {
        private readonly byte[] _buffer;
        private int _bytesLeft;
        private ILogWriter _log;
        private readonly IHttpRequestParser _parser;
        private readonly int _bufferSize;
        private IHttpRequest _currentRequest;
        private ClientCertificate _clientCertificate;
		private IPEndPoint _localEndPoint;

        /// <summary>
        /// Gets or sets whether the context is available for re use.
        /// </summary>
        public bool IsAvailable { get; set; }

		/// <summary>
		/// This context have been cleaned, which means that it can be reused.
		/// </summary>
    	public event EventHandler Cleaned = delegate { };

		/// <summary>
		/// Context have been started (a new client have connected)
		/// </summary>
    	public event EventHandler Started = delegate { };

        /// <summary>
		/// Initializes a new instance of the <see cref="HttpClientContext"/> class.
        /// </summary>
        /// <param name="secured">true if the connection is secured (SSL/TLS)</param>
        /// <param name="remoteEndPoint">client that connected.</param>
        /// <param name="stream">Stream used for communication</param>
        /// <param name="clientCertificate">Client security certificate</param>
        /// <param name="parserFactory">Used to create a <see cref="IHttpRequestParser"/>.</param>
        /// <param name="bufferSize">Size of buffer to use when reading data. Must be at least 4096 bytes.</param>
        /// <param name="socket">Client socket</param>
        /// <exception cref="SocketException">If <see cref="Socket.BeginReceive(byte[],int,int,SocketFlags,AsyncCallback,object)"/> fails</exception>
        /// <exception cref="ArgumentException">Stream must be writable and readable.</exception>
        public HttpClientContext(bool secured, IPEndPoint remoteEndPoint, Stream stream, 
            ClientCertificate clientCertificate, IRequestParserFactory parserFactory, int bufferSize, Socket socket)
        {
            Check.Require(remoteEndPoint, "remoteEndPoint");
            Check.NotEmpty(remoteEndPoint.Address.ToString(), "remoteEndPoint.Address");
            Check.Require(stream, "stream");
            Check.Require(parserFactory, "parser");
            Check.Min(4096, bufferSize, "bufferSize");
            Check.Require(socket, "socket");

            if (!stream.CanWrite || !stream.CanRead)
                throw new ArgumentException("Stream must be writable and readable.");

            _bufferSize = bufferSize;
			RemoteAddress = remoteEndPoint.Address.ToString();
			RemotePort = remoteEndPoint.Port.ToString();
            _log = NullLogWriter.Instance;
            _parser = parserFactory.CreateParser(_log);
            _parser.RequestCompleted += OnRequestCompleted;
            _parser.RequestLineReceived += OnRequestLine;
            _parser.HeaderReceived += OnHeaderReceived;
            _parser.BodyBytesReceived += OnBodyBytesReceived;
        	_localEndPoint = (IPEndPoint)socket.LocalEndPoint;

            HttpRequest request = new HttpRequest();
            request._remoteEndPoint = remoteEndPoint;
            request.Secure = secured;
            _currentRequest = request;

            IsSecured = secured;
            _stream = stream;
            _clientCertificate = clientCertificate;
            _buffer = new byte[bufferSize];

        }

        /// <summary>
        /// Set this to <c>false</c> to keep the connection alive after the response
        /// has been sent, even if <see cref="ConnectionType.Close"/> is used
        /// </summary>
        public bool EndWhenDone { get; set; }

        /// <summary>
        /// Process incoming body bytes.
        /// </summary>
        /// <param name="sender"><see cref="IHttpRequestParser"/></param>
        /// <param name="e">Bytes</param>
        protected virtual void OnBodyBytesReceived(object sender, BodyEventArgs e)
        {
            _currentRequest.AddToBody(e.Buffer, e.Offset, e.Count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnHeaderReceived(object sender, HeaderEventArgs e)
        {
            if (string.Compare(e.Name, "expect", true) == 0 && e.Value.Contains("100-continue"))
            {
                Respond("HTTP/1.0", HttpStatusCode.Continue, "Please continue mate.");
            }

            _currentRequest.AddHeader(e.Name, e.Value);
        }

        private void OnRequestLine(object sender, RequestLineEventArgs e)
        {
            _currentRequest.Method = e.HttpMethod;
            _currentRequest.HttpVersion = e.HttpVersion;
            _currentRequest.UriPath = e.UriPath;

            // Initialize _currentRequest.Uri to a synthesized path. This will be the final value if the host header is not sent
            string authority = _localEndPoint.Address.ToString();
            if (_localEndPoint.Port != 80) authority += ":" + _localEndPoint.Port;
            _currentRequest.Uri = new Uri((IsSecured ? "https://" : "http://") + authority + e.UriPath);
        }

        /// <summary>
        /// Overload to specify own type.
        /// </summary>
        /// <remarks>
        /// Must be specified before the context is being used.
        /// </remarks>
        protected IHttpRequest CurrentRequest
        { get { return _currentRequest; } set { _currentRequest = value; } }

        /// <summary>
        /// Start reading content.
        /// </summary>
        /// <remarks>
        /// Make sure to call base.Start() if you override this method.
        /// </remarks>
        public virtual void Start()
        {
            try
            {
                _stream.BeginRead(_buffer, 0, _bufferSize, OnReceive, null);
            }
            catch (IOException err)
            {
                LogWriter.Write(this, LogPrio.Debug, err.ToString());
            }

        	Started(this, EventArgs.Empty);
        }

        /// <summary>
        /// Clean up context.
        /// </summary>
        /// <remarks>
        /// Make sure to call base.Cleanup() if you override the method.
        /// </remarks>
        public virtual void Cleanup()
        {
        	if (Stream == null) 
				return;
            
        	Stream.Dispose();
        	Stream = null;
        	_currentRequest.Clear();
        	_bytesLeft = 0;
        	Cleaned(this, EventArgs.Empty);
        	_parser.Clear();
        }

        /// <summary>
        /// Close this context
        /// </summary>
        public void Close()
        {
            Cleanup();
            IsAvailable = true;
        }

        /// <summary>
        /// Using SSL or other encryption method.
        /// </summary>
        [Obsolete("Use IsSecured instead.")]
        public bool Secured
        {
            get { return IsSecured; }
        }

        /// <summary>
        /// Using SSL or other encryption method.
        /// </summary>
        public bool IsSecured { get; internal set; }

        /// <summary>
        /// Specify which logger to use.
        /// </summary>
        public ILogWriter LogWriter
        {
            get { return _log; }
            set 
			{ 
				_log = value ?? NullLogWriter.Instance;
				_parser.LogWriter = _log;
			}
        }

        private Stream _stream;

        /// <summary>
        /// Gets or sets the network stream.
        /// </summary>
        internal Stream Stream
        {
            get { return _stream; }
            private set { _stream = value; }
        }

        /// <summary>
        /// Assign a stream and local endpoint to this client context
        /// </summary>
        /// <param name="stream">Stream to assign to this context</param>
        /// <param name="localEndPoint">Local endpoint to assign to this context</param>
        public void Assign(Stream stream, IPEndPoint localEndPoint)
		{
			Stream = stream;
			_localEndPoint = localEndPoint;
		}

        /// <summary>
        /// Gets the client's security certificate.
        /// </summary>
        public ClientCertificate ClientCertificate
        {
            get { return _clientCertificate; }
        }

        /// <summary>
        /// Gets or sets IP address that the client connected from.
        /// </summary>
        internal string RemoteAddress { get; set; }

        /// <summary>
        /// Gets or sets port that the client connected from.
        /// </summary>
        internal string RemotePort { get; set; }

        /// <summary>
        /// Disconnect from client
        /// </summary>
        /// <param name="error">error to report in the <see cref="Disconnected"/> event.</param>
        public void Disconnect(SocketError error)
        {
            // disconnect may not throw any exceptions
            try
            {
                //_sock.Disconnect(true);
                if (error == SocketError.Success)
                {
                    if (Stream is ReusableSocketNetworkStream)
                        Stream.Flush();
                }
                //Stream.Close();
                Disconnected(this, new DisconnectedEventArgs(error));
            }
            catch (Exception err)
            {
                LogWriter.Write(this, LogPrio.Error, "Disconnect threw an exception: " + err);
            }
        }

        /// <exception cref="BadRequestException"><c>BadRequestException</c>.</exception>
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int bytesRead = Stream.EndRead(ar);
                if (bytesRead == 0)
                {
                    Disconnect(SocketError.ConnectionReset);
                    return;
                }
                _bytesLeft += bytesRead;
                if (_bytesLeft > _buffer.Length)
                {
#if DEBUG
                    throw new BadRequestException("Too large HTTP header: " + Encoding.UTF8.GetString(_buffer, 0, bytesRead));
#else
                    throw new BadRequestException("Too large HTTP header: " + _bytesLeft);
#endif
                }

#if DEBUG
#pragma warning disable 219
                string temp = Encoding.ASCII.GetString(_buffer, 0, _bytesLeft);
                LogWriter.Write(this, LogPrio.Trace, "Received: " + temp);
#pragma warning restore 219
#endif
                int offset = _parser.Parse(_buffer, 0, _bytesLeft);
                if (Stream == null)
                    return; // "Connection: Close" in effect.

                // try again to see if we can parse another message (check parser to see if it is looking for a new message)
                int oldOffset = offset;
                while (_parser.CurrentState == RequestParserState.FirstLine && offset != 0 && _bytesLeft - offset > 0)
                {
#if DEBUG
                    temp = Encoding.ASCII.GetString(_buffer, offset, _bytesLeft - offset);
                    LogWriter.Write(this, LogPrio.Trace, "Processing: " + temp);
#endif
                    offset = _parser.Parse(_buffer, offset, _bytesLeft - offset);
                    if (Stream == null)
                        return; // "Connection: Close" in effect.
                }

                // need to be able to move prev bytes, so restore offset.
                if (offset == 0)
                    offset = oldOffset;

                // workaround until the source of the bug can be identified.
                if (_bytesLeft < offset)
                {
                    _log.Write(this, LogPrio.Error, "Have a bug where _bytesLeft is less then offset, trying to fix. BytsLeft: " + _bytesLeft + ", offset: " + offset);
                    _bytesLeft = offset;
                }

                // copy unused bytes to the beginning of the array
                if (offset > 0 && _bytesLeft != offset)
                    Buffer.BlockCopy(_buffer, offset, _buffer, 0, _bytesLeft - offset);

                _bytesLeft -= offset;
                if (Stream != null && Stream.CanRead)
                    Stream.BeginRead(_buffer, _bytesLeft, _buffer.Length - _bytesLeft, OnReceive, null);
                else
                {
                    _log.Write(this, LogPrio.Warning, "Could not read any more from the socket.");
                    Disconnect(SocketError.Success);
                }
            }
            catch (BadRequestException err)
            {
                LogWriter.Write(this, LogPrio.Warning, "Bad request, responding with it. Error: " + err);
                try
                {
                    Respond("HTTP/1.0", HttpStatusCode.BadRequest, err.Message);
                }
                catch (Exception err2)
                {
                    LogWriter.Write(this, LogPrio.Fatal, "Failed to reply to a bad request. " + err2);
                }
                Disconnect(SocketError.NoRecovery);
            }
            catch (IOException err)
            {
                LogWriter.Write(this, LogPrio.Debug, "Failed to end receive: " + err.Message);
                if (err.InnerException is SocketException)
                    Disconnect((SocketError)((SocketException)err.InnerException).ErrorCode);
                else
                    Disconnect(SocketError.ConnectionReset);
            }
            catch (ObjectDisposedException err)
            {
                LogWriter.Write(this, LogPrio.Debug, "Failed to end receive : " + err.Message);
                Disconnect(SocketError.NotSocket);
            }
            catch (NullReferenceException err)
            {
                LogWriter.Write(this, LogPrio.Debug, "Failed to end receive : NullRef: " + err.Message);
                Disconnect(SocketError.NoRecovery);
            }
            catch (Exception err)
            {
                LogWriter.Write(this, LogPrio.Warning, "Bad request, responding with it. Error: " + err);
                try
                    { Respond("HTTP/1.0", HttpStatusCode.BadRequest, err.Message); }
                catch (Exception err2)
                    { LogWriter.Write(this, LogPrio.Error, "Failed to reply to a bad request. " + err2); }
                Disconnect(SocketError.NoRecovery);
            }
        }

        private void OnRequestCompleted(object source, EventArgs args)
        {
            _currentRequest.AddHeader("remote_addr", RemoteAddress);
            _currentRequest.AddHeader("remote_port", RemotePort);
            _currentRequest.Body.Seek(0, SeekOrigin.Begin);
            RequestReceived(this, new RequestEventArgs(_currentRequest));
			_currentRequest.Clear();
        }

        /// <summary>
        /// Send a response.
        /// </summary>
        /// <param name="httpVersion">Either <see cref="HttpHelper.HTTP10"/> or <see cref="HttpHelper.HTTP11"/></param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="reason">reason for the status code.</param>
        /// <param name="body">HTML body contents, can be null or empty.</param>
        /// <param name="contentType">A content type to return the body as, i.e. 'text/html' or 'text/plain', defaults to 'text/html' if null or empty</param>
        /// <exception cref="ArgumentException">If <paramref name="httpVersion"/> is invalid.</exception>
        public void Respond(string httpVersion, HttpStatusCode statusCode, string reason, string body, string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                contentType = "text/html";

            if (string.IsNullOrEmpty(httpVersion) || !httpVersion.StartsWith("HTTP/1"))
                throw new ArgumentException("Invalid HTTP version");

            if (string.IsNullOrEmpty(reason))
                reason = statusCode.ToString();

            string response = string.IsNullOrEmpty(body)
                                  ? httpVersion + " " + (int) statusCode + " " + reason + "\r\n\r\n"
                                  : string.Format("{0} {1} {2}\r\nContent-Type: {5}\r\nContent-Length: {3}\r\n\r\n{4}",
                                                  httpVersion, (int) statusCode, reason ?? statusCode.ToString(),
                                                  body.Length, body, contentType);
            byte[] buffer = Encoding.ASCII.GetBytes(response);

            Send(buffer);
        }

        /// <summary>
        /// Send a response.
        /// </summary>
        /// <param name="httpVersion">Either <see cref="HttpHelper.HTTP10"/> or <see cref="HttpHelper.HTTP11"/></param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="reason">reason for the status code.</param>
        public void Respond(string httpVersion, HttpStatusCode statusCode, string reason)
        {
            Respond(httpVersion, statusCode, reason, null, null);
        }

        /// <summary>
        /// Send a response.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void Respond(string body)
        {
            if (body == null) 
                throw new ArgumentNullException("body");
            Respond("HTTP/1.1", HttpStatusCode.OK, HttpStatusCode.OK.ToString(), body, null);
        }

        /// <summary>
        /// send a whole buffer
        /// </summary>
        /// <param name="buffer">buffer to send</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Send(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            Send(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Send data using the stream
        /// </summary>
        /// <param name="buffer">Contains data to send</param>
        /// <param name="offset">Start position in buffer</param>
        /// <param name="size">number of bytes to send</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Send(byte[] buffer, int offset, int size)
        {
            
            if (offset + size > buffer.Length)
                throw new ArgumentOutOfRangeException("offset", offset, "offset + size is beyond end of buffer.");

            if (Stream != null && Stream.CanWrite)
            {
                try
                {
                    Stream.Write(buffer, offset, size);
                } 
                catch (IOException)
                {
                       
                }
            }

        }

        /// <summary>
        /// The context have been disconnected.
        /// </summary>
        /// <remarks>
        /// Event can be used to clean up a context, or to reuse it.
        /// </remarks>
        public event EventHandler<DisconnectedEventArgs> Disconnected = delegate{};
        /// <summary>
        /// A request have been received in the context.
        /// </summary>
        public event EventHandler<RequestEventArgs> RequestReceived = delegate{};

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool unmanaged)
        {
            if (unmanaged)
            {
                if (Stream != null)
                {
                    try
                    {
                        if (Stream.CanWrite)
                            Stream.Flush();

                        Cleanup();

                    }
                    catch (IOException)
                    {

                    }
                }
               
            }
            
        }

        ~HttpClientContext()
        {
            Dispose();
        }
    }
}
