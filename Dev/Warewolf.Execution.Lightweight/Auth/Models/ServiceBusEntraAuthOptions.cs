/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// Dedicated, narrow-audience Entra options for tokens carried inside a Service Bus
/// message (the "trigger-via-service-bus" audience — see
/// <c>docs/ServiceBusSecureTrigger-Architecture.md</c> Section 4.1).
///
/// <para>
/// A distinct type (rather than a second instance of <see cref="EntraAuthOptions"/>) so it
/// can be registered as its own DI singleton alongside the HTTP-path
/// <see cref="EntraAuthOptions"/> without keyed-service registration. Deliberately does
/// <b>not</b> fall back to <c>WAREWOLF_ENTRA_AUDIENCE</c> / <c>WAREWOLF_ENTRA_CLIENT_ID</c> —
/// a token minted for the general HTTP audience must never validate against this dedicated
/// audience (confused-deputy / token-reuse prevention, spec §4.2 step 2).
/// </para>
/// </summary>
public sealed class ServiceBusEntraAuthOptions : EntraAuthOptions
{
    /// <summary>
    /// Reads <c>WAREWOLF_ENTRA_TENANT_ID</c> (shared tenant with the HTTP path) and the
    /// dedicated <c>WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE</c> audience. When the dedicated
    /// audience is not configured, <see cref="EntraAuthOptions.IsEnabled"/> is <c>false</c>
    /// and the Service Bus trigger dead-letters every message (fail closed).
    /// </summary>
    public static ServiceBusEntraAuthOptions FromEnvironment() => new()
    {
        TenantId = Environment.GetEnvironmentVariable("WAREWOLF_ENTRA_TENANT_ID"),
        Audience = Environment.GetEnvironmentVariable("WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE"),
    };
}
