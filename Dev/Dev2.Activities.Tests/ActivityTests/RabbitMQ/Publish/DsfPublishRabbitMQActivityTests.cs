using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;

namespace Dev2.Tests.Activities.ActivityTests.RabbitMQ.Publish
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DsfPublishRabbitMQActivityTests
    {
        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfPublishRabbitMQActivity_Construct")]
        public void DsfSqlBulkInsertActivity_Construct_Paramterless_SetsDefaultPropertyValues()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            DsfPublishRabbitMQActivity dsfPublishRabbitMQActivity = new DsfPublishRabbitMQActivity();
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfPublishRabbitMQActivity);
            Assert.AreEqual("RabbitMQ Publish", dsfPublishRabbitMQActivity.DisplayName);
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfPublishRabbitMQActivity_Execute")]
        public void DsfPublishRabbitMQActivity_Execute_Sucess()
        {
            //------------Setup for test--------------------------
            DsfPublishRabbitMQActivity dsfPublishRabbitMQActivity = new DsfPublishRabbitMQActivity();

            const string queueName = "Q1", message = "Test Message";
            byte[] body = Encoding.UTF8.GetBytes(message);
            Mock<IRabbitMQSource> rabbitMQSource = new Mock<IRabbitMQSource>();
            Mock<IConnectionFactory> connectionFactory = new Mock<IConnectionFactory>();
            Mock<IConnection> connection = new Mock<IConnection>();
            Mock<IModel> channel = new Mock<IModel>();

            connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);

            connection.Setup(c => c.CreateModel()).Returns(channel.Object);
            channel.Setup(c => c.QueueDeclare(queueName, false, false, false, null));
            channel.Setup(c => c.BasicPublish(string.Empty, queueName, null, body));

            PrivateObject p = new PrivateObject(dsfPublishRabbitMQActivity);
            p.SetProperty("ConnectionFactory", connectionFactory.Object);
            p.SetProperty("Connection", connection.Object);
            p.SetProperty("Channel", channel.Object);
            p.SetProperty("RabbitMQSource", rabbitMQSource.Object);

            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", queueName }, { "Message", message } });

            //------------Assert Results-------------------------
            connectionFactory.Verify(c => c.CreateConnection(), Times.Once);
            connection.Verify(c => c.CreateModel(), Times.Once);
            channel.Verify(c => c.QueueDeclare(queueName, false, false, false, null), Times.Once);
            channel.Verify(c => c.BasicPublish(string.Empty, queueName, null, body), Times.Once);
            Assert.AreEqual(result.ToString(), "Success");
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfPublishRabbitMQActivity_Execute")]
        public void DsfPublishRabbitMQActivity_Execute_Failure_NullSource()
        {
            //------------Setup for test--------------------------
            DsfPublishRabbitMQActivity dsfPublishRabbitMQActivity = new DsfPublishRabbitMQActivity();

            var fakeRabbitMQSourceCatalog = new Mock<IFakeRabbitMQSourceCatalog>();
            fakeRabbitMQSourceCatalog.Setup(f => f.GetRabbitMQSourceStub()).Returns<IRabbitMQSource>(null);

            PrivateObject p = new PrivateObject(dsfPublishRabbitMQActivity);
            p.SetProperty("FakeRabbitMQSourceCatalog", fakeRabbitMQSourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(result.ToString(), "Failure: Source has been deleted.");
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfPublishRabbitMQActivity_Execute")]
        public void DsfPublishRabbitMQActivity_Execute_Failure_NoParams()
        {
            //------------Setup for test--------------------------
            DsfPublishRabbitMQActivity dsfPublishRabbitMQActivity = new DsfPublishRabbitMQActivity();
            PrivateObject p = new PrivateObject(dsfPublishRabbitMQActivity);

            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(result.ToString(), "Failure: Queue Name and Message are required.");
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfPublishRabbitMQActivity_Execute")]
        public void DsfPublishRabbitMQActivity_Execute_Failure_InvalidParams()
        {
            //------------Setup for test--------------------------
            DsfPublishRabbitMQActivity dsfPublishRabbitMQActivity = new DsfPublishRabbitMQActivity();
            PrivateObject p = new PrivateObject(dsfPublishRabbitMQActivity);

            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string> { { "Param1", "Blah1" }, { "Param2", "Blah2" } });

            //------------Assert Results-------------------------
            Assert.AreEqual(result.ToString(), "Failure: Queue Name and Message are required.");
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfPublishRabbitMQActivity_Execute")]
        public void DsfPublishRabbitMQActivity_Execute_Failure_NullException()
        {
            //------------Setup for test--------------------------
            DsfPublishRabbitMQActivity dsfPublishRabbitMQActivity = new DsfPublishRabbitMQActivity();

            Mock<IConnectionFactory> connectionFactory = new Mock<IConnectionFactory>();
            Mock<IConnection> connection = new Mock<IConnection>();
            Mock<IModel> channel = new Mock<IModel>();

            connectionFactory.Setup(c => c.CreateConnection()).Returns<IConnection>(null);

            PrivateObject p = new PrivateObject(dsfPublishRabbitMQActivity);
            p.SetProperty("ConnectionFactory", connectionFactory.Object);

            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", "Q1" }, { "Message", "Test message" } });

            //------------Assert Results-------------------------
            connectionFactory.Verify(c => c.CreateConnection(), Times.Once);
            connection.Verify(c => c.CreateModel(), Times.Never);
            channel.Verify(c => c.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()), Times.Never);
            channel.Verify(c => c.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Never);
            Assert.AreEqual(result.ToString(), "Failure");
        }
    }

    // ReSharper restore InconsistentNaming
}