/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.Threading;
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
        private Timer connectionTimer = null;
        private bool isConsumerCancelled = false;
        private DateTime consumerCancelledDateTime = DateTime.MinValue;
        private string currentProcessId;

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

            var initialCount = Environment.ProcessorCount * 5;
            var throttler = new SemaphoreSlim(initialCount);
            var eventConsumer = new EventingBasicConsumer(channel);
            eventConsumer.Received += (model, eventArgs) =>
            {
                if (isConsumerCancelled) return;

                var body = eventArgs.Body;
                var headers = new Warewolf.Data.Headers();
                headers["Warewolf-Custom-Transaction-Id"] = new[] { eventArgs.BasicProperties.CorrelationId };

                try
                {
                    throttler.Wait();
                    var resultTask = consumer.Consume(body, headers);
                    resultTask.Wait();

                    if (resultTask.Result == Data.ConsumerResult.Success)
                    {
                        channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                }
                finally
                {
                    throttler.Release();
                }
            };
            eventConsumer.ConsumerCancelled += (model, ea) =>
            {
                if (isConsumerCancelled) return;
                isConsumerCancelled = true;
                consumerCancelledDateTime = DateTime.UtcNow;
            };

            channel.BasicConsume(queue: rabbitConfig.QueueName, autoAck: false, consumer: eventConsumer);

            connectionTimer = new Timer(state =>
            {
                if (throttler.CurrentCount != initialCount) return;

                try
                {
                    if (isConsumerCancelled)
                        throw new Exception("Consumer cancelled at " +
                            consumerCancelledDateTime.ToString("dd MMM HH:mm:ss:ffff") + " UTC");

                    channel.QueueDeclarePassive(rabbitConfig.QueueName);
                }
                catch (Exception ex)
                {
                    try
                    {
                        connectionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        connectionTimer.Dispose();
                    }
                    catch
                    {

                    }
                    throw new Exception(GetCurrentProcessId(), ex);
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));
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

        private string GetCurrentProcessId()
        {
            if (null == currentProcessId)
            {
                try
                {
                    currentProcessId = "Process Id : " + Process.GetCurrentProcess().Id.ToString();
                }
                catch
                {
                    currentProcessId = "<No process id>";
                }
            }
            return currentProcessId;
        }
    }
}