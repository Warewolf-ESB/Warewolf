using System;
using Dev2.Common.ExtMethods;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Providers.Errors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.TO
{
    [TestClass]
    public class CompileMessageTOTests
    {
        [TestMethod]
        [TestCategory("CompileMessageTOUnitTest")]
        [Description("Test for CompileMessageTO's 'ToErrorInfo' method: A valid CompileMessageTO is constructed and converted to an ErrorInfo object successfully")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void CompileMessageTO_CompileMessageTOUnitTest_ToErrorInfo_CorrectErrorInfoReturned()
        // ReSharper restore InconsistentNaming
        {
            //init
            var message = new CompileMessageTO();
            var expectedID = Guid.NewGuid();
            message.UniqueID = expectedID;
            message.ErrorType = ErrorType.Critical;
            var expectedFixType = FixType.ReloadMapping;
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
    }
}
