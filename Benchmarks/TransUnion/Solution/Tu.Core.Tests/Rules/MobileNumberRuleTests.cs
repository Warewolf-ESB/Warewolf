using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public class MobileNumberRuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MobileNumberRule_IsValid")]
        public void MobileNumberRule_IsValid_CorrectRules_Invoked()
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(false).Verifiable();
            validator.Setup(v => v.IsLengthEqual(It.IsAny<string>(), It.Is<int>(i => i == 10))).Returns(true).Verifiable();
            validator.Setup(v => v.IsNumeric(It.IsAny<string>())).Returns(true).Verifiable();

            var rule = new MobileNumberRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            rule.IsValid(It.IsAny<string>());

            //------------Assert Results-------------------------
            validator.Verify(v => v.IsNullOrEmpty(It.IsAny<string>()));
            validator.Verify(v => v.IsLengthEqual(It.IsAny<string>(), It.Is<int>(i => i == 10)));
            validator.Verify(v => v.IsNumeric(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MobileNumberRule_IsValid")]
        public void MobileNumberRule_IsValid_IsNull_True()
        {
            MobileNumberRule_IsValid_Verify(expected: true, isNull: true, isLengthEqualTo10: false, isNumeric: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MobileNumberRule_IsValid")]
        public void MobileNumberRule_IsValid_IsNotNullAndLengthNot10_False()
        {
            MobileNumberRule_IsValid_Verify(expected: false, isNull: false, isLengthEqualTo10: false, isNumeric: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MobileNumberRule_IsValid")]
        public void MobileNumberRule_IsValid_IsNotNullAndLength10_True()
        {
            MobileNumberRule_IsValid_Verify(expected: true, isNull: false, isLengthEqualTo10: true, isNumeric: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MobileNumberRule_IsValid")]
        public void MobileNumberRule_IsValid_IsNotNullAndNotNumeric_False()
        {
            MobileNumberRule_IsValid_Verify(expected: false, isNull: false, isLengthEqualTo10: true, isNumeric: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MobileNumberRule_IsValid")]
        public void MobileNumberRule_IsValid_IsNotNullAndNumeric_True()
        {
            MobileNumberRule_IsValid_Verify(expected: true, isNull: false, isLengthEqualTo10: true, isNumeric: true);
        }

        static void MobileNumberRule_IsValid_Verify(bool expected, bool isNull, bool isLengthEqualTo10, bool isNumeric)
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(isNull);
            validator.Setup(v => v.IsLengthEqual(It.IsAny<string>(), It.IsAny<int>())).Returns(isLengthEqualTo10);
            validator.Setup(v => v.IsNumeric(It.IsAny<string>())).Returns(isNumeric);

            var rule = new MobileNumberRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            var actual = rule.IsValid(isNull ? null : It.IsAny<string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
