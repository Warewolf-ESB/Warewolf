/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Text.Json;
using Dev2.Common;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// WOLF-8516: single JSON app setting replacing the 2 individual dev-only debug env vars
/// (<c>DEBUG_AZURE_KEYVAULT_SECRET</c>, <c>DEBUG_PRINCIPAL_TOKEN</c>), consumed by
/// <see cref="HostEnvironmentConfig.Load"/>. Dev-only and never set by any deploy script —
/// <c>local.settings.json</c> only. <b>Never set in production</b> — see
/// <see cref="HostEnvironmentConfig.DebugKeyVaultSecret"/> / <see cref="HostEnvironmentConfig.DebugPrincipalToken"/>,
/// both gated on <see cref="HostEnvironmentConfig.IsDevelopment"/> regardless of this var's
/// content.
///
/// <code>
///   WAREWOLF_DEBUG_CONFIG = {"keyVaultSecret":"...","principalToken":"..."}
/// </code>
/// </summary>
public sealed class DebugConfig
{
    public const string EnvVar = "WAREWOLF_DEBUG_CONFIG";

    public string KeyVaultSecret { get; init; }
    public string PrincipalToken { get; init; }

    static readonly DebugConfig Empty = new();

    public static DebugConfig FromEnvironment()
    {
        var raw = Environment.GetEnvironmentVariable(EnvVar);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Empty;
        }

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<DebugConfig>(raw, options) ?? Empty;
        }
        catch (JsonException ex)
        {
            Dev2Logger.Warn(
                $"DebugConfig failed to parse {EnvVar} — treating all fields as unset. " +
                $"ExceptionType={ex.GetType().Name}",
                "DebugConfig-FromEnvironment");
            return Empty;
        }
    }
}
