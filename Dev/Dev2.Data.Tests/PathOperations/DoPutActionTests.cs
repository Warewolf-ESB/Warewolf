/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Factories;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.PathOperations;
using Dev2.Data.PathOperations.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Text;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class DoPutActionTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPutAction))]
        public void DoPutAction_ExecuteOperation_ExpectIOException()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();

            var ss = new DoPutAction( new MemoryStream(), mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, "testWhereToPut");
            //-------------------------Act---------------------------
            //-------------------------Assert------------------------
            Assert.ThrowsException<IOException>(() => ss.ExecuteOperation());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPutAction))]
        public void DoPutAction_ExecuteOperation_IsPathRooted_false_ExpectIOException()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockFilePath = new Mock<IFilePath>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            mockFilePath.Setup(o => o.IsPathRooted(It.IsAny<string>())).Returns(false);

            var ss = new DoPutAction(new MemoryStream(), mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, "TestWhere", mockDev2LogonProvider.Object, mockFileWrapper.Object, new Mock<IFileStreamFactory>().Object, mockFilePath.Object, new Mock<IMemoryStreamFactory>().Object, (arg1, arg2)=> mockWindowsImpersonationContext.Object);
            //-------------------------Act---------------------------
            //-------------------------Assert------------------------
            Assert.ThrowsException<IOException>(()=> ss.ExecuteOperation());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPutAction))]
        public void DoPutAction_ExecuteOperation_IsPathRooted_True_ExpectArgumentNullException()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockFilePath = new Mock<IFilePath>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();
            var mockFileStreamFactory = new Mock<IFileStreamFactory>();
            var mockMemoryStreamFactory = new Mock<IMemoryStreamFactory>();

            mockMemoryStreamFactory.Setup(o => o.New(It.IsAny<byte[]>())).Throws(new ArgumentNullException());
            mockActivityIOPath.Setup(o => o.Path).Returns("src/path");
            mockFileStreamFactory.Setup(o => o.New(It.IsAny<string>(), FileMode.Append)).Returns(new MemoryStream());
            mockFilePath.Setup(o => o.IsPathRooted(It.IsAny<string>())).Returns(true);

            var ss = new DoPutAction(new MemoryStream(), mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, "TestWhere", mockDev2LogonProvider.Object, mockFileWrapper.Object, mockFileStreamFactory.Object, mockFilePath.Object, mockMemoryStreamFactory.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //-------------------------Act---------------------------
            //-------------------------Assert------------------------
            Assert.ThrowsException<ArgumentNullException>(() => ss.ExecuteOperation());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPutAction))]
        public void DoPutAction_ExecuteOperation_IsPathRooted_True__ImpersonatedUser_IsNull_ExpectArgumentNullException()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockFilePath = new Mock<IFilePath>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();
            var mockFileStreamFactory = new Mock<IFileStreamFactory>();
            var mockMemoryStreamFactory = new Mock<IMemoryStreamFactory>();

            mockMemoryStreamFactory.Setup(o => o.New(It.IsAny<byte[]>())).Throws(new ArgumentNullException());
            mockActivityIOPath.Setup(o => o.Path).Returns("src/path");
            mockFileStreamFactory.Setup(o => o.New(It.IsAny<string>(), FileMode.Append)).Returns(new MemoryStream());
            mockFilePath.Setup(o => o.IsPathRooted(It.IsAny<string>())).Returns(true);

            var ss = new DoPutAction(new MemoryStream(), mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, "TestWhere", mockDev2LogonProvider.Object, mockFileWrapper.Object, mockFileStreamFactory.Object, mockFilePath.Object, mockMemoryStreamFactory.Object, (arg1, arg2) => null);
            //-------------------------Act---------------------------
            //-------------------------Assert------------------------
            Assert.ThrowsException<ArgumentNullException>(() => ss.ExecuteOperation());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPutAction))]
        public void DoPutAction_ExecuteOperation_IsPathRooted_True_ImpersonatedUser_IsNull_ExpectTrue()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockFileStreamFactory = new Mock<IFileStreamFactory>();
            var mockMemoryStreamFactory = new Mock<IMemoryStreamFactory>();
            var mockFilePath = new Mock<IFilePath>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            var path = "testPath";
            mockFileStreamFactory.Setup(o => o.New(path, FileMode.Append)).Returns(new MemoryStream());
            mockMemoryStreamFactory.Setup(o => o.New(It.IsAny<byte[]>())).Returns(new MemoryStream());
            mockFilePath.Setup(o => o.IsPathRooted(It.IsAny<string>())).Returns(true);
            mockActivityIOPath.Setup(o => o.Path).Returns(path);
            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(false);

            var ss = new DoPutAction(new MemoryStream(), mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, "TestWhere", mockDev2LogonProvider.Object, mockFileWrapper.Object, mockFileStreamFactory.Object, mockFilePath.Object, mockMemoryStreamFactory.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //-------------------------Act---------------------------
            var xx = ss.ExecuteOperation();
            //-------------------------Assert------------------------
            mockFilePath.VerifyAll();
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(0,xx);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPutAction))]
        public void DoPutAction_ExecuteOperation_IsPathRooted_True__ImpersonatedUser_IsNotNull_IsBase64String_ExpectSuccess()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockFilePath = new Mock<IFilePath>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            mockFilePath.Setup(o => o.IsPathRooted(It.IsAny<string>())).Returns(true);
            mockActivityIOPath.Setup(o => o.Path).Returns("testPath");
            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(true);

            var ss = new DoPutAction(new MemoryStream(), mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, "TestWhere", mockDev2LogonProvider.Object, mockFileWrapper.Object, new Mock<IFileStreamFactory>().Object, mockFilePath.Object, new Mock<IMemoryStreamFactory>().Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //-------------------------Act---------------------------
            var xx = ss.ExecuteOperation();
            //-------------------------Assert------------------------
            mockFilePath.VerifyAll();
            mockActivityIOPath.VerifyAll();
            mockDev2CRUDOperationTO.VerifyAll();
            Assert.AreEqual(0, xx);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPutAction))]
        public void DoPutAction_ExecuteOperation_IsPathRooted_True__ImpersonatedUser_IsNotNull_IsNotBase64String_ExpectSuccess()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockFilePath = new Mock<IFilePath>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            mockFilePath.Setup(o => o.IsPathRooted(It.IsAny<string>())).Returns(true);
            mockActivityIOPath.Setup(o => o.Path).Returns("testPath");
            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(true);

            var ss = new DoPutAction(new MemoryStream(Encoding.ASCII.GetBytes("disIsnnOtaBase64sTrIng=3421")), mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, "TestWhere", mockDev2LogonProvider.Object, mockFileWrapper.Object, new Mock<IFileStreamFactory>().Object, mockFilePath.Object, new Mock<IMemoryStreamFactory>().Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //-------------------------Act---------------------------
            var xx = ss.ExecuteOperation();
            //-------------------------Assert------------------------
            mockFilePath.VerifyAll();
            mockActivityIOPath.VerifyAll();
            mockDev2CRUDOperationTO.VerifyAll();
            Assert.AreEqual(27, xx);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPutAction))]
        public void DoPutAction_ExecuteOperation_IsPathRooted_True_ImpersonatedUser_IsNotNull_FileExist_IsBase64String_ExpectSuccess()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockFileStreamFactory = new Mock<IFileStreamFactory>();
            var mockMemoryStreamFactory = new Mock<IMemoryStreamFactory>();
            var mockFilePath = new Mock<IFilePath>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            var path = "testPath";
            mockFileStreamFactory.Setup(o => o.New(path, FileMode.Append)).Returns(new MemoryStream());
            mockMemoryStreamFactory.Setup(o => o.New(It.IsAny<byte[]>())).Returns(new MemoryStream());
            mockFilePath.Setup(o => o.IsPathRooted(It.IsAny<string>())).Returns(true);
            mockActivityIOPath.Setup(o => o.Path).Returns(path);
            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(false);

            var ss = new DoPutAction(new MemoryStream(), mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, "TestWhere", mockDev2LogonProvider.Object, mockFileWrapper.Object, mockFileStreamFactory.Object, mockFilePath.Object, mockMemoryStreamFactory.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //-------------------------Act---------------------------
            var xx = ss.ExecuteOperation();
            //-------------------------Assert------------------------
            mockFilePath.VerifyAll();
            mockActivityIOPath.VerifyAll();
            mockDev2CRUDOperationTO.VerifyAll();
            Assert.AreEqual(0, xx);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPutAction))]
        public void DoPutAction_ExecuteOperation_IsPathRooted_True_ImpersonatedUser_IsNotNull_FileExist_IsNotBase64String_ExpectSuccess()
        {
            //-------------------------Arrange-----------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockFileStreamFactory = new Mock<IFileStreamFactory>();
            var mockMemoryStreamFactory = new Mock<IMemoryStreamFactory>();
            var mockFilePath = new Mock<IFilePath>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            var path = "testPath";
            mockFileStreamFactory.Setup(o => o.New(path, FileMode.Append)).Returns(new MemoryStream());
            mockMemoryStreamFactory.Setup(o => o.New(It.IsAny<byte[]>())).Returns(new MemoryStream());
            mockFilePath.Setup(o => o.IsPathRooted(It.IsAny<string>())).Returns(true);
            mockActivityIOPath.Setup(o => o.Path).Returns(path);
            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(false);

            var ss = new DoPutAction(new MemoryStream(Encoding.ASCII.GetBytes("disIsnnOtaBase64sTrIng=3421")), mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, "TestWhere", mockDev2LogonProvider.Object, mockFileWrapper.Object, mockFileStreamFactory.Object, mockFilePath.Object, mockMemoryStreamFactory.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //-------------------------Act---------------------------
            var xx = ss.ExecuteOperation();
            //-------------------------Assert------------------------
            mockFilePath.VerifyAll();
            mockActivityIOPath.VerifyAll();
            mockDev2CRUDOperationTO.VerifyAll();
            Assert.AreEqual(27, xx);
        }
    }
}
