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
using Warewolf.Enums;
using Warewolf.Licensing;

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

        static NameValueCollection _defaultSettings;

        private static NameValueCollection CreateDefaultConfig()
        {
            return SubscriptionConfig.CreateSettings(DefaultCustomerId, DefaultPlanId, DefaultSubscriptionId, DefaultStatus, DefaultSubscriptionSiteName, DefaultSubscriptionKey);
        }

        /*[TestMethod]
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
        }*/

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
        public void SubscriptionConfig_WithDefaultSettings_Expected_LoadsDefaultValues()
        {
            _defaultSettings = CreateDefaultConfig();
            var config = new SubscriptionConfigMock(_defaultSettings);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultCustomerId), config.CustomerId);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultPlanId), config.PlanId);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultSubscriptionId), config.SubscriptionId);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultSubscriptionSiteName), config.SubscriptionSiteName);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultSubscriptionKey), config.SubscriptionKey);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultStatus), config.Status);

            Assert.IsNull(config.SaveConfigSettings);

            Assert.AreEqual(0, config.SaveConfigHitCount);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionConfig))]
        public void SubscriptionConfig_UpdateSubscriptionSettings_LoadsNewValues()
        {
            _defaultSettings = CreateDefaultConfig();
            var config = new SubscriptionConfigMock(_defaultSettings);
            const string SubscriptionKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            const string SubscriptionSiteName = "warewolf-test";
            const string PlanId = "developer";
            const string CustomerId = "newCustomer";
            const SubscriptionStatus Status = SubscriptionStatus.InTrial;
            const string SubscriptionId = "5467897";

            var newSubscriptionData = new SubscriptionData
            {
                CustomerId = CustomerId,
                SubscriptionId = SubscriptionId,
                PlanId = PlanId,
                Status = Status,
                SubscriptionSiteName = SubscriptionSiteName,
                SubscriptionKey = SubscriptionKey
            };
            config.UpdateSubscriptionSettings(newSubscriptionData);

            Assert.IsNotNull(config.SaveConfigSettings);
            Assert.AreEqual(1, config.SaveConfigHitCount);
        }
    }
}