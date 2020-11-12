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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_GetBytes()
        {
            var someStringBytes = Encoding.UTF8.GetBytes("some string");
            
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockPath = new Mock<IActivityIOOperationsEndPoint>();
            var mockIoPath = new Mock<IActivityIOPath>();
            var ioPath = mockIoPath.Object;
            mockPath.Setup(o => o.IOPath).Returns(ioPath);
            mockPath.Setup(o => o.Get(ioPath, It.IsAny<List<string>>()))
                .Callback<IActivityIOPath, List<string>>((path, filesToRemove) => Assert.AreEqual(0, filesToRemove.Count))
                .Returns(new MemoryStream(someStringBytes));

            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object);
            var result = broker.GetBytes(mockPath.Object);
            
            CollectionAssert.AreEqual(someStringBytes, result);

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
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object, mockZipFileFactory.Object);

            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            var dst = mockDst.Object;

            var mockArgs = new Mock<IDev2PutRawOperationTO>();
            var args = mockArgs.Object;

            mockImplementation.Setup(o => o.CreateTmpFile()).Returns("tmp file name");
            mockImplementation.Setup(o => o.MoveTmpFileToDestination(dst, "tmp file name")).Returns(ActivityIOBrokerBaseDriver.ResultOk);
            mockDst.Setup(o => o.RequiresLocalTmpStorage()).Returns(true);

            var result = broker.PutRaw(dst, args);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultOk, result);

            mockDst.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);
            mockImplementation.Verify(o => o.CreateTmpFile(), Times.Once);
            mockImplementation.Verify(o => o.WriteToLocalTempStorage(dst, args, "tmp file name"), Times.Once);
            mockImplementation.Verify(o => o.MoveTmpFileToDestination(dst, "tmp file name"), Times.Once);
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
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object, mockZipFileFactory.Object);

            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            mockDst.Setup(o => o.PathExist(It.IsAny<Dev2ActivityIOPath>())).Returns(true);
            var dst = mockDst.Object;
            var mockArgs = new Mock<IDev2PutRawOperationTO>();
            var args = mockArgs.Object;

            mockImplementation.Setup(o => o.CreateTmpFile()).Returns("tmp file name");
            mockImplementation.Setup(o => o.WriteToRemoteTempStorage(dst, args, "tmp file name")).Returns(ActivityIOBrokerBaseDriver.ResultOk);

            var result = broker.PutRaw(dst, args);

            Assert.AreEqual(ActivityIOBrokerBaseDriver.ResultOk, result);

            mockDst.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);
            mockDst.Verify(o => o.PathExist(It.IsAny<Dev2ActivityIOPath>()), Times.Once);

            mockImplementation.Verify(o => o.CreateTmpFile(), Times.Once);
            mockImplementation.Verify(o => o.WriteToRemoteTempStorage(dst, args, "tmp file name"), Times.Once);
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
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object, mockZipFileFactory.Object);

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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_PutRaw_FileNotExists_WriteData_Success()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockImplementation = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object, mockZipFileFactory.Object);

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
        public void Dev2ActivityIOBroker_PutRaw_FileNotExists_CreateEndPoint_Fails()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockImplementation = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object, mockZipFileFactory.Object);

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
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object, mockZipFileFactory.Object);

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
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object, mockZipFileFactory.Object);

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
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object, mockZipFileFactory.Object);

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
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object, mockZipFileFactory.Object);

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
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object, mockImplementation.Object, mockValidator.Object, mockZipFileFactory.Object);

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
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);

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
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);

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
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);

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
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Rename_GivenSourceAndDestinationDifferentPathType_ShouldThrowExc()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);

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
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Rename_GivenSourceAndDestinationSamePathTypePathExistsOverwriteFalse_ShouldThrowException()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);

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
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Rename_GivenSourceAndDestinationSamePathTypePathExistsOverwriteTrue_ShouldDeleteDestFile()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);

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
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Rename_GivenSourceAndDestinationTypesNotEqual()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);

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
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Copy_SourceDirectoryInfoFails()
        {
            string expectedOutcome = ActivityIOBrokerBaseDriver.ResultBad;
            IDirectoryInfo directoryInfo = null;
            Dev2ActivityIOBroker_Copy_DestinationPutFails_Implementation(directoryInfo, 0, expectedOutcome);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Copy_DestinationPut()
        {
            string expectedOutcome = ActivityIOBrokerBaseDriver.ResultOk;
            var srcDirectory = new Mock<IDirectoryInfo>().Object;
            Dev2ActivityIOBroker_Copy_DestinationPutFails_Implementation(srcDirectory, 0, expectedOutcome);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Copy_DestinationPutFails()
        {
            string expectedOutcome = ActivityIOBrokerBaseDriver.ResultBad;
            var srcDirectory = new Mock<IDirectoryInfo>().Object;

            Dev2ActivityIOBroker_Copy_DestinationPutFails_Implementation(srcDirectory, -1, expectedOutcome);
        }

        void Dev2ActivityIOBroker_Copy_DestinationPutFails_Implementation(IDirectoryInfo srcDirectory, int putReturnCode, string expectedOutcome)
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);

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
            if (srcDirectory != null)
            {
                srcFileInfo.Setup(o => o.Directory).Returns(srcDirectory);
            }
            mockFile.Setup(o => o.Info("src path")).Returns(srcFileInfo.Object);

            var stream = new MemoryStream();

            mockSrc.Setup(o => o.Get(srcIoPath, It.IsAny<List<string>>())).Returns(stream);


            var src = mockSrc.Object;
            var dst = mockDst.Object;
            var args = mockArgs.Object;

            var srcDirString = srcDirectory?.ToString();
            mockDst.Setup(o => o.Put(stream, dst.IOPath, args, srcDirString, It.IsAny<List<string>>())).Returns(putReturnCode);

            string ok = null;

            mockValidator.Setup(o => o.ValidateCopySourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>()))
                .Callback<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, IDev2CRUDOperationTO, Func<string>>((src1, dst1, args1, func) => {
                    ok = func();
                })
                .Returns(() => ok);


            var result = broker.Copy(src, dst, args);

            Assert.AreEqual(expectedOutcome, result);

            mockSrc.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);

            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Copy_RequiresLocalTmpStorage()
        {
            string expectedOutcome = ActivityIOBrokerBaseDriver.ResultOk;
            var putReturnCode = 0;
            IDirectoryInfo srcDirectory = null;
            bool srcHasRoot = false;
            Dev2ActivityIOBroker_Copy_RequiresLocalTmpStorage(srcDirectory, putReturnCode, expectedOutcome, srcHasRoot);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Copy_RequiresLocalTmpStorage_SrcHasRoot()
        {
            string expectedOutcome = ActivityIOBrokerBaseDriver.ResultOk;
            var putReturnCode = 0;
            IDirectoryInfo srcDirectory = null;
            bool srcHasRoot = true;
            Dev2ActivityIOBroker_Copy_RequiresLocalTmpStorage(srcDirectory, putReturnCode, expectedOutcome, srcHasRoot);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Copy_RequiresLocalTmpStorage_PutFails()
        {
            string expectedOutcome = ActivityIOBrokerBaseDriver.ResultBad;
            var putReturnCode = -1;
            IDirectoryInfo srcDirectory = null;
            bool srcHasRoot = true;
            Dev2ActivityIOBroker_Copy_RequiresLocalTmpStorage(srcDirectory, putReturnCode, expectedOutcome, srcHasRoot);
        }

        void Dev2ActivityIOBroker_Copy_RequiresLocalTmpStorage(IDirectoryInfo srcDirectory, int putReturnCode, string expectedOutcome, bool srcHasRoot)
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);

            var mockSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockSrcIoPath = new Mock<IActivityIOPath>();
            if (srcHasRoot) {
                mockSrcIoPath.Setup(o => o.Path).Returns(@"c:\src path");
            } else
            {
                mockSrcIoPath.Setup(o => o.Path).Returns("src path");
            }
            var srcIoPath = mockSrcIoPath.Object;
            mockSrc.Setup(o => o.IOPath).Returns(srcIoPath);
            mockSrc.Setup(o => o.RequiresLocalTmpStorage()).Returns(true);
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
            if (srcDirectory != null)
            {
                srcFileInfo.Setup(o => o.Directory).Returns(srcDirectory);
            }
            mockFile.Setup(o => o.Info("src path")).Returns(srcFileInfo.Object);

            var stream = new MemoryStream();

            mockSrc.Setup(o => o.Get(srcIoPath, It.IsAny<List<string>>())).Returns(stream);


            var src = mockSrc.Object;
            var dst = mockDst.Object;
            var args = mockArgs.Object;

            if (srcHasRoot)
            {
                mockDst.Setup(o => o.Put(stream, dst.IOPath, args, @"c:\", It.IsAny<List<string>>())).Returns(putReturnCode);
            } else {
                mockDst.Setup(o => o.Put(stream, dst.IOPath, args, null, It.IsAny<List<string>>())).Returns(putReturnCode);
            }

            string ok = null;

            mockValidator.Setup(o => o.ValidateCopySourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>()))
                .Callback<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, IDev2CRUDOperationTO, Func<string>>((src1, dst1, args1, func) => {
                    ok = func();
                })
                .Returns(() => ok);


            var result = broker.Copy(src, dst, args);

            Assert.AreEqual(expectedOutcome, result);

            mockSrc.Verify(o => o.RequiresLocalTmpStorage(), Times.Once);

            if (srcHasRoot)
            {
                mockDst.Verify(o => o.Put(stream, dst.IOPath, args, @"c:\", It.IsAny<List<string>>()), Times.Once);
            } else
            {
                mockDst.Verify(o => o.Put(stream, dst.IOPath, args, null, It.IsAny<List<string>>()), Times.Once);
            }
            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Move()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);


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


            broker.Move(mockSrc.Object, mockDst.Object, args);

            mockValidator.Verify(o => o.ValidateCopySourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>()), Times.Once);
            mockSrc.Verify(o => o.Delete(srcPath), Times.Once);

            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Move_Fails()
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);


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

            mockValidator.Setup(o => o.ValidateCopySourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>())).Returns(ActivityIOBrokerBaseDriver.ResultBad);

            broker.Move(mockSrc.Object, mockDst.Object, args);

            mockValidator.Verify(o => o.ValidateCopySourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>()), Times.Once);
            mockSrc.Verify(o => o.Delete(srcPath), Times.Never);

            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Move_CopyThrows()
        {
            var exception = new Exception("some exception");

            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);


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

            mockValidator.Setup(o => o.ValidateCopySourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>())).Throws(exception);

            var hadException = false;
            try
            {
                broker.Move(mockSrc.Object, mockDst.Object, args);
            } catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(exception, e);
            }
            Assert.IsTrue(hadException);

            mockValidator.Verify(o => o.ValidateCopySourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>()), Times.Once);
            mockSrc.Verify(o => o.Delete(srcPath), Times.Never);

            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_UnZip()
        {
            Dev2ActivityIOBroker_UnZip_Implementation(ActivityIOBrokerBaseDriver.ResultOk, false, false);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_UnZip_SrcRequiresLocalStorage()
        {
            Dev2ActivityIOBroker_UnZip_Implementation(ActivityIOBrokerBaseDriver.ResultOk, true, false);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_UnZip_DstRequiresLocalStorage()
        {
            Dev2ActivityIOBroker_UnZip_Implementation(ActivityIOBrokerBaseDriver.ResultOk, false, true);
        }

        void Dev2ActivityIOBroker_UnZip_Implementation(string expectedStatus, bool srcRequiresLocalTempStorage, bool dstRequiresLocalTempStorage)
        {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);

            var args = new Dev2UnZipOperationTO("some password", true);

            var mockSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockSrcPath = new Mock<IActivityIOPath>();
            mockSrcPath.Setup(o => o.Path).Returns("some src path");
            var srcPath = mockSrcPath.Object;
            mockSrc.Setup(o => o.IOPath).Returns(srcPath);
            mockSrc.Setup(o => o.RequiresLocalTmpStorage()).Returns(srcRequiresLocalTempStorage);
            var stream = new MemoryStream();
            mockSrc.Setup(o => o.Get(srcPath, It.IsAny<List<string>>())).Returns(stream);
            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            var dstPath = new Mock<IActivityIOPath>().Object;
            mockDst.Setup(o => o.IOPath).Returns(dstPath);
            mockDst.Setup(o => o.PathExist(dstPath)).Returns(true);
            mockDst.Setup(o => o.RequiresLocalTmpStorage()).Returns(dstRequiresLocalTempStorage);

            var src = mockSrc.Object;
            var dst = mockDst.Object;


            string ok = null;
            mockValidator.Setup(o => o.ValidateUnzipSourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>()))
                .Callback<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, IDev2UnZipOperationTO, Func<string>>((src1, dst1, args1, func) => {
                    ok = func();
                })
                .Returns(() => ok);

            if (dstRequiresLocalTempStorage)
            {
                mockValidator.Setup(o => o.ValidateCopySourceDestinationFileOperation(It.IsAny<IActivityIOOperationsEndPoint>(), dst, It.IsAny<IDev2CRUDOperationTO>(), It.IsAny<Func<string>>())).Returns(ActivityIOBrokerBaseDriver.ResultOk);
            }

            var zip = new Mock<IIonicZipFileWrapper>().Object;
            if (srcRequiresLocalTempStorage)
            {
                mockDriver.Setup(o => o.CreateTmpFile()).Returns("tmp file name");
                mockZipFileFactory.Setup(o => o.Read("tmp file name")).Returns(zip);
            }
            else
            {
                mockZipFileFactory.Setup(o => o.Read(stream)).Returns(zip);
            }

            if (dstRequiresLocalTempStorage)
            {
                mockCommon.Setup(o => o.CreateTmpDirectory()).Returns(@"c:\mock tmp dir name");

            }

            var result = broker.UnZip(src, dst, args);

            Assert.AreEqual(expectedStatus, result);

            if (srcRequiresLocalTempStorage)
            {
                mockZipFileFactory.Verify(o => o.Read("tmp file name"), Times.Once);
                mockFile.Verify(o => o.Delete("tmp file name"), Times.Once);
                mockDriver.Verify(o => o.CreateTmpFile(), Times.Once);
            }
            else
            {
                mockZipFileFactory.Verify(o => o.Read(stream), Times.Once);
            }
            if (dstRequiresLocalTempStorage)
            {
                mockValidator.Verify(o => o.ValidateCopySourceDestinationFileOperation(It.IsAny<IActivityIOOperationsEndPoint>(), dst, It.IsAny<IDev2CRUDOperationTO>(), It.IsAny<Func<string>>()), Times.Once);
            }
            else
            {
                mockCommon.Verify(o => o.ExtractFile(args, zip, dstPath.Path), Times.Once);
                mockValidator.Verify(o => o.ValidateUnzipSourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>()), Times.Once);
            }

            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.AtLeastOnce);
        }



        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Zip()
        {
            var path = @"c:\some src path";
            Dev2ActivityIOBroker_Zip_Implementation(false, path);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Zip_Directory()
        {
            var path = @"c:\some src path";
            Dev2ActivityIOBroker_Zip_Implementation(true, path);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Zip_StarWildcard()
        {
            var path = @"c:\some src path\*";
            Dev2ActivityIOBroker_Zip_Implementation(false, path);
        }

        void Dev2ActivityIOBroker_Zip_Implementation(bool isDirectory, string srcPathString) {
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();
            var mockDriver = new Mock<IActivityIOBrokerMainDriver>();
            var mockValidator = new Mock<IActivityIOBrokerValidatorDriver>();
            var mockZipFileFactory = new Mock<IIonicZipFileWrapperFactory>();
            var broker = new Dev2ActivityIOBroker(mockFile.Object, mockCommon.Object, mockDriver.Object, mockValidator.Object, mockZipFileFactory.Object);

            var args = new Dev2ZipOperationTO("some ratio", "some password", "some name", true);

            var mockSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockSrcPath = new Mock<IActivityIOPath>();
            mockSrcPath.Setup(o => o.Path).Returns(srcPathString);
            var srcPath = mockSrcPath.Object;
            if (isDirectory)
            {
                mockSrc.Setup(o => o.PathIs(srcPath)).Returns(enPathType.Directory);
            }
            mockSrc.Setup(o => o.IOPath).Returns(srcPath);
            var stream = new MemoryStream();
            mockSrc.Setup(o => o.Get(srcPath, It.IsAny<List<string>>())).Returns(stream);
            var mockDst = new Mock<IActivityIOOperationsEndPoint>();
            var dstPath = new Mock<IActivityIOPath>().Object;
            mockDst.Setup(o => o.IOPath).Returns(dstPath);

            var src = mockSrc.Object;
            var dst = mockDst.Object;

            string ok = null;
            mockValidator.Setup(o => o.ValidateZipSourceDestinationFileOperation(src, dst, args, It.IsAny<Func<string>>()))
                .Callback<IActivityIOOperationsEndPoint, IActivityIOOperationsEndPoint, IDev2ZipOperationTO, Func<string>>((src1, dst1, args1, func) =>
                {
                    ok = func();
                })
                .Returns(() => ok)
                .Verifiable();

            if (isDirectory)
            {
                mockDriver.Setup(o => o.ZipDirectoryToALocalTempFile(src, args)).Returns("some result one");
            }
            else
            {
                mockDriver.Setup(o => o.ZipFileToALocalTempFile(src, args)).Returns("some result two");
            }

            broker.Zip(src, dst, args);

            if (isDirectory || srcPathString.Contains("*"))
            {
                mockDriver.Verify(o => o.ZipDirectoryToALocalTempFile(src, args), Times.Once);
            }
            else
            {
                mockDriver.Verify(o => o.ZipFileToALocalTempFile(src, args), Times.Once);
            }
            mockDriver.Verify(o => o.TransferTempZipFileToDestination(src, dst, args, It.IsAny<string>()), Times.Once);
            mockValidator.Verify();

            mockDriver.Verify(o => o.RemoveAllTmpFiles(), Times.AtLeastOnce);
        }
    }
}
