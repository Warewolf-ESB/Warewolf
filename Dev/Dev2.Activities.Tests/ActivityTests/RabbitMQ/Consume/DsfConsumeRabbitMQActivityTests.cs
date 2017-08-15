using Dev2.Activities.RabbitMQ.Consume;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
using System.Reflection;



namespace Dev2.Tests.Activities.ActivityTests.RabbitMQ.Consume
{
    [TestClass]
    public class DsfConsumeRabbitMQActivityTests : BaseActivityUnitTest
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
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfConsumeRabbitMQActivity_Construct")]
        public void DsfConsumeRabbitMQActivity_ConstructorRC_Should_SetsDefaultPropertyValuesIsObject()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResourceCatalog>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity(mock.Object)
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
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfConsumeRabbitMQActivity_Construct")]
        public void DsfConsumeRabbitMQActivity_ConstructorRM_Should_SetsDefaultPropertyValuesIsObject()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResponseManager>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity(mock.Object)
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
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfConsumeRabbitMQActivity_Prefetch")]
        public void DsfConsumeRabbitMQActivity_Prefetch_Should_SetsDefaultPropertyValuesIsObject()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();
            //------------Assert Results-------------------------
            Assert.AreEqual("1", dsfConsumeRabbitMQActivity.Prefetch);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AssignResult_GivenIsObject_ShouldSetResponsemanagerIsObject()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IResponseManager>();
            mock.SetupProperty(manager => manager.IsObject);
            var dstaObj = new Mock<IDSFDataObject>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null,
                IsObject = true
            };
            PrivateObject privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfConsumeRabbitMQActivity.IsObject);
            //---------------Execute Test ----------------------
            privateObject.Invoke("AssignResult", dstaObj.Object, 1);
            //---------------Test Result -----------------------
            mock.VerifySet(manager => manager.IsObject = true, Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AssignResult_GivenIsObject_ShouldSetResponsemanagerObjectName()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IResponseManager>();
            mock.SetupProperty(manager => manager.ObjectName);
            var dstaObj = new Mock<IDSFDataObject>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null,
                IsObject = true,
                ObjectName = "a"
            };
            PrivateObject privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfConsumeRabbitMQActivity.IsObject);
            //---------------Execute Test ----------------------
            privateObject.Invoke("AssignResult", dstaObj.Object, 1);
            //---------------Test Result -----------------------
            mock.VerifySet(manager => manager.ObjectName = "a", Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AssignResult_GivenIsObjectNoMessages_ShouldNotSetResponsemanagerPushResponeToEnvironment()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IResponseManager>();
            mock.SetupProperty(manager => manager.ObjectName);

            var dstaObj = new Mock<IDSFDataObject>();
            mock.Setup(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dstaObj.Object, It.IsAny<bool>())).Verifiable();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null,
                IsObject = true,
                ObjectName = "a"
            };
            PrivateObject privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfConsumeRabbitMQActivity.IsObject);
            //---------------Execute Test ----------------------
            privateObject.Invoke("AssignResult", dstaObj.Object, 1);
            //---------------Test Result -----------------------
            mock.Verify(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dstaObj.Object, It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AssignResult_GivenIsObjectOneMessages_ShouldSetResponsemanagerPushResponeToEnvironment()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IResponseManager>();
            mock.SetupProperty(manager => manager.ObjectName);

            var dstaObj = new Mock<IDSFDataObject>();
            mock.Setup(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dstaObj.Object, It.IsAny<bool>())).Verifiable();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null,
                IsObject = true,
                ObjectName = "a",
                _messages = new List<string>(new[] { "a" })
            };
            PrivateObject privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfConsumeRabbitMQActivity.IsObject);
            //---------------Execute Test ----------------------
            privateObject.Invoke("AssignResult", dstaObj.Object, 1);
            //---------------Test Result -----------------------
            mock.Verify(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dstaObj.Object, It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AssignResult_GivenIsObjectManyMessages_ShouldSetResponsemanagerPushResponeToEnvironment()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IResponseManager>();
            mock.SetupProperty(manager => manager.ObjectName);

            var dstaObj = new Mock<IDSFDataObject>();
            mock.Setup(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dstaObj.Object, It.IsAny<bool>())).Verifiable();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity(mock.Object)
            {
                RabbitMQSourceResourceId = Guid.Empty,
                QueueName = string.Empty,
                Prefetch = null,
                IsObject = true,
                ObjectName = "a",
                _messages = new List<string>(new[] { "a", "b" })
            };
            PrivateObject privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            //---------------Assert Precondition----------------
            Assert.IsTrue(dsfConsumeRabbitMQActivity.IsObject);
            //---------------Execute Test ----------------------
            privateObject.Invoke("AssignResult", dstaObj.Object, 1);
            //---------------Test Result -----------------------
            mock.Verify(manager => manager.PushResponseIntoEnvironment(It.IsAny<string>(), 1, dstaObj.Object, It.IsAny<bool>()), Times.Exactly(2));
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfConsumeRabbitMQActivity_Construct")]
        public void DsfConsumeRabbitMQActivity_ConstructorRC_Should_SetsDefaultPropertyValuesObjectName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResponseManager>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity(mock.Object)
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
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfConsumeRabbitMQActivity_Construct")]
        public void DsfConsumeRabbitMQActivity_ConstructorRM_Should_SetsDefaultPropertyValuesObjectName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResponseManager>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity(mock.Object)
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
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfConsumeRabbitMQActivity_Construct")]
        public void DsfConsumeRabbitMQActivity_Constructor_Should_SetsInitialiseResponseManager()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResponseManager>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity(mock.Object)
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
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Construct")]
        public void DsfConsumeRabbitMQActivity_ConstructorRC_Should_SetsDefaultPropertyValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var mock = new Mock<IResourceCatalog>();
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity(mock.Object)
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
            if (result != null)
            {
                Assert.AreEqual(result[0], "Failure: Source has been deleted.");
            }
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_Blank_QueueShould_Return_BlankQueueException_NoTimeout()
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
            if (result != null)
            {
                Assert.AreEqual(result[0], "Failure: Queue Name is required.");
            }
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_Blank_QueueShould_Return_BlankQueueException_Timeout()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity()
            {
                TimeOut = "1"
            };

            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);

            //------------Execute Test---------------------------
            var result = privateObject.Invoke("PerformExecution", new Dictionary<string, string>()) as List<string>;

            //------------Assert Results-------------------------
            if (result != null)
            {
                Assert.AreEqual(result[0], "Failure: Queue Name is required.");
            }
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_NoQueue_Shouold_Return_Exception_NoTimeout()
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
            if (result != null)
            {
                Assert.AreEqual(result[0], "Failure: Queue Name is required.");
            }
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_NoQueue_Shouold_Return_Exception_NoTimeout_Timeout()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity()
            {
                TimeOut = "1"
            };

            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource.Object);

            var p = new PrivateObject(dsfConsumeRabbitMQActivity);
            p.SetProperty("ResourceCatalog", resourceCatalog.Object);
            //------------Execute Test---------------------------
            var result = p.Invoke("PerformExecution", new Dictionary<string, string> { { "Param1", "Blah1" }, { "Param2", "Blah2" } }) as List<string>;
            //------------Assert Results-------------------------
            if (result != null)
            {
                Assert.AreEqual(result[0], "Failure: Queue Name is required.");
            }
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        [ExpectedException(typeof(TargetInvocationException))]
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
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity() { TimeOut = "0" };

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
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void DsfConsumeRabbitMQActivity_Execute_Empty_Queue_Exception_NoTimeout()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity() { TimeOut = "0" };

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
            dsfConsumeRabbitMQActivity._messages = new List<string> { "Message" };

            var debugInputs = dsfConsumeRabbitMQActivity.GetDebugInputs(dataList, It.IsAny<int>());
            var debugOutputs = dsfConsumeRabbitMQActivity.GetDebugOutputs(dataList, It.IsAny<int>());
            privateObject.Invoke("AssignResult", dataObj, It.IsAny<int>());
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(debugInputs.Count > 0);
            Assert.IsTrue(debugOutputs.Count > 0);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void DsfConsumeRabbitMQActivity_ExecuteIsObject_DsfBaseActivity_MethodsGivenOjectOutput()
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
            dsfConsumeRabbitMQActivity._messages = new List<string> { "Message" };
            dsfConsumeRabbitMQActivity.IsObject = true;
            dsfConsumeRabbitMQActivity.ObjectName = "[[@Human]]";

            var debugOutputs = dsfConsumeRabbitMQActivity.GetDebugOutputs(dataList, It.IsAny<int>());
            privateObject.Invoke("AssignResult", dataObj, It.IsAny<int>());
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(debugOutputs.Count > 0);
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_UnExisting_Queue_Returns_QeueuNotFoundException_NoTimeout()
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
                Assert.AreEqual(ex.InnerException.Message, string.Format("Queue {0} not found", queueName));
            }
            //------------Assert Results-------------------------            
        }

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_UnExisting_Queue_Returns_QeueuNotFoundException_Timeout()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity()
            {
                TimeOut = "1"
            };

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
                .Throws(new Exception($"Queue '{queueName}' not found"));

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ConnectionFactory", connectionFactory.Object);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            privateObject.SetProperty("Channel", channel.Object);
            //------------Execute Test---------------------------
            try
            {
                privateObject.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", queueName } });
                Assert.Fail("no exceptions thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.InnerException.Message, string.Format("Queue {0} not found", queueName));
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
                Assert.AreEqual(ex.InnerException.Message, string.Format("Queue {0} not found", queueName));
            }
            //------------Assert Results-------------------------            
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_ItemsInQueue_ExecuteAndReqeue_WhenExecutedMultipleTimes_ShouldNotAppend_NoTimeout()
        {
            //------------Setup for test--------------------------
            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());
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
            var basicGetResult1 = new BasicGetResult(1, true, queueName, "", 1, new BasicProperties(), Encoding.Default.GetBytes("hello"));
            var basicGetResult2 = new BasicGetResult(2, true, queueName, "", 1, new BasicProperties(), Encoding.Default.GetBytes("world"));
            channel.SetupSequence(model => model.BasicGet(queueName, It.IsAny<bool>())).Returns(basicGetResult1).Returns(basicGetResult2);
            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ConnectionFactory", connectionFactory.Object);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            privateObject.SetProperty("QueueName", queueName);
            dsfConsumeRabbitMQActivity.Response = "[[msgs().message]]";
            dsfConsumeRabbitMQActivity.ReQueue = true;
            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Results-------------------------  
            IList<string> actualRecset;
            string error;
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "msgs", "message", out actualRecset, out error);
            Assert.AreEqual(1, actualRecset.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformExecution_Given_ItemsInQueue_ExecuteAndReqeue_WhenExecutedMultipleTimes_ShouldNotAppend_Timeout()
        {
            //------------Setup for test--------------------------
            var dataObj = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>());
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity() { TimeOut = "1" };

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
            var basicGetResult1 = new BasicGetResult(1, true, queueName, "", 1, new BasicProperties(), Encoding.Default.GetBytes("hello"));
            var basicGetResult2 = new BasicGetResult(2, true, queueName, "", 1, new BasicProperties(), Encoding.Default.GetBytes("world"));
            channel.SetupSequence(model => model.BasicGet(queueName, It.IsAny<bool>())).Returns(basicGetResult1).Returns(basicGetResult2);
            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ConnectionFactory", connectionFactory.Object);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            privateObject.SetProperty("QueueName", queueName);
            dsfConsumeRabbitMQActivity.Response = "[[msgs().message]]";
            dsfConsumeRabbitMQActivity.ReQueue = true;
            //------------Execute Test---------------------------
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            privateObject.Invoke("ExecuteTool", dataObj, 0);
            //------------Assert Results-------------------------  
            IList<string> actualRecset;
            string error;
            GetRecordSetFieldValueFromDataList(dataObj.Environment, "msgs", "message", out actualRecset, out error);
            Assert.AreEqual(1, actualRecset.Count);
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

        [TestMethod]
        [Owner("Mthembu Sanele")]
        [TestCategory("DsfConsumeRabbitMQActivity_Execute")]
        public void PerformSerialization_ShouldNotError()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();

            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new RabbitMQSource
            {
                HostName = "rsaklfsvrdev",
                Port = 5672,
                UserName = "test",
                Password = "test"
            };


            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(rabbitMQSource);

            var privateObject = new PrivateObject(dsfConsumeRabbitMQActivity);
            privateObject.SetProperty("ResourceCatalog", resourceCatalog.Object);
            dsfConsumeRabbitMQActivity.ReQueue = true;
            try
            {
                privateObject.Invoke("PerformExecution", new Dictionary<string, string> { { "QueueName", "HuggsTest" } });
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Queue Q1 not found", ex.Message);
            }
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();

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
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfConsumeRabbitMQActivity_GetOutputs")]
        public void DsfConsumeRabbitMQActivity_GetOutputs_ShouldIncludeResponse()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity
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
        [Owner("Hagashen Naidu")]
        public void DsfConsumeRabbitMQActivity_ShouldSerializeChannel_ShouldReturnFalse()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();
            //------------Execute Test---------------------------
            var shouldSerialize = dsfConsumeRabbitMQActivity.ShouldSerializeChannel();
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldSerialize);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void DsfConsumeRabbitMQActivity_ShouldSerializeConnection_ShouldReturnFalse()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();
            //------------Execute Test---------------------------
            var shouldSerialize = dsfConsumeRabbitMQActivity.ShouldSerializeConnection();
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldSerialize);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void DsfConsumeRabbitMQActivity_ShouldSerializeConnectionFactory_ShouldReturnFalse()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();
            //------------Execute Test---------------------------
            var shouldSerialize = dsfConsumeRabbitMQActivity.ShouldSerializeConnectionFactory();
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldSerialize);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void DsfConsumeRabbitMQActivity_ShouldSerializeConsumer_ShouldReturnFalse()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();
            //------------Execute Test---------------------------
            var shouldSerialize = dsfConsumeRabbitMQActivity.ShouldSerializeConsumer();
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldSerialize);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void DsfConsumeRabbitMQActivity_ShouldSerializeRabbitSource_ShouldReturnFalse()
        {
            //------------Setup for test--------------------------
            var dsfConsumeRabbitMQActivity = new DsfConsumeRabbitMQActivity();
            //------------Execute Test---------------------------
            var shouldSerialize = dsfConsumeRabbitMQActivity.ShouldSerializeRabbitSource();
            //------------Assert Results-------------------------
            Assert.IsFalse(shouldSerialize);
        }
    }
}