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

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class DoCreateDirectoryTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoCreateDirectory))]
        public void DoCreateDirectory_ExecuteOperation_ImpersonatedUser_IsNull_ExpectArgumentNullException()
        {
            //---------------------------Arrange--------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();

            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(true);

            var doCreateDirectory = new DoCreateDirectory(mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object);
            //---------------------------Act------------------------------
            //---------------------------Assert---------------------------
            mockDev2CRUDOperationTO.VerifyAll();
            Assert.ThrowsException<ArgumentNullException>(() => doCreateDirectory.ExecuteOperation());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoCreateDirectory))]
        public void DoCreateDirectory_ExecuteOperation_ImpersonatedUser_IsNull_ExpectTrue()
        {
            //---------------------------Arrange--------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            var doCreateDirectory = new DoCreateDirectory(mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, mockDev2LogonProvider.Object, mockFileWrapper.Object, mockDirectory.Object, (arg1, arg2) => null);
            //---------------------------Act------------------------------
            var isExecuteOperation = doCreateDirectory.ExecuteOperation();
            //---------------------------Assert---------------------------
            Assert.IsTrue(isExecuteOperation);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoCreateDirectory))]
        public void DoCreateDirectory_ExecuteOperation_ImpersonatedUser_IsNotNull_ExpectTrue()
        {
            //---------------------------Arrange--------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            var doCreateDirectory = new DoCreateDirectory(mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, mockDev2LogonProvider.Object, mockFileWrapper.Object, mockDirectory.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //---------------------------Act------------------------------
            var isExecuteOperation = doCreateDirectory.ExecuteOperation();
            //---------------------------Assert---------------------------
            Assert.IsTrue(isExecuteOperation);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoCreateDirectory))]
        public void DoCreateDirectory_ExecuteOperation_ImpersonatedUser_IsNotNull_RequiresOverwrite_True_ExpectTrue()
        {
            //---------------------------Arrange--------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(true);

            var doCreateDirectory = new DoCreateDirectory(mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, mockDev2LogonProvider.Object, mockFileWrapper.Object, mockDirectory.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //---------------------------Act------------------------------
            var isExecuteOperation = doCreateDirectory.ExecuteOperation();
            //---------------------------Assert---------------------------
            mockDev2CRUDOperationTO.VerifyAll();
            Assert.IsTrue(isExecuteOperation);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoCreateDirectory))]
        public void DoCreateDirectory_ExecuteOperation_ImpersonatedUser_IsNotNull_RequiresOverwrite_True_ExecuteOperationWithAuthDirectoryExist_True_ExpectTrue()
        {
            //---------------------------Arrange--------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(true);

            mockDirectory.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);
            mockActivityIOPath.Setup(o => o.Path).Returns("testPath");

            var doCreateDirectory = new DoCreateDirectory(mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, mockDev2LogonProvider.Object, mockFileWrapper.Object, mockDirectory.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //---------------------------Act------------------------------
            var isExecuteOperation = doCreateDirectory.ExecuteOperation();
            //---------------------------Assert---------------------------
            mockDirectory.VerifyAll();
            mockActivityIOPath.VerifyAll();
            mockDev2CRUDOperationTO.VerifyAll();
            Assert.IsTrue(isExecuteOperation);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoCreateDirectory))]
        public void DoCreateDirectory_ExecuteOperation_ImpersonatedUser_IsNull_RequiresOverwrite_True_ExecuteOperationWithAuthDirectoryExist_True_ExpectTrue()
        {
            //---------------------------Arrange--------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2CRUDOperationTO = new Mock<IDev2CRUDOperationTO>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFileWrapper = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            mockDev2CRUDOperationTO.Setup(o => o.Overwrite).Returns(true);

            mockDirectory.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);

            var doCreateDirectory = new DoCreateDirectory(mockActivityIOPath.Object, mockDev2CRUDOperationTO.Object, mockDev2LogonProvider.Object, mockFileWrapper.Object, mockDirectory.Object, (arg1, arg2) => null);
            //---------------------------Act------------------------------
            var isExecuteOperation = doCreateDirectory.ExecuteOperation();
            //---------------------------Assert---------------------------
            mockDev2CRUDOperationTO.VerifyAll();
            mockDirectory.VerifyAll();
            Assert.IsTrue(isExecuteOperation);
        }
    }
}
