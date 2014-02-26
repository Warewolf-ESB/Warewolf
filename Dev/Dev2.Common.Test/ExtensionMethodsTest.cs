using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ExtensionMethodsTest
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_IsEqual")]
        public void ExtensionMethods_IsEqual_WhenComparingTwoStringBuilder_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var thisValue = new StringBuilder("<a></a>");
            var thatValue = new StringBuilder("<a></a>");

            //------------Execute Test---------------------------
            var result = thisValue.IsEqual(thatValue);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_IsEqual")]
        public void ExtensionMethods_IsEqual_WhenComparingTwoStringBuilderNotTheSame_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var thisValue = new StringBuilder("<a></a>");
            var thatValue = new StringBuilder("<a></a><x/>");

            //------------Execute Test---------------------------
            var result = thisValue.IsEqual(thatValue);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ToStringBuilder")]
        public void ExtensionMethods_ToStringBuilder_XElement_ExpectStringBuilder()
        {
            //------------Setup for test--------------------------
            var expected = "<x><y /></x>";
            var xe = XElement.Parse(expected);

            //------------Execute Test---------------------------
            var result = xe.ToStringBuilder();

            //------------Assert Results-------------------------
            StringAssert.Contains(result.ToString(), expected);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_WriteToFile")]
        public void ExtensionMethods_WriteToFile_WhenSavingStringBuilderToFileWithExistingDataThatIsShorter_ExpectSavedFileWithNoMangle()
        {
            //------------Setup for test--------------------------
            var tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, "this is going to be some very long test just to ensure we can over write it");
            const string val = "<x><y>1</y></x>";
            StringBuilder value = new StringBuilder(val);

            //------------Execute Test---------------------------
            value.WriteToFile(tmpFile, Encoding.UTF8);

            //------------Assert Results-------------------------
            var result = File.ReadAllText(tmpFile);

            // clean up ;)
            File.Delete(tmpFile);

            Assert.AreEqual(val, result, "WriteToFile did not truncate");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_WriteToFile")]
        public void ExtensionMethods_WriteToFile_WhenSavingStringBuilder_ExpectSavedFile()
        {
            //------------Setup for test--------------------------
            var tmpFile = Path.GetTempFileName();
            var val = "<x><y>1</y></x>";
            StringBuilder value = new StringBuilder(val);

            //------------Execute Test---------------------------
            value.WriteToFile(tmpFile, Encoding.UTF8);

            //------------Assert Results-------------------------
            var result = File.ReadAllText(tmpFile);

            // clean up ;)
            File.Delete(tmpFile);

            Assert.AreEqual(val, result, "WriteToFile did not write");
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_CleanEncodingHeaderForXmlSave")]
        public void ExtensionMethods_CleanEncodingHeaderForXmlSave_WhenSavingXElement_ExpectEncodingHeaderRemoved()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("<x><y>1</y></x>");

            //------------Execute Test---------------------------
            var xe = value.ToXElement();

            var sb = new StringBuilder();
            using(var sw = new StringWriter(sb))
            {
                xe.Save(sw, SaveOptions.DisableFormatting);
            }

            var res = sb.CleanEncodingHeaderForXmlSave();

            //------------Assert Results-------------------------
            var result = res.Contains("encoding=");

            Assert.IsFalse(result, "Encoding Header Not Removed");
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ToStreamForXmlLoad")]
        public void ExtensionMethods_ToStreamForXmlLoad_WhenLoadingXElement_ExpectValidXElement()
        {
            //------------Setup for test--------------------------
            var val = "<x><y>1</y></x>";
            StringBuilder value = new StringBuilder(val);

            //------------Execute Test---------------------------
            string result;
            var xe = value.ToXElement();

            result = xe.ToString(SaveOptions.DisableFormatting);

            //------------Assert Results-------------------------
            Assert.AreEqual(val, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_IndexOf")]
        public void ExtensionMethods_IndexOf_WhenStringBuilderContainsValue_ExpectValidIndex()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.IndexOf("b", 0, true);

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_IndexOf")]
        public void ExtensionMethods_IndexOf_WhenStringBuilderDoesNotContainValue_ExpectNegativeIndex()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.IndexOf("q", 0, true);

            //------------Assert Results-------------------------
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_IndexOf")]
        public void ExtensionMethods_IndexOf_WhenStringBuilderDoesNotContainValueAndCaseMatchOn_ExpectNegativeIndex()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.IndexOf("A", 0, false);

            //------------Assert Results-------------------------
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_IndexOf")]
        public void ExtensionMethods_IndexOf_WhenStringBuilderDoesContainValueAndIndexIsAfter_ExpectNegativeIndex()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.IndexOf("a", 1, false);

            //------------Assert Results-------------------------
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_Substring")]
        public void ExtensionMethods_Substring_WhenStartAndEndInBound_ExpectString()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.Substring(0, 2);

            //------------Assert Results-------------------------
            Assert.AreEqual("a ", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_Substring")]
        public void ExtensionMethods_Substring_WhenStartAndEndInBoundAndNotStartingAtZero_ExpectString()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.Substring(2, 3);

            //------------Assert Results-------------------------
            Assert.AreEqual("b c", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_Substring")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ExtensionMethods_Substring_WhenStartAndEndOutOfBound_ExpectString()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            value.Substring(0, 20);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_Contains")]
        public void ExtensionMethods_Contains_WhenSubstringIsContained_ExpectTrue()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.Contains(" b");

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_Contains")]
        public void ExtensionMethods_Contains_WhenSubstringIsNotContained_ExpectFalse()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.Contains(" bq");

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_Escape")]
        public void ExtensionMethods_Escape_WhenXmlString_ExpectEscapedStringBuilder()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("<x>this \" is' & neat</x>");

            //------------Execute Test---------------------------
            var result = value.Escape();

            //------------Assert Results-------------------------
            Assert.AreEqual("&lt;x&gt;this &quot; is&apos; &amp; neat&lt;/x&gt;", result.ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_Escape")]
        public void ExtensionMethods_Escape_WhenNonXmlString_ExpectSameStringBuilder()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("how cool");

            //------------Execute Test---------------------------
            var result = value.Escape();

            //------------Assert Results-------------------------
            Assert.AreEqual("how cool", result.ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_Escape")]
        public void ExtensionMethods_Unescape_WhenEscapedXmlString_ExpectUnescapedStringBuilder()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("&lt;x&gt;this &quot; is&apos; &amp; neat&lt;/x&gt;");

            //------------Execute Test---------------------------
            var result = value.Unescape();

            //------------Assert Results-------------------------
            Assert.AreEqual("<x>this \" is' & neat</x>", result.ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_Escape")]
        public void ExtensionMethods_Unescape_WhenNormalString_ExpectSameStringBuilder()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("twigs are snail broad swords");

            //------------Execute Test---------------------------
            var result = value.Unescape();

            //------------Assert Results-------------------------
            Assert.AreEqual("twigs are snail broad swords", result.ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_LastIndexOf")]
        public void ExtensionMethods_LastIndexOf_WhenNormalString_ExpectLastIndex()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("aaa bbb aaa ccc ddd aaa eee bbb");

            //------------Execute Test---------------------------
            var result = value.LastIndexOf("bbb", false);

            //------------Assert Results-------------------------
            Assert.AreEqual(28, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_LastIndexOf")]
        public void ExtensionMethods_LastIndexOf_WhenNormalStringWithCaseIgnore_ExpectLastIndex()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("aaa bbb aaa ccc ddd aaa eee BBB");

            //------------Execute Test---------------------------
            var result = value.LastIndexOf("bbb", true);

            //------------Assert Results-------------------------
            Assert.AreEqual(28, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_LastIndexOf")]
        public void ExtensionMethods_LastIndexOf_WhenNormalStringInMiddle_ExpectLastIndex()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("aaa bbb aaa ccc ddd aaa eee bbb");

            //------------Execute Test---------------------------
            var result = value.LastIndexOf("ccc", false);

            //------------Assert Results-------------------------
            Assert.AreEqual(12, result);
        }
    }
}
