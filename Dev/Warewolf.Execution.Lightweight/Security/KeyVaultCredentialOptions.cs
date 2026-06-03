/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Security;

/// <summary>
/// Credential selection options passed to <see cref="KeyVaultCredentialFactory"/>.
/// Decouples credential strategy from both the environment configuration and
/// the Key Vault client, making each component independently testable.
/// </summary>
public sealed record KeyVaultCredentialOptions
{
    /// <summary>
    /// <c>true</c> when the host is running locally or inside a development container
    /// (<c>AZURE_FUNCTIONS_ENVIRONMENT=Development</c> or <c>ASPNETCORE_ENVIRONMENT=Development</c>).
    /// Selects a focused developer credential chain instead of Managed Identity.
    /// </summary>
    internal bool IsDevelopment { get; init; }

    /// <summary>
    /// Azure AD tenant ID used in development to pin the credential chain to the
    /// correct tenant, preventing multi-tenant token ambiguity and MFA-loop failures.
    /// Source: <c>AZURE_TENANT_ID</c> environment variable.
    /// Ignored in production — Managed Identity has no configurable tenant.
    /// </summary>
    internal string? TenantId { get; init; }

    /// <summary>
    /// Client ID of a User-Assigned Managed Identity.
    /// When <c>null</c> or empty the System-Assigned Managed Identity is used.
    /// Only meaningful in production (<see cref="IsDevelopment"/> = <c>false</c>).
    /// Source: <c>AZURE_CLIENT_ID</c> environment variable.
    /// </summary>
    internal string? ManagedIdentityClientId { get; init; }
}
