/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Runtime.Subscription;
using System;
using System.Text.Json.Serialization;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>get_license_status</c> MCP tool: a read-only view of this instance's locally
/// persisted license/subscription state, over the same authenticated <c>/mcp-api/*</c> surface as
/// every other MCP tool — no Chargebee call is made (mirrors
/// <see cref="LicensingHttpFunction.IsLicensed"/>'s own "fast local check" behaviour), and no
/// special permission beyond the baseline <c>/mcp-api/*</c> authentication gate is required, since
/// <see cref="LicensingHttpFunction.IsLicensed"/> itself is anonymous. Exists mainly so an MCP
/// caller driving <see cref="SetLicenseTool"/> can confirm the resulting state without needing a
/// separate raw HTTP call to <c>GET /IsLicensed</c>.
/// </summary>
internal static class GetLicenseStatusTool
{
    internal const string ToolName = "get_license_status";

    internal static GetLicenseStatusResult Handle(ISubscriptionProvider subscriptionProvider)
    {
        if (subscriptionProvider is null)
        {
            throw new ArgumentNullException(nameof(subscriptionProvider));
        }

        var data = subscriptionProvider.GetSubscriptionData();
        return new GetLicenseStatusResult(
            data.IsLicensed,
            data.Status?.ToString(),
            data.PlanId,
            data.CustomerId,
            data.SubscriptionId,
            data.StopExecutions);
    }
}

/// <summary>The full <c>get_license_status</c> response payload. Never includes the subscription key.</summary>
internal sealed record GetLicenseStatusResult(
    [property: JsonPropertyName("isLicensed")] bool IsLicensed,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("customerId")] string CustomerId,
    [property: JsonPropertyName("subscriptionId")] string SubscriptionId,
    [property: JsonPropertyName("stopExecutions")] bool StopExecutions);
