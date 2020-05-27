/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Tests.Activities.TOTests
{
    [TestClass]
    public class CaseConvertDTOTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Massimo Guerrera")]
        [TestCategory("CaseConvertDTO_Constructor")]
        public void CaseConvertDTO_Constructor_FullConstructor_DefaultValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var caseConvertDTO = new CaseConvertTO(string.Empty,null,null,1);
            //------------Assert Results-------------------------
            Assert.AreEqual("UPPER", caseConvertDTO.ConvertType);            
            Assert.AreEqual(string.Empty, caseConvertDTO.Result);
            Assert.AreEqual(1, caseConvertDTO.IndexNumber);
            Assert.IsNotNull(caseConvertDTO.Errors);
        }

        #region CanAdd Tests

        [TestMethod]
        [Timeout(60000)]
        [Owner("Massimo Guerrera")]
        [TestCategory("CaseConvertTO_CanAdd")]
        public void CaseConvertTO_CanAdd_StringToConvertEmpty_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var caseConvertTO = new CaseConvertTO { StringToConvert = string.Empty };
            //------------Assert Results-------------------------
            Assert.IsFalse(caseConvertTO.CanAdd());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Massimo Guerrera")]
        [TestCategory("CaseConvertTO_CanAdd")]
        public void CaseConvertTO_CanAdd_StringToConvertnHasData_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var caseConvertTO = new CaseConvertTO { StringToConvert = "Value" };
            //------------Assert Results-------------------------
            Assert.IsTrue(caseConvertTO.CanAdd());
        }

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvertTO_GetRuleSet_StringToConvert_ReturnsStringToConvertRule()
        {
            //------------Setup for test--------------------------
            var caseConvertTO = new CaseConvertTO { StringToConvert = "Value" };
            //------------Execute Test---------------------------
            var ruleSet = caseConvertTO.GetRuleSet("StringToConvert", "");
            //------------Assert Results-------------------------
            Assert.IsNotNull(ruleSet);
            Assert.AreEqual(1, ruleSet.Rules.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvertTO_ClearRow_StringToConvert_ReturnsStringToConvertRule()
        {
            //------------Setup for test--------------------------
            var caseConvertTO = new CaseConvertTO { StringToConvert = "Value" };
            //------------Execute Test---------------------------
            caseConvertTO.ClearRow();
            //------------Assert Results-------------------------
            Assert.AreEqual("UPPER", caseConvertTO.ConvertType);
            Assert.AreEqual(string.Empty, caseConvertTO.StringToConvert);
            Assert.AreEqual(string.Empty, caseConvertTO.Result);
        }

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvertTO_GetRuleSet_ConvertType_ReturnsNoRule()
        {
            //------------Setup for test--------------------------
            var caseConvertTO = new CaseConvertTO { StringToConvert = "Value" };
            //------------Execute Test---------------------------
            var ruleSet = caseConvertTO.GetRuleSet("ConvertType", "");
            //------------Assert Results-------------------------
            Assert.IsNotNull(ruleSet);
            Assert.AreEqual(0, ruleSet.Rules.Count);
        }

        #endregion

        #region CanRemove Tests

        [TestMethod]
        [Timeout(60000)]
        [Owner("Massimo Guerrera")]
        [TestCategory("CaseConvertTO_CanRemove")]
        public void CaseConvertTO_CanRemove_StringToConvertEmpty_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var caseConvertTO = new CaseConvertTO { StringToConvert = string.Empty };
            //------------Assert Results-------------------------
            Assert.IsTrue(caseConvertTO.CanRemove());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Massimo Guerrera")]
        [TestCategory("CaseConvertTO_CanRemove")]
        public void CaseConvertTO_CanRemove_StringToConvertWithData_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var caseConvertTO = new CaseConvertTO { StringToConvert = "Value" };
            //------------Assert Results-------------------------
            Assert.IsFalse(caseConvertTO.CanRemove());
        }

        #endregion
    }
}
