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
            var channel = CreateChannel(config as RabbitConfig);
            return new RabbitPublisher(config as RabbitConfig, channel);
        }

        public void StartConsuming(IQueueConfig config, IConsumer consumer)
        {
            var rabbitConfig = config as RabbitConfig; 
            IModel channel = CreateChannel(config as RabbitConfig);

            var eventConsumer = new EventingBasicConsumer(channel);
            eventConsumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body;

                consumer.Consume(body);

                channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            channel.BasicConsume(queue: rabbitConfig.QueueName,
                                        noAck: false,
                                        consumer: eventConsumer);
        }

        private IModel CreateChannel(RabbitConfig rConfig)
        {
            return rConfig.CreateChannel(_connection);
        }

        private bool _isDisposed = false;

        protected virtual void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    _connection.Dispose();
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}