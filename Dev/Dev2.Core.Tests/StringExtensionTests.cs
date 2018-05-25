﻿/*
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


namespace Dev2.Common.Test
{
    [TestClass]
    public class StringExtensionTests
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StringExtensions_IsJSON")]
        public void StringExtensionss_IsJSON_WhenValidJSON_ExpectTrue()
        {
            //------------Setup for test--------------------------
            const string fragment = "{}";

            //------------Execute Test---------------------------
            var result = fragment.IsJSON();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StringExtensions_IsJSON")]
        public void StringExtensionss_IsJSON_WhenValidXML_ExpectFalse()
        {
            //------------Setup for test--------------------------
            const string fragment = "<x></x>";

            //------------Execute Test---------------------------
            var result = fragment.IsJSON();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StringExtensions_IsJSON")]
        public void StringExtensionss_IsJSON_WhenValidText_ExpectFalse()
        {
            //------------Setup for test--------------------------
            const string fragment = "{ hello } { name }";

            //------------Execute Test---------------------------
            var result = fragment.IsJSON();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StringExtension_Text")]
        public void StringExtensions_Text_NotLatinCharacter_ShowMessageBox_TextMadeEmpty()
        {
            //------------Setup for test-------------------------
            const string Text = "أَبْجَدِي";
            //------------Execute Test---------------------------
            var checkHasUnicodeInText = Text.ContainsUnicodeCharacter();
            //------------Assert Results-------------------------
            Assert.IsTrue(checkHasUnicodeInText);
        }

        [TestMethod]
        public void StringExtensions_BlankText_IsNotDate() => Assert.IsFalse("".IsDate(), "Blank string is date");

        [TestMethod]
        public void StringExtensions_XmlFragment_IsXml() => Assert.IsTrue("<frag>ment</frag><ment>frag</ment>".IsXml(), "Xml Fragment is not xml");

        [TestMethod]
        public void StringExtensions_MalformedXml_IsNotXml() => Assert.IsFalse("<frag>ment</ment><frag>frag</ment>".IsXml(), "Malformed Xml is still xml");

        [TestMethod]
        public void StringExtensions_Html_IsNotXml() => Assert.IsFalse("<html><frag>frag</frag>ment</html>".IsXml(), "Malformed Xml is still xml");
    }
}
