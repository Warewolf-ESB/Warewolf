/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.Interfaces.Enums;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class Dev2ActivityIOBrokerTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Construct()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();

            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Get()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockPath = new Mock<IActivityIOOperationsEndPoint>();
            var mockIoPath = new Mock<IActivityIOPath>();
            var ioPath = mockIoPath.Object;
            mockPath.Setup(o => o.IOPath).Returns(ioPath);
            mockPath.Setup(o => o.Get(ioPath, It.IsAny<List<string>>()))
                .Callback<IActivityIOPath, List<string>>((path, filesToRemove) => Assert.AreEqual(0, filesToRemove.Count))
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("some string")));

            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object);
            var result = broker.Get(mockPath.Object);

            Assert.AreEqual("some string", result);

            mockPath.Verify(o => o.IOPath, Times.Exactly(1));
            mockPath.Verify(o => o.Get(ioPath, It.IsAny<List<string>>()), Times.Once);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_Get_GivenPath_ShouldReturnFileEncodingContents()
        {
            var broker = ActivityIOFactory.CreateOperationsBroker();
            var fileMock = new Mock<IActivityIOOperationsEndPoint>();
            fileMock.Setup(point => point.Get(It.IsAny<IActivityIOPath>(), It.IsAny<List<string>>())).Returns(new ByteBuffer(Encoding.ASCII.GetBytes("")));

            var stringEncodingContents = broker.Get(fileMock.Object, true);
            Assert.IsNotNull(stringEncodingContents);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_Get_GivenPath_ShouldReturnFileDecodedContents()
        {
            var broker = ActivityIOFactory.CreateOperationsBroker();
            var fileMock = new Mock<IActivityIOOperationsEndPoint>();

            const string iAmGood = "I am good";
            fileMock.Setup(point => point.Get(It.IsAny<IActivityIOPath>(), It.IsAny<List<string>>())).Returns(new ByteBuffer(Encoding.ASCII.GetBytes(iAmGood)));

            var stringEncodingContents = broker.Get(fileMock.Object, true);

            Assert.IsNotNull(stringEncodingContents);
            Assert.AreEqual(iAmGood, stringEncodingContents);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_PutRaw_RequiresLocalTmpStorage()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockImplementation = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object);

            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            var dst = mockDst.Object;

            var mockArgs = new Mock<IDev2PutRawOperationTO>();
            var args = mockArgs.Object;

            mockImplementation.Setup(o => o.CreateTmpFile()).Returns("tmp file name");
            mockImplementation.Setup(o => o.MoveTmpFileToDestination(dst, "tmp file name", ActivityIOBrokerBaseDriver.ResultOk)).Returns(ActivityIOBrokerBaseDriver.ResultOk);
            mockDst.Setup(o => o.RequiresLocalTmpStorage()).Returns(true);

            var result = broker.PutRaw(dst, args);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultOk, result);

            mockDst.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);
            mockImplementation.Verify(o => o.CreateTmpFile(), Times.Once);
            mockImplementation.Verify(o => o.WriteToLocalTempStorage(dst, args, "tmp file name"), Times.Once);
            mockImplementation.Verify(o => o.MoveTmpFileToDestination(dst, "tmp file name", ActivityIOBrokerBaseDriver.ResultOk), Times.Once);
            mockImplementation.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_PutRaw_FileExists()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockImplementation = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object);

            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            mockDst.Setup(o => o.PathExist(It.IsAny<Dev2ActivityIOPath>())).Returns(true);
            var dst = mockDst.Object;
            var mockArgs = new Mock<IDev2PutRawOperationTO>();
            var args = mockArgs.Object;

            mockImplementation.Setup(o => o.CreateTmpFile()).Returns("tmp file name");
            mockImplementation.Setup(o => o.WriteToRemoteTempStorage(dst, args, ActivityIOBrokerBaseDriver.ResultOk, "tmp file name")).Returns(ActivityIOBrokerBaseDriver.ResultOk);

            var result = broker.PutRaw(dst, args);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultOk, result);

            mockDst.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);
            mockDst.Verify(o => o.PathExist(It.IsAny<Dev2ActivityIOPath>()), Times.Once);

            mockImplementation.Verify(o => o.CreateTmpFile(), Times.Once);
            mockImplementation.Verify(o => o.WriteToRemoteTempStorage(dst, args, result, "tmp file name"), Times.Once);
            mockImplementation.Verify(o => o.RemoveTmpFile("tmp file name"), Times.Once);

            mockImplementation.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_PutRaw_FileNotExists_WriteData_Fails()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockImplementation = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object);

            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            var dst = mockDst.Object;
            var mockArgs = new Mock<IDev2PutRawOperationTO>();
            var args = mockArgs.Object;

            mockImplementation.Setup(o => o.CreateEndPoint(dst, It.IsAny<Dev2CRUDOperationTO>(), true)).Returns(ActivityIOBrokerBaseDriver.ResultOk);

            var result = broker.PutRaw(dst, args);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultBad, result);

            mockDst.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);
            mockDst.Verify(o => o.PathExist(It.IsAny<Dev2ActivityIOPath>()), Times.Once);
            mockImplementation.Verify(o => o.CreateEndPoint(dst, It.IsAny<Dev2CRUDOperationTO>(), true), Times.Once);
            mockImplementation.Verify(o => o.WriteDataToFile(args, dst), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_PutRaw_FileNotExists_CreateEndPoint_Fails()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockImplementation = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object);

            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            var dst = mockDst.Object;
            var mockArgs = new Mock<IDev2PutRawOperationTO>();
            var args = mockArgs.Object;

            mockImplementation.Setup(o => o.CreateEndPoint(dst, It.IsAny<Dev2CRUDOperationTO>(), true)).Returns(ActivityIOBrokerBaseDriver.ResultBad);

            var result = broker.PutRaw(dst, args);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultBad, result);

            mockDst.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);
            mockDst.Verify(o => o.PathExist(It.IsAny<Dev2ActivityIOPath>()), Times.Once);
            mockImplementation.Verify(o => o.CreateEndPoint(dst, It.IsAny<Dev2CRUDOperationTO>(), true), Times.Once);
            mockImplementation.Verify(o => o.WriteDataToFile(args, dst), Times.Never);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_PutRaw_FileNotExists()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockImplementation = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object);

            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            var dst = mockDst.Object;
            var mockArgs = new Mock<IDev2PutRawOperationTO>();
            var args = mockArgs.Object;

            mockImplementation.Setup(o => o.CreateEndPoint(dst, It.IsAny<Dev2CRUDOperationTO>(), true)).Returns(ActivityIOBrokerBaseDriver.ResultOk);
            mockImplementation.Setup(o => o.WriteDataToFile(args, dst)).Returns(true);

            var result = broker.PutRaw(dst, args);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultOk, result);

            mockDst.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);
            mockDst.Verify(o => o.PathExist(It.IsAny<Dev2ActivityIOPath>()), Times.Once);
            mockImplementation.Verify(o => o.CreateEndPoint(dst, It.IsAny<Dev2CRUDOperationTO>(), true), Times.Once);
            mockImplementation.Verify(o => o.WriteDataToFile(args, dst), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Delete_GivenDeleteIsFalse_ShouldReturnResultBad()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockImplementation = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object);

            var dstMock = new Mock<IActivityIOOperationsEndPoint>();
            var ioPath = new Mock<IActivityIOPath>().Object;
            dstMock.Setup(o => o.IOPath).Returns(ioPath);
            dstMock.Setup(o => o.Delete(It.IsAny<IActivityIOPath>())).Returns(false);

            var delete = broker.Delete(dstMock.Object);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultBad, delete);

            dstMock.Verify(o => o.Delete(ioPath), Times.Once);
            mockImplementation.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Delete_GivenDeleteThrows_ShouldReturnResultBad()
        {
            var exception = new Exception("some exception");

            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockImplementation = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object);

            var dstMock = new Mock<IActivityIOOperationsEndPoint>();
            var ioPath = new Mock<IActivityIOPath>().Object;
            dstMock.Setup(o => o.IOPath).Returns(ioPath);
            dstMock.Setup(o => o.Delete(It.IsAny<IActivityIOPath>())).Throws(exception);

            var delete = broker.Delete(dstMock.Object);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultBad, delete);

            dstMock.Verify(o => o.Delete(ioPath), Times.Once);
            mockImplementation.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Delete_GivenDeleteIsTrue_ShouldReturnResultOk()
        {
            //---------------Set up test pack-------------------
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockImplementation = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object);

            var dstMock = new Mock<IActivityIOOperationsEndPoint>();
            var ioPath = new Mock<IActivityIOPath>().Object;
            dstMock.Setup(o => o.IOPath).Returns(ioPath);
            dstMock.Setup(o => o.Delete(It.IsAny<IActivityIOPath>())).Returns(true);

            var delete = broker.Delete(dstMock.Object);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultOk, delete);

            dstMock.Verify(o => o.Delete(ioPath), Times.Once);
            mockImplementation.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_ListDirectory_GivenFolders_ShouldReturnEmptyList()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockImplementation = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object);

            var endPoint = new Mock<IActivityIOOperationsEndPoint>().Object;
            var expectedList = new List<IActivityIOPath>();

            var readTypes = ReadTypes.Folders;
            mockImplementation.Setup(o => o.ListDirectory(endPoint, readTypes)).Returns(expectedList);

            var result = broker.ListDirectory(endPoint, readTypes);

            Assert.AreEqual(expectedList, result);

            mockImplementation.Verify(o => o.ListDirectory(endPoint, readTypes), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Create_GivenDestination_ShouldCreateFileCorrectly()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object);

            var dstPath = new Mock<IActivityIOPath>();
            var args = new Dev2CRUDOperationTO(true);
            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            mockDst.Setup(point => point.IOPath.Username).Returns("userName");
            mockDst.Setup(point => point.IOPath.Password).Returns("Password");
            mockDst.Setup(point => point.PathSeperator()).Returns(",");
            mockDst.Setup(point => point.IOPath.Path).Returns("path");
            mockDst.Setup(point => point.PathExist(dstPath.Object)).Returns(true);
            var dst = mockDst.Object;

            mockDriver.Setup(o => o.CreateEndPoint(dst, args, true)).Returns(ActivityIOBrokerBaseDriver.ResultOk);

            var result = broker.Create(dst, args, true);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultOk, result);

            mockCommon.Verify(o => o.ValidateEndPoint(dst, args), Times.Once);
            mockDriver.Verify(o => o.CreateEndPoint(dst, args, true), Times.Once);
            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Create_CreateEndPoint_Fails()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object);

            var dstPath = new Mock<IActivityIOPath>();
            var args = new Dev2CRUDOperationTO(true);
            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            mockDst.Setup(point => point.IOPath.Username).Returns("userName");
            mockDst.Setup(point => point.IOPath.Password).Returns("Password");
            mockDst.Setup(point => point.PathSeperator()).Returns(",");
            mockDst.Setup(point => point.IOPath.Path).Returns("path");
            mockDst.Setup(point => point.PathExist(dstPath.Object)).Returns(true);
            var dst = mockDst.Object;

            mockDriver.Setup(o => o.CreateEndPoint(dst, args, true)).Returns(ActivityIOBrokerBaseDriver.ResultBad);

            var result = broker.Create(mockDst.Object, args, true);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultBad, result);

            mockDriver.Verify(o => o.CreateEndPoint(dst, args, true), Times.Once);
            mockCommon.Verify(o => o.ValidateEndPoint(dst, args), Times.Once);
            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Create_CreateEndPoint_Throws()
        {
            var exception = new Exception("some exception");

            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object);

            var dstPath = new Mock<IActivityIOPath>();
            var args = new Dev2CRUDOperationTO(true);
            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            mockDst.Setup(point => point.IOPath.Username).Returns("userName");
            mockDst.Setup(point => point.IOPath.Password).Returns("Password");
            mockDst.Setup(point => point.PathSeperator()).Returns(",");
            mockDst.Setup(point => point.IOPath.Path).Returns("path");
            mockDst.Setup(point => point.PathExist(dstPath.Object)).Returns(true);
            var dst = mockDst.Object;

            mockDriver.Setup(o => o.CreateEndPoint(dst, args, true)).Throws(exception);

            string result = null;
            var hadException = false;
            try
            {
                result = broker.Create(mockDst.Object, args, true);
            } catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(exception, e);
            }
            Assert.IsTrue(hadException);

            Assert.IsNull(result);

            mockDriver.Verify(o => o.CreateEndPoint(dst, args, true), Times.Once);
            mockCommon.Verify(o => o.ValidateEndPoint(dst, args), Times.Once);
            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_Rename_GivenSourceAndDestinationDifferentPathType_ShouldThrowExc()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object);

            var args = new Dev2CRUDOperationTO(true);
            var src = new Mock<IActivityIOOperationsEndPoint>();
            var dst = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

            var hadException = false;
            try
            {
                broker.Rename(src.Object, dst.Object, args);
            }
            catch (Exception exc)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.SourceAndDestinationNOTFilesOrDirectory, exc.Message);
            }
            Assert.IsTrue(hadException);

            src.Verify(o => o.Delete(It.IsAny<IActivityIOPath>()), Times.Never);
            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_Rename_GivenSourceAndDestinationSamePathTypePathExistsOverwriteFalse_ShouldThrowException()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object);

            var args = new Dev2CRUDOperationTO(false);
            var src = new Mock<IActivityIOOperationsEndPoint>();
            var dst = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);

            var hadException = false;
            try
            {
                broker.Rename(src.Object, dst.Object, args);
            }
            catch (Exception exc)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.DestinationDirectoryExist, exc.Message);
            }
            Assert.IsTrue(hadException);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_Rename_GivenSourceAndDestinationSamePathTypePathExistsOverwriteTrue_ShouldDeleteDestFile()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object);

            var args = new Dev2CRUDOperationTO(true);

            var mockSrc = new Mock<IActivityIOOperationsEndPoint>();
            var srcPath = new Mock<IActivityIOPath>().Object;
            mockSrc.Setup(o => o.IOPath).Returns(srcPath);
            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            var dstPath = new Mock<IActivityIOPath>().Object;
            mockDst.Setup(o => o.IOPath).Returns(dstPath);
            mockDst.Setup(o => o.PathExist(dstPath)).Returns(true);

            var src = mockSrc.Object;
            var dst = mockDst.Object;

            mockValidator.Setup(o => o.ValidateCopySourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>())).Returns(ActivityIOBrokerBaseDriver.ResultOk);


            broker.Rename(mockSrc.Object, mockDst.Object, args);

            mockDst.Verify(o => o.Delete(dstPath), Times.Once);

            mockValidator.Verify(o => o.ValidateCopySourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>()), Times.Once);

            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Dev2ActivityIOBroker_Rename_GivenSourceAndDestinationTypesNotEqual()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object);

            var args = new Dev2CRUDOperationTO(true);

            var mockSrc = new Mock<IActivityIOOperationsEndPoint>();
            mockSrc.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            var srcPath = new Mock<IActivityIOPath>().Object;
            mockSrc.Setup(o => o.IOPath).Returns(srcPath);
            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            var dstPath = new Mock<IActivityIOPath>().Object;
            mockSrc.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
            mockDst.Setup(o => o.IOPath).Returns(dstPath);
            mockDst.Setup(o => o.PathExist(dstPath)).Returns(true);

            var src = mockSrc.Object;
            var dst = mockDst.Object;

            var hadException = false;
            try
            {
                broker.Rename(mockSrc.Object, mockDst.Object, args);
            }
            catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.SourceAndDestinationNOTFilesOrDirectory, e.Message);
            }
            Assert.IsTrue(hadException);

            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.AtLeastOnce);
        }

        [TestMethod]
        public void Dev2ActivityIOBroker_Copy()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object);

            var mockSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockSrcIoPath = new Mock<IActivityIOPath>();
            mockSrcIoPath.Setup(o => o.Path).Returns("src path");
            var srcIoPath = mockSrcIoPath.Object;
            mockSrc.Setup(o => o.IOPath).Returns(srcIoPath);
            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDstIoPath = new Mock<IActivityIOPath>();
            mockDstIoPath.Setup(o => o.Path).Returns("dst path");
            var dstIoPath = mockDstIoPath.Object;
            mockDst.Setup(o => o.PathSeperator()).Returns(@"\");
            mockDst.Setup(o => o.IOPath).Returns(dstIoPath);
            mockDst.Setup(o => o.PathIs(dstIoPath)).Returns(enPathType.Directory);
            var mockArgs = new Mock<IDev2CRUDOperationTO>();
            var srcFileInfo = new Mock<IFileInfo>();
            srcFileInfo.Setup(o => o.Name).Returns("src file name");
            srcFileInfo.Setup(o => o.Directory).Returns(new Mock<IDirectoryInfo>().Object);
            mockFile.Setup(o => o.Info("src path")).Returns(srcFileInfo.Object);

            var stream = new MemoryStream();

            mockSrc.Setup(o => o.Get(srcIoPath, It.IsAny<List<string>>())).Returns(stream);

            var src = mockSrc.Object;
            var dst = mockDst.Object;
            var args = mockArgs.Object;

            int putReturnCode = 1;

            mockDst.Setup(o => o.Put(stream, dst.IOPath, args, "Mock<Dev2.Common.Interfaces.Wrappers.IDirectoryInfo:00000001>.Object", It.IsAny<List<string>>())).Returns(putReturnCode);

            mockValidator.Setup(o => o.ValidateCopySourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>()))
                .Callback<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, IDev2CRUDOperationTO,Func<string>>((src1, dst1, args1, func) => {
                    func();
                })
                .Returns(ActivityIOBrokerBaseDriver.ResultOk);


            var result = broker.Copy(src, dst, args);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultOk, result);

            mockSrc.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);
            mockDst.Verify(o => o.Put(stream, dst.IOPath, args, "Mock<Dev2.Common.Interfaces.Wrappers.IDirectoryInfo:00000001>.Object", It.IsAny<List<string>>()), Times.Once);

            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        [TestMethod]
        public void Dev2ActivityIOBroker_Copy_SourceDirectoryInfoFails()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object);

            var mockSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockSrcIoPath = new Mock<IActivityIOPath>();
            mockSrcIoPath.Setup(o => o.Path).Returns("src path");
            var srcIoPath = mockSrcIoPath.Object;
            mockSrc.Setup(o => o.IOPath).Returns(srcIoPath);
            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDstIoPath = new Mock<IActivityIOPath>();
            mockDstIoPath.Setup(o => o.Path).Returns("dst path");
            var dstIoPath = mockDstIoPath.Object;
            mockDst.Setup(o => o.PathSeperator()).Returns(@"\");
            mockDst.Setup(o => o.IOPath).Returns(dstIoPath);
            mockDst.Setup(o => o.PathIs(dstIoPath)).Returns(enPathType.Directory);
            var mockArgs = new Mock<IDev2CRUDOperationTO>();
            var srcFileInfo = new Mock<IFileInfo>();
            srcFileInfo.Setup(o => o.Name).Returns("src file name");
            mockFile.Setup(o => o.Info("src path")).Returns(srcFileInfo.Object);

            var stream = new MemoryStream();

            mockSrc.Setup(o => o.Get(srcIoPath, It.IsAny<List<string>>())).Returns(stream);


            var src = mockSrc.Object;
            var dst = mockDst.Object;
            var args = mockArgs.Object;

            int putReturnCode = 0;

            mockDst.Setup(o => o.Put(stream, dst.IOPath, args, "Mock<Dev2.Common.Interfaces.Wrappers.IDirectoryInfo:00000001>.Object", It.IsAny<List<string>>())).Returns(putReturnCode);

            mockValidator.Setup(o => o.ValidateCopySourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>()))
                .Callback<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, IDev2CRUDOperationTO, Func<string>>((src1, dst1, args1, func) => {
                    func();
                })
                .Returns(ActivityIOBrokerBaseDriver.ResultOk);


            var result = broker.Copy(src, dst, args);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultBad, result);

            mockSrc.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);
            mockDst.Verify(o => o.Put(stream, dst.IOPath, args, "Mock<Dev2.Common.Interfaces.Wrappers.IDirectoryInfo:00000001>.Object", It.IsAny<List<string>>()), Times.Once);

            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        // Move

        // UnZip

        // Zip
    }
}
