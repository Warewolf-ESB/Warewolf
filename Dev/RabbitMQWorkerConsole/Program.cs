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
using Warewolf.Driver.RabbitMQ;

namespace QueueWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var processedArgs = new ProcessArgs(args);

            var requestForwarder = new WarewolfWebRequestForwarder(new HttpClientFactory(), processedArgs.WorkflowUrl, processedArgs.ValueKey);

            var queueSource = new RabbitMQSource();

            Console.WriteLine($"Starting: {processedArgs.HostName} Queue: {processedArgs.QueueName}");
            Console.WriteLine($"Workflow: {processedArgs.WorkflowUrl} ValueKey: {processedArgs.ValueKey}");

            //TODO: Replace with fetchQueueFigures
            var config = new RabbitConfig
            {
                QueueName = processedArgs.QueueName,
                Exchange = "direct",
                RoutingKey = "",
            };

            using (var connection = queueSource.NewConnection(config))
            connection.StartConsuming(config, requestForwarder);

        }
    }
}
