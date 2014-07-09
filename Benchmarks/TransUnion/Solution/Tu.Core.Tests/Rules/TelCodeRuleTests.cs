using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public class TelCodeRuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TelCodeRule_IsValid")]
        public void TelCodeRule_IsValid_CorrectRules_Invoked()
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(false).Verifiable();
            validator.Setup(v => v.IsLengthEqual(It.IsAny<string>(), It.Is<int>(i => i == 3))).Returns(true).Verifiable();

            var rule = new TelCodeRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            rule.IsValid(It.IsAny<string>());

            //------------Assert Results-------------------------
            validator.Verify(v => v.IsNullOrEmpty(It.IsAny<string>()));
            validator.Verify(v => v.IsLengthEqual(It.IsAny<string>(), It.Is<int>(i => i == 3)));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TelCodeRule_IsValid")]
        public void TelCodeRule_IsValid_IsNull_True()
        {
            TelCodeRule_IsValid_Verify(expected: true, isNull: true, isLengthEqualTo3: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TelCodeRule_IsValid")]
        public void TelCodeRule_IsValid_IsNotNullAndLengthNot3_False()
        {
            TelCodeRule_IsValid_Verify(expected: false, isNull: false, isLengthEqualTo3: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TelCodeRule_IsValid")]
        public void TelCodeRule_IsValid_IsNotNullAndLength3_True()
        {
            TelCodeRule_IsValid_Verify(expected: true, isNull: false, isLengthEqualTo3: true);
        }

        static void TelCodeRule_IsValid_Verify(bool expected, bool isNull, bool isLengthEqualTo3)
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(isNull);
            validator.Setup(v => v.IsLengthEqual(It.IsAny<string>(), It.IsAny<int>())).Returns(isLengthEqualTo3);

            var rule = new TelCodeRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            var actual = rule.IsValid(isNull ? null : It.IsAny<string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
