/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Warewolf.Streams;
using Warewolf.Triggers;
using IConnection = RabbitMQ.Client.IConnection;

namespace Warewolf.Driver.RabbitMQ
{
    public class RabbitConnection : IQueueConnection
    {
        readonly IConnection _connection;

        public RabbitConnection(IConnection connection)
        {
            _connection = connection;
        }

        public bool IsOpen => _connection.IsOpen;

        public IPublisher NewPublisher(IStreamConfig config)
        {
            var channel = CreateChannel(config as RabbitConfig);
            return new RabbitPublisher(config as RabbitConfig, channel);
        }

        public void StartConsuming(IStreamConfig config, IConsumer consumer)
        {
            var rabbitConfig = config as RabbitConfig; 
            var channel = CreateChannel(rabbitConfig);

            var eventConsumer = new EventingBasicConsumer(channel);
            eventConsumer.Received += (model, eventArgs) =>
            {
                var body = eventArgs.Body;
                var headers = new Warewolf.Data.Headers();
                headers["CustomTransactionID"] = new[] { eventArgs.BasicProperties.CorrelationId };
                var resultTask = consumer.Consume(body, headers);
                resultTask.Wait();
                if (resultTask.Result == Data.ConsumerResult.Success)
                {
                    channel.BasicAck(eventArgs.DeliveryTag, false);
                }

            };

            channel.BasicConsume(queue: rabbitConfig.QueueName,
                                        autoAck: false,
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

        public void DeleteQueue(IStreamConfig config)
        {
            var rabbitConfig = config as RabbitConfig;
            var channel = _connection.CreateModel();
            channel.QueueDelete(rabbitConfig.QueueName);
        }
    }
}