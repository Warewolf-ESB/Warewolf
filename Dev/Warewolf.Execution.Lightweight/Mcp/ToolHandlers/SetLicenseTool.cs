/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Runtime.Subscription;
using ModelContextProtocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Enums;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Licensing;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>set_license</c> MCP tool: licenses (or re-licenses/updates the plan of) this
/// already-deployed instance over its own authenticated HTTP surface — no Kudu/VFS file access,
/// redeploy, or restart required.
///
/// <para>
/// <b>Why this exists alongside <see cref="LicensingHttpFunction"/>'s existing
/// <c>SaveSubscriptionData</c>/<c>POST /secure/Subscriptions</c> route.</b> That route (and the
/// <c>ISubscriptionProvider.SaveSubscriptionData</c> it calls) is the Chargebee plan/status-update
/// path: it deliberately keeps this instance's existing <c>SubscriptionKey</c>/
/// <c>SubscriptionSiteName</c> no matter what a caller passes, because a Chargebee webhook/plan
/// change must never be able to re-key or re-site an instance. That means it <b>cannot bootstrap a
/// never-licensed or broken-installation instance</b> (see
/// docs/SubscriptionConfig-IsolatedWorker-Resolution-Spec.md) — one with no valid
/// <c>SubscriptionKey</c> at all — since the one field that matters most for that can't be set.
/// This tool calls the new, additive <see cref="ISubscriptionProvider.SetLicense"/> instead, which
/// allows <c>SubscriptionKey</c> to be set explicitly while still pinning
/// <c>SubscriptionSiteName</c> to whatever this instance already has (never caller-settable,
/// through either method).
/// </para>
///
/// <para>
/// <b>The subscription key is a secret — never travels as literal JSON.</b> <c>subscriptionKey</c>
/// may be (and should be) a single <c>${NAME}</c> placeholder, resolved via
/// <see cref="IMcpSecretResolver"/> at call time — exactly the mechanism <see cref="AddSourceTool"/>
/// uses for password/secret-shaped connection fields (production: a secret already staged in this
/// host's Key Vault, fetched via its own Managed Identity; Key-Vault-less local dev: an environment
/// variable of that name on this host). The literal key never appears in the request, the response,
/// or the calling agent's transcript. Omit <c>subscriptionKey</c> entirely to leave the current key
/// unchanged (e.g. when only updating <c>status</c>/<c>planId</c>).
/// </para>
///
/// <para>
/// <b>Partial update.</b> Every field besides <c>status</c> defaults to "keep the current value"
/// when omitted — a caller flipping <c>StopExecutions</c> off, say, doesn't need to re-supply
/// every other field. <c>status</c> itself also defaults to the current value if omitted, but is
/// almost always the one field a caller changing anything about a license actually wants to set.
/// </para>
///
/// <para>
/// <b>Scope and permission.</b> Like <see cref="SetVarTool"/>, this is a whole-host operation, not
/// scoped to a single workflow/resource — it always resolves permission against the <i>global</i>
/// role map and requires <see cref="WorkflowPermission.Administrator"/>.
/// </para>
/// </summary>
internal static class SetLicenseTool
{
    internal const string ToolName = "set_license";

    /// <summary>
    /// Sentinel passed as the "workflow name" to <see cref="ListWorkflowsTool.HasPermission"/>,
    /// mirroring <see cref="SetVarTool.HostScopeName"/> — never collides with a real resource, so
    /// lookup always falls through to the global role map.
    /// </summary>
    const string HostScopeName = "$mcp/set_license";

    internal static async Task<SetLicenseResult> Handle(
        IWorkflowAuthPolicyLoader authPolicyLoader,
        IMcpSecretResolver secretResolver,
        ISubscriptionProvider subscriptionProvider,
        ClaimsPrincipal? user,
        [Description("Chargebee/customer identifier. Omit to keep the current value.")]
        string? customerId = null,
        [Description("Subscription plan id (e.g. 'developer', 'enterprise'). Omit to keep the current value.")]
        string? planId = null,
        [Description("Chargebee subscription id. Omit to keep the current value.")]
        string? subscriptionId = null,
        [Description("Azure Marketplace resource id, when licensed via Marketplace. Omit to keep the current value.")]
        string? marketplaceResourceId = null,
        [Description("License status. One of: NotActive, Future, InTrial, Active, NonRenewing, Paused, Cancelled. Omit to keep the current value.")]
        string? status = null,
        [Description("The license/subscription key. IMPORTANT: never put the literal key in this JSON — set this to " +
            "\"${secret-name}\" for a secret already staged in this host's Key Vault (e.g. via `az keyvault secret set`) " +
            "instead; the server fetches the real value itself and never echoes it back. (Only on a Key-Vault-less local " +
            "dev host does \"${NAME}\" instead fall back to that host's own environment variable NAME.) Omit entirely to " +
            "leave the current key unchanged.")]
        string? subscriptionKey = null,
        [Description("Whether workflow executions should be blocked despite an otherwise-licensed status. Omit to keep the current value.")]
        bool? stopExecutions = null,
        CancellationToken cancellationToken = default)
    {
        if (subscriptionProvider is null)
        {
            throw new ArgumentNullException(nameof(subscriptionProvider));
        }

        var principal = user as WorkflowClaimsPrincipal;
        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, HostScopeName, WorkflowPermission.Administrator))
        {
            throw new McpException("You do not have permission to change this server's license. This requires the Administrator permission.");
        }

        var current = subscriptionProvider.GetSubscriptionData();

        var resolvedStatus = current.Status ?? SubscriptionStatus.NotActive;
        if (!string.IsNullOrWhiteSpace(status) && !Enum.TryParse(status, ignoreCase: true, out resolvedStatus))
        {
            throw new McpException(
                $"`status` '{status}' is not a recognised license status. Use one of: {string.Join(", ", Enum.GetNames(typeof(SubscriptionStatus)))}.");
        }

        var resolvedKey = current.SubscriptionKey;
        var keyChanged = false;
        if (!string.IsNullOrWhiteSpace(subscriptionKey))
        {
            try
            {
                resolvedKey = await AddSourceTool.ResolveSecretPlaceholdersAsync(
                    subscriptionKey, "subscriptionKey", secretResolver, new List<string>(), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (McpException ex)
            {
                throw new McpException($"`subscriptionKey`: {ex.Message}");
            }

            keyChanged = true;
        }

        var isLicensed = resolvedStatus == SubscriptionStatus.Active || resolvedStatus == SubscriptionStatus.InTrial;

        var newData = new SubscriptionData
        {
            CustomerId = customerId ?? current.CustomerId,
            PlanId = planId ?? current.PlanId,
            SubscriptionId = subscriptionId ?? current.SubscriptionId,
            MarketplaceResourceId = marketplaceResourceId ?? current.MarketplaceResourceId,
            Status = resolvedStatus,
            SubscriptionKey = resolvedKey,
            StopExecutions = stopExecutions ?? current.StopExecutions,
            IsLicensed = isLicensed
        };

        subscriptionProvider.SetLicense(newData);

        return new SetLicenseResult(
            isLicensed,
            resolvedStatus.ToString(),
            newData.PlanId,
            newData.StopExecutions,
            keyChanged);
    }
}

/// <summary>
/// The full <c>set_license</c> response payload. Never includes the subscription key.
/// </summary>
internal sealed record SetLicenseResult(
    [property: JsonPropertyName("isLicensed")] bool IsLicensed,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("stopExecutions")] bool StopExecutions,
    [property: JsonPropertyName("keyChanged")] bool KeyChanged);
