/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Xml.Linq;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Common.NetStandard20;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    // PBI 5656 - 2013.05.20 - TWR - Created
    [TestClass]
    [TestCategory("Runtime Hosting")]
    public class WebSourceTests
    {
        [TestMethod]
        [TestCategory(nameof(WebSource))]
        public void WebSource_Constructor_With_Default_Expected_Initializes_Properties()
        {
            var source = new WebSource();
            Assert.AreEqual(Guid.Empty, source.ResourceID);
            Assert.AreEqual("WebSource", source.ResourceType);
        }

        [TestMethod]
        [TestCategory(nameof(WebSource))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WebSource_Constructor_With_Null_Xml_Expected_Throws_ArgumentNullException()
        {
            _ = new WebSource(null);
        }

        [TestMethod]
        [TestCategory(nameof(WebSource))]
        public void WebSource_Constructor_With_Invalid_Xml_Expected_Does_Not_Throw_Exception_And_Initializes_Properties()
        {
            var xml = new XElement("root");
            var source = new WebSource(xml);
            Assert.AreNotEqual(Guid.Empty, source.ResourceID);
            Assert.IsTrue(source.IsUpgraded);
            Assert.AreEqual("WebSource", source.ResourceType);
        }

        [TestMethod]
        [TestCategory(nameof(WebSource))]
        public void WebSource_Constructor_With_Valid_Xml_Expected_Initializes_Properties()
        {
            var xml = XmlResource.Fetch("WebSource");

            var source = new WebSource(xml);
            Assert.AreEqual(Guid.Parse("f62e08d9-0359-4baa-8af3-08e0d812d6c6"), source.ResourceID);
            Assert.AreEqual("WebSource", source.ResourceType);
            Assert.AreEqual("http://www.webservicex.net/globalweather.asmx", source.Address);
            Assert.AreEqual("/GetCitiesByCountry?CountryName=South%20Africa", source.DefaultQuery);
            Assert.AreEqual("user1234", source.UserName);
            Assert.AreEqual("Password1234", source.Password);
        }

        [TestMethod]
        [TestCategory(nameof(WebSource))]
        public void WebSource_ToXml_Expected_Serializes_Properties()
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

        [TestMethod]
        [TestCategory(nameof(WebSource))]
        public void WebSource_Dispose_Client_Expected_Disposes_And_Nulls_Client()
        {
            var source = new WebSource { Client = new WebClientWrapper() };

            Assert.IsNotNull(source.Client);
            source.DisposeClient();
            Assert.IsNull(source.Client);
        }

        [TestMethod]
        [TestCategory(nameof(WebSource))]
        public void WebSource_Dispose_Expected_Disposes_And_Nulls_Client()
        {
            var source = new WebSource { Client = new WebClientWrapper() };

            Assert.IsNotNull(source.Client);
            source.Dispose();
            Assert.IsNull(source.Client);
        }
    }
}
