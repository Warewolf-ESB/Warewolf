/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for GetLicenseStatusTool (get_license_status): maps
 *  ISubscriptionProvider.GetSubscriptionData() onto the response shape, the
 *  null-provider guard, and that the response never includes the
 *  subscription key (schema-level check, matching SetLicenseTool's own
 *  guarantee).
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;
using Dev2.Runtime.Subscription;
using Warewolf.Enums;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Licensing;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class GetLicenseStatusToolTests
    {
        private sealed class FakeSubscriptionProvider : ISubscriptionProvider
        {
            public string SubscriptionKey { get; set; } = "should-never-appear-in-response";
            public string SubscriptionSiteName { get; set; } = "warewolf";
            public string CustomerId { get; set; } = "cust-1";
            public string PlanId { get; set; } = "enterprise";
            public string SubscriptionId { get; set; } = "sub-1";
            public string MarketplaceResourceId { get; set; } = string.Empty;
            public bool StopExecutions { get; set; }
            public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
            public bool IsLicensed { get; set; } = true;

            public void SaveSubscriptionData(ISubscriptionData subscriptionData) { }
            public void SetLicense(ISubscriptionData subscriptionData) { }

            public ISubscriptionData GetSubscriptionData() => new SubscriptionData
            {
                CustomerId = CustomerId,
                PlanId = PlanId,
                SubscriptionId = SubscriptionId,
                MarketplaceResourceId = MarketplaceResourceId,
                Status = Status,
                SubscriptionSiteName = SubscriptionSiteName,
                SubscriptionKey = SubscriptionKey,
                IsLicensed = IsLicensed,
                StopExecutions = StopExecutions,
            };

            public ISubscriptionData DefaultSubscription() => GetSubscriptionData();
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Handle_NullProvider_Throws()
        {
            GetLicenseStatusTool.Handle(null!);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_MapsSubscriptionDataFields()
        {
            var provider = new FakeSubscriptionProvider();

            var result = GetLicenseStatusTool.Handle(provider);

            Assert.IsTrue(result.IsLicensed);
            Assert.AreEqual("Active", result.Status);
            Assert.AreEqual("enterprise", result.PlanId);
            Assert.AreEqual("cust-1", result.CustomerId);
            Assert.AreEqual("sub-1", result.SubscriptionId);
            Assert.IsFalse(result.StopExecutions);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Response_NeverContainsTheSubscriptionKey()
        {
            var provider = new FakeSubscriptionProvider();

            var result = GetLicenseStatusTool.Handle(provider);

            var serialized = JsonSerializer.Serialize(result);
            StringAssert.DoesNotMatch(serialized, new System.Text.RegularExpressions.Regex("should-never-appear-in-response"));
        }
    }
}
