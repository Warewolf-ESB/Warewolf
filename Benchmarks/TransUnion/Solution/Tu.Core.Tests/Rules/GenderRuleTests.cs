using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public class GenderRuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GenderRule_IsValid")]
        public void GenderRule_IsValid_CorrectRules_Invoked()
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(false).Verifiable();
            validator.Setup(v => v.IsLengthEqual(It.IsAny<string>(), It.Is<int>(i => i == 1))).Returns(true).Verifiable();

            var rule = new GenderRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            rule.IsValid(It.IsAny<string>());

            //------------Assert Results-------------------------
            validator.Verify(v => v.IsNullOrEmpty(It.IsAny<string>()));
            validator.Verify(v => v.IsLengthEqual(It.IsAny<string>(), It.Is<int>(i => i == 1)));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GenderRule_IsValid")]
        public void GenderRule_IsValid_IsNull_True()
        {
            GenderRule_IsValid_Verify(expected: true, isNull: true, isLengthEqualTo1: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GenderRule_IsValid")]
        public void GenderRule_IsValid_IsNotNullAndLengthNot1_False()
        {
            GenderRule_IsValid_Verify(expected: false, isNull: false, isLengthEqualTo1: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GenderRule_IsValid")]
        public void GenderRule_IsValid_IsNotNullAndLength1_True()
        {
            GenderRule_IsValid_Verify(expected: true, isNull: false, isLengthEqualTo1: true);
        }

        static void GenderRule_IsValid_Verify(bool expected, bool isNull, bool isLengthEqualTo1)
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(isNull);
            validator.Setup(v => v.IsLengthEqual(It.IsAny<string>(), It.IsAny<int>())).Returns(isLengthEqualTo1);

            var rule = new GenderRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            var actual = rule.IsValid(isNull ? null : It.IsAny<string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
