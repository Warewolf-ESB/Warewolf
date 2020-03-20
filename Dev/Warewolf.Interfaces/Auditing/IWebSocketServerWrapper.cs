/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Fleck;
using System;

namespace Warewolf.Interfaces.Auditing
{
    public interface IWebSocketServerWrapper
    {
        void Dispose();
        void Start(Action<IWebSocketConnection> config);
    }

    public class WebSocketServerWrapper : IWebSocketServer, IWebSocketServerWrapper
    {
        private readonly IWebSocketServer _webSocketServer;

        public WebSocketServerWrapper(string endPoint)
        {
            _webSocketServer = new WebSocketServer(endPoint)
            {
                RestartAfterListenError = true
            };
        }

        public void Dispose()
        {
            _webSocketServer.Dispose();
        }

        public void Start(Action<IWebSocketConnection> config)
        {
            _webSocketServer.Start(config);
        }

    }
}
