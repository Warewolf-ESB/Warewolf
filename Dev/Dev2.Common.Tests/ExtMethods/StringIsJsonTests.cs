/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Tests.ExtMethods
{
    [TestClass]

    public class StringIsJsonTests
    {
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
    }
}
