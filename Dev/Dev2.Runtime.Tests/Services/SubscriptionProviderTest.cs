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
            Assert.AreEqual("", provider.SubscriptionId);
            Assert.AreEqual("NotRegistered", provider.PlanId);
            Assert.AreEqual(SubscriptionStatus.NotActive, provider.Status);
            Assert.AreEqual("warewolf-test", provider.SubscriptionSiteName);
            Assert.AreEqual("test_VMxitsiobdAyth62k0DiqpAUKocG6sV3", provider.SubscriptionKey);
        }

        static Mock<ISubscriptionConfig> CreateConfig()
        {
            return CreateConfig(
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultCustomerId),
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultSubscriptionId),
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultPlanId),
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultStatus),
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultSubscriptionSiteName),
                SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultSubscriptionKey));
        }

        static Mock<ISubscriptionConfig> CreateConfig(
            string customerId,
            string subscriptionId,
            string planId,
            string status,
            string subscriptionSiteName,
            string subscriptionKey)
        {
            var config = new Mock<ISubscriptionConfig>();
            config.Setup(c => c.CustomerId).Returns(customerId);
            config.Setup(c => c.SubscriptionId).Returns(subscriptionId);
            config.Setup(c => c.PlanId).Returns(planId);
            config.Setup(c => c.SubscriptionSiteName).Returns(subscriptionSiteName);
            config.Setup(c => c.SubscriptionKey).Returns(subscriptionKey);
            config.Setup(c => c.Status).Returns(status);
            return config;
        }

        public class SubscriptionProviderImpl : SubscriptionProvider
        {
            public SubscriptionProviderImpl(ISubscriptionConfig config)
                : base(config)
            {
            }
        }
    }
}