using Dev2.Common.ExtMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;

namespace Dev2.Common.Test
{
    [TestClass][ExcludeFromCodeCoverage]
    public class StringExtensionTests
    {
        [TestMethod]
        [TestCategory("StringExtensionUnitTest")]
        [Description("Test for 'ValidateCategoryName' string extention method: A valid resource category name ('new_category.var') is passed to it and true is expected to be returned back")]
        [Owner("Ashley Lewis")]
// ReSharper disable InconsistentNaming
        public void StringExtension_StringExtensionUnitTest_ValidateCategoryName_TrueIsReturned()
// ReSharper restore InconsistentNaming
        {
            Assert.IsTrue("new_category.var".IsValidCategoryName(), "Valid category name was rejected by the validation function");
        }

        [TestMethod]
        [TestCategory("StringExtensionUnitTest")]
        [Description("Test for 'ValidateCategoryName' string extention method: An invalid resource category name ('new/<category>') is passed to it and true is expected to be returned back")]
        [Owner("Ashley Lewis")]
// ReSharper disable InconsistentNaming
        public void StringExtension_StringExtensionUnitTest_ValidateCategoryName_FalseIsReturned()
// ReSharper restore InconsistentNaming
        {
            Assert.IsFalse("new/<category>".IsValidCategoryName(), "Invalid category name passed validation");
        }
    }
}
