/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Enums;
using Warewolf.Licensing;

namespace Warewolf.LicencingTests

{
    [TestClass]
    public class LicencingTests
    {
        private static ILicenseData GetLicenseData()
        {
            return new LicenseData();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLicense))]
        public void Licencing_Subscription_Create_NewCustomer_NewPlan()
        {
            var licenseData = GetLicenseData();
            licenseData.CustomerLastName = "Dom";
            licenseData.CustomerFirstName = "john";
            licenseData.CustomerEmail = "john2@user.com";
            licenseData.PlanId = "developer";

            var resultLicenseData = GetLicenseData();
            resultLicenseData.CustomerLastName = "Dom";
            resultLicenseData.CustomerFirstName = "john";
            resultLicenseData.CustomerEmail = "john2@user.com";
            resultLicenseData.PlanId = "developer";
            resultLicenseData.Status = SubscriptionStatus.Active;
            resultLicenseData.CustomerId = "asdsadsdsadsad";
            resultLicenseData.SubscriptionId = "asdsadsdsadsad";

            var mockLicenceApiConfig = new Mock<ILicenceApiConfig>();
            mockLicenceApiConfig.Setup(o => o.SiteName).Returns("sitename");
            mockLicenceApiConfig.Setup(o => o.ApiKey).Returns("apikey");

            var mockChargebeeApiWrapper = new Mock<IWarewolfLicense>();
            mockChargebeeApiWrapper.Setup(o => o.CreatePlan(licenseData)).Returns(licenseData);

            var license = new WarewolfLicense(mockLicenceApiConfig.Object, mockChargebeeApiWrapper.Object);
            var result = license.CreatePlan(licenseData);

            Assert.AreEqual(licenseData.PlanId, result.PlanId);
            Assert.AreEqual(result.CustomerId, result.CustomerId);
            Assert.AreEqual(result.SubscriptionId, result.SubscriptionId);
            Assert.AreEqual(licenseData.CustomerFirstName, result.CustomerFirstName);
            Assert.AreEqual(licenseData.CustomerLastName, result.CustomerLastName);
            Assert.AreEqual(licenseData.CustomerEmail, result.CustomerEmail);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLicense))]
        public void Licencing_Subscription_ExistingCustomer_UpgradePlan_WithBankDetails_Immediately()
        {
            var licenseData = GetLicenseData();
            licenseData.CustomerId = "cbdemo_prepaid_card";
            licenseData.SubscriptionId = "cbdemo_prepaid_card";
            licenseData.PlanId = "developer";
            licenseData.CardCvv = 100;
            licenseData.CardExpiryYear = 2022;
            licenseData.CardExpiryMonth = 12;
            licenseData.CustomerEmail = "john2@user.com";
            licenseData.EndOfTerm = false;

            var resultLicenseData = GetLicenseData();
            resultLicenseData.CustomerId = "cbdemo_prepaid_card";
            resultLicenseData.SubscriptionId = "cbdemo_prepaid_card";
            resultLicenseData.PlanId = "developer";
            resultLicenseData.Status = SubscriptionStatus.Active;

            var mockLicenceApiConfig = new Mock<ILicenceApiConfig>();
            mockLicenceApiConfig.Setup(o => o.SiteName).Returns("sitename");
            mockLicenceApiConfig.Setup(o => o.ApiKey).Returns("apikey");

            var mockChargebeeApiWrapper = new Mock<IWarewolfLicense>();
            mockChargebeeApiWrapper.Setup(o => o.UpgradePlan(licenseData)).Returns(resultLicenseData);

            var license = new WarewolfLicense(mockLicenceApiConfig.Object, mockChargebeeApiWrapper.Object);
            var resultData = license.UpgradePlan(licenseData);
            Assert.AreEqual(licenseData.CustomerId, resultData.CustomerId);
            Assert.AreEqual(licenseData.SubscriptionId, resultData.SubscriptionId);
            Assert.AreEqual(SubscriptionStatus.Active, resultData.Status);
            Assert.AreEqual(licenseData.PlanId, resultData.PlanId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLicense))]
        public void Licencing_Subscription_ExistingCustomer_UpgradePlan_WithBankDetails_EndOfTerm()
        {
            var licenseData = GetLicenseData();
            licenseData.CustomerId = "cbdemo_prepaid_card";
            licenseData.SubscriptionId = "cbdemo_prepaid_card";
            licenseData.PlanId = "enterprise";
            licenseData.CardCvv = 100;
            licenseData.CardExpiryYear = 2022;
            licenseData.CardExpiryMonth = 12;
            licenseData.CustomerEmail = "john2@user.com";
            licenseData.EndOfTerm = true;

            var resultLicenseData = GetLicenseData();
            resultLicenseData.CustomerId = "cbdemo_prepaid_card";
            resultLicenseData.SubscriptionId = "cbdemo_prepaid_card";
            resultLicenseData.PlanId = "enterprise";
            resultLicenseData.Status = SubscriptionStatus.Active;

            var mockLicenceApiConfig = new Mock<ILicenceApiConfig>();
            mockLicenceApiConfig.Setup(o => o.SiteName).Returns("sitename");
            mockLicenceApiConfig.Setup(o => o.ApiKey).Returns("apikey");

            var mockChargebeeApiWrapper = new Mock<IWarewolfLicense>();
            mockChargebeeApiWrapper.Setup(o => o.UpgradePlan(licenseData)).Returns(resultLicenseData);

            var license = new WarewolfLicense(mockLicenceApiConfig.Object, mockChargebeeApiWrapper.Object);
            var resultData = license.UpgradePlan(licenseData);
            Assert.AreEqual(licenseData.CustomerId, resultData.CustomerId);
            Assert.AreEqual(licenseData.SubscriptionId, resultData.SubscriptionId);
            Assert.AreEqual(SubscriptionStatus.Active, resultData.Status);
            Assert.AreEqual(licenseData.PlanId, resultData.PlanId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLicense))]
        public void Licencing_Subscription_RetrievePlan_Active_IsValid_True()
        {
            var licenseData = GetLicenseData();
            licenseData.CustomerId = "cbdemo_prepaid_card";
            licenseData.SubscriptionId = "cbdemo_prepaid_card";
            licenseData.PlanId = "developer";

            var resultLicenseData = GetLicenseData();
            resultLicenseData.CustomerId = "cbdemo_prepaid_card";
            resultLicenseData.SubscriptionId = "cbdemo_prepaid_card";
            resultLicenseData.PlanId = "developer";
            resultLicenseData.Status = SubscriptionStatus.Active;

            var mockLicenceApiConfig = new Mock<ILicenceApiConfig>();
            mockLicenceApiConfig.Setup(o => o.SiteName).Returns("sitename");
            mockLicenceApiConfig.Setup(o => o.ApiKey).Returns("apikey");

            var mockChargebeeApiWrapper = new Mock<IWarewolfLicense>();
            mockChargebeeApiWrapper.Setup(o => o.Retrieve(licenseData)).Returns(resultLicenseData);

            var license = new WarewolfLicense(mockLicenceApiConfig.Object, mockChargebeeApiWrapper.Object);
            var resultData = license.Retrieve(licenseData);

            Assert.AreEqual(licenseData.CustomerId, resultData.CustomerId);
            Assert.AreEqual(licenseData.SubscriptionId, resultData.SubscriptionId);
            Assert.AreEqual(SubscriptionStatus.Active, resultData.Status);
            Assert.AreEqual(licenseData.PlanId, resultData.PlanId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLicense))]
        public void Licencing_Subscription_RetrievePlan_InTrial_IsValid_True()
        {
            var licenseData = GetLicenseData();
            licenseData.CustomerId = "cbdemo_prepaid_card";
            licenseData.SubscriptionId = "cbdemo_prepaid_card";
            licenseData.PlanId = "developer";

            var resultLicenseData = GetLicenseData();
            resultLicenseData.CustomerId = "cbdemo_prepaid_card";
            resultLicenseData.SubscriptionId = "cbdemo_prepaid_card";
            resultLicenseData.PlanId = "developer";
            resultLicenseData.Status = SubscriptionStatus.InTrial;

            var mockLicenceApiConfig = new Mock<ILicenceApiConfig>();
            mockLicenceApiConfig.Setup(o => o.SiteName).Returns("sitename");
            mockLicenceApiConfig.Setup(o => o.ApiKey).Returns("apikey");

            var mockChargebeeApiWrapper = new Mock<IWarewolfLicense>();
            mockChargebeeApiWrapper.Setup(o => o.Retrieve(licenseData)).Returns(resultLicenseData);

            var license = new WarewolfLicense(mockLicenceApiConfig.Object, mockChargebeeApiWrapper.Object);
            var resultData = license.Retrieve(licenseData);

            Assert.AreEqual(licenseData.CustomerId, resultData.CustomerId);
            Assert.AreEqual(licenseData.SubscriptionId, resultData.SubscriptionId);
            Assert.AreEqual(SubscriptionStatus.InTrial, resultData.Status);
            Assert.AreEqual(licenseData.PlanId, resultData.PlanId);
            Assert.IsTrue(resultData.IsLicensed);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLicense))]
        public void Licencing_Subscription_RetrievePlan_Future_IsValid_False()
        {
            var licenseData = GetLicenseData();
            licenseData.CustomerId = "16BjmNSXISIQjctO";
            licenseData.SubscriptionId = "16BjmNSXISIQjctO";
            licenseData.PlanId = "developer";

            var resultLicenseData = GetLicenseData();
            resultLicenseData.CustomerId = "16BjmNSXISIQjctO";
            resultLicenseData.SubscriptionId = "16BjmNSXISIQjctO";
            resultLicenseData.PlanId = "developer";
            resultLicenseData.Status = SubscriptionStatus.Future;

            var mockLicenceApiConfig = new Mock<ILicenceApiConfig>();
            mockLicenceApiConfig.Setup(o => o.SiteName).Returns("sitename");
            mockLicenceApiConfig.Setup(o => o.ApiKey).Returns("apikey");

            var mockChargebeeApiWrapper = new Mock<IWarewolfLicense>();
            mockChargebeeApiWrapper.Setup(o => o.Retrieve(licenseData)).Returns(resultLicenseData);

            var license = new WarewolfLicense(mockLicenceApiConfig.Object, mockChargebeeApiWrapper.Object);
            var resultData = license.Retrieve(licenseData);

            Assert.AreEqual(licenseData.CustomerId, resultData.CustomerId);
            Assert.AreEqual(SubscriptionStatus.Future, resultData.Status);
            Assert.AreEqual(licenseData.PlanId, resultData.PlanId);
            Assert.IsFalse(resultData.IsLicensed);
        }
    }
}