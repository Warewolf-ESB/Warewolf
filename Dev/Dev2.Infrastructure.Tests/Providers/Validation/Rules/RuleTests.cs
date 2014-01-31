using System.Diagnostics.CodeAnalysis;
using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
{
    [TestClass][ExcludeFromCodeCoverage]
    public class RuleTests
    {
        //[TestMethod]
        //[Owner("Hagashen Naidu")]
        //[TestCategory("ValueCannotBeNullRule_Construct")]
        //public void ValueCannotBeNullRule_Construct_NullValueToCheck_DoesNotThrowException()
        //{
        //    //------------Setup for test--------------------------
        //    //------------Execute Test---------------------------
        //    var valueCannotBeNullRule = new ValueCannotBeNullRule(null);
        //    //------------Assert Results------------------------- 
        //    Assert.IsNull(valueCannotBeNullRule.ValueToCheck);
        //}        
        
        //[TestMethod]
        //[Owner("Hagashen Naidu")]
        //[TestCategory("ValueCannotBeNullRule_Validate")]
        //public void ValueCannotBeNullRule_Validate_NullValueToCheck_ReturnFalse()
        //{
        //    //------------Setup for test--------------------------
        //    var valueCannotBeNullRule = new ValueCannotBeNullRule(null);
        //    //------------Execute Test---------------------------
        //    var check = valueCannotBeNullRule.Check();
        //    //------------Assert Results------------------------- 
        //    Assert.IsNotNull(check);
        //    Assert.AreEqual("The value cannot be null.", check.Message);
        //    Assert.AreEqual("Please provide a value for this field.", check.FixData);
        //}      
        
        //[TestMethod]
        //[Owner("Hagashen Naidu")]
        //[TestCategory("ValueCannotBeNullRule_Validate")]
        //public void ValueCannotBeNullRule_Validate_NotNullValueToCheck_ReturnTrue()
        //{
        //    //------------Setup for test--------------------------
        //    var valueCannotBeNullRule = new ValueCannotBeNullRule("Value");
        //    //------------Execute Test---------------------------
        //    var check = valueCannotBeNullRule.Check();
        //    //------------Assert Results------------------------- 
        //    Assert.IsNull(check);
        //}

        //[TestMethod]
        //[Owner("Hagashen Naidu")]
        //[TestCategory("StringCannotBeEmptyOrNullRule_Construct")]
        //public void StringCannotBeEmptyOrNullRule_Construct_NullValueToCheck_DoesNotThrowException()
        //{
        //    //------------Setup for test--------------------------
        //    //------------Execute Test---------------------------
        //    var stringCannotBeEmptyOrNullRule = new IsStringNullOrEmptyRule(null);
        //    //------------Assert Results------------------------- 
        //    Assert.IsNull(stringCannotBeEmptyOrNullRule.ValueToCheck);
        //}

        //[TestMethod]
        //[Owner("Hagashen Naidu")]
        //[TestCategory("StringCannotBeEmptyOrNullRule_Validate")]
        //public void StringCannotBeEmptyOrNullRule_Validate_NullValueToCheck_ReturnFalse()
        //{
        //    //------------Setup for test--------------------------
        //    var stringCannotBeEmptyOrNullRule = new IsStringNullOrEmptyRule(null);
        //    //------------Execute Test---------------------------
        //    var check = stringCannotBeEmptyOrNullRule.Check();
        //    //------------Assert Results------------------------- 
        //    Assert.IsNotNull(check);
        //    Assert.AreEqual("The value cannot be null.", check.Message);
        //    Assert.AreEqual("Please provide a value for this field.", check.FixData);
        //}

        //[TestMethod]
        //[Owner("Hagashen Naidu")]
        //[TestCategory("StringCannotBeEmptyOrNullRule_Validate")]
        //public void StringCannotBeEmptyOrNullRule_Validate_NotNullValueToCheck_ReturnTrue()
        //{
        //    //------------Setup for test--------------------------
        //    var stringCannotBeEmptyOrNullRule = new IsStringNullOrEmptyRule("Value");
        //    //------------Execute Test---------------------------
        //    var check = stringCannotBeEmptyOrNullRule.Check();
        //    //------------Assert Results------------------------- 
        //    Assert.IsNull(check);
        //}
    }
}
