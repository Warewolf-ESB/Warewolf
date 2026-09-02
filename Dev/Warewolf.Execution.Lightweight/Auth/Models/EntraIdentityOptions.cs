/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Text.Json;
using Dev2.Common;

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// WOLF-8516: single JSON app setting replacing the 4 individual Entra identity env vars
/// (<c>WAREWOLF_ENTRA_TENANT_ID</c>, <c>WAREWOLF_ENTRA_AUDIENCE</c>, <c>WAREWOLF_ENTRA_CLIENT_ID</c>,
/// <c>WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE</c>), consumed by <see cref="EntraAuthOptions"/>,
/// <see cref="ServiceBusEntraAuthOptions"/>, and <c>Security.SecureConfigLoader</c> — all 3
/// previously read the individual env vars independently. Rotating the Entra app registration
/// now updates one setting instead of 3-4, atomically (no window where only some values have
/// been rotated).
///
/// <code>
///   WAREWOLF_ENTRA_CONFIG = {"tenantId":"...","audience":"...","clientId":"...","serviceBusAudience":"..."}
/// </code>
///
/// A missing/absent var, or a field missing from the JSON, yields <c>null</c> for that field —
/// identical to the previous "env var not set" behaviour. Malformed JSON is logged and treated
/// the same as an absent var (every consumer already tolerates all-null fields — see
/// <see cref="EntraAuthOptions.IsEnabled"/> / <see cref="ServiceBusEntraAuthOptions"/>'s
/// fail-closed doc comments) rather than failing startup.
/// </summary>
public sealed class EntraIdentityOptions
{
    public const string EnvVar = "WAREWOLF_ENTRA_CONFIG";

    public string TenantId { get; init; }
    public string Audience { get; init; }
    public string ClientId { get; init; }
    public string ServiceBusAudience { get; init; }

    static readonly EntraIdentityOptions Empty = new();

    public static EntraIdentityOptions FromEnvironment()
    {
        var raw = Environment.GetEnvironmentVariable(EnvVar);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Empty;
        }

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<EntraIdentityOptions>(raw, options) ?? Empty;
        }
        catch (JsonException ex)
        {
            Dev2Logger.Warn(
                $"EntraIdentityOptions failed to parse {EnvVar} — treating all fields as unset " +
                $"(Entra token validation fails closed). ExceptionType={ex.GetType().Name}",
                "EntraIdentityOptions-FromEnvironment");
            return Empty;
        }
    }
}
