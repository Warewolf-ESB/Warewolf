/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Tests
{
    [TestClass]
    public class GatherSystemInformationTOTests
    {
        [TestMethod]
        public void GatherSystemInformationTOShouldImplementIDev2TOFn()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            var informationTO = new GatherSystemInformationTO();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(informationTO,typeof(IDev2TOFn));
        }

        [TestMethod]
        public void ConstructorWhereParametersExpectSetsProperties()
        {
            //------------Setup for test--------------------------
            const string ResultVariable = "[[Output]]";
            const enTypeOfSystemInformationToGather TOGather = enTypeOfSystemInformationToGather.OperatingSystem;
            const int IndexNumber = 0;
            //------------Execute Test---------------------------
            var gatherSystemInformationTO = new GatherSystemInformationTO(TOGather,ResultVariable,IndexNumber);
            //------------Assert Results-------------------------
            Assert.IsNotNull(gatherSystemInformationTO);
            Assert.AreEqual(ResultVariable,gatherSystemInformationTO.Result);
            Assert.AreEqual(TOGather,gatherSystemInformationTO.EnTypeOfSystemInformation);
            Assert.AreEqual(IndexNumber,gatherSystemInformationTO.IndexNumber);

        }

        #region CanAdd Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("GatherSystemInformationTO_CanAdd")]
        public void GatherSystemInformationTO_CanAdd_ResultEmpty_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var gatherSystemInformationTO = new GatherSystemInformationTO { Result = string.Empty };
            //------------Assert Results-------------------------
            Assert.IsFalse(gatherSystemInformationTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("GatherSystemInformationTO_CanAdd")]
        public void GatherSystemInformationTO_CanAdd_ResultHasData_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var gatherSystemInformationTO = new GatherSystemInformationTO { Result = "Value" };
            //------------Assert Results-------------------------
            Assert.IsTrue(gatherSystemInformationTO.CanAdd());
        }

        #endregion

        #region CanRemove Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("GatherSystemInformationTO_CanRemove")]
        public void GatherSystemInformationTO_CanRemove_ResultEmpty_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var gatherSystemInformationTO = new GatherSystemInformationTO { Result = string.Empty };
            //------------Assert Results-------------------------
            Assert.IsTrue(gatherSystemInformationTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("GatherSystemInformationTO_CanRemove")]
        public void GatherSystemInformationTO_CanRemove_ResultWithData_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var gatherSystemInformationTO = new GatherSystemInformationTO { Result = "Value" };
            //------------Assert Results-------------------------
            Assert.IsFalse(gatherSystemInformationTO.CanRemove());
        }

        #endregion

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(GatherSystemInformationTO))]
        public void GatherSystemInformationTO_FourArgConstructor_PreservesInsertedFlag()
        {
            var to = new GatherSystemInformationTO(enTypeOfSystemInformationToGather.OperatingSystemVersion, "[[r]]", 2, true);

            Assert.IsTrue(to.Inserted);
            Assert.AreEqual(enTypeOfSystemInformationToGather.OperatingSystemVersion, to.EnTypeOfSystemInformation);
            Assert.AreEqual("[[r]]", to.Result);
            Assert.AreEqual(2, to.IndexNumber);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(GatherSystemInformationTO))]
        public void GatherSystemInformationTO_ClearRow_BlanksResult()
        {
            var to = new GatherSystemInformationTO(enTypeOfSystemInformationToGather.ComputerName, "[[a]]", 1);

            to.ClearRow();

            Assert.AreEqual("", to.Result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(GatherSystemInformationTO))]
        public void GatherSystemInformationTO_IsResultFocused_GetSet_RoundTrip()
        {
            var to = new GatherSystemInformationTO { IsResultFocused = true };

            Assert.IsTrue(to.IsResultFocused);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(GatherSystemInformationTO))]
        public void GatherSystemInformationTO_GetRuleSet_ResultWithValue_AddsExpressionRule()
        {
            var to = new GatherSystemInformationTO { Result = "[[a]]" };

            var ruleSet = (RuleSet)to.GetRuleSet("Result", "<DataList></DataList>");

            Assert.AreEqual(1, ruleSet.Rules.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(GatherSystemInformationTO))]
        public void GatherSystemInformationTO_GetRuleSet_ResultEmpty_AddsEmptyRule()
        {
            var to = new GatherSystemInformationTO();

            var ruleSet = (RuleSet)to.GetRuleSet("Result", "<DataList></DataList>");

            Assert.AreEqual(1, ruleSet.Rules.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(GatherSystemInformationTO))]
        public void GatherSystemInformationTO_GetRuleSet_UnknownProperty_ReturnsEmptyRuleSet()
        {
            var to = new GatherSystemInformationTO { Result = "[[a]]" };

            var ruleSet = (RuleSet)to.GetRuleSet("SomethingElse", "<DataList></DataList>");

            Assert.AreEqual(0, ruleSet.Rules.Count);
        }
    }

  
}
