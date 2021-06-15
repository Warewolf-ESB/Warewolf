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
                if (decisionType == enDecisionType.Choose)
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
            var errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.Choose);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_Choose, "Decision Choose Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsError);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsError, "Decision IsError Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNotError);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotError, "Decision IsNotError Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNull);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNull, "Decision IsNull Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNotNull);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotNull, "Decision IsNotNull Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNumeric);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNumeric, "Decision IsNumeric Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNotNumeric);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotNumeric, "Decision IsNotNumeric Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsText);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsText, "Decision IsText Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNotText);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotText, "Decision IsNotText Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsAlphanumeric);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsAlphanumeric, "Decision IsAlphanumeric Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNotAlphanumeric);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotAlphanumeric, "Decision IsNotAlphanumeric Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsXML);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsXML, "Decision IsXML Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNotXML);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotXML, "Decision IsNotXML Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsDate);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsDate, "Decision IsDate Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNotDate);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotDate, "Decision IsNotDate Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsEmail);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsEmail, "Decision IsEmail Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNotEmail);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotEmail, "Decision IsNotEmail Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsRegEx);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsRegEx, "Decision IsRegEx Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.NotRegEx);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotRegEx, "Decision NotRegEx Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsEqual);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_Equals, "Decision IsEqual Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNotEqual);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotEqual, "Decision IsNotEqual Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsLessThan);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsLessThan, "Decision IsLessThan Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsLessThanOrEqual);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsLessThanOrEqual, "Decision IsLessThanOrEqual Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsGreaterThan);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsGreaterThan, "Decision IsGreaterThan Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsGreaterThanOrEqual);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsGreaterThanOrEqual, "Decision IsGreaterThanOrEqual Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsContains);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsContains, "Decision IsContains Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.NotContain);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotContain, "Decision NotContain Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsEndsWith);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsEndsWith, "Decision IsEndsWith Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.NotEndsWith);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotEndsWith, "Decision NotEndsWith Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsStartsWith);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsStartsWith, "Decision IsStartsWith Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.NotStartsWith);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotStartsWith, "Decision NotStartsWith Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsBetween);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsBetween, "Decision IsBetween Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.NotBetween);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_NotBetween, "Decision NotBetween Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsBinary);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsBinary, "Decision IsBinary Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNotBinary);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotBinary, "Decision IsNotBinary Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsHex);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsHex, "Decision IsHex Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsNotHex);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsNotHex, "Decision IsNotHex Failure Error Message Wrong.");
            errorMessage = DecisionDisplayHelper.GetFailureMessage(enDecisionType.IsBase64);
            Assert.AreEqual(errorMessage, Warewolf.Resource.Messages.Messages.Test_FailureMessage_IsBase64, "Decision IsBase64 Failure Error Message Wrong.");
        }
    }
}