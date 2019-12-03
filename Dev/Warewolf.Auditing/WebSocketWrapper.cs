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
using Dev2.Common;
using Warewolf.Interfaces.Pooling;
using Warewolf.Pooling;
using System.Collections.Concurrent;

namespace Warewolf.Auditing
{
    public class WebSocketFactory : IWebSocketFactory
    {
        private static readonly ConcurrentDictionary<string, System.Net.WebSockets.Managed.ClientWebSocket> WebSocketConnectionPool = new ConcurrentDictionary<string, System.Net.WebSockets.Managed.ClientWebSocket>();

        public IWebSocketWrapper New()
        {
            var clientWebSocket = WebSocketConnectionPool.GetOrAdd(Config.Auditing.Endpoint, new System.Net.WebSockets.Managed.ClientWebSocket());

            if (clientWebSocket.State == WebSocketState.None || clientWebSocket.State == WebSocketState.Open)
            {
                return new WebSocketWrapper(clientWebSocket, Config.Auditing.Endpoint);
            }
            else
            {
                WebSocketConnectionPool.TryRemove(Config.Auditing.Endpoint, out System.Net.WebSockets.Managed.ClientWebSocket removeClientwebSocket);

                var newClientWebSocket = WebSocketConnectionPool.GetOrAdd(Config.Auditing.Endpoint, new System.Net.WebSockets.Managed.ClientWebSocket());
                return new WebSocketWrapper(newClientWebSocket, Config.Auditing.Endpoint);
            }
        }
    }

    // TODO: move to Warewolf.Common and rewrite the class
    internal class WebSocketWrapper : IWebSocketWrapper
    {
        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        private readonly System.Net.WebSockets.Managed.ClientWebSocket _clientWebSocket;
        private readonly Uri _uri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        private Action<WebSocketWrapper> _onConnected;
        private Action<string, WebSocketWrapper> _onMessage;
        private Action<WebSocketWrapper> _onDisconnected;

        public WebSocketWrapper(System.Net.WebSockets.Managed.ClientWebSocket clientWebSocket, string uri)
        {
            _clientWebSocket = clientWebSocket;
            _uri = new Uri(uri);
            _cancellationToken = _cancellationTokenSource.Token;

            if (_clientWebSocket.State == WebSocketState.None)
            {
                _clientWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            }
        }
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
            await _clientWebSocket.ConnectAsync(_uri, _cancellationToken);
            CallOnConnected();
            WatchForMessagesFromServer();
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
    }
}
