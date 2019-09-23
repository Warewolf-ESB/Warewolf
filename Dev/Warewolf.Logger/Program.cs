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
using System.Linq;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;

namespace Warewolf.Logger
{
    public class Program
    {

        static void Main(string[] args)
        {
            var config = new LoggerContext(args);
            if (config.Errors.Count() > 0)
            {
                Environment.Exit(1);
            };
            var impl = new Implementation(config, new WebSocketServerFactory(), new ConsoleWindowFactory(), new LogServerFactory(), new Writer());
            impl.Run();
            impl.Pause();
        }

        public class Implementation
        {
            private readonly IConsoleWindowFactory _consoleWindowFactory;
            private readonly IWebSocketServerFactory _webSocketServerFactory;
            private readonly ILoggerContext _loggerContext;
            private readonly IWriter _writer;
            private readonly ILogServerFactory _logServerFactory;

            public Implementation(ILoggerContext loggerContext, IWebSocketServerFactory webSocketServerFactory, IConsoleWindowFactory consoleWindowFactory, ILogServerFactory logServerFactory, IWriter writer)
            {
                _consoleWindowFactory = consoleWindowFactory;

                _loggerContext = loggerContext;
                _webSocketServerFactory = webSocketServerFactory;
                _writer = writer;
                _logServerFactory = logServerFactory;
            }

            public void Run()
            {
                _ = _consoleWindowFactory.New();

                var logServer = _logServerFactory.New(_webSocketServerFactory, _writer, _loggerContext);
                logServer.Start(); 
            }

            public void Pause()
            {
                _writer.ReadLine();
            }
        }
    }
}
