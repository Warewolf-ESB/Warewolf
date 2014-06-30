using Dev2.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.Validation
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class IsValidExpressionRuleTest
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidExpressionRule_Check")]
        public void IsValidExpressionRule_Check_InvalidVariable_RaisesError()
        {
            //------------Setup for test--------------------------
            var validator = new IsValidExpressionRule(() => "[[res#]]", "<ADL><rec><field1/></rec><var1/></ADL>");
            //------------Execute Test---------------------------
            var result = validator.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
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
    }
}
