/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Runtime.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using Warewolf.OS.IO;
using Warewolf.Security.Encryption;
using Warewolf.Trigger.Queue;
using Warewolf.Triggers;

namespace Dev2.Tests.Runtime.Triggers
{
    [TestClass]
    public class TriggersCatalogTests
    {
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

            var serializerInstance = new Mock<IBuilderSerializer>().Object;
            var mockDirectory = new Mock<IDirectory>();
            var directory = mockDirectory.Object;
            var file = new Mock<IFile>().Object;
            var fileSystemWatcherWrapper = new Mock<IFileSystemWatcher>().Object;

            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            new TriggersCatalog(directory, file, queueTriggersPath, serializerInstance, fileSystemWatcherWrapper);
            //------------Assert Results-------------------------
            mockDirectory.Verify(o => o.CreateIfNotExists(queueTriggersPath), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_SaveTriggerQueue_ShouldSave()
        {
            var queueTriggersPath = QueueTriggersPath;
            var contents = "";
            var triggerId = Guid.NewGuid();
            var path = queueTriggersPath + "\\" + triggerId + ".bite";

            var serializerInstance = new Mock<IBuilderSerializer>();
            serializerInstance.Setup(serializer => serializer.Serialize(It.IsAny<ITriggerQueue>())).Returns(contents);
            var directory = new Mock<IDirectory>().Object;
            var mockFile = new Mock<IFile>();
            mockFile.Setup(file => file.WriteAllText(path, It.IsAny<string>()));
            var fileSystemWatcherWrapper = new Mock<IFileSystemWatcher>().Object;

            var triggerCatalog = GetTriggersCatalog(directory, mockFile.Object, queueTriggersPath, serializerInstance.Object, fileSystemWatcherWrapper);

            var triggerQueue = SaveRandomTriggerQueue(triggerCatalog, triggerId);

            mockFile.Verify(file => file.WriteAllText(path, It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_SaveTriggerQueue_WhenHasTriggerId_ShouldSave_NotUpdateTriggerId()
        {
            var queueTriggersPath = QueueTriggersPath;

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
            var serializer = new Dev2.Common.Serializers.Dev2JsonSerializer();

            var mockSerializer = new Mock<IBuilderSerializer>();
            mockSerializer.Setup(o => o.Serialize(It.IsAny<ITriggerQueue>())).Returns(serializer.Serialize(triggerQueueEvent));
            var serializerInstance = mockSerializer.Object;

            var path = queueTriggersPath + "\\" + triggerId + ".bite";

            var directory = new Mock<IDirectory>().Object;
            var mockFile = new Mock<IFile>();
            var savedData = string.Empty;
            mockFile.Setup(o => o.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((filename, data) => {
                savedData = data;
            });

            var file = mockFile.Object;
            var fileSystemWatcherWrapper = new Mock<IFileSystemWatcher>().Object;

            var triggerCatalog = GetTriggersCatalog(directory, file, queueTriggersPath, serializerInstance, fileSystemWatcherWrapper);

            triggerCatalog.SaveTriggerQueue(triggerQueueEvent);


            mockFile.Verify(o => o.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            var isEncrypted = DpapiWrapper.CanBeDecrypted(savedData);
            Assert.IsTrue(isEncrypted);

            var decryptedTrigger = DpapiWrapper.Decrypt(savedData);

            var theSavedTrigger = serializer.Deserialize<ITriggerQueue>(decryptedTrigger);
            Assert.IsNotNull(theSavedTrigger);
            Assert.AreEqual(workflowName, theSavedTrigger.WorkflowName);

            Assert.AreEqual(triggerId, theSavedTrigger.TriggerId);

            mockFile.Setup(o => o.Exists(path)).Returns(() => savedData != string.Empty);
            triggerCatalog.DeleteTriggerQueue(triggerQueueEvent);

            mockFile.Verify(o => o.Delete(path), Times.Once);

            Assert.AreEqual(0, triggerCatalog.Queues.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_DeleteTrigger_ShouldOnlyDeleteRequestedTrigger()
        {
            var queueTriggersPath = QueueTriggersPath;
            var contents = "";

            var serializerInstance = new Mock<IBuilderSerializer>();
            serializerInstance.Setup(serializer => serializer.Serialize(It.IsAny<ITriggerQueue>())).Returns(contents);
            var directory = new Mock<IDirectory>().Object;
            var mockFile = new Mock<IFile>();
            mockFile.Setup(file => file.WriteAllText(queueTriggersPath, contents));
            var fileSystemWatcherWrapper = new Mock<IFileSystemWatcher>().Object;

            var triggerCatalog = GetTriggersCatalog(directory, mockFile.Object, queueTriggersPath, serializerInstance.Object, fileSystemWatcherWrapper);

            var triggerQueue = SaveRandomTriggerQueue(triggerCatalog, Guid.Empty);

            var path = queueTriggersPath + "\\" + triggerQueue.TriggerId + ".bite";
            mockFile.Setup(file => file.Exists(path)).Returns(true);

            triggerCatalog.DeleteTriggerQueue(triggerQueue);
            mockFile.Verify(file => file.Exists(path), Times.Once);
            mockFile.Verify(file => file.Delete(path), Times.Once);
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

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_LoadQueuesByResourceId_ExpectNone()
        {
            var mockDirectoryWrapper = new Mock<IDirectory>();
            const string fileName = "somefile.bite";
            mockDirectoryWrapper.Setup(o => o.GetFiles(It.IsAny<string>())).Returns(new[] {fileName});

            var mockFileWrapper = new Mock<IFile>();
            var mockSerializer = new Mock<ISerializer>();
            var mockFileSystemWatcher = new Mock<IFileSystemWatcher>();

            var decryptedTrigger = "serialized queue data";
            var expected = DpapiWrapper.Encrypt(decryptedTrigger);
            mockFileWrapper.Setup(o => o.ReadAllText(fileName)).Returns(expected);
            var expectedTrigger = new TriggerQueue();
            mockSerializer.Setup(o => o.Deserialize<ITriggerQueue>(decryptedTrigger)).Returns(expectedTrigger);

            var catalog = GetTriggersCatalog(mockDirectoryWrapper.Object, mockFileWrapper.Object, "some path", mockSerializer.Object, mockFileSystemWatcher.Object);
            var triggerQueue = catalog.LoadQueueTriggerFromFile(fileName);

            mockSerializer.Verify(o => o.Deserialize<ITriggerQueue>(decryptedTrigger), Times.Exactly(2));
            Assert.AreEqual(expectedTrigger, triggerQueue);

            var triggerQueues = catalog.LoadQueuesByResourceId(Guid.NewGuid());

            Assert.AreEqual(0, triggerQueues.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_LoadQueuesByResourceId_ExpectQueue()
        {
            var mockDirectoryWrapper = new Mock<IDirectory>();
            const string fileName = "somefile.bite";
            mockDirectoryWrapper.Setup(o => o.GetFiles(It.IsAny<string>())).Returns(new[] {fileName});

            var mockFileWrapper = new Mock<IFile>();
            var mockSerializer = new Mock<ISerializer>();
            var mockFileSystemWatcher = new Mock<IFileSystemWatcher>();

            var expectedResourceId = Guid.NewGuid();
            var decryptedTrigger = "serialized queue data";
            var expected = DpapiWrapper.Encrypt(decryptedTrigger);
            mockFileWrapper.Setup(o => o.ReadAllText("somefile.bite")).Returns(expected);
            var expectedTrigger = new TriggerQueue {ResourceId = expectedResourceId};
            mockSerializer.Setup(o => o.Deserialize<ITriggerQueue>(decryptedTrigger)).Returns(expectedTrigger);

            var catalog = GetTriggersCatalog(mockDirectoryWrapper.Object, mockFileWrapper.Object, "some path", mockSerializer.Object, mockFileSystemWatcher.Object);
            var triggerQueue = catalog.LoadQueueTriggerFromFile("somefile.bite");

            mockSerializer.Verify(o => o.Deserialize<ITriggerQueue>(decryptedTrigger), Times.Exactly(2));
            Assert.AreEqual(expectedTrigger, triggerQueue);

            var triggerQueues = catalog.LoadQueuesByResourceId(expectedResourceId);

            Assert.AreEqual(1, triggerQueues.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_PathFromResourceId()
        {
            var triggerId = Guid.NewGuid().ToString();
            var expectedPath = Common.EnvironmentVariables.QueueTriggersPath + @"\" + triggerId + ".bite";
            var path = TriggersCatalog.Instance.PathFromResourceId(triggerId);
            Assert.AreEqual(expectedPath, path);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_Instance()
        {
            var catalogInstance = TriggersCatalog.Instance;
            Assert.IsNotNull(catalogInstance);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_FileSystemWatcher_Created()
        {
            var directory = "some path";
            var fileName = Guid.NewGuid().ToString() + ".bite";
            var files = new string[] { fileName };
            var mockDirectoryWrapper = new Mock<IDirectory>();
            mockDirectoryWrapper.Setup(directoryWrapper => directoryWrapper.GetFiles("some path")).Returns(files);
            var mockFileWrapper = new Mock<IFile>();
            var mockSerializer = new Mock<ISerializer>();

            var mockFileSystemWatcher = new Mock<IFileSystemWatcher>();

            var triggerId = Guid.NewGuid();
            var decryptedTrigger = "serialized queue data";
            var expected = DpapiWrapper.Encrypt(decryptedTrigger);
            mockFileWrapper.Setup(o => o.ReadAllText(fileName)).Returns(expected);
            var expectedTrigger = new TriggerQueue
            {
                TriggerId = triggerId
            };
            mockSerializer.Setup(o => o.Deserialize<ITriggerQueue>(decryptedTrigger)).Returns(expectedTrigger);

            var catalog = GetTriggersCatalog(mockDirectoryWrapper.Object, mockFileWrapper.Object, directory, mockSerializer.Object, mockFileSystemWatcher.Object);

            mockFileSystemWatcher.Raise(fileWatcher => fileWatcher.Created += null, null, new FileSystemEventArgs(WatcherChangeTypes.Created, directory, fileName));

            mockDirectoryWrapper.Verify(directoryWrapper => directoryWrapper.GetFiles(directory), Times.Exactly(2));

            Assert.AreEqual(1, catalog.Queues.Count);
            Assert.AreEqual(triggerId, catalog.Queues[0].TriggerId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_FileSystemWatcher_Changed()
        {
            var directory = "some path";
            var fileName = Guid.NewGuid().ToString() + ".bite";
            var files = new string[] { fileName };
            var mockDirectoryWrapper = new Mock<IDirectory>();
            mockDirectoryWrapper.Setup(directoryWrapper => directoryWrapper.GetFiles("some path")).Returns(files);
            var mockFileWrapper = new Mock<IFile>();
            var mockSerializer = new Mock<ISerializer>();

            var mockFileSystemWatcher = new Mock<IFileSystemWatcher>();

            var triggerId = Guid.NewGuid();
            var decryptedTrigger = "serialized queue data";
            var expected = DpapiWrapper.Encrypt(decryptedTrigger);
            mockFileWrapper.Setup(o => o.ReadAllText(fileName)).Returns(expected);
            var expectedTrigger = new TriggerQueue
            {
                TriggerId = triggerId
            };
            mockSerializer.Setup(o => o.Deserialize<ITriggerQueue>(decryptedTrigger)).Returns(expectedTrigger);

            var catalog = GetTriggersCatalog(mockDirectoryWrapper.Object, mockFileWrapper.Object, directory, mockSerializer.Object, mockFileSystemWatcher.Object);

            mockFileSystemWatcher.Raise(fileWatcher => fileWatcher.Changed += null, null, new FileSystemEventArgs(WatcherChangeTypes.Changed, directory, fileName));

            mockDirectoryWrapper.Verify(directoryWrapper => directoryWrapper.GetFiles(directory), Times.Exactly(2));

            Assert.AreEqual(1, catalog.Queues.Count);
            Assert.AreEqual(triggerId, catalog.Queues[0].TriggerId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_FileSystemWatcher_Deleted()
        {
            var directory = "some path";
            var fileName = Guid.NewGuid().ToString() + ".bite";
            var files = new string[] { fileName };
            var mockDirectoryWrapper = new Mock<IDirectory>();
            mockDirectoryWrapper.Setup(directoryWrapper => directoryWrapper.GetFiles("some path")).Returns(files);
            var mockFileWrapper = new Mock<IFile>();
            var mockSerializer = new Mock<ISerializer>();

            var mockFileSystemWatcher = new Mock<IFileSystemWatcher>();

            var triggerId = Guid.NewGuid();
            var decryptedTrigger = "serialized queue data";
            var expected = DpapiWrapper.Encrypt(decryptedTrigger);
            mockFileWrapper.Setup(o => o.ReadAllText(fileName)).Returns(expected);
            var expectedTrigger = new TriggerQueue
            {
                TriggerId = triggerId
            };
            mockSerializer.Setup(o => o.Deserialize<ITriggerQueue>(decryptedTrigger)).Returns(expectedTrigger);

            var catalog = GetTriggersCatalog(mockDirectoryWrapper.Object, mockFileWrapper.Object, directory, mockSerializer.Object, mockFileSystemWatcher.Object);

            mockFileSystemWatcher.Raise(fileWatcher => fileWatcher.Deleted += null, null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, directory, fileName));

            mockDirectoryWrapper.Verify(directoryWrapper => directoryWrapper.GetFiles(directory), Times.Exactly(2));

            Assert.AreEqual(1, catalog.Queues.Count);
            Assert.AreEqual(triggerId, catalog.Queues[0].TriggerId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_FileSystemWatcher_Renamed()
        {
            var directory = "some path";
            var fileName = Guid.NewGuid().ToString() + ".bite";
            var oldName = Guid.NewGuid().ToString() + ".bite";
            var files = new string[] { fileName };
            var mockDirectoryWrapper = new Mock<IDirectory>();
            mockDirectoryWrapper.Setup(directoryWrapper => directoryWrapper.GetFiles("some path")).Returns(files);
            var mockFileWrapper = new Mock<IFile>();
            var mockSerializer = new Mock<ISerializer>();

            var mockFileSystemWatcher = new Mock<IFileSystemWatcher>();

            var triggerId = Guid.NewGuid();
            var decryptedTrigger = "serialized queue data";
            var expected = DpapiWrapper.Encrypt(decryptedTrigger);
            mockFileWrapper.Setup(o => o.ReadAllText(fileName)).Returns(expected);
            var expectedTrigger = new TriggerQueue
            {
                TriggerId = triggerId
            };
            mockSerializer.Setup(o => o.Deserialize<ITriggerQueue>(decryptedTrigger)).Returns(expectedTrigger);

            var catalog = GetTriggersCatalog(mockDirectoryWrapper.Object, mockFileWrapper.Object, directory, mockSerializer.Object, mockFileSystemWatcher.Object);

            mockFileSystemWatcher.Raise(fileWatcher => fileWatcher.Renamed += null, null, new RenamedEventArgs(WatcherChangeTypes.Renamed, directory, fileName, oldName));

            mockDirectoryWrapper.Verify(directoryWrapper => directoryWrapper.GetFiles(directory), Times.Exactly(2));

            Assert.AreEqual(1, catalog.Queues.Count);
            Assert.AreEqual(triggerId, catalog.Queues[0].TriggerId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggersCatalog))]
        public void TriggersCatalog_FileSystemWatcher_Error()
        {
            var directory = "some path";
            var fileName = Guid.NewGuid().ToString() + ".bite";
            var oldName = Guid.NewGuid().ToString() + ".bite";
            var files = new string[] { fileName };
            var mockDirectoryWrapper = new Mock<IDirectory>();
            mockDirectoryWrapper.Setup(directoryWrapper => directoryWrapper.GetFiles("some path")).Returns(files);
            var mockFileWrapper = new Mock<IFile>();
            var mockSerializer = new Mock<ISerializer>();

            var mockFileSystemWatcher = new Mock<IFileSystemWatcher>();

            var triggerId = Guid.NewGuid();
            var decryptedTrigger = "serialized queue data";
            var expected = DpapiWrapper.Encrypt(decryptedTrigger);
            mockFileWrapper.Setup(o => o.ReadAllText(fileName)).Returns(expected);
            var expectedTrigger = new TriggerQueue
            {
                TriggerId = triggerId
            };
            mockSerializer.Setup(o => o.Deserialize<ITriggerQueue>(decryptedTrigger)).Returns(expectedTrigger);

            var catalog = GetTriggersCatalog(mockDirectoryWrapper.Object, mockFileWrapper.Object, directory, mockSerializer.Object, mockFileSystemWatcher.Object);

            mockFileSystemWatcher.Raise(fileWatcher => fileWatcher.Error += null, null, new ErrorEventArgs(new Exception()));

            mockDirectoryWrapper.Verify(directoryWrapper => directoryWrapper.GetFiles(directory), Times.Exactly(1));

            Assert.AreEqual(1, catalog.Queues.Count);
            Assert.AreEqual(triggerId, catalog.Queues[0].TriggerId);
        }

        TriggerQueue SaveRandomTriggerQueue(ITriggersCatalog triggerCatalog, Guid triggerId)
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
                TriggerId = triggerId,
                QueueSourceId = mockResource.Object.ResourceID,
                QueueName = queue,
                WorkflowName = workflowName
            };

            triggerCatalog.SaveTriggerQueue(triggerQueueEvent);

            return triggerQueueEvent;
        }

        ITriggersCatalog GetTriggersCatalog(IDirectory directory, IFile file, string queueTriggersPath, ISerializer serializer, IFileSystemWatcher fileSystemWatcherWrapper)
        {
            return new TriggersCatalog(directory, file, queueTriggersPath, serializer, fileSystemWatcherWrapper);
        }
    }
}
