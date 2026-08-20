/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for SetLicenseTool (set_license): Administrator permission
 *  gating (whole-host operation, like set_var), partial-update semantics
 *  (omitted fields keep the current value), status-string validation,
 *  "${NAME}" subscriptionKey secret-reference resolution via
 *  IMcpSecretResolver (and that a literal key is still accepted as-is),
 *  that SubscriptionSiteName is never settable through this tool, and that
 *  the response never echoes the subscription key back.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Runtime.Subscription;
using Warewolf.Enums;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Licensing;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class SetLicenseToolTests
    {
        // ── Test doubles (matches SetVarToolTests'/AddSourceToolTests' conventions) ──

        private sealed class StubAuthPolicyLoader : IWorkflowAuthPolicyLoader
        {
            public bool IsConfigEffective { get; set; }
            public int PolicyCount => 0;
            public PolicyLookupResult GetPolicy(string workflowName) => PolicyLookupResult.ConfigMissing();

            public Func<string, IEnumerable<string>, WorkflowPermission> EffectivePermissions { get; set; } =
                (_, _) => WorkflowPermission.None;

            public WorkflowPermission GetEffectivePermissions(string workflowName, IEnumerable<string> callerRoles) =>
                EffectivePermissions(workflowName, callerRoles);

            public void Reload() { }
        }

        /// <summary>
        /// In-memory stand-in for the real static <c>SubscriptionProvider.Instance</c> singleton
        /// — lets these tests exercise SetLicenseTool's logic without touching real process-wide
        /// static state (see docs/SubscriptionConfig-IsolatedWorker-Resolution-Spec.md for why
        /// that state is a real file-backed singleton in production).
        /// </summary>
        private sealed class FakeSubscriptionProvider : ISubscriptionProvider
        {
            public string SubscriptionKey { get; private set; } = "existing-key";
            public string SubscriptionSiteName { get; private set; } = "existing-site";
            public string CustomerId { get; private set; } = "existing-customer";
            public string PlanId { get; private set; } = "existing-plan";
            public string SubscriptionId { get; private set; } = "existing-subscription";
            public string MarketplaceResourceId { get; private set; } = "existing-marketplace-id";
            public bool StopExecutions { get; private set; } = true;
            public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.NotActive;
            public bool IsLicensed { get; private set; }

            public int SetLicenseCallCount { get; private set; }
            public ISubscriptionData? LastSetLicenseData { get; private set; }

            public void SaveSubscriptionData(ISubscriptionData subscriptionData) =>
                throw new NotSupportedException("SetLicenseTool does not call SaveSubscriptionData.");

            public void SetLicense(ISubscriptionData subscriptionData)
            {
                SetLicenseCallCount++;
                LastSetLicenseData = subscriptionData;

                CustomerId = subscriptionData.CustomerId;
                PlanId = subscriptionData.PlanId;
                SubscriptionId = subscriptionData.SubscriptionId;
                MarketplaceResourceId = subscriptionData.MarketplaceResourceId;
                Status = subscriptionData.Status ?? SubscriptionStatus.NotActive;
                SubscriptionKey = subscriptionData.SubscriptionKey;
                StopExecutions = subscriptionData.StopExecutions;
                IsLicensed = subscriptionData.IsLicensed;
                // SubscriptionSiteName deliberately never assigned from subscriptionData here —
                // this fake mirrors the production SubscriptionProvider.SetLicense contract.
            }

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

        private sealed class FakeSecretResolver : IMcpSecretResolver
        {
            private readonly Dictionary<string, string> _secrets = new(StringComparer.Ordinal);

            public FakeSecretResolver With(string name, string value)
            {
                _secrets[name] = value;
                return this;
            }

            public Task<string> ResolveAsync(string name, CancellationToken cancellationToken) =>
                _secrets.TryGetValue(name, out var value)
                    ? Task.FromResult(value)
                    : throw new McpException($"Secret reference '${{{name}}}' could not be resolved: no secret named '{name}' is staged.");
        }

        private static WorkflowClaimsPrincipal Principal(params string[] roles) =>
            new(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice") }
                    .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
                "Bearer", ClaimTypes.Name, ClaimTypes.Role));

        static readonly StubAuthPolicyLoader OpenPolicy = new() { IsConfigEffective = false };
        static readonly FakeSecretResolver NoSecrets = new();

        private static Task<SetLicenseResult> Handle(
            ISubscriptionProvider subscriptionProvider,
            IWorkflowAuthPolicyLoader? authPolicyLoader = null,
            IMcpSecretResolver? secretResolver = null,
            ClaimsPrincipal? user = null,
            string? customerId = null,
            string? planId = null,
            string? subscriptionId = null,
            string? marketplaceResourceId = null,
            string? status = null,
            string? subscriptionKey = null,
            bool? stopExecutions = null) =>
            SetLicenseTool.Handle(
                authPolicyLoader ?? OpenPolicy, secretResolver ?? NoSecrets, subscriptionProvider, user,
                customerId, planId, subscriptionId, marketplaceResourceId, status, subscriptionKey, stopExecutions);

        // ── Tests: null guard ────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Handle_NullSubscriptionProvider_Throws()
        {
            await Handle(null!, status: "Active");
        }

        // ── Tests: permission gating ─────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecureConfigEffective_NoPermission_ThrowsPermissionDenied()
        {
            var loader = new StubAuthPolicyLoader { IsConfigEffective = true, EffectivePermissions = (_, _) => WorkflowPermission.None };

            await Assert.ThrowsExceptionAsync<McpException>(
                () => Handle(new FakeSubscriptionProvider(), loader, user: Principal("Developers"), status: "Active"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecureConfigEffective_ContributeOnly_ThrowsPermissionDenied()
        {
            // set_license requires Administrator specifically, like set_var — Contribute must not be enough.
            var loader = new StubAuthPolicyLoader { IsConfigEffective = true, EffectivePermissions = (_, _) => WorkflowPermission.Contribute };

            await Assert.ThrowsExceptionAsync<McpException>(
                () => Handle(new FakeSubscriptionProvider(), loader, user: Principal("Developers"), status: "Active"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecureConfigEffective_WithAdministratorPermission_Succeeds()
        {
            var loader = new StubAuthPolicyLoader { IsConfigEffective = true, EffectivePermissions = (_, _) => WorkflowPermission.Administrator };
            var provider = new FakeSubscriptionProvider();

            var result = await Handle(provider, loader, user: Principal("Admins"), status: "Active");

            Assert.IsTrue(result.IsLicensed);
            Assert.AreEqual(1, provider.SetLicenseCallCount);
        }

        // ── Tests: partial-update semantics ──────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_OmittedFields_KeepCurrentValues()
        {
            var provider = new FakeSubscriptionProvider();

            await Handle(provider, stopExecutions: false);

            Assert.AreEqual("existing-customer", provider.LastSetLicenseData!.CustomerId);
            Assert.AreEqual("existing-plan", provider.LastSetLicenseData.PlanId);
            Assert.AreEqual("existing-subscription", provider.LastSetLicenseData.SubscriptionId);
            Assert.AreEqual("existing-marketplace-id", provider.LastSetLicenseData.MarketplaceResourceId);
            Assert.AreEqual(SubscriptionStatus.NotActive, provider.LastSetLicenseData.Status);
            Assert.AreEqual("existing-key", provider.LastSetLicenseData.SubscriptionKey);
            Assert.IsFalse(provider.LastSetLicenseData.StopExecutions);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SuppliedFields_OverrideCurrentValues()
        {
            var provider = new FakeSubscriptionProvider();

            await Handle(provider, customerId: "new-customer", planId: "enterprise", subscriptionId: "sub-99",
                marketplaceResourceId: "mp-99", status: "Active", stopExecutions: true);

            Assert.AreEqual("new-customer", provider.LastSetLicenseData!.CustomerId);
            Assert.AreEqual("enterprise", provider.LastSetLicenseData.PlanId);
            Assert.AreEqual("sub-99", provider.LastSetLicenseData.SubscriptionId);
            Assert.AreEqual("mp-99", provider.LastSetLicenseData.MarketplaceResourceId);
            Assert.AreEqual(SubscriptionStatus.Active, provider.LastSetLicenseData.Status);
            Assert.IsTrue(provider.LastSetLicenseData.StopExecutions);
        }

        // ── Tests: status validation ──────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_InvalidStatus_ThrowsWithValidValuesListed()
        {
            var ex = await Assert.ThrowsExceptionAsync<McpException>(
                () => Handle(new FakeSubscriptionProvider(), status: "SuperActive"));

            StringAssert.Contains(ex.Message, "SuperActive");
            StringAssert.Contains(ex.Message, "Active");
            StringAssert.Contains(ex.Message, "NotActive");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_StatusIsCaseInsensitive()
        {
            var provider = new FakeSubscriptionProvider();

            var result = await Handle(provider, status: "active");

            Assert.AreEqual("Active", result.Status);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_OmittedStatus_KeepsCurrentStatus()
        {
            var provider = new FakeSubscriptionProvider();

            var result = await Handle(provider, customerId: "x");

            Assert.AreEqual(SubscriptionStatus.NotActive.ToString(), result.Status);
        }

        // ── Tests: subscriptionKey secret handling ───────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SubscriptionKeyPlaceholder_ResolvesViaSecretResolver()
        {
            var provider = new FakeSubscriptionProvider();
            var resolver = new FakeSecretResolver().With("license-key", "resolved-real-key-value");

            var result = await Handle(provider, secretResolver: resolver, subscriptionKey: "${license-key}", status: "Active");

            Assert.IsTrue(result.KeyChanged);
            Assert.AreEqual("resolved-real-key-value", provider.SubscriptionKey);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SubscriptionKeyPlaceholder_Unresolvable_Throws()
        {
            var provider = new FakeSubscriptionProvider();

            await Assert.ThrowsExceptionAsync<McpException>(
                () => Handle(provider, subscriptionKey: "${does-not-exist}", status: "Active"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_LiteralSubscriptionKey_IsStillAcceptedAsIs()
        {
            var provider = new FakeSubscriptionProvider();

            var result = await Handle(provider, subscriptionKey: "literal-key-no-placeholder", status: "Active");

            Assert.IsTrue(result.KeyChanged);
            Assert.AreEqual("literal-key-no-placeholder", provider.SubscriptionKey);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_OmittedSubscriptionKey_LeavesKeyUnchanged()
        {
            var provider = new FakeSubscriptionProvider();

            var result = await Handle(provider, status: "Active");

            Assert.IsFalse(result.KeyChanged);
            Assert.AreEqual("existing-key", provider.SubscriptionKey);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SubscriptionSiteName_IsNeverSettable()
        {
            // set_license exposes no subscriptionSiteName parameter at all — this asserts that
            // the provider's site name is untouched regardless of what else is set.
            var provider = new FakeSubscriptionProvider();

            await Handle(provider, customerId: "x", planId: "y", subscriptionId: "z", status: "Active", subscriptionKey: "new-key");

            Assert.AreEqual("existing-site", provider.SubscriptionSiteName);
        }

        // ── Tests: response shape ──────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Response_NeverContainsTheSubscriptionKey()
        {
            var provider = new FakeSubscriptionProvider();

            var result = await Handle(provider, subscriptionKey: "hunter2literal", status: "Active");

            var serialized = JsonSerializer.Serialize(result);
            StringAssert.DoesNotMatch(serialized, new System.Text.RegularExpressions.Regex("hunter2literal"));
        }
    }
}
