/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Resources;
using Dev2.Controller;
using Dev2.Data.ServiceModel;
using Dev2.Network;
using Dev2.Studio.Core;
using Dev2.Util;
using System;
using System.Collections.Generic;
using Warewolf.Common;
using Warewolf.Data;
using Warewolf.Triggers;

namespace QueueWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var processArgs = CommandLine.ParseArguments(args);

            var applicationServerUri = new Uri(string.IsNullOrEmpty(AppUsageStats.LocalHost) ? $"https://{Environment.MachineName.ToLowerInvariant()}:3143" : AppUsageStats.LocalHost);
            var actualWebServerUri = new Uri(applicationServerUri.ToString().ToUpper().Replace("localhost".ToUpper(), Environment.MachineName));
            var environmentConnection = new ServerProxy(actualWebServerUri);

            Console.Write("connecting to server: " + actualWebServerUri + "...");
            environmentConnection.Connect(Guid.Empty);

            var resourceCatalogProxy = new ResourceCatalogProxy(environmentConnection);

            var config = new WorkerContext(processArgs, resourceCatalogProxy);

            new Implementation(config).Run();
        }

        private class Implementation
        {
            private readonly IWorkerContext _config;

            public Implementation(IWorkerContext config)
            {
                _config = config;
            }

            public void Run()
            {
                var deadletterPublisher = CreateDeadLetterPublisher();

                var requestForwarder = new WarewolfWebRequestForwarder(new HttpClientFactory(), deadletterPublisher, _config.WorkflowUrl, _config.ValueKeys);

                Console.WriteLine("Starting: {_config.HostName} Queue: {_config.QueueName}");
                Console.WriteLine("Workflow: {_config.WorkflowUrl} ValueKey: {_config.ValueKey}");

                var queue = _config.Source;
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
