
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Configuration.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ExtensionMethodsTests
    {
        const string AttributeName = "say";
        const string AttributeValue = "hello";
        const string ElementName = "talk";
        const string ElementValue = "world";

        static XElement CreateXml()
        {
            return new XElement("Test", new XAttribute(AttributeName, AttributeValue), new XElement(ElementName, ElementValue));
        }

        #region AttributeSafe

        [TestMethod]
        public void AttributeSafeWithInvalidArgumentsReturnsEmptyString()
        {
            var result = ExtensionMethods.AttributeSafe(null, null);
            Assert.AreEqual(string.Empty, result);

            var elem = CreateXml();
            result = elem.AttributeSafe(null);
            Assert.AreEqual(string.Empty, result);
            result = elem.AttributeSafe(string.Empty);
            Assert.AreEqual(string.Empty, result);
            result = elem.AttributeSafe("y");
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void AttributeSafeWithValidArgumentsReturnsAttributeValue()
        {
            var elem = CreateXml();
            var result = elem.AttributeSafe(AttributeName);
            Assert.AreEqual(AttributeValue, result);
        }

        #endregion

        #region ElementSafe

        [TestMethod]
        public void ElementSafeWithInvalidArgumentsReturnsEmptyString()
        {
            var result = ExtensionMethods.ElementSafe(null, null);
            Assert.AreEqual(string.Empty, result);

            var elem = CreateXml();
            result = elem.ElementSafe(null);
            Assert.AreEqual(string.Empty, result);
            result = elem.ElementSafe(string.Empty);
            Assert.AreEqual(string.Empty, result);
            result = elem.ElementSafe("y");
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void ElementSafeWithValidArgumentsReturnsElementValue()
        {
            var elem = CreateXml();
            var result = elem.ElementSafe(ElementName);
            Assert.AreEqual(ElementValue, result);
        }

        #endregion
    }
}
