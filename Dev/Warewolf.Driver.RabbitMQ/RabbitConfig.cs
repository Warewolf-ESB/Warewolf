/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using RabbitMQ.Client;
using System.Collections.Generic;
using Warewolf.Triggers;
using IConnection = RabbitMQ.Client.IConnection;

namespace Warewolf.Driver.RabbitMQ
{
    public class RabbitConfig : IQueueConfig
    {
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
        public IBasicProperties BasicProperties { get; set; }
        public string QueueName { get; set; }
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }
        public IDictionary<string, object> Arguments { get; set; }

        public RabbitConfig()
        {
        }

        //options
        internal IModel CreateChannel(IConnection connection)
        {
            var channel = connection.CreateModel();
            channel.QueueDeclare(QueueName, Durable, Exclusive, AutoDelete, Arguments);

              //configure channel using options here             
            return channel;
        }
    }
}
