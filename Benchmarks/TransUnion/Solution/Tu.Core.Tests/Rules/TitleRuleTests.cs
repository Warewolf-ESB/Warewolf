using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public class TitleRuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TitleRule_IsValid")]
        public void TitleRule_IsValid_CorrectRules_Invoked()
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsLengthLessThanOrEqualTo(It.IsAny<string>(), It.Is<int>(i => i == 50))).Returns(true).Verifiable();

            var rule = new TitleRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            rule.IsValid(It.IsAny<string>());

            //------------Assert Results-------------------------
            validator.Verify(v => v.IsLengthLessThanOrEqualTo(It.IsAny<string>(), It.Is<int>(i => i == 50)));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TitleRule_IsValid")]
        public void TitleRule_IsValid_AllOk_True()
        {
            TitleRule_IsValid_Verify(expected: true, isNull: false, isLengthLessThanOrEqualTo50: true);
            TitleRule_IsValid_Verify(expected: true, isNull: true, isLengthLessThanOrEqualTo50: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TitleRule_IsValid")]
        public void TitleRule_IsValid_IsLengthLessThanOrEqualTo50_False()
        {
            TitleRule_IsValid_Verify(expected: false, isNull: false, isLengthLessThanOrEqualTo50: false);
        }

        static void TitleRule_IsValid_Verify(bool expected, bool isNull, bool isLengthLessThanOrEqualTo50)
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsLengthLessThanOrEqualTo(It.IsAny<string>(), It.IsAny<int>())).Returns(isLengthLessThanOrEqualTo50);

            var rule = new TitleRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            var actual = rule.IsValid(isNull ? null : It.IsAny<string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
