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
using Dev2.Network;
using Dev2.Runtime.Triggers;
using Dev2.Util;
using System;
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

                new QueueWorkerImplementation(config).Run();

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
                var deadletterPublisher = CreateDeadLetterPublisher();

                var requestForwarder = new WarewolfWebRequestForwarder(new HttpClientFactory(), deadletterPublisher, _config.WorkflowUrl, _config.ValueKeys);

                var queue = _config.Source;

                Console.WriteLine($"Starting listening: {queue.ResourceName} Queue: {_config.QueueName}");
                Console.WriteLine($"Workflow: {_config.WorkflowUrl} Inputs: {_config.Inputs}");

                var connection = queue.NewConnection();
                connection.StartConsuming(_config.QueueConfig, requestForwarder);
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
                    var publisher = deadLetterConnection.NewPublisher(_config.QueueConfig);
                    publisher.Publish(value);
                }
            }
        }
    }
}
