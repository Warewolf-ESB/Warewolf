
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Data.ServiceModel.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Data.Tests.TO
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CompileMessageTOTests
    {
        [TestMethod]
        [TestCategory("CompileMessageTOUnitTest")]
        [Description("Test for CompileMessageTO's 'ToErrorInfo' method: A valid CompileMessageTO is constructed and converted to an ErrorInfo object successfully")]
        [Owner("Ashley Lewis")]
        public void CompileMessageTO_CompileMessageTOUnitTest_ToErrorInfo_CorrectErrorInfoReturned()
        {
            //init
            var message = new CompileMessageTO();
            var expectedID = Guid.NewGuid();
            message.UniqueID = expectedID;
            message.ErrorType = ErrorType.Critical;
            const FixType expectedFixType = FixType.ReloadMapping;
            message.MessageType = CompileMessageType.MappingChange;
            message.MessagePayload = "Test Fix Data";

            //exe
            var actual = message.ToErrorInfo();

            //aserts
            Assert.AreEqual(expectedID, actual.InstanceID, "ToErrorInfo created an error info object with an incorrect InstanceID");
            Assert.AreEqual(ErrorType.Critical, actual.ErrorType, "ToErrorInfo created an error info object with an incorrect ErrorType");
            Assert.AreEqual(expectedFixType, actual.FixType, "ToErrorInfo created an error info object with an incorrect FixType");
            Assert.AreEqual(CompileMessageType.MappingChange.GetDescription(), actual.Message, "ToErrorInfo created an error info object with an incorrect Message");
            Assert.AreEqual("Test Fix Data", actual.FixData, "ToErrorInfo created an error info object with incorrect FixData");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CompileMessageTO_Clone")]
        public void CompileMessageTO_Clone_ShouldCloneAllProperties()
        {
            //------------Setup for test--------------------------
            var message = new CompileMessageTO();
            var uniqueID = Guid.NewGuid();
            var workspaceID = Guid.NewGuid();
            const string serviceName = "Some Service Name";
            var messageID = Guid.NewGuid();
            var serviceID = Guid.NewGuid();
            const ErrorType errorType = ErrorType.Critical;
            const FixType fixType = FixType.ReloadMapping;
            const CompileMessageType messageType = CompileMessageType.MappingChange;
            const string messagePayload = "Test Fix Data";
            message.UniqueID = uniqueID;
            message.WorkspaceID = workspaceID;
            message.ServiceID = serviceID;
            message.MessageID = messageID;
            message.ErrorType = errorType;
            message.ServiceName = serviceName;
            message.MessageType = messageType;
            message.MessagePayload = messagePayload;
            //------------Execute Test---------------------------
            var clonedTO = message.Clone();
            //------------Assert Results-------------------------
            Assert.AreEqual(workspaceID, clonedTO.WorkspaceID);
            Assert.AreEqual(messageID, clonedTO.MessageID);
            Assert.AreEqual(serviceID, clonedTO.ServiceID);
            Assert.AreEqual(uniqueID, clonedTO.UniqueID);
            Assert.AreEqual(serviceName, clonedTO.ServiceName);
            Assert.AreEqual(errorType, clonedTO.ErrorType);
            Assert.AreEqual(fixType, clonedTO.ToFixType());
            Assert.AreEqual(messageType, clonedTO.MessageType);
            Assert.AreEqual(messagePayload, clonedTO.MessagePayload);
        }
    }
}
