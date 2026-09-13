/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Azure;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Dev2.Common;
using ModelContextProtocol;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Mcp.Secrets;

/// <summary>
/// Production <see cref="IMcpSecretResolver"/>: resolves <c>${NAME}</c> references by fetching
/// the Key Vault secret named <c>NAME</c> from the same vault already configured for this host's
/// AES key material (see <see cref="Security.KeyVaultSecretManager"/>), using the same
/// <see cref="TokenCredential"/> (Managed Identity in Azure).
///
/// <para>
/// The secret must already exist in the vault — this resolver only ever performs a
/// <c>GET secrets/{name}</c>, never a write — so whoever wants a bot to be able to reference
/// <c>${my-db-password}</c> stages it once (e.g. <c>az keyvault secret set --name my-db-password
/// --value ...</c>), outside of, and prior to, any MCP <c>add_source</c> call. This is a one-time,
/// low-friction step (no Function App restart/redeploy, unlike an App Setting), and it means the
/// real secret value is never present in an MCP request, response, or calling agent's transcript
/// — only the reference name is.
/// </para>
/// </summary>
internal sealed class KeyVaultMcpSecretResolver : IMcpSecretResolver
{
    readonly SecretClient _client;

    public KeyVaultMcpSecretResolver(string vaultUri, TokenCredential credential)
    {
        if (string.IsNullOrWhiteSpace(vaultUri))
        {
            throw new ArgumentNullException(nameof(vaultUri));
        }

        _client = new SecretClient(new Uri(vaultUri), credential ?? throw new ArgumentNullException(nameof(credential)));
    }

    public async Task<string> ResolveAsync(string name, CancellationToken cancellationToken)
    {
        const string executionId = "KeyVaultMcpSecretResolver-Resolve";

        try
        {
            KeyVaultSecret secret = await _client.GetSecretAsync(name, version: null, cancellationToken).ConfigureAwait(false);
            return secret.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
        {
            Dev2Logger.Warn($"KeyVaultMcpSecretResolver secret '{name}' was not found in Key Vault.", executionId);
            throw new McpException(
                $"Secret reference '${{{name}}}' could not be resolved: no secret named '{name}' exists in this " +
                "host's configured Key Vault. Stage it first (e.g. `az keyvault secret set --name " +
                $"{name} --value ...`), then retry.");
        }
        catch (RequestFailedException ex)
        {
            Dev2Logger.Error($"KeyVaultMcpSecretResolver failed to fetch secret '{name}' from Key Vault.", ex, executionId);
            throw new McpException(
                $"Secret reference '${{{name}}}' could not be resolved: Key Vault request failed ({ex.Status}: {ex.Message}).");
        }
    }
}
