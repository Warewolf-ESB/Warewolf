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
using Dev2.Runtime.Subscription;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Enums;
using Warewolf.Licensing;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SubscriptionProviderTest
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionProvider))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubscriptionProvider_ConstructorWithNull_Expected_ThrowsArgumentNullException()
        {
            var provider = new SubscriptionProviderImpl(null);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionProvider))]
        public void SubscriptionProvider_ConstructorWithDefaultConfig_Expected_ReturnsDefaultValues()
        {
            var config = CreateConfig();
            var provider = new SubscriptionProviderImpl(config.Object);
            Assert.AreEqual("", provider.CustomerId);
            Assert.AreEqual(true, provider.StopExecutions);
            Assert.AreEqual("", provider.SubscriptionId);
            Assert.AreEqual("NotRegistered", provider.PlanId);
            Assert.AreEqual(SubscriptionStatus.NotActive, provider.Status);
            Assert.AreEqual("warewolf-test", provider.SubscriptionSiteName);
            Assert.AreEqual("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3", provider.SubscriptionKey);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionProvider))]
        public void SubscriptionProvider_GetSubscriptionData_ReturnsData()
        {
            var config = CreateConfig();
            var providerIml = new SubscriptionProviderImpl(config.Object);
            var provider = providerIml.GetSubscriptionData();
            Assert.AreEqual(true, provider.StopExecutions);
            Assert.AreEqual("", provider.CustomerId);
            Assert.AreEqual("", provider.SubscriptionId);
            Assert.AreEqual("NotRegistered", provider.PlanId);
            Assert.AreEqual(SubscriptionStatus.NotActive, provider.Status);
            Assert.AreEqual("warewolf-test", provider.SubscriptionSiteName);
            Assert.AreEqual("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3", provider.SubscriptionKey);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionProvider))]
        public void SubscriptionProvider_SaveSubscriptionData()
        {
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("16BjmNSXISIQjctO");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3");
            mockSubscriptionData.Setup(o => o.PlanId).Returns("developer");
            mockSubscriptionData.Setup(o => o.StopExecutions).Returns(true);
            mockSubscriptionData.Setup(o => o.Status).Returns(SubscriptionStatus.Active);
            mockSubscriptionData.Setup(o => o.CustomerId).Returns("VMxitsiobdAyth62k0DiqpAUKocG6sV3");
            mockSubscriptionData.Setup(o => o.SubscriptionId).Returns("VMxitsiobdAyth62k0DiqpAUKocG6sV3");

            var config = new Mock<ISubscriptionConfig>();
            config.Setup(c => c.CustomerId).Returns(SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultCustomerId));
            config.Setup(c => c.SubscriptionId).Returns(SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultSubscriptionId));
            config.Setup(c => c.PlanId).Returns(SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultPlanId));
            config.Setup(c => c.SubscriptionSiteName).Returns(SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultSubscriptionSiteName));
            config.Setup(c => c.SubscriptionKey).Returns(SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultSubscriptionKey));
            config.Setup(c => c.Status).Returns(SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultStatus));
            config.Setup(c => c.StopExecutions).Returns(bool.Parse(SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultStopExecutions)));

            var providerIml = new SubscriptionProviderImpl(config.Object);
            config.Setup(o => o.UpdateSubscriptionSettings(It.IsAny<ISubscriptionData>())).Verifiable();
            providerIml.SaveSubscriptionData(mockSubscriptionData.Object);

            config.Verify(o => o.UpdateSubscriptionSettings(It.IsAny<ISubscriptionData>()), Times.Once);
        }

        static Mock<ISubscriptionConfig> CreateConfig()
        {
            return CreateConfig(
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultCustomerId),
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultSubscriptionId),
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultPlanId),
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultStatus),
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultSubscriptionSiteName),
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultSubscriptionKey),
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultStopExecutions));
        }

        static Mock<ISubscriptionConfig> CreateConfig(
            string customerId,
            string subscriptionId,
            string planId,
            string status,
            string subscriptionSiteName,
            string subscriptionKey,
            string stopExecutions)
        {
            var config = new Mock<ISubscriptionConfig>();
            config.Setup(c => c.CustomerId).Returns(customerId);
            config.Setup(c => c.SubscriptionId).Returns(subscriptionId);
            config.Setup(c => c.PlanId).Returns(planId);
            config.Setup(c => c.SubscriptionSiteName).Returns(subscriptionSiteName);
            config.Setup(c => c.SubscriptionKey).Returns(subscriptionKey);
            config.Setup(c => c.Status).Returns(status);
            config.Setup(c => c.StopExecutions).Returns(bool.Parse(stopExecutions));
            return config;
        }

        private class SubscriptionProviderImpl : SubscriptionProvider
        {
            public SubscriptionProviderImpl(ISubscriptionConfig config)
                : base(config)
            {
            }
        }
    }
}