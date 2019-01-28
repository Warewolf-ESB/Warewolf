/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class ActivityIOBrokerMainDriverTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_GetFileNameFromEndPoint_GivenEndPoint_ShouldReturnFileName()
        {
            var broker = ActivityIOFactory.CreateOperationsBroker();
            var obj = new PrivateType(broker.GetType());
            var mockEndpoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockActIo = new Mock<IActivityIOPath>();
            const string path = "C:\\Home\\txt\\a.srx";
            mockActIo.Setup(p => p.Path).Returns(path);
            mockEndpoint.Setup(point => point.PathSeperator()).Returns(",");
            mockEndpoint.Setup(point => point.IOPath).Returns(mockActIo.Object);

            var pathReturned = obj.InvokeStatic("GetFileNameFromEndPoint", mockEndpoint.Object);

            Assert.AreEqual(path, pathReturned);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_GetFileNameFromEndPoint_GivenEndPoint_ShouldReturnFileName_Overload()
        {
            var broker = ActivityIOFactory.CreateOperationsBroker();
            var prType = new PrivateType(broker.GetType());
            var mockEndpoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockActIo = new Mock<IActivityIOPath>();
            const string path = "C:\\Home\\txt\\a.srx";
            mockActIo.Setup(p => p.Path).Returns(path);
            mockEndpoint.Setup(point => point.PathSeperator()).Returns(",");
            mockEndpoint.Setup(point => point.IOPath).Returns(mockActIo.Object);

            var args = new object[]
            {
                mockEndpoint.Object, mockActIo.Object
            };
            var pathReturned = prType.InvokeStatic("GetFileNameFromEndPoint", args);

            Assert.AreEqual(path, pathReturned);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_ListDirectory_GivenFilesAndFolders_ShouldReturnEmptyList()
        {
            var broker = ActivityIOFactory.CreateOperationsBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);

            var activityIOPaths = broker.ListDirectory(endPoint.Object, ReadTypes.FilesAndFolders);

            Assert.AreEqual(0, activityIOPaths.Count);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_ListDirectory_GivenFiles_ShouldReturnEmptyList()
        {
            var broker = ActivityIOFactory.CreateOperationsBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListFilesInDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);

            var activityIOPaths = broker.ListDirectory(endPoint.Object, ReadTypes.Files);

            Assert.AreEqual(0, activityIOPaths.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_ListDirectory_GivenFolders_ShouldReturnEmptyList()
        {
            var broker = ActivityIOFactory.CreateOperationsBroker();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mock = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListFoldersInDirectory(It.IsAny<IActivityIOPath>())).Returns(mock.Object);

            var activityIOPaths = broker.ListDirectory(endPoint.Object, ReadTypes.Folders);

            Assert.AreEqual(0, activityIOPaths.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_CreateDirectory_GivenValidInterfaces_ShouldCallsCreateDirectoryCorrectly()
        {
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            endPoint.Setup(o => o.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);

            var driver = new ActivityIOBrokerBaseDriver();
            var result = driver.CreateDirectory(endPoint.Object, dev2CrudOperationTO);

            Assert.IsTrue(result);
            endPoint.Verify(o => o.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO));
        }
    }
}
