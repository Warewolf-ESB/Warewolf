/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    // PBI 953 - 2013.05.16 - TWR - Created
    [TestClass]
    public class EmailSourceTests
    {
        #region CTOR

        [TestMethod]
        public void EmailSourceContructorWithDefaultExpectedInitializesProperties()
        {
            var source = new EmailSource();
            Assert.AreEqual(Guid.Empty, source.ResourceID);
            Assert.AreEqual("EmailSource", source.ResourceType);
            Assert.AreEqual(EmailSource.DefaultTimeout, source.Timeout);
            Assert.AreEqual(EmailSource.DefaultPort, source.Port);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmailSourceContructorWithNullXmlExpectedThrowsArgumentNullException()
        {
            var source = new EmailSource(null);
        }

        [TestMethod]
        public void EmailSourceContructorWithInvalidXmlExpectedDoesNotThrowExceptionAndInitializesProperties()
        {
            var xml = new XElement("root");
            var source = new EmailSource(xml);
            Assert.AreNotEqual(Guid.Empty, source.ResourceID);
            Assert.IsTrue(source.IsUpgraded);
            Assert.AreEqual("EmailSource", source.ResourceType);
            Assert.AreEqual(EmailSource.DefaultTimeout, source.Timeout);
            Assert.AreEqual(EmailSource.DefaultPort, source.Port);
        }


        [TestMethod]
        public void EmailSourceContructorWithValidXmlExpectedInitializesProperties()
        {
            var xml = XmlResource.Fetch("EmailSource");

            var source = new EmailSource(xml);
            Assert.AreEqual(Guid.Parse("bf810e43-3633-4638-9d0a-56473ef54151"), source.ResourceID);
            Assert.AreEqual("EmailSource", source.ResourceType);
            Assert.AreEqual("smtp.gmail.com", source.Host);
            Assert.AreEqual(465, source.Port);
            Assert.AreEqual(true, source.EnableSsl);
            Assert.AreEqual(30000, source.Timeout);
            Assert.AreEqual("user@gmail.com", source.UserName);
            Assert.AreEqual("1234", source.Password);
        }

        [TestMethod]
        public void EmailSourceContructorWithCorruptXmlExpectedInitializesProperties()
        {
            var xml = XmlResource.Fetch("EmailSourceCorrupt");

            var source = new EmailSource(xml);
            Assert.AreEqual(Guid.Parse("bf810e43-3633-4638-9d0a-56473ef54151"), source.ResourceID);
            Assert.AreEqual("EmailSource", source.ResourceType);
            Assert.AreEqual("smtp.gmail.com", source.Host);
            Assert.AreEqual(EmailSource.DefaultPort, source.Port);
            Assert.AreEqual(false, source.EnableSsl);
            Assert.AreEqual(EmailSource.DefaultTimeout, source.Timeout);
            Assert.AreEqual("user@gmail.com", source.UserName);
            Assert.AreEqual("1234", source.Password);
        }

        #endregion

        #region ToXml

        [TestMethod]
        public void EmailSourceToXmlExpectedSerializesProperties()
        {
            var expected = new EmailSource
            {
                Host = "smtp.mydomain.com",
                Port = 25,
                EnableSsl = false,
                UserName = "user@mydomain.com",
                Password = "mypassword",
                Timeout = 1000,
                TestFromAddress = "user@mydomain.com",
                TestToAddress = "user2@mydomain2.com"
            };

            var xml = expected.ToXml();

            var actual = new EmailSource(xml);

            Assert.AreEqual(expected.ResourceType, actual.ResourceType);
            Assert.AreEqual(expected.Host, actual.Host);
            Assert.AreEqual(expected.Port, actual.Port);
            Assert.AreEqual(expected.EnableSsl, actual.EnableSsl);
            Assert.AreEqual(expected.UserName, actual.UserName);
            Assert.AreEqual(expected.Password, actual.Password);
            Assert.AreEqual(expected.Timeout, actual.Timeout);
            Assert.IsNull(actual.TestFromAddress);
            Assert.IsNull(actual.TestToAddress);
        }

        #endregion
    }
}
