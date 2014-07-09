using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public class ForenameRuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ForenameRule_IsValid")]
        public void ForenameRule_IsValid_CorrectRules_Invoked()
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(false).Verifiable();
            validator.Setup(v => v.IsNumeric(It.IsAny<string>())).Returns(false).Verifiable();
            validator.Setup(v => v.IsUpper(It.IsAny<string>())).Returns(true).Verifiable();
            validator.Setup(v => v.IsCountOfLessThanOrEqualTo(It.IsAny<string>(), It.Is<char>(c => c == '-'), It.Is<int>(i => i == 1))).Returns(true).Verifiable();
            validator.Setup(v => v.IsCountOfLessThanOrEqualTo(It.IsAny<string>(), It.Is<char>(c => c == '\''), It.Is<int>(i => i == 1))).Returns(true).Verifiable();
            validator.Setup(v => v.ContainsSpecialChars(It.IsAny<string>())).Returns(false).Verifiable();
            validator.Setup(v => v.IsLengthGreaterThan(It.IsAny<string>(), It.Is<int>(i => i == 1))).Returns(true).Verifiable();
            validator.Setup(v => v.IsLengthLessThanOrEqualTo(It.IsAny<string>(), It.Is<int>(i => i == 50))).Returns(true).Verifiable();

            var rule = new ForenameRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            rule.IsValid(It.IsAny<string>());

            //------------Assert Results-------------------------
            validator.Verify(v => v.IsNullOrEmpty(It.IsAny<string>()));
            validator.Verify(v => v.IsNumeric(It.IsAny<string>()));
            validator.Verify(v => v.IsUpper(It.IsAny<string>()));
            validator.Verify(v => v.IsCountOfLessThanOrEqualTo(It.IsAny<string>(), It.Is<char>(c => c == '-'), It.Is<int>(i => i == 1)));
            validator.Verify(v => v.IsCountOfLessThanOrEqualTo(It.IsAny<string>(), It.Is<char>(c => c == '\''), It.Is<int>(i => i == 1)));
            validator.Verify(v => v.ContainsSpecialChars(It.IsAny<string>()));
            validator.Verify(v => v.IsLengthGreaterThan(It.IsAny<string>(), It.Is<int>(i => i == 1)));
            validator.Verify(v => v.IsLengthLessThanOrEqualTo(It.IsAny<string>(), It.Is<int>(i => i == 50)));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ForenameRule_IsValid")]
        public void ForenameRule_IsValid_AllOk_True()
        {
            ForenameRule_IsValid_Verify(expected: true, isNull: false, isNumeric: false, isUpper: true, isCountOfHypenLessThanOrEqualTo1: true, isCountOfApostropheLessThanOrEqualTo1: true, containsSpecialChars: false, isLengthGreaterThan1: true, isLengthLessThanOrEqualTo50: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ForenameRule_IsValid")]
        public void ForenameRule_IsValid_Null_False()
        {
            ForenameRule_IsValid_Verify(expected: false, isNull: true, isNumeric: false, isUpper: true, isCountOfHypenLessThanOrEqualTo1: true, isCountOfApostropheLessThanOrEqualTo1: true, containsSpecialChars: false, isLengthGreaterThan1: true, isLengthLessThanOrEqualTo50: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ForenameRule_IsValid")]
        public void ForenameRule_IsValid_Numeric_False()
        {
            ForenameRule_IsValid_Verify(expected: false, isNull: false, isNumeric: true, isUpper: true, isCountOfHypenLessThanOrEqualTo1: true, isCountOfApostropheLessThanOrEqualTo1: true, containsSpecialChars: false, isLengthGreaterThan1: true, isLengthLessThanOrEqualTo50: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ForenameRule_IsValid")]
        public void ForenameRule_IsValid_Lower_False()
        {
            ForenameRule_IsValid_Verify(expected: false, isNull: false, isNumeric: false, isUpper: false, isCountOfHypenLessThanOrEqualTo1: true, isCountOfApostropheLessThanOrEqualTo1: true, containsSpecialChars: false, isLengthGreaterThan1: true, isLengthLessThanOrEqualTo50: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ForenameRule_IsValid")]
        public void ForenameRule_IsValid_CountOfHypenGreaterThan1_False()
        {
            ForenameRule_IsValid_Verify(expected: false, isNull: false, isNumeric: false, isUpper: true, isCountOfHypenLessThanOrEqualTo1: false, isCountOfApostropheLessThanOrEqualTo1: true, containsSpecialChars: false, isLengthGreaterThan1: true, isLengthLessThanOrEqualTo50: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ForenameRule_IsValid")]
        public void ForenameRule_IsValid_CountOfApostropheGreaterThan1_False()
        {
            ForenameRule_IsValid_Verify(expected: false, isNull: false, isNumeric: false, isUpper: true, isCountOfHypenLessThanOrEqualTo1: true, isCountOfApostropheLessThanOrEqualTo1: false, containsSpecialChars: false, isLengthGreaterThan1: true, isLengthLessThanOrEqualTo50: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ForenameRule_IsValid")]
        public void ForenameRule_IsValid_ContainsSpecialChars_False()
        {
            ForenameRule_IsValid_Verify(expected: false, isNull: false, isNumeric: false, isUpper: true, isCountOfHypenLessThanOrEqualTo1: true, isCountOfApostropheLessThanOrEqualTo1: true, containsSpecialChars: true, isLengthGreaterThan1: true, isLengthLessThanOrEqualTo50: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ForenameRule_IsValid")]
        public void ForenameRule_IsValid_LengthLessThanOrEqualTo1_False()
        {
            ForenameRule_IsValid_Verify(expected: false, isNull: false, isNumeric: false, isUpper: true, isCountOfHypenLessThanOrEqualTo1: true, isCountOfApostropheLessThanOrEqualTo1: true, containsSpecialChars: false, isLengthGreaterThan1: false, isLengthLessThanOrEqualTo50: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ForenameRule_IsValid")]
        public void ForenameRule_IsValid_LengthGreaterThan50_False()
        {
            ForenameRule_IsValid_Verify(expected: false, isNull: false, isNumeric: false, isUpper: true, isCountOfHypenLessThanOrEqualTo1: true, isCountOfApostropheLessThanOrEqualTo1: true, containsSpecialChars: false, isLengthGreaterThan1: true, isLengthLessThanOrEqualTo50: false);
        }

        static void ForenameRule_IsValid_Verify(bool expected, bool isNull, bool isNumeric, bool isUpper, bool isCountOfHypenLessThanOrEqualTo1, bool isCountOfApostropheLessThanOrEqualTo1, bool containsSpecialChars, bool isLengthGreaterThan1, bool isLengthLessThanOrEqualTo50)
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(isNull);
            validator.Setup(v => v.IsNumeric(It.IsAny<string>())).Returns(isNumeric);
            validator.Setup(v => v.IsUpper(It.IsAny<string>())).Returns(isUpper);
            validator.Setup(v => v.IsCountOfLessThanOrEqualTo(It.IsAny<string>(), It.Is<char>(c => c == '-'), It.Is<int>(i => i == 1))).Returns(isCountOfHypenLessThanOrEqualTo1);
            validator.Setup(v => v.IsCountOfLessThanOrEqualTo(It.IsAny<string>(), It.Is<char>(c => c == '\''), It.Is<int>(i => i == 1))).Returns(isCountOfApostropheLessThanOrEqualTo1);
            validator.Setup(v => v.ContainsSpecialChars(It.IsAny<string>())).Returns(containsSpecialChars);
            validator.Setup(v => v.IsLengthGreaterThan(It.IsAny<string>(), It.Is<int>(i => i == 1))).Returns(isLengthGreaterThan1);
            validator.Setup(v => v.IsLengthLessThanOrEqualTo(It.IsAny<string>(), It.Is<int>(i => i == 50))).Returns(isLengthLessThanOrEqualTo50);

            var rule = new ForenameRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            var actual = rule.IsValid(isNull ? null : It.IsAny<string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
