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
    public class ActivityIOBrokerValidatorDriverTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateUnzipSourceDestinationFileOperation_InvalidSource()
        {
            //---------------Set up test pack-------------------
            var driver = new ActivityIOBrokerValidatorDriver();

            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.IOPath.Path).Returns("");
            src.Setup(point => point.PathSeperator()).Returns(",");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.SetupProperty(point => point.IOPath.Path);
            dst.Setup(point => point.PathSeperator()).Returns(",");
            var args = new Dev2UnZipOperationTO("password", false);
            Func<string> performAfterValidation = () => "Success";

            var hadException = false;
            try
            {
                driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
            }
            catch (Exception ex)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.SourceCannotBeAnEmptyString, ex.Message);
            }
            Assert.IsTrue(hadException);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateUnzipSourceDestinationFileOperation_DestinationNotADirectory()
        {
            //---------------Set up test pack-------------------
            var driver = new ActivityIOBrokerValidatorDriver();

            var srcPath = Path.GetTempPath();
            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.IOPath.Path).Returns("");
            src.Setup(point => point.PathSeperator()).Returns(",");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.SetupProperty(point => point.IOPath.Path);
            dst.Setup(point => point.PathSeperator()).Returns(",");
            var args = new Dev2UnZipOperationTO("password", false);
            Func<string> performAfterValidation = () => "Success";

            src.Setup(point => point.IOPath.Path).Returns(srcPath);
            dst.Setup(point => point.IOPath.Path).Returns("");

            var hadException = false;
            try
            {
                driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
            }
            catch (Exception ex1)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.DestinationMustBeADirectory, ex1.Message);
            }
            Assert.IsTrue(hadException);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateUnzipSourceDestinationFileOperation_SourceNotAFile()
        {
            //---------------Set up test pack-------------------
            var driver = new ActivityIOBrokerValidatorDriver();

            var srcPath = Path.GetTempPath();
            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.IOPath.Path).Returns("");
            src.Setup(point => point.PathSeperator()).Returns(",");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.SetupProperty(point => point.IOPath.Path);
            dst.Setup(point => point.PathSeperator()).Returns(",");
            var args = new Dev2UnZipOperationTO("password", false);
            Func<string> performAfterValidation = () => "Success";

            src.Setup(point => point.IOPath.Path).Returns(srcPath);
            dst.Setup(point => point.IOPath.Path).Returns("");

            dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
            src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

            var hadException = false;
            try
            {
                driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
            }
            catch (Exception ex2)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.SourceMustBeAFile, ex2.Message);
            }
            Assert.IsTrue(hadException);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateUnzipSourceDestinationFileOperation_DestinationDirectoryExists()
        {
            //---------------Set up test pack-------------------
            var driver = new ActivityIOBrokerValidatorDriver();

            var srcPath = Path.GetTempPath();
            var src = new Mock<IActivityIOOperationsEndPoint>();
            src.Setup(point => point.IOPath.Path).Returns("");
            src.Setup(point => point.PathSeperator()).Returns(",");

            var dst = new Mock<IActivityIOOperationsEndPoint>();
            dst.SetupProperty(point => point.IOPath.Path);
            dst.Setup(point => point.PathSeperator()).Returns(",");
            var args = new Dev2UnZipOperationTO("password", false);
            Func<string> performAfterValidation = () => "Success";

            src.Setup(point => point.IOPath.Path).Returns(srcPath);
            dst.Setup(point => point.IOPath.Path).Returns("");

            dst.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
            src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

            src.Setup(point => point.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            dst.Setup(point => point.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);

            var hadException = false;
            try
            {
                driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
            }
            catch (Exception ex3)
            {
                hadException = true;
                Assert.AreEqual(ErrorResource.DestinationDirectoryExist, ex3.Message);
                args = new Dev2UnZipOperationTO("pa", true);

                var invoke = driver.ValidateUnzipSourceDestinationFileOperation(src.Object, dst.Object, args, performAfterValidation);
                Assert.AreEqual(performAfterValidation.Invoke(), invoke.ToString());
            }
            Assert.IsTrue(hadException);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateCopySourceDestinationFileOperation_PathIs_NotDirectory_ExpectSuccess()
        {
            //---------------------------Arrange-----------------------------
            var mockActivityIOOperationsEndPointSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockActivityIOOperationsEndPointDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockActivityIOPathSrc = new Mock<IActivityIOPath>();
            var mockActivityIOPathDst = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();

            var srcPath = Path.GetTempPath();
            const string dstPath = "C:\\Test_TempPath\\";

            mockActivityIOOperationsEndPointSrc.Setup(o => o.IOPath).Returns(mockActivityIOPathSrc.Object);
            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathSeperator()).Returns(@"\");
            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);
            mockActivityIOOperationsEndPointDst.Setup(o => o.CreateDirectory(It.IsAny<IActivityIOPath>(), It.IsAny<IDev2CRUDOperationTO>())).Returns(true);

            mockActivityIOPathSrc.Setup(o => o.Path).Returns(srcPath);
            mockActivityIOPathDst.Setup(o => o.Path).Returns(dstPath);

            mockCommon.Setup(o => o.ValidateSourceAndDestinationPaths(It.IsAny<IActivityIOOperationsEndPoint>(), It.IsAny<IActivityIOOperationsEndPoint>()));

            var driver = new ActivityIOBrokerValidatorDriver(mockFile.Object, mockCommon.Object);
            //---------------------------Act---------------------------------
            var fileOperation = driver.ValidateCopySourceDestinationFileOperation(mockActivityIOOperationsEndPointSrc.Object, mockActivityIOOperationsEndPointDst.Object, mockDev2CRUDOperationTO.Object, () => "test func");
            //---------------------------Assert------------------------------
            Assert.AreEqual("test func", fileOperation);
            mockActivityIOPathSrc.VerifyAll();
            mockActivityIOPathDst.VerifyAll();
            mockActivityIOOperationsEndPointDst.VerifyAll();
            mockCommon.VerifyAll();
            mockActivityIOOperationsEndPointSrc.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateCopySourceDestinationFileOperation_PathIs_Directory_ExpectFail()
        {
            //---------------------------Arrange-----------------------------
            var mockActivityIOOperationsEndPointSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockActivityIOOperationsEndPointDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockActivityIOPathSrc = new Mock<IActivityIOPath>();
            var mockActivityIOPathDst = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();

            var srcPath = Path.GetTempPath();
            const string dstPath = "C:\\Test_TempPath\\";

            mockActivityIOOperationsEndPointSrc.Setup(o => o.IOPath).Returns(mockActivityIOPathSrc.Object);
            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathSeperator()).Returns(@"\");
            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);
            mockActivityIOOperationsEndPointDst.Setup(o => o.CreateDirectory(It.IsAny<IActivityIOPath>(), It.IsAny<IDev2CRUDOperationTO>())).Returns(true);

            mockActivityIOPathSrc.Setup(o => o.Path).Returns(srcPath);
            mockActivityIOPathDst.Setup(o => o.Path).Returns(dstPath);

            mockActivityIOOperationsEndPointSrc.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
            mockActivityIOOperationsEndPointSrc.Setup(o => o.ListFilesInDirectory(It.IsAny<IActivityIOPath>())).Returns(new List<IActivityIOPath>());
            mockActivityIOOperationsEndPointSrc.Setup(o => o.ListFoldersInDirectory(It.IsAny<IActivityIOPath>())).Returns(new List<IActivityIOPath>());

            mockActivityIOOperationsEndPointDst.Setup(o => o.ListFilesInDirectory(It.IsAny<IActivityIOPath>())).Returns(new List<IActivityIOPath>());

            mockCommon.Setup(o => o.ValidateSourceAndDestinationPaths(It.IsAny<IActivityIOOperationsEndPoint>(), It.IsAny<IActivityIOOperationsEndPoint>()));

            var driver = new ActivityIOBrokerValidatorDriver(mockFile.Object, mockCommon.Object);
            //---------------------------Act---------------------------------
            var fileOperation = driver.ValidateCopySourceDestinationFileOperation(mockActivityIOOperationsEndPointSrc.Object, mockActivityIOOperationsEndPointDst.Object, mockDev2CRUDOperationTO.Object, () => "test func");
            //---------------------------Assert------------------------------
            Assert.AreEqual("Success", fileOperation);
            mockActivityIOPathSrc.VerifyAll();
            mockActivityIOPathDst.VerifyAll();
            mockCommon.VerifyAll();
            mockActivityIOOperationsEndPointDst.VerifyAll();
            mockActivityIOOperationsEndPointSrc.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateCopySourceDestinationFileOperation_PathIs_File_IsStarWildCard_ExpectSuccess()
        {
            //---------------------------Arrange-----------------------------
            var mockActivityIOOperationsEndPointSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockActivityIOOperationsEndPointDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockActivityIOPathSrc = new Mock<IActivityIOPath>();
            var mockActivityIOPathDst = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();

            var srcPath = Path.GetTempPath() + "*";
            const string dstPath = "C:\\Test_TempPath\\";

            mockActivityIOOperationsEndPointSrc.Setup(o => o.IOPath).Returns(mockActivityIOPathSrc.Object);
            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathSeperator()).Returns(@"\");
            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);
            mockActivityIOOperationsEndPointDst.Setup(o => o.CreateDirectory(It.IsAny<IActivityIOPath>(), It.IsAny<IDev2CRUDOperationTO>())).Returns(true);

            mockActivityIOPathSrc.Setup(o => o.Path).Returns(srcPath);
            mockActivityIOPathDst.Setup(o => o.Path).Returns(dstPath);

            mockActivityIOOperationsEndPointSrc.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);
            mockActivityIOOperationsEndPointSrc.Setup(o => o.ListFilesInDirectory(It.IsAny<IActivityIOPath>())).Returns(new List<IActivityIOPath>());
            mockActivityIOOperationsEndPointSrc.Setup(o => o.ListFoldersInDirectory(It.IsAny<IActivityIOPath>())).Returns(new List<IActivityIOPath>());

            mockActivityIOOperationsEndPointDst.Setup(o => o.ListFilesInDirectory(It.IsAny<IActivityIOPath>())).Returns(new List<IActivityIOPath>());
            mockCommon.Setup(o => o.ValidateSourceAndDestinationPaths(It.IsAny<IActivityIOOperationsEndPoint>(), It.IsAny<IActivityIOOperationsEndPoint>()));

            var driver = new ActivityIOBrokerValidatorDriver(mockFile.Object, mockCommon.Object);
            //---------------------------Act---------------------------------
            var fileOperation = driver.ValidateCopySourceDestinationFileOperation(mockActivityIOOperationsEndPointSrc.Object, mockActivityIOOperationsEndPointDst.Object, mockDev2CRUDOperationTO.Object, () => "test func");
            //---------------------------Assert------------------------------
            Assert.AreEqual("Success", fileOperation);
            mockActivityIOPathSrc.VerifyAll();
            mockActivityIOPathDst.VerifyAll();
            mockCommon.VerifyAll();
            mockActivityIOOperationsEndPointDst.VerifyAll();
            mockActivityIOOperationsEndPointSrc.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateCopySourceDestinationFileOperation_EnsureFilesDontExists_Overwrite_False_PathTypeIsFile_ExpectException()
        {
            //---------------------------Arrange-----------------------------
            var mockActivityIOOperationsEndPointSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockActivityIOOperationsEndPointDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockActivityIOPathSrc = new Mock<IActivityIOPath>();
            var mockActivityIOPathDst = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();

            const string dstPath = "C:\\Test_TempPath\\";

            mockActivityIOOperationsEndPointSrc.Setup(o => o.IOPath).Returns(mockActivityIOPathSrc.Object);
            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathSeperator()).Returns(@"\");

            mockActivityIOPathDst.Setup(o => o.Path).Returns(dstPath);

            mockActivityIOOperationsEndPointSrc.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);

            mockCommon.Setup(o => o.ValidateSourceAndDestinationPaths(It.IsAny<IActivityIOOperationsEndPoint>(), It.IsAny<IActivityIOOperationsEndPoint>()));

            var driver = new ActivityIOBrokerValidatorDriver(mockFile.Object, mockCommon.Object);
            //---------------------------Act---------------------------------
            //---------------------------Assert------------------------------
            Assert.ThrowsException<Exception>(() => driver.ValidateCopySourceDestinationFileOperation(mockActivityIOOperationsEndPointSrc.Object, mockActivityIOOperationsEndPointDst.Object, mockDev2CRUDOperationTO.Object, () => "test func"), "A file with the same name exists on the destination and overwrite is set to false");
            mockActivityIOOperationsEndPointDst.VerifyAll();
            mockActivityIOOperationsEndPointSrc.VerifyAll();
            mockActivityIOPathSrc.VerifyAll();
            mockActivityIOPathDst.VerifyAll();
            mockCommon.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateCopySourceDestinationFileOperation_EnsureFilesDontExists_Overwrite_False_PathTypeIsNotFile_ExpectFuncInvoke()
        {
            //---------------------------Arrange-----------------------------
            var mockActivityIOOperationsEndPointSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockActivityIOOperationsEndPointDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockActivityIOPathSrc = new Mock<IActivityIOPath>();
            var mockActivityIOPathDst = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();

            var srcPath = Path.GetTempPath();
            const string dstPath = "C:\\Test_TempPath\\";

            mockActivityIOOperationsEndPointSrc.Setup(o => o.IOPath).Returns(mockActivityIOPathSrc.Object);
            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);

            mockActivityIOOperationsEndPointSrc.Setup(o => o.PathSeperator()).Returns(@"\");
            mockActivityIOOperationsEndPointDst.Setup(o => o.PathSeperator()).Returns(@"\");

            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);

            mockActivityIOOperationsEndPointDst.Setup(o => o.ListDirectory(It.IsAny<IActivityIOPath>())).Returns(new List<IActivityIOPath>());

            mockActivityIOPathDst.Setup(o => o.Path).Returns(dstPath);
            mockActivityIOPathSrc.Setup(o => o.Path).Returns(srcPath);

            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(false);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);

            mockCommon.Setup(o => o.ValidateSourceAndDestinationPaths(It.IsAny<IActivityIOOperationsEndPoint>(), It.IsAny<IActivityIOOperationsEndPoint>()));

            var driver = new ActivityIOBrokerValidatorDriver(mockFile.Object, mockCommon.Object);
            //---------------------------Act---------------------------------
            var fileOperation = driver.ValidateCopySourceDestinationFileOperation(mockActivityIOOperationsEndPointSrc.Object, mockActivityIOOperationsEndPointDst.Object, mockDev2CRUDOperationTO.Object, () => "test func");
            //---------------------------Assert------------------------------
            Assert.AreEqual("test func", fileOperation);
            mockActivityIOOperationsEndPointDst.VerifyAll();
            mockActivityIOOperationsEndPointSrc.VerifyAll();
            mockActivityIOPathSrc.VerifyAll();
            mockActivityIOPathDst.VerifyAll();
            mockCommon.VerifyAll();
            mockDev2CRUDOperationTO.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateCopySourceDestinationFileOperation_EnsureFilesDontExists_Overwrite_True_ExpectSuccess()
        {
            //---------------------------Arrange-----------------------------
            var mockActivityIOOperationsEndPointSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockActivityIOOperationsEndPointDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockActivityIOPathSrc = new Mock<IActivityIOPath>();
            var mockActivityIOPathDst = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();

            var srcPath = Path.GetTempPath() + "*";
            const string dstPath = "C:\\Test_TempPath\\";

            mockActivityIOOperationsEndPointSrc.Setup(o => o.IOPath).Returns(mockActivityIOPathSrc.Object);
            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);

            mockActivityIOOperationsEndPointSrc.Setup(o => o.ListFilesInDirectory(mockActivityIOPathSrc.Object)).Returns(new List<IActivityIOPath>());

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathSeperator()).Returns(@"\");

            mockActivityIOPathDst.Setup(o => o.Path).Returns(dstPath);
            mockActivityIOPathSrc.Setup(o => o.Path).Returns(srcPath);

            var list = new List<IActivityIOPath>();
            list.Add(new Dev2ActivityIOPath(enActivityIOPathType.FileSystem, dstPath, null, null, true, null));

            mockActivityIOOperationsEndPointSrc.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.File);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);

            mockCommon.Setup(o => o.ValidateSourceAndDestinationPaths(It.IsAny<IActivityIOOperationsEndPoint>(), It.IsAny<IActivityIOOperationsEndPoint>()));

            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(true);

            var driver = new ActivityIOBrokerValidatorDriver(mockFile.Object, mockCommon.Object);
            //---------------------------Act---------------------------------
            var fileOperation = driver.ValidateCopySourceDestinationFileOperation(mockActivityIOOperationsEndPointSrc.Object, mockActivityIOOperationsEndPointDst.Object, mockDev2CRUDOperationTO.Object, () => "test func");
            //---------------------------Assert------------------------------
            Assert.AreEqual("Success", fileOperation);
            mockActivityIOOperationsEndPointDst.VerifyAll();
            mockActivityIOOperationsEndPointSrc.VerifyAll();
            mockActivityIOPathSrc.VerifyAll();
            mockActivityIOPathDst.VerifyAll();
            mockDev2CRUDOperationTO.Verify();
            mockCommon.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateZipSourceDestinationFileOperation_DstPathExists_ExpectException()
        {
            //---------------------------Arrange-----------------------------
            var mockActivityIOOperationsEndPointSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockActivityIOOperationsEndPointDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDev2ZipOperationTO = new Mock<IDev2ZipOperationTO>();
            var mockActivityIOPathSrc = new Mock<IActivityIOPath>();
            var mockActivityIOPathDst = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();

            const string dstPath = "C:\\Test_TempPath\\Temp_File.txt";

            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathSeperator()).Returns(@"\");

            mockActivityIOPathDst.Setup(o => o.Path).Returns(dstPath);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathExist(It.IsAny<IActivityIOPath>())).Returns(true);

            var driver = new ActivityIOBrokerValidatorDriver(mockFile.Object, mockCommon.Object);
            //---------------------------Act---------------------------------
            //---------------------------Assert------------------------------
            Assert.ThrowsException<Exception>(() => driver.ValidateZipSourceDestinationFileOperation(mockActivityIOOperationsEndPointSrc.Object, mockActivityIOOperationsEndPointDst.Object, mockDev2ZipOperationTO.Object, () => "test func"), "Destination file already exists and overwrite is set to false");
            mockActivityIOOperationsEndPointDst.VerifyAll();
            mockActivityIOOperationsEndPointSrc.VerifyAll();
            mockActivityIOPathSrc.VerifyAll();
            mockActivityIOPathDst.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateZipSourceDestinationFileOperation_DstPathIs_Directory_ExpectException()
        {
            //---------------------------Arrange-----------------------------
            var mockActivityIOOperationsEndPointSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockActivityIOOperationsEndPointDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDev2ZipOperationTO = new Mock<IDev2ZipOperationTO>();
            var mockActivityIOPathSrc = new Mock<IActivityIOPath>();
            var mockActivityIOPathDst = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();

            var srcPath = Path.GetTempPath() + @"\Temp_SrcFile.txt";
            const string dstPath = "C:\\Test_TempPath\\Temp_File.txt";

            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);
            mockActivityIOOperationsEndPointSrc.Setup(o => o.IOPath).Returns(mockActivityIOPathSrc.Object);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathSeperator()).Returns(@"\");
            mockActivityIOOperationsEndPointSrc.Setup(o => o.PathSeperator()).Returns(@"\");
            mockActivityIOOperationsEndPointDst.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

            mockActivityIOPathDst.Setup(o => o.Path).Returns(dstPath);
            mockActivityIOPathSrc.Setup(o => o.Path).Returns(srcPath);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathExist(It.IsAny<IActivityIOPath>())).Returns(false);

            var driver = new ActivityIOBrokerValidatorDriver(mockFile.Object, mockCommon.Object);
            //---------------------------Act---------------------------------
            //---------------------------Assert------------------------------
            Assert.ThrowsException<Exception>(() => driver.ValidateZipSourceDestinationFileOperation(mockActivityIOOperationsEndPointSrc.Object, mockActivityIOOperationsEndPointDst.Object, mockDev2ZipOperationTO.Object, () => "test func"), @"Recursive Directory Create Failed For [ C:\Test_TempPath\Temp_File.txt ]");
            mockActivityIOOperationsEndPointDst.VerifyAll();
            mockActivityIOOperationsEndPointSrc.VerifyAll();
            mockActivityIOPathSrc.VerifyAll();
            mockActivityIOPathDst.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateZipSourceDestinationFileOperation_SrcPathIs_Directory_ExpectException()
        {
            //---------------------------Arrange-----------------------------
            var mockActivityIOOperationsEndPointSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockActivityIOOperationsEndPointDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDev2ZipOperationTO = new Mock<IDev2ZipOperationTO>();
            var mockActivityIOPathSrc = new Mock<IActivityIOPath>();
            var mockActivityIOPathDst = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();

            var srcPath = Path.GetTempPath() + @"\Temp_SrcFile.txt";
            const string dstPath = "C:\\Test_TempPath\\Temp_File.txt";

            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);
            mockActivityIOOperationsEndPointSrc.Setup(o => o.IOPath).Returns(mockActivityIOPathSrc.Object);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathSeperator()).Returns(@"\");
            mockActivityIOOperationsEndPointSrc.Setup(o => o.PathSeperator()).Returns(@"\");
            mockActivityIOOperationsEndPointSrc.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
            mockActivityIOOperationsEndPointDst.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

            mockActivityIOPathDst.Setup(o => o.Path).Returns(dstPath);
            mockActivityIOPathSrc.Setup(o => o.Path).Returns(srcPath);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathExist(It.IsAny<IActivityIOPath>())).Returns(false);

            var driver = new ActivityIOBrokerValidatorDriver(mockFile.Object, mockCommon.Object);
            //---------------------------Act---------------------------------
            //---------------------------Assert------------------------------
            Assert.ThrowsException<Exception>(() => driver.ValidateZipSourceDestinationFileOperation(mockActivityIOOperationsEndPointSrc.Object, mockActivityIOOperationsEndPointDst.Object, mockDev2ZipOperationTO.Object, () => "test func"), @"Recursive Directory Create Failed For [ C:\Test_TempPath\Temp_File.txt ]");
            mockActivityIOOperationsEndPointDst.VerifyAll();
            mockActivityIOOperationsEndPointSrc.VerifyAll();
            mockActivityIOPathSrc.VerifyAll();
            mockActivityIOPathDst.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ActivityIOBrokerValidatorDriver))]
        public void ActivityIOBrokerValidatorDriver_ValidateZipSourceDestinationFileOperation_SrcPathIs_Directory_ExpectException1()
        {
            //---------------------------Arrange-----------------------------
            var mockActivityIOOperationsEndPointSrc = new Mock<IActivityIOOperationsEndPoint>();
            var mockActivityIOOperationsEndPointDst = new Mock<IActivityIOOperationsEndPoint>();
            var mockDev2ZipOperationTO = new Mock<IDev2ZipOperationTO>();
            var mockActivityIOPathSrc = new Mock<IActivityIOPath>();
            var mockActivityIOPathDst = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockCommon = new Mock<ICommon>();

            var srcPath = Path.GetTempPath() + @"\Temp_SrcFile.txt";
            const string dstPath = "C:\\Test_TempPath\\Temp_File.txt";

            mockActivityIOOperationsEndPointDst.Setup(o => o.IOPath).Returns(mockActivityIOPathDst.Object);
            mockActivityIOOperationsEndPointSrc.Setup(o => o.IOPath).Returns(mockActivityIOPathSrc.Object);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathSeperator()).Returns(@"\");
            mockActivityIOOperationsEndPointSrc.Setup(o => o.PathSeperator()).Returns(@"\");
            mockActivityIOOperationsEndPointSrc.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);
            mockActivityIOOperationsEndPointDst.Setup(o => o.PathIs(It.IsAny<IActivityIOPath>())).Returns(enPathType.Directory);

            mockActivityIOOperationsEndPointDst.Setup(o => o.CreateDirectory(It.IsAny<IActivityIOPath>(), It.IsAny<IDev2CRUDOperationTO>())).Returns(true);

            mockActivityIOPathDst.Setup(o => o.Path).Returns(dstPath);
            mockActivityIOPathSrc.Setup(o => o.Path).Returns(srcPath);

            mockActivityIOOperationsEndPointDst.Setup(o => o.PathExist(It.IsAny<IActivityIOPath>())).Returns(false);

            var driver = new ActivityIOBrokerValidatorDriver(mockFile.Object, mockCommon.Object);
            //---------------------------Act---------------------------------
            var fileOperation = driver.ValidateZipSourceDestinationFileOperation(mockActivityIOOperationsEndPointSrc.Object, mockActivityIOOperationsEndPointDst.Object, mockDev2ZipOperationTO.Object, () => "test func");
            //---------------------------Assert------------------------------
            Assert.AreEqual("test func", fileOperation);
            mockActivityIOOperationsEndPointDst.VerifyAll();
            mockActivityIOOperationsEndPointSrc.VerifyAll();
            mockActivityIOPathSrc.VerifyAll();
            mockActivityIOPathDst.VerifyAll();
        }
    }
}
