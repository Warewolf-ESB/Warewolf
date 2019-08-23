/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using IConnection = RabbitMQ.Client.IConnection;
using System;
using Warewolf.Triggers;

namespace Warewolf.Driver.RabbitMQ.Tests
{
    [TestClass]
    public class RabbitMQDriverTests
    {

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RabbitMQSource))]
        public void RabbitMQSource_GivenSourceCreateNewConnection_Success()
        {
            //----------------------Arrange----------------------
            var queueSource = new RabbitMQSource();

            var testConsumer = new TestConsumer();

            var config = new RabbitConfig
            {
                QueueName = "testQueueName",
                Exchange = "",
                RoutingKey = "testQueueName",
            };

            //----------------------Act--------------------------

            using (var connection = queueSource.NewConnection(config))
            {
                connection.StartConsuming(config, testConsumer);
            }

            int i = 0;
            while (!testConsumer.IsDataReceived)
            {
                Thread.Sleep(100);
                if (i >= 30)
                {
                    break;
                }
                i++;
            }
            //----------------------Assert-----------------------
            Assert.IsFalse(testConsumer.IsDataReceived);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RabbitMQSource))]
        public void RabbitMQSource_Publish_Success()
        {
            //----------------------Arrange----------------------
            var queueSource = new RabbitMQSource();

            var config = new RabbitConfig
            {
                QueueName = "testQueueName",
                Exchange = "",
                RoutingKey = "testQueueName",
            };

            //----------------------Act--------------------------
            var message = Guid.NewGuid().ToString();
            var data = Encoding.UTF8.GetBytes(message);
            var connection = queueSource.NewConnection(config);
            var publisher = connection.NewPublisher(config);
            publisher.Publish(data);

            //------------------------Assert----------------------
            using (var testPublishSuccess = new TestPublishSuccess())
            {
                var sentData = testPublishSuccess.GetSentMessage(config.QueueName);
                Assert.AreEqual(config.Exchange, sentData.Exchange);
                Assert.AreEqual(config.RoutingKey, sentData.RoutingKey);
                Assert.AreEqual(message, Encoding.UTF8.GetString(sentData.Body));
            }
        }

        public class TestPublishSuccess : IDisposable
        {
            readonly IConnectionFactory _factory;
            private IConnection _connection;
            private IModel _channel;

            public TestPublishSuccess()
            {
                _factory = new ConnectionFactory() { HostName = "rsaklfsvrdev.dev2.local", UserName = "test", Password = "test" };
            }

            private IConnection NewConnection()
            {
                return _connection = _factory.CreateConnection();
            }

            public BasicGetResult GetSentMessage(string queueName)
            {
                _channel = NewConnection().CreateModel();
                return _channel.BasicGet(queue: queueName, autoAck:false);
            }

            private bool _isDisposed = false;

            protected virtual void Dispose(bool isDisposing)
            {
                if (!_isDisposed)
                {
                    if (isDisposing)
                    {
                        _channel.Dispose();
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

        public class TestConsumer : IConsumer
        {
            public TestConsumer()
            {
            }

            public bool IsDataReceived { get; internal set; }

            public void Consume(byte[] body)
            {
                IsDataReceived = true;
            }
        }
    }
}
