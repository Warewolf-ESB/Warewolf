using System;
using System.Data;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Configuration.Tests.Settings
{
    [TestClass]
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
            var settings = new SettingsBaseMock("xx", null);
        }

        [TestMethod]
        public void ConstructorWithValidNameArgumentsExpectedSetsProperties()
        {
            var settings = new SettingsBaseMock("x", "y");
            Assert.AreEqual("x", settings.SettingName);
            Assert.AreEqual("y", settings.DisplayName);
        }

        [TestMethod]
        [ExpectedException(typeof(NoNullAllowedException))]
        public void ConstructorWithInvalidXmlArgumentExpectedThrowsNoNullAllowedException()
        {
            var settings = new SettingsBaseMock(new XElement("x", new XElement("y"), new XElement("z")));
        }

        [TestMethod]
        public void ConstructorWithValidXmlArgumentExpectedInitializesAllProperties()
        {
            var xml = new XElement("Settings", new XAttribute("DisplayName", "hello"));
            var settings = new SettingsBaseMock(xml);

            Assert.AreEqual("hello", settings.DisplayName);
            Assert.AreEqual(xml.Name, settings.SettingName);
        }

        #endregion

        #region ToXmlExpectedReturnsXml

        [TestMethod]
        public void ToXmlExpectedReturnsXml()
        {
            var settings = new SettingsBaseMock("x", "y");
            var result = settings.ToXml();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(XElement));
        }

        [TestMethod]
        public void ToXmlExpectedSerializesEachProperty()
        {
            var settings = new SettingsBaseMock("x", "y");

            var result = settings.ToXml();
            Assert.AreEqual(settings.SettingName, result.Name);
            Assert.AreEqual(settings.DisplayName, result.AttributeSafe("DisplayName"));
        }

        #endregion
    }
}
