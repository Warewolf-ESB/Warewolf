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
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using Warewolf.Resource.Errors;
using Warewolf.Streams;
using Warewolf.Triggers;
using IConnection = RabbitMQ.Client.IConnection;

namespace Warewolf.Driver.RabbitMQ
{
    public class RabbitConnection : IQueueConnection
    {
        private readonly IConnection _connection;
        private readonly IManualResetEventFactory _resetEventFactory;
        private IModel _channel;

        public RabbitConnection(IConnection connection, IModel channel)
            :this(connection)
        {
            _channel = channel;
        }

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

                var resultTask = consumer.Consume(body);
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

        public void StartConsumingWithTimeOut(IEventingBasicConsumerFactory consumerFactory, IManualResetEventFactory resetEventFactory, IStreamConfig stream, IConsumer consumer, int time)
        {
            var config = stream as RabbitConfig;

            using (var signal = resetEventFactory.New(initialState: false))
            {
                var eventConsumer =  consumerFactory.New(channel: _channel); 

                eventConsumer.Received += (model, eventArgs) =>
                {
                    var body = eventArgs.Body;

                    var resultTask = consumer.Consume(body);
                    resultTask.Wait();
                    if (resultTask.Result == Data.ConsumerResult.Success)
                    {
                        _channel.BasicAck(eventArgs.DeliveryTag, false);
                    }

                    signal.Set();
                };

                try
                {
                    var args = new Dictionary<string, object>();

                    _channel.BasicConsume(queue: config.QueueName,
                                        autoAck: false,
                                        noLocal: true,
                                        exclusive: false,
                                        arguments: args,
                                        consumerTag: eventConsumer.ConsumerTag,
                                        consumer: eventConsumer);
                }
                catch (Exception)
                {
                    throw new Exception(string.Format(ErrorResource.RabbitQueueNotFound, config.QueueName));
                }

                var timeout = !signal.WaitOne(TimeSpan.FromSeconds(time));

                if (timeout)
                {
                    _channel.BasicCancel(eventConsumer.ConsumerTag);
                }
            }
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

        public void BasicQos(IStreamConfig stream)
        {
            var config = stream as RabbitConfig;
            var channel = _connection.CreateModel();

            channel.BasicQos(config.PrefetchSize, config.PrefetchCount, config.Acknwoledge);
        }

        public BasicGetResult BasicGet(string queueName, bool acknowledge)
        {
            var channel = _connection.CreateModel();
            return channel.BasicGet(queueName, acknowledge);
        }

    }
}