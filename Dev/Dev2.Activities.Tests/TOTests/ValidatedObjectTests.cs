using System.Collections.Generic;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.TOTests
{
    [TestClass]
    public class ValidatedObjectTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ValidatedObject_Constructor")]
        public void ValidatedObject_Constructor_Properties_Initialized()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            var validatedObject = new TestValidatedObject();

            //------------Assert Results-------------------------
            Assert.IsNotNull(validatedObject.Errors);
            Assert.AreEqual("", validatedObject.Error);
            Assert.IsNull(validatedObject[It.IsAny<string>()]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ValidatedObject_Validate")]
        public void ValidatedObject_ValidatePropertyNameOnly_InvokesGetRuleSet()
        {
            //------------Setup for test--------------------------
            const string ExpectedPropertyName = "TestProp";
            string actualPropertyName = null;
            var validatedObject = new Mock<ValidatedObject> { CallBase = true };
            validatedObject.Setup(v => v.GetRuleSet(It.IsAny<string>()))
                .Callback((string propertyName) => actualPropertyName = propertyName)
                .Returns(new RuleSet())
                .Verifiable();

            //------------Execute Test---------------------------
            validatedObject.Object.Validate(ExpectedPropertyName);

            //------------Assert Results-------------------------
            validatedObject.Verify(v => v.GetRuleSet(It.IsAny<string>()));
            Assert.AreEqual(ExpectedPropertyName, actualPropertyName);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ValidatedObject_Validate")]
        public void ValidatedObject_ValidatePropertyNameAndRuleSet_NullRuleSet_DoesNotInvokesRuleSetAndReturnsTrue()
        {
            //------------Setup for test--------------------------
            const string PropertyName = "TestProp";
            var ruleSet = new Mock<IRuleSet>();
            ruleSet.Setup(r => r.ValidateRules()).Verifiable();

            var validatedObject = new Mock<ValidatedObject> { CallBase = true };
            validatedObject.Setup(v => v.GetRuleSet(It.IsAny<string>())).Returns(ruleSet.Object);

            //------------Execute Test---------------------------
            var result = validatedObject.Object.Validate(PropertyName, null);

            //------------Assert Results-------------------------
            var errors = validatedObject.Object.Errors;
            Assert.AreEqual(1, errors.Count);
            Assert.IsNotNull(errors[PropertyName]);

            ruleSet.Verify(r => r.ValidateRules(), Times.Never());
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ValidatedObject_Validate")]
        public void ValidatedObject_ValidatePropertyNameAndRuleSet_NonNullRuleSet_InvokesRuleSetValidateAndReturnsFalse()
        {
            //------------Setup for test--------------------------
            var expectedErrors = new List<IActionableErrorInfo> { new ActionableErrorInfo() };

            const string PropertyName = "TestProp";
            var ruleSet = new Mock<IRuleSet>();
            ruleSet.Setup(r => r.ValidateRules())
                .Returns(expectedErrors)
                .Verifiable();

            var validatedObject = new Mock<ValidatedObject> { CallBase = true };
            validatedObject.Setup(v => v.GetRuleSet(It.IsAny<string>())).Returns(ruleSet.Object);

            //------------Execute Test---------------------------
            var result = validatedObject.Object.Validate(PropertyName, ruleSet.Object);

            //------------Assert Results-------------------------
            var errors = validatedObject.Object.Errors;
            Assert.AreEqual(1, errors.Count);

            var actualErrors = errors[PropertyName];
            Assert.IsNotNull(actualErrors);
            CollectionAssert.AreEqual(expectedErrors, actualErrors);

            ruleSet.Verify(r => r.ValidateRules(), Times.Once());
            Assert.AreEqual(expectedErrors.Count == 0, result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ValidatedObject_Validate")]
        public void ValidatedObject_ValidatePropertyNameAndRuleSet_PropertyNameIsNull_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var validatedObject = new Mock<ValidatedObject> { CallBase = true };

            //------------Execute Test---------------------------
            var result = validatedObject.Object.Validate(null, null);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }
    }

    public class TestValidatedObject : ValidatedObject
    {
        public override IRuleSet GetRuleSet(string propertyName)
        {
            return null;
        }
    }
}
