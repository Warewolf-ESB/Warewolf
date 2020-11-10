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
using System.Threading;
using CommandLine;
using Dev2.Common;
using Dev2.Common.Interfaces.Communication;
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
            var result = CommandLine.Parser.Default.ParseArguments<Args>(args);
            return result.MapResult(
                options => new Impl(options).Run(), _ => 1);
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
                var context = new HangfireContext(_options);
                var implConfig = new Implementation.ConfigImpl
                {
                    ExecutionLoggerFactory = new ExecutionLoggerFactory(),
                    Writer = new Writer(),
                    PauseHelper = new PauseHelper(),
                    ExitHelper = new ExitHelper(),
                };
                var implementation = new Implementation(context, implConfig);
                implementation.Run();
                implementation.WaitForExit();
                return 0;
            }
        }

        internal class Implementation
        {
            private readonly IExecutionLogPublisher _logger;
            private readonly IWriter _writer;
            private readonly IPauseHelper _pause;
            private readonly IExitHelper _exit;
            private readonly IHangfireContext _hangfireContext;
            readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            public Implementation(IHangfireContext hangfireContext, ConfigImpl implConfig)
            {
                _logger = implConfig.ExecutionLoggerFactory.New(new JsonSerializer(), new WebSocketPool());
                _writer = implConfig.Writer;
                _pause = implConfig.PauseHelper;
                _exit = implConfig.ExitHelper;
                _hangfireContext = hangfireContext;
                _persistence = Config.Persistence;
                _deserializer = new Dev2JsonSerializer();
            }

            public Implementation(IHangfireContext hangfireContext, ConfigImpl configImpl, PersistenceSettings persistenceSettings, IBuilderSerializer deserializer)
                : this(hangfireContext, configImpl)
            {
                _persistence = persistenceSettings;
                _deserializer = deserializer;
            }

            public void Run()
            {
                try
                {
                    if (_hangfireContext.Verbose)
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
                        return;
                    }

                    ConfigureServerStorage(connectionString);
                    var dashboardEndpoint = _persistence.DashboardHostname + ":" + _persistence.DashboardPort;
                    var options = new StartOptions();
                    options.Urls.Add(dashboardEndpoint);
                    WebApp.Start<Dashboard>(options);
                    _writer.WriteLine("Hangfire dashboard started...");
                    _logger.Info("Hangfire dashboard started...");
                    _ = new BackgroundJobServer();
                    _writer.WriteLine("Hangfire server started...");
                    _logger.Info("Hangfire server started...");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Hangfire Server OnError, Error details:{ex.Message}");
                    _writer.WriteLine($"Hangfire Server OnError, Error details:{ex.Message}");
                }
            }

            private static PersistenceSettings _persistence;
            private readonly IBuilderSerializer _deserializer;

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

            public void WaitForExit()
            {
                if (_hangfireContext.Verbose)
                {
                    _pause.Pause();
                }
                else
                {
                    _waitHandle.WaitOne();
                }
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