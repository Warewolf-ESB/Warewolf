/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Dev2.Runtime.Security;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class HostSecurityProviderTest
    {
        static XElement TestXml;
        static XElement TestXmlServerSigned;
        static XElement TestXmlSystemSigned;
        static XElement TestXmlInternallySigned;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            TestXml = XmlResource.Fetch("HostSecurityProvider");
            TestXmlServerSigned = XmlResource.Fetch("HostSecurityProviderServerSigned");
            TestXmlSystemSigned = XmlResource.Fetch("HostSecurityProviderSystemSigned");
            TestXmlInternallySigned = XmlResource.Fetch("InternallySigned");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecureConfig))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HostSecurityProvider_ConstructorWithNull_Expected_ThrowsArgumentNullException()
        {
            var provider = new HostSecurityProviderImpl(null);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        public void HostSecurityProvider_ConstructorWithDefaultConfig_Expected_ReturnsDefaultValues()
        {
            var config = CreateConfig();
            var provider = new HostSecurityProviderImpl(config.Object);
            Assert.AreEqual(HostSecureConfigTests.DefaultServerID, provider.ServerID);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HostSecurityProvider_SignXmlWithNull_Expected_ThrowsArgumentNullException()
        {
            var config = CreateConfig();
            var provider = new HostSecurityProviderImpl(config.Object);
            provider.SignXml(null);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        [ExpectedException(typeof(XmlException))]
        public void HostSecurityProvider_SignXmlWithInvalidXml_Expected_ThrowsXmlException()
        {
            var config = CreateConfig();
            var provider = new HostSecurityProviderImpl(config.Object);
            provider.SignXml(new StringBuilder("xxx"));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        public void HostSecurityProvider_SignXmlWithSignedXml_Expected_OneSignatureAdded()
        {
            var config = CreateConfig();
            var provider = new HostSecurityProviderImpl(config.Object);
            var signedXml = provider.SignXml(new StringBuilder(TestXmlServerSigned.ToString()));

            var xml = XElement.Parse(signedXml.ToString());

            XNamespace xmlns = "http://www.w3.org/2000/09/xmldsig#";
            var signatures = xml.Elements(xmlns + "Signature");
            Assert.AreEqual(0, signatures.Count());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        public void HostSecurityProvider_SignXmlWithServerKey_Expected_SignatureAdded()
        {
            var config = CreateConfig();
            var provider = new HostSecurityProviderImpl(config.Object);
            var signedXml = provider.SignXml(new StringBuilder(TestXml.ToString()));

            var xml = XElement.Parse(signedXml.ToString());

            XNamespace xmlns = "http://www.w3.org/2000/09/xmldsig#";
            var signature = xml.Element(xmlns + "Signature");
            Assert.IsNull(signature);
            var resultXml = signedXml.ToString().Replace("\r\n", "");
            var expected = TestXmlServerSigned.ToString().Replace("\r\n", "");
            Assert.AreNotEqual(expected, resultXml);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        public void HostSecurityProvider_SignXmlWithSystemKey_Expected_SignatureAdded()
        {
            var config = CreateConfig(true);
            var provider = new HostSecurityProviderImpl(config.Object);
            var signedXml = provider.SignXml(new StringBuilder(TestXml.ToString()));

            var xml = XElement.Parse(signedXml.ToString());

            XNamespace xmlns = "http://www.w3.org/2000/09/xmldsig#";
            var signature = xml.Element(xmlns + "Signature");
            Assert.IsNull(signature);

            var expected = TestXmlSystemSigned.ToString().Replace("\r\n", "");
            var resultXml = signedXml.ToString().Replace("\r\n", "");

            Assert.AreNotEqual(expected, resultXml);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        public void HostSecurityProvider_SignXmlWithoutServerID_Expected_ServerIDAdded()
        {
            var config = CreateConfig();
            var provider = new HostSecurityProviderImpl(config.Object);
            var testXml = XElement.Parse(TestXml.ToString());
            testXml.RemoveAttributes();
            var signedXml = provider.SignXml(new StringBuilder(testXml.ToString()));

            var xml = XElement.Parse(signedXml.ToString());
            var serverID = xml.Attribute("ServerID");
            Assert.IsNotNull(serverID);
            Assert.AreEqual(config.Object.ServerID.ToString(), serverID.Value);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        public void HostSecurityProvider_SignXmlWithServerID_Expected_ServerIDOverwritten()
        {
            var config = CreateConfig(
                Guid.NewGuid(),
                HostSecureConfig.CreateKey(HostSecureConfigTests.DefaultServerKey),
                HostSecureConfig.CreateKey(HostSecureConfigTests.DefaultSystemKeyPublic),
                HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultCustomerId),
                HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultSubscriptionId),
                HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultPlanId),
                HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultConfigSitename),
                HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultConfigKey));

            var provider = new HostSecurityProviderImpl(config.Object);

            var originalID = Guid.Parse(TestXml.Attribute("ServerID")?.Value);
            var signedXml = provider.SignXml(new StringBuilder(TestXml.ToString()));

            var xml = XElement.Parse(signedXml.ToString());
            var serverID = xml.Attribute("ServerID");
            Assert.IsNotNull(serverID);
            Assert.AreNotEqual(originalID, serverID.Value);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void HostSecurityProvider_VerifyXmlWithNull_Expected_ThrowsArgumentNullException()
        {
            var config = CreateConfig();
            var provider = new HostSecurityProviderImpl(config.Object);
            provider.VerifyXml(null);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        public void HostSecurityProvider_VerifyXmlWithInvalidServerID_Expected_ReturnsFalse()
        {
            var config = CreateConfig();
            var provider = new HostSecurityProviderImpl(config.Object);
            var testXml = XElement.Parse(TestXmlServerSigned.ToString());
            testXml.SetAttributeValue("ServerID", Guid.NewGuid());
            var verified = provider.VerifyXml(new StringBuilder(testXml.ToString()));
            Assert.IsTrue(verified);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        public void HostSecurityProvider_VerifyXmlWithInvalidKeys_Expected_ReturnsFalse()
        {
            var config = CreateConfig(new RSACryptoServiceProvider(), new RSACryptoServiceProvider(), "","","", "", "");
            var provider = new HostSecurityProviderImpl(config.Object);

            var verified = provider.VerifyXml(new StringBuilder(TestXmlServerSigned.ToString()));
            Assert.IsTrue(verified);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        public void HostSecurityProvider_VerifyXmlWithValidKeys_Expected_ReturnsTrue()
        {
            var config = CreateConfig();
            var provider = new HostSecurityProviderImpl(config.Object);

            var verified = provider.VerifyXml(new StringBuilder(TestXmlServerSigned.ToString()));
            Assert.IsTrue(verified);

            verified = provider.VerifyXml(new StringBuilder(TestXmlSystemSigned.ToString()));
            Assert.IsTrue(verified);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecurityProvider))]
        public void HostSecurityProvider_VerifyXmlWhichIsInternallySignedWithValidKeys_Expected_ReturnsTrue()
        {
            var config = CreateConfig();
            var provider = new HostSecurityProviderImpl(config.Object);

            var verified = provider.VerifyXml(new StringBuilder(TestXmlInternallySigned.ToString()));
            Assert.IsTrue(verified);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(HostSecureConfig))]
        public void HostSecurityProvider_VerifyXmlWithService_Expected_ReturnsTrue()
        {
            var xml = XmlResource.Fetch("Calculate_RecordSet_Subtract").ToString();
            var verified = HostSecurityProvider.Instance.VerifyXml(new StringBuilder(xml));
            Assert.IsTrue(verified);
        }

        static Mock<ISecureConfig> CreateConfig(bool useSystemPrivateKeyAsServerKey = false)
        {
            if (useSystemPrivateKeyAsServerKey)
            {
                return CreateConfig(
                    HostSecureConfig.CreateKey(HostSecureConfigTests.DefaultSystemKeyPrivate),
                    HostSecureConfig.CreateKey(HostSecureConfigTests.DefaultSystemKeyPublic),
                    HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultCustomerId),
                    HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultSubscriptionId),
                    HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultPlanId),
                    HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultConfigSitename),
                    HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultConfigKey));
            }

            return CreateConfig(
                HostSecureConfig.CreateKey(HostSecureConfigTests.DefaultServerKey),
                HostSecureConfig.CreateKey(HostSecureConfigTests.DefaultSystemKeyPublic),
                HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultCustomerId),
                HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultSubscriptionId),
                HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultPlanId),
                HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultConfigSitename),
                HostSecureConfig.DecryptKey(HostSecureConfigTests.DefaultConfigKey));
        }

        static Mock<ISecureConfig> CreateConfig(RSACryptoServiceProvider serverKey, RSACryptoServiceProvider systemKey, string customerId, string subscriptionId,string planId, string configSitename, string configKey)
        {
            return CreateConfig(HostSecureConfigTests.DefaultServerID, serverKey, systemKey, customerId,subscriptionId,planId, configSitename, configKey);
        }

        static Mock<ISecureConfig> CreateConfig(Guid serverID, RSACryptoServiceProvider serverKey,
            RSACryptoServiceProvider systemKey, string customerId,string subscriptionId,string planId, string configSitename, string configKey)
        {
            var config = new Mock<ISecureConfig>();
            config.Setup(c => c.ServerID).Returns(serverID);
            config.Setup(c => c.SystemKey).Returns(systemKey);
            config.Setup(c => c.ServerKey).Returns(serverKey);

            config.Setup(c => c.CustomerId).Returns(customerId);
            config.Setup(c => c.SubscriptionId).Returns(subscriptionId);
            config.Setup(c => c.PlanId).Returns(planId);
            config.Setup(c => c.ConfigSitename).Returns(configSitename);
            config.Setup(c => c.ConfigKey).Returns(configKey);
            return config;
        }

        public class HostSecurityProviderImpl : HostSecurityProvider
        {
            public HostSecurityProviderImpl(ISecureConfig config)
                : base(config)
            {
            }
        }
    }
}