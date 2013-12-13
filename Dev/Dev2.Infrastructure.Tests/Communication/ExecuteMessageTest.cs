using Dev2.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Communication
{
    [TestClass]
    public class ExecuteMessageTest
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExecuteMessage_Constructor")]
        public void ExecuteMessage_Constructor_WhenSettingMessage_ExpectMessageWithNoErrors()
        {
            //------------Setup for test--------------------------
            var executeMessage = new ExecuteMessage { HasError = false};
            executeMessage.SetMessage("the message");
 
            //------------Assert Results-------------------------

            Assert.AreEqual("the message", executeMessage.Message.ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExecuteMessage_Constructor")]
        public void ExecuteMessage_Constructor_WhenSettingHasErrors_ExpectHasErrorsTrue()
        {
            //------------Setup for test--------------------------
            var executeMessage = new ExecuteMessage { HasError = true};
            executeMessage.SetMessage("the message");

            //------------Assert Results-------------------------

            Assert.IsTrue(executeMessage.HasError);
        }
    }
}
