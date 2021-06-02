/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using ChargeBee.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Licensing;

namespace Warewolf.LicencingTests
{
    [TestClass]
    public class WarewolfLicenseWrapperTests
    {
        private static ISubscriptionData GetLicenseData()
        {
            return new SubscriptionData();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Subscription))]
        public void WarewolfLicenseWrapper_Retrieve()
        {
            // LicenseSettings.CustomerId = "16BjmNSXISIQjctO";
            // LicenseSettings.SubscriptionId = "16BjmNSXISIQjctO";
            // LicenseSettings.PlanId = "developer";
            // LicenseSettings.ApiKey = "test_VMxitsiobdAyth62k0DiqpAUKocG6sV3";
            // LicenseSettings.SiteName = "warewolf-test";
            var mockSubscriptionAdmin = new Mock<ISubscriptionAdmin>();
            mockSubscriptionAdmin.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionAdmin.Setup(o => o.SubscriptionSiteName).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            ApiConfig.Configure(mockSubscriptionAdmin.Object.SubscriptionSiteName, mockSubscriptionAdmin.Object.SubscriptionKey);

            var licenseData = GetLicenseData();
            //licenseData.CustomerId = LicenseSettings.CustomerId;
            //licenseData.SubscriptionId = LicenseSettings.SubscriptionId;
            var warewolfLicense = new Subscription();

            var result = warewolfLicense.Retrieve(licenseData.SubscriptionId);
            Assert.IsNotNull(result.CustomerFirstName);
            Assert.IsNotNull(result.CustomerLastName);
            Assert.IsNotNull(result.CustomerEmail);
            Assert.IsNotNull(result.CustomerId);
            Assert.IsNotNull(result.SubscriptionId);
            Assert.IsNotNull(result.PlanId);
            Assert.IsNotNull(result.TrialEnd);
            Assert.IsNotNull(result.Status);
        }
    }
}