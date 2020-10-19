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
using System.Runtime.CompilerServices;
using System.Threading;
using CommandLine;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin.Hosting;
using Warewolf.Auditing;
using Warewolf.Common;
using Warewolf.Security.Encryption;
using Warewolf.Streams;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Warewolf.HangfireServer
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var implConfig = new Impl.Config
            {
                HangfireExecutionLoggerFactory = new HangfireExecutionLogger.HangfireExecutionLoggerFactory(),
                Writer = new Writer(),
            };
            var result = CommandLine.Parser.Default.ParseArguments<Args>(args);
            return result.MapResult(
                options => new Impl(options, implConfig).Run(),
                _ => 1);
        }

        internal class Impl
        {
            private readonly IArgs _options;
            private readonly IHangfireExecutionLogPublisher _logger;
            private readonly IWriter _writer;

            public Impl(Args options, Config implConfig)
            {
                _options = options;
                _logger = implConfig.HangfireExecutionLoggerFactory.New(new JsonSerializer(), new WebSocketPool());
                _writer = implConfig.Writer;
            }

            public int Run()
            {
                if (_options.ShowConsole)
                {
                    _ = new ConsoleWindow();
                }

                var context = new HangfireContext(_options);
                var implementation = new Implementation(context, _logger, _writer, new PauseHelper());
                implementation.Run();
                implementation.WaitForExit();
                return 0;
            }

            internal class Config
            {
                public IWriter Writer { get; set; }
                public HangfireExecutionLogger.IHangfireExecutionLoggerFactory HangfireExecutionLoggerFactory { get; set; }
            }
        }
    }

    internal class Implementation
    {
        private readonly IHangfireExecutionLogPublisher _logger;
        private readonly IWriter _writer;
        private BackgroundJobServer _server;
        private IHangfireContext _context;
        private readonly IPauseHelper _pause;
        readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        public Implementation(IHangfireContext context, IHangfireExecutionLogPublisher logger, IWriter writer, IPauseHelper pause)
        {
            _context = context;
            _logger = logger;
            _writer = writer;
            _pause = pause;
        }

        public void Run()
        {
            _writer.WriteLine("Starting Hangfire server...");

            GlobalConfiguration.Configuration
                .UseFilter(new LogEverythingAttribute())
                .UseSqlServerStorage(ConnectionString(), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });
            var dashboardEndpoint = Dev2.Common.Config.Persistence.DashboardHostname + ":" + Dev2.Common.Config.Persistence.DashboardPort;
            var options = new StartOptions();
            options.Urls.Add(dashboardEndpoint);
            WebApp.Start<Dashboard>(options);
            _writer.WriteLine("Hangfire dashboard started...");
            _server = new BackgroundJobServer();
            _writer.WriteLine("Hangfire server started...");
        }

        private string ConnectionString()
        {
            var payload = Dev2.Common.Config.Persistence.PersistenceDataSource.Payload;
            if (Dev2.Common.Config.Persistence.EncryptDataSource)
            {
                payload = payload.CanBeDecrypted() ? DpapiWrapper.Decrypt(payload) : payload;
            }

            var source = new Dev2JsonSerializer().Deserialize<DbSource>(payload);
            return source.ConnectionString;
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