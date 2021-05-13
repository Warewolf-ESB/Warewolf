/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class PerformBoolIOOperationTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformBoolIOOperation))]
        public void PerformBoolIOOperation_PathIs_Directory_AreEqual_ExpectException()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            
            //-------------------------Act---------------------------
            //-------------------------Assert------------------------
            Assert.ThrowsException<NullReferenceException>(() => PerformBoolIOOperation.PathIs(mockActivityIOPath.Object, mockFile.Object, mockDirectory.Object));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformBoolIOOperation))]
        public void PerformBoolIOOperation_PathIs_IsDirectory_AreEqual_ExpectTrue()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            mockActivityIOPath.Setup(o => o.Path).Returns("testPath");
            //-------------------------Act---------------------------
            var pathIs = PerformBoolIOOperation.PathIs(mockActivityIOPath.Object, mockFile.Object, mockDirectory.Object);
            //-------------------------Assert------------------------
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(enPathType.Directory, pathIs);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformBoolIOOperation))]
        public void PerformBoolIOOperation_PathIs_File_IsStarWildCard_False_AreEqual_ExpectTrue()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            var path = @"C:\bigcompanycom\file.txt";

            mockActivityIOPath.Setup(o => o.Path).Returns(path);
            mockFile.Setup(o => o.Exists(path)).Returns(true);
            mockFile.Setup(o => o.GetAttributes(It.IsAny<string>())).Returns(FileAttributes.Compressed | FileAttributes.System);
            //-------------------------Act---------------------------
            var pathIs = PerformBoolIOOperation.PathIs(mockActivityIOPath.Object, mockFile.Object, mockDirectory.Object);
            //-------------------------Assert------------------------
            mockFile.VerifyAll();
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(enPathType.File, pathIs);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformBoolIOOperation))]
        public void PerformBoolIOOperation_PathIs_File_IsStarWildCard_True_AreEqual_ExpectTrue()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            var path = @"C:\bigcompanycom\file.*";

            mockActivityIOPath.Setup(o => o.Path).Returns(path);
            mockDirectory.Setup(o => o.Exists(path)).Returns(true);
            //-------------------------Act---------------------------
            var pathIs = PerformBoolIOOperation.PathIs(mockActivityIOPath.Object, mockFile.Object, mockDirectory.Object);
            //-------------------------Assert------------------------
            mockDirectory.VerifyAll();
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(enPathType.File, pathIs);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformBoolIOOperation))]
        public void PerformBoolIOOperation_PathIs_Directory_AreEqual_ExpectTrue()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            var path = @"C:\bigcompanycom\file.txt";

            mockActivityIOPath.Setup(o => o.Path).Returns(path);
            mockFile.Setup(o => o.Exists(path)).Returns(true);
            mockFile.Setup(o => o.GetAttributes(It.IsAny<string>())).Returns(FileAttributes.Directory | FileAttributes.System);
            //-------------------------Act---------------------------
            var pathIs = PerformBoolIOOperation.PathIs(mockActivityIOPath.Object, mockFile.Object, mockDirectory.Object);
            //-------------------------Assert------------------------
            mockFile.VerifyAll();
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(enPathType.Directory, pathIs);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformBoolIOOperation))]
        public void PerformBoolIOOperation_RequiresOverwrite_Overwrite_IsFalse_ExpectIsNullTrue()
        {
            //-------------------------Arrange-----------------------
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();

            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(false);
            //-------------------------Act---------------------------
            var pathIs = PerformBoolIOOperation.RequiresOverwrite(mockDev2CRUDOperationTO.Object, mockActivityIOPath.Object, mockDev2LogonProvider.Object);
            //-------------------------Assert------------------------
            mockDev2CRUDOperationTO.VerifyAll();
            Assert.IsNull(pathIs);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformBoolIOOperation))]
        public void PerformBoolIOOperation_RequiresOverwrite_Overwrite_IsTrue_ExpectIsNullFalse()
        {
            //-------------------------Arrange-----------------------
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();

            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(true);
            //-------------------------Act---------------------------
            var pathIs = PerformBoolIOOperation.RequiresOverwrite(mockDev2CRUDOperationTO.Object, mockActivityIOPath.Object, mockDev2LogonProvider.Object);
            //-------------------------Assert------------------------
            mockDev2CRUDOperationTO.VerifyAll();
            Assert.IsNotNull(pathIs);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformBoolIOOperation))]
        public void PerformBoolIOOperation_FileExist_IsTrue_ExpectTrue()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockFile = new Mock<IFile>();

            mockActivityIOPath.Setup(o => o.Path).Returns(@"C:\");
            mockFile.Setup(o => o.Exists(@"C:\")).Returns(true);
            //-------------------------Act---------------------------
            var fileExist = PerformBoolIOOperation.FileExist(mockActivityIOPath.Object, mockFile.Object);
            //-------------------------Assert------------------------
            mockFile.VerifyAll();
            mockActivityIOPath.VerifyAll();
            Assert.IsTrue(fileExist);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(PerformBoolIOOperation))]
        public void PerformBoolIOOperation_DirectoryExist_IsTrue_ExpectTrue()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDirectory = new Mock<IDirectory>();

            mockActivityIOPath.Setup(o => o.Path).Returns(@"C:\");
            mockDirectory.Setup(o => o.Exists(@"C:\")).Returns(true);
            //-------------------------Act---------------------------
            var directoryExist = PerformBoolIOOperation.DirectoryExist(mockActivityIOPath.Object, mockDirectory.Object);
            //-------------------------Assert------------------------
            mockDirectory.VerifyAll();
            mockActivityIOPath.VerifyAll();
            Assert.IsTrue(directoryExist);
        }

    }
}
