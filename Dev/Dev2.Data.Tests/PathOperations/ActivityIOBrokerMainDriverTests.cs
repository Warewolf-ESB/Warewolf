/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class ActivityIOBrokerDriverTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerMainDriver))]
        public void Dev2ActivityIOBroker_RemoveTmpFile()
        {
            //---------------Set up test pack-------------------
            var file = new Mock<IFile>();
            var common = new Mock<ICommon>();

            var driver = new ActivityIOBrokerMainDriver(file.Object, common.Object);

            driver.RemoveTmpFile("some file");

            file.Verify(o => o.Delete("some file"), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerMainDriver))]
        public void Dev2ActivityIOBroker_RemoveTmpFile_GivenEmptyFile_ShouldThrowAndLogException()
        {
            var exception = new Exception("empty file name");
            var file = new Mock<IFile>();
            file.Setup(o => o.Delete("")).Throws(exception);

            var common = new Mock<ICommon>();

            var driver = new ActivityIOBrokerMainDriver(file.Object, common.Object);

            var hadException = false;
            try
            {
                driver.RemoveTmpFile(string.Empty);
            }
            catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(exception, e);
            }
            Assert.IsTrue(hadException);

            file.Verify(o => o.Delete(""), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_MoveTmpFileToDestination_GiventmpFile()
        {
            //---------------Set up test pack-------------------
            var tempFileName = Path.GetTempFileName();
            try
            {
                var commonMock = new Mock<ICommon>();
                var fileMock = new Mock<IFile>();
                fileMock.Setup(file => file.ReadAllBytes(It.IsAny<string>())).Returns(Encoding.ASCII.GetBytes("Hello world"));
                var driver = new ActivityIOBrokerMainDriver(fileMock.Object, commonMock.Object);
                var privateObject = driver;
                var dst = new Mock<IActivityIOOperationsEndPoint>();
                dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(false);
                dst.Setup(point => point.PathSeperator()).Returns(@"\");
                dst.Setup(point => point.IOPath.Path).Returns(tempFileName);
                dst.Setup(point => point.Put(It.IsAny<Stream>(), It.IsAny<IActivityIOPath>(), It.IsAny<Dev2CRUDOperationTO>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(1);

                dst.Setup(o => o.PathExist(It.IsAny<IActivityIOPath>()))
                    .Returns<IActivityIOPath>(path => {
                        if (path.Path.EndsWith(@"\", StringComparison.InvariantCulture))
                        {
                            return true;
                        }
                        return false;
                    });

                var result = driver.MoveTmpFileToDestination(dst.Object, tempFileName);
                Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultOk, result);

            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_MoveTmpFileToDestination_GivenDirectoryCreateFails_ShouldFail()
        {
            //---------------Set up test pack-------------------
            var tempFileName = Path.GetTempFileName();
            try
            {
                var commonMock = new Mock<ICommon>();
                var fileMock = new Mock<IFile>();
                fileMock.Setup(file => file.ReadAllBytes(It.IsAny<string>())).Returns(Encoding.ASCII.GetBytes("Hello world"));
                var driver = new ActivityIOBrokerMainDriver(fileMock.Object, commonMock.Object);
                var privateObject = driver;
                var dst = new Mock<IActivityIOOperationsEndPoint>();
                dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(false);
                dst.Setup(point => point.PathSeperator()).Returns(@"\");
                dst.Setup(point => point.IOPath.Path).Returns(tempFileName);
                dst.Setup(point => point.Put(It.IsAny<Stream>(), It.IsAny<IActivityIOPath>(), It.IsAny<Dev2CRUDOperationTO>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(1);

                dst.Setup(o => o.PathExist(It.IsAny<IActivityIOPath>()))
                    .Returns<IActivityIOPath>(path => {
                        return false;
                    });

                var result = driver.MoveTmpFileToDestination(dst.Object, tempFileName);
                Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultBad, result);

            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_MoveTmpFileToDestination_GivenPutFails_ShouldFail()
        {
            //---------------Set up test pack-------------------
            var tempFileName = Path.GetTempFileName();
            try
            {
                var commonMock = new Mock<ICommon>();
                var fileMock = new Mock<IFile>();
                fileMock.Setup(file => file.ReadAllBytes(It.IsAny<string>())).Returns(Encoding.ASCII.GetBytes("Hello world"));
                var driver = new ActivityIOBrokerMainDriver(fileMock.Object, commonMock.Object);
                var privateObject = driver;
                var dst = new Mock<IActivityIOOperationsEndPoint>();
                dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(false);
                dst.Setup(point => point.PathSeperator()).Returns(@"\");
                dst.Setup(point => point.IOPath.Path).Returns(tempFileName);
                dst.Setup(point => point.Put(It.IsAny<Stream>(), It.IsAny<IActivityIOPath>(), It.IsAny<Dev2CRUDOperationTO>(), It.IsAny<string>(), It.IsAny<List<string>>())).Returns(-1);

                dst.Setup(o => o.PathExist(It.IsAny<IActivityIOPath>()))
                    .Returns<IActivityIOPath>(path => {
                        return true;
                    });

                var result = driver.MoveTmpFileToDestination(dst.Object, tempFileName);
                Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultBad, result);

            }
            finally
            {
                File.Delete(tempFileName);
            }
        }
    }
}
