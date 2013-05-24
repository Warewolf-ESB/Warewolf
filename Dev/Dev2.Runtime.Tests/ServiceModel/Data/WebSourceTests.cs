using System;
using System.Xml.Linq;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices.Test.XML;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    // PBI 5656 - 2013.05.20 - TWR - Created
    [TestClass]
    public class WebSourceTests
    {
        #region CTOR

        [TestMethod]
        public void WebSourceContructorWithDefaultExpectedInitializesProperties()
        {
            var source = new WebSource();
            Assert.AreEqual(Guid.Empty, source.ResourceID);
            Assert.AreEqual(ResourceType.WebSource, source.ResourceType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSourceContructorWithNullXmlExpectedThrowsArgumentNullException()
        {
            var source = new WebSource(null);
        }

        [TestMethod]
        public void WebSourceContructorWithInvalidXmlExpectedDoesNotThrowExceptionAndInitializesProperties()
        {
            var xml = new XElement("root");
            var source = new WebSource(xml);
            Assert.AreNotEqual(Guid.Empty, source.ResourceID);
            Assert.IsTrue(source.IsUpgraded);
            Assert.AreEqual(ResourceType.WebSource, source.ResourceType);
        }

        [TestMethod]
        public void WebSourceContructorWithValidXmlExpectedInitializesProperties()
        {
            var xml = XmlResource.Fetch("WebSource");

            var source = new WebSource(xml);
            Assert.AreEqual(Guid.Parse("f62e08d9-0359-4baa-8af3-08e0d812d6c6"), source.ResourceID);
            Assert.AreEqual(ResourceType.WebSource, source.ResourceType);
            Assert.AreEqual("http://www.webservicex.net/globalweather.asmx", source.Address);
            Assert.AreEqual("/GetCitiesByCountry?CountryName=South%20Africa", source.DefaultQuery);
            Assert.AreEqual("user1234", source.UserName);
            Assert.AreEqual("Password1234", source.Password);
        }

        #endregion

        #region ToXml

        [TestMethod]
        public void WebSourceToXmlExpectedSerializesProperties()
        {
            var expected = new WebSource
            {
                Address = "http://www.webservicex.net/globalweather.asmx",
                DefaultQuery = "/GetCitiesByCountry?CountryName=US",
                AuthenticationType = AuthenticationType.User,
                UserName = "user123",
                Password = "mypassword",
            };

            var xml = expected.ToXml();

            var actual = new WebSource(xml);

            Assert.AreEqual(expected.ResourceType, actual.ResourceType);
            Assert.AreEqual(expected.Address, actual.Address);
            Assert.AreEqual(expected.DefaultQuery, actual.DefaultQuery);
            Assert.AreEqual(expected.UserName, actual.UserName);
            Assert.AreEqual(expected.Password, actual.Password);
            Assert.IsNull(actual.Response);
        }

        #endregion

    }
}
