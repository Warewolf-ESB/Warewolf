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
using CommandLine;
using Dev2.Common;
using Dev2.Common.Serializers;
using Dev2.Runtime.ServiceModel.Data;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin.Hosting;
using Warewolf;
using Warewolf.Auditing;
using Warewolf.Common;
using Warewolf.Security.Encryption;
using Warewolf.Streams;
using static HangfireServer.ExecutionLogger;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace HangfireServer
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var implConfig = new Implementation.ConfigImpl
            {
                ExecutionLoggerFactory = new ExecutionLoggerFactory(),
                Writer = new Writer(),
                PauseHelper = new PauseHelper(),
                ExitHelper = new ExitHelper(),
            };
            var result = CommandLine.Parser.Default.ParseArguments<Args>(args);
            return result.MapResult(
                options => new Implementation(options, implConfig).Run(),
                _ => 1);
        }

        public class Implementation
        {
            private readonly IArgs _options;
            private readonly IExecutionLogPublisher _logger;
            private readonly IWriter _writer;
            private readonly IPauseHelper _pause;
            private readonly IExitHelper _exit;
            private readonly IHangfireContext _hangfireContext;

            public Implementation(IArgs options, ConfigImpl implConfig)
            {
                _options = options;
                _logger = implConfig.ExecutionLoggerFactory.New(new JsonSerializer(), new WebSocketPool());
                _writer = implConfig.Writer;
                _pause = implConfig.PauseHelper;
                _exit = implConfig.ExitHelper;
                _hangfireContext = new HangfireContext(_options);
            }

            public int Run()
            {
                if (_options.ShowConsole)
                {
                    _ = new ConsoleWindow();
                }

                _writer.WriteLine("Starting Hangfire server...");
                _logger.Info("Starting Hangfire server...");

                var connectionString = ConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    _writer.WriteLine("Fatal Error: Could not find persistence config file. Hangfire server is unable to start.");
                    _writer.Write("Press any key to exit...");
                    _pause.Pause();
                    _exit.Exit();
                    return 0;
                }

                ConfigureServerStorage(connectionString);
                var dashboardEndpoint = Config.Persistence.DashboardHostname + ":" + Config.Persistence.DashboardPort;
                var options = new StartOptions();
                options.Urls.Add(dashboardEndpoint);
                WebApp.Start<Dashboard>(options);
                _writer.WriteLine("Hangfire dashboard started...");
                _ = new BackgroundJobServer();
                _writer.WriteLine("Hangfire server started...");
                return 0;
            }

            private void ConfigureServerStorage(string connectionString)
            {
                var logEverythingAttribute = new LogEverythingAttribute(_logger);

                GlobalConfiguration.Configuration
                                .UseFilter(logEverythingAttribute)
                                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                                {
                                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                                    QueuePollInterval = TimeSpan.Zero,
                                    UseRecommendedIsolationLevel = true,
                                    DisableGlobalLocks = true
                                });
            }

            private static string ConnectionString()
            {
                var payload = Config.Persistence.PersistenceDataSource.Payload;
                if (string.IsNullOrEmpty(payload))
                {
                    return string.Empty;
                }
                if (Config.Persistence.EncryptDataSource)
                {
                    payload = payload.CanBeDecrypted() ? DpapiWrapper.Decrypt(payload) : payload;
                }

                var source = new Dev2JsonSerializer().Deserialize<DbSource>(payload);
                return source.ConnectionString;
            }

            private void WaitForExit()
            {
                if (_hangfireContext.Verbose)
                {
                    _pause.Pause();
                }
                else
                {
                    _exit.Exit();
                }
            }

            public class ConfigImpl
            {
                public IWriter Writer { get; set; }
                public IExecutionLoggerFactory ExecutionLoggerFactory { get; set; }
                public IPauseHelper PauseHelper { get; set; }
                public IExitHelper ExitHelper { get; set; }
            }
        }
    }
}