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
using Moq;
using Warewolf.Driver.RabbitMQ;
using System.Text;

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

            var config = new RabbitConfig();

            //----------------------Act--------------------------

            var connection = queueSource.NewConnection(config);
            connection.StartConsuming(config, testConsumer);

            //----------------------Assert-----------------------
            Assert.IsFalse(testConsumer.IsDataReceived);
        }

        [TestMethod]
        [Owner("Sphamandla Dube")]
        [TestCategory(nameof(RabbitMQSource))]
        public void RabbitMQSource_Publish_Success()
        {
            //----------------------Arrange----------------------
            var mockQueueSource = new Mock<IQueueSource>();
            var mockQueueConnection = new Mock<IQueueConnection>();
            var mockConfig = new Mock<IQueueConfig>();
            var mockPublisher = new Mock<IPublisher>();

            var testConsumer = new TestConsumer();
            var config = mockConfig.Object;

            mockQueueConnection.Setup(o => o.NewPublisher(config)).Returns(mockPublisher.Object);

            mockQueueSource.Setup(o => o.NewConnection(config)).Returns(mockQueueConnection.Object);

            var queueSource = mockQueueSource.Object;

            //----------------------Act--------------------------
            var data = Encoding.UTF8.GetBytes("Hello");
            var connection = queueSource.NewConnection(config);
            var publisher = connection.NewPublisher(config);
            publisher.Publish(data);


            mockPublisher.Verify(o => o.Publish(data), Times.Once);
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
