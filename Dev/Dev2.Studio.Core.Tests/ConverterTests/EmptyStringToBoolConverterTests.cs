using System.Diagnostics.CodeAnalysis;
using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EmptyStringToBoolConverterTests
    {
        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToBoolConverter")]
        [Description("When a string is empty, expect true when istrueisempty equals true")]
        public void EmptyStringToBoolConverter_UnitTest_TrueWhenEmptyStringEmpty_ExpectsTrue()
        {
            var converter = new EmptyStringToBoolConverter();
            converter.IsTrueWhenEmpty = true;
            var actual = (bool)converter.Convert(null, typeof(bool), null, null);
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToBoolConverter")]
        [Description("When a string is null, expect true when istrueisempty equals true")]
        public void EmptyStringToBoolConverter_UnitTest_TrueWhenEmptyStringNull_ExpectsTrue()
        {
            var converter = new EmptyStringToBoolConverter();
            converter.IsTrueWhenEmpty = true;
            var actual = (bool)converter.Convert(string.Empty, typeof(bool), null, null);
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToBoolConverter")]
        [Description("When a string is white space, expect true when istrueisempty equals true")]
        public void EmptyStringToBoolConverter_UnitTest_TrueWhenEmptyStringWhiteSpace_ExpectsTrue()
        {
            var converter = new EmptyStringToBoolConverter();
            converter.IsTrueWhenEmpty = true;
            var actual = (bool)converter.Convert(" ", typeof(bool), null, null);
            Assert.IsTrue(actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToBoolConverter")]
        [Description("When a string is not empty, null or whitespace, expect false when istrueisempty equals true")]
        public void EmptyStringToBoolConverter_UnitTest_TrueWhenEmptyStringValue_ExpectsFalse()
        {
            var converter = new EmptyStringToBoolConverter();
            converter.IsTrueWhenEmpty = true;
            var actual = (bool)converter.Convert("Anything", typeof(bool), null, null);
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToBoolConverter")]
        [Description("When a string is empty, expect dalse when istrueisempty equals falsee")]
        public void EmptyStringToBoolConverter_UnitTest_FalseWhenEmptyStringEmpty_ExpectsTrue()
        {
            var converter = new EmptyStringToBoolConverter();
            converter.IsTrueWhenEmpty = false;
            var actual = (bool)converter.Convert(null, typeof(bool), null, null);
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToBoolConverter")]
        [Description("When a string is null, expect false when istrueisempty equals false")]
        public void EmptyStringToBoolConverter_UnitTest_FalseWhenEmptyStringNull_ExpectsTrue()
        {
            var converter = new EmptyStringToBoolConverter();
            converter.IsTrueWhenEmpty = false;
            var actual = (bool)converter.Convert(string.Empty, typeof(bool), null, null);
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToBoolConverter")]
        [Description("When a string is white space, expect false when istrueisempty equals false")]
        public void EmptyStringToBoolConverter_UnitTest_FalseWhenEmptyStringWhiteSpace_ExpectsTrue()
        {
            var converter = new EmptyStringToBoolConverter();
            converter.IsTrueWhenEmpty = false;
            var actual = (bool)converter.Convert(" ", typeof(bool), null, null);
            Assert.IsFalse(actual);
        }

        [TestMethod]
        [Owner("Jurie Smit")]
        [TestCategory("EmptyStringToBoolConverter")]
        [Description("When a string is not empty, null or whitespace, expect true when istrueisempty equals false")]
        public void EmptyStringToBoolConverter_UnitTest_FalseWhenEmptyStringValue_ExpectsFalse()
        {
            var converter = new EmptyStringToBoolConverter();
            converter.IsTrueWhenEmpty = false;
            var actual = (bool)converter.Convert("Anything", typeof(bool), null, null);
            Assert.IsTrue(actual);
        }
    }
}
