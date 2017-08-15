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
        [TestCategory("DecisionDisplayHeler_GetValue")]
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
    }
}