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
using Warewolf.Streams;

namespace Warewolf.Driver.RabbitMQ
{
    public class RabbitPublisher : IPublisher
    {
        private readonly string _exchange;
        private readonly string _routingKey;
        private readonly IBasicProperties _basicProperties;
        private IModel _channel;

        public RabbitPublisher(RabbitConfig config, IModel channel)
        {
            _exchange = config.Exchange ?? string.Empty;
            _routingKey = config.RoutingKey ?? config.QueueName;
            _basicProperties = config.BasicProperties;
            _channel = channel;
        }

        public void Publish(byte[] value)
        {
            _channel.BasicPublish(exchange: _exchange, routingKey: _routingKey, basicProperties: _basicProperties, body: value);
        }
    }
}