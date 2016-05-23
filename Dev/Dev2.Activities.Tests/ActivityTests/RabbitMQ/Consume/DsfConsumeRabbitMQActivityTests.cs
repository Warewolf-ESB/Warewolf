using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.RabbitMQ.Consume
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DsfConsumeRabbitMQActivityTests
    {
        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Construct")]
        public void DsfConsumeRabbitMQActivity_Construct_Paramterless_SetsDefaultPropertyValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();
            dsfConsumeRabbitMQActivity.RabbitMQSourceResourceId = Guid.Empty;
            dsfConsumeRabbitMQActivity.QueueName = string.Empty;
            dsfConsumeRabbitMQActivity.Prefetch = null;
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfConsumeRabbitMQActivity);
            Assert.AreEqual("RabbitMQ Consume", dsfConsumeRabbitMQActivity.DisplayName);
            Assert.IsTrue(string.IsNullOrEmpty(dsfConsumeRabbitMQActivity.QueueName));
            Assert.AreEqual(Guid.Empty, dsfConsumeRabbitMQActivity.RabbitMQSourceResourceId);
            Assert.IsNull(dsfConsumeRabbitMQActivity.Prefetch);
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void DsfConsumeRabbitMQActivity_Execute_Failure_NullSource()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns<RabbitMQSource>(null);

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = privateObject.Invoke("PerformExecution", new Dictionary<string, string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(result.ToString(), "Failure: Source has been deleted.");
        }       

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void DsfConsumeRabbitMQActivity_Execute_Failure_NoParams()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();

            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = privateObject.Invoke("PerformExecution", new Dictionary<string, string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(result.ToString(), "Failure: Queue Name is required.");
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void DsfConsumeRabbitMQActivity_Execute_Failure_InvalidParams()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();

            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            var p = new PrivateObject(dsfConsumeRabbitMQActivity);
            p.SetProperty("ResourceCatalog", resourceCatalog.Object);
            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string> { { "Param1", "Blah1" }, { "Param2", "Blah2" } });
            //------------Assert Results-------------------------
            Assert.AreEqual(result.ToString(), "Failure: Queue Name is required.");
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        [ExpectedException(typeof(Exception))]
        public void DsfConsumeRabbitMQActivity_Execute_Failure_NullException()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();

            
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();
            var connectionFactory = new Mock<ConnectionFactory>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);
            connectionFactory.Setup(c => c.CreateConnection()).Returns<IConnection>(null);

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ConnectionFactory", connectionFactory.Object);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = privateObject.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", "Q1" }});
            //------------Assert Results-------------------------
            Assert.Fail("Exception not thrown");
        }


        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]        
        public void DsfConsumeRabbitMQActivity_Execute_Empty_Queue_Exception()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();

            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();
            var connectionFactory = new Mock<ConnectionFactory>();
            var connection = new Mock<IConnection>();
            var channel = new Mock<IModel>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);
            connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            connection.Setup(c => c.CreateModel()).Returns(channel.Object);
            channel.Setup(c => c.BasicQos(0, 1, false));
            channel.Setup(c => c.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<QueueingBasicConsumer>()));

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ConnectionFactory", connectionFactory.Object);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            try
            {
                var result = privateObject.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", queueName } });
            }
            catch(Exception ex)
            {
                Assert.AreEqual(ex.Message, string.Format("Nothing in the Queue : {0}", queueName));
            }            
            //------------Assert Results-------------------------            
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void DsfConsumeRabbitMQActivity_Execute_Requeue_Should_Not_Remove_MessageFrom_Queue()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();

            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();
            var connectionFactory = new Mock<ConnectionFactory>();
            var connection = new Mock<IConnection>();
            var channel = new Mock<IModel>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);
            connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            connection.Setup(c => c.CreateModel()).Returns(channel.Object);
            channel.Setup(c => c.BasicQos(0, 1, false));
            channel.Setup(c => c.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<QueueingBasicConsumer>()));
            channel.Setup(c=>c.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()));

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ConnectionFactory", connectionFactory.Object);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            privateObject.SetProperty("Channel", channel.Object);
            //------------Execute Test---------------------------                        
            dsfConsumeRabbitMQActivity.ReQueue = true;
            var result = privateObject.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", queueName } });
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]        
        public void DsfConsumeRabbitMQActivity_Execute_Consume_From_UnExisting_Queue_Exception()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();

            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();
            var connectionFactory = new Mock<ConnectionFactory>();
            var connection = new Mock<IConnection>();
            var channel = new Mock<IModel>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);
            connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            connection.Setup(c => c.CreateModel()).Returns(channel.Object);
            channel.Setup(c => c.BasicQos(0, 1, false));
            channel.Setup(c => c.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<QueueingBasicConsumer>()))
                .Throws(new Exception(string.Format("Queue '{0}' not found", queueName)));

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ConnectionFactory", connectionFactory.Object);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            privateObject.SetProperty("Channel", channel.Object);
            //------------Execute Test---------------------------
            try
            {
                var result = privateObject.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", queueName } });
            }
            catch(Exception ex)
            {
                Assert.AreEqual(ex.Message, string.Format("Queue '{0}' not found", queueName));
            }            
            //------------Assert Results-------------------------            
        }
    }
}