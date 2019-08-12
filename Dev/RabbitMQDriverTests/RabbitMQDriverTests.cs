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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Driver.RabbitMQ;
using System.Text;
using System.Threading;

namespace RabbitMQDriverTests
{
    [TestClass]
    public class RabbitMQDriverTests
    {

        [TestMethod]
        [Owner("Sphamandla Dube")]
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

            var connection = queueSource.NewConnection(config);
            connection.StartConsuming(config, testConsumer);

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
        [Owner("Sphamandla Dube")]
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
            var data = Encoding.UTF8.GetBytes("Hello");
            var connection = queueSource.NewConnection(config);
            var publisher = connection.NewPublisher(config);
            publisher.Publish(data);
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
