/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Threading;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Mcp.Secrets;

/// <summary>
/// Resolves a named secret referenced by an MCP tool caller as <c>${NAME}</c> in a JSON request
/// (see <see cref="Warewolf.Execution.Lightweight.Mcp.ToolHandlers.AddSourceTool"/>) into its
/// real value, without the caller ever supplying — or the server ever echoing back — that value.
///
/// <para>
/// <b>Why this exists.</b> Two naive designs for keeping secrets out of MCP request/response JSON
/// both fail:
/// <list type="bullet">
///   <item>
///     Resolving <c>${NAME}</c> against the calling <i>client's</i> environment before the
///     request is sent doesn't help — the resolved literal secret still ends up in the JSON that
///     travels over the wire to this server (and typically also in the calling agent's own
///     conversation transcript). The exposure this is meant to prevent already happened.
///   </item>
///   <item>
///     Resolving purely against <i>this server's own</i> process environment variables only
///     works when whoever needs the secret available can cheaply add it there. On an Azure
///     Function App that means editing App Settings and restarting/redeploying — impractical
///     for a bot creating a source conversationally, and it doesn't scale to per-source secrets.
///   </item>
/// </list>
/// </para>
///
/// <para>
/// <b>The fix: indirection through a secret store the server already trusts.</b> In production
/// (<see cref="KeyVaultMcpSecretResolver"/>) <c>NAME</c> is a secret <i>name already staged in
/// Azure Key Vault</i> by whoever provisions the source (e.g. <c>az keyvault secret set</c> —
/// no Function App redeploy required) — the server fetches the value itself via its own Managed
/// Identity at save time. The literal secret never appears in the request JSON, the response, or
/// the calling agent's transcript; only the reference name does. In local development without
/// Key Vault configured (<see cref="EnvironmentMcpSecretResolver"/>), <c>NAME</c> falls back to
/// an environment variable on the machine actually running the server — legitimate there
/// specifically because the developer running the process is also the one who can trivially set
/// it, which is exactly the constraint that makes the naive approaches above unworkable in
/// production.
/// </para>
/// </summary>
internal interface IMcpSecretResolver
{
    /// <summary>
    /// Resolves <paramref name="name"/> to its secret value.
    /// </summary>
    /// <exception cref="ModelContextProtocol.McpException">
    /// Thrown when <paramref name="name"/> cannot be resolved, so an unresolved reference fails
    /// the tool call rather than silently persisting a literal <c>${NAME}</c> placeholder into
    /// the saved source.
    /// </exception>
    Task<string> ResolveAsync(string name, CancellationToken cancellationToken);
}
