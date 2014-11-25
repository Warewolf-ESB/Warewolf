
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
using Dev2.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.Validation
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class IsValidExpressionRuleTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_InvalidVariable_RaisesError()
        {
            //------------Setup for test--------------------------
            var validator = new IsValidExpressionRule(() => "[[res#]]", "<ADL><rec><field1/></rec><var1/></ADL>");
            //------------Execute Test---------------------------
            var errorInfo = validator.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(errorInfo);
            Assert.AreEqual("The - Variable name [[res#]] contains invalid character(s)", errorInfo.Message);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_ValidVariable_RaisesNoError()
        {
            //------------Setup for test--------------------------
            var validator = new IsValidExpressionRule(() => "[[var1]]", "<ADL><rec><field1/></rec><var1/></ADL>");
            //------------Execute Test---------------------------
            var result = validator.Check();
            //------------Assert Results-------------------------
            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_VariableIsEmptyString_RaisesNoError()
        {
            //------------Setup for test--------------------------
            var validator = new IsValidExpressionRule(() => "", "<ADL><rec><field1/></rec><var1/></ADL>");
            //------------Execute Test---------------------------
            var result = validator.Check();
            //------------Assert Results-------------------------
            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_MalformedVariable_RaisesAnError()
        {
            //------------Setup for test--------------------------
            var validator = new IsValidExpressionRule(() => "h]]", "<ADL><rec><field1/></rec><var1/></ADL>");
            //------------Execute Test---------------------------
            var errorInfo = validator.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(errorInfo);
            Assert.AreEqual("The - Invalid expression: opening and closing brackets don't match.", errorInfo.Message);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_VariableExpressionIsValid_ReturnsNoError()
        {
            //------------Setup for test--------------------------
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"{0}\" IsEditable=\"\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            var rule = new IsValidExpressionRule(() => "[[a]]", datalist) { LabelText = "MyVar" };
            //------------Execute Test---------------------------
            var errorInfo = rule.Check();
            //------------Assert Results-------------------------
            Assert.IsNull(errorInfo);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_VariableExpressionHasAnUnderscore_ReturnsAnError()
        {
            //------------Setup for test--------------------------
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"{0}\" IsEditable=\"\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            var rule = new IsValidExpressionRule(() => "[[a_b]]", datalist) { LabelText = "MyVar" };
            //------------Execute Test---------------------------
            var errorInfo = rule.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(errorInfo);
            Assert.AreEqual("MyVar -  [[a_b]] does not exist in your variable list", errorInfo.Message);
        }



        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_ExpressionWithPlainTextIsValid_ReturnsNoError()
        {
            //------------Setup for test--------------------------
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            var rule = new IsValidExpressionRule(() => "[[var]]%", datalist) { LabelText = "MyRecSet" };
            //------------Execute Test---------------------------
            var errorInfo = rule.Check();
            //------------Assert Results-------------------------
            Assert.IsNull(errorInfo);
        }




        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_RecordsetExpressionIsValid_ReturnsNoError()
        {
            //------------Setup for test--------------------------
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            var rule = new IsValidExpressionRule(() => "[[rec().set]]", datalist) { LabelText = "MyRecSet" };
            //------------Execute Test---------------------------
            var errorInfo = rule.Check();
            //------------Assert Results-------------------------
            Assert.IsNull(errorInfo);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_VaribaleExpressionHasSpecialCharacter_ReturnsAnError()
        {
            //------------Setup for test--------------------------
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"{0}\" IsEditable=\"\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            var rule = new IsValidExpressionRule(() => "[[a$]]", datalist) { LabelText = "MyVar" };
            //------------Execute Test---------------------------
            var errorInfo = rule.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(errorInfo);
            Assert.AreEqual("MyVar - Variable name [[a$]] contains invalid character(s)", errorInfo.Message);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_RecordsetExpressionHasSpecialCharacter_ReturnsAnError()
        {
            //------------Setup for test--------------------------
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            var rule = new IsValidExpressionRule(() => "[[rec#().set]]", datalist) { LabelText = "MyRecSet" };
            //------------Execute Test---------------------------
            var errorInfo = rule.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(errorInfo);
            Assert.AreEqual("MyRecSet - Recordset name [[rec#]] contains invalid character(s)", errorInfo.Message);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_RecordsetHasANegativeIndex_ReturnsAnError()
        {
            //------------Setup for test--------------------------
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            var rule = new IsValidExpressionRule(() => "[[rec(-1).set]]", datalist) { LabelText = "MyRecSet" };
            //------------Execute Test---------------------------
            var errorInfo = rule.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(errorInfo);
            Assert.AreEqual("MyRecSet - Recordset index [ -1 ] is not greater than zero", errorInfo.Message);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_RecordsetHasAnInvalidIndex_ReturnsAnError()
        {
            //------------Setup for test--------------------------
            const string trueString = "True";
            const string noneString = "None";
            var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);

            var rule = new IsValidExpressionRule(() => "[[rec(**).set]]", datalist) { LabelText = "MyRecSet" };
            //------------Execute Test---------------------------
            var errorInfo = rule.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(errorInfo);
            Assert.AreEqual("MyRecSet - Recordset index (**) contains invalid character(s)", errorInfo.Message);
        }
    }
}
