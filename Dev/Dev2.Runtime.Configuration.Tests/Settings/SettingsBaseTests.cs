
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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Configuration.Tests.Settings
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SettingsBaseTests
    {
        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullSettingsNameExpectedThrowsArgumentNullException()
        {
            var settings = new SettingsBaseMock(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullDisplayNameExpectedThrowsArgumentNullException()
        {
            var settings = new SettingsBaseMock("xx", null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullWebServerUriExpectedThrowsArgumentNullException()
        {
            var settings = new SettingsBaseMock("xx", "xxx", null);
        }

        [TestMethod]
        public void ConstructorWithValidNameArgumentsExpectedSetsProperties()
        {
            var settings = new SettingsBaseMock("x", "y", "localhost");
            Assert.AreEqual("x", settings.SettingName);
            Assert.AreEqual("y", settings.DisplayName);
            Assert.AreEqual("localhost", settings.WebServerUri);
        }

        [TestMethod]
        [ExpectedException(typeof(NoNullAllowedException))]
        public void ConstructorWithInvalidXmlArgumentExpectedThrowsNoNullAllowedException()
        {
            var settings = new SettingsBaseMock(new XElement("x", new XElement("y"), new XElement("z")), "webserverUri");
        }

        [TestMethod]
        public void ConstructorWithValidXmlArgumentExpectedInitializesAllProperties()
        {
            var xml = new XElement("Settings", new XAttribute("DisplayName", "hello"));
            var settings = new SettingsBaseMock(xml, "localhost");

            Assert.AreEqual("hello", settings.DisplayName);
            Assert.AreEqual(xml.Name, settings.SettingName);
            Assert.AreEqual("localhost", settings.WebServerUri);
        }

        [TestMethod]
        [ExpectedException(typeof(NoNullAllowedException))]
        public void ConstructorWithValidXmlArgumentNullWebserverExpectedException()
        {
            var xml = new XElement("Settings", new XAttribute("DisplayName", "hello"));
            var settings = new SettingsBaseMock(xml, null);

            Assert.AreEqual("hello", settings.DisplayName);
            Assert.AreEqual(xml.Name, settings.SettingName);
        }

        #endregion

        #region ToXmlExpectedReturnsXml

        [TestMethod]
        public void ToXmlExpectedReturnsXml()
        {
            var settings = new SettingsBaseMock("x", "y", "localhost");
            var result = settings.ToXml();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(XElement));
        }

        [TestMethod]
        public void ToXmlExpectedSerializesEachProperty()
        {
            var settings = new SettingsBaseMock("x", "y", "localhost");

            var result = settings.ToXml();
            Assert.AreEqual(settings.SettingName, result.Name);
            Assert.AreEqual(settings.DisplayName, result.AttributeSafe("DisplayName"));
        }

        #endregion
    }
}
