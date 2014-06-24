using System.Diagnostics.CodeAnalysis;
using Dev2.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Communication
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DesignValidationMemoTests
    {
        [TestMethod]
        [Description("Constructor must initialize Errors list and set IsValid to true.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void DesignValidationMemoConstructor_UnitTest_Intialization_ErrorsNotNullAndIsValidTrue()
        // ReSharper restore InconsistentNaming
        {
            var memo = new DesignValidationMemo();
            Assert.IsNotNull(memo.Errors);
            Assert.IsTrue(memo.IsValid);
        }
    }
}
