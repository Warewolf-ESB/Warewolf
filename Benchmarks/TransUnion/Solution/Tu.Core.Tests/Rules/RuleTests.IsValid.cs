using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    public partial class RuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Rule_IsValid")]
        public void Rule_IsValid_IRuleDefaultImpl_ReturnsFalse()
        {
            //------------Setup for test--------------------------

            var validator = new Mock<IValidator>();

            //------------Execute Test---------------------------
            var rule = new Rule(validator.Object, "xx");
            var actual = rule.IsValid(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
            Assert.AreEqual(0, rule.Errors.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Rule_IsValid")]
        public void Rule_IsValid_TrueResult_DoesNotAddError()
        {
            //------------Setup for test--------------------------
            const string ErrorMessage = "Test Error";

            var validator = new Mock<IValidator>();

            //------------Execute Test---------------------------
            var rule = new Rule(validator.Object, "xx");
            var actual = rule.IsValid(() => true, ErrorMessage);

            //------------Assert Results-------------------------
            Assert.IsTrue(actual);
            Assert.AreEqual(0, rule.Errors.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Rule_IsValid")]
        public void Rule_IsValid_FalseResult_DoesAddError()
        {
            //------------Setup for test--------------------------
            const string ErrorMessage = "Test Error";
            const string FieldName = "TestField1";

            var validator = new Mock<IValidator>();

            //------------Execute Test---------------------------
            var rule = new Rule(validator.Object, FieldName);
            var actual = rule.IsValid(() => false, ErrorMessage);

            //------------Assert Results-------------------------
            Assert.IsFalse(actual);
            Assert.AreEqual(1, rule.Errors.Count);
            Assert.AreEqual(FieldName + " " + ErrorMessage, rule.Errors[0]);
        }
    }
}
