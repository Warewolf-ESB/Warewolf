/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Triggers;
using Dev2.Triggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using Warewolf.Trigger;

namespace Dev2.Tests.Runtime.Triggers
{
    [TestClass]
    public class TriggersCatalogTests
    {
        public static IDirectory DirectoryWrapperInstance()
        {
            return new DirectoryWrapper();
        }

        [TestInitialize]
        public void CleanupTestDirectory()
        {
            if (Directory.Exists(EnvironmentVariables.TriggersPath))
            {
                DirectoryWrapperInstance().CleanUp(EnvironmentVariables.TriggersPath);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_Constructor_TestPathDoesNotExist_ShouldCreateIt()
        {
            //------------Setup for test--------------------------
            //------------Assert Preconditions-------------------
            Assert.IsFalse(Directory.Exists(EnvironmentVariables.TriggersPath));
            //------------Execute Test---------------------------
            new TriggersCatalog();
            //------------Assert Results-------------------------
            Assert.IsTrue(Directory.Exists(EnvironmentVariables.TriggersPath));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_SaveTriggerQueue_ShouldSave()
        {
            var source = "TestResource";
            var queue = "TestQueueName";
            var workflowName = "TestWorkflow";

            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns(source);
            
            var mockTriggerQueue = new Mock<ITriggerQueue>();
            mockTriggerQueue.Setup(triggerQueue => triggerQueue.QueueSourceId).Returns(Guid.NewGuid());
            mockTriggerQueue.Setup(triggerQueue => triggerQueue.QueueName).Returns(queue);
            mockTriggerQueue.Setup(triggerQueue => triggerQueue.WorkflowName).Returns(workflowName);

            var triggerCatalog = new TriggersCatalog();

            triggerCatalog.SaveTriggerQueue(mockTriggerQueue.Object);

            var filePath = $"{source}_{queue}_{workflowName}";

            var path = EnvironmentVariables.TriggersPath + "\\" + filePath + ".bite";

            var triggerQueueFiles = Directory.EnumerateFiles(EnvironmentVariables.TriggersPath).ToList();

            Assert.AreEqual(1, triggerQueueFiles.Count);
            Assert.AreEqual(path, triggerQueueFiles[0]);

            triggerCatalog.DeleteTriggerQueue(mockTriggerQueue.Object);

            triggerQueueFiles = Directory.EnumerateFiles(EnvironmentVariables.TriggersPath).ToList();

            Assert.AreEqual(0, triggerQueueFiles.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_DeleteAllTriggerQueues()
        {
            var source = "TestResource";
            var queue = "TestQueueName";
            var workflowName = "TestWorkflow";

            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns(source);

            var mockTriggerQueue = new Mock<ITriggerQueue>();
            mockTriggerQueue.Setup(triggerQueue => triggerQueue.QueueSourceId).Returns(Guid.NewGuid());
            mockTriggerQueue.Setup(triggerQueue => triggerQueue.QueueName).Returns(queue);
            mockTriggerQueue.Setup(triggerQueue => triggerQueue.WorkflowName).Returns(workflowName);

            var triggerCatalog = new TriggersCatalog();

            triggerCatalog.SaveTriggerQueue(mockTriggerQueue.Object);

            var filePath = $"{source}_{queue}_{workflowName}";

            var path = EnvironmentVariables.TriggersPath + "\\" + filePath + ".bite";

            var triggerQueueFiles = Directory.EnumerateFiles(EnvironmentVariables.TriggersPath).ToList();

            Assert.AreEqual(1, triggerQueueFiles.Count);
            Assert.AreEqual(path, triggerQueueFiles[0]);

            triggerCatalog.DeleteAllTriggerQueues();

            Assert.IsFalse(Directory.Exists(EnvironmentVariables.TriggersPath));

            var newTriggerCatalog = new TriggersCatalog();

            Assert.IsTrue(Directory.Exists(EnvironmentVariables.TriggersPath));
        }
    }
}
