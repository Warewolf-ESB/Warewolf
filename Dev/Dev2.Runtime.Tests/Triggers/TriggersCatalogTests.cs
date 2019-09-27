/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Serializers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Warewolf.OS.IO;
using Warewolf.Security.Encryption;
using Warewolf.Trigger.Queue;
using Warewolf.Triggers;

namespace Dev2.Tests.Runtime.Triggers
{
    [TestClass]
    public class TriggersCatalogTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var resourcePath = Path.Combine(@"C:\ProgramData\Warewolf\Triggers");

            if (Directory.Exists(resourcePath))
            {
                DirectoryWrapperInstance().CleanUp(resourcePath);
            }
        }
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

        public static IFileSystemWatcher FileSystemWatcherWrapperInstance()
        {
            return new FileSystemWatcherWrapper();
        }


        public void CleanupTestDirectory(string queueTriggersPath)
        {
            if (Directory.Exists(queueTriggersPath))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                DirectoryWrapperInstance().CleanUp(queueTriggersPath);
            }
        }
        public static string QueueTriggersPath
        {
            get
            {
                var resourcePath = Path.Combine(@"C:\ProgramData\Warewolf\Triggers", Guid.NewGuid().ToString());
                return resourcePath;
            }
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_Constructor_TestPathDoesNotExist_ShouldCreateIt()
        {
            //------------Setup for test--------------------------
            var queueTriggersPath = QueueTriggersPath;
            try
            {
                //------------Assert Preconditions-------------------
                Assert.IsFalse(Directory.Exists(queueTriggersPath));
                //------------Execute Test---------------------------
                new TriggersCatalog(DirectoryWrapperInstance(), FileWrapperInstance(), queueTriggersPath, SerializerInstance(), FileSystemWatcherWrapperInstance());
                //------------Assert Results-------------------------
                Assert.IsTrue(Directory.Exists(queueTriggersPath));
            }
            finally
            {
                CleanupTestDirectory(queueTriggersPath);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_SaveTriggerQueue_ShouldSave()
        {
            var queueTriggersPath = QueueTriggersPath;
            try
            {
                var queue = "TestQueueName";
                var workflowName = "TestWorkflow";

                var mockTriggerQueue = new Mock<ITriggerQueue>();
                mockTriggerQueue.Setup(triggerQueue => triggerQueue.TriggerId).Returns(Guid.NewGuid());
                mockTriggerQueue.Setup(triggerQueue => triggerQueue.QueueSourceId).Returns(Guid.NewGuid());
                mockTriggerQueue.Setup(triggerQueue => triggerQueue.QueueName).Returns(queue);
                mockTriggerQueue.Setup(triggerQueue => triggerQueue.WorkflowName).Returns(workflowName);

                var triggerCatalog = GetTriggersCatalog(queueTriggersPath);
                var triggerQueueEvent = mockTriggerQueue.Object;
                triggerCatalog.SaveTriggerQueue(triggerQueueEvent);

                var path = queueTriggersPath + "\\" + triggerQueueEvent.TriggerId + ".bite";

                var triggerQueueFiles = Directory.EnumerateFiles(queueTriggersPath).ToList();

                Assert.AreEqual(1, triggerQueueFiles.Count);
                Assert.AreEqual(path, triggerQueueFiles[0]);

                ThoroughlyDeleteTrigger(triggerCatalog, mockTriggerQueue.Object);

                triggerQueueFiles = Directory.EnumerateFiles(queueTriggersPath).ToList();

                Assert.AreEqual(0, triggerQueueFiles.Count);
            }
            finally
            {
                CleanupTestDirectory(queueTriggersPath);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_SaveTriggerQueue_WhenHasTriggerId_ShouldSave_NotUpdateTriggerId()
        {
            var queueTriggersPath = QueueTriggersPath;
            try
            {
                var source = "TestResource";
                var queue = "TestQueueName";
                var workflowName = "TestWorkflow";
                var triggerId = Guid.NewGuid();

                var mockResource = new Mock<IResource>();
                mockResource.Setup(resource => resource.ResourceName).Returns(source);
                mockResource.Setup(resource => resource.ResourceID).Returns(Guid.NewGuid());

                var triggerQueueEvent = new TriggerQueue
                {
                    QueueSourceId = mockResource.Object.ResourceID,
                    QueueName = queue,
                    WorkflowName = workflowName,
                    TriggerId = triggerId
                };

                var triggerCatalog = GetTriggersCatalog(queueTriggersPath);
                triggerCatalog.SaveTriggerQueue(triggerQueueEvent);

                var path = queueTriggersPath + "\\" + triggerId + ".bite";

                var triggerQueueFiles = Directory.EnumerateFiles(queueTriggersPath).ToList();

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

                ThoroughlyDeleteTrigger(triggerCatalog, triggerQueueEvent);

                triggerQueueFiles = Directory.EnumerateFiles(queueTriggersPath).ToList();

                Assert.AreEqual(0, triggerQueueFiles.Count);
            }
            finally
            {
                CleanupTestDirectory(queueTriggersPath);
            }
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_SaveTriggerQueue_ShouldSaveEncrypted()
        {
            var queueTriggersPath = QueueTriggersPath;

            try
            {
                var source = "TestResource";
                var queue = "TestQueueName";
                var workflowName = "TestWorkflow";

                var mockResource = new Mock<IResource>();
                mockResource.Setup(resource => resource.ResourceName).Returns(source);
                mockResource.Setup(resource => resource.ResourceID).Returns(Guid.NewGuid());

                var triggerQueueEvent = new TriggerQueue
                {
                    TriggerId = Guid.NewGuid(),
                    QueueSourceId = mockResource.Object.ResourceID,
                    QueueName = queue,
                    WorkflowName = workflowName
                };

                var triggerCatalog = GetTriggersCatalog(queueTriggersPath);

                triggerCatalog.SaveTriggerQueue(triggerQueueEvent);

                var path = queueTriggersPath + "\\" + triggerQueueEvent.TriggerId + ".bite";

                var triggerQueueFiles = Directory.EnumerateFiles(queueTriggersPath).ToList();

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

                ThoroughlyDeleteTrigger(triggerCatalog, triggerQueueEvent);

                triggerQueueFiles = Directory.EnumerateFiles(queueTriggersPath).ToList();

                Assert.AreEqual(0, triggerQueueFiles.Count);
            }
            finally
            {
                CleanupTestDirectory(queueTriggersPath);
            }
        }

        void ThoroughlyDeleteTrigger(ITriggersCatalog triggerCatalog, ITriggerQueue triggerQueue)
        {
            var retryCount = 0;
            try
            {
                triggerCatalog.DeleteTriggerQueue(triggerQueue);
            }
            catch(IOException e)
            {
                if (e.Message.Contains("it is being used by another process") && retryCount++ < 10)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    throw e;
                }
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_Load_ShouldLoadAllTriggerQueuesInDirectory()
        {
            var queueTriggersPath = QueueTriggersPath;
            try
            {
                var triggerCatalog = GetTriggersCatalog(queueTriggersPath);

                SaveRandomTriggerQueue(triggerCatalog);

                var triggerQueueFiles = Directory.EnumerateFiles(queueTriggersPath).ToList();
                Assert.AreEqual(1, triggerQueueFiles.Count);

                Thread.Sleep(500);
                var triggerQueueEvents = triggerCatalog.Queues;
                Assert.AreEqual(1, triggerQueueEvents.Count);

                SaveRandomTriggerQueue(triggerCatalog);

                triggerQueueFiles = Directory.EnumerateFiles(queueTriggersPath).ToList();
                Assert.AreEqual(2, triggerQueueFiles.Count);

                Thread.Sleep(500);
                triggerQueueEvents = triggerCatalog.Queues;
                Assert.AreEqual(2, triggerQueueEvents.Count);
            }
            finally
            {
                CleanupTestDirectory(queueTriggersPath);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_DeleteTrigger_ShouldOnlyDeleteRequestedTrigger()
        {
            var queueTriggersPath = QueueTriggersPath;
            try
            {
                var triggerCatalog = GetTriggersCatalog(queueTriggersPath);

                SaveRandomTriggerQueue(triggerCatalog);
                SaveRandomTriggerQueue(triggerCatalog);
                SaveRandomTriggerQueue(triggerCatalog);

                var triggerQueueEvents = triggerCatalog.Queues;

                ThoroughlyDeleteTrigger(triggerCatalog, triggerQueueEvents[1]);
                triggerQueueEvents = triggerCatalog.Queues;
                Assert.AreEqual(2, triggerQueueEvents.Count);
            }
            catch
            {
                CleanupTestDirectory(queueTriggersPath);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_LoadQueueTriggerFromFile()
        {
            var directoryWrapper = new Mock<IDirectory>().Object;
            var mockFileWrapper = new Mock<IFile>();
            var mockSerializer = new Mock<ISerializer>();
            var mockFileSystemWatcher = new Mock<IFileSystemWatcher>();

            var decryptedTrigger = "serialized queue data";
            var expected = DpapiWrapper.Encrypt(decryptedTrigger);
            mockFileWrapper.Setup(o => o.ReadAllText("somefile.bite")).Returns(expected);
            var expectedTrigger = new TriggerQueue();
            mockSerializer.Setup(o => o.Deserialize<ITriggerQueue>(decryptedTrigger)).Returns(expectedTrigger);

            var catalog = GetTriggersCatalog(directoryWrapper, mockFileWrapper.Object, "some path", mockSerializer.Object, mockFileSystemWatcher.Object);
            var actual = catalog.LoadQueueTriggerFromFile("somefile.bite");

            mockSerializer.Verify(o => o.Deserialize<ITriggerQueue>(decryptedTrigger), Times.Once);
            Assert.AreEqual(expectedTrigger, actual);
        }

        void SaveRandomTriggerQueue(ITriggersCatalog triggerCatalog)
        {
            var randomizer = new Random();
            var source = "TestResource" + randomizer.Next(1, 10000);
            var queue = "TestQueueName" + randomizer.Next(1, 10000);
            var workflowName = "TestWorkflow" + randomizer.Next(1, 10000);

            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceName).Returns(source);
            mockResource.Setup(resource => resource.ResourceID).Returns(Guid.NewGuid());

            var triggerQueueEvent = new TriggerQueue
            {
                QueueSourceId = mockResource.Object.ResourceID,
                QueueName = queue,
                WorkflowName = workflowName
            };

            triggerCatalog.SaveTriggerQueue(triggerQueueEvent);
        }

        ITriggersCatalog GetTriggersCatalog(string queueTriggersPath)
        {
            return GetTriggersCatalog(DirectoryWrapperInstance(), FileWrapperInstance(), queueTriggersPath, SerializerInstance(), FileSystemWatcherWrapperInstance());
        }

        ITriggersCatalog GetTriggersCatalog(IDirectory directory, IFile file, string queueTriggersPath, ISerializer serializer, IFileSystemWatcher fileSystemWatcher)
        {
            return new TriggersCatalog(directory, file, queueTriggersPath, serializer, fileSystemWatcher);
        }
    }
}
