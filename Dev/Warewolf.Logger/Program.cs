/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using CommandLine;
using Fleck;
using System.Collections.Generic;
using System.Threading;
using Dev2.Network;
using Warewolf.Common;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;

namespace Warewolf.Logger
{
    public static class Program
    {
        static int Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<Args>(args);
            return result.MapResult(
                options => new Impl(options).Run(),
                _ => 1);
        }

        internal class Impl
        {
            private readonly IArgs _options;

            public Impl(Args options)
            {
                _options = options;
            }

            public int Run()
            {

                var context = new LoggerContext(_options);

                var implementation = new Implementation(context, new WebSocketServerFactory(), new ConsoleWindowFactory(), new LogServerFactory(), new Writer(), new PauseHelper());
                implementation.Run();
                implementation.WaitForExit();
                return 0;
            }
        }

        internal class Implementation
        {
            private readonly IConsoleWindowFactory _consoleWindowFactory;
            private readonly IWebSocketServerFactory _webSocketServerFactory;
            private readonly ILoggerContext _context;
            private readonly IWriter _writer;
            private readonly IPauseHelper _pause;
            private readonly ILogServerFactory _logServerFactory;
            readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

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

            public void WaitForExit()
            {
                if (_context.Verbose)
                {
                    _pause.Pause();
                }
                else
                {
                    _waitHandle.WaitOne();
                }
            }
        }
    }
}