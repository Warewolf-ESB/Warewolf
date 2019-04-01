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
using Dev2.Common.Common;
using Dev2.Data.Interfaces;
using Dev2.Data.PathOperations;
using Dev2.Data.PathOperations.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class DoDeleteOperationTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoDeleteOperation))]
        public void DoDeleteOperation_ExecuteOperation_CTOR1Param_ImpersonatedUser_IsNull_ExpectFalse()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            //---------------------------Act-------------------------------
            var doDeleteOperation = new DoDeleteOperation(mockActivityIOPath.Object);
            //---------------------------Assert----------------------------
            Assert.IsFalse(doDeleteOperation.ExecuteOperation());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoDeleteOperation))]
        public void DoDeleteOperation_ExecuteOperation_CTOR2Param_ImpersonatedUser_IsNull_ExpectFalse()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            //---------------------------Act-------------------------------
            var doDeleteOperation = new DoDeleteOperation(mockActivityIOPath.Object, mockDev2LogonProvider.Object);
            //---------------------------Assert----------------------------
            Assert.IsFalse(doDeleteOperation.ExecuteOperation());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoDeleteOperation))]
        public void DoDeleteOperation_ExecuteOperation_CTOR3Param_ExecuteOperationWithAuth_Catch_ExpectFalse()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();
            var mockDeleteHelper = new Mock<IDeleteHelper>();
            //---------------------------Act-------------------------------
            var doDeleteOperation = new DoDeleteOperation(mockDeleteHelper.Object, mockActivityIOPath.Object, mockDev2LogonProvider.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //---------------------------Assert----------------------------
            Assert.IsFalse(doDeleteOperation.ExecuteOperation());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoDeleteOperation))]
        public void DoDeleteOperation_ExecuteOperationWithAuth_CTOR3Param_ImpersonatedUser_IsNull_ExpectTrue()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();

            const string serverLogFile = @"C:\ProgramData\Warewolf\Server Log\wareWolf-Server.*";

            mockActivityIOPath.Setup(o => o.Path).Returns(serverLogFile);

            var mockDeleteHelper = new Mock<IDeleteHelper>();
            mockDeleteHelper.Setup(o => o.Delete(serverLogFile)).Returns(true);

            //---------------------------Act-------------------------------
            var doDeleteOperation = new DoDeleteOperation(mockDeleteHelper.Object, mockActivityIOPath.Object, mockDev2LogonProvider.Object, (arg1, arg2) => null);
            //---------------------------Assert----------------------------
            var result = doDeleteOperation.ExecuteOperationWithAuth();

            mockDeleteHelper.Verify(o => o.Delete(serverLogFile), Times.Once);

            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DoDeleteOperation))]
        public void DoDeleteOperation_ExecuteOperationWithAuth_CTOR3Param_DeleteHelperThrows_ReturnFalse()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();

            const string serverLogFile = @"C:\ProgramData\Warewolf\Server Log\wareWolf-Server.*";

            mockActivityIOPath.Setup(o => o.Path).Returns(serverLogFile);

            var mockDeleteHelper = new Mock<IDeleteHelper>();
            mockDeleteHelper.Setup(o => o.Delete(serverLogFile)).Throws(new Exception());

            //---------------------------Act-------------------------------
            var doDeleteOperation = new DoDeleteOperation(mockDeleteHelper.Object, mockActivityIOPath.Object, mockDev2LogonProvider.Object, (arg1, arg2) => null);
            //---------------------------Assert----------------------------
            var result = doDeleteOperation.ExecuteOperationWithAuth();

            mockDeleteHelper.Verify(o => o.Delete(serverLogFile), Times.Once);

            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DoDeleteOperation))]
        public void DoDeleteOperation_ExecuteOperationWithAuth_CTOR3Param_ImpersonatedUser_IsNull_ExpectException()
        {
            //---------------------------Arrange---------------------------
            var mockActivityIOPath = new Mock<IActivityIOPath>();
            var mockDev2LogonProvider = new Mock<IDev2LogonProvider>();
            var mockWindowsImpersonationContext = new Mock<IWindowsImpersonationContext>();

            const string serverLogFile = @"C:\ProgramData\Warewolf\Server Log\wareWolf-Server.*";
            
            mockActivityIOPath.Setup(o => o.Path).Returns(serverLogFile);
            mockWindowsImpersonationContext.Setup(o => o.Undo()).Throws<Exception>();
            var mockDeleteHelper = new Mock<IDeleteHelper>();
            //---------------------------Act-------------------------------
            var doDeleteOperation = new DoDeleteOperation(mockDeleteHelper.Object, mockActivityIOPath.Object, mockDev2LogonProvider.Object, (arg1, arg2) => mockWindowsImpersonationContext.Object);
            //---------------------------Assert----------------------------
            Assert.IsFalse(doDeleteOperation.ExecuteOperation());
        }
    }
}
