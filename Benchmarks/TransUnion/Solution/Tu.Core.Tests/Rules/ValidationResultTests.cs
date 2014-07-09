using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public class ValidationResultTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ValidationResult_Constructor")]
        public void ValidationResult_Constructor_Errors_NotNull()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var validationResult = new ValidationResult();

            //------------Assert Results-------------------------
            Assert.IsNotNull(validationResult.Errors);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ValidationResult_Errors")]
        [Description("Errors must be settable for serialization.")]
        public void ValidationResult_Errors_IsSettable()
        {
            //------------Setup for test--------------------------
            var validationResult = new ValidationResult();
            var errorsList = new List<string>(new[] { "hello" });

            //------------Execute Test---------------------------
            validationResult.Errors = errorsList;

            //------------Assert Results-------------------------
            Assert.AreSame(errorsList, validationResult.Errors);
        }
    }
}
