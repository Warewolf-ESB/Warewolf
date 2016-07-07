using System;
using System.Collections.Generic;
using System.Text;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    public class Dev2ActivityIOBrokerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateInstance_GivenThrowsNoExpetion_ShouldBeIActivityOperationsBroker()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            try
            {
                var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
                Assert.IsInstanceOfType(activityOperationsBroker, typeof(IActivityOperationsBroker));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Get_GivenPath_ShouldReturnFileEncodingContents()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            var fileMock = new Mock<IActivityIOOperationsEndPoint>();
            fileMock.Setup(point => point.Get(It.IsAny<IActivityIOPath>(), It.IsAny<List<string>>())).Returns(new ByteBuffer(new byte[3]));
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var stringEncodingContents = activityOperationsBroker.Get(fileMock.Object, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(stringEncodingContents);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Get_GivenPath_ShouldReturnFileDecodedContents()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            var fileMock = new Mock<IActivityIOOperationsEndPoint>();

            const string iAmGood = "I am good";
            fileMock.Setup(point => point.Get(It.IsAny<IActivityIOPath>(), It.IsAny<List<string>>())).Returns(new ByteBuffer(Encoding.ASCII.GetBytes(iAmGood)));
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var stringEncodingContents = activityOperationsBroker.Get(fileMock.Object, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(stringEncodingContents);
            Assert.AreEqual(iAmGood, stringEncodingContents);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsBase64_GivenStartsWithBase64_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            PrivateObject obj = new PrivateObject(activityOperationsBroker);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var invoke = obj.Invoke("IsBase64", "Content-Type:BASE64SomeJunkdata");
            //---------------Test Result -----------------------
            Assert.IsTrue(bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetFileNameFromEndPoint_GivenEndPoint_ShouldReturnFileName()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            PrivateObject obj = new PrivateObject(activityOperationsBroker);
            var mockEndpoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockActIo = new Mock<IActivityIOPath>();
            const string path = "C:\\Home\\txt\\a.srx";
            mockActIo.Setup(p => p.Path).Returns(path);
            mockEndpoint.Setup(point => point.PathSeperator()).Returns(",");
            mockEndpoint.Setup(point => point.IOPath).Returns(mockActIo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var pathReturned = obj.Invoke("GetFileNameFromEndPoint", mockEndpoint.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual(path, pathReturned);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetFileNameFromEndPoint_GivenEndPoint_ShouldReturnFileName_Overload()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            PrivateObject obj = new PrivateObject(activityOperationsBroker);
            var mockEndpoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockActIo = new Mock<IActivityIOPath>();
            const string path = "C:\\Home\\txt\\a.srx";
            mockActIo.Setup(p => p.Path).Returns(path);
            mockEndpoint.Setup(point => point.PathSeperator()).Returns(",");
            mockEndpoint.Setup(point => point.IOPath).Returns(mockActIo.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var pathReturned = obj.Invoke("GetFileNameFromEndPoint", mockEndpoint.Object, mockActIo.Object);
            //---------------Test Result -----------------------
            Assert.AreEqual(path, pathReturned);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ListDirectory_GivenFilesAndFolders_ShouldReturnEmptyList()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var activityIOPaths = activityOperationsBroker.ListDirectory(endPoint.Object, ReadTypes.FilesAndFolders);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, activityIOPaths.Count);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ListDirectory_GivenFiles_ShouldReturnEmptyList()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListFilesInDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var activityIOPaths = activityOperationsBroker.ListDirectory(endPoint.Object, ReadTypes.Files);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, activityIOPaths.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ListDirectory_GivenFolders_ShouldReturnEmptyList()
        {
            //---------------Set up test pack-------------------
            var activityOperationsBroker = ActivityIOFactory.CreateOperationsBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListFoldersInDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var activityIOPaths = activityOperationsBroker.ListDirectory(endPoint.Object, ReadTypes.Folders);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, activityIOPaths.Count);
        }
    }
}
