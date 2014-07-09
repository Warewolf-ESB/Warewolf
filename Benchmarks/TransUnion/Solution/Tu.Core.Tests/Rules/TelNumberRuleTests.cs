using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public class TelNumberRuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TelNumberRule_IsValid")]
        public void TelNumberRule_IsValid_CorrectRules_Invoked()
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(false).Verifiable();
            validator.Setup(v => v.IsLengthEqual(It.IsAny<string>(), It.Is<int>(i => i == 7))).Returns(true).Verifiable();

            var rule = new TelNumberRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            rule.IsValid(It.IsAny<string>());

            //------------Assert Results-------------------------
            validator.Verify(v => v.IsNullOrEmpty(It.IsAny<string>()));
            validator.Verify(v => v.IsLengthEqual(It.IsAny<string>(), It.Is<int>(i => i == 7)));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TelNumberRule_IsValid")]
        public void TelNumberRule_IsValid_IsNull_True()
        {
            TelNumberRule_IsValid_Verify(expected: true, isNull: true, isLengthEqualTo7: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TelNumberRule_IsValid")]
        public void TelNumberRule_IsValid_IsNotNullAndLengthNot7_False()
        {
            TelNumberRule_IsValid_Verify(expected: false, isNull: false, isLengthEqualTo7: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TelNumberRule_IsValid")]
        public void TelNumberRule_IsValid_IsNotNullAndLength7_True()
        {
            TelNumberRule_IsValid_Verify(expected: true, isNull: false, isLengthEqualTo7: true);
        }

        static void TelNumberRule_IsValid_Verify(bool expected, bool isNull, bool isLengthEqualTo7)
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(isNull);
            validator.Setup(v => v.IsLengthEqual(It.IsAny<string>(), It.IsAny<int>())).Returns(isLengthEqualTo7);

            var rule = new TelNumberRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            var actual = rule.IsValid(isNull ? null : It.IsAny<string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
