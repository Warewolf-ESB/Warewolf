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
using System.Collections.Specialized;
using Dev2.Runtime.Subscription;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SubscriptionConfigTests
    {
        internal const string DefaultCustomerId = "";
        internal const string DefaultSubscriptionId = "";
        internal const string DefaultPlanId = "qj2HmQwVsUt12btj/iXadA==";
        internal const string DefaultSubscriptionKey = "wCYcjqzbAiHIneFFib+LCrn73SSkOlRzm4QxP+mkeHsH7e3surKN5liDsrv39JFR";
        internal const string DefaultSubscriptionSiteName = "L8NilnImZ18r8VCMD88AdQ==";
        internal const string DefaultStatus = "aT/AoVWEMyf6OPvaYp47Gw==";
        static NameValueCollection _newSettings;
        static NameValueCollection _defaultSettings;

        private static NameValueCollection CreateDefaultConfig()
        {
            return SubscriptionConfig.CreateSettings(DefaultCustomerId, DefaultPlanId, DefaultSubscriptionId, DefaultStatus, DefaultSubscriptionSiteName, DefaultSubscriptionKey);
        }
        private static NameValueCollection CreateBlankConfig()
        {
            return SubscriptionConfig.CreateSettings("", "", "", "", "", "");
        }
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _defaultSettings = CreateDefaultConfig();
            _newSettings = SubscriptionConfig.CreateSettings(DefaultCustomerId, DefaultPlanId, DefaultSubscriptionId, DefaultStatus, DefaultSubscriptionSiteName, DefaultSubscriptionKey);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionConfig))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubscriptionConfig_WithoutConfig_Expected_ThrowsArgumentNullException()
        {
            new SubscriptionConfig(null);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionConfig))]
        public void CreateEncryptions()
        {
            // Keep this for when we need to encrypt the live keys
            // TEST
            // var value = "warewolf-test";
            //  var encryptedData = SecurityEncryption.Encrypt(value);
            //  var decryptedData = SecurityEncryption.Decrypt(encryptedData);
            //   Assert.AreEqual(value, decryptedData.TrimEnd('\0'));

            //var value2 = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            //var encryptedData2 = SecurityEncryption.Encrypt(value2);
            // var decryptedData2 = SecurityEncryption.Decrypt(encryptedData2);
            //  Assert.AreEqual(value2, decryptedData2.TrimEnd('\0'));

            //var value3 = "NotActive";
            // var encryptedData3 = SecurityEncryption.Encrypt(value3);
            // var decryptedData3 = SecurityEncryption.Decrypt(encryptedData3);
            //  Assert.AreEqual(value3, decryptedData3.TrimEnd('\0'));
            //LIVE
            //   var valueLive = "warewolf";
            //   var encryptedBytesLive = SecurityEncryption.Encrypt(valueLive);
            //   var value2Live = "live_bcdR3fp1fm1YeQYhrzaLjp0Qy5rcuwVRzo";
            // var encryptedBytes2Live = SecurityEncryption.Encrypt(value2Live);
            //
            //var decryptedBytes = SecurityEncryption.Decrypt(encryptedBytes2Live);
            // Assert.AreEqual(value, decryptedBytes);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionConfig))]
        public void SubscriptionConfig_WithDefaultSettings_Expected_LoadsDefaultValues()
        {
            TestConfig(DefaultCustomerId, DefaultSubscriptionId, DefaultStatus, DefaultSubscriptionSiteName, DefaultSubscriptionKey, DefaultPlanId, false);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionConfig))]
        public void SubscriptionConfig_WithNewSettings_Expected_LoadsNewValues()
        {
            _defaultSettings = CreateBlankConfig();
            _newSettings = SubscriptionConfig.CreateSettings("", "", "","" ,"", "");

            TestConfig("", "", "","" ,"", "", true);
        }

        static void TestConfig(string expectedCustomerId, string expectedSubscriptionId, string expectedStatus, string expectedSubscriptionSiteName, string expectedSubscriptionKey, string expectedPlanId, bool isNewConfig)
        {
            var config = new SubscriptionConfigMock(isNewConfig ? _newSettings : _defaultSettings);
            TestConfig(expectedCustomerId, expectedSubscriptionId, expectedStatus, expectedSubscriptionSiteName, expectedSubscriptionKey, expectedPlanId, isNewConfig, config);
        }

        static void TestConfig(string expectedCustomerId, string expectedSubscriptionId, string expectedStatus, string expectedSubscriptionSiteName, string expectedSubscriptionKey, string expectedPlanId, bool isNewConfig, SubscriptionConfigMock config)
        {
            if(isNewConfig)
            {
                Assert.AreEqual("", config.CustomerId);
                Assert.AreEqual("NotRegistered", config.PlanId);
                Assert.AreEqual("", config.SubscriptionId);
                Assert.AreEqual("warewolf-test", config.SubscriptionSiteName);
                Assert.AreEqual("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3", config.SubscriptionKey);
                Assert.AreEqual("NotActive", config.Status);
                Assert.IsNotNull(config.SaveConfigSettings);
                Assert.AreNotEqual(_newSettings, config.SaveConfigSettings);

                Assert.AreEqual(1, config.SaveConfigHitCount);
                Assert.AreEqual(1, config.ProtectConfigHitCount);
            }
            else
            {
                Assert.AreEqual(SubscriptionConfig.DecryptKey(expectedCustomerId), config.CustomerId);
                Assert.AreEqual(SubscriptionConfig.DecryptKey(expectedPlanId), config.PlanId);
                Assert.AreEqual(SubscriptionConfig.DecryptKey(expectedSubscriptionId), config.SubscriptionId);
                Assert.AreEqual(SubscriptionConfig.DecryptKey(expectedSubscriptionSiteName), config.SubscriptionSiteName);
                Assert.AreEqual(SubscriptionConfig.DecryptKey(expectedSubscriptionKey), config.SubscriptionKey);
                Assert.AreEqual(SubscriptionConfig.DecryptKey(expectedStatus), config.Status);
                Assert.IsNull(config.SaveConfigSettings);

                Assert.AreEqual(0, config.SaveConfigHitCount);
                Assert.AreEqual(0, config.ProtectConfigHitCount);
            }
        }
    }
}