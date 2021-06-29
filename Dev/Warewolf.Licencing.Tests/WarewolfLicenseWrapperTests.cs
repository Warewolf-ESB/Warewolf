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

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Subscription))]
        public void WarewolfLicenseWrapper_SubscriptionExists_SameUser_SameMachine_ReturnsTrue()
        {
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.MachineName).Returns("321654");
            mockSubscriptionData.Setup(o => o.CustomerFirstName).Returns("Candice");
            mockSubscriptionData.Setup(o => o.CustomerLastName).Returns("Daniel");
            mockSubscriptionData.Setup(o => o.CustomerEmail).Returns("candice.daniel@dev2.co.za");
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("warewolf-test");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");
            ApiConfig.Configure(mockSubscriptionData.Object.SubscriptionSiteName, mockSubscriptionData.Object.SubscriptionKey);

            var warewolfLicense = new Subscription();
            var result = warewolfLicense.SubscriptionExists(mockSubscriptionData.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Subscription))]
        public void WarewolfLicenseWrapper_SubscriptionExists_DifferentUser_SameMachine_ReturnsFalse()
        {
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.MachineName).Returns("321654");
            mockSubscriptionData.Setup(o => o.CustomerFirstName).Returns("Test");
            mockSubscriptionData.Setup(o => o.CustomerLastName).Returns("Test");
            mockSubscriptionData.Setup(o => o.CustomerEmail).Returns("Test@dev2.co.za");
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("warewolf-test");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");
            ApiConfig.Configure(mockSubscriptionData.Object.SubscriptionSiteName, mockSubscriptionData.Object.SubscriptionKey);

            var warewolfLicense = new Subscription();
            var result = warewolfLicense.SubscriptionExists(mockSubscriptionData.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Subscription))]
        public void WarewolfLicenseWrapper_SubscriptionExists_SameUser_DifferentMachine_ReturnsFalse()
        {
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.MachineName).Returns("00000");
            mockSubscriptionData.Setup(o => o.CustomerFirstName).Returns("Candice");
            mockSubscriptionData.Setup(o => o.CustomerLastName).Returns("Daniel");
            mockSubscriptionData.Setup(o => o.CustomerEmail).Returns("candice.daniel@dev2.co.za");
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("warewolf-test");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");
            ApiConfig.Configure(mockSubscriptionData.Object.SubscriptionSiteName, mockSubscriptionData.Object.SubscriptionKey);

            var warewolfLicense = new Subscription();
            var result = warewolfLicense.SubscriptionExists(mockSubscriptionData.Object);
            Assert.IsFalse(result);
        }
    }
}