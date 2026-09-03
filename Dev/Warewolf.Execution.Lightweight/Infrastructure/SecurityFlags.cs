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
/// WOLF-8516: single JSON app setting replacing the 3 individual security kill-switch env vars
/// (<c>BYPASS_SECURE_CONFIG</c>, <c>WAREWOLF_SUPER_ADMIN_ENABLED</c>,
/// <c>SkipFailureToRetrieveSecret</c>), consumed by <c>Auth.WorkflowAuthPolicyLoader</c> (bypass
/// + super-admin) and <see cref="HostEnvironmentConfig"/> (skip-failure). An operator flips any
/// one of the three flags mid-incident with a single
/// <c>az functionapp config appsettings set --settings WAREWOLF_SECURITY_FLAGS='{"bypassSecureConfig":true}'</c>
/// instead of remembering three separate setting names.
///
/// <code>
///   WAREWOLF_SECURITY_FLAGS = {"bypassSecureConfig":false,"superAdminEnabled":false,"skipFailureToRetrieveSecret":false}
/// </code>
///
/// All fields default to <c>false</c> when absent — identical to the previous
/// "env var not set" behaviour, preserving the safe-by-default posture.
///
/// <b>Deliberately NOT cached</b> — <c>WorkflowAuthPolicyLoader.GetEffectivePermissions</c> and
/// <c>GetPolicy</c> re-parse this on every call so <c>superAdminEnabled</c>/<c>bypassSecureConfig</c>
/// keep their existing hot-toggle behaviour (flip the app setting, no restart needed). Do not
/// route this through <see cref="HostEnvironmentConfig"/>'s load-once-at-startup snapshot.
/// </summary>
public sealed class SecurityFlags
{
    public const string EnvVar = "WAREWOLF_SECURITY_FLAGS";

    public bool BypassSecureConfig { get; init; }
    public bool SuperAdminEnabled { get; init; }
    public bool SkipFailureToRetrieveSecret { get; init; }

    static readonly SecurityFlags AllFalse = new();

    public static SecurityFlags FromEnvironment()
    {
        var raw = Environment.GetEnvironmentVariable(EnvVar);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return AllFalse;
        }

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<SecurityFlags>(raw, options) ?? AllFalse;
        }
        catch (JsonException ex)
        {
            Dev2Logger.Warn(
                $"SecurityFlags failed to parse {EnvVar} — treating all flags as false " +
                $"(safe default). ExceptionType={ex.GetType().Name}",
                "SecurityFlags-FromEnvironment");
            return AllFalse;
        }
    }
}
