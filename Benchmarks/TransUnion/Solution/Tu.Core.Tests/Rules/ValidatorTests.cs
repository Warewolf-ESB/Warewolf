using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Extensions;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public class ValidatorTests
    {
        [TestMethod]
        [TestCategory("Validator_Constructor")]
        [Owner("Trevor Williams-Ros")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Validator_UnitTest_Constructor_WithNullArg_ThrowsArgumentNullException()
        {
            var validator = new Validator(null);
        }

        [TestMethod]
        [TestCategory("Validator_ValidationRules")]
        [Owner("Trevor Williams-Ros")]
        public void Validator_UnitTest_IsNullOrEmpty_Done()
        {
            var validator = new Validator(new Mock<IRegexUtilities>().Object);

            Assert.IsFalse(validator.IsNullOrEmpty("xx"));
            Assert.IsTrue(validator.IsNullOrEmpty(""));
            Assert.IsTrue(validator.IsNullOrEmpty(null));
        }

        [TestMethod]
        [TestCategory("Validator_ValidationRules")]
        [Owner("Trevor Williams-Ros")]
        public void Validator_UnitTest_IsLengthEqual_Done()
        {
            var validator = new Validator(new Mock<IRegexUtilities>().Object);

            Assert.IsTrue(validator.IsLengthEqual("xx", 2));
            Assert.IsTrue(validator.IsLengthEqual("", 0));
            Assert.IsTrue(validator.IsLengthEqual(null, 0));
        }

        [TestMethod]
        [TestCategory("Validator_ValidationRules")]
        [Owner("Trevor Williams-Ros")]
        public void Validator_UnitTest_IsLengthLessThanOrEqualTo_Done()
        {
            var validator = new Validator(new Mock<IRegexUtilities>().Object);

            Assert.IsFalse(validator.IsLengthLessThanOrEqualTo("xxxx", 3));
            Assert.IsTrue(validator.IsLengthLessThanOrEqualTo("xxx", 3));
            Assert.IsTrue(validator.IsLengthLessThanOrEqualTo("xx", 3));
            Assert.IsTrue(validator.IsLengthLessThanOrEqualTo("x", 3));
            Assert.IsTrue(validator.IsLengthLessThanOrEqualTo("", 3));
            Assert.IsTrue(validator.IsLengthLessThanOrEqualTo(null, 3));
        }

        [TestMethod]
        [TestCategory("Validator_ValidationRules")]
        [Owner("Trevor Williams-Ros")]
        public void Validator_UnitTest_IsLengthGreaterThan_Done()
        {
            var validator = new Validator(new Mock<IRegexUtilities>().Object);

            Assert.IsTrue(validator.IsLengthGreaterThan("xxx", 1));
            Assert.IsTrue(validator.IsLengthGreaterThan("xx", 1));
            Assert.IsFalse(validator.IsLengthGreaterThan("x", 1));

            Assert.IsTrue(validator.IsLengthGreaterThan("x", 0));
            Assert.IsFalse(validator.IsLengthGreaterThan("", 0));
            Assert.IsFalse(validator.IsLengthGreaterThan(null, 0));
        }

        [TestMethod]
        [TestCategory("Validator_ValidationRules")]
        [Owner("Trevor Williams-Ros")]
        public void Validator_UnitTest_IsNumeric_Done()
        {
            var validator = new Validator(new Mock<IRegexUtilities>().Object);

            Assert.IsFalse(validator.IsNumeric("xx"));
            Assert.IsTrue(validator.IsNumeric("35454"));
        }

        [TestMethod]
        [TestCategory("Validator_ValidationRules")]
        [Owner("Trevor Williams-Ros")]
        public void Validator_UnitTest_IsUpper_Done()
        {
            var validator = new Validator(new Mock<IRegexUtilities>().Object);

            Assert.IsFalse(validator.IsUpper("xX"));
            Assert.IsTrue(validator.IsUpper("YY"));
            Assert.IsTrue(validator.IsUpper("YY BB-CC'S"));
        }

        [TestMethod]
        [TestCategory("Validator_ValidationRules")]
        [Owner("Trevor Williams-Ros")]
        public void Validator_UnitTest_StartsWith_Done()
        {
            var validator = new Validator(new Mock<IRegexUtilities>().Object);

            Assert.IsFalse(validator.StartsWith("xxABC", "ABC"));
            Assert.IsTrue(validator.StartsWith("xxABC", "xxA"));
            Assert.IsTrue(validator.StartsWith("xxABC", ""));
            Assert.IsFalse(validator.StartsWith("xxABC", null));
            Assert.IsFalse(validator.StartsWith("", "x"));

            Assert.IsFalse(validator.StartsWith(null, "x"));
            Assert.IsTrue(validator.StartsWith(null, ""));
            Assert.IsFalse(validator.StartsWith(null, null));

        }

        [TestMethod]
        [TestCategory("Validator_ValidationRules")]
        [Owner("Trevor Williams-Ros")]
        public void Validator_UnitTest_IsCountOfLessThanOrEqualTo_Done()
        {
            var validator = new Validator(new Mock<IRegexUtilities>().Object);

            Assert.IsFalse(validator.IsCountOfLessThanOrEqualTo("xxCasbcbcd", 'c', 1));
            Assert.IsTrue(validator.IsCountOfLessThanOrEqualTo("xxCasbcbcd", 'c', 2));
            Assert.IsTrue(validator.IsCountOfLessThanOrEqualTo("xxCasbCbCd", 'c', 0));
            Assert.IsTrue(validator.IsCountOfLessThanOrEqualTo("", 'c', 0));
            Assert.IsTrue(validator.IsCountOfLessThanOrEqualTo(null, 'c', 0));
        }

        [TestMethod]
        [TestCategory("Validator_ValidationRules")]
        [Owner("Trevor Williams-Ros")]
        public void Validator_UnitTest_ContainsSpecialChars_Done()
        {
            var validator = new Validator(new Mock<IRegexUtilities>().Object);

            const string InvalidChars = "`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?";

            foreach(var s in InvalidChars.Select(invalidChar => new string(new[] { 'x', invalidChar })))
            {
                Assert.IsTrue(validator.ContainsSpecialChars(s));
            }

            Assert.IsFalse(validator.ContainsSpecialChars("xx"));
        }


        [TestMethod]
        [TestCategory("Validator_ValidationRules")]
        [Owner("Trevor Williams-Ros")]
        public void Validator_UnitTest_IsValidEmailAddress_Done()
        {
            var regexUtilities = new Mock<IRegexUtilities>();
            regexUtilities.Setup(r => r.IsValidEmail(It.IsAny<string>())).Verifiable();

            var validator = new Validator(regexUtilities.Object);
            validator.IsValidEmailAddress(It.IsAny<string>());

            regexUtilities.Verify(r => r.IsValidEmail(It.IsAny<string>()));
        }
    }
}
