/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.ExtMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace Dev2.Tests.ExtMethods
{
    [TestClass]

    public class StringExtensionTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsNumeric_Text_IsNull()
        {
            string result = null;
            Assert.IsFalse(result.IsNumeric(), "Null is not Numeric");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsNumeric_Text_IsEmpty()
        {
            string result = "";
            Assert.IsFalse(result.IsNumeric(), "Empty string is not Numeric");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsNumeric_StringHasASpace_False()
        {
            var result = "123 142".IsNumeric();
            Assert.IsFalse(result, "123 142 is not Numeric");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsNumeric_StringIsANumericWithASpecialChar_False()
        {
            var result = "123#142".IsNumeric();
            Assert.IsFalse(result, "123#142 is not Numeric");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsNumeric_StringIsNumeric_True()
        {
            var result = "123142".IsNumeric();
            Assert.IsTrue(result, "123142 IsNumeric");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsNumeric_StringIsNumericWithAPeriod_True()
        {
            var result = "123.142".IsNumeric();
            Assert.IsTrue(result, "123.142 IsNumeric");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        [DoNotParallelize]
        public void StringExtension_IsNumeric_StringIsNegativeNumericWithAPeriod_True()
        {
            //------------Execute Test---------------------------
            var result = "-123.142".IsNumeric(out decimal val);
            //------------Assert Results-------------------------
            Assert.IsTrue(result, "-123.142 IsNumeric");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsJSON_CurlyBrackets_ExpectTrue()
        {
            const string fragment = "{}";
            var result = fragment.IsJSON();
            Assert.IsTrue(result, "{} is valid JSON");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsJSON_SquareBrackets_ExpectTrue()
        {
            const string fragment = "[]";
            var result = fragment.IsJSON();
            Assert.IsTrue(result, "[] is valid JSON");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsJSON_WhenValidXML_ExpectFalse()
        {
            const string fragment = "<x></x>";
            var result = fragment.IsJSON();
            Assert.IsFalse(result, "<x></x> is not valid JSON");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsJSON_WhenValidText_ExpectFalse()
        {
            const string fragment = "{ hello } { name }";
            var result = fragment.IsJSON();
            Assert.IsFalse(result, "{ hello } { name } is not valid JSON");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_Text_NotLatinCharacter_ShowMessageBox_TextMadeEmpty()
        {
            const string Text = "أَبْجَدِي";
            var checkHasUnicodeInText = Text.ContainsUnicodeCharacter();
            Assert.IsTrue(checkHasUnicodeInText);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_Text_ContainsUnicodeCharacter_isNull()
        {
            const string Text = null;
            var checkHasUnicodeInText = Text.ContainsUnicodeCharacter();
            Assert.IsFalse(checkHasUnicodeInText, "ContainsUnicodeCharacter is Null");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_Text_ContainsUnicodeCharacter_isEmpty()
        {
            const string Text = "";
            var checkHasUnicodeInText = Text.ContainsUnicodeCharacter();
            Assert.IsFalse(checkHasUnicodeInText, "ContainsUnicodeCharacter is Empty");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_BlankText_IsEmpty() => Assert.IsFalse("".IsDate(), "Blank string is date");

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_BlankText_IsNull()
        {
            const string Text = null;
            Assert.IsFalse(Text.IsDate(), "Null string is not Date");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsDate_isValid()
        {
            DateTime today = DateTime.Today;
            Assert.IsTrue(today.ToString("dd/MM/yyyy").IsDate(), "Date is Valid");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_XmlFragment_IsXml_WithParameters()
        {
            bool isFragment = false;
            bool result = false;
            if (StringExtension.IsXml("<frag>ment</frag><ment>frag</ment>", out isFragment))
            {
                result = true;
            }
            Assert.AreEqual(true, isFragment);
            Assert.AreEqual(false, result);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_XmlFragment_IsXml_WithCData()
        {
            bool isFragment = false;
            bool result = false;
            if (StringExtension.IsXml("<![CDATA[An in-depth look at creating applications with XML, using <, >,]]>", out isFragment))
            {
                result = true;
            }
            Assert.AreEqual(false, isFragment);
            Assert.AreEqual(false, result);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_XmlFragment_IsXml_ValidXML()
        {
            Assert.IsTrue("<xml><frag>ment</frag><ment>frag</ment></xml>".IsXml(), "Is valid XML not a fragment");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_XmlFragment_IsXml_inValidXML()
        {
            Assert.IsFalse("xml><frag>ment</frag><ment>frag</ment></xml>".IsXml(), "Is valid XML not a fragment");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_XmlFragment_IsXml()
        {
            Assert.IsTrue("<frag>ment</frag><ment>frag</ment>".IsXml(), "Xml Fragment is not xml");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_XmlFragment_IsXml_IsNull()
        {
            const string XmlFragment = null;
            Assert.IsFalse(XmlFragment.IsXml(), "Xml Fragment is Null");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_XmlFragment_IsXml_IsEmpty()
        {
            const string XmlFragment = "";
            Assert.IsFalse(XmlFragment.IsXml(), "Xml Fragment is Empty");
        }


        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_MalformedXml_IsNotXml()
        {
            Assert.IsFalse("<frag>ment</ment><frag>frag</ment>".IsXml(), "Malformed Xml is still xml");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_Html_IsNotXml()
        {
            Assert.IsFalse("<html><frag>frag</frag>ment</html>".IsXml(), "Malformed Xml is still xml");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsAlphaNumeric_True()
        {
            Assert.IsTrue("321nbd".IsAlphaNumeric(), "IsAlphaNumeric is True");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsAlphaNumeric_False()
        {
            Assert.IsFalse("! @ # & ( ) – [ { } ] : ; ', ? / *".IsAlphaNumeric(), "IsAlphaNumeric is False");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsAlphaNumeric_isEmpty()
        {
            Assert.IsFalse("".IsAlphaNumeric(), "Text is Empty");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsAlphaNumeric_isNull()
        {
            string result = null;
            Assert.IsFalse(result.IsAlphaNumeric(), "Text is Null");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsAlpha_True()
        {
            Assert.IsTrue("321nbd".IsAlphaNumeric(), "IsAlpha is True");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsAlpha_False()
        {
            Assert.IsFalse("! @ # & ( ) – [ { } ] : ; ', ? / *".IsAlpha(), "IsAlphaNumeric is False");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsAlpha_isEmpty()
        {
            Assert.IsFalse("".IsAlpha(), "Text is Empty");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsAlpha_isNull()
        {
            string result = null;
            Assert.IsFalse(result.IsAlpha(), "Text is Null");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsEmail_isEmpty()
        {
            Assert.IsFalse("".IsEmail(), "IsEmail is Empty");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsEmail_isNull()
        {
            string result = null;
            Assert.IsFalse(result.IsEmail(), "IsEmail is Null");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsEmail_isValid()
        {
            string result = "email@email.com";
            Assert.IsTrue(result.IsEmail(), "IsEmail is Valid");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsEmail_isNotValid()
        {
            string result = "emailemail.com";
            Assert.IsFalse(result.IsEmail(), "IsEmail is not Valid");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsEmail_isNotValid2()
        {
            string result = "email@emailcom";
            Assert.IsFalse(result.IsEmail(), "IsEmail is not Valid");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsWholeNumber_True()
        {
            Assert.IsTrue("2002".IsWholeNumber(), "IsWholeNumber is True");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsWholeNumber_False()
        {
            Assert.IsFalse("00.00".IsWholeNumber(), "IsWholeNumber is False");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsWholeNumber_isEmpty()
        {
            Assert.IsFalse("".IsWholeNumber(), "IsWholeNumber is Empty");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsWholeNumber_isNull()
        {
            string result = null;
            Assert.IsFalse(result.IsWholeNumber(), "IsWholeNumber is Null");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsRealNumber_True()
        {
            int decimalPlacesToShowValue;
            Assert.IsTrue("2002".IsRealNumber(out decimalPlacesToShowValue), "IsRealNumber is True");
            Assert.AreEqual(2002, decimalPlacesToShowValue);
            Assert.IsTrue("-2002".IsRealNumber(out decimalPlacesToShowValue), "IsRealNumber is True");
            Assert.AreEqual(-2002, decimalPlacesToShowValue);

        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsRealNumber_False()
        {
            int decimalPlacesToShowValue;
            Assert.IsFalse("00.00".IsRealNumber(out decimalPlacesToShowValue), "IsRealNumber is False");
            Assert.AreEqual(0, decimalPlacesToShowValue);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsRealNumber_isEmpty()
        {
            int decimalPlacesToShowValue;
            Assert.IsFalse("".IsRealNumber(out decimalPlacesToShowValue), "IsRealNumber is Empty");
            Assert.AreEqual(0, decimalPlacesToShowValue);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsRealNumber_isNull()
        {
            string result = null;
            int decimalPlacesToShowValue;
            Assert.IsFalse(result.IsRealNumber(out decimalPlacesToShowValue), "IsRealNumber is Null");
            Assert.AreEqual(0, decimalPlacesToShowValue);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsBinary_isNull()
        {
            string result = null;
            Assert.IsFalse(result.IsBinary(), "IsBinary is Null");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsBinary_isValid()
        {
            var binary = Convert.ToString(5, 2);
            Assert.IsTrue(binary.IsBinary(), "IsBinary is Valid");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsBinary_notValid()
        {
            Assert.IsFalse("5555".IsBinary(), "IsBinary is not Valid");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsBase64_isNull()
        {
            string result = null;
            Assert.IsFalse(result.IsBase64(), "IsBase64 is Null");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsBase64_isValid()
        {
            byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes("String to encode to base64");
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);
            Assert.IsTrue(returnValue.IsBase64(), "IsBase64 is Valid");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsBase64_notValid()
        {
            byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes("String that is not base64");
            string returnValue = Convert.ToString(toEncodeAsBytes);
            Assert.IsFalse(returnValue.IsBase64(), "IsBase64 is not Valid");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsHex_isNull()
        {
            string result = null;
            Assert.IsFalse(result.IsHex(), "IsHex is Null");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsHex_isValid()
        {
            byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes("String to encode to hex");
            var hexString = BitConverter.ToString(toEncodeAsBytes).Replace("-", "");
            Assert.IsTrue(hexString.IsHex(), "IsHex is Valid");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsHex_notValid()
        {
            byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes("String that is not hex");
            string returnValue = Convert.ToString(toEncodeAsBytes);
            Assert.IsFalse(returnValue.IsHex(), "IsHex is not Valid");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_ReverseString()
        {
            string stringToSearchIn = "12345";
            stringToSearchIn = stringToSearchIn.ReverseString();
            Assert.AreEqual("54321", stringToSearchIn);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_RemoveWhiteSpace()
        {
            string stringToSearchIn = " 123  45 ";
            stringToSearchIn = stringToSearchIn.RemoveWhiteSpace();
            Assert.AreEqual("12345", stringToSearchIn);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_ExceptChars()
        {
            var stringToSearchIn = "\n32 16\t54\r";
            stringToSearchIn = stringToSearchIn.ExceptChars(new[] { ' ', '\t', '\n', '\r' });
            Assert.AreEqual("321654", stringToSearchIn);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_SpaceCaseInsenstiveComparisions()
        {
            var stringToSearchIn = "hElL o";
            Assert.IsTrue(stringToSearchIn.SpaceCaseInsenstiveComparision("hello"));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_SpaceCaseInsenstiveComparisions_False()
        {
            var stringToSearchIn = "hElL o";
            Assert.IsFalse(stringToSearchIn.SpaceCaseInsenstiveComparision("helloworld"));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_SpaceCaseInsenstiveComparisions_BothNull_False()
        {
            string stringToSearchIn = null;
            Assert.IsTrue(stringToSearchIn.SpaceCaseInsenstiveComparision(null));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_SpaceCaseInsenstiveComparisions_ExceptChars_True()
        {
            var stringToSearchIn = "\n32 16\t54\r";
            Assert.IsTrue(stringToSearchIn.SpaceCaseInsenstiveComparision("321654"));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_SpaceCaseInsenstiveComparisions_ExceptChars_False()
        {
            string stringa = null;
            Assert.IsFalse(stringa.SpaceCaseInsenstiveComparision("321654"));
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsAlphaNumericRegex_False()
        {
            Assert.IsFalse("&lt;&gt;dfdg 444".IsAlphaNumeric(), "IsAlphaNumericRegex is False");
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("StringExtension")]
        public void StringExtension_IsAlphaNumericRegex_True()
        {
            Assert.IsFalse(" 223 eeddd23".IsAlphaNumeric(), "IsAlphaNumericRegex is True");
        }
    }
}
