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
using Dev2.Services.Security;
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
        internal const string DefaultMarketplaceResourceId = "";
        internal const string DefaultPlanId = "qj2HmQwVsUt12btj/iXadA==";
        internal const string DefaultSubscriptionKey = "wCYcjqzbAiHIneFFib+LCrn73SSkOlRzm4QxP+mkeHsH7e3surKN5liDsrv39JFR";
        internal const string DefaultSubscriptionSiteName = "L8NilnImZ18r8VCMD88AdQ==";
        internal const string DefaultStatus = "aT/AoVWEMyf6OPvaYp47Gw==";
        internal const string DefaultStopExecutions = "r/EOk8xFEhRno3TYRCvIKQ==";
        static NameValueCollection _defaultSettings;

        private static NameValueCollection CreateDefaultConfig()
        {
            return SubscriptionConfig.CreateSettings(DefaultCustomerId, DefaultPlanId, DefaultSubscriptionId, DefaultStatus, DefaultSubscriptionSiteName, DefaultSubscriptionKey, DefaultStopExecutions, DefaultMarketplaceResourceId);
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

          //  var value = "true";
          //  var encryptedData = SecurityEncryption.Encrypt(value);
         //   var decryptedData = SecurityEncryption.Decrypt(encryptedData);
          //  Assert.AreEqual(value, decryptedData.TrimEnd('\0'));
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
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultMarketplaceResourceId), config.MarketplaceResourceId);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultSubscriptionSiteName), config.SubscriptionSiteName);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultSubscriptionKey), config.SubscriptionKey);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultStatus), config.Status);
            Assert.AreEqual(bool.Parse(SubscriptionConfig.DecryptKey(DefaultStopExecutions)), config.StopExecutions);
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
            const string MarketplaceResourceId = "8f14e45f-ceea-467e-abd0-2c1a1c8b9600";
            const string StopExecutions = "true";
            var newSubscriptionData = new SubscriptionData
            {
                CustomerId = CustomerId,
                SubscriptionId = SubscriptionId,
                MarketplaceResourceId = MarketplaceResourceId,
                PlanId = PlanId,
                Status = Status,
                SubscriptionSiteName = SubscriptionSiteName,
                SubscriptionKey = SubscriptionKey,
                StopExecutions = bool.Parse(StopExecutions)
            };
            config.UpdateSubscriptionSettings(newSubscriptionData);

            Assert.IsNotNull(config.SaveConfigSettings);
            Assert.AreEqual(1, config.SaveConfigHitCount);
            Assert.AreEqual(MarketplaceResourceId, SubscriptionConfig.DecryptKey(config.SaveConfigSettings["MarketplaceResourceId"]));
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionConfig))]
        public void SubscriptionConfig_WithPopulatedMarketplaceResourceId_Expected_RoundTripsThroughEncryptDecrypt()
        {
            const string MarketplaceResourceId = "8f14e45f-ceea-467e-abd0-2c1a1c8b9600";
            var settings = SubscriptionConfig.CreateSettings(
                DefaultCustomerId,
                DefaultPlanId,
                DefaultSubscriptionId,
                DefaultStatus,
                DefaultSubscriptionSiteName,
                DefaultSubscriptionKey,
                DefaultStopExecutions,
                Dev2.Services.Security.SecurityEncryption.Encrypt(MarketplaceResourceId));

            var config = new SubscriptionConfigMock(settings);

            Assert.AreEqual(MarketplaceResourceId, config.MarketplaceResourceId);
        }

        // ── Absolute base-path file resolution (isolated-worker CWD-relative bug fix) ──
        // See docs/SubscriptionConfig-IsolatedWorker-Resolution-Spec.md. These exercise the real
        // file-backed constructor path end-to-end (no SaveConfig mocking) — the path that had
        // zero coverage before this fix, which is exactly how the original bug shipped unnoticed.

        string _tempDir = string.Empty;

        [TestInitialize]
        public void SetupBasePathTests() => _tempDir = System.IO.Directory.CreateDirectory(
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), "subscriptionconfig-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void CleanupBasePathTests()
        {
            if (System.IO.Directory.Exists(_tempDir))
            {
                System.IO.Directory.Delete(_tempDir, recursive: true);
            }
        }

        string LicenseFilePath => System.IO.Path.Combine(_tempDir, "Warewolf License.secureconfig");

        static void WriteRealLicenseFile(
            string path, string customerId, string planId, string subscriptionId, string status,
            string subscriptionSiteName, string subscriptionKey, string stopExecutions, string marketplaceResourceId = "")
        {
            var root = new System.Xml.Linq.XElement("subscriptionSettings",
                new System.Xml.Linq.XElement("add", new System.Xml.Linq.XAttribute("key", "CustomerId"), new System.Xml.Linq.XAttribute("value", customerId)),
                new System.Xml.Linq.XElement("add", new System.Xml.Linq.XAttribute("key", "SubscriptionId"), new System.Xml.Linq.XAttribute("value", subscriptionId)),
                new System.Xml.Linq.XElement("add", new System.Xml.Linq.XAttribute("key", "MarketplaceResourceId"), new System.Xml.Linq.XAttribute("value", marketplaceResourceId)),
                new System.Xml.Linq.XElement("add", new System.Xml.Linq.XAttribute("key", "Status"), new System.Xml.Linq.XAttribute("value", status)),
                new System.Xml.Linq.XElement("add", new System.Xml.Linq.XAttribute("key", "PlanId"), new System.Xml.Linq.XAttribute("value", planId)),
                new System.Xml.Linq.XElement("add", new System.Xml.Linq.XAttribute("key", "SubscriptionKey"), new System.Xml.Linq.XAttribute("value", subscriptionKey)),
                new System.Xml.Linq.XElement("add", new System.Xml.Linq.XAttribute("key", "SubscriptionSiteName"), new System.Xml.Linq.XAttribute("value", subscriptionSiteName)),
                new System.Xml.Linq.XElement("add", new System.Xml.Linq.XAttribute("key", "StopExecutions"), new System.Xml.Linq.XAttribute("value", stopExecutions)));
            new System.Xml.Linq.XDocument(new System.Xml.Linq.XDeclaration("1.0", "utf-8", ""), root).Save(path);
        }

        [TestMethod]
        [TestCategory(nameof(SubscriptionConfig))]
        public void SubscriptionConfig_ForBasePath_MissingFile_WritesDefaultsAtThatExactAbsolutePath()
        {
            Assert.IsFalse(System.IO.File.Exists(LicenseFilePath));

            var config = SubscriptionConfig.ForBasePath(_tempDir);

            Assert.IsTrue(System.IO.File.Exists(LicenseFilePath),
                "Defaults must be written at the resolved absolute path passed to ForBasePath, never a CWD-relative one.");
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultPlanId), config.PlanId);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultSubscriptionKey), config.SubscriptionKey);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultSubscriptionSiteName), config.SubscriptionSiteName);
        }

        [TestMethod]
        [TestCategory(nameof(SubscriptionConfig))]
        public void SubscriptionConfig_ForBasePath_ExistingRealFile_IsReadCorrectly_AndNotOverwritten()
        {
            // This is the exact reproduction from the spec: a genuine, plaintext-staged license
            // file with real values must be read as-is on the very first construction, and must
            // NOT be silently replaced with SubscriptionProvider's default/broken-installation
            // constants — which is what the original CWD-relative bug did.
            const string CustomerId = "real-customer";
            const string PlanId = "enterprise";
            const string SubscriptionId = "sub-12345";
            const string Status = "Active";
            const string SubscriptionSiteName = "warewolf";
            const string SubscriptionKey = "Azq9KATrttxzMIhF";
            const string StopExecutions = "false";

            WriteRealLicenseFile(LicenseFilePath, CustomerId, PlanId, SubscriptionId, Status, SubscriptionSiteName, SubscriptionKey, StopExecutions);
            var fileContentBefore = System.IO.File.ReadAllText(LicenseFilePath);

            var config = SubscriptionConfig.ForBasePath(_tempDir);

            Assert.AreEqual(CustomerId, config.CustomerId);
            Assert.AreEqual(PlanId, config.PlanId);
            Assert.AreEqual(SubscriptionId, config.SubscriptionId);
            Assert.AreEqual(Status, config.Status);
            Assert.AreEqual(SubscriptionSiteName, config.SubscriptionSiteName);
            Assert.AreEqual(SubscriptionKey, config.SubscriptionKey);
            Assert.AreEqual(bool.Parse(StopExecutions), config.StopExecutions);

            // Plaintext input re-encrypts and re-saves in place (existing isPlainText behaviour) —
            // so the file content is expected to CHANGE, but must still round-trip to the SAME
            // real values, never SubscriptionProvider's default/live constants.
            var fileContentAfter = System.IO.File.ReadAllText(LicenseFilePath);
            Assert.AreNotEqual(fileContentBefore, fileContentAfter, "Plaintext staged values should have been re-encrypted in place.");

            var reloaded = SubscriptionConfig.ForBasePath(_tempDir);
            Assert.AreEqual(CustomerId, reloaded.CustomerId);
            Assert.AreEqual(PlanId, reloaded.PlanId);
            Assert.AreEqual(SubscriptionKey, reloaded.SubscriptionKey);
            Assert.AreEqual(SubscriptionSiteName, reloaded.SubscriptionSiteName);
        }

        [TestMethod]
        [TestCategory(nameof(SubscriptionConfig))]
        public void SubscriptionConfig_ForBasePath_FileWithEmptyKeyValues_FallsBackToDefaults()
        {
            WriteRealLicenseFile(LicenseFilePath, "", "", "", "", "", "", "false");

            // Pre-existing behaviour (unchanged by this fix): the "broken installation" branch
            // writes fresh defaults to disk, but does not populate the constructing instance's
            // own in-memory properties. A subsequent construction reads the now-default file back
            // correctly — which is what this test actually verifies: the write landed at the same
            // absolute path this instance itself resolved, not a CWD-relative one.
            SubscriptionConfig.ForBasePath(_tempDir);

            var reloaded = SubscriptionConfig.ForBasePath(_tempDir);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultPlanId), reloaded.PlanId);
            Assert.AreEqual(SubscriptionConfig.DecryptKey(DefaultSubscriptionKey), reloaded.SubscriptionKey);
        }

        [TestMethod]
        [TestCategory(nameof(SubscriptionConfig))]
        public void SubscriptionConfig_ForBasePath_IsUnaffectedByCurrentWorkingDirectory()
        {
            // Regression test for the original defect: resolution must depend ONLY on the
            // explicit base path, never on Environment.CurrentDirectory — reproduces the
            // isolated-worker host's "CWD != deployment directory" scenario directly.
            const string CustomerId = "cwd-independence-customer";
            WriteRealLicenseFile(LicenseFilePath, CustomerId, "developer", "sub-1", "Active", "warewolf", "real-key-value", "false");

            var originalCwd = Environment.CurrentDirectory;
            var otherDir = System.IO.Directory.CreateDirectory(
                System.IO.Path.Combine(System.IO.Path.GetTempPath(), "subscriptionconfig-cwd-" + Guid.NewGuid().ToString("N"))).FullName;
            try
            {
                Environment.CurrentDirectory = otherDir;

                var config = SubscriptionConfig.ForBasePath(_tempDir);

                Assert.AreEqual(CustomerId, config.CustomerId,
                    "Resolution must use the explicit base path, not a CWD-relative lookup that would miss the real file.");
                Assert.IsFalse(System.IO.File.Exists(System.IO.Path.Combine(otherDir, "Warewolf License.secureconfig")),
                    "Nothing should ever be written relative to the current working directory.");
            }
            finally
            {
                Environment.CurrentDirectory = originalCwd;
                System.IO.Directory.Delete(otherDir, recursive: true);
            }
        }
    }
}