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
    public class DoGetFilesAsPerTypeOperationTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoGetFilesAsPerTypeOperation))]
        public void DoGetFilesAsPerTypeOperation_Ctor_WithNoPath_ExpectNullReferenceException()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            //---------------------------Act-------------------------------
            //---------------------------Assert----------------------------
            Assert.ThrowsException<NullReferenceException>(() => new DoGetFilesAsPerTypeOperation(mockActivityIOPath.Object, Interfaces.Enums.ReadTypes.Files));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoGetFilesAsPerTypeOperation))]
        public void DoGetFilesAsPerTypeOperation_ExecuteOperation_WithUnknownPath_ExpectException()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();

            mockActivityIOPath.Setup(o => o.Path).Returns("testPath");
            //---------------------------Act-------------------------------
            var doGetFilesAsPerTypeOperation = new DoGetFilesAsPerTypeOperation(mockActivityIOPath.Object, Interfaces.Enums.ReadTypes.Files);
            //---------------------------Assert----------------------------
            Assert.ThrowsException<Exception>(() => doGetFilesAsPerTypeOperation.ExecuteOperation());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoGetFilesAsPerTypeOperation))]
        public void DoGetFilesAsPerTypeOperation_ExecuteOperation_ImpersonatedUser_IsNull_AreEqual_ExpectException()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            const string serverLogFile = @"C:\ProgramData\Warewolf\Server Log\wareWolf-Server.log";

            mockActivityIOPath.Setup(o => o.Path).Returns(serverLogFile);
            //---------------------------Act-------------------------------
            var doGetFilesAsPerTypeOperation = new DoGetFilesAsPerTypeOperation(mockActivityIOPath.Object, Interfaces.Enums.ReadTypes.Files, mockDev2LogonProvider.Object, mockFile.Object, mockDirectory.Object, (agr1, arg2) => null)
            {

            };
            //---------------------------Assert----------------------------
            mockActivityIOPath.VerifyAll();
            Assert.ThrowsException<Exception>(() => doGetFilesAsPerTypeOperation.ExecuteOperation());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoGetFilesAsPerTypeOperation))]
        public void DoGetFilesAsPerTypeOperation_ExecuteOperation_ImpersonatedUser_IsNull_IsStarWildCard_True_AreEqual_ExpectTrue()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            const string serverLogFile = @"C:\ProgramData\Warewolf\Server Log\wareWolf-Server.*";

            mockActivityIOPath.Setup(o => o.Path).Returns(serverLogFile);
            //---------------------------Act-------------------------------
            var doGetFilesAsPerTypeOperation = new DoGetFilesAsPerTypeOperation(mockActivityIOPath.Object, Interfaces.Enums.ReadTypes.Files, mockDev2LogonProvider.Object, mockFile.Object, mockDirectory.Object, (agr1, arg2) => null)
            {

            };
            var list = doGetFilesAsPerTypeOperation.ExecuteOperation();
            //---------------------------Assert----------------------------
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoGetFilesAsPerTypeOperation))]
        public void DoGetFilesAsPerTypeOperation_ExecuteOperation_ImpersonatedUser_IsNotNull_AreEqual_ExpectTrue()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();
            mockWindowsImpersonationContext.Setup(w => w.Identity).Returns(WindowsIdentity.GetCurrent());

            const string serverLogFile = @"C:\ProgramData\Warewolf\Server Log\wareWolf-Server.log";

            mockActivityIOPath.Setup(o => o.Path).Returns(serverLogFile);
            //---------------------------Act-------------------------------
            var doGetFilesAsPerTypeOperation = new DoGetFilesAsPerTypeOperation(mockActivityIOPath.Object, Interfaces.Enums.ReadTypes.Files, mockDev2LogonProvider.Object, mockFile.Object, mockDirectory.Object, (agr1, arg2) => mockWindowsImpersonationContext.Object)
            {

            };
            var list = doGetFilesAsPerTypeOperation.ExecuteOperation();
            //---------------------------Assert----------------------------
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoGetFilesAsPerTypeOperation))]
        public void DoGetFilesAsPerTypeOperation_ExecuteOperation_ImpersonatedUser_IsNotNull_IsStarWildCard_True_AreEqual_ExpectTrue()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();
            mockWindowsImpersonationContext.Setup(w => w.Identity).Returns(WindowsIdentity.GetCurrent());

            const string serverLogFile = @"C:\ProgramData\Warewolf\Server Log\wareWolf-Server.*";

            mockActivityIOPath.Setup(o => o.Path).Returns(serverLogFile);
            //---------------------------Act-------------------------------
            var doGetFilesAsPerTypeOperation = new DoGetFilesAsPerTypeOperation(mockActivityIOPath.Object, Interfaces.Enums.ReadTypes.Files, mockDev2LogonProvider.Object, mockFile.Object, mockDirectory.Object, (agr1, arg2) => mockWindowsImpersonationContext.Object)
            {

            };
            var list = doGetFilesAsPerTypeOperation.ExecuteOperationWithAuth();
            //---------------------------Assert----------------------------
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoGetFilesAsPerTypeOperation))]
        public void DoGetFilesAsPerTypeOperation_ExecuteOperation_DirectoryExists_True_AreEqual_ExpectTrue()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockFile = new Mock<IFile>();
            var mockDirectory = new Mock<IDirectory>();

            const string serverLogFile = @"C:\ProgramData\Warewolf\Server Log\wareWolf-Server.log";

            mockDirectory.Setup(o => o.Exists(It.IsAny<string>())).Returns(true);
            mockActivityIOPath.Setup(o => o.Path).Returns(serverLogFile);
            //---------------------------Act-------------------------------
            var doGetFilesAsPerTypeOperation = new DoGetFilesAsPerTypeOperation(mockActivityIOPath.Object, Interfaces.Enums.ReadTypes.Files, mockDev2LogonProvider.Object, mockFile.Object, mockDirectory.Object, (agr1, arg2) => null)
            {

            };
            var list = doGetFilesAsPerTypeOperation.ExecuteOperation();
            //---------------------------Assert----------------------------
            mockDirectory.VerifyAll();
            mockActivityIOPath.VerifyAll();
            Assert.AreEqual(0, list.Count);
        }
    }
}
