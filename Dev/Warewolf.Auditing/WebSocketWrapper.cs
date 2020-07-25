/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using Warewolf.Interfaces.Auditing;
using Warewolf.Interfaces.Pooling;
using Warewolf.Pooling;
using System.Collections.Concurrent;
using Dev2.Common;

namespace Warewolf.Auditing
{
    public class WebSocketPool : IWebSocketPool
    {
        private static readonly ConcurrentDictionary<Uri, IObjectPool<IWebSocketWrapper>> WebSocketConnectionPool = new ConcurrentDictionary<Uri, IObjectPool<IWebSocketWrapper>>();

        public IWebSocketWrapper Acquire(string endpoint)
        {
            var pool = WebSocketConnectionPool.GetOrAdd(new Uri(endpoint), CreateObjectPool);
            var clientWebSocket = pool.AcquireObject();

            return clientWebSocket;
        }
        public void Release(IWebSocketWrapper webSocketWrapper)
        {
            var pool = WebSocketConnectionPool.GetOrAdd(webSocketWrapper.Uri, CreateObjectPool);
            pool.ReleaseObject(webSocketWrapper);
        }

        private static IObjectPool<IWebSocketWrapper> CreateObjectPool(Uri endpoint)
        {
            IWebSocketWrapper CreateObject()
            {
                return new WebSocketWrapper(new System.Net.WebSockets.Managed.ClientWebSocket(), endpoint).Connect();
            }
            bool ValidateObject(IWebSocketWrapper instance)
            {
                return instance.State == WebSocketState.None || instance.State == WebSocketState.Open;
            }

            return new ObjectPool<IWebSocketWrapper>(CreateObject, ValidateObject);
        }
    }

    // TODO: move to Warewolf.Common and rewrite the class
    internal class WebSocketWrapper : IWebSocketWrapper
    {
        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        private readonly System.Net.WebSockets.Managed.ClientWebSocket _clientWebSocket;
        public Uri Uri { get; private set;}
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        private Action<WebSocketWrapper> _onConnected;
        private Action<string, WebSocketWrapper> _onMessage;
        private Action<WebSocketWrapper> _onDisconnected;
        private Task _messageReceiveWorker;

        public WebSocketWrapper(System.Net.WebSockets.Managed.ClientWebSocket clientWebSocket, Uri uri)
        {
            _clientWebSocket = clientWebSocket;
            Uri = uri;
            _cancellationToken = _cancellationTokenSource.Token;

            if (_clientWebSocket.State == WebSocketState.None)
            {
                _clientWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(60);
            }
        }
        public WebSocketState State => _clientWebSocket.State;

        public IWebSocketWrapper Connect()
        {
            if (!IsOpen())
            {
                var task = ConnectAsync();
                task.Wait();
                return this;
            }
            else
            {
                return this;
            }
        }

        public IWebSocketWrapper OnConnect(Action<IWebSocketWrapper> onConnect)
        {
            _onConnected = onConnect;
            return this;
        }

        public IWebSocketWrapper Close()
        {
            var task = _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", _cancellationToken);
            task.Wait();
            return this;
        }

        public IWebSocketWrapper OnDisconnect(Action<IWebSocketWrapper> onDisconnect)
        {
            _onDisconnected = onDisconnect;
            return this;
        }

        public IWebSocketWrapper OnMessage(Action<string, IWebSocketWrapper> onMessage)
        {
            _onMessage = onMessage;
            return this;
        }

        public void SendMessage(string message)
        {
            var task = SendMessageAsync(message);
            task.Wait();
        }

        public void SendMessage(byte[] message)
        {
            var task = SendMessageAsync(message);
            task.Wait();
        }

        public bool IsOpen()
        {
            return _clientWebSocket.State == WebSocketState.Open;
        }

        private async Task SendMessageAsync(string message)
        {
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            await SendMessageAsync(messageBuffer);
        }

        private async Task SendMessageAsync(byte[] messageBuffer)
        {
            var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / SendChunkSize);

            try
            {
                if (_clientWebSocket.State != WebSocketState.Open)
                {
                    throw new WebSocketException("Connection is not open.");
                }

                for (var i = 0; i < messagesCount; i++)
                {
                    var offset = (SendChunkSize * i);
                    var count = SendChunkSize;
                    var lastMessage = ((i + 1) == messagesCount);

                    if ((count * (i + 1)) > messageBuffer.Length)
                    {
                        count = messageBuffer.Length - offset;
                    }

                    await _clientWebSocket.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text, lastMessage, _cancellationToken);
                }
            }
            catch (Exception)
            {
                CallOnDisconnected();
            }
        }

        private async Task ConnectAsync()
        {
            await _clientWebSocket.ConnectAsync(Uri, _cancellationToken);
            CallOnConnected();
            _messageReceiveWorker = WatchForMessagesFromServer();
        }

        private async Task WatchForMessagesFromServer()
        {
            var buffer = new byte[ReceiveChunkSize];

            try
            {
                while (_clientWebSocket.State == WebSocketState.Open)
                {
                    var stringResult = new StringBuilder();


                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                            CallOnDisconnected();
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            stringResult.Append(str);
                        }

                    } while (!result.EndOfMessage);

                    CallOnMessage(stringResult);

                }
            }
            catch (Exception)
            {
                CallOnDisconnected();
            }
            finally
            {
                _clientWebSocket.Dispose();
            }
        }
        private void CallOnMessage(StringBuilder stringResult)
        {
            if (_onMessage != null)
            {
                RunInTask(() => _onMessage(stringResult.ToString(), this));
            }
        }

        private void CallOnDisconnected()
        {
            if (_onDisconnected != null)
            {
                RunInTask(() => _onDisconnected(this));
            }
        }

        private void CallOnConnected()
        {
            if (_onConnected != null)
            {
                RunInTask(() => _onConnected(this));
            }
        }

        private static void RunInTask(Action action)
        {
            Task.Factory.StartNew(action);
        }

        private bool _isDisposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (!_messageReceiveWorker.IsCompleted)
                    {
                        Dev2Logger.Warn("websocket message receive worker still running is disposing WebSocketWrapper", Guid.Empty.ToString());
                    }
                    if (IsOpen())
                    {
                        _cancellationTokenSource.Cancel();
                        _clientWebSocket.Dispose();
                    }
                }

                _isDisposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
