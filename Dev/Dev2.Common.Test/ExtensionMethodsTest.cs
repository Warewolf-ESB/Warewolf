
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Dev2.Common.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Common.Tests
{
    [TestClass]
    public class ExtensionMethodsTest
    {

        // ReturnAsTagSet

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ToByteArray")]
        public void ExtensionMethods_ReturnAsTagSet_WhenValidString_ExpectTagSet()
        {
            //------------Setup for test--------------------------
            const string tag = "foo";
            //------------Execute Test---------------------------

            var tagSet = tag.ReturnAsTagSet();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, tagSet.Length);
            StringAssert.Contains(tagSet[0], "<foo>");
            StringAssert.Contains(tagSet[1], "</foo>");

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ToByteArray")]
        public void ExtensionMethods_ToByteArray_WhenValidString_ExpectValidBytes()
        {
            //------------Setup for test--------------------------
            const string input = "test message";
            var bytes = Encoding.UTF8.GetBytes(input);
            //------------Execute Test---------------------------
            using(Stream s = new MemoryStream(bytes))
            {
                var result = s.ToByteArray();

                //------------Assert Results-------------------------
                Assert.AreEqual(result.ToString(), bytes.ToString());
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ToBase64String")]
        public void ExtensionMethods_ToBase64String_WhenValidString_ExpectBase64Data()
        {
            //------------Setup for test--------------------------
            const string input = "test message";
            var bytes = Encoding.UTF8.GetBytes(input);
            //------------Execute Test---------------------------
            using(Stream s = new MemoryStream(bytes))
            {
                var result = s.ToBase64String();

                //------------Assert Results-------------------------
                Assert.AreEqual(result, "dGVzdCBtZXNzYWdl");
            }


        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ToObservableCollection")]
        public void ExtensionMethods_ToObservableCollection_WhenIEnumableContainsData_ExpectCollection()
        {
            //------------Setup for test--------------------------
            List<string> input = new List<string> { "foo", "bar" };

            //------------Execute Test---------------------------
            var result = input.ToObservableCollection();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("foo", result[0]);
            Assert.AreEqual("bar", result[1]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ToObservableCollection")]
        public void ExtensionMethods_ToObservableCollection_WhenIEnumableContainsNothing_ExpectEmptyCollection()
        {
            //------------Setup for test--------------------------
            List<string> input = new List<string>();

            //------------Execute Test---------------------------
            var result = input.ToObservableCollection();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ElementStringSafe")]
        public void ExtensionMethods_ElementStringSafe_WhenElementExist_ExpectElement()
        {
            //------------Setup for test--------------------------
            const string msg = "<x><y>y value</y></x>";
            var sb = new StringBuilder(msg);

            //------------Execute Test---------------------------

            var xe = sb.ToXElement();
            var result = xe.ElementStringSafe("y");

            //------------Assert Results-------------------------
            StringAssert.Contains(result, "<y>y value</y>");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ElementSafeStringBuilder")]
        public void ExtensionMethods_ElementStringSafe_WhenElementDoesNotExist_ExpectEmptyString()
        {
            //------------Setup for test--------------------------
            const string msg = "<x><y>y value</y></x>";
            var sb = new StringBuilder(msg);

            //------------Execute Test---------------------------

            var xe = sb.ToXElement();
            var result = xe.ElementStringSafe("q");

            //------------Assert Results-------------------------
            Assert.AreEqual(result, string.Empty);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ElementSafeStringBuilder")]
        public void ExtensionMethods_ElementSafeStringBuilder_WhenElementExist_ExpectElement()
        {
            //------------Setup for test--------------------------
            const string msg = "<x><y>y value</y></x>";
            var sb = new StringBuilder(msg);

            //------------Execute Test---------------------------

            var xe = sb.ToXElement();
            var result = xe.ElementSafeStringBuilder("y");

            //------------Assert Results-------------------------
            StringAssert.Contains(result.ToString(), "y");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ElementSafeStringBuilder")]
        public void ExtensionMethods_ElementSafeStringBuilder_WhenElementDoesNotExist_ExpectEmptyString()
        {
            //------------Setup for test--------------------------
            const string msg = "<x><y>y value</y></x>";
            var sb = new StringBuilder(msg);

            //------------Execute Test---------------------------

            var xe = sb.ToXElement();
            var result = xe.ElementSafeStringBuilder("q");

            //------------Assert Results-------------------------
            Assert.AreEqual(result.ToString(), string.Empty);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ElementSafeStringBuilder")]
        public void ExtensionMethods_ElementSafe_WhenElementExist_ExpectElement()
        {
            //------------Setup for test--------------------------
            const string msg = "<x><y>y value</y></x>";
            var sb = new StringBuilder(msg);

            //------------Execute Test---------------------------

            var xe = sb.ToXElement();
            var result = xe.ElementSafe("y");

            //------------Assert Results-------------------------
            StringAssert.Contains(result, "y");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ElementSafeStringBuilder")]
        public void ExtensionMethods_ElementSafe_WhenElementDoesNotExist_ExpectEmptyString()
        {
            //------------Setup for test--------------------------
            const string msg = "<x><y>y value</y></x>";
            var sb = new StringBuilder(msg);

            //------------Execute Test---------------------------

            var xe = sb.ToXElement();
            var result = xe.ElementSafe("q");

            //------------Assert Results-------------------------
            Assert.AreEqual(result, string.Empty);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_AttributeSafe")]
        public void ExtensionMethods_AttributeSafe_WhenAttributeExist_ExpectAttributeValue()
        {
            //------------Setup for test--------------------------
            const string msg = "<x foo=\"bar\">test message</x>";
            var sb = new StringBuilder(msg);

            //------------Execute Test---------------------------

            var xe = sb.ToXElement();
            var result = xe.AttributeSafe("foo");

            //------------Assert Results-------------------------
            StringAssert.Contains(result, "bar");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_AttributeSafe")]
        public void ExtensionMethods_AttributeSafe_WhenAttributeDoesNotExist_ExpectEmptyString()
        {
            //------------Setup for test--------------------------
            const string msg = "<x foo=\"bar\">test message</x>";
            var sb = new StringBuilder(msg);

            //------------Execute Test---------------------------

            var xe = sb.ToXElement();
            var result = xe.AttributeSafe("foo2");

            //------------Assert Results-------------------------
            Assert.AreEqual(result, string.Empty);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_EncodeForXmlDocument")]
        public void ExtensionMethods_EncodeForXmlDocument_WhenValidUTF8XmlDocument_ExpectStream()
        {
            //------------Setup for test--------------------------
            const string msg = "<x>test message</x>";
            var sb = new StringBuilder(msg);
            //------------Execute Test---------------------------

            using(var result = sb.EncodeForXmlDocument())
            {

                //------------Assert Results-------------------------
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Position);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_EncodeForXmlDocument")]
        public void ExtensionMethods_EncodeForXmlDocument_WhenValidUnicodeXmlDocument_ExpectStream()
        {
            //------------Setup for test--------------------------
            byte[] bytes = { (byte)'<', (byte)'x', (byte)'/', (byte)'>' };

            var msg = Encoding.Unicode.GetString(bytes);
            var sb = new StringBuilder(msg);
            //------------Execute Test---------------------------

            using(var result = sb.EncodeForXmlDocument())
            {

                //------------Assert Results-------------------------
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Position);
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_EncodeForXmlDocument")]
        [ExpectedException(typeof(XmlException))]
        public void ExtensionMethods_EncodeForXmlDocument_WhenInvalidXmlDocument_ExpectException()
        {
            //------------Setup for test--------------------------
            var sb = new StringBuilder("aa");
            //------------Execute Test---------------------------

            sb.EncodeForXmlDocument();
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_GetAllMessages")]
        public void ExtensionMethods_GetAllMessages_WhenUnrollingException_ExpectFullExceptionList()
        {
            //------------Setup for test--------------------------
            var innerException = new Exception("Inner Exception");
            var ex = new Exception("Test Error", innerException);
            const string expected = "Test Error\r\nInner Exception";

            //------------Execute Test---------------------------
            var result = ex.GetAllMessages();

            //------------Assert Results-------------------------
            StringAssert.Contains(result, expected);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_IsEqual")]
        public void ExtensionMethods_IsEqual_WhenComparingTwoStringBuilder_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var thisValue = new StringBuilder("<a></a>");
            var thatValue = new StringBuilder("<b></b>");

            //------------Execute Test---------------------------
            var result = thisValue.IsEqual(thatValue);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

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
            const string expected = "<x><y /></x>";
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
            const string val = "<x><y>1</y></x>";
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
        [ExpectedException(typeof(XmlException))]
        public void ExtensionMethods_ToStreamForXmlLoad_WhenLoadingXElement_ExpectException()
        {
            //------------Setup for test--------------------------
            const string val = "<x><y>1</y>.</</x>";
            StringBuilder value = new StringBuilder(val);

            //------------Execute Test---------------------------
            value.ToXElement();
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ToStreamForXmlLoad")]
        public void ExtensionMethods_ToStreamForXmlLoad_WhenLoadingXElement_ExpectValidXElement()
        {
            //------------Setup for test--------------------------
            const string val = "<x><y>1</y></x>";
            StringBuilder value = new StringBuilder(val);

            //------------Execute Test---------------------------
            var xe = value.ToXElement();

            string result = xe.ToString(SaveOptions.DisableFormatting);

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


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ExtractXmlAttributeFromUnsafeXml")]
        public void ExtensionMethods_ExtractXmlAttributeFromUnsafeXml_WhenAttributePresent_ExpectAttributeValue()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("<Action Name=\"bug11827service\" Type=\"InvokeWebService\" SourceID=\"fd54cecb-eebf-485a-aff7-e97835853c93\" SourceName=\"bug11827src\" SourceMethod=\"\" RequestUrl=\"\" RequestMethod=\"Get\" JsonPath=\"\">");

            //------------Execute Test---------------------------
            var result = value.ExtractXmlAttributeFromUnsafeXml("SourceName=\"");

            //------------Assert Results-------------------------
            StringAssert.Contains("bug11827src", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ExtractXmlAttributeFromUnsafeXml")]
        public void ExtensionMethods_ExtractXmlAttributeFromUnsafeXml_WhenAttributeNotPresent_ExpectEmptyString()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("<Action Name=\"bug11827service\" Type=\"InvokeWebService\" SourceID=\"fd54cecb-eebf-485a-aff7-e97835853c93\" SourceName=\"bug11827src\" SourceMethod=\"\" RequestUrl=\"\" RequestMethod=\"Get\" JsonPath=\"\">");

            //------------Execute Test---------------------------
            var result = value.ExtractXmlAttributeFromUnsafeXml("SourceNamePlanPath=\"");

            //------------Assert Results-------------------------
            StringAssert.Contains(string.Empty, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExtensionMethods_ExtractXmlAttributeFromUnsafeXml")]
        public void ExtensionMethods_ExtractXmlAttributeFromUnsafeXml_WhenAttributePresentAndEndTagInvalid_ExpectEmptyString()
        {
            //------------Setup for test--------------------------
            StringBuilder value = new StringBuilder("<Action Name=\"bug11827service\" Type=\"InvokeWebService\" SourceID=\"fd54cecb-eebf-485a-aff7-e97835853c93\" SourceName=\"bug11827src\" SourceMethod=\"\" RequestUrl=\"\" RequestMethod=\"Get\" JsonPath=\"\">");

            //------------Execute Test---------------------------
            var result = value.ExtractXmlAttributeFromUnsafeXml("SourceNamePlanPath=\"", "!!");

            //------------Assert Results-------------------------
            StringAssert.Contains(string.Empty, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExtensionMethods_IsNullOrEmpty")]
        public void ExtensionMethods_IsNullOrEmpty_NullStringBuilder_True()
        {
            //------------Setup for test--------------------------
            StringBuilder sb = null;
            //------------Execute Test---------------------------
            // ReSharper disable ExpressionIsAlwaysNull
            var isNullOrEmpty = sb.IsNullOrEmpty();
            // ReSharper restore ExpressionIsAlwaysNull
            //------------Assert Results-------------------------
            Assert.IsTrue(isNullOrEmpty);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExtensionMethods_IsNullOrEmpty")]
        public void ExtensionMethods_IsNullOrEmpty_EmptyStringBuilder_True()
        {
            //------------Setup for test--------------------------
            StringBuilder sb = new StringBuilder();
            //------------Execute Test---------------------------
            // ReSharper disable ExpressionIsAlwaysNull
            var isNullOrEmpty = sb.IsNullOrEmpty();
            // ReSharper restore ExpressionIsAlwaysNull
            //------------Assert Results-------------------------
            Assert.IsTrue(isNullOrEmpty);
        }   
    
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExtensionMethods_IsNullOrEmpty")]
        public void ExtensionMethods_IsNullOrEmpty_NullStringStringBuilder_True()
        {
            //------------Setup for test--------------------------
            StringBuilder sb = new StringBuilder(null);
            //------------Execute Test---------------------------
            // ReSharper disable ExpressionIsAlwaysNull
            var isNullOrEmpty = sb.IsNullOrEmpty();
            // ReSharper restore ExpressionIsAlwaysNull
            //------------Assert Results-------------------------
            Assert.IsTrue(isNullOrEmpty);
        } 

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExtensionMethods_IsNullOrEmpty")]
        public void ExtensionMethods_IsNullOrEmpty_NonEmptyStringBuilder_False()
        {
            //------------Setup for test--------------------------
            StringBuilder sb = new StringBuilder("Hello");
            //------------Execute Test---------------------------
            // ReSharper disable ExpressionIsAlwaysNull
            var isNullOrEmpty = sb.IsNullOrEmpty();
            // ReSharper restore ExpressionIsAlwaysNull
            //------------Assert Results-------------------------
            Assert.IsFalse(isNullOrEmpty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExtensionMethods_ToStringBuilder")]
        public void ExtensionMethods_ToStringBuilder_String_StringBuilder()
        {
            //------------Setup for test--------------------------
            const string myString = "This is my string";
            
            //------------Execute Test---------------------------
            StringBuilder stringBuilder = myString.ToStringBuilder();
            //------------Assert Results-------------------------
            StringAssert.Contains(stringBuilder.ToString(),myString);
        }
    }
}
