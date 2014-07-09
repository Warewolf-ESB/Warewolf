using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public class EmailAddressRuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailAddressRule_IsValid")]
        public void EmailAddressRule_IsValid_CorrectRules_Invoked()
        {
            //------------Setup for test--------------------------

            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(false).Verifiable();
            validator.Setup(v => v.IsLengthLessThanOrEqualTo(It.IsAny<string>(), It.Is<int>(i => i == 50))).Returns(true).Verifiable();
            validator.Setup(v => v.IsValidEmailAddress(It.IsAny<string>())).Returns(true).Verifiable();

            var rule = new EmailAddressRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            rule.IsValid(It.IsAny<string>());

            //------------Assert Results-------------------------
            validator.Verify(v => v.IsNullOrEmpty(It.IsAny<string>()));
            validator.Verify(v => v.IsLengthLessThanOrEqualTo(It.IsAny<string>(), It.Is<int>(i => i == 50)));
            validator.Verify(v => v.IsValidEmailAddress(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailAddressRule_IsValid")]
        public void EmailAddressRule_IsValid_IsNull_True()
        {
            EmailAddressRule_IsValid_Verify(expected: true, isNull: true, isLengthLessThanOrEqualTo50: false, isValidEmail: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailAddressRule_IsValid")]
        public void EmailAddressRule_IsValid_IsNotNullAndLengthGreaterThan50_False()
        {
            EmailAddressRule_IsValid_Verify(expected: false, isNull: false, isLengthLessThanOrEqualTo50: false, isValidEmail: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailAddressRule_IsValid")]
        public void EmailAddressRule_IsValid_IsNotNullAndLengthLessThanOrEqualTo50_True()
        {
            EmailAddressRule_IsValid_Verify(expected: true, isNull: false, isLengthLessThanOrEqualTo50: true, isValidEmail: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailAddressRule_IsValid")]
        public void EmailAddressRule_IsValid_IsNotNullAndNotValidEmailAddress_False()
        {
            EmailAddressRule_IsValid_Verify(expected: false, isNull: false, isLengthLessThanOrEqualTo50: true, isValidEmail: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("EmailAddressRule_IsValid")]
        public void EmailAddressRule_IsValid_IsNotNullAndValidEmailAddress_True()
        {
            EmailAddressRule_IsValid_Verify(expected: true, isNull: false, isLengthLessThanOrEqualTo50: true, isValidEmail: true);
        }

        static void EmailAddressRule_IsValid_Verify(bool expected, bool isNull, bool isLengthLessThanOrEqualTo50, bool isValidEmail)
        {
            //------------Setup for test--------------------------
            var validator = new Mock<IValidator>();
            validator.Setup(v => v.IsNullOrEmpty(It.IsAny<string>())).Returns(isNull);
            validator.Setup(v => v.IsLengthLessThanOrEqualTo(It.IsAny<string>(), It.IsAny<int>())).Returns(isLengthLessThanOrEqualTo50);
            validator.Setup(v => v.IsValidEmailAddress(It.IsAny<string>())).Returns(isValidEmail);

            var rule = new EmailAddressRule(validator.Object, "xx");

            //------------Execute Test---------------------------
            var actual = rule.IsValid(isNull ? null : It.IsAny<string>());

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
    }
}
