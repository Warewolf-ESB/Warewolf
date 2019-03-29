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
using Ionic.Zip;

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

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_ZipFileToALocalTempFile_RequiresTmpStorage_ExpectSuccess()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var driver = new ActivityIOBrokerMainDriver(mockFile.Object, mockCommon.Object);

            var tmpFileName = driver.CreateTmpFile();


            var mockSrc = new Mock<IActivityIOOperationsEndPoint>();
            mockSrc.Setup(o => o.IOPath.Path).Returns(tmpFileName);
            var mockArgs = new Mock<IDev2ZipOperationTO>();
            mockArgs.Setup(o => o.CompressionRatio).Returns("compressionLevel");

            var filename = driver.ZipFileToALocalTempFile(mockSrc.Object, mockArgs.Object);

            Assert.IsTrue(ZipFile.IsZipFile(filename));
            driver.RemoveAllTmpFiles();

            mockSrc.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);
            mockSrc.Verify(o => o.IOPath.Path, Times.Once);

            mockCommon.Verify(o => o.CreateTmpDirectory(), Times.Never);
            mockCommon.Verify(o => o.ExtractZipCompressionLevel("compressionLevel"), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_ZipFileToALocalTempFile_RequiresTmpStorage_False_ExpectSuccess()
        {
            var mockFile = new Mock<IFile>();
            mockFile.Setup(o => o.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>())).Callback<string, byte[]>((filename, bytes) => new Dev2.Common.Wrappers.FileWrapper().WriteAllBytes(filename, bytes));
            var mockCommon = new Mock<ICommon>();
            var tmpDirectory = new Data.Util.CommonDataUtils().CreateTmpDirectory();
            mockCommon.Setup(o => o.CreateTmpDirectory()).Returns(tmpDirectory);
            var driver = new ActivityIOBrokerMainDriver(mockFile.Object, mockCommon.Object);

            var tmpFileName = driver.CreateTmpFile();


            var mockSrc = new Mock<IActivityIOOperationsEndPoint>();
            mockSrc.Setup(o => o.IOPath.Path).Returns(tmpFileName);
            mockSrc.Setup(o => o.RequiresLocalTmpStorage()).Returns(true);
            mockSrc.Setup(o => o.PathSeperator()).Returns(@"\");
            using (var tmpStream = new MemoryStream(new byte[] { 0x11, 0x22 }))
            {
                mockSrc.Setup(o => o.Get(It.IsAny<IActivityIOPath>(), It.IsAny<List<string>>())).Returns(tmpStream);
                var mockArgs = new Mock<IDev2ZipOperationTO>();
                mockArgs.Setup(o => o.CompressionRatio).Returns("compressionLevel");

                var filename = driver.ZipFileToALocalTempFile(mockSrc.Object, mockArgs.Object);

                Assert.IsTrue(ZipFile.IsZipFile(filename));
                driver.RemoveAllTmpFiles();
                Assert.IsFalse(File.Exists(tmpDirectory));

                mockSrc.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);
                mockSrc.Verify(o => o.IOPath.Path, Times.Exactly(2));

                mockCommon.Verify(o => o.CreateTmpDirectory(), Times.Once);
                mockCommon.Verify(o => o.ExtractZipCompressionLevel("compressionLevel"), Times.Once);

                mockFile.Verify(o => o.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
            }
        }
    }
}
