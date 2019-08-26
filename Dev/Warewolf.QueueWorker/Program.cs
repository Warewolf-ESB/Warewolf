/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Warewolf.Common;
using Warewolf.Data;

namespace QueueWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var processArgs = CommandLine.ParseArguments(args);
            var config = new Config(processArgs);
            new Implementation(config).Run();
        }

        private class Implementation
        {
            private readonly IConfig _config;

            public Implementation(IConfig config)
            {
                _config = config;
            }

            public void Run()
            {
                var requestForwarder = new WarewolfWebRequestForwarder(new HttpClientFactory(), _config.WorkflowUrl, _config.ValueKeys);

                Console.WriteLine("Starting: {_config.HostName} Queue: {_config.QueueName}");
                Console.WriteLine("Workflow: {_config.WorkflowUrl} ValueKey: {_config.ValueKey}");

                //TODO: Replace with fetchQueueFigures
                //var config

                //using (var connection = queueSource.NewConnection(_config))
                //    connection.StartConsuming(_config, requestForwarder);

            }
        }
    }
}
