using System;
using Warewolf.Common;
using Warewolf.Data;
using Warewolf.Driver.RabbitMQ;

namespace Warewolf.QueueWorkerConsole
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
