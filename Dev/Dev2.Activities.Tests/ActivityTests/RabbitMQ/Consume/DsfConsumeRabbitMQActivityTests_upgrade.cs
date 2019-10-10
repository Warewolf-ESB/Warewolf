using Dev2.Activities.RabbitMQ.Consume;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using RabbitMQ.Client.Framing;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using System.Linq;
using Dev2.Common.State;
using Moq;
using Warewolf.Driver.RabbitMQ;
using Warewolf.Triggers;
using RabbitMQSource = Dev2.Data.ServiceModel.RabbitMQSource;
using RabbitMQ.Client.Events;

namespace Dev2.Tests.Activities.ActivityTests.RabbitMQ.Consume
{
    [TestClass]
    public class DsfConsumeRabbitMQActivityTests_upgrade : BaseActivityUnitTest
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_Constructor_Should_SetsDefaultPropertyValues1()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_ConstructorRC_Should_SetsDefaultPropertyValuesIsObject1()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResourceCatalog>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfConsumeRabbitMQActivity);
            Assert.IsFalse(dsfConsumeRabbitMQActivity.IsObject);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_ConstructorRM_Should_SetsDefaultPropertyValuesIsObject1()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResponseManager>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfConsumeRabbitMQActivity);
            Assert.IsFalse(dsfConsumeRabbitMQActivity.IsObject);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_Prefetch_Should_SetsDefaultPropertyValuesIsObject1()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade();
            //------------Assert Results-------------------------
            Assert.AreEqual("1", dsfConsumeRabbitMQActivity.Prefetch);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void AssignResult_GivenIsObject_ShouldSetResponsemanagerIsObject1()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IResponseManager>();
            mock.SetupProperty(manager => manager.IsObject);
            var dataObj = new Mock<IDSFDataObject>();
            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null,
                IsObject = true
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfConsumeRabbitMQActivity.IsObject);
            //---------------Execute Test ----------------------
            dsfConsumeRabbitMQActivity.TestAssignResult(dataObj.Object, 1);
            //---------------Test Result -----------------------
            mock.VerifySet(manager => manager.IsObject = true, Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void AssignResult_GivenIsObject_ShouldSetResponsemanagerObjectName1()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IResponseManager>();
            mock.SetupProperty(manager => manager.ObjectName);
            var dataObj = new Mock<IDSFDataObject>();
            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null,
                IsObject = true,
                ObjectName = "a"
            };

            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfConsumeRabbitMQActivity.IsObject);
            //---------------Execute Test ----------------------
            dsfConsumeRabbitMQActivity.TestAssignResult(dataObj.Object, 1);
            //---------------Test Result -----------------------
            mock.VerifySet(manager => manager.ObjectName = "a", Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void AssignResult_GivenIsObjectNoMessages_ShouldNotSetResponsemanagerPushResponeToEnvironment1()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IResponseManager>();
            mock.SetupProperty(manager => manager.ObjectName);

            var dataObj = new Mock<IDSFDataObject>();
            mock.Setup(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dataObj.Object, It.IsAny<bool>())).Verifiable();
            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null,
                IsObject = true,
                ObjectName = "a"
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfConsumeRabbitMQActivity.IsObject);
            //---------------Execute Test ----------------------
            dsfConsumeRabbitMQActivity.TestAssignResult(dataObj.Object, 1);
            //---------------Test Result -----------------------
            mock.Verify(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dataObj.Object, It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void AssignResult_GivenIsObjectOneMessages_ShouldSetResponsemanagerPushResponeToEnvironment1()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IResponseManager>();
            mock.SetupProperty(manager => manager.ObjectName);

            var dataObj = new Mock<IDSFDataObject>();
            mock.Setup(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dataObj.Object)).Verifiable();
            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null,
                IsObject = true,
                ObjectName = "a",
                _messages = new List<string>(new[] { "a" })
            };

            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfConsumeRabbitMQActivity.IsObject);
            //---------------Execute Test ----------------------
            dsfConsumeRabbitMQActivity.TestAssignResult(dataObj.Object, 1);
            //---------------Test Result -----------------------
            mock.Verify(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dataObj.Object), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void AssignResult_GivenIsObjectManyMessages_ShouldSetResponsemanagerPushResponeToEnvironment1()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IResponseManager>();
            mock.SetupProperty(manager => manager.ObjectName);

            var dataObj = new Mock<IDSFDataObject>();
            mock.Setup(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dataObj.Object)).Verifiable();
            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null,
                IsObject = true,
                ObjectName = "a",
                _messages = new List<string>(new[] { "a", "b" })
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfConsumeRabbitMQActivity.IsObject);
            //---------------Execute Test ----------------------
            dsfConsumeRabbitMQActivity.TestAssignResult(dataObj.Object, 1);
            //---------------Test Result -----------------------
            mock.Verify(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dataObj.Object), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_ConstructorRC_Should_SetsDefaultPropertyValuesObjectName1()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResponseManager>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfConsumeRabbitMQActivity);
            Assert.AreEqual(null, dsfConsumeRabbitMQActivity.ObjectName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_ConstructorRM_Should_SetsDefaultPropertyValuesObjectName1()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResponseManager>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfConsumeRabbitMQActivity);
            Assert.AreEqual(null, dsfConsumeRabbitMQActivity.ObjectName);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_Constructor_Should_SetsInitialiseResponseManager1()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResponseManager>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null
            };
            //------------Assert Results-------------------------
            Assert.IsNotNull(dsfConsumeRabbitMQActivity.ResponseManager);
            Assert.IsTrue(ReferenceEquals(mock.Object, dsfConsumeRabbitMQActivity.ResponseManager));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_ConstructorRC_Should_SetsDefaultPropertyValues1()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResourceCatalog>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade(mock.Object)
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
            Assert.IsNull(dsfConsumeRabbitMQActivity.Consumer);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_Given_NoSource_Should_Return_NullSourceException1()
        {
            //------------Setup for test--------------------------
            var resourceCatalog = new Mock<IResourceCatalog>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade
            {
                ResourceCatalog = resourceCatalog.Object
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns<RabbitMQSource>(null);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            if (dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>()) is List<string> result)
            {
                Assert.AreEqual(result[0], "Failure: Source has been deleted.");
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_Given_Blank_QueueShould_Return_BlankQueueException_NoTimeout1()
        {
            //------------Setup for test--------------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade
            {
                ResourceCatalog = resourceCatalog.Object
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            if (dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>()) is List<string> result)
            {
                Assert.AreEqual(result[0], "Failure: Queue Name is required.");
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_Given_Blank_QueueShould_Return_BlankQueueException_Timeout1()
        {
            //------------Setup for test--------------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade()
            {
                ResourceCatalog = resourceCatalog.Object,
                TimeOut = "1"
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            if (dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>()) is List<string> result)
            {
                Assert.AreEqual(result[0], "Failure: Queue Name is required.");
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_Given_NoQueue_Shouold_Return_Exception_NoTimeout1()
        {
            //------------Setup for test--------------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade
            {
                ResourceCatalog = resourceCatalog.Object
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            if (dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string> { { "Param1", "Blah1" }, { "Param2", "Blah2" } }) is List<string> result)
            {
                Assert.AreEqual(result[0], "Failure: Queue Name is required.");
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_Given_NoQueue_Shouold_Return_Exception_NoTimeout_Timeout1()
        {
            //------------Setup for test--------------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade()
            {
                ResourceCatalog = resourceCatalog.Object,
                TimeOut = "1"
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            if (dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string> { { "Param1", "Blah1" }, { "Param2", "Blah2" } }) is List<string> result)            {
                Assert.AreEqual(result[0], "Failure: Queue Name is required.");
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_Given_DsfBaseActivity_Inputs_ReturnsInputsInTheDebug1()
        {
            //------------Setup for test--------------------------
            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade
            {
                ResourceCatalog = resourceCatalog.Object,
                QueueName = queueName,
                ReQueue = true,
                Acknowledge = true,
                Response = "[[response]]",
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            dsfConsumeRabbitMQActivity.GetDebugInputs(It.IsAny<IExecutionEnvironment>(), It.IsAny<int>());
            dsfConsumeRabbitMQActivity.GetDebugOutputs(It.IsAny<IExecutionEnvironment>(), It.IsAny<int>());
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_PerformExecutionOnChannel_ReQueue_True_OnEmptyQueue_NoSuccess()
        {
            //------------Setup for test--------------------------
            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();
            var channel = new Mock<IModel>();
            var queueConnection = new Mock<IQueueConnection>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade(new EventingBasicConsumerFactory(), new ManualResetEventFactory(), new QueueConsumer(), channel.Object)
            {
                ResourceCatalog = resourceCatalog.Object,
                QueueName = queueName,
                Prefetch = "1",
                ReQueue = true,
                Acknowledge = true,
            };

            var source = new RabbitMQSource
            {
                HostName = "rsaklfsvrdev.dev2.local",
                UserName = "test",
                Password = "test"
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(source);

            var response = dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>() { { "QueueName", queueName }, { "Prefetch", "1" } });
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected: 0, actual: response.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_PerformExecutionOnChannel_ReQueue_True_OnEmptyQueue_Success()
        {
            //------------Setup for test--------------------------
            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();
            var channel = new Mock<IModel>();
            var queueConnection = new Mock<IQueueConnection>();

            var getResult = new BasicGetResult((ulong)1, false, "testExchannge", "testRoutingKey", (uint)1, new BasicProperties(), Encoding.UTF8.GetBytes("test message"));

            channel.Setup(o => o.BasicGet(It.IsAny<string>(), It.IsAny<bool>())).Returns(getResult);

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade(new EventingBasicConsumerFactory(), new ManualResetEventFactory(), new QueueConsumer(), channel.Object)
            {
                ResourceCatalog = resourceCatalog.Object,
                QueueName = queueName,
                Prefetch = "1",
                ReQueue = true,
                Acknowledge = true,
            };

            var source = new RabbitMQSource
            {
                HostName = "rsaklfsvrdev.dev2.local",
                UserName = "test",
                Password = "test"
            };
            
            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(source);

            var response = dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>() { { "QueueName", queueName }, { "Prefetch", "1" } });
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected: "test message", actual: response[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_PerformExecutionOnChannel_ReQueue_False_OnEmptyQueue_NoTimeOut_Success()
        {
            //------------Setup for test--------------------------
            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();
            var channel = new Mock<IModel>();
            var queueConnection = new Mock<IQueueConnection>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade(new EventingBasicConsumerFactory(), new ManualResetEventFactory(), new QueueConsumer(), channel.Object)
            {
                ResourceCatalog = resourceCatalog.Object,
                QueueName = queueName,
                Prefetch = "1",
                ReQueue = false,
                Acknowledge = true,
            };

            var source = new RabbitMQSource
            {
                HostName = "rsaklfsvrdev.dev2.local",
                UserName = "test",
                Password = "test"
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(source);

            var response = dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>() { { "QueueName", queueName }, { "Prefetch", "1" } });
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected: 0, actual: response.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_PerformExecutionOnChannel_ReQueue_False_OnEmptyQueue_WithTimeOut_Success()
        {
            //------------Setup for test--------------------------
            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();
            var channel = new Mock<IModel>();
            var mockResetFactory = new Mock<IManualResetEventFactory>();
            var mockResetWrapper = new Mock<IManualResetEventWrapper>();
            var mockConsumerFactory = new Mock<IEventingBasicConsumerFactory>();

            var consumer = new QueueConsumer();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade(mockConsumerFactory.Object, mockResetFactory.Object, consumer, channel.Object)
            {
                ResourceCatalog = resourceCatalog.Object,
                QueueName = queueName,
                Prefetch = "1",
                TimeOut = "1",
                ReQueue = false,
                Acknowledge = true,
            };

            var source = new RabbitMQSource
            {
                HostName = "rsaklfsvrdev.dev2.local",
                UserName = "test",
                Password = "test"
            };

            var testConsumerWrapper = new TestEventingBasicConsumerWrapper(channel.Object)
            {
                ConsumerTag = "testConsumerTag"
            };
            var eventArgs = new BasicDeliverEventArgs("testConsumerTeg", (ulong)1, false, "testExchange", "testRoutingKey", new BasicProperties(), Encoding.UTF8.GetBytes("test message"));

            mockConsumerFactory.Setup(o => o.New(It.IsAny<IModel>())).Returns(testConsumerWrapper);
            mockResetWrapper.Setup(o => o.WaitOne(It.IsAny<TimeSpan>())).Returns(false);
            mockResetFactory.Setup(o => o.New(It.IsAny<bool>())).Returns(mockResetWrapper.Object);
            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(source);

            //------------Execute Test---------------------------
            var responseBefore = dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>() { { "QueueName", queueName }, { "Prefetch", "1" } });
            testConsumerWrapper.TiggerRecievedForTests(eventArgs);
            var responseAfter = dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>() { { "QueueName", queueName }, { "Prefetch", "1" } });

            //------------Assert Results-------------------------

            mockResetWrapper.Verify(o => o.Set(), Times.Once());
            channel.Verify(o => o.BasicCancel(It.IsAny<string>()), Times.AtLeast(2));
            channel.Verify(o => o.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());

            Assert.AreEqual(expected: "test message", consumer.Messages[0]);
            Assert.AreEqual(expected: 1, consumer.Messages.Count());

            Assert.AreEqual(expected: 0, actual: responseBefore.Count); 
            Assert.AreEqual(expected: 1, actual: responseAfter.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_PerformExecutionOnChannel_ReQueue_False_NotEmptyQueue_WithTimeOut_Success()
        {
            //------------Setup for test--------------------------
            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();
            var channel = new Mock<IModel>();
            var queueConnection = new Mock<IQueueConnection>();

            var getResult = new BasicGetResult((ulong)1, false, "testExchannge", "testRoutingKey", (uint)1, new BasicProperties(), Encoding.UTF8.GetBytes("test message"));

            channel.Setup(o => o.BasicGet(It.IsAny<string>(), It.IsAny<bool>())).Returns(getResult);

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade(new EventingBasicConsumerFactory(), new ManualResetEventFactory(), new QueueConsumer(), channel.Object)
            {
                ResourceCatalog = resourceCatalog.Object,
                QueueName = queueName,
                Prefetch = "1",
                TimeOut = "1",
                ReQueue = false,
                Acknowledge = true,
            };

            var source = new RabbitMQSource
            {
                HostName = "rsaklfsvrdev.dev2.local",
                UserName = "test",
                Password = "test"
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(source);

            var response = dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>() { { "QueueName", queueName }, { "Prefetch", "1" } });
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected: 0, actual: response.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_Execute_DsfBaseActivity_MethodsGivenInputs1()
        {
            //------------Setup for test--------------------------
            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade
            {
                ResourceCatalog = resourceCatalog.Object,
                QueueName = queueName,
                ReQueue = true,
                Prefetch = "2",
                Response = "[[response]]",
                _messages = new List<string> { "Message" }
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            var dataList = new ExecutionEnvironment();
            dataList.AllErrors.Add("Some Error");

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var debugInputs = dsfConsumeRabbitMQActivity.GetDebugInputs(dataList, It.IsAny<int>());
            var debugOutputs = dsfConsumeRabbitMQActivity.GetDebugOutputs(dataList, It.IsAny<int>());
            dsfConsumeRabbitMQActivity.TestAssignResult(dataObj, It.IsAny<int>());
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(debugInputs.Count > 0);
            Assert.IsTrue(debugOutputs.Count > 0);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_ExecuteIsObject_DsfBaseActivity_MethodsGivenOjectOutput1()
        {
            //------------Setup for test--------------------------
            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade
            {
                QueueName = queueName,
                ReQueue = true,
                Prefetch = "2",
                Response = "[[response]]",
                _messages = new List<string> { "Message" },
                IsObject = true,
                ObjectName = "[[@Human]]",
            };

            var source = new RabbitMQSource
            {
                HostName = "rsaklfsvrdev.dev2.local",
                UserName = "test",
                Password = "test"
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(source);

            var dataList = new ExecutionEnvironment();
            dataList.AllErrors.Add("Some Error");

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var debugOutputs = dsfConsumeRabbitMQActivity.GetDebugOutputs(dataList, It.IsAny<int>());
            dsfConsumeRabbitMQActivity.TestAssignResult(dataObj, It.IsAny<int>());
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(debugOutputs.Count > 0);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_AssignResult_IsObject_False_UpdateEnvironmentRecordsets_EnvironmentAssign_Success()
        {
            //------------Setup for test--------------------------
            const string queueName = "Q1";
            var resourceCatalog = new Mock<IResourceCatalog>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade
            {
                QueueName = queueName,
                ReQueue = true,
                Prefetch = "2",
                Response = "response",
                _messages = new List<string> { "Message" },
                IsObject = false,
                ObjectName = "[[@Human]]",
            };

            var source = new RabbitMQSource
            {
                HostName = "rsaklfsvrdev.dev2.local",
                UserName = "test",
                Password = "test"
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(source);

            var dataList = new ExecutionEnvironment();
            dataList.AllErrors.Add("Some Error");

            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());

            var debugOutputs = dsfConsumeRabbitMQActivity.GetDebugOutputs(dataList, It.IsAny<int>());
            //------------Execute Test---------------------------
            dsfConsumeRabbitMQActivity.TestAssignResult(dataObj, It.IsAny<int>());
            //------------Assert Results-------------------------
            Assert.IsTrue(debugOutputs.Count > 0);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void PerformSerialization_ShouldNotError1()
        {
            //------------Setup for test--------------------------
            var resourceCatalog = new Mock<IResourceCatalog>();

            var dsfConsumeRabbitMQActivity = new TestDsfConsumeRabbitMQActivity_upgrade
            {
                ResourceCatalog = resourceCatalog.Object,
                ReQueue = true
            };

            var rabbitMQSource = new RabbitMQSource
            {
                HostName = "rsaklfsvrdev.dev2.local",
                Port = 5672,
                UserName = "test",
                Password = "test"
            };

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource);
            
            try
            {
                dsfConsumeRabbitMQActivity.TestPerformExecution(new Dictionary<string, string> { { "QueueName", "HuggsTest" } });
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Queue Q1 not found", ex.Message);
            }
            var serializer = new Dev2JsonSerializer();

            //------------Execute Test---------------------------
            try
            {
                var serializedAct = serializer.SerializeToBuilder(dsfConsumeRabbitMQActivity);
                //------------Assert Results-------------------------
                Assert.IsNotNull(serializedAct);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_GetOutputs_ShouldIncludeResponse1()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade
            {
                Result = "[[res]]",
                Response = "[[data]]"
            };
            //------------Execute Test---------------------------
            var outputs = dsfConsumeRabbitMQActivity.GetOutputs();
            //------------Assert Results-------------------------
            Assert.IsNotNull(outputs);
            Assert.AreEqual(2, outputs.Count);
            Assert.AreEqual("[[data]]", outputs[0]);
            Assert.AreEqual("[[res]]", outputs[1]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_ShouldSerializeChannel_ShouldReturnFalse1()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade();
            //------------Execute Test---------------------------
            var shouldSerialize = dsfConsumeRabbitMQActivity.ShouldSerializeChannel();
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldSerialize);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_ShouldSerializeConnection_ShouldReturnFalse1()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade();
            //------------Execute Test---------------------------
            var shouldSerialize = dsfConsumeRabbitMQActivity.ShouldSerializeConnection();
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldSerialize);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_ShouldSerializeConnectionFactory_ShouldReturnFalse1()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade();
            //------------Execute Test---------------------------
            var shouldSerialize = dsfConsumeRabbitMQActivity.ShouldSerializeConnectionFactory();
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldSerialize);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_ShouldSerializeConsumer_ShouldReturnFalse1()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade();
            //------------Execute Test---------------------------
            var shouldSerialize = dsfConsumeRabbitMQActivity.ShouldSerializeConsumer();
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldSerialize);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_ShouldSerializeRabbitSource_ShouldReturnFalse1()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity_upgrade();
            //------------Execute Test---------------------------
            var shouldSerialize = dsfConsumeRabbitMQActivity.ShouldSerializeRabbitSource();
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldSerialize);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_GetState_ReturnsStateVariable1()
        {
            //---------------Set up test pack-------------------
            var sourceId = Guid.NewGuid();
            //------------Setup for test--------------------------
            var act = new DsfConsumeRabbitMQActivity_upgrade
            {
                QueueName = "bob",
                Acknowledge = true,
                IsObject = false,
                ObjectName = "[[@result]]",
                Prefetch = "10",
                RabbitMQSourceResourceId = sourceId,
                ReQueue = false,
                TimeOut = "100",
                Result = "[[res]]",
                Response = "[[data]]"
            };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(10, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "QueueName",
                    Type = StateVariable.StateType.Input,
                    Value = "bob"
                },
                new StateVariable
                {
                    Name = "Acknowledge",
                    Type = StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name = "IsObject",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name = "ObjectName",
                    Type = StateVariable.StateType.Input,
                    Value = "[[@result]]"
                },
                new StateVariable
                {
                    Name = "Prefetch",
                    Type = StateVariable.StateType.Input,
                    Value = "10"
                },
                new StateVariable
                {
                    Name = "RabbitMQSourceResourceId",
                    Type = StateVariable.StateType.Input,
                    Value = sourceId.ToString()
                },
                new StateVariable
                {
                    Name = "ReQueue",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name = "TimeOut",
                    Type = StateVariable.StateType.Input,
                    Value = "100"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "[[res]]"
                },
                new StateVariable
                {
                    Name="Response",
                    Type = StateVariable.StateType.Output,
                    Value = "[[data]]"
                }
            };

            var iter = act.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfConsumeRabbitMQActivity_upgrade))]
        public void DsfConsumeRabbitMQActivity_PerformExecution_ExecuteWithTimeout_StartConsumingWithTimeOut_ConsumesSentMessage()
        {
            //-------------------------------Arrange------------------------------
            var mockResetFactory = new Mock<IManualResetEventFactory>();
            var mockResetWrapper = new Mock<IManualResetEventWrapper>();
            var mockConsumerFactory = new Mock<IEventingBasicConsumerFactory>();
            var mockChannel = new Mock<IModel>();

            var eventArgs = new BasicDeliverEventArgs("testConsumerTeg", (ulong)1, false, "testExchange", "testRoutingKey", new BasicProperties(), Encoding.UTF8.GetBytes("test message"));
            var consumer = new QueueConsumer();

            var config = new RabbitConfig
            {
                Durable = true,
                QueueName = "q1",
                PrefetchSize = 0,
                PrefetchCount = 1,
                Acknwoledge = false
            };

            var testConsumerWrapper = new TestEventingBasicConsumerWrapper(mockChannel.Object)
            {
                ConsumerTag = "testConsumerTag"
            };

            mockConsumerFactory.Setup(o => o.New(It.IsAny<IModel>())).Returns(testConsumerWrapper);
            mockResetWrapper.Setup(o => o.WaitOne(It.IsAny<TimeSpan>())).Returns(false);
            mockResetFactory.Setup(o => o.New(It.IsAny<bool>())).Returns(mockResetWrapper.Object);

            var resource = new RabbitMQSource
            {
                HostName = "rsaklfsvrdev.dev2.local",
                UserName = "test",
                Password = "test"
            };

            //-------------------------------Act----------------------------------
            using (var connection = resource.NewConnection(mockChannel.Object))
            {
                connection.StartConsumingWithTimeOut(mockConsumerFactory.Object, mockResetFactory.Object, config, consumer, 10);
            }

            testConsumerWrapper.TiggerRecievedForTests(eventArgs);
            //-------------------------------Assert-------------------------------
            mockResetWrapper.Verify(o => o.Set(), Times.Once);
            mockChannel.Verify(o => o.BasicCancel(It.IsAny<string>()), Times.Once);
            mockChannel.Verify(o => o.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once);

            Assert.AreEqual(expected: "test message", consumer.Messages[0]);
            Assert.AreEqual(expected: 1, consumer.Messages.Count());
        }
    }


    class TestDsfConsumeRabbitMQActivity_upgrade : DsfConsumeRabbitMQActivity_upgrade
    {
        public TestDsfConsumeRabbitMQActivity_upgrade()
        {

        }

        public TestDsfConsumeRabbitMQActivity_upgrade(IEventingBasicConsumerFactory consumerFactory, IManualResetEventFactory eventFactory, QueueConsumer consumer, IModel channel)
            :base(consumerFactory, eventFactory, consumer, channel)
        {

        }
        
        public TestDsfConsumeRabbitMQActivity_upgrade(IResponseManager responseManager) 
            : base(responseManager)
        {
        }

        public List<string> TestPerformExecution(Dictionary<string, string> evaluatedValues)
        {
            return base.PerformExecution(evaluatedValues);
        }

        public List<string> TestPerformExecution(IModel channel, Dictionary<string, string> evaluatedValues)
        {
            return base.PerformExecution(evaluatedValues);
        }

        public void TestAssignResult(IDSFDataObject dataObject, int update)
        {
            base.AssignResult(dataObject, update);
        }
    }

    internal class TestEventingBasicConsumerWrapper : IEventingBasicConsumerWrapper
    {
        private IModel @object;

        public TestEventingBasicConsumerWrapper(IModel @object)
        {
            this.@object = @object;
        }

        public string ConsumerTag { get; set; }

        public IModel Model => throw new NotImplementedException();

        public event EventHandler<BasicDeliverEventArgs> Received;
        public event EventHandler<ConsumerEventArgs> ConsumerCancelled;

        public void TiggerRecievedForTests(BasicDeliverEventArgs basicDeliverEventArgs)
        {
            Received?.Invoke(this, basicDeliverEventArgs);
        }

        public void TiggerConsumerCancelled(ConsumerEventArgs consumerEventArgs)
        {
            ConsumerCancelled?.Invoke(this, consumerEventArgs);
        }

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