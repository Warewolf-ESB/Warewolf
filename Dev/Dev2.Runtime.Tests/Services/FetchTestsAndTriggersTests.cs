using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.Services.ESB.Management.Services;
using Dev2.Runtime.Services.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Trigger.Queue;
using Warewolf.Triggers;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchTestsAndTriggersTests
    {
        [TestMethod]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(FetchTestsAndTriggers))]
        public void FetchTestsAndTriggers_Execute_ExpectTriggerQueuesFetched()
        {
            // ------------ Setup ------------------
            var fetchTestsAndTriggers = new FetchTestsAndTriggers();
            var resourceIds = new List<Guid>
    {
        Guid.NewGuid(),
        Guid.NewGuid(),
        Guid.NewGuid()
    };

            var expectedQueues = new List<ITriggerQueue>
    {
        new TriggerQueue { ResourceId = resourceIds[0], QueueName = "First" },
        new TriggerQueue { ResourceId = resourceIds[1], QueueName = "Second" },
        new TriggerQueue { ResourceId = resourceIds[2], QueueName = "Third" }
    };

            var allReturnedQueues = new List<ITriggerQueue>();
            var loadCalled = false;

            var mockTriggersCatalog = new Mock<ITriggersCatalog>();
            mockTriggersCatalog
                .Setup(c => c.FetchQueuesByResourceId(It.IsAny<Guid>(), It.IsAny<bool>()))
                .Callback<Guid, bool>((id, isQueueLoad) =>
                {
                    if (!isQueueLoad)
                    {
                        loadCalled = true;
                    }

                    var queue = expectedQueues.FirstOrDefault(q => q.ResourceId == id);
                    if (queue != null)
                    {
                        allReturnedQueues.Add(queue);
                    }
                })
                .Returns<Guid, bool>((id, _) => expectedQueues.Where(q => q.ResourceId == id).ToList());

            var mockTestCatalog = new Mock<ITestCatalog>();
            mockTestCatalog.Setup(c => c.Fetch(It.IsAny<Guid>()))
                .Returns<Guid>(id => new List<IServiceTestModelTO>());

            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>
    {
        { "resourceIDs", serializer.SerializeToBuilder(resourceIds) }
    };

            fetchTestsAndTriggers.TriggersCatalog = mockTriggersCatalog.Object;
            fetchTestsAndTriggers.TestCatalog = mockTestCatalog.Object;

            var workspace = new Mock<IWorkspace>();

            // ------------ Execute ------------------
            var response = fetchTestsAndTriggers.Execute(inputs, workspace.Object);
            var message = serializer.Deserialize<CompressedExecuteMessage>(response);
            var result = serializer.Deserialize<ResourceTestTriggerData>(message.GetDecompressedMessage());

            // ------------ Assert ------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedQueues.Count, result.TriggersCount);
            Assert.IsTrue(loadCalled, "Expected Load to be triggered only on first FetchQueuesByResourceId call");

            // Ensure FetchQueuesByResourceId was called with correct isQueueLoads flow
            mockTriggersCatalog.Verify(c => c.FetchQueuesByResourceId(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(3));
        }

        [TestMethod]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(FetchTestsAndTriggers))]
        public void FetchTestsAndTriggers_Execute_ExpectCorrectTestAndTriggerCounts()
        {
            //------------Setup for test--------------------------
            var fetchTestsAndTriggers = new FetchTestsAndTriggers();

            var resourceIds = new List<Guid>
    {
        Guid.NewGuid(),
        Guid.NewGuid()
    };

            var expectedTests = new List<IServiceTestModelTO>
    {
        new Mock<IServiceTestModelTO>().Object,
        new Mock<IServiceTestModelTO>().Object
    };

            var expectedTriggers = new List<ITriggerQueue>
    {
        new TriggerQueue { ResourceId = resourceIds[0], QueueName = "Trigger 1" },
        new TriggerQueue { ResourceId = resourceIds[1], QueueName = "Trigger 2" }
    };

            var testCatalog = new Mock<ITestCatalog>();
            testCatalog.Setup(c => c.Fetch(resourceIds[0])).Returns(expectedTests);
            testCatalog.Setup(c => c.Fetch(resourceIds[1])).Returns(new List<IServiceTestModelTO>());

            bool isFirst = true;
            var triggerCallCount = 0;

            var triggersCatalog = new Mock<ITriggersCatalog>();
            triggersCatalog.Setup(c => c.FetchQueuesByResourceId(It.IsAny<Guid>(), It.IsAny<bool>()))
                .Callback<Guid, bool>((id, isQueueLoad) =>
                {
                    if (!isQueueLoad)
                        Assert.IsTrue(isFirst, "isQueueLoads should only be false on the first call");

                    isFirst = false;
                    triggerCallCount++;
                })
                .Returns<Guid, bool>((id, _) =>
                {
                    return expectedTriggers.Where(t => t.ResourceId == id).ToList();
                });

            var serializer = new Dev2JsonSerializer();
            var ws = new Mock<IWorkspace>();
            var inputs = new Dictionary<string, StringBuilder>
    {
        { "resourceIDs", serializer.SerializeToBuilder(resourceIds) }
    };

            fetchTestsAndTriggers.TestCatalog = testCatalog.Object;
            fetchTestsAndTriggers.TriggersCatalog = triggersCatalog.Object;

            //------------Execute Test---------------------------
            var resultBuilder = fetchTestsAndTriggers.Execute(inputs, ws.Object);
            var resultMessage = serializer.Deserialize<CompressedExecuteMessage>(resultBuilder);
            var testTriggerData = serializer.Deserialize<ResourceTestTriggerData>(resultMessage.GetDecompressedMessage());

            //------------Assert Results-------------------------
            testCatalog.Verify(c => c.Fetch(It.IsAny<Guid>()), Times.Exactly(2));
            triggersCatalog.Verify(c => c.FetchQueuesByResourceId(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Exactly(2));

            Assert.AreEqual(expectedTests.Count, testTriggerData.TestsCount);
            Assert.AreEqual(expectedTriggers.Count, testTriggerData.TriggersCount);
            Assert.AreEqual(2, triggerCallCount);
        }

        [TestMethod]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(FetchTestsAndTriggers))]
        public void FetchTestsAndTriggers_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var fetch = new FetchTestsAndTriggers();
            //------------Execute Test---------------------------
            var result = fetch.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, result);
        }

        [TestMethod]
        [Owner("Yogesh Rajpurohitt")]
        [TestCategory(nameof(FetchTestsAndTriggers))]
        public void FetchTestsAndTriggers_GetAuthorizationContextForService_ShouldReturnAny()
        {
            //------------Setup for test--------------------------
            var fetch = new FetchTestsAndTriggers();
            //------------Execute Test---------------------------
            var authContext = fetch.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, authContext);
        }

        [TestMethod]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(FetchTestsAndTriggers))]
        public void FetchTestsAndTriggers_HandlesType_ShouldReturnCorrectName()
        {
            //------------Setup for test--------------------------
            var fetch = new FetchTestsAndTriggers();
            //------------Assert Results-------------------------
            Assert.AreEqual(nameof(FetchTestsAndTriggers), fetch.HandlesType());
        }

        [TestMethod]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(FetchTestsAndTriggers))]
        public void FetchTestsAndTriggers_Execute_NullValues_ShouldReturnError()
        {
            //------------Setup for test--------------------------
            var fetch = new FetchTestsAndTriggers();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var resultBuilder = fetch.Execute(null, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(resultBuilder);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(FetchTestsAndTriggers))]
        public void FetchTestsAndTriggers_Execute_ResourceIDsNotPresent_ShouldReturnError()
        {
            //------------Setup for test--------------------------
            var fetch = new FetchTestsAndTriggers();
            var serializer = new Dev2JsonSerializer();
            var values = new Dictionary<string, StringBuilder>
        {
            { "somethingElse", new StringBuilder("value") }
        };
            //------------Execute Test---------------------------
            var resultBuilder = fetch.Execute(values, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(resultBuilder);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(FetchTestsAndTriggers))]
        public void FetchTestsAndTriggers_Execute_ResourceIDsInvalidFormat_ShouldReturnError()
        {
            //------------Setup for test--------------------------
            var fetch = new FetchTestsAndTriggers();
            var serializer = new Dev2JsonSerializer();
            var values = new Dictionary<string, StringBuilder>
        {
            { "resourceIDs", new StringBuilder("[\"not-a-guid\"]") }
        };
            //------------Execute Test---------------------------
            var resultBuilder = fetch.Execute(values, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(resultBuilder);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }


    }
}
