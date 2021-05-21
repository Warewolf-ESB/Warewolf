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
using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Licensing;

namespace Warewolf.LicencingTests
{
    [TestClass]
    public class WarewolfLicenseWrapperTests
    {
        private static ILicenseData GetLicenseData()
        {
            return new LicenseData();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLicenseWrapper))]
        public void WarewolfLicenseWrapper_Retrieve()
        {
            GlobalConstants.LicenseCustomerId = "AzZlx3SU5eKkLDer";
            GlobalConstants.LicensePlanId = "bronze";
            GlobalConstants.ApiKey = "test_cuS2mLPoVv50eDQju3mquk0aC0UM3YYor";
            GlobalConstants.SiteName = "warewolfio-test";

            ApiConfig.Configure(GlobalConstants.SiteName, GlobalConstants.ApiKey);

            var licenseData = GetLicenseData();
            licenseData.CustomerId = GlobalConstants.LicenseCustomerId;
            var warewolfLicense = new WarewolfLicenseWrapper();

            var result = warewolfLicense.Retrieve(licenseData);
            Assert.IsNotNull(result.CustomerFirstName);
            Assert.IsNotNull(result.CustomerLastName);
            Assert.IsNotNull(result.CustomerEmail);
            Assert.IsNotNull(result.CustomerId);
            Assert.IsNotNull(result.PlanId);
            Assert.IsNotNull(result.TrialEnd);
            Assert.IsNotNull(result.Status);
        }
    }
}