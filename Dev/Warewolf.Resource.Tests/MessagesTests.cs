/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Resource.Tests
{
    [TestClass]
    public class MessagesTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Messages.Messages))]
        public void Messages_Tests()
        {
            Assert.AreEqual("Test message from Warewolf for Email Service Source", Messages.Messages.Test_EmailServerSource_EmailBody);
            Assert.AreEqual("Test Email Service Source", Messages.Messages.Test_EmailServerSource_Header);
            Assert.AreEqual("Failed: Assert Choose. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_Choose);
            Assert.AreEqual("Failed: Assert Equal. Expected Equal To '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_Equals);
            Assert.AreEqual("Failed: Expected Error containing '{0}' but got '{1}'", Messages.Messages.Test_FailureMessage_Error);
            Assert.AreEqual("Failed: Assert Is Alphanumeric. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsAlphanumeric);
            Assert.AreEqual("Failed: Assert Is Base64. Expected Base64 value for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsBase64);
            Assert.AreEqual("Failed: Assert Is Between. Expected '{1}' to be Between '{0}' and '{3}' but got {2}", Messages.Messages.Test_FailureMessage_IsBetween);
            Assert.AreEqual("Failed: Assert Is Binary. Expected Binary value for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsBinary);
            Assert.AreEqual("Failed: Assert Is Contains. Expected Contains '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsContains);
            Assert.AreEqual("Failed: Assert Is Date. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsDate);
            Assert.AreEqual("Failed: Assert Is Email. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsEmail);
            Assert.AreEqual("Failed: Assert Is Ends With. Expected End With '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsEndsWith);
            Assert.AreEqual("Failed: Assert Is Error. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsError);
            Assert.AreEqual("Failed: Assert Is Greater Than. Expected Greater Than '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsGreaterThan);
            Assert.AreEqual("Failed: Assert Is Greater Than Or Equal. Expected Greater Than or Equal to '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsGreaterThanOrEqual);
            Assert.AreEqual("Failed: Assert Is Hex. Expected Hex value for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsHex);
            Assert.AreEqual("Failed: Assert Is Less Than. Expected Less Than '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsLessThan);
            Assert.AreEqual("Failed: Assert Is Less Than Or Equal. Expected Less Than or Equal to '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsLessThanOrEqual);
            Assert.AreEqual("Failed: Assert Is Not Alphanumeric. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotAlphanumeric);
            Assert.AreEqual("Failed: Assert Is Not Base64. Expected Not Base64 value for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotBase64);
            Assert.AreEqual("Failed: Assert Is Not Binary. Expected Not Binary value for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotBinary);
            Assert.AreEqual("Failed: Assert Is Not Date. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotDate);
            Assert.AreEqual("Failed: Assert Is Not Email. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotEmail);
            Assert.AreEqual("Failed: Assert Is Not Equal. Expected Not Equal To '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotEqual);
            Assert.AreEqual("Failed: Assert Is Not Error. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotError);
            Assert.AreEqual("Failed: Assert Is Not Hex. Expected Not Hex value for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotHex);
            Assert.AreEqual("Failed: Assert Is Not Null. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotNull);
            Assert.AreEqual("Failed: Assert Is Not Numeric. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotNumeric);
            Assert.AreEqual("Failed: Assert Is Not Text. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotText);
            Assert.AreEqual("Failed: Assert Is Not XML. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNotXML);
            Assert.AreEqual("Failed: Assert Is Null. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNull);
            Assert.AreEqual("Failed: Assert Is Numeric. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsNumeric);
            Assert.AreEqual("Failed: Assert Is Reg Ex. Expected to match RegEx '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsRegEx);
            Assert.AreEqual("Failed: Assert Is Starts With. Expected Start With '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsStartsWith);
            Assert.AreEqual("Failed: Assert Is Text. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsText);
            Assert.AreEqual("Failed: Assert Is XML. Expected '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_IsXML);
            Assert.AreEqual("Failed: Expected no error but got '{0}'", Messages.Messages.Test_FailureMessage_NoErrorExpected);
            Assert.AreEqual("Failed: Assert Is Between. Expected '{1}' Not to be Between '{0}' and '{3}' but got {2}", Messages.Messages.Test_FailureMessage_NotBetween);
            Assert.AreEqual("Failed: Assert Not Contain. Expected Not Contain '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_NotContain);
            Assert.AreEqual("Failed: Assert Not Ends With. Expected Not End With '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_NotEndsWith);
            Assert.AreEqual("Failed: Assert Not Reg Ex. Expected Not to match RegEx '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_NotRegEx);
            Assert.AreEqual("Failed: Assert Not Starts With. Expected Not Start With '{0}' for '{1}' but got '{2}'", Messages.Messages.Test_FailureMessage_NotStartsWith);
            Assert.AreEqual("Failed", Messages.Messages.Test_FailureResult);
            Assert.AreEqual("Invalid", Messages.Messages.Test_InvalidResult);
            Assert.AreEqual("Failed: The user running the test is not authorized to execute resource {0}.", Messages.Messages.Test_NotAuthorizedMsg);
            Assert.AreEqual("Invalid: Nothing to assert.", Messages.Messages.Test_NothingToAssert);
            Assert.AreEqual("Passed", Messages.Messages.Test_PassedResult);
            Assert.AreEqual("Pending", Messages.Messages.Test_PendingResult);
            Assert.AreEqual("ResourceDelete", Messages.Messages.Test_ResourceDeleteResult);
            Assert.AreEqual("ResourcpathUpdated", Messages.Messages.Test_ResourcpathUpdatedResult);

        }
    }
}
