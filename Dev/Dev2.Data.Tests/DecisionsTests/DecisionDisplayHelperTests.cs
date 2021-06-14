using Dev2.Data.Decisions.Operations;
using Dev2.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class DecisionDisplayHelperTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        public void DecisionDisplayHelper_GetValue_ShouldMatchToRsOpHandleTypes()
        {
            //------------Setup for test--------------------------
            var allOptions = FindRecsetOptions.FindAllDecision();
            var allMatched = true;
            //------------Execute Test---------------------------
            foreach(var findRecsetOptionse in allOptions)
            {
                var decisionType = DecisionDisplayHelper.GetValue(findRecsetOptionse.HandlesType());
                if (decisionType == EnDecisionType.Choose)
                {
                    allMatched = false;
                    Assert.Fail($"{findRecsetOptionse.HandlesType()} not found");
                }
            }
            //------------Assert Results-------------------------
            Assert.IsTrue(allMatched);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        public void DecisionDisplayHelper_GetErrorMessage_EnumShouldMatchToErrorMessage()
        {
            var errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.Choose);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_Choose, "Decision Choose Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsError);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsError, "Decision IsError Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNotError);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotError, "Decision IsNotError Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNull);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNull, "Decision IsNull Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNotNull);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotNull, "Decision IsNotNull Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNumeric);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNumeric, "Decision IsNumeric Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNotNumeric);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotNumeric, "Decision IsNotNumeric Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsText);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsText, "Decision IsText Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNotText);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotText, "Decision IsNotText Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsAlphanumeric);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsAlphanumeric, "Decision IsAlphanumeric Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNotAlphanumeric);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotAlphanumeric, "Decision IsNotAlphanumeric Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsXml);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsXML, "Decision IsXML Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNotXml);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotXML, "Decision IsNotXML Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsDate);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsDate, "Decision IsDate Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNotDate);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotDate, "Decision IsNotDate Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsEmail);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsEmail, "Decision IsEmail Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNotEmail);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotEmail, "Decision IsNotEmail Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsRegEx);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsRegEx, "Decision IsRegEx Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.NotRegEx);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotRegEx, "Decision NotRegEx Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsEqual);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_Equals, "Decision IsEqual Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNotEqual);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotEqual, "Decision IsNotEqual Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsLessThan);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsLessThan, "Decision IsLessThan Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsLessThanOrEqual);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsLessThanOrEqual, "Decision IsLessThanOrEqual Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsGreaterThan);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsGreaterThan, "Decision IsGreaterThan Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsGreaterThanOrEqual);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsGreaterThanOrEqual, "Decision IsGreaterThanOrEqual Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsContains);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsContains, "Decision IsContains Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.NotContain);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotContain, "Decision NotContain Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsEndsWith);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsEndsWith, "Decision IsEndsWith Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.NotEndsWith);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotEndsWith, "Decision NotEndsWith Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsStartsWith);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsStartsWith, "Decision IsStartsWith Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.NotStartsWith);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotStartsWith, "Decision NotStartsWith Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsBetween);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsBetween, "Decision IsBetween Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.NotBetween);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotBetween, "Decision NotBetween Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsBinary);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsBinary, "Decision IsBinary Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNotBinary);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotBinary, "Decision IsNotBinary Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsHex);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsHex, "Decision IsHex Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsNotHex);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotHex, "Decision IsNotHex Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(EnDecisionType.IsBase64);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsBase64, "Decision IsBase64 Failure Error Message Wrong.");
        }
    }
}