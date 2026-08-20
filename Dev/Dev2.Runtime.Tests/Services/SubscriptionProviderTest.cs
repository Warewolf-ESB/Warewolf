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
    [DoNotParallelize] // SubscriptionProvider._config/_theInstance are static (shared mutable
                        // state): SaveSubscriptionData/SetLicense both reassign them, so tests
                        // must run serially or they race each other's mocks.
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
            Assert.AreEqual("", provider.MarketplaceResourceId);
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
            Assert.AreEqual("", provider.MarketplaceResourceId);
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
            mockSubscriptionData.Setup(o => o.MarketplaceResourceId).Returns("8f14e45f-ceea-467e-abd0-2c1a1c8b9600");

            var config = new Mock<ISubscriptionConfig>();
            config.Setup(c => c.CustomerId).Returns(SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultCustomerId));
            config.Setup(c => c.SubscriptionId).Returns(SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultSubscriptionId));
            config.Setup(c => c.MarketplaceResourceId).Returns(SubscriptionConfig.DecryptKey(SubscriptionConfigTests.DefaultMarketplaceResourceId));
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

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionProvider))]
        public void SubscriptionProvider_SaveSubscriptionData_NeverChangesSubscriptionKeyOrSiteName()
        {
            // SaveSubscriptionData is the Chargebee plan/status-update path: it must always keep
            // this instance's existing SubscriptionKey/SubscriptionSiteName, no matter what the
            // caller passes — see SetLicense (below) for the method that DOES allow the key to change.
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("attacker-supplied-site");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("attacker-supplied-key");
            mockSubscriptionData.Setup(o => o.PlanId).Returns("developer");
            mockSubscriptionData.Setup(o => o.Status).Returns(SubscriptionStatus.Active);
            mockSubscriptionData.Setup(o => o.CustomerId).Returns("cust-1");
            mockSubscriptionData.Setup(o => o.SubscriptionId).Returns("sub-1");
            mockSubscriptionData.Setup(o => o.MarketplaceResourceId).Returns(string.Empty);

            var config = CreateConfig();
            var providerIml = new SubscriptionProviderImpl(config.Object);

            ISubscriptionData captured = null;
            config.Setup(o => o.UpdateSubscriptionSettings(It.IsAny<ISubscriptionData>()))
                .Callback<ISubscriptionData>(d => captured = d);

            providerIml.SaveSubscriptionData(mockSubscriptionData.Object);

            Assert.IsNotNull(captured);
            Assert.AreEqual(providerIml.SubscriptionKey, captured.SubscriptionKey);
            Assert.AreEqual(providerIml.SubscriptionSiteName, captured.SubscriptionSiteName);
            Assert.AreNotEqual("attacker-supplied-key", captured.SubscriptionKey);
            Assert.AreNotEqual("attacker-supplied-site", captured.SubscriptionSiteName);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionProvider))]
        public void SubscriptionProvider_SetLicense_SetsSubscriptionKeyFromSuppliedData()
        {
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("caller-supplied-site-ignored");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("new-real-license-key");
            mockSubscriptionData.Setup(o => o.PlanId).Returns("enterprise");
            mockSubscriptionData.Setup(o => o.StopExecutions).Returns(false);
            mockSubscriptionData.Setup(o => o.Status).Returns(SubscriptionStatus.Active);
            mockSubscriptionData.Setup(o => o.CustomerId).Returns("cust-2");
            mockSubscriptionData.Setup(o => o.SubscriptionId).Returns("sub-2");
            mockSubscriptionData.Setup(o => o.MarketplaceResourceId).Returns(string.Empty);

            var config = CreateConfig();
            var providerIml = new SubscriptionProviderImpl(config.Object);

            ISubscriptionData captured = null;
            config.Setup(o => o.UpdateSubscriptionSettings(It.IsAny<ISubscriptionData>()))
                .Callback<ISubscriptionData>(d => captured = d);

            providerIml.SetLicense(mockSubscriptionData.Object);

            config.Verify(o => o.UpdateSubscriptionSettings(It.IsAny<ISubscriptionData>()), Times.Once);
            Assert.IsNotNull(captured);
            Assert.AreEqual("new-real-license-key", captured.SubscriptionKey);
            Assert.AreEqual("enterprise", captured.PlanId);
            Assert.AreEqual("cust-2", captured.CustomerId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionProvider))]
        public void SubscriptionProvider_SetLicense_StillPinsSubscriptionSiteNameToCurrentInstance()
        {
            var mockSubscriptionData = new Mock<ISubscriptionData>();
            mockSubscriptionData.Setup(o => o.SubscriptionSiteName).Returns("caller-supplied-site-must-be-ignored");
            mockSubscriptionData.Setup(o => o.SubscriptionKey).Returns("new-real-license-key");
            mockSubscriptionData.Setup(o => o.PlanId).Returns("enterprise");
            mockSubscriptionData.Setup(o => o.Status).Returns(SubscriptionStatus.Active);
            mockSubscriptionData.Setup(o => o.CustomerId).Returns("cust-3");
            mockSubscriptionData.Setup(o => o.SubscriptionId).Returns("sub-3");
            mockSubscriptionData.Setup(o => o.MarketplaceResourceId).Returns(string.Empty);

            var config = CreateConfig();
            var providerIml = new SubscriptionProviderImpl(config.Object);

            ISubscriptionData captured = null;
            config.Setup(o => o.UpdateSubscriptionSettings(It.IsAny<ISubscriptionData>()))
                .Callback<ISubscriptionData>(d => captured = d);

            providerIml.SetLicense(mockSubscriptionData.Object);

            Assert.IsNotNull(captured);
            Assert.AreEqual(providerIml.SubscriptionSiteName, captured.SubscriptionSiteName);
            Assert.AreNotEqual("caller-supplied-site-must-be-ignored", captured.SubscriptionSiteName);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SubscriptionProvider))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubscriptionProvider_SetLicense_WithNull_ThrowsArgumentNullException()
        {
            var config = CreateConfig();
            var providerIml = new SubscriptionProviderImpl(config.Object);

            providerIml.SetLicense(null);
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
            config.Setup(c => c.MarketplaceResourceId).Returns(string.Empty);
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