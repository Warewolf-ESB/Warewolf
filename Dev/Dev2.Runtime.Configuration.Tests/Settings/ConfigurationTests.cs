
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Diagnostics.CodeAnalysis;
using Dev2.Runtime.Configuration.Tests.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Tests.Settings
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ConfigurationTests
    {
        #region CTOR

        [TestMethod]
        public void ConstructorWithDefaultExpectedInitializesAllProperties()
        {
            var config = new Configuration.Settings.Configuration("localhost");
            ValidateInitializesAllProperties(config);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullXmlExpectedThrowsArgumentNullException()
        {
            // ReSharper disable UnusedVariable
            var config = new Configuration.Settings.Configuration((XElement)null);
            // ReSharper restore UnusedVariable
        } 
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullWebServerUriExpectedThrowsArgumentNullException()
        {
            // ReSharper disable UnusedVariable
            var config = new Configuration.Settings.Configuration((string)null);
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
        
        [TestMethod]
        public void ConstructorWithValidXmlNullWebServerUriArgumentExpectedThrowsArgumentNullException()
        {
            var config = new Configuration.Settings.Configuration(XmlResource.Fetch("Settings"));
            ValidateInitializesAllProperties(config);
        }

        #endregion

        #region ToXmlExpectedReturnsXml

        [TestMethod]
        public void ToXmlExpectedReturnsXml()
        {
            var config = new Configuration.Settings.Configuration("localhost");
            var result = config.ToXml();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(XElement));
        }

        [TestMethod]
        public void ToXmlExpectedInvokesToXmlForEachProperty()
        {
            var config = new Configuration.Settings.Configuration("localhost");
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

        [TestMethod]
        public void HasErrorReturnsFalse()
        {
            var config = new Configuration.Settings.Configuration(XmlResource.Fetch("Settings"));
            Assert.IsFalse(config.HasError);
        }

        [TestMethod]
        public void HasErrorReturnsTrueWhenLoggingError()
        {
            var config = new Configuration.Settings.Configuration(XmlResource.Fetch("Settings"));
            config.Logging.Error = "Error";
            Assert.IsTrue(config.HasError);
        }

        [TestMethod]
        public void HasErrorReturnsTrueWhenSecurityError()
        {
            var config = new Configuration.Settings.Configuration(XmlResource.Fetch("Settings"));
            config.Security.Error = "Error";
            Assert.IsTrue(config.HasError);
        }

        [TestMethod]
        public void HasErrorReturnsTrueWhenBackupError()
        {
            var config = new Configuration.Settings.Configuration(XmlResource.Fetch("Settings"));
            config.Backup.Error = "Error";
            Assert.IsTrue(config.HasError);
        }

        [TestMethod]
        public void LoggingSettingChangedExpectsHasChangesTrueWhenNotInitializating()
        {
            var config = new Configuration.Settings.Configuration(XmlResource.Fetch("Settings"));
            config.Logging.IsDataAndTimeLogged = true;
            Assert.IsTrue(config.HasChanges);
        }

        [TestMethod]
        public void LoggingSettingChangedExpectsHasChangesFalseWhenInitializating()
        {
            var config = new Configuration.Settings.Configuration(XmlResource.Fetch("Settings"));
            Assert.IsFalse(config.HasChanges);
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
