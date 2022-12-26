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
using Dev2.Data.PathOperations;
using Dev2.Data.PathOperations.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Security.Principal;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class DoPathExistOperationTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPathExistOperation))]
        public void DoPathExistOperation_CTOR_Path_IsNull_ExpectNullReferenceException()
        {
            //--------------------------Arrange-------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();

            var doPathExistOperation = new DoPathExistOperation(mockActivityIOPath.Object);
            //--------------------------Act-----------------------------
            //--------------------------Assert--------------------------
            Assert.ThrowsException<NullReferenceException>(() => doPathExistOperation.ExecuteOperation());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPathExistOperation))]
        public void DoPathExistOperation_ImpersonatedUser_Null_IsFalse_ExpectFalse()
        {
            //--------------------------Arrange-------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockfileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            mockActivityIOPath.Setup(o => o.Path).Returns("testPath");

            var doPathExistOperation = new DoPathExistOperation(mockActivityIOPath.Object, mockDev2LogonProvider.Object, mockfileWrapper.Object, mockDirectory.Object, (arg1, arg2) => null);
            //--------------------------Act-----------------------------
            var isExecuteOperation = doPathExistOperation.ExecuteOperation();
            //--------------------------Assert--------------------------
            mockActivityIOPath.VerifyAll();
            Assert.IsFalse(isExecuteOperation);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPathExistOperation))]
        public void DoPathExistOperation_ImpersonatedUser_IsNotNull_PathExists_IsTrue_ExpectTrue()
        {
            //--------------------------Arrange-------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockfileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            mockActivityIOPath.Setup(o => o.Path).Returns("testPath");
            mockDirectory.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);

            var doPathExistOperation = new DoPathExistOperation(mockActivityIOPath.Object, mockDev2LogonProvider.Object, mockfileWrapper.Object, mockDirectory.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //--------------------------Act-----------------------------
            var isExecuteOperation = doPathExistOperation.ExecuteOperation();
            //--------------------------Assert--------------------------
            mockActivityIOPath.VerifyAll();
            mockDirectory.VerifyAll();
            Assert.IsTrue(isExecuteOperation);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPathExistOperation))]
        public void DoPathExistOperation_ImpersonatedUser_IsNull_DirectoryPathExists_IsTrue_ExpectTrue()
        {
            //--------------------------Arrange-------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockfileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            mockActivityIOPath.Setup(o => o.Path).Returns("testPath");
            mockDirectory.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);

            var doPathExistOperation = new DoPathExistOperation(mockActivityIOPath.Object, mockDev2LogonProvider.Object, mockfileWrapper.Object, mockDirectory.Object, (arg1, arg2) => null);
            //--------------------------Act-----------------------------
            var isExecuteOperation = doPathExistOperation.ExecuteOperation();
            //--------------------------Assert--------------------------
            mockActivityIOPath.VerifyAll();
            mockDirectory.VerifyAll();
            Assert.IsTrue(isExecuteOperation);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPathExistOperation))]
        public void DoPathExistOperation_ImpersonatedUser_IsNull_FileWrapperPathExists_IsTrue_ExpectTrue()
        {
            //--------------------------Arrange-------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockfileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            mockActivityIOPath.Setup(o => o.Path).Returns("ftp://testPath/looger.log");
            mockfileWrapper.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);

            var doPathExistOperation = new DoPathExistOperation(mockActivityIOPath.Object, mockDev2LogonProvider.Object, mockfileWrapper.Object, mockDirectory.Object, (arg1, arg2) => null);
            //--------------------------Act-----------------------------
            var isExecuteOperation = doPathExistOperation.ExecuteOperation();
            //--------------------------Assert--------------------------
            mockActivityIOPath.VerifyAll();
            mockfileWrapper.VerifyAll();
            Assert.IsTrue(isExecuteOperation);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPathExistOperation))]
        public void DoPathExistOperation_ExecuteOperationWithAuth_IsNotNull_FileWrapperPathExists_IsTrue_ExpectTrue()
        {
            //--------------------------Arrange-------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockfileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();
            mockWindowsImpersonationContext.Setup(w => w.Identity).Returns(WindowsIdentity.GetCurrent());

            mockActivityIOPath.Setup(o => o.Path).Returns("ftp://testPath/looger.log");
            mockfileWrapper.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);

            var doPathExistOperation = new DoPathExistOperation(mockActivityIOPath.Object, mockDev2LogonProvider.Object, mockfileWrapper.Object, mockDirectory.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //--------------------------Act-----------------------------
            var isExecuteOperation = doPathExistOperation.ExecuteOperationWithAuth();
            //--------------------------Assert--------------------------
            mockActivityIOPath.VerifyAll();
            mockfileWrapper.VerifyAll();
            Assert.IsTrue(isExecuteOperation);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPathExistOperation))]
        public void DoPathExistOperation_ExecuteOperationWithAuth_IsNotNull_FileWrapperPathExists_IsFalse_ExpectFalse()
        {
            //--------------------------Arrange-------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockfileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();
            mockWindowsImpersonationContext.Setup(w => w.Identity).Returns(WindowsIdentity.GetCurrent());

            mockActivityIOPath.Setup(o => o.Path).Returns("ftp://testPath/looger.log");
            mockDirectory.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);

            var doPathExistOperation = new DoPathExistOperation(mockActivityIOPath.Object, mockDev2LogonProvider.Object, mockfileWrapper.Object, mockDirectory.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //--------------------------Act-----------------------------
            var isExecuteOperation = doPathExistOperation.ExecuteOperationWithAuth();
            //--------------------------Assert--------------------------
            mockActivityIOPath.VerifyAll();
            mockDirectory.VerifyAll();
            Assert.IsFalse(isExecuteOperation);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoPathExistOperation))]
        public void DoPathExistOperation_ExecuteOperationWithAuth_IsNull_FileWrapperPathExists_IsFalse_ExpectFalse()
        {
            //--------------------------Arrange-------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockfileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();
            mockWindowsImpersonationContext.Setup(w => w.Identity).Returns(WindowsIdentity.GetCurrent());

            mockActivityIOPath.Setup(o => o.Path).Returns("ftp://testPath/looger.log");
            mockDirectory.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);

            var doPathExistOperation = new DoPathExistOperation(mockActivityIOPath.Object, mockDev2LogonProvider.Object, mockfileWrapper.Object, mockDirectory.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //--------------------------Act-----------------------------
            var isExecuteOperation = doPathExistOperation.ExecuteOperationWithAuth();
            //--------------------------Assert--------------------------
            mockActivityIOPath.VerifyAll();
            mockDirectory.VerifyAll();
            Assert.IsFalse(isExecuteOperation);
        }
    }
}
