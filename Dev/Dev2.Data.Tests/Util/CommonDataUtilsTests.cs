using System;
using System.Collections.Generic;
using System.IO;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Search;
using Dev2.Common.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Data.Tests.Util
{
    [TestClass]
    public class CommonDataUtilsTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ValidateEndPoint_EmptyPath_ExpectException()
        {
            var commonDataUtils = new CommonDataUtils();
            var mockEndPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockPath = new Mock<IActivityIOPath>();
            mockPath.Setup(o => o.Path).Returns("");
            mockEndPoint.Setup(o => o.IOPath).Returns(mockPath.Object);
            var endPoint = mockEndPoint.Object;
            IDev2CRUDOperationTO dev2CRUDOperationTO = null;

            bool hadException = false;
            try
            {
                commonDataUtils.ValidateEndPoint(endPoint, dev2CRUDOperationTO);
            }
            catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.SourceCannotBeAnEmptyString, e.Message);
            }
            Assert.IsTrue(hadException, "expected exception");

            mockEndPoint.Verify(o => o.IOPath, Times.Once);
            mockPath.Verify(o => o.Path, Times.Once);
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ValidateEndPoint_NullPath_ExpectException()
        {
            var commonDataUtils = new CommonDataUtils();
            var mockEndPoint = new Mock<IActivityIOOperationsEndPoint>();
            mockEndPoint.Setup(o => o.IOPath).Returns(() => null);
            var endPoint = mockEndPoint.Object;
            IDev2CRUDOperationTO dev2CRUDOperationTO = null;

            bool hadException = false;
            try
            {
                commonDataUtils.ValidateEndPoint(endPoint, dev2CRUDOperationTO);
            }
            catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.SourceCannotBeAnEmptyString, e.Message);
            }
            Assert.IsTrue(hadException, "expected exception");

            mockEndPoint.Verify(o => o.IOPath, Times.Once);
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ValidateEndPoint_ValidPath_OverwriteFalse_ExpectException()
        {
            const string expectedPath = "c:\\somePath";
            var commonDataUtils = new CommonDataUtils();
            var mockEndPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockPath = new Mock<IActivityIOPath>();
            mockPath.Setup(o => o.Path).Returns(expectedPath);
            mockEndPoint.Setup(o => o.IOPath).Returns(mockPath.Object);

            mockEndPoint.Setup(o => o.PathExist(mockPath.Object)).Returns(true);

            var endPoint = mockEndPoint.Object;
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(false);

            var hadException = false;
            try
            {
                commonDataUtils.ValidateEndPoint(endPoint, mockDev2CRUDOperationTO.Object);
            }
            catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.DestinationDirectoryExist, e.Message);
            }
            Assert.IsTrue(hadException, "expected exception");

            mockEndPoint.Verify(o => o.IOPath, Times.Exactly(2));
            mockPath.Verify(o => o.Path, Times.Once);

            mockEndPoint.Verify(o => o.PathExist(mockPath.Object), Times.Once);
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ValidateEndPoint_ValidPath_Success()
        {
            const string expectedPath = "c:\\somePath";
            var commonDataUtils = new CommonDataUtils();
            var mockEndPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockPath = new Mock<IActivityIOPath>();
            mockPath.Setup(o => o.Path).Returns(expectedPath);
            mockEndPoint.Setup(o => o.IOPath).Returns(mockPath.Object);

            mockEndPoint.Setup(o => o.PathExist(mockPath.Object)).Returns(true);

            var endPoint = mockEndPoint.Object;
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(true);

            commonDataUtils.ValidateEndPoint(endPoint, mockDev2CRUDOperationTO.Object);
            
            mockEndPoint.Verify(o => o.IOPath, Times.Exactly(2));
            mockPath.Verify(o => o.Path, Times.Once);

            mockEndPoint.Verify(o => o.PathExist(mockPath.Object), Times.Once);
        }
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ValidateEndPoint_ValidPathNotExists_Success()
        {
            const string expectedPath = "c:\\somePath";
            var commonDataUtils = new CommonDataUtils();
            var mockEndPoint = new Mock<IActivityIOOperationsEndPoint>();
            var mockPath = new Mock<IActivityIOPath>();
            mockPath.Setup(o => o.Path).Returns(expectedPath);
            mockEndPoint.Setup(o => o.IOPath).Returns(mockPath.Object);

            mockEndPoint.Setup(o => o.PathExist(mockPath.Object)).Returns(false);

            var endPoint = mockEndPoint.Object;
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(true);
            try
            {
                commonDataUtils.ValidateEndPoint(endPoint, mockDev2CRUDOperationTO.Object);
            }
            catch (Exception e)
            {
                Assert.AreEqual(ErrorResource.SourceCannotBeAnEmptyString, e.Message);
            }
            mockEndPoint.Verify(o => o.IOPath, Times.Exactly(2));
            mockPath.Verify(o => o.Path, Times.Once);

            mockEndPoint.Verify(o => o.PathExist(mockPath.Object), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ExtractFile_NullZip_DoesNotThrow()
        {
            var commonDataUtils = new CommonDataUtils();
            IIonicZipFileWrapper wrapper = null;
            commonDataUtils.ExtractFile(null, wrapper, null);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ExtractFile_NoPassword()
        {
            const string path = "some path";

            var mockUnzipOpTo = new Mock<IDev2UnZipOperationTO>();

            var commonDataUtils = new CommonDataUtils();
            var mockZipFileWrapper = new Mock<IIonicZipFileWrapper>();
            var mockZipEntry = new Mock<IZipEntry>();
            mockZipFileWrapper.Setup(o => o.GetEnumerator()).Returns(new List<IZipEntry>() {
                mockZipEntry.Object
            }.GetEnumerator());

            commonDataUtils.ExtractFile(mockUnzipOpTo.Object, mockZipFileWrapper.Object, path);
            mockZipFileWrapper.Verify(o => o.Dispose(), Times.Once);
            mockZipEntry.Verify(o => o.Extract(path, FileOverwrite.No), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ExtractFile_WithPassword_OverwriteTrue()
        {
            const string somePath = "some path";
            const string somePassword = "some password";

            var mockUnzipOpTo = new Mock<IDev2UnZipOperationTO>();
            mockUnzipOpTo.Setup(o => o.ArchivePassword).Returns(somePassword);
            mockUnzipOpTo.Setup(o => o.Overwrite).Returns(true);

            var commonDataUtils = new CommonDataUtils();
            var mockZipFileWrapper = new Mock<IIonicZipFileWrapper>();
            var mockZipEntry = new Mock<IZipEntry>();
            mockZipFileWrapper.Setup(o => o.GetEnumerator()).Returns(new List<IZipEntry>() {
                mockZipEntry.Object
            }.GetEnumerator());

            commonDataUtils.ExtractFile(mockUnzipOpTo.Object, mockZipFileWrapper.Object, somePath);

            mockUnzipOpTo.VerifyGet(o => o.ArchivePassword, Times.Exactly(2));
            mockZipFileWrapper.VerifySet(o => o.Password = somePassword, Times.Once);
            mockZipFileWrapper.Verify(o => o.Dispose(), Times.Once);
            mockZipEntry.Verify(o => o.Extract(somePath, FileOverwrite.Yes), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ExtractFile_Extract_Fails()
        {
            const string somePath = "some path";
            const string somePassword = "some password";

            var mockUnzipOpTo = new Mock<IDev2UnZipOperationTO>();
            mockUnzipOpTo.Setup(o => o.ArchivePassword).Returns(somePassword);

            var commonDataUtils = new CommonDataUtils();
            var mockZipFileWrapper = new Mock<IIonicZipFileWrapper>();
            var mockZipEntry = new Mock<IZipEntry>();
            mockZipEntry.Setup(o => o.Extract(somePath, FileOverwrite.No)).Throws(new Ionic.Zip.BadPasswordException("some exception"));

            mockZipFileWrapper.Setup(o => o.GetEnumerator()).Returns(new List<IZipEntry>() {
                mockZipEntry.Object
            }.GetEnumerator());

            var hadException = false;
            try
            {
                commonDataUtils.ExtractFile(mockUnzipOpTo.Object, mockZipFileWrapper.Object, somePath);
            } catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.InvalidArchivePassword, e.Message);
            }
            Assert.IsTrue(hadException);

            mockUnzipOpTo.VerifyGet(o => o.ArchivePassword, Times.Exactly(2));
            mockZipFileWrapper.VerifySet(o => o.Password = somePassword, Times.Once);
            mockZipFileWrapper.Verify(o => o.Dispose(), Times.Once);
            mockZipEntry.Verify(o => o.Extract(somePath, FileOverwrite.No), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_TempFile()
        {
            var filename = CommonDataUtils.TempFile("ext");
            Assert.IsTrue(filename.EndsWith(".ext"));
            Assert.IsFalse(File.Exists(filename));
            Assert.IsTrue(filename.Length > 36);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ExtractZipCompressionLevel()
        {
            //---------------Set up test pack-------------------
            var commonDataUtils = new CommonDataUtils();
            var level = commonDataUtils.ExtractZipCompressionLevel("BestCompression");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(level);
            Assert.AreEqual(Ionic.Zlib.CompressionLevel.BestCompression, level);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ExtractZipCompressionLevel_UnknownLevel_ShouldUseDefault()
        {
            //---------------Set up test pack-------------------
            var commonDataUtils = new CommonDataUtils();
            var level = commonDataUtils.ExtractZipCompressionLevel("Test");
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(level);
            Assert.AreEqual(Ionic.Zlib.CompressionLevel.Default, level);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CommonDataUtils_IsNotFtpTypePath()
        {
            bool IsNotFtpTypePathTest(string fileName)
            {
                var pathMock = new Mock<IActivityIOPath>();
                pathMock.Setup(path => path.Path).Returns(fileName);

                var value = CommonDataUtils.IsNotFtpTypePath(pathMock.Object);
                return bool.Parse(value.ToString());
            }

            Assert.IsTrue(IsNotFtpTypePathTest("C:\\Home\\a.txt"));

            Assert.IsFalse(IsNotFtpTypePathTest("ftp://Home//a.txt"));
            Assert.IsFalse(IsNotFtpTypePathTest("ftps://Home//a.txt"));
            Assert.IsFalse(IsNotFtpTypePathTest("sftp://Home//a.txt"));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ValidateSourceAndDestinationPaths_EmptySourceThrows()
        {
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.Setup(path => path.Path).Returns("");
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.Setup(path => path.Path).Returns("some relative path");
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");

            var commonDataUtils = new CommonDataUtils();
            var hadException = false;
            try
            {
                commonDataUtils.ValidateSourceAndDestinationPaths(src.Object, dst.Object);
            }
            catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.SourceCannotBeAnEmptyString, e.Message);
            }
            Assert.IsTrue(hadException, "expected exception");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ValidateSourceAndDestinationPaths_EmptyDestinationBecomesSource()
        {
            const string srcPathString = "C:\\Home\\a.txt";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.SetupGet(o => o.Path).Returns(srcPathString);
            var pathSet = false;

            var dstPath = new Mock<IActivityIOPath>();
            dstPath.SetupGet(path => path.Path).Returns(() => pathSet ? srcPath.Object.Path : "");
            dstPath.SetupSet(path => path.Path = srcPathString).Callback<string>((s) => pathSet = true);

            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");

            var commonDataUtils = new CommonDataUtils();
            var hadException = false;
            try
            {
                commonDataUtils.ValidateSourceAndDestinationPaths(src.Object, dst.Object);
            }
            catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.DestinationDirectoryCannotBeAChild, e.Message);
            }
            Assert.IsTrue(hadException);
            Assert.IsTrue(pathSet);
            dstPath.VerifySet(o => o.Path = srcPathString, Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ValidateSourceAndDestinationPaths_DestinationIsSubdir_Fails()
        {
            const string srcPathString = @"C:\Home\Subdir";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.SetupGet(o => o.Path).Returns(srcPathString);
            var pathSet = false;

            const string dstPathString = @"C:\Home\Subdir\MoreSubDir\MoreSubDir\a.txt";
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.SetupGet(path => path.Path).Returns(() => pathSet ? srcPath.Object.Path : dstPathString);
            dstPath.SetupSet(path => path.Path = srcPathString).Callback<string>((s) => pathSet = true);

            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");

            var commonDataUtils = new CommonDataUtils();
            var hadException = false;
            try
            {
                commonDataUtils.ValidateSourceAndDestinationPaths(src.Object, dst.Object);
            }
            catch (Exception e)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.DestinationDirectoryCannotBeAChild, e.Message);
            }
            Assert.IsTrue(hadException);

            Assert.IsFalse(pathSet);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ValidateSourceAndDestinationPaths_UncDestination()
        {
            const string srcPathString = "C:\\Home\\a.txt";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.SetupGet(o => o.Path).Returns(srcPathString);
            var pathSet = false;

            const string dstPathString = @"\\Home\Subdir\MoreSubDir\a.txt";
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.SetupGet(path => path.Path).Returns(() => pathSet ? srcPath.Object.Path : dstPathString);
            dstPath.SetupSet(path => path.Path = srcPathString).Callback<string>((s) => pathSet = true);

            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");

            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.ValidateSourceAndDestinationPaths(src.Object, dst.Object);

            Assert.IsFalse(pathSet);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_ValidateSourceAndDestinationPaths_GivenIsPathRooted_ShouldReturnFalse()
        {
            const string txt = "C:\\Home\\a.txt";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.Setup(path => path.Path).Returns(txt);
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.Setup(path => path.Path).Returns("some relative path");
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");

            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.ValidateSourceAndDestinationPaths(src.Object, dst.Object);

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_IsUncFileTypePath()
        {
            Assert.IsTrue(CommonDataUtils.IsUncFileTypePath(@"\\Home\a.txt"));
            Assert.IsFalse(CommonDataUtils.IsUncFileTypePath(@"\Home\a.txt"));
            Assert.IsFalse(CommonDataUtils.IsUncFileTypePath(@"C:\Home\a.txt"));
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        [ExpectedException(typeof(Exception))]
        public void CommonDataUtils_AddMissingFileDirectoryParts_GivenNullSource_ShouldReturnError()
        {
            var commonDataUtils = new CommonDataUtils();
            var tempFile = Path.GetTempFileName();
            const string newFileName = "ZippedTempFile";
            var zipPathName = Path.GetTempPath() + newFileName + ".zip";
            var scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempFile, string.Empty, null, true, ""));
            var dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(zipPathName, string.Empty, null, true, ""));
            Assert.IsNotNull(commonDataUtils);
            scrEndPoint.IOPath.Path = string.Empty;
            commonDataUtils.AddMissingFileDirectoryParts(scrEndPoint, dstEndPoint);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_AddMissingFileDirectoryParts_GivenNullDestination_ShouldReturnError()
        {
            var commonDataUtils = new CommonDataUtils();
            var tempFile = Path.GetTempFileName();
            const string newFileName = "ZippedTempFile";
            var zipPathName = Path.GetTempPath() + newFileName + ".zip";
            var scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(tempFile, string.Empty, null, true, ""));
            var dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(ActivityIOFactory.CreatePathFromString(zipPathName, string.Empty, null, true, ""));
            Assert.IsNotNull(commonDataUtils);
            dstEndPoint.IOPath.Path = string.Empty;
            commonDataUtils.AddMissingFileDirectoryParts(scrEndPoint, dstEndPoint);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_AddMissingFileDirectoryParts_GivenDestinationPathIsDirectory_SourcePathIsNotDirectory()
        {
            const string file = "C:\\Home\\a.txt";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.Setup(path => path.Path).Returns(file);
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.Setup(path => path.Path).Returns(file);
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");
            dst.Setup(p => p.PathIs(dstPath.Object)).Returns(enPathType.Directory);

            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.AddMissingFileDirectoryParts(src.Object, dst.Object);
            dstPath.VerifySet(p => p.Path = @"C:\Home\");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_AddMissingFileDirectoryParts_GivenDestinationPathIsDirectory_SourcePathIsFile()
        {
            const string file = "C:\\Parent\\a.txt";
            const string dstfile = "C:\\Parent\\Child\\";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.Setup(path => path.Path).Returns(file);
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.Setup(path => path.Path).Returns(dstfile);
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");
            src.Setup(p => p.PathIs(srcPath.Object)).Returns(enPathType.Directory);

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");
            dst.Setup(p => p.PathIs(dstPath.Object)).Returns(enPathType.Directory);

            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.AddMissingFileDirectoryParts(src.Object, dst.Object);
            dstPath.VerifySet(p => p.Path = @"C:\Parent\Child\a.txt");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_AddMissingFileDirectoryParts_GivenDestinationPathIsDirectoryOfSource_SourcePathIsDirectory()
        {
            const string file = @"C:\Parent\";
            const string dstfile = @"C:\Parent\Child1\Child2\";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.Setup(path => path.Path).Returns(file);
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.Setup(path => path.Path).Returns(dstfile);
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");
            src.Setup(p => p.PathIs(srcPath.Object)).Returns(enPathType.Directory);

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");
            dst.Setup(p => p.PathIs(dstPath.Object)).Returns(enPathType.Directory);

            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.AddMissingFileDirectoryParts(src.Object, dst.Object);
            dstPath.VerifySet(p => p.Path = @"C:\Parent\Child1\Child2\Parent");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_AddMissingFileDirectoryParts_GivenDestinationPathIsDirectory_SourcePathIsDirectory()
        {
            const string file = @"C:\Parent1\";
            const string dstfile = @"C:\Parent2\";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.Setup(path => path.Path).Returns(file);
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.Setup(path => path.Path).Returns(dstfile);
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");
            src.Setup(p => p.PathIs(srcPath.Object)).Returns(enPathType.Directory);

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");
            dst.Setup(p => p.PathIs(dstPath.Object)).Returns(enPathType.Directory);

            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.AddMissingFileDirectoryParts(src.Object, dst.Object);
            dstPath.VerifySet(p => p.Path = @"C:\Parent2\Parent1");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_AddMissingFileDirectoryParts_GivenDestinationPathIsSubDirectory_SourcePathIsFile()
        {
            const string file = @"C:\Parent\file1.txt";
            const string dstfile = @"C:\Parent\Child1\Child2\";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.Setup(path => path.Path).Returns(file);
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.Setup(path => path.Path).Returns(dstfile);
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");
            src.Setup(p => p.PathIs(srcPath.Object)).Returns(enPathType.Directory);

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");
            dst.Setup(p => p.PathIs(dstPath.Object)).Returns(enPathType.Directory);

            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.AddMissingFileDirectoryParts(src.Object, dst.Object);
            dstPath.VerifySet(p => p.Path = @"C:\Parent\Child1\Child2\file1.txt");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_AddMissingFileDirectoryParts_GivenDestinationPathIsDirectory_SourcePathIsFile1()
        {
            const string file = @"C:\Parent1\file1.txt";
            const string dstfile = @"C:\Parent2\Child1\Child2\";
            var srcPath = new Mock<IActivityIOPath>();
            srcPath.Setup(path => path.Path).Returns(file);
            var dstPath = new Mock<IActivityIOPath>();
            dstPath.Setup(path => path.Path).Returns(dstfile);
            var dev2CrudOperationTO = new Dev2CRUDOperationTO(true);

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            src.Setup(p => p.IOPath).Returns(srcPath.Object);
            src.Setup(p => p.PathSeperator()).Returns("\\");
            src.Setup(p => p.PathIs(srcPath.Object)).Returns(enPathType.Directory);

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.Setup(p => p.CreateDirectory(It.IsAny<IActivityIOPath>(), dev2CrudOperationTO)).Returns(true);
            dst.Setup(p => p.IOPath).Returns(dstPath.Object);
            dst.Setup(p => p.PathSeperator()).Returns("\\");
            dst.Setup(p => p.PathIs(dstPath.Object)).Returns(enPathType.Directory);

            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.AddMissingFileDirectoryParts(src.Object, dst.Object);
            dstPath.VerifySet(p => p.Path = @"C:\Parent2\Child1\Child2\file1.txt");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_CreateTmpDirectory_ShouldCreateFolderInTheCorrectLocation()
        {
            string fullName = "";
            var mockDirectoryInfo = new Mock<IDirectoryInfo>();
            mockDirectoryInfo.Setup(o => o.FullName).Returns(() => fullName);
            var mockDirectoryWrapper = new Mock<IDirectory>();
            mockDirectoryWrapper.Setup(o => o.CreateDirectory(It.IsAny<string>())).Returns<string>(s => { fullName = s; return mockDirectoryInfo.Object; });
            var commonDataUtils = new CommonDataUtils(mockDirectoryWrapper.Object);
            var path = commonDataUtils.CreateTmpDirectory();
            //---------------Test Result -----------------------
            Assert.AreEqual(fullName, path);
            StringAssert.Contains(path, GlobalConstants.TempLocation);
            mockDirectoryWrapper.Verify(o => o.CreateDirectory(fullName), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_CreateTmpDirectory_ShouldPropagateExceptions()
        {
            var expectedException = new Exception("some exception");
            var mockDirectoryInfo = new Mock<IDirectoryInfo>();
            var mockDirectoryWrapper = new Mock<IDirectory>();
            mockDirectoryWrapper.Setup(o => o.CreateDirectory(It.IsAny<string>())).Throws(expectedException);
            var commonDataUtils = new CommonDataUtils(mockDirectoryWrapper.Object);

            Exception actualException = null;
            try
            {
                var path = commonDataUtils.CreateTmpDirectory();
            } catch (Exception e)
            {
                actualException = e;
            }
            Assert.AreEqual(expectedException, actualException);
            //---------------Test Result -----------------------
            mockDirectoryWrapper.Verify(o => o.CreateDirectory(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_CreateObjectInputs_GivenRecSet_ShouldCreateObjectInput_OnTheInnerEnvToo()
        {
            var commonDataUtils = new CommonDataUtils();
            Assert.IsNotNull(commonDataUtils);

            var outerExeEnv = new ExecutionEnvironment();
            var recUsername = "[[rec(1).UserName]]";
            outerExeEnv.Assign(recUsername, "Sanele", 0);
            var definitions = new List<IDev2Definition>
            {
                new Dev2Definition("Name", "UserName", "Sanele", false, "NoName", false, recUsername)
            };
            Assert.IsNotNull(definitions);
            var innerExecEnv = new ExecutionEnvironment();
            var prObj = new Warewolf.Testing.PrivateObject(innerExecEnv);
            var warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.Scalar.Count == 0);
            commonDataUtils.CreateObjectInputs(outerExeEnv, definitions, innerExecEnv, 0);
            warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.Scalar.Count == 1);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_CreateObjectInputs_GivenVariable_ShouldCreateObjectInput_OnTheInnerEnvToo()
        {
            var commonDataUtils = new CommonDataUtils();
            Assert.IsNotNull(commonDataUtils);

            var outerExeEnv = new ExecutionEnvironment();
            var recUsername = "[[UserName]]";
            outerExeEnv.Assign(recUsername, "Sanele", 0);
            var definitions = new List<IDev2Definition>
            {
                new Dev2Definition("Name", "UserName", "Sanele", false, "NoName", false, recUsername)
            };
            Assert.IsNotNull(definitions);
            var innerExecEnv = new ExecutionEnvironment();
            var prObj = new Warewolf.Testing.PrivateObject(innerExecEnv);
            var warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.Scalar.Count == 0);
            commonDataUtils.CreateObjectInputs(outerExeEnv, definitions, innerExecEnv, 0);
            warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.Scalar.Count == 1);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_CreateObjectInputs_GivenJSon_ShouldCreateObjectInput_OnTheInnerEnvToo()
        {
            var commonDataUtils = new CommonDataUtils();
            Assert.IsNotNull(commonDataUtils);

            var outerExeEnv = new ExecutionEnvironment();
            var recUsername = "[[@Person().UserName]]";
            outerExeEnv.Assign(recUsername, "Sanele", 0);
            var definitions = new List<IDev2Definition>
            {
                new Dev2Definition("Name", "UserName", "Sanele", false, "NoName", false, recUsername)
            };
            Assert.IsNotNull(definitions);
            var innerExecEnv = new ExecutionEnvironment();
            var prObj = new Warewolf.Testing.PrivateObject(innerExecEnv);
            var warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.JsonObjects.Count == 0);
            commonDataUtils.CreateObjectInputs(outerExeEnv, definitions, innerExecEnv, 0);
            warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.IsTrue(warewolfEnvironment != null && warewolfEnvironment.JsonObjects.Count == 1);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_CreateScalarInputs_ShouldMapToOuterEnvironment()
        {
            var commonDataUtils = new CommonDataUtils();
            Assert.IsNotNull(commonDataUtils);

            var outerExeEnv = new ExecutionEnvironment();
            outerExeEnv.Assign("[[rec(1).UserName]]", "username1 v1", 0);
            outerExeEnv.Assign("[[rec(2).UserName]]", "username2 v2", 0);
            var inputScalarList = new List<IDev2Definition>
            {
                new Dev2Definition("UserName", "UserName", "UserName", false, "NoName", false, ""),
                new Dev2Definition("Input2", "Input2", "Input2", false, "NoInput2", false, "inpv2"),
                new Dev2Definition("Input3", "Input3", "Input3", false, "1234", false, "4321"),
                new Dev2Definition("UserName2", "UserName2", "UserName2", false, "NoNames", false, "[[rec(*).UserName]]"),
            };
            Assert.IsNotNull(inputScalarList);
            var innerExecEnv = new ExecutionEnvironment();
            Assert.IsNotNull(innerExecEnv);
            var prObj = new Warewolf.Testing.PrivateObject(innerExecEnv);
            var warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.AreEqual(0, warewolfEnvironment.Scalar.Count);
            commonDataUtils.CreateScalarInputs(outerExeEnv, inputScalarList, innerExecEnv, 0);
            warewolfEnvironment = prObj.GetField("_env") as DataStorage.WarewolfEnvironment;
            Assert.AreEqual(4, warewolfEnvironment.Scalar.Count);

            Assert.IsTrue(warewolfEnvironment.Scalar["UserName"].IsNothing);

            var v2 = warewolfEnvironment.Scalar["Input2"] as DataStorage.WarewolfAtom.DataString;
            Assert.AreEqual("inpv2", v2.Item);

            var v3 = warewolfEnvironment.Scalar["Input3"] as DataStorage.WarewolfAtom.Int;
            Assert.AreEqual(4321, v3.Item);

            var v4 = warewolfEnvironment.Scalar["UserName2"] as DataStorage.WarewolfAtom.DataString;
            Assert.AreEqual("username2 v2", v4.Item);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_CreateRecordSetsInputs()
        {
            var mockOuterEnvironment = new Mock<IExecutionEnvironment>();
            var outerEnv = new ExecutionEnvironment();
            outerEnv.Assign("[[rec().Name]]", "someval1", 0);
            outerEnv.Assign("[[rec().Name]]", "someval2", 0);
            outerEnv.Assign("[[rec().Age]]", "25", 0);
            outerEnv.Assign("[[rec().Age]]", "26", 0);

            mockOuterEnvironment.Setup(o => o.AssignDataShape("[[rec().Name]]")).Callback<string>(s => outerEnv.AssignDataShape(s));
            mockOuterEnvironment.Setup(o => o.AssignDataShape("[[rec().Age]]")).Callback<string>(s => outerEnv.AssignDataShape(s));
            mockOuterEnvironment.Setup(o => o.Eval(It.IsAny<string>(), 0)).Returns<string, int>((s, i) => outerEnv.Eval(s, i));

            var inputRecSets = new Mock<IRecordSetCollection>();
            var mockedInput = new Mock<IRecordSetDefinition>();
            var nameColumn = new Mock<IDev2Definition>();
            var ageColumn = new Mock<IDev2Definition>();


            mockedInput.Setup(p => p.SetName).Returns("rec");

            nameColumn.Setup(p => p.Name).Returns("Name");
            nameColumn.Setup(p => p.IsRecordSet).Returns(true);
            nameColumn.Setup(p => p.RecordSetName).Returns("rec");
            nameColumn.Setup(p => p.RawValue).Returns("[[rec().Name]]");
            nameColumn.Setup(p => p.Value).Returns("[[rec().Name]]");
            ageColumn.Setup(p => p.Name).Returns("Age");
            ageColumn.Setup(p => p.IsRecordSet).Returns(true);
            ageColumn.Setup(p => p.RecordSetName).Returns("rec");
            ageColumn.Setup(p => p.RawValue).Returns("v2");

            mockedInput.Setup(p => p.Columns).Returns(new List<IDev2Definition> { nameColumn.Object, ageColumn.Object });
            inputRecSets.Setup(p => p.RecordSets).Returns(new List<IRecordSetDefinition> { mockedInput.Object });


            var input1 = new Dev2Definition("Name", "rec().Name", "Name", "rec", false, "NoName", false, "[[rec(*).Name]]");
            var input2 = new Dev2Definition("Age", "rec().Age", "Age", "rec", false, "0", false, "[[rec(*).Age]]");


            var inputs = new List<IDev2Definition>
             {
                input1, input2
            };

            var env = new ExecutionEnvironment();
            env.Assign("[[rec().Name]]", "someval", 0);
            env.Assign("[[rec().Age]]", "25", 0);
            var mockEnv = new Mock<IExecutionEnvironment>();
            mockEnv.Setup(o => o.AssignDataShape("[[rec().Name]]")).Callback<string>(s => env.AssignDataShape(s));
            mockEnv.Setup(o => o.AssignDataShape("[[rec().Age]]")).Callback<string>(s => env.AssignDataShape(s));
            mockEnv.Setup(o => o.Eval(It.IsAny<string>(), 0)).Returns<string, int>((s, i) => env.Eval(s, i));

            int update = 0;
            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.CreateRecordSetsInputs(mockOuterEnvironment.Object, inputRecSets.Object, inputs, mockEnv.Object, update);
            mockEnv.Verify(o => o.AssignWithFrame(It.IsAny<WarewolfParserInterop.AssignValue>(), 0), Times.Once);
            mockEnv.Verify(o => o.EvalAssignFromNestedStar("[[rec(*).Name]]", It.IsAny<CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult>(), 0), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_CreateRecordSetsInputs_GivenNullValue()
        {
            var outerEnvironment = new Mock<IExecutionEnvironment>();
            var inputRecSets = new Mock<IRecordSetCollection>();
            var mockedInput = new Mock<IRecordSetDefinition>();
            var nameColumn = new Mock<IDev2Definition>();
            var ageColumn = new Mock<IDev2Definition>();
            var input1 = new Mock<IDev2Definition>();
            var input2 = new Mock<IDev2Definition>();

            mockedInput.Setup(p => p.SetName).Returns("Person");

            nameColumn.Setup(p => p.Name).Returns("Name");
            nameColumn.Setup(p => p.IsRecordSet).Returns(true);
            nameColumn.Setup(p => p.RecordSetName).Returns("Person");
            ageColumn.Setup(p => p.Name).Returns("Age");
            ageColumn.Setup(p => p.IsRecordSet).Returns(true);
            ageColumn.Setup(p => p.RecordSetName).Returns("Person");

            mockedInput.Setup(p => p.Columns).Returns(new List<IDev2Definition> { nameColumn.Object, ageColumn.Object });
            inputRecSets.Setup(p => p.RecordSets).Returns(new List<IRecordSetDefinition> { mockedInput.Object });


            input1.Setup(p => p.IsRecordSet).Returns(true);
            input1.Setup(p => p.MapsTo).Returns("[[Person().Name]]");
            input1.Setup(p => p.RecordSetName).Returns("Person");
            input2.Setup(p => p.IsRecordSet).Returns(true);
            input2.Setup(p => p.MapsTo).Returns("[[Person().Age]]");
            input2.Setup(p => p.RecordSetName).Returns("Person");

            var inputs = new List<IDev2Definition>
             {
                input1.Object, input2.Object
            };

            var env = new Mock<IExecutionEnvironment>();
            int update = 0;
            var commonDataUtils = new CommonDataUtils();
            commonDataUtils.CreateRecordSetsInputs(outerEnvironment.Object, inputRecSets.Object, inputs, env.Object, update);
            env.Verify(p => p.AssignDataShape("[[Person().Name]]"), Times.AtLeastOnce);
            env.Verify(p => p.AssignDataShape("[[Person().Age]]"), Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_GenerateDefsFromDataListForDebug_GivenNullValue()
        {
            const string trueString = "True";
            var datalist = string.Format("<DataList><Person Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"Both\" ><Name Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"Input\" /></Person></DataList>", trueString);
            var commonDataUtils = new CommonDataUtils();
            var results = commonDataUtils.GenerateDefsFromDataListForDebug(datalist, enDev2ColumnArgumentDirection.Both);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("Name", results[0].Name);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_GenerateDefsFromDataList_GivenDataList_ShouldReturnList()
        {
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            try
            {
                var generateDefsFromDataList = DataListUtil.GenerateDefsFromDataList(datalist, enDev2ColumnArgumentDirection.Input);
                Assert.IsNotNull(generateDefsFromDataList);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);

            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_GenerateDefsFromDataList_GivenDataListAndSearchFilter_ShouldReturnList()
        {
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            var mockSearchParameters = new Mock<ISearch>();
            mockSearchParameters.Setup(o => o.SearchInput).Returns("");
            var mockSearchOptions = new Mock<ISearchOptions>();
            mockSearchParameters.Setup(o => o.SearchOptions).Returns(mockSearchOptions.Object);

            var generateDefsFromDataList = DataListUtil.GenerateDefsFromDataList(datalist, enDev2ColumnArgumentDirection.Both, true, mockSearchParameters.Object);
            Assert.IsNotNull(generateDefsFromDataList);

            mockSearchParameters.Verify(o => o.SearchInput, Times.AtLeastOnce);
            mockSearchParameters.Verify(o => o.SearchOptions, Times.AtLeastOnce);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_GenerateDefsFromDataList_GivenDataList_ShouldReurnListWithEntries()
        {
            const string trueString = "True";
            const string noneString = "Input";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" IsJson=\"True\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            var generateDefsFromDataList = DataListUtil.GenerateDefsFromDataList(datalist, enDev2ColumnArgumentDirection.Input);
            Assert.AreEqual(3, generateDefsFromDataList.Count);

            var expected = new Dev2Definition("@var", "", "", false, "", false, "");
            Assert.AreEqual(expected.Name, generateDefsFromDataList[0].Name);
            Assert.AreEqual(expected.MapsTo, generateDefsFromDataList[0].MapsTo);
            Assert.AreEqual(expected.Value, generateDefsFromDataList[0].Value);
            Assert.AreEqual(expected.IsEvaluated, generateDefsFromDataList[0].IsEvaluated);
            Assert.AreEqual(expected.DefaultValue, generateDefsFromDataList[0].DefaultValue);
            Assert.AreEqual(expected.IsRequired, generateDefsFromDataList[0].IsRequired);
            Assert.AreEqual(expected.RawValue, generateDefsFromDataList[0].RawValue);
            Assert.IsTrue(generateDefsFromDataList[0].IsObject);

            expected.Name = "a";
            Assert.AreEqual(expected.Name, generateDefsFromDataList[1].Name);
            Assert.AreEqual(expected.MapsTo, generateDefsFromDataList[1].MapsTo);
            Assert.AreEqual(expected.Value, generateDefsFromDataList[1].Value);
            Assert.AreEqual(expected.IsEvaluated, generateDefsFromDataList[1].IsEvaluated);
            Assert.AreEqual(expected.DefaultValue, generateDefsFromDataList[1].DefaultValue);
            Assert.AreEqual(expected.IsRequired, generateDefsFromDataList[1].IsRequired);
            Assert.AreEqual(expected.RawValue, generateDefsFromDataList[1].RawValue);

            expected.Name = "set";
            Assert.AreEqual(expected.Name, generateDefsFromDataList[2].Name);
            Assert.AreEqual(expected.MapsTo, generateDefsFromDataList[2].MapsTo);
            Assert.AreEqual(expected.Value, generateDefsFromDataList[2].Value);
            Assert.AreEqual(expected.IsEvaluated, generateDefsFromDataList[2].IsEvaluated);
            Assert.AreEqual(expected.DefaultValue, generateDefsFromDataList[2].DefaultValue);
            Assert.AreEqual(expected.IsRequired, generateDefsFromDataList[2].IsRequired);
            Assert.AreEqual(expected.RawValue, generateDefsFromDataList[2].RawValue);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_GenerateDefsFromDataListForDebug_GivenEmpty_ShouldReturnEmptyList()
        {
            var generateDefsFromDataListForDebug = DataListUtil.GenerateDefsFromDataListForDebug("", enDev2ColumnArgumentDirection.Output);

            Assert.AreEqual(0, generateDefsFromDataListForDebug.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_GenerateDefsFromDataListForDebug_GivenEmptyDataList_ShouldReturnEmptyList()
        {
            var generateDefsFromDataListForDebug = DataListUtil.GenerateDefsFromDataListForDebug("<Datalist></Datalist>", enDev2ColumnArgumentDirection.Output);

            Assert.AreEqual(0, generateDefsFromDataListForDebug.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_GenerateDefsFromDataListForDebug_GivenLoadedDataList_ShouldReturnDebugList()
        {
            const string datalist = @"<DataList><Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" ><Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /><Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /></Car><Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" /><Person Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Age Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Age><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><School Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Location Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Location></School></Person></DataList>";
            var generateDefsFromDataListForDebug = DataListUtil.GenerateDefsFromDataListForDebug(datalist, enDev2ColumnArgumentDirection.Output);

            Assert.AreEqual(2, generateDefsFromDataListForDebug.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(CommonDataUtils))]
        public void CommonDataUtils_GenerateDefsFromDataListForDebug_GivenLoadedDataList_ShouldReturnDebugListWithObject()
        {
            const string datalist = @"<DataList><Car Description=""A recordset of information about a car"" IsEditable=""True"" ColumnIODirection=""Both"" ><Make Description=""Make of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /><Model Description=""Model of vehicle"" IsEditable=""True"" ColumnIODirection=""None"" /></Car><Country Description=""name of Country"" IsEditable=""True"" ColumnIODirection=""Both"" IsJson=""True"" /><Person Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Age Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Age><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><School Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ><Name Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Name><Location Description="""" IsEditable=""True"" IsJson=""True"" IsArray=""False"" ColumnIODirection=""None"" ></Location></School></Person></DataList>";
            var generateDefsFromDataListForDebug = DataListUtil.GenerateDefsFromDataListForDebug(datalist, enDev2ColumnArgumentDirection.Output);

            Assert.AreEqual(2, generateDefsFromDataListForDebug.Count);
            Assert.IsTrue(generateDefsFromDataListForDebug[0].IsRecordSet);
            Assert.AreEqual("Car", generateDefsFromDataListForDebug[0].RecordSetName);
            Assert.AreEqual("", generateDefsFromDataListForDebug[0].MapsTo);
            Assert.AreEqual("", generateDefsFromDataListForDebug[0].Value);
            Assert.IsFalse(generateDefsFromDataListForDebug[0].IsEvaluated);
            Assert.AreEqual("", generateDefsFromDataListForDebug[0].DefaultValue);
            Assert.IsFalse(generateDefsFromDataListForDebug[0].IsRequired);
            Assert.AreEqual("", generateDefsFromDataListForDebug[0].RawValue);

            Assert.IsFalse(generateDefsFromDataListForDebug[1].IsObject);
            Assert.AreEqual("@Country", generateDefsFromDataListForDebug[1].Name);
            Assert.AreEqual("", generateDefsFromDataListForDebug[1].MapsTo);
            Assert.AreEqual("", generateDefsFromDataListForDebug[1].Value);
            Assert.IsFalse(generateDefsFromDataListForDebug[1].IsEvaluated);
            Assert.AreEqual("", generateDefsFromDataListForDebug[1].DefaultValue);
            Assert.IsFalse(generateDefsFromDataListForDebug[1].IsRequired);
            Assert.AreEqual("", generateDefsFromDataListForDebug[1].RawValue);
        }

    }
}
