
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.TOTests
{
    /// <summary>
    /// Summary description for CaseConvertDTOTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CaseConvertDTOTests
    {
        [TestMethod]
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
            Assert.IsNull(caseConvertDTO.Errors);
        }

        #region CanAdd Tests

        [TestMethod]
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

        #endregion

        #region CanRemove Tests

        [TestMethod]
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
