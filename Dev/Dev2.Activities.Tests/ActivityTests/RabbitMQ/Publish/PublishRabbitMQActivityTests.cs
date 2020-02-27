/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Runtime.Interfaces;
using Dev2.Common.State;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.ExtMethods;
using Dev2.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client.Framing;
using Warewolf.Data.Options;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests.RabbitMQ.Publish
{
    [TestClass]
    public class PublishRabbitMQActivityTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_Construct_Paramterless_SetsDefaultPropertyValues()
        {
            //------------Execute Test---------------------------
            var publishRabbitMQActivity = new PublishRabbitMQActivity();
            //------------Assert Results-------------------------
            Assert.IsNotNull(publishRabbitMQActivity);
            Assert.AreEqual("RabbitMQ Publish", publishRabbitMQActivity.DisplayName);
        }
        
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_Execute_Success()
        {
            //------------Setup for test--------------------------
            var env = CreateExecutionEnvironment();
            const string queueName = "Q1", message = "Test Message";
            var param = new Dictionary<string, string> {{"QueueName", queueName}, {"Message", message}};
            var body = Encoding.UTF8.GetBytes(message);
            
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(true);
            dataObject.Setup(o => o.ExecutionID).Returns(new Guid?());
            dataObject.Setup(o => o.CustomTransactionID).Returns(new Guid?().ToString());
            dataObject.Setup(o => o.Environment).Returns(env);
           
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();
            var connectionFactory = new Mock<ConnectionFactory>();
            var connection = new Mock<IConnection>();
            var channel = new Mock<IModel>();
            var mockBasicProperties = new Mock<IBasicProperties>();
            mockBasicProperties.SetupAllProperties();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(rabbitMQSource.Object);
            connectionFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);
            connection.Setup(c => c.CreateModel()).Returns(channel.Object);
            channel.Setup(c => c.QueueDeclare(queueName, false, false, false, null));
            channel.Setup(c => c.BasicPublish(string.Empty, queueName, null, body));
            channel.Setup(c => c.CreateBasicProperties()).Returns(mockBasicProperties.Object);

            var publishRabbitMQActivity =
                new TestPublishRabbitMQActivity(resourceCatalog.Object, connectionFactory.Object);

            //------------Execute Test---------------------------
            publishRabbitMQActivity.TestExecuteTool(dataObject.Object);
            var result = publishRabbitMQActivity.TestPerformExecution(param);

            //------------Assert Results-------------------------
            resourceCatalog.Verify(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce);
            connectionFactory.Verify(c => c.CreateConnection(), Times.Once);
            connection.Verify(c => c.CreateModel(), Times.Once);
            channel.Verify(
                c => c.ExchangeDeclare(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<bool>(), It.IsAny<bool>(),
                    It.IsAny<IDictionary<string, object>>()), Times.Once);
            channel.Verify(
                c => c.QueueDeclare(It.IsAny<String>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
                    It.IsAny<IDictionary<string, object>>()), Times.Once);
            channel.Verify(
                c => c.BasicPublish(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<IBasicProperties>(),
                    It.IsAny<byte[]>()), Times.Once);
            Assert.AreEqual(result[0], "Success");
            Assert.IsTrue(mockBasicProperties.Object.Persistent);
        }

      
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_Execute_Failure_NullSource()
        {
            //------------Setup for test--------------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns<RabbitMQSource>(null);

            var env = CreateExecutionEnvironment();
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(true);
            dataObject.Setup(o => o.ExecutionID).Returns(new Guid?());
            dataObject.Setup(o => o.CustomTransactionID).Returns(new Guid?().ToString());
            dataObject.Setup(o => o.Environment).Returns(env);

            var publishRabbitMQActivity =
                new TestPublishRabbitMQActivity(resourceCatalog.Object, new ConnectionFactory());

            //------------Execute Test---------------------------
            publishRabbitMQActivity.TestExecuteTool(dataObject.Object);
            var result = publishRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>());
            //------------Assert Results-------------------------

            Assert.AreEqual(result[0], "Failure: Source has been deleted.");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_Execute_Failure_NoParams()
        {
            //------------Setup for test--------------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(rabbitMQSource.Object);

            var env = CreateExecutionEnvironment();
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(true);
            dataObject.Setup(o => o.ExecutionID).Returns(new Guid?());
            dataObject.Setup(o => o.CustomTransactionID).Returns(new Guid?().ToString());
            dataObject.Setup(o => o.Environment).Returns(env);

            var publishRabbitMQActivity =
                new TestPublishRabbitMQActivity(resourceCatalog.Object, new ConnectionFactory());

            //------------Execute Test---------------------------
            publishRabbitMQActivity.TestExecuteTool(dataObject.Object);
            var result = publishRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(result[0], "Failure: Queue Name and Message are required.");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_Execute_Failure_InvalidParams()
        {
            //------------Setup for test--------------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();
            var param = new Dictionary<string, string> { };
            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(rabbitMQSource.Object);

            var env = CreateExecutionEnvironment();
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(true);
            dataObject.Setup(o => o.ExecutionID).Returns(new Guid?());
            dataObject.Setup(o => o.CustomTransactionID).Returns(new Guid?().ToString());
            dataObject.Setup(o => o.Environment).Returns(env);

            var publishRabbitMQActivity =
                new TestPublishRabbitMQActivity(resourceCatalog.Object, new ConnectionFactory());
            //------------Execute Test---------------------------
            publishRabbitMQActivity.TestExecuteTool(dataObject.Object);
            var result = publishRabbitMQActivity.TestPerformExecution(param);
            //------------Assert Results-------------------------
            Assert.AreEqual(result[0], "Failure: Queue Name and Message are required.");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        [ExpectedException(typeof(Exception))]
        public void PublishRabbitMQActivity_Execute_Failure_NullException()
        {
            //------------Setup for test--------------------------
            var resourceCatalog = new Mock<IResourceCatalog>();
            var rabbitMQSource = new Mock<RabbitMQSource>();
            var connectionFactory = new Mock<ConnectionFactory>();

            resourceCatalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(rabbitMQSource.Object);
            connectionFactory.Setup(c => c.CreateConnection()).Returns<IConnection>(null);

            var env = CreateExecutionEnvironment();
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(true);
            dataObject.Setup(o => o.ExecutionID).Returns(new Guid?());
            dataObject.Setup(o => o.CustomTransactionID).Returns(new Guid?().ToString());
            dataObject.Setup(o => o.Environment).Returns(env);

            var publishRabbitMQActivity =
                new TestPublishRabbitMQActivity(resourceCatalog.Object, connectionFactory.Object);

            //------------Execute Test---------------------------
            publishRabbitMQActivity.TestExecuteTool(dataObject.Object);
            var result = publishRabbitMQActivity.TestPerformExecution(new Dictionary<string, string>
                {{"QueueName", "Q1"}, {"Message", "Test message"}});
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            var sourceId = Guid.NewGuid();
            //------------Setup for test--------------------------
            var act = new PublishRabbitMQActivity
            {
                QueueName = "bob",
                IsDurable = true,
                IsExclusive = false,
                Message = "hello",
                RabbitMQSourceResourceId = sourceId,
                IsAutoDelete = false,
                Result = "[[res]]",
            };


            //  (act.BasicProperties.AutoCorrela) = "test-123";
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(8, stateItems.Count());

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
                    Name = "BasicProperties",
                    Type = StateVariable.StateType.Input,
                    Value = "Warewolf.Data.Options.RabbitMqPublishOptions"
                },
                new StateVariable
                {
                    Name = "IsDurable",
                    Type = StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name = "IsExclusive",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name = "Message",
                    Type = StateVariable.StateType.Input,
                    Value = "hello"
                },
                new StateVariable
                {
                    Name = "RabbitMQSourceResourceId",
                    Type = StateVariable.StateType.Input,
                    Value = sourceId.ToString()
                },
                new StateVariable
                {
                    Name = "IsAutoDelete",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name = "Result",
                    Type = StateVariable.StateType.Output,
                    Value = "[[res]]"
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
        static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }
        class TestPublishRabbitMQActivity : PublishRabbitMQActivity
        {
            public TestPublishRabbitMQActivity(IResourceCatalog resourceCatalog, ConnectionFactory connectionFactory)
                : base(resourceCatalog, connectionFactory)
            {
            }

            public void TestExecuteTool(IDSFDataObject dataObject)
            {
                base.ExecuteTool(dataObject, 0);
            }

            public List<string> TestPerformExecution(Dictionary<string, string> evaluatedValues)
            {
                return base.PerformExecution(evaluatedValues);
            }
        }
    }
}