/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using IConnection = RabbitMQ.Client.IConnection;

namespace Warewolf.Driver.RabbitMQ
{
    internal class RabbitConnection : IQueueConnection
    {
        readonly IConnection _connection;

        public RabbitConnection(IConnection connection)
        {
            _connection = connection;
        }

        public bool IsOpen => _connection.IsOpen;

        public IPublisher NewPublisher(IQueueConfig config)
        {
            throw new System.NotImplementedException();
        }

        public void StartConsuming(IQueueConfig config, IConsumer consumer)
        {
            var rconfig = config as RabbitConfig;
            var channel = rconfig.CreateChannel(_connection);

            var evConsumer = new EventingBasicConsumer(channel);
            evConsumer.Received += (ch, ea) =>
            {
                var body = ea.Body;

                consumer.Consume(body);

                channel.BasicAck(ea.DeliveryTag, false);
            };
        }
    }
}