﻿/*
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
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerBaseDriver))]
        public void ActivityIOBrokerBaseDriver_GetFileNameFromEndPoint_GivenEndPoint_ShouldReturnFileName()
        {
            var driver = new ActivityIOBrokerBaseDriver();
            var mockEndpoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockActIo = new Mock<IActivityIOPath>();
            const string path = "C:\\Home\\txt\\a.srx";
            mockActIo.Setup(p => p.Path).Returns(path);
            mockEndpoint.Setup(point => point.PathSeperator()).Returns(",");
            mockEndpoint.Setup(point => point.IOPath).Returns(mockActIo.Object);

            var pathReturned = driver.GetFileNameFromEndPoint(mockEndpoint.Object);

            Assert.AreEqual(path, pathReturned);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerBaseDriver))]
        public void ActivityIOBrokerBaseDriver_GetFileNameFromEndPoint_GivenEndPoint_ShouldReturnFileName_Overload()
        {
            var driver = new ActivityIOBrokerBaseDriver();
            var mockEndpoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockActIo = new Mock<IActivityIOPath>();
            const string path = "C:\\Home\\txt\\a.srx";
            mockActIo.Setup(p => p.Path).Returns(path);
            mockEndpoint.Setup(point => point.PathSeperator()).Returns(",");
            mockEndpoint.Setup(point => point.IOPath).Returns(mockActIo.Object);

            var pathReturned = driver.GetFileNameFromEndPoint(mockEndpoint.Object, mockActIo.Object);

            Assert.AreEqual(path, pathReturned);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerBaseDriver))]

        public void ActivityIOBrokerBaseDriver_ListDirectory_FilesAndFolders()
        {
            var broker = new ActivityIOBrokerBaseDriver();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var ioPath = new Mock<IActivityIOPath>().Object;
            endPoint.Setup(o => o.IOPath).Returns(ioPath);
            var mockList = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListDirectory(It.IsAny<IActivityIOPath>())).Returns(mockList.Object);

            var returnedList = broker.ListDirectory(endPoint.Object, ReadTypes.FilesAndFolders);

            Assert.AreEqual(0, returnedList.Count);
            Assert.AreEqual(mockList.Object, returnedList);

            endPoint.Verify(o => o.ListDirectory(ioPath), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerBaseDriver))]
        public void ActivityIOBrokerBaseDriver_ListDirectory_Files()
        {
            var broker = new ActivityIOBrokerBaseDriver();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var ioPath = new Mock<IActivityIOPath>().Object;
            endPoint.Setup(o => o.IOPath).Returns(ioPath);
            var mockList = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListFilesInDirectory(It.IsAny<IActivityIOPath>())).Returns(mockList.Object);

            var returnedList = broker.ListDirectory(endPoint.Object, ReadTypes.Files);

            Assert.AreEqual(0, returnedList.Count);
            Assert.AreEqual(mockList.Object, returnedList);

            endPoint.Verify(o => o.ListFilesInDirectory(ioPath), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerBaseDriver))]
        public void ActivityIOBrokerBaseDriver_ListDirectory_Folders()
        {
            var broker = new ActivityIOBrokerBaseDriver();
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var ioPath = new Mock<IActivityIOPath>().Object;
            endPoint.Setup(o => o.IOPath).Returns(ioPath);
            var mockList = new Mock<IList<IActivityIOPath>>();
            endPoint.Setup(point => point.ListFoldersInDirectory(It.IsAny<IActivityIOPath>())).Returns(mockList.Object);

            var returnedList = broker.ListDirectory(endPoint.Object, ReadTypes.Folders);

            Assert.AreEqual(0, returnedList.Count);
            Assert.AreEqual(mockList.Object, returnedList);

            endPoint.Verify(o => o.ListFoldersInDirectory(ioPath), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerBaseDriver))]
        public void ActivityIOBrokerBaseDriver_CreateDirectory_GivenValidInterfaces_ShouldCallsCreateDirectoryCorrectly()
        {
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);
            var endPoint = new Mock<IActivityIOOperationsEndPoint>();
            var ioPath = new Mock<IActivityIOPath>().Object;
            endPoint.Setup(o => o.IOPath).Returns(ioPath);
            endPoint.Setup(o => o.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);

            var driver = new ActivityIOBrokerBaseDriver();
            var result = driver.CreateDirectory(endPoint.Object, dev2CrudOperationTO);

            Assert.IsTrue(result);
            endPoint.Verify(o => o.CreateDirectory(ioPath, dev2CrudOperationTO));
        }
    }
}
