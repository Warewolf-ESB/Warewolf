/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using ModelContextProtocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Mcp.Secrets;

/// <summary>
/// Development-only <see cref="IMcpSecretResolver"/>: resolves <c>${NAME}</c> references against
/// this process's own environment variables. Registered only when Key Vault is not configured
/// (<see cref="Infrastructure.HostEnvironmentConfig.EncryptionEnabled"/> is <c>false</c>) — see
/// <see cref="IMcpSecretResolver"/> for why this is a safe fallback locally but not a substitute
/// for <see cref="KeyVaultMcpSecretResolver"/> in production.
/// </summary>
internal sealed class EnvironmentMcpSecretResolver : IMcpSecretResolver
{
    public Task<string> ResolveAsync(string name, CancellationToken cancellationToken)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (value is null)
        {
            throw new McpException(
                $"Secret reference '${{{name}}}' could not be resolved: no environment variable named '{name}' " +
                "is set on this MCP server host. (Key Vault is not configured for this host, so secret " +
                "references fall back to this host's own environment variables — set one before retrying, " +
                "or configure Key Vault for production-grade secret storage.)");
        }

        return Task.FromResult(value);
    }
}
