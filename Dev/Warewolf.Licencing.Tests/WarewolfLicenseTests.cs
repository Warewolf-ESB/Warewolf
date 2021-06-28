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
    public class WarewolfLicenseTests
    {
        private static ISubscriptionData GetSubscriptionData()
        {
            return new SubscriptionData();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLicense))]
        public void WarewolfLicense_Subscription_Create_NewCustomer_NewPlan()
        {
            var subscriptionData = GetSubscriptionData();
            subscriptionData.CustomerLastName = "Dom";
            subscriptionData.CustomerFirstName = "john";
            subscriptionData.MachineName = "MachineName";
            subscriptionData.CustomerEmail = "john2@user.com";
            subscriptionData.PlanId = "developer";
            subscriptionData.NoOfCores = 1;

            var resultSubscriptionData = GetSubscriptionData();
            resultSubscriptionData.CustomerLastName = "Dom";
            resultSubscriptionData.CustomerFirstName = "john";
            resultSubscriptionData.CustomerEmail = "john2@user.com";
            resultSubscriptionData.PlanId = "developer";
            resultSubscriptionData.MachineName = "MachineName";
            resultSubscriptionData.Status = SubscriptionStatus.Active;
            resultSubscriptionData.CustomerId = "asdsadsdsadsad";
            resultSubscriptionData.SubscriptionId = "asdsadsdsadsad";
            resultSubscriptionData.NoOfCores = 1;

            var mockSubscriptionAdmin = new Mock<ISubscriptionAdmin>();
            mockSubscriptionAdmin.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionAdmin.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var mockSubscription = new Mock<ISubscription>();
            mockSubscription.Setup(o => o.CreatePlan(subscriptionData)).Returns(resultSubscriptionData);

            var license = new WarewolfLicense(mockSubscription.Object);
            var result = license.CreatePlan(subscriptionData);

            Assert.AreEqual(subscriptionData.PlanId, result.PlanId);
            Assert.AreEqual(result.CustomerId, result.CustomerId);
            Assert.AreEqual(result.SubscriptionId, result.SubscriptionId);
            Assert.AreEqual(subscriptionData.CustomerFirstName, result.CustomerFirstName);
            Assert.AreEqual(subscriptionData.MachineName, result.MachineName);
            Assert.AreEqual(subscriptionData.CustomerLastName, result.CustomerLastName);
            Assert.AreEqual(subscriptionData.CustomerEmail, result.CustomerEmail);
            Assert.AreEqual(subscriptionData.NoOfCores, result.NoOfCores);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLicense))]
        public void WarewolfLicense_Subscription_RetrievePlan_Active_IsValid_True()
        {
            var subscriptionData = GetSubscriptionData();
            subscriptionData.CustomerId = "16BjmNSXISIQjctO";
            subscriptionData.SubscriptionId = "16BjmNSXISIQjctO";
            subscriptionData.PlanId = "developer";
            subscriptionData.NoOfCores = 1;
            subscriptionData.MachineName = "MachineName";

            var resultSubscriptionData = GetSubscriptionData();
            resultSubscriptionData.CustomerId = "16BjmNSXISIQjctO";
            resultSubscriptionData.SubscriptionId = "16BjmNSXISIQjctO";
            resultSubscriptionData.PlanId = "developer";
            resultSubscriptionData.Status = SubscriptionStatus.Active;
            resultSubscriptionData.NoOfCores = 1;
            resultSubscriptionData.MachineName = "MachineName";

            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionId).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var mockSubscription = new Mock<ISubscription>();
            mockSubscription.Setup(o => o.RetrievePlan(subscriptionData.SubscriptionId)).Returns(resultSubscriptionData);

            var license = new WarewolfLicense(mockSubscription.Object);
            var resultData = license.RetrievePlan( mockSubscriptionData.Object.SubscriptionId,
                mockSubscriptionData.Object.SubscriptionKey,
                mockSubscriptionData.Object.SubscriptionSiteName);

            Assert.AreEqual(subscriptionData.CustomerId, resultData.CustomerId);
            Assert.AreEqual(subscriptionData.SubscriptionId, resultData.SubscriptionId);
            Assert.AreEqual(SubscriptionStatus.Active, resultData.Status);
            Assert.AreEqual(subscriptionData.PlanId, resultData.PlanId);
            Assert.AreEqual(subscriptionData.MachineName, resultData.MachineName);
            Assert.AreEqual(subscriptionData.NoOfCores, resultData.NoOfCores);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLicense))]
        public void WarewolfLicense_Subscription_RetrievePlan_InTrial_IsValid_True()
        {
            var subscriptionData = GetSubscriptionData();
            subscriptionData.CustomerId = "16BjmNSXISIQjctO";
            subscriptionData.SubscriptionId = "16BjmNSXISIQjctO";
            subscriptionData.PlanId = "developer";
            subscriptionData.NoOfCores = 1;
            subscriptionData.MachineName = "MachineName";

            var resultSubscriptionData = GetSubscriptionData();
            resultSubscriptionData.CustomerId = "16BjmNSXISIQjctO";
            resultSubscriptionData.SubscriptionId = "16BjmNSXISIQjctO";
            resultSubscriptionData.PlanId = "developer";
            resultSubscriptionData.Status = SubscriptionStatus.InTrial;
            resultSubscriptionData.NoOfCores = 1;
            resultSubscriptionData.MachineName = "MachineName";

            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionId).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var mockSubscription = new Mock<ISubscription>();
            mockSubscription.Setup(o => o.RetrievePlan(subscriptionData.SubscriptionId)).Returns(resultSubscriptionData);

            var license = new WarewolfLicense(mockSubscription.Object);
            var resultData = license.RetrievePlan( mockSubscriptionData.Object.SubscriptionId,
                mockSubscriptionData.Object.SubscriptionKey,
                mockSubscriptionData.Object.SubscriptionSiteName);

            Assert.AreEqual(subscriptionData.CustomerId, resultData.CustomerId);
            Assert.AreEqual(subscriptionData.SubscriptionId, resultData.SubscriptionId);
            Assert.AreEqual(SubscriptionStatus.InTrial, resultData.Status);
            Assert.AreEqual(subscriptionData.PlanId, resultData.PlanId);
            Assert.AreEqual(subscriptionData.NoOfCores, resultData.NoOfCores);
            Assert.AreEqual(subscriptionData.MachineName, resultData.MachineName);
            Assert.IsTrue(resultData.IsLicensed);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(WarewolfLicense))]
        public void WarewolfLicense_Subscription_RetrievePlan_Future_IsValid_False()
        {
            var subscriptionData = GetSubscriptionData();
            subscriptionData.CustomerId = "16BjmNSXISIQjctO";
            subscriptionData.SubscriptionId = "16BjmNSXISIQjctO";
            subscriptionData.PlanId = "developer";
            subscriptionData.NoOfCores = 1;
            subscriptionData.MachineName = "MachineName";

            var resultSubscriptionData = GetSubscriptionData();
            resultSubscriptionData.CustomerId = "16BjmNSXISIQjctO";
            resultSubscriptionData.SubscriptionId = "16BjmNSXISIQjctO";
            resultSubscriptionData.PlanId = "developer";
            resultSubscriptionData.Status = SubscriptionStatus.Future;
            resultSubscriptionData.NoOfCores = 1;
            resultSubscriptionData.MachineName = "MachineName";

            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionId).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var mockSubscription = new Mock<ISubscription>();
            mockSubscription.Setup(o => o.RetrievePlan(subscriptionData.SubscriptionId)).Returns(resultSubscriptionData);

            var license = new WarewolfLicense(mockSubscription.Object);
            var resultData = license.RetrievePlan( mockSubscriptionData.Object.SubscriptionId,
                mockSubscriptionData.Object.SubscriptionKey,
                mockSubscriptionData.Object.SubscriptionSiteName);

            Assert.AreEqual(subscriptionData.CustomerId, resultData.CustomerId);
            Assert.AreEqual(SubscriptionStatus.Future, resultData.Status);
            Assert.AreEqual(subscriptionData.PlanId, resultData.PlanId);
            Assert.AreEqual(subscriptionData.NoOfCores, resultData.NoOfCores);
            Assert.AreEqual(subscriptionData.MachineName, resultData.MachineName);
            Assert.IsFalse(resultData.IsLicensed);
        }
    }
}