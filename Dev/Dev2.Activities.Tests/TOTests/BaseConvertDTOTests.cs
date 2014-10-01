
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.TOTests
{
    /// <summary>
    /// Summary description for BaseConvertDTOTests
    /// </summary>
    [TestClass]
    public class BaseConvertDTOTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("BaseConvertDTO_Constructor")]
        public void BaseConvertDTO_Constructor_FullConstructor_DefaultValues()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var baseConvertDTO = new BaseConvertTO(string.Empty,null,null,null,1);
            //------------Assert Results-------------------------
            Assert.AreEqual("Text", baseConvertDTO.FromType);
            Assert.AreEqual("Base 64", baseConvertDTO.ToType);
            Assert.AreEqual(string.Empty, baseConvertDTO.ToExpression);
            Assert.AreEqual(1, baseConvertDTO.IndexNumber);
            Assert.IsNull(baseConvertDTO.Errors);
        }

        #region CanAdd Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("BaseConvertTO_CanAdd")]
        public void BaseConvertTO_CanAdd_FromExpressionEmpty_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var baseConvertTO = new BaseConvertTO() { FromExpression = string.Empty };
            //------------Assert Results-------------------------
            Assert.IsFalse(baseConvertTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("BaseConvertTO_CanAdd")]
        public void BaseConvertTO_CanAdd_FromExpressionHasData_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var baseConvertTO = new BaseConvertTO() { FromExpression = "Value" };
            //------------Assert Results-------------------------
            Assert.IsTrue(baseConvertTO.CanAdd());
        }

        #endregion

        #region CanRemove Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("BaseConvertTO_CanRemove")]
        public void BaseConvertTO_CanRemove_FromExpressionEmpty_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var baseConvertTO = new BaseConvertTO(){FromExpression = string.Empty};
            //------------Assert Results-------------------------
            Assert.IsTrue(baseConvertTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("BaseConvertTO_CanRemove")]
        public void BaseConvertTO_CanRemove_FromExpressionWithData_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var baseConvertTO = new BaseConvertTO(){FromExpression = "Value"};
            //------------Assert Results-------------------------
            Assert.IsFalse(baseConvertTO.CanRemove());
        }

        #endregion
    }
}
