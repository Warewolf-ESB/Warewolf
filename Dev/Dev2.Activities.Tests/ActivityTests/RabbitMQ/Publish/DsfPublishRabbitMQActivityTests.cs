using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Runtime.Interfaces;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.RabbitMQ.Publish
{
    [TestClass]
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
            Mock<IResourceCatalog> resourceCatalog = new Mock<IResourceCatalog>();
            Mock<RabbitMQSource> rabbitMQSource = new Mock<RabbitMQSource>();
            Mock<ConnectionFactory> connectionFactory = new Mock<ConnectionFactory>();
            Mock<IConnection> connection = new Mock<IConnection>();
            Mock<IModel> channel = new Mock<IModel>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);
            connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            connection.Setup(c => c.CreateModel()).Returns(channel.Object);
            channel.Setup(c => c.QueueDeclare(queueName, false, false, false, null));
            channel.Setup(c => c.BasicPublish(string.Empty, queueName, null, body));

            PrivateObject p = new PrivateObject(dsfPublishRabbitMQActivity);
            p.SetProperty("ConnectionFactory", connectionFactory.Object);
            p.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", queueName }, { "Message", message } }) as List<string>;

            //------------Assert Results-------------------------
            resourceCatalog.Verify(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
            connectionFactory.Verify(c => c.CreateConnection(), Times.Once);
            connection.Verify(c => c.CreateModel(), Times.Once);
            channel.Verify(c => c.ExchangeDeclare(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()), Times.Once);
            channel.Verify(c => c.QueueDeclare(It.IsAny<String>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()), Times.Once);
            channel.Verify(c => c.BasicPublish(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()), Times.Once);
            if(result != null)
            {
                Assert.AreEqual(result[0], "Success");
            }
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfPublishRabbitMQActivity_Execute")]
        public void DsfPublishRabbitMQActivity_Execute_Failure_NullSource()
        {
            //------------Setup for test--------------------------
            DsfPublishRabbitMQActivity dsfPublishRabbitMQActivity = new DsfPublishRabbitMQActivity();

            Mock<IResourceCatalog> resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns<RabbitMQSource>(null);

            PrivateObject p = new PrivateObject(dsfPublishRabbitMQActivity);
            p.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string>()) as List<string>;

            //------------Assert Results-------------------------
            if(result != null)
            {
                Assert.AreEqual(result[0], "Failure: Source has been deleted.");
            }
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfPublishRabbitMQActivity_Execute")]
        public void DsfPublishRabbitMQActivity_Execute_Failure_NoParams()
        {
            //------------Setup for test--------------------------
            DsfPublishRabbitMQActivity dsfPublishRabbitMQActivity = new DsfPublishRabbitMQActivity();

            Mock<IResourceCatalog> resourceCatalog = new Mock<IResourceCatalog>();
            Mock<RabbitMQSource> rabbitMQSource = new Mock<RabbitMQSource>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            PrivateObject p = new PrivateObject(dsfPublishRabbitMQActivity);
            p.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string>()) as List<string>;

            //------------Assert Results-------------------------
            if(result != null)
            {
                Assert.AreEqual(result[0], "Failure: Queue Name and Message are required.");
            }
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfPublishRabbitMQActivity_Execute")]
        public void DsfPublishRabbitMQActivity_Execute_Failure_InvalidParams()
        {
            //------------Setup for test--------------------------
            DsfPublishRabbitMQActivity dsfPublishRabbitMQActivity = new DsfPublishRabbitMQActivity();

            Mock<IResourceCatalog> resourceCatalog = new Mock<IResourceCatalog>();
            Mock<RabbitMQSource> rabbitMQSource = new Mock<RabbitMQSource>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            PrivateObject p = new PrivateObject(dsfPublishRabbitMQActivity);
            p.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string> { { "Param1", "Blah1" }, { "Param2", "Blah2" } }) as List<string>;

            //------------Assert Results-------------------------
            if(result != null)
            {
                Assert.AreEqual(result[0], "Failure: Queue Name and Message are required.");
            }
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfPublishRabbitMQActivity_Execute")]
        [ExpectedException(typeof(Exception))]
        public void DsfPublishRabbitMQActivity_Execute_Failure_NullException()
        {
            //------------Setup for test--------------------------
            DsfPublishRabbitMQActivity dsfPublishRabbitMQActivity = new DsfPublishRabbitMQActivity();

            Mock<IResourceCatalog> resourceCatalog = new Mock<IResourceCatalog>();
            Mock<RabbitMQSource> rabbitMQSource = new Mock<RabbitMQSource>();
            Mock<ConnectionFactory> connectionFactory = new Mock<ConnectionFactory>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);
            connectionFactory.Setup(c => c.CreateConnection()).Returns<IConnection>(null);

            PrivateObject p = new PrivateObject(dsfPublishRabbitMQActivity);
            p.SetProperty("ConnectionFactory", connectionFactory.Object);
            p.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", "Q1" }, { "Message", "Test message" } });

            //------------Assert Results-------------------------
            Assert.Fail("Exception not thrown");
        }
    }
}