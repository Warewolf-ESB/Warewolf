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
using System.Text;
using Warewolf.Logging;
using Warewolf.Streams;

namespace Warewolf.Logger
{
    public class LoggerConnection : ILoggerConnection
    {
        private WebSocketServer _server;

        public void Dispose()
        {
            _server.Dispose();
        }
        public void StartConsuming(ILoggerConfig config, IConsumer consumer)
        {
            _server = new WebSocketServer(config.ServerLoggingAddress);
            _server.RestartAfterListenError = true;
            _server.Start(socket =>
            {
                //socket.OnOpen = () => Console.WriteLine("Open!");
                //socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = message => consumer.Consume(Encoding.Default.GetBytes(message));
            });

        }
        public ILoggerPublisher NewPublisher() => throw new System.NotImplementedException();
    }
}