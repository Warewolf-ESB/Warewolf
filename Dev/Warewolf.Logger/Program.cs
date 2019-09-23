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
using System.Collections.Generic;
using System.Linq;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;

namespace Warewolf.Logger
{
    public class Program
    {

        static void Main(string[] args)
        {
            var context = new LoggerContext(args);
            if (context.Errors.Count() > 0)
            {
                Environment.Exit(1);
            };
            var impl = new Implementation(context, new WebSocketServerFactory(), new ConsoleWindowFactory(), new LogServerFactory(), new Writer(), new PauseHelper());
            impl.Run();
            impl.Pause();
        }

        public class Implementation
        {
            private readonly IConsoleWindowFactory _consoleWindowFactory;
            private readonly IWebSocketServerFactory _webSocketServerFactory;
            private readonly ILoggerContext _context;
            private readonly IWriter _writer;
            private readonly IPauseHelper _pause;
            private readonly ILogServerFactory _logServerFactory;

            public Implementation(ILoggerContext context, IWebSocketServerFactory webSocketServerFactory, IConsoleWindowFactory consoleWindowFactory, ILogServerFactory logServerFactory, IWriter writer, IPauseHelper pause)
            {
                _consoleWindowFactory = consoleWindowFactory;

                _context = context;
                _webSocketServerFactory = webSocketServerFactory;
                _writer = writer;
                _pause = pause;
                _logServerFactory = logServerFactory;
            }

            public void Run()
            {
                if (_context.Verbose)
                {
                    _ = _consoleWindowFactory.New();
                }

                var logServer = _logServerFactory.New(_webSocketServerFactory, _writer, _context);
                logServer.Start(new List<IWebSocketConnection>()); 
            }

            public void Pause()
            {
                _pause.Pause();
            }
        }
    }
}
