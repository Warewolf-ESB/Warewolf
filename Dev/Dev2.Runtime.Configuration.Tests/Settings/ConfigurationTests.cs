using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.Runtime.Configuration.Tests.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Configuration.Tests.Settings
{
    [TestClass]
    public class ConfigurationTests
    {
        #region CTOR

        [TestMethod]
        public void ConstructorWithDefaultExpectedInitializesAllProperties()
        {
            var config = new Configuration.Settings.Configuration();
            ValidateInitializesAllProperties(config);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullXmlExpectedThrowsArgumentNullException()
        {
            // ReSharper disable UnusedVariable
            var config = new Configuration.Settings.Configuration(null);
            // ReSharper restore UnusedVariable
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorWithInvalidXmlVersionExpectedThrowsArgumentException()
        {
            // ReSharper disable UnusedVariable
            var config = new Configuration.Settings.Configuration(new XElement("x", new XElement("y"), new XElement("z")));
            // ReSharper restore UnusedVariable
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithInvalidXmlExpectedThrowsArgumentNullException()
        {
            // ReSharper disable UnusedVariable
            var config = new Configuration.Settings.Configuration(new XElement("x", new XAttribute("Version", "1.0"), new XElement("y"), new XElement("z")));
            // ReSharper restore UnusedVariable
        }

        [TestMethod]
        public void ConstructorWithValidXmlArgumentExpectedInitializesAllProperties()
        {
            var config = new Configuration.Settings.Configuration(XmlResource.Fetch("Settings"));
            ValidateInitializesAllProperties(config);
        }

        #endregion

        #region ToXmlExpectedReturnsXml

        [TestMethod]
        public void ToXmlExpectedReturnsXml()
        {
            var config = new Configuration.Settings.Configuration();
            var result = config.ToXml();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(XElement));
        }

        [TestMethod]
        public void ToXmlExpectedInvokesToXmlForEachProperty()
        {
            var config = new Configuration.Settings.Configuration();
            var result = config.ToXml();

            var properties = config.GetType().GetProperties();
            foreach(var property in properties)
            {
                var value = property.GetValue(config);

                Version version;
                Configuration.Settings.SettingsBase settings;

                if((settings = value as Configuration.Settings.SettingsBase) != null)
                {
                    var expected = settings.ToXml().ToString(SaveOptions.DisableFormatting);
                    // ReSharper disable PossibleNullReferenceException
                    var actual = result.Element(settings.SettingName).ToString(SaveOptions.DisableFormatting);
                    // ReSharper restore PossibleNullReferenceException
                    Assert.AreEqual(expected, actual);
                }
                else if((version = value as Version) != null)
                {
                    var actual = result.AttributeSafe(property.Name);
                    var expected = version.ToString(2);
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        #endregion

        //
        // Static helpers
        //

        #region ValidateInitializesAllProperties

        static void ValidateInitializesAllProperties(Configuration.Settings.Configuration config)
        {
            var properties = config.GetType().GetProperties();

            foreach(var value in properties.Select(property => property.GetValue(config)))
            {
                Assert.IsNotNull(value);
            }
        }

        #endregion

    }
}
