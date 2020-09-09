/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using ChinhDo.Transactions;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class ExtensionMethodsTest
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
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
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_ReadToEnd_ExpectReadStream()
        {
            //------------Setup for test--------------------------
            const string input = "test message";
            var bytes = Encoding.UTF8.GetBytes(input);
            //------------Execute Test---------------------------
            using (Stream s = new MemoryStream(bytes))
            {
                var result = s.ReadToEnd();

                using (var ss = new MemoryStream(bytes))
                {
                    using (var r = new StreamReader(ss))
                    {
                        //------------Assert Results-------------------------
                        Assert.AreEqual(result, r.ReadToEnd());
                    };
                };
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsBase64Stream_GivenNonBase64StringBytes_ExpectFalse()
        {
            //------------Setup for test--------------------------
            const string input = "test message";
            var bytes = Encoding.UTF8.GetBytes(input);
            //------------Execute Test---------------------------
            using (Stream s = new MemoryStream(bytes))
            {
                var result = s.IsBase64Stream(out byte[] outputBytes);
                
                //------------Assert Results-------------------------
                Assert.IsFalse(result);
                Assert.IsNull(outputBytes);
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsBase64Stream_GivenBase64StringBytes_ExpectTrue()
        {
            //------------Setup for test--------------------------
            const string input = "dGVzdCBtZXNzYWdl";
            var bytes = Encoding.UTF8.GetBytes(input);
            //------------Execute Test---------------------------
            using (Stream s = new MemoryStream(bytes))
            {
                var result = s.IsBase64Stream(out byte[] outputBytes);

                //------------Assert Results-------------------------
                Assert.IsTrue(result);
                Assert.IsNotNull(outputBytes);
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_ToBase64String_ExpectBase64String()
        {
            //------------Setup for test--------------------------
            const string input = "test message";
            var bytes = Encoding.UTF8.GetBytes(input);
            //------------Execute Test---------------------------
            var base64String = bytes.ToBase64String(Base64FormattingOptions.InsertLineBreaks);
            
            //------------Assert Results-------------------------
            Assert.AreEqual(Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks), base64String);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_Base64StringToByteArray_InCorrect_ExpectByteArray()
        {
            //------------Setup for test--------------------------
            const string input = "dGVzdCBtZXNzYWdl=3421";
            var bytes = Encoding.UTF8.GetBytes(input);
            //------------Execute Test---------------------------
            var base64String = bytes.ToBase64String(Base64FormattingOptions.InsertLineBreaks);

            //------------Assert Results-------------------------
            Assert.AreEqual(base64String.Base64StringToByteArray().ToString(), Convert.FromBase64String(base64String).ToString());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_Base64StringToByteArray_ExpectByteArray()
        {
            //------------Setup for test--------------------------
            const string input = "test message";
            var bytes = Encoding.UTF8.GetBytes(input);
            //------------Execute Test---------------------------
            var base64String = bytes.ToBase64String(Base64FormattingOptions.InsertLineBreaks);

            //------------Assert Results-------------------------
            Assert.AreEqual(base64String.Base64StringToByteArray().ToString(), Convert.FromBase64String(base64String).ToString());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsBase64Stream_ExpectTrue()
        {
            //------------Setup for test--------------------------
            const string input = "test message";
            var bytes = Encoding.UTF8.GetBytes(input);
            //------------Execute Test---------------------------
            var base64String = bytes.ToBase64String(Base64FormattingOptions.InsertLineBreaks);

            //------------Assert Results-------------------------
            Assert.AreEqual(base64String.Base64StringToByteArray().ToString(), Convert.FromBase64String(base64String).ToString());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsBase64String_ExpectFalseCurrentBytes()
        {
            //------------Setup for test--------------------------
            const string input = "test message";
            //------------Execute Test---------------------------
            var result = input.IsBase64String(out byte[] bytes);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            Assert.AreEqual(Encoding.ASCII.GetBytes(input).ToString(), bytes.ToString());
            using (var s = new StreamReader(new MemoryStream(bytes)))
            {
                Assert.AreEqual(input, s.ReadToEnd());
            };
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsBase64String_ExpectTrue()
        {
            //------------Setup for test--------------------------
            const string input = "dGVzdCBtZXNzYWdl";
            //------------Execute Test---------------------------
            var result = input.IsBase64String(out byte[] bytes);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            Assert.AreEqual(Encoding.ASCII.GetBytes(input).ToString(), bytes.ToString());
            using (var s = new StreamReader(new MemoryStream(bytes)))
            {
                Assert.AreEqual("test message", s.ReadToEnd());
            };

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_ToObservableCollection_WhenIEnumableContainsData_ExpectCollection()
        {
            //------------Setup for test--------------------------
            var input = new List<string> { "foo", "bar" };

            //------------Execute Test---------------------------
            var result = input.ToObservableCollection();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("foo", result[0]);
            Assert.AreEqual("bar", result[1]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_ToObservableCollection_WhenIEnumableContainsNothing_ExpectEmptyCollection()
        {
            //------------Setup for test--------------------------
            var input = new List<string>();

            //------------Execute Test---------------------------
            var result = input.ToObservableCollection();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, result.Count);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
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
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_WriteToFile_WhenSavingStringBuilderToFileWithExistingDataThatIsShorter_ExpectSavedFileWithNoMangle()
        {
            //------------Setup for test--------------------------
            var tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, "this is going to be some very long test just to ensure we can over write it");
            const string val = "<x><y>1</y></x>";
            var value = new StringBuilder(val);

            //------------Execute Test---------------------------
            IFileManager fm = new TxFileManager();
            value.WriteToFile(tmpFile, Encoding.UTF8,fm);

            //------------Assert Results-------------------------
            var result = File.ReadAllText(tmpFile);

            // clean up ;)
            File.Delete(tmpFile);

            Assert.AreEqual(val, result, "WriteToFile did not truncate");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_WriteToFile_RollbackIfError()
        {
            //------------Setup for test--------------------------
            var tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, "this is going to be some very long test just to ensure we can over write it");
            const string val = "<x><y>1</y></x>";
            var value = new StringBuilder(val);

            //------------Execute Test---------------------------
            IFileManager fm = new TxFileManager();
            value.WriteToFile(tmpFile, Encoding.UTF8, fm);

            //------------Assert Results-------------------------
            var result = File.ReadAllText(tmpFile);

            // clean up ;)
            File.Delete(tmpFile);

            Assert.AreEqual(val, result, "WriteToFile did not truncate");
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_WriteToFile_WhenSavingStringBuilder_ExpectSavedFile()
        {
            //------------Setup for test--------------------------
            var tmpFile = Path.GetTempFileName();
            const string val = "<x><y>1</y></x>";
            var value = new StringBuilder(val);

            //------------Execute Test---------------------------
            value.WriteToFile(tmpFile, Encoding.UTF8,new TxFileManager());

            //------------Assert Results-------------------------
            var result = File.ReadAllText(tmpFile);

            // clean up ;)
            File.Delete(tmpFile);

            Assert.AreEqual(val, result, "WriteToFile did not write");
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_CleanEncodingHeaderForXmlSave_WhenSavingXElement_ExpectEncodingHeaderRemoved()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("<x><y>1</y></x>");

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
        [TestCategory(nameof(ExtensionMethods))]
        [ExpectedException(typeof(XmlException))]
        public void ExtensionMethods_ToStreamForXmlLoad_WhenLoadingXElement_ExpectException()
        {
            //------------Setup for test--------------------------
            const string val = "<x><y>1</y>.</</x>";
            var value = new StringBuilder(val);

            //------------Execute Test---------------------------
            value.ToXElement();
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_ToStreamForXmlLoad_WhenLoadingXElement_ExpectValidXElement()
        {
            //------------Setup for test--------------------------
            const string val = "<x><y>1</y></x>";
            var value = new StringBuilder(val);

            //------------Execute Test---------------------------
            var xe = value.ToXElement();

            var result = xe.ToString(SaveOptions.DisableFormatting);

            //------------Assert Results-------------------------
            Assert.AreEqual(val, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IndexOf_WhenStringBuilderContainsValue_ExpectValidIndex()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.IndexOf("b", 0, true);

            //------------Assert Results-------------------------
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IndexOf_WhenStringBuilderDoesNotContainValue_ExpectNegativeIndex()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.IndexOf("q", 0, true);

            //------------Assert Results-------------------------
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IndexOf_WhenStringBuilderDoesNotContainValueAndCaseMatchOn_ExpectNegativeIndex()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.IndexOf("A", 0, false);

            //------------Assert Results-------------------------
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IndexOf_WhenStringBuilderDoesContainValueAndIndexIsAfter_ExpectNegativeIndex()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.IndexOf("a", 1, false);

            //------------Assert Results-------------------------
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_Substring_WhenStartAndEndInBound_ExpectString()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.Substring(0, 2);

            //------------Assert Results-------------------------
            Assert.AreEqual("a ", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_Substring_WhenStartAndEndInBoundAndNotStartingAtZero_ExpectString()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.Substring(2, 3);

            //------------Assert Results-------------------------
            Assert.AreEqual("b c", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ExtensionMethods_Substring_WhenStartAndEndOutOfBound_ExpectString()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            value.Substring(0, 20);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_Contains_WhenSubstringIsContained_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.Contains(" b");

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_Contains_WhenSubstringIsNotContained_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("a b c");

            //------------Execute Test---------------------------
            var result = value.Contains(" bq");

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_Unescape_WhenEscapedXmlString_ExpectUnescapedStringBuilder()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("&lt;x&gt;this &quot; is&apos; &amp; neat&lt;/x&gt;");

            //------------Execute Test---------------------------
            var result = value.Unescape();

            //------------Assert Results-------------------------
            Assert.AreEqual("<x>this \" is' & neat</x>", result.ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_Unescape_WhenNormalString_ExpectSameStringBuilder()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("twigs are snail broad swords");

            //------------Execute Test---------------------------
            var result = value.Unescape();

            //------------Assert Results-------------------------
            Assert.AreEqual("twigs are snail broad swords", result.ToString());
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_LastIndexOf_WhenNormalString_ExpectLastIndex()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("aaa bbb aaa ccc ddd aaa eee bbb");

            //------------Execute Test---------------------------
            var result = value.LastIndexOf("bbb", false);

            //------------Assert Results-------------------------
            Assert.AreEqual(28, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_LastIndexOf_WhenNormalStringWithCaseIgnore_ExpectLastIndex()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("aaa bbb aaa ccc ddd aaa eee BBB");

            //------------Execute Test---------------------------
            var result = value.LastIndexOf("bbb", true);

            //------------Assert Results-------------------------
            Assert.AreEqual(28, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_LastIndexOf_WhenNormalStringInMiddle_ExpectLastIndex()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("aaa bbb aaa ccc ddd aaa eee bbb");

            //------------Execute Test---------------------------
            var result = value.LastIndexOf("ccc", false);

            //------------Assert Results-------------------------
            Assert.AreEqual(12, result);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_ExtractXmlAttributeFromUnsafeXml_WhenAttributePresent_ExpectAttributeValue()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("<Action Name=\"bug11827service\" Type=\"InvokeWebService\" SourceID=\"fd54cecb-eebf-485a-aff7-e97835853c93\" SourceName=\"bug11827src\" SourceMethod=\"\" RequestUrl=\"\" RequestMethod=\"Get\" JsonPath=\"\">");

            //------------Execute Test---------------------------
            var result = value.ExtractXmlAttributeFromUnsafeXml("SourceName=\"");

            //------------Assert Results-------------------------
            StringAssert.Contains("bug11827src", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_ExtractXmlAttributeFromUnsafeXml_WhenAttributeNotPresent_ExpectEmptyString()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("<Action Name=\"bug11827service\" Type=\"InvokeWebService\" SourceID=\"fd54cecb-eebf-485a-aff7-e97835853c93\" SourceName=\"bug11827src\" SourceMethod=\"\" RequestUrl=\"\" RequestMethod=\"Get\" JsonPath=\"\">");

            //------------Execute Test---------------------------
            var result = value.ExtractXmlAttributeFromUnsafeXml("SourceNamePlanPath=\"");

            //------------Assert Results-------------------------
            StringAssert.Contains(string.Empty, result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_ExtractXmlAttributeFromUnsafeXml_WhenAttributePresentAndEndTagInvalid_ExpectEmptyString()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("<Action Name=\"bug11827service\" Type=\"InvokeWebService\" SourceID=\"fd54cecb-eebf-485a-aff7-e97835853c93\" SourceName=\"bug11827src\" SourceMethod=\"\" RequestUrl=\"\" RequestMethod=\"Get\" JsonPath=\"\">");

            //------------Execute Test---------------------------
            var result = value.ExtractXmlAttributeFromUnsafeXml("SourceNamePlanPath=\"", "!!");

            //------------Assert Results-------------------------
            StringAssert.Contains(string.Empty, result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsNullOrEmpty_NullStringBuilder_True()
        {
            //------------Setup for test--------------------------
            StringBuilder sb = null;
            //------------Execute Test---------------------------
            
            var isNullOrEmpty = sb.IsNullOrEmpty();
            
            //------------Assert Results-------------------------
            Assert.IsTrue(isNullOrEmpty);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsNullOrEmpty_EmptyStringBuilder_True()
        {
            //------------Setup for test--------------------------
            var sb = new StringBuilder();
            //------------Execute Test---------------------------

            var isNullOrEmpty = sb.IsNullOrEmpty();
            
            //------------Assert Results-------------------------
            Assert.IsTrue(isNullOrEmpty);
        }   
    
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsNullOrEmpty_NullStringStringBuilder_True()
        {
            //------------Setup for test--------------------------
            var sb = new StringBuilder(null);
            //------------Execute Test---------------------------

            var isNullOrEmpty = sb.IsNullOrEmpty();
            
            //------------Assert Results-------------------------
            Assert.IsTrue(isNullOrEmpty);
        } 

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsNullOrEmpty_NonEmptyStringBuilder_False()
        {
            //------------Setup for test--------------------------
            var sb = new StringBuilder("Hello");
            //------------Execute Test---------------------------

            var isNullOrEmpty = sb.IsNullOrEmpty();
            
            //------------Assert Results-------------------------
            Assert.IsFalse(isNullOrEmpty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_ToStringBuilder_String_StringBuilder()
        {
            //------------Setup for test--------------------------
            const string myString = "This is my string";
            
            //------------Execute Test---------------------------
            var stringBuilder = myString.ToStringBuilder();
            //------------Assert Results-------------------------
            StringAssert.Contains(stringBuilder.ToString(),myString);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_EscapeString_SBString_IsNull_ExpectNull()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var escapeString = Dev2.Common.Common.ExtensionMethods.EscapeString(null);
            //------------Assert Results-------------------------
            Assert.IsNull(escapeString);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_EscapeString_SBString_IsNotNull_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var value = "<Action Name=\"bug11827service\" Type=\"InvokeWebService\" SourceID=\"fd54cecb-eebf-485a-aff7-e97835853c93\" SourceName=\"bug11827src\" SourceMethod=\"\" RequestUrl=\"\" RequestMethod=\"Get\" JsonPath=\"\">";
            //------------Execute Test---------------------------
            var escapeString =  Dev2.Common.Common.ExtensionMethods.EscapeString(value);
            //------------Assert Results-------------------------
            Assert.AreEqual("&lt;Action Name=&quot;bug11827service&quot; Type=&quot;InvokeWebService&quot; SourceID=&quot;fd54cecb-eebf-485a-aff7-e97835853c93&quot; SourceName=&quot;bug11827src&quot; SourceMethod=&quot;&quot; RequestUrl=&quot;&quot; RequestMethod=&quot;Get&quot; JsonPath=&quot;&quot;&gt;", escapeString);
        }
        
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_UnescapeString_SBString_IsNull_ExpectNull()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var escapeString = Dev2.Common.Common.ExtensionMethods.UnescapeString(null);
            //------------Assert Results-------------------------
            Assert.IsNull(escapeString);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_UnescapeString_SBString_IsNotNull_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var value = "<Action Name=\"bug11827service\" Type=\"InvokeWebService\" SourceID=\"fd54cecb-eebf-485a-aff7-e97835853c93\" SourceName=\"bug11827src\" SourceMethod=\"\" RequestUrl=\"\" RequestMethod=\"Get\" JsonPath=\"\">";
            var value1 = "&lt;Action Name=&quot;bug11827service&quot; Type=&quot;InvokeWebService&quot; SourceID=&quot;fd54cecb-eebf-485a-aff7-e97835853c93&quot; SourceName=&quot;bug11827src&quot; SourceMethod=&quot;&quot; RequestUrl=&quot;&quot; RequestMethod=&quot;Get&quot; JsonPath=&quot;&quot;&gt;";
            //------------Execute Test---------------------------
            var unEscapeString = Dev2.Common.Common.ExtensionMethods.UnescapeString(value1);
            //------------Assert Results-------------------------
            Assert.AreEqual(value, unEscapeString);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_LastIndexOf_WithoutSameChar_ExpectFail()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("sdf");
            //------------Execute Test---------------------------
            var lastIndexOf = Dev2.Common.Common.ExtensionMethods.LastIndexOf(value, "rdf" , true);
            //------------Assert Results-------------------------
            Assert.AreEqual(-1, lastIndexOf);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_LastIndexOf_WithSameChar_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("sdf");
            //------------Execute Test---------------------------
            var lastIndexOf = Dev2.Common.Common.ExtensionMethods.LastIndexOf(value, "sdf", true);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, lastIndexOf);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_LastIndexOf_StartIndexSetter_WithSameChar_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("sdf");
            //------------Execute Test---------------------------
            var lastIndexOf = Dev2.Common.Common.ExtensionMethods.LastIndexOf(value, "sdf", 1, true);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, lastIndexOf);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_LastIndexOf_StartIndexSetter_IgnoreCase_WithSameChar_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("sdf");
            //------------Execute Test---------------------------
            var lastIndexOf = Dev2.Common.Common.ExtensionMethods.LastIndexOf(value, "sDf", 1, true);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, lastIndexOf);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_LastIndexOf_StartIndexSetter_IgnoreCase_False_WithSameChar_ExpectFail()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("sdf");
            //------------Execute Test---------------------------
            var lastIndexOf = Dev2.Common.Common.ExtensionMethods.LastIndexOf(value, "sDf", 1, false);
            //------------Assert Results-------------------------
            Assert.AreEqual(-1, lastIndexOf);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_LastIndexOf_StartIndexSetter_IgnoreCase_False_WithSameChar_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var value = new StringBuilder("aaa bbb aaa ccc ddd aaa eee bbb");
            //------------Execute Test---------------------------
            var lastIndexOf = Dev2.Common.Common.ExtensionMethods.LastIndexOf(value, "bbb", 8, false);
            //------------Assert Results-------------------------
            Assert.AreEqual(4, lastIndexOf);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsValidXml_InputIsNotXml_ExpectFail()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var isValidXml = Dev2.Common.Common.ExtensionMethods.IsValidXml("invalid xml");
            //------------Assert Results-------------------------
            Assert.IsFalse(isValidXml);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsValidXml_InputIsInValidXml_ExpectFail()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var isValidXml = Dev2.Common.Common.ExtensionMethods.IsValidXml("<invalid xml>");
            //------------Assert Results-------------------------
            Assert.IsFalse(isValidXml);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsValidXml_InputIsValidXml_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var str =
                    @"<?xml version=""1.0""?>  
                    <!-- comment at the root level -->  
                    <Root>  
                        <Child>Content</Child>  
                    </Root>";
            //------------Execute Test---------------------------
            var isValidXml = Dev2.Common.Common.ExtensionMethods.IsValidXml(str);
            //------------Assert Results-------------------------
            Assert.IsTrue(isValidXml);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsValidJson_InputIsValidJson_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var str = @"{
                            'books': [
                                {
                                  'title' : 'The Great Gatsby',
                                  'author' : 'F. Scott Fitzgerald'
                                },
                                {
                                  'title' : 'The Grapes of Wrath',
                                  'author' : 'John Steinbeck'
                                }
                            ]
                        }";
            //------------Execute Test---------------------------
            var isValidJson = Dev2.Common.Common.ExtensionMethods.IsValidJson(str);
            //------------Assert Results-------------------------
            Assert.IsTrue(isValidJson);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsValidJson_InputIsInValidJson_ExpectFail()
        {
            //------------Setup for test--------------------------
            var str = @"{
                            'books': [
                                {
                                  'title' : 'The Great Gatsby',
                                  'author' : 'F. Scott Fitzgerald'
                                },
                                {
                                  'title' : 'The Grapes of Wrath',
                                  'author' : 'John Steinbeck'
                                }
                            ]
                        ";
            //------------Execute Test---------------------------
            var isValidJson = Dev2.Common.Common.ExtensionMethods.IsValidJson(str);
            //------------Assert Results-------------------------
            Assert.IsFalse(isValidJson);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ExtensionMethods))]
        public void ExtensionMethods_IsValidJson_InputIsInValidJsonChar_ExpectFail()
        {
            //------------Setup for test--------------------------
            var str = @"{
                            'books': [
                                {
                                  'title : 'The Great Gatsby',
                                  'author' : 'F. Scott Fitzgerald'
                                },
                                {
                                  'title' : 'The Grapes of Wrath',
                                  'author' : 'John Steinbeck'
                                }
                            ]
                        }";
            //------------Execute Test---------------------------
            var isValidJson = Dev2.Common.Common.ExtensionMethods.IsValidJson(str);
            //------------Assert Results-------------------------
            Assert.IsFalse(isValidJson);
        }

        class Person
        {
            string Name { get; set; }
            string SurName { get; set; }
            int Age { get; set; }
             List<Person> Ps { get; set; }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DeepCopy_GivenPesron_ShouldCopy()
        {
            //---------------Set up test pack-------------------
            var p = new Person();
            //---------------Assert Precondition----------------
            var deepCopy = ObjectExtensions.DeepCopy(p);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            var referenceEquals = ReferenceEquals(p, deepCopy);
            Assert.IsFalse(referenceEquals);
        }
    }
}
