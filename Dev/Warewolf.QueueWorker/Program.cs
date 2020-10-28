/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using CommandLine;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Network;
using Dev2.Runtime.Hosting;
using Dev2.Studio.Interfaces;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Warewolf;
using Warewolf.Auditing;
using Warewolf.Common;
using Warewolf.Execution;
using Warewolf.Streams;
using Warewolf.Triggers;
using static Warewolf.Common.ExecutionLogger;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace QueueWorker
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var implConfig = new Implementation.Config
            {
                ExecutionLoggerFactory = new ExecutionLoggerFactory(),
                Writer = new Writer(),
                ServerProxyFactory = new ServerProxyFactory(),
                ResourceCatalogProxyFactory = new ResourceCatalogProxyFactory(),
                WorkerContextFactory = new WorkerContextFactory(),
                TriggersCatalogFactory = new TriggersCatalogFactory(),
                FilePath = new FilePathWrapper(),
                FileSystemWatcherFactory = new FileSystemWatcherFactory(),
                EnvironmentWrapper = new EnvironmentWrapper(),
                QueueWorkerImplementationFactory = new QueueWorkerImplementationFactory()
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
            private readonly IFilePath _filePath;
            private readonly IEnvironmentWrapper _environment;

            private readonly IServerProxyFactory _serverProxyFactory;
            private readonly IResourceCatalogProxyFactory _resourceCatalogProxyFactory;
            private readonly IWorkerContextFactory _workerContextFactory;
            private readonly ITriggersCatalogFactory _triggersCatalogFactory;
            private readonly IFileSystemWatcherFactory _fileSystemWatcherFactory;
            private readonly IQueueWorkerImplementationFactory _queueWorkerImplementationFactory;

            public Implementation(IArgs options, Config implConfig)
            { 
                _options = options;
                _logger = implConfig.ExecutionLoggerFactory.New(new JsonSerializer(), new WebSocketPool());
                _writer = implConfig.Writer;
                _filePath = implConfig.FilePath;
                _environment = implConfig.EnvironmentWrapper;

                _serverProxyFactory = implConfig.ServerProxyFactory;
                _resourceCatalogProxyFactory = implConfig.ResourceCatalogProxyFactory;
                _workerContextFactory = implConfig.WorkerContextFactory;
                _triggersCatalogFactory = implConfig.TriggersCatalogFactory;
                _fileSystemWatcherFactory = implConfig.FileSystemWatcherFactory;
                _queueWorkerImplementationFactory = implConfig.QueueWorkerImplementationFactory;
            }

            public int Run()
            {
                if (_options.ShowConsole)
                {
                    _ = new ConsoleWindow();
                }

                var serverEndpoint = _options.ServerEndpoint;
                var triggerId = _options.TriggerId;
                var environmentConnection = _serverProxyFactory.New(serverEndpoint);

                _writer.Write("Connecting to server: " + serverEndpoint + "...");
                _logger.Info("Connecting to server: " + serverEndpoint + "...");

                Task<bool> connectTask = TryConnectingToWarewolfServer(environmentConnection);
                if (connectTask.Result is false)
                {
                    _writer.WriteLine("failed.");
                    _logger.Info("Connecting to server: " + serverEndpoint + "... unsuccessful");
                    return 0;
                }

                _writer.WriteLine("done.");
                _logger.Info("Connecting to server: " + serverEndpoint + "... successful");

                var resourceCatalogProxy = _resourceCatalogProxyFactory.New(environmentConnection);

                _writer.Write(@"Loading trigger resource: " + triggerId + " ...");
                _logger.Info(@"Loading trigger resource: " + triggerId + " ...");
                var config = TryGetConfig(resourceCatalogProxy, _triggersCatalogFactory.New(), _filePath);
                if (config is null)
                {
                    return 0;
                }
                else
                {
                    _writer.WriteLine("done.");
                    _logger.Info(@"Loading trigger resource: " + triggerId + " ... successful");

                    _writer.Write("Start watching trigger resource: " + triggerId);
                    _logger.Info("Start watching trigger resource: " + triggerId);
                    using (var watcher = _fileSystemWatcherFactory.New())
                    {
                        config.WatchTriggerResource(watcher);
                        watcher.Created += (o, t) => _environment.Exit(1);
                        watcher.Changed += (o, t) => _environment.Exit(0);
                        watcher.Deleted += (o, t) => _environment.Exit(0);
                        watcher.Renamed += (o, t) => _environment.Exit(0);

                        _queueWorkerImplementationFactory.New(config).Run();
                    }
                }

                return 0;
            }

            private Task<bool> TryConnectingToWarewolfServer(IEnvironmentConnection environmentConnection)
            {
                try
                {
                    var connectTask = environmentConnection.ConnectAsync(Guid.Empty);
                    connectTask.Wait();
                    return connectTask;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, _options.ServerEndpoint);
                    return Task.FromResult(false);
                }
                
            }

            private IWorkerContext TryGetConfig(IResourceCatalogProxy resourceCatalogProxy, ITriggersCatalog triggersCatalog, IFilePath filePath)
            {
                var triggerId = _options.TriggerId;

                IWorkerContext workerContext;
                try
                {
                    workerContext = _workerContextFactory.New(_options, resourceCatalogProxy, triggersCatalog, filePath);
                }
                catch (Exception ex)
                {
                    _writer.WriteLine("failed.");

                    _writer.Write(ex.Message);
                    _logger.Error(ex.Message, triggerId);

                    return null;
                }
                return workerContext;
            }

            internal class Config
            {
                public IFilePath FilePath { get; set; }
                public IWriter Writer { get; set; }
                public IEnvironmentWrapper EnvironmentWrapper { get; set; }

                public IExecutionLoggerFactory ExecutionLoggerFactory { get; set; }
                public IServerProxyFactory ServerProxyFactory { get; set; }
                public IResourceCatalogProxyFactory ResourceCatalogProxyFactory { get; set; }
                public IWorkerContextFactory WorkerContextFactory { get; set; }
                public ITriggersCatalogFactory TriggersCatalogFactory { get; set; }
                public IFileSystemWatcherFactory FileSystemWatcherFactory { get; set; }
                internal IQueueWorkerImplementationFactory QueueWorkerImplementationFactory { get; set; }
            };
        }


        internal interface IQueueWorkerImplementationFactory
        {
            IQueueWorkerImplementation New(IWorkerContext config);
        }

        internal class QueueWorkerImplementationFactory : IQueueWorkerImplementationFactory
        {
            public IQueueWorkerImplementation New(IWorkerContext config)
            {
                return new QueueWorkerImplementation(config);
            }
        }

        internal interface IQueueWorkerImplementation
        {
            void Run();
        }

        private class QueueWorkerImplementation : IQueueWorkerImplementation
        {
            private readonly IWorkerContext _config;

            public QueueWorkerImplementation(IWorkerContext config)
            {
                _config = config;
            }

            public void Run()
            {
                var logger = new ExecutionLogger(new JsonSerializer(), new WebSocketPool());
                logger.Info("Starting queue worker", _config.QueueName);
                try
                {
                    if (_config.Source != null)
                    {
                        var deadletterPublisher = CreateDeadLetterPublisher();

                        var requestForwarder = new WarewolfWebRequestForwarder(new HttpClientFactory(), deadletterPublisher, _config.WorkflowUrl, _config.Username, _config.Password, _config.Inputs, _config.MapEntireMessage);
                        var loggingForwarder = new LoggingConsumerWrapper(logger, requestForwarder, _config.TriggerId, _config.Username);

                        var queue = _config.Source;

                        Console.WriteLine($"Starting listening: {queue.ResourceName} Queue: {_config.QueueName}");
                        Console.WriteLine($"Workflow: {_config.WorkflowUrl} Inputs: {_config.Inputs}");

                        var connection = queue.NewConnection();
                        connection.StartConsuming(_config.QueueConfig, loggingForwarder);
                    }
                    else
                    {
                        Console.WriteLine("Failed to start queue worker: No queue source.");
                        logger.Error("Failed to start queue worker: No queue source", _config.QueueName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    logger.Error(ex.Message, _config.QueueName);
                }
            }

            private IPublisher CreateDeadLetterPublisher()
            {
                var deadletterPublisher = new DeadLetterPublisher(_config);
                return deadletterPublisher;
            }

            class DeadLetterPublisher : IPublisher
            {
                private readonly IWorkerContext _config;

                public DeadLetterPublisher(IWorkerContext config)
                {
                    _config = config;
                }

                public void Publish(byte[] value)
                {
                    var deadLetterSource = _config.DeadLetterSink;
                    var deadLetterConnection = deadLetterSource.NewConnection();
                    var publisher = deadLetterConnection.NewPublisher(_config.DeadLetterConfig);
                    publisher.Publish(value);
                }
            }
        }
    }
}