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
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Triggers;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Serializers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Triggers;
using Dev2.Triggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using Warewolf.Security.Encryption;
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

        public static IFile FileWrapperInstance()
        {
            return new FileWrapper();
        }

        public static ISerializer SerializerInstance()
        {
            return new Dev2JsonSerializer();
        }

        string QueueTriggersPath
        {
            get;set;
        }

        [TestInitialize]
        public void CleanupTestDirectory()
        {
            QueueTriggersPath = EnvironmentVariables.QueueTriggersPath;
            if (Directory.Exists(QueueTriggersPath))
            {
                DirectoryWrapperInstance().CleanUp(QueueTriggersPath);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_Constructor_TestPathDoesNotExist_ShouldCreateIt()
        {
            //------------Setup for test--------------------------
            QueueTriggersPath = EnvironmentVariables.QueueTriggersPath;
            //------------Assert Preconditions-------------------
            Assert.IsFalse(Directory.Exists(QueueTriggersPath));
            //------------Execute Test---------------------------
            new TriggersCatalog(DirectoryWrapperInstance(), FileWrapperInstance(), QueueTriggersPath, SerializerInstance());
            //------------Assert Results-------------------------
            Assert.IsTrue(Directory.Exists(QueueTriggersPath));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_SaveTriggerQueue_ShouldSave()
        {
            var queue = "TestQueueName";
            var workflowName = "TestWorkflow";
            
            var mockTriggerQueue = new Mock<ITriggerQueue>();
            mockTriggerQueue.Setup(triggerQueue => triggerQueue.QueueSourceId).Returns(Guid.NewGuid());
            mockTriggerQueue.Setup(triggerQueue => triggerQueue.QueueName).Returns(queue);
            mockTriggerQueue.Setup(triggerQueue => triggerQueue.WorkflowName).Returns(workflowName);

            var triggerCatalog = GetTriggersCatalog();
            var triggerQueueEvent = mockTriggerQueue.Object;
            triggerCatalog.SaveTriggerQueue(triggerQueueEvent);

            var path = QueueTriggersPath + "\\" + triggerQueueEvent.TriggerId + ".bite";

            var triggerQueueFiles = Directory.EnumerateFiles(QueueTriggersPath).ToList();

            Assert.AreEqual(1, triggerQueueFiles.Count);
            Assert.AreEqual(path, triggerQueueFiles[0]);

            triggerCatalog.DeleteTriggerQueue(mockTriggerQueue.Object);

            triggerQueueFiles = Directory.EnumerateFiles(QueueTriggersPath).ToList();

            Assert.AreEqual(0, triggerQueueFiles.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_SaveTriggerQueue_WhenHasTriggerId_ShouldSave_NotUpdateTriggerId()
        {
            var source = "TestResource";
            var queue = "TestQueueName";
            var workflowName = "TestWorkflow";
            var triggerId = Guid.NewGuid();

            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns(source);
            mockResource.Setup(resource => resource.ResourceID).Returns(Guid.NewGuid());

            var triggerQueueEvent = new TriggerQueue();
            triggerQueueEvent.QueueSourceId = mockResource.Object.ResourceID;
            triggerQueueEvent.QueueName = queue;
            triggerQueueEvent.WorkflowName = workflowName;
            triggerQueueEvent.TriggerId = triggerId;

            var triggerCatalog = GetTriggersCatalog();
            triggerCatalog.SaveTriggerQueue(triggerQueueEvent);

            var path = QueueTriggersPath + "\\" + triggerId + ".bite";

            var triggerQueueFiles = Directory.EnumerateFiles(QueueTriggersPath).ToList();

            Assert.AreEqual(1, triggerQueueFiles.Count);
            Assert.AreEqual(path, triggerQueueFiles[0]);

            var savedData = File.ReadAllText(path);
            var isEncrypted = DpapiWrapper.CanBeDecrypted(savedData);
            Assert.IsTrue(isEncrypted);

            var decryptedTrigger = DpapiWrapper.Decrypt(savedData);
            var serializer = new Dev2JsonSerializer();

            var theSavedTrigger = serializer.Deserialize<ITriggerQueue>(decryptedTrigger);
            Assert.IsNotNull(theSavedTrigger);
            Assert.AreEqual(workflowName, theSavedTrigger.WorkflowName);
            Assert.AreEqual(triggerId, theSavedTrigger.TriggerId);

            triggerCatalog.DeleteTriggerQueue(triggerQueueEvent);

            triggerQueueFiles = Directory.EnumerateFiles(QueueTriggersPath).ToList();

            Assert.AreEqual(0, triggerQueueFiles.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_SaveTriggerQueue_ShouldSaveEncrypted()
        {
            var source = "TestResource";
            var queue = "TestQueueName";
            var workflowName = "TestWorkflow";

            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns(source);
            mockResource.Setup(resource => resource.ResourceID).Returns(Guid.NewGuid());

            var triggerQueueEvent =  new TriggerQueue();
            triggerQueueEvent.QueueSourceId= mockResource.Object.ResourceID;
            triggerQueueEvent.QueueName= queue;
            triggerQueueEvent.WorkflowName= workflowName;

            var triggerCatalog = GetTriggersCatalog();
            
            triggerCatalog.SaveTriggerQueue(triggerQueueEvent);

            var path = QueueTriggersPath + "\\" + triggerQueueEvent.TriggerId + ".bite";

            var triggerQueueFiles = Directory.EnumerateFiles(QueueTriggersPath).ToList();

            Assert.AreEqual(1, triggerQueueFiles.Count);
            Assert.AreEqual(path, triggerQueueFiles[0]);

            var savedData = File.ReadAllText(path);
            var isEncrypted = DpapiWrapper.CanBeDecrypted(savedData);
            Assert.IsTrue(isEncrypted);

            var decryptedTrigger = DpapiWrapper.Decrypt(savedData);
            var serializer = new Dev2JsonSerializer();

            var theSavedTrigger = serializer.Deserialize<ITriggerQueue>(decryptedTrigger);
            Assert.IsNotNull(theSavedTrigger);
            Assert.AreEqual(workflowName, theSavedTrigger.WorkflowName);

            triggerCatalog.DeleteTriggerQueue(triggerQueueEvent);

            triggerQueueFiles = Directory.EnumerateFiles(QueueTriggersPath).ToList();

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

            var triggerCatalog = GetTriggersCatalog();
            var triggerQueueEvent = mockTriggerQueue.Object;
            triggerCatalog.SaveTriggerQueue(triggerQueueEvent);


            var path = QueueTriggersPath + "\\" + triggerQueueEvent.TriggerId + ".bite";

            var triggerQueueFiles = Directory.EnumerateFiles(QueueTriggersPath).ToList();

            Assert.AreEqual(1, triggerQueueFiles.Count);
            Assert.AreEqual(path, triggerQueueFiles[0]);

            triggerCatalog.DeleteAllTriggerQueues();

            Assert.IsFalse(Directory.Exists(QueueTriggersPath));

            var newTriggerCatalog = GetTriggersCatalog();

            Assert.IsTrue(Directory.Exists(QueueTriggersPath));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggerCatalog_Load_ShouldLoadAllTriggerQueuesInDirectory()
        {
            var triggerCatalog = GetTriggersCatalog();

            SaveRandomTriggerQueue(triggerCatalog);
            SaveRandomTriggerQueue(triggerCatalog);
            SaveRandomTriggerQueue(triggerCatalog);

            var triggerQueueFiles = Directory.EnumerateFiles(QueueTriggersPath).ToList();
            Assert.AreEqual(3, triggerQueueFiles.Count);

            triggerCatalog.Load();

            var triggerQueueEvents = triggerCatalog.Queues;
            Assert.AreEqual(3, triggerQueueEvents.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggerCatalog_DeleteTrigger_ShouldOnlyDeleteRequestedTrigger()
        {
            var triggerCatalog = GetTriggersCatalog();

            SaveRandomTriggerQueue(triggerCatalog);
            SaveRandomTriggerQueue(triggerCatalog);
            SaveRandomTriggerQueue(triggerCatalog);

            triggerCatalog.Load();
            var triggerQueueEvents = triggerCatalog.Queues;

            triggerCatalog.DeleteTriggerQueue(triggerQueueEvents[1]);

            Assert.AreEqual(2, triggerQueueEvents.Count);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggerCatalog_LoadQueueTriggerFromFile()
        {
            var directoryWrapper = new Mock<IDirectory>().Object;
            var mockFileWrapper = new Mock<IFile>();
            var mockSerializer = new Mock<ISerializer>();

            var decryptedTrigger = "serialized queue data";
            var expected = DpapiWrapper.Encrypt(decryptedTrigger);
            mockFileWrapper.Setup(o => o.ReadAllText("somefile.bite")).Returns(expected);
            var expectedTrigger = new TriggerQueue();
            mockSerializer.Setup(o => o.Deserialize<ITriggerQueue>(decryptedTrigger)).Returns(expectedTrigger);

            var catalog = GetTriggersCatalog(directoryWrapper, mockFileWrapper.Object, "some path", mockSerializer.Object);
            var actual = catalog.LoadQueueTriggerFromFile("somefile.bite");

            mockSerializer.Verify(o => o.Deserialize<ITriggerQueue>(decryptedTrigger), Times.Once);
            Assert.AreEqual(expectedTrigger, actual);
        }

        void SaveRandomTriggerQueue(ITriggersCatalog triggerCatalog)
        {
            var randomizer = new Random();
            var source = "TestResource"+randomizer.Next(1,10000);
            var queue = "TestQueueName" + randomizer.Next(1, 10000);
            var workflowName = "TestWorkflow" + randomizer.Next(1, 10000);

            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns(source);
            mockResource.Setup(resource => resource.ResourceID).Returns(Guid.NewGuid());

            var triggerQueueEvent = new TriggerQueue();
            triggerQueueEvent.QueueSourceId = mockResource.Object.ResourceID;
            triggerQueueEvent.QueueName = queue;
            triggerQueueEvent.WorkflowName = workflowName;

            triggerCatalog.SaveTriggerQueue(triggerQueueEvent);
        }

        ITriggersCatalog GetTriggersCatalog()
        {
            return GetTriggersCatalog(DirectoryWrapperInstance(), FileWrapperInstance(), EnvironmentVariables.QueueTriggersPath, SerializerInstance());
        }

        ITriggersCatalog GetTriggersCatalog(IDirectory directory,IFile file,string queueTriggersPath,ISerializer serializer)
        {
            return new TriggersCatalog(directory, file, queueTriggersPath, serializer);
        }
    }
}
