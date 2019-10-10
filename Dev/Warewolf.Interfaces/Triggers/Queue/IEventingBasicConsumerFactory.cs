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

namespace Warewolf.Triggers
{
    public interface IEventingBasicConsumerFactory
    {
        IEventingBasicConsumerWrapper New(IModel channel);
    }

    public class EventingBasicConsumerFactory : IEventingBasicConsumerFactory
    {
        public IEventingBasicConsumerWrapper New(IModel channel)
        {
            return new EventingBasicConsumerWrapper(channel);
        }
    }

    public interface IEventingBasicConsumerWrapper : IBasicConsumer
    {
        string ConsumerTag { get; }

        event EventHandler<BasicDeliverEventArgs> Received;
    }

    public class EventingBasicConsumerWrapper : IEventingBasicConsumerWrapper
    {
        private readonly EventingBasicConsumer _consumer;
        private readonly IModel _channel;

        public EventingBasicConsumerWrapper(IModel channel)
        {
            _channel = channel;
            _consumer = new EventingBasicConsumer(_channel);
        }

        public string ConsumerTag => _consumer.ConsumerTag;

        public IModel Model => _channel;

        public event EventHandler<BasicDeliverEventArgs> Received;
        public event EventHandler<ConsumerEventArgs> ConsumerCancelled;

        public void HandleBasicCancel(string consumerTag)
        {
            throw new NotImplementedException();
        }

        public void HandleBasicCancelOk(string consumerTag)
        {
            throw new NotImplementedException();
        }

        public void HandleBasicConsumeOk(string consumerTag)
        {
            throw new NotImplementedException();
        }

        public void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            throw new NotImplementedException();
        }

        public void HandleModelShutdown(object model, ShutdownEventArgs reason)
        {
            throw new NotImplementedException();
        }
    }

}