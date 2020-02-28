/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using CommandLine;
using Dev2.Common;
using Dev2.Common.Wrappers;
using Dev2.Network;
using Dev2.Runtime.Hosting;
using System;
using Warewolf.Auditing;
using Warewolf.Common;
using Warewolf.Data;
using Warewolf.Streams;

namespace QueueWorker
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<Args>(args);
            return result.MapResult(
                options => new Implementation(options).Run(),
                _ => 1);
        }

        internal class Implementation
        {
            private readonly Args _options;

            public Implementation(Args options)
            {
                this._options = options;
            }
            public int Run()
            {
                if (_options.ShowConsole)
                {
                    _ = new ConsoleWindow();
                }
                var serverEndpoint = _options.ServerEndpoint;
                var environmentConnection = new ServerProxy(serverEndpoint);

                Console.Write("connecting to server: " + serverEndpoint + "...");
                environmentConnection.Connect(Guid.Empty);
                Console.WriteLine("done.");
                var resourceCatalogProxy = new ResourceCatalogProxy(environmentConnection);

                var config = new WorkerContext(_options, resourceCatalogProxy, TriggersCatalog.Instance);

                using (var watcher = new FileSystemWatcherWrapper())
                {
                    config.WatchTriggerResource(watcher);
                    watcher.Created += (o, t) => Environment.Exit(1);
                    watcher.Changed += (o, t) => Environment.Exit(0);
                    watcher.Deleted += (o, t) => Environment.Exit(0);
                    watcher.Renamed += (o, t) => Environment.Exit(0);

                    new QueueWorkerImplementation(config).Run();
                }

                return 0;
            }
        }

        private class QueueWorkerImplementation
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
                        //logger.Error("Failed to start queue worker: No queue source", _config.QueueName);
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
