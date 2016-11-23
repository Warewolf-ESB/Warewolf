using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using Dev2.DynamicServices;
using Dev2.Runtime.Interfaces;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.RabbitMQ.Consume
{
    [TestClass]
    public class DsfConsumeRabbitMQActivityTests
    {
        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Construct")]
        public void DsfConsumeRabbitMQActivity_Constructor_Should_SetsDefaultPropertyValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null
            };
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
        public void PerformExecution_Given_NoSource_Should_Return_NullSourceException()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns<RabbitMQSource>(null);

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = privateObject.Invoke("PerformExecution", new Dictionary<string, string>()) as List<string>;

            //------------Assert Results-------------------------
            if(result != null)
            {
                Assert.AreEqual(result[0], "Failure: Source has been deleted.");
            }
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_Blank_QueueShould_Return_BlankQueueException()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();

            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = privateObject.Invoke("PerformExecution", new Dictionary<string, string>()) as List<string>;

            //------------Assert Results-------------------------
            if(result != null)
            {
                Assert.AreEqual(result[0], "Failure: Queue Name is required.");
            }
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_NoQueue_Shouold_Return_Exception()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();

            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            var p = new PrivateObject(dsfConsumeRabbitMQActivity);
            p.SetProperty("ResourceCatalog", resourceCatalog.Object);
            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string> { { "Param1", "Blah1" }, { "Param2", "Blah2" } }) as List<string>;
            //------------Assert Results-------------------------
            if(result != null)
            {
                Assert.AreEqual(result[0], "Failure: Queue Name is required.");
            }
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        [ExpectedException(typeof(Exception))]
        public void PerformExecution_Given_No_Source_Should_Return_NullException()
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
            privateObject.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", "Q1" } });
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
            privateObject.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", queueName } });
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, string.Format("Nothing in the Queue : {0}", queueName));
            }
            //------------Assert Results-------------------------            
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_DsfBaseActivity_Inputs_ReturnsInputsInTheDebug()
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
            channel.Setup(c => c.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()));

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ConnectionFactory", connectionFactory.Object);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            privateObject.SetProperty("Channel", channel.Object);

            dsfConsumeRabbitMQActivity.QueueName = queueName;
            dsfConsumeRabbitMQActivity.ReQueue = true;
            dsfConsumeRabbitMQActivity.Acknowledge = true;

            dsfConsumeRabbitMQActivity.Response = "[[response]]";

            dsfConsumeRabbitMQActivity.GetDebugInputs(It.IsAny<IExecutionEnvironment>(), It.IsAny<int>());
            dsfConsumeRabbitMQActivity.GetDebugOutputs(It.IsAny<IExecutionEnvironment>(), It.IsAny<int>());
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------            
        }
        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void DsfConsumeRabbitMQActivity_Execute_DsfBaseActivity_MethodsGivenInputs()
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
            channel.Setup(c => c.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()));

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ConnectionFactory", connectionFactory.Object);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            privateObject.SetProperty("Channel", channel.Object);

            var dataList = new ExecutionEnvironment();
            dataList.AllErrors.Add("Some Error");

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            dsfConsumeRabbitMQActivity.QueueName = queueName;
            dsfConsumeRabbitMQActivity.ReQueue = true;
            dsfConsumeRabbitMQActivity.Prefetch = "2";

            dsfConsumeRabbitMQActivity.Response = "[[response]]";
            dsfConsumeRabbitMQActivity._messages = new List<string>{ "Message"};

            var debugInputs = dsfConsumeRabbitMQActivity.GetDebugInputs(dataList, It.IsAny<int>());
            var debugOutputs = dsfConsumeRabbitMQActivity.GetDebugOutputs(dataList, It.IsAny<int>());
            privateObject.Invoke("AssignResult", dataObj, It.IsAny<int>());
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(debugInputs.Count > 0);
            Assert.IsTrue(debugOutputs.Count > 0);
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_UnExisting_Queue_Returns_QeueuNotFoundException()
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
                privateObject.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", queueName } });
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, string.Format("Queue {0} not found", queueName));
            }
            //------------Assert Results-------------------------            
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_ConsumeAndRequeue_From_UnExisting_Queue_ReturnQeueuNotFoundException()
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
            channel.Setup(c => c.BasicGet(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws(new Exception(string.Format("Queue '{0}' not found", queueName)));

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ConnectionFactory", connectionFactory.Object);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            privateObject.SetProperty("Channel", channel.Object);

            dsfConsumeRabbitMQActivity.ReQueue = true;
            //------------Execute Test---------------------------
            try
            {
                privateObject.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", queueName } });
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, string.Format("Queue {0} not found", queueName));
            }
            //------------Assert Results-------------------------            
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_EmptyQueue_ExecuteAndReqeue_Empty_Queue_Exception()
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

            dsfConsumeRabbitMQActivity.ReQueue = true;
            //------------Execute Test---------------------------
            try
            {
                privateObject.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", queueName } });
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, string.Format("Nothing in the Queue : {0}", queueName));
            }
            //------------Assert Results-------------------------            
        }
    }
}