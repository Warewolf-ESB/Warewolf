/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Azure.Core;
using Azure.Identity;
using Dev2.Common;

namespace Warewolf.Execution.Lightweight.Security;

/// <summary>
/// Builds the minimal <see cref="TokenCredential"/> required for Azure Key Vault access.
///
/// Two distinct, non-overlapping paths are selected based on the runtime environment:
///
/// <list type="bullet">
///   <item>
///     <b>Production (Azure cloud):</b>
///     A single <see cref="ManagedIdentityCredential"/> — no credential chain, no
///     external process spawning, no environment-variable scanning.  Resolves in one
///     IMDS HTTP call (≈10 ms on Azure infrastructure).  Supports both
///     System-Assigned (no client ID) and User-Assigned
///     (<see cref="KeyVaultCredentialOptions.ManagedIdentityClientId"/> set) identities.
///   </item>
///   <item>
///     <b>Development / local container</b>
///     (<c>AZURE_FUNCTIONS_ENVIRONMENT=Development</c> or <c>ASPNETCORE_ENVIRONMENT=Development</c>):
///     A focused <see cref="ChainedTokenCredential"/> with exactly three providers,
///     tried in order of expected resolution speed:
///     <list type="number">
///       <item><see cref="EnvironmentCredential"/> — instant for service-principal / CI-pipeline auth
///             (<c>AZURE_CLIENT_ID</c> + <c>AZURE_CLIENT_SECRET</c> + <c>AZURE_TENANT_ID</c>);
///             also enables credential pass-through into local Docker containers.</item>
///       <item><see cref="AzureCliCredential"/> — primary path for developers using <c>az login</c>.</item>
///       <item><see cref="VisualStudioCredential"/> — fallback for Visual Studio IDE sign-in.</item>
///     </list>
///     Note: when <c>DEBUG_AZURE_KEYVAULT_SECRET</c> is set, the credential chain is never
///     invoked — the Key Vault call is bypassed entirely in development.
///     When <see cref="KeyVaultCredentialOptions.TenantId"/> is provided, all chain members
///     are pinned to that tenant, preventing MFA-loop failures caused by multi-tenant ambiguity.
///   </item>
/// </list>
/// </summary>
internal static class KeyVaultCredentialFactory
{
    /// <summary>
    /// Creates the optimal <see cref="TokenCredential"/> for the supplied options.
    /// </summary>
    internal static TokenCredential Create(KeyVaultCredentialOptions options)
    {
        const string executionId = "KeyVaultCredentialFactory";

        // Tenant and managed-identity client IDs are Debug-only — they identify the
        // deployment's Entra objects and must not reach production sinks.
        Dev2Logger.Info($"KeyVaultCredentialFactory Create starting. IsDevelopment: {options.IsDevelopment}", executionId);
        Dev2Logger.Debug($"KeyVaultCredentialFactory Create starting. IsDevelopment: {options.IsDevelopment}, TenantId: {options.TenantId ?? "(not set)"}, ManagedIdentityClientId: {options.ManagedIdentityClientId ?? "(not set)"}", executionId);

        try
        {
            var credential = options.IsDevelopment
                ? BuildDevelopmentChain(options.TenantId)
                : BuildCloudCredential(options.ManagedIdentityClientId);

            Dev2Logger.Info("KeyVaultCredentialFactory Create completed.", executionId);
            Dev2Logger.Debug($"KeyVaultCredentialFactory Create completed. Credential type: {credential.GetType().Name}", executionId);
            return credential;
        }
        catch (Exception ex)
        {
            Dev2Logger.Error($"KeyVaultCredentialFactory Create failed. ExceptionType={ex.GetType().Name}", executionId);
            Dev2Logger.Debug("KeyVaultCredentialFactory Create failure details.", ex, executionId);
            throw;
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Production path: single <see cref="ManagedIdentityCredential"/>, zero chain overhead.
    /// </summary>
    private static TokenCredential BuildCloudCredential(string? managedIdentityClientId)
    {
        const string executionId = "KeyVaultCredentialFactory-Cloud";

        if (string.IsNullOrWhiteSpace(managedIdentityClientId))
        {
            Dev2Logger.Info("KeyVaultCredentialFactory BuildCloudCredential using System-Assigned Managed Identity", executionId);
            return new ManagedIdentityCredential();
        }
        else
        {
            // Identity KIND at Info (the operational signal); the client ID is Debug-only.
            Dev2Logger.Info("KeyVaultCredentialFactory BuildCloudCredential using User-Assigned Managed Identity", executionId);
            Dev2Logger.Debug($"KeyVaultCredentialFactory BuildCloudCredential using User-Assigned Managed Identity. ClientId: {managedIdentityClientId}", executionId);
            return new ManagedIdentityCredential(
                ManagedIdentityId.FromUserAssignedClientId(managedIdentityClientId));
        }
    }

    /// <summary>
    /// Development path: focused chain of credential providers that cover every
    /// local developer workflow, tenant-pinned when a tenant ID is supplied.
    /// </summary>
    private static ChainedTokenCredential BuildDevelopmentChain(string? tenantId)
    {
        const string executionId = "KeyVaultCredentialFactory-Dev";

        Dev2Logger.Info("KeyVaultCredentialFactory BuildDevelopmentChain creating credential chain", executionId);
        Dev2Logger.Debug($"KeyVaultCredentialFactory BuildDevelopmentChain creating credential chain. TenantId: {tenantId ?? "(not set)"}", executionId);

        return new(
            // 0. Service-principal / container pass-through — resolves instantly when env vars present
            new EnvironmentCredential(),

            // 1. Azure CLI — most common local dev tool; pinning tenant avoids MFA re-auth loops
            new AzureCliCredential(new AzureCliCredentialOptions { TenantId = tenantId }),

            // 2. Visual Studio — fallback for IDE sign-in
            new VisualStudioCredential(new VisualStudioCredentialOptions { TenantId = tenantId })
        );
    }
}
