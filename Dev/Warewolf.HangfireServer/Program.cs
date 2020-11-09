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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CommandLine;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin.Hosting;
using Warewolf;
using Warewolf.Auditing;
using Warewolf.Common;
using Warewolf.Execution;
using Warewolf.Security.Encryption;
using Warewolf.Streams;
using static Warewolf.Common.NetStandard20.ExecutionLogger;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace HangfireServer
{
    public static class Program
    {
        [ExcludeFromCodeCoverage]
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

        internal class Implementation
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
                _persistence = Config.Persistence;
                _deserializer = new Dev2JsonSerializer();
            }

            public Implementation(IArgs options, ConfigImpl configImpl, PersistenceSettings persistenceSettings, Dev2JsonSerializer deserializer)
                :this(options, configImpl)
            {
                _persistence = persistenceSettings;
                _deserializer = deserializer;
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
                    _logger.Error("Fatal Error: Could not find persistence config file. Hangfire server is unable to start.");
                    _writer.WriteLine("Fatal Error: Could not find persistence config file. Hangfire server is unable to start.");
                    _writer.Write("Press any key to exit...");
                    WaitForExit();
                    return 0;
                }

                ConfigureServerStorage(connectionString);
                var dashboardEndpoint = _persistence.DashboardHostname + ":" + _persistence.DashboardPort;
                var options = new StartOptions();
                options.Urls.Add(dashboardEndpoint);
                WebApp.Start<Dashboard>(options);
                _writer.WriteLine("Hangfire dashboard started...");
                _ = new BackgroundJobServer();
                _writer.WriteLine("Hangfire server started...");
                return 0;
            }

            private static PersistenceSettings _persistence;
            private readonly Dev2JsonSerializer _deserializer;

            private void ConfigureServerStorage(string connectionString)
            {
                var resumptionAttribute = new ResumptionAttribute(_logger, null);

                GlobalConfiguration.Configuration
                                .UseFilter(resumptionAttribute)
                                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                                {
                                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                                    QueuePollInterval = TimeSpan.Zero,
                                    UseRecommendedIsolationLevel = true,
                                    DisableGlobalLocks = true
                                });
            }

            private string ConnectionString()
            {
                var payload = _persistence.PersistenceDataSource.Payload;
                if (string.IsNullOrEmpty(payload))
                {
                    return string.Empty;
                }
                if (_persistence.EncryptDataSource)
                {
                    payload = payload.CanBeDecrypted() ? DpapiWrapper.Decrypt(payload) : payload;
                }
                var source = _deserializer.Deserialize<DbSource>(payload);
                return source.ConnectionString;
            }

            private void WaitForExit()
            {
                _pause.Pause();
                _exit.Exit();
            }

            internal class ConfigImpl
            {
                public IWriter Writer { get; set; }
                public IExecutionLoggerFactory ExecutionLoggerFactory { get; set; }
                public IPauseHelper PauseHelper { get; set; }
                public IExitHelper ExitHelper { get; set; }
            }
        }
    }
}