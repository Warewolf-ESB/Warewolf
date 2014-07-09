using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public class GovIDRuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GovIDRule_IsValid")]
        public void GovIDRule_IsValid_CorrectRules_Invoked()
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(false).Verifiable();
            validator.Setup(v => v.IsLengthEqual(It.IsAny<string>(), It.Is<int>(i => i == 13))).Returns(true).Verifiable();
            validator.Setup(v => v.IsNumeric(It.IsAny<string>())).Returns(true).Verifiable();
            validator.Setup(v => v.StartsWith(It.IsAny<string>(), It.Is<string>(s => s == "0000"))).Returns(false).Verifiable();

            var rule = new GovIDRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            rule.IsValid(It.IsAny<string>());

            //------------Assert Results-------------------------
            validator.Verify(v => v.IsNullOrEmpty(It.IsAny<string>()));
            validator.Verify(v => v.IsLengthEqual(It.IsAny<string>(), It.Is<int>(i => i == 13)));
            validator.Verify(v => v.IsNumeric(It.IsAny<string>()));
            validator.Verify(v => v.StartsWith(It.IsAny<string>(), It.Is<string>(s => s == "0000")));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GovIDRule_IsValid")]
        public void GovIDRule_IsValid_AllOk_True()
        {
            GovIDRule_IsValid_Verify(expected: true, isNull: false, isLength13: true, isNumeric: true, startsWith4OrMoreZeroes: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GovIDRule_IsValid")]
        public void GovIDRule_IsValid_IsNull_False()
        {
            GovIDRule_IsValid_Verify(expected: false, isNull: true, isLength13: true, isNumeric: true, startsWith4OrMoreZeroes: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GovIDRule_IsValid")]
        public void GovIDRule_IsValid_IsLength13_False()
        {
            GovIDRule_IsValid_Verify(expected: false, isNull: false, isLength13: false, isNumeric: true, startsWith4OrMoreZeroes: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GovIDRule_IsValid")]
        public void GovIDRule_IsValid_IsNumeric_False()
        {
            GovIDRule_IsValid_Verify(expected: false, isNull: false, isLength13: true, isNumeric: false, startsWith4OrMoreZeroes: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GovIDRule_IsValid")]
        public void GovIDRule_IsValid_StartsWith4orMoreZeroes_False()
        {
            GovIDRule_IsValid_Verify(expected: false, isNull: false, isLength13: true, isNumeric: true, startsWith4OrMoreZeroes: true);
        }

        static void GovIDRule_IsValid_Verify(bool expected, bool isNull, bool isLength13, bool isNumeric, bool startsWith4OrMoreZeroes)
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(isNull);
            validator.Setup(v => v.IsLengthEqual(It.IsAny<string>(), It.IsAny<int>())).Returns(isLength13);
            validator.Setup(v => v.IsNumeric(It.IsAny<string>())).Returns(isNumeric);
            validator.Setup(v => v.StartsWith(It.IsAny<string>(), It.IsAny<string>())).Returns(startsWith4OrMoreZeroes);

            var rule = new GovIDRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            var actual = rule.IsValid(isNull ? null : It.IsAny<string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
