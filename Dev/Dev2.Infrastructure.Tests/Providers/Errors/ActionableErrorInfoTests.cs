
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Providers.Errors
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class ActionableErrorInfoTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActionableErrorInfo_Constructor")]
        public void ActionableErrorInfo_Constructor_NoParameters_DoesNotInitializeProperties()
        {
            //------------Setup for test--------------------------           

            //------------Execute Test---------------------------
            var actionableErrorInfo = new ActionableErrorInfo();

            //------------Assert Results-------------------------
            Assert.AreEqual(ErrorType.None, actionableErrorInfo.ErrorType);
            Assert.IsNull(actionableErrorInfo.FixData);
            Assert.AreEqual(FixType.None, actionableErrorInfo.FixType);
            Assert.AreEqual(Guid.Empty, actionableErrorInfo.InstanceID);
            Assert.IsNull(actionableErrorInfo.Message);
            Assert.IsNull(actionableErrorInfo.StackTrace);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActionableErrorInfo_Constructor")]
        public void ActionableErrorInfo_Constructor_WithErrorInfo_InitializesProperties()
        {
            //------------Setup for test--------------------------
            var errorInfo = new ErrorInfo
            {
                ErrorType = ErrorType.Critical,
                FixData = "FixData",
                FixType = FixType.ReloadMapping,
                InstanceID = Guid.NewGuid(),
                Message = "Message",
                StackTrace = "StackTrace"
            };

            var actionInvoked = false;
            var action = new Action(() => { actionInvoked = true; });

            //------------Execute Test---------------------------
            var actionableErrorInfo = new ActionableErrorInfo(errorInfo, action);

            //------------Assert Results-------------------------
            Assert.AreEqual(errorInfo.ErrorType, actionableErrorInfo.ErrorType);
            Assert.AreEqual(errorInfo.FixData, actionableErrorInfo.FixData);
            Assert.AreEqual(errorInfo.FixType, actionableErrorInfo.FixType);
            Assert.AreEqual(errorInfo.InstanceID, actionableErrorInfo.InstanceID);
            Assert.AreEqual(errorInfo.Message, actionableErrorInfo.Message);
            Assert.AreEqual(errorInfo.StackTrace, actionableErrorInfo.StackTrace);

            actionableErrorInfo.Do();
            Assert.IsTrue(actionInvoked);
        }
    }
}
