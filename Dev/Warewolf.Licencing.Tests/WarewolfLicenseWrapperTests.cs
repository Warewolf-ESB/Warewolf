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

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Subscription))]
        public void WarewolfLicenseWrapper_Retrieve()
        {
            const string SubscriptionId = "16BjmNSXISIQjctO";
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionId).Returns(SubscriptionId);
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("warewolf-test");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");
            ApiConfig.Configure(mockSubscriptionData.Object.SubscriptionSiteName, mockSubscriptionData.Object.SubscriptionKey);

            var warewolfLicense = new Subscription();
            var result = warewolfLicense.RetrievePlan(SubscriptionId);
            Assert.IsNotNull(result.CustomerFirstName);
            Assert.IsNotNull(result.MachineName);
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