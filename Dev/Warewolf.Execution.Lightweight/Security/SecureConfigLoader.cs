/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using Dev2.Services.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Loads and decrypts the <c>secure.config</c> file from the server bin directory
    /// (<see cref="AppContext.BaseDirectory"/>) once at startup and exposes the result
    /// as a process-wide singleton via <see cref="Config"/>.
    ///
    /// File-not-found and decryption failures both yield <see cref="SecureConfigData.AllowAll"/>
    /// so that the engine keeps running in open-access mode rather than hard-failing.
    ///
    /// The config file format mirrors the Warewolf server: the content is AES-CBC–encrypted
    /// JSON (using <see cref="SecurityEncryption"/>) that deserialises to a
    /// <c>SecuritySettingsTO</c> containing the HMAC secret key and the list of
    /// <c>WindowsGroupPermission</c> entries.
    /// </summary>
    internal static class SecureConfigLoader
    {
        static readonly Lazy<SecureConfigData> _initial =
            new(Load, LazyThreadSafetyMode.ExecutionAndPublication);

        // Optional override that supersedes the lazy-initialised value once Reload() runs.
        // volatile guarantees concurrent readers always see the latest reference.
        static volatile SecureConfigData? _override;

        /// <summary>Returns the security config loaded at first access (thread-safe).</summary>
        internal static SecureConfigData Config => _override ?? _initial.Value;

        // ── Internal for unit tests ───────────────────────────────────────────

        /// <summary>
        /// Loads config from an explicit file path.  Used by tests to inject a
        /// specific config without touching the real <c>AppContext.BaseDirectory</c>.
        /// </summary>
        internal static SecureConfigData LoadFrom(string configPath) => ReadConfig(configPath);

        /// <summary>
        /// (POL-08) Re-reads the configured secure.config file and atomically
        /// replaces the cached instance so subsequent accesses to
        /// <see cref="Config"/> reflect the new state.
        /// </summary>
        internal static void Reload()
        {
            _override = Load();
        }

        // ── Implementation ────────────────────────────────────────────────────

        /// <summary>
        /// The name of the environment variable (Azure App Setting) that can override
        /// the default config path.  Set this to a full file path when mounting
        /// <c>secure.config</c> from an Azure File Share rather than deploying it with
        /// the package, e.g.:
        /// <code>
        ///   WAREWOLF_SECURE_CONFIG = /mnt/warewolf-config/secure.config
        /// </code>
        /// </summary>
        internal const string ConfigPathEnvVar    = "WAREWOLF_SECURE_CONFIG";

        /// <summary>
        /// Optional Microsoft Entra tenant ID.  When set, Entra tokens from other
        /// tenants are rejected.  Maps to the <c>tid</c> claim in the token payload.
        /// </summary>
        internal const string EntraTenantIdEnvVar = "WAREWOLF_ENTRA_TENANT_ID";

        /// <summary>
        /// Optional Entra application audience (client ID or <c>api://…</c> URI).
        /// When set, the token's <c>aud</c> claim must match exactly.
        /// </summary>
        internal const string EntraAudienceEnvVar  = "WAREWOLF_ENTRA_AUDIENCE";

        static SecureConfigData Load()
        {
            // 1. Explicit override via App Setting / environment variable.
            var envPath = Environment.GetEnvironmentVariable(ConfigPathEnvVar);
            if (!string.IsNullOrWhiteSpace(envPath))
                return ReadConfig(envPath);

            // 2. Deployed alongside the function app binaries (zip-deploy / local run).
            //    AppContext.BaseDirectory is the directory that contains the .exe / host.json.
            var binPath = Path.Combine(AppContext.BaseDirectory, "secure.config");
            return ReadConfig(binPath);
        }

        static SecureConfigData ReadConfig(string configPath)
        {
            var entraTenantId = Environment.GetEnvironmentVariable(EntraTenantIdEnvVar) ?? string.Empty;
            var entraAudience = Environment.GetEnvironmentVariable(EntraAudienceEnvVar)  ?? string.Empty;

            if (!File.Exists(configPath))
                return SecureConfigData.AllowAll;

            try
            {
                string encryptedData;
                using var stream = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                encryptedData = reader.ReadToEnd();

                var decrypted = SecurityEncryption.TryDecrypt(encryptedData);
                var settings  = JsonConvert.DeserializeObject<SecuritySettingsTO>(decrypted);

                if (settings is null)
                {
                    // File existed and decrypted cleanly, but deserialised to nothing — distinct
                    // from "no file" (line above) and worth knowing about on a live deployment,
                    // since it silently degrades this instance to open-access/deny-all depending
                    // on BYPASS_SECURE_CONFIG, indistinguishable from "config genuinely absent"
                    // without this log line.
                    Dev2Logger.Error(
                        $"SecureConfigLoader: secure.config at '{configPath}' decrypted but deserialised to null; falling back to AllowAll.",
                        LogExecutionId);
                    return SecureConfigData.AllowAll;
                }

                // If the config has no secret key yet, generate one so the engine can
                // still issue and validate JWT tokens consistently within this process.
                var secretKey = settings.SecretKey;
                if (string.IsNullOrEmpty(secretKey))
                {
                    using var hmac = new HMACSHA256();
                    secretKey = Convert.ToBase64String(hmac.Key);
                }

                var permissions = BuildPermissions(settings);
                var loginWorkflowName = settings.AuthenticationOverrideWorkflow?.Name ?? string.Empty;
                return new SecureConfigData(
                    isLoaded:          true,
                    secretKey:         secretKey,
                    permissions:       permissions,
                    entraTenantId:     entraTenantId,
                    entraAudience:     entraAudience,
                    loginWorkflowName: loginWorkflowName);
            }
            catch (Exception ex)
            {
                // Previously swallowed with no trace at all: a transient read (file locked mid-write,
                // FileShare.ReadWrite above notwithstanding) or decrypt/deserialize failure on ANY
                // one instance silently drops that instance to AllowAll (open-access if
                // BYPASS_SECURE_CONFIG=true, deny-all/503 otherwise) while sibling instances that
                // read the file fine keep enforcing normally — indistinguishable from a deliberately
                // absent config without this log line. On Azure Functions Consumption there is no
                // instance affinity, so two calls in the same logical session can land on different
                // instances in exactly this split state (observed against warewolfserver-mcp: a
                // create_workflow response with null httpEndpoints immediately followed by a
                // Public/{name} invoke that got a real permission-denied response).
                Dev2Logger.Error(
                    $"SecureConfigLoader: failed to read/decrypt secure.config at '{configPath}'; falling back to AllowAll.",
                    ex, LogExecutionId);
                return SecureConfigData.AllowAll;
            }
        }

        const string LogExecutionId = "SecureConfigLoader";

        static IReadOnlyList<PermissionEntry> BuildPermissions(SecuritySettingsTO settings)
        {
            if (settings.WindowsGroupPermissions is null or { Count: 0 })
                return Array.Empty<PermissionEntry>();

            var permissions = settings.WindowsGroupPermissions;

            // Mirror the full server's SecuritySettings.ProcessSettingsFile behavior:
            // ensure built-in Administrators and Public (Guests) groups always exist.
            // The server auto-adds these if missing so that admin users always have
            // access and the Public group is always available for anonymous resolution.
            var hasAdmin = permissions.Any(p =>
                p.IsServer &&
                string.Equals(p.WindowsGroup, WindowsGroupPermission.BuiltInAdministratorsText, StringComparison.OrdinalIgnoreCase));
            var hasGuests = permissions.Any(p =>
                p.IsServer &&
                string.Equals(p.WindowsGroup, WindowsGroupPermission.BuiltInGuestsText, StringComparison.OrdinalIgnoreCase));

            if (!hasAdmin || !hasGuests)
            {
                var mutable = permissions.ToList();
                if (!hasAdmin)
                    mutable.Insert(0, WindowsGroupPermission.CreateAdministrators());
                if (!hasGuests)
                    mutable.Add(WindowsGroupPermission.CreateGuests());
                permissions = mutable;
            }

            // Normalize legacy "BuiltIn\\Administrators" group name to the canonical form
            // (mirrors server's ProcessSettingsFile normalization).
            return permissions
                .Select(p => new PermissionEntry(
                    GroupName:    NormalizeGroupName(p.WindowsGroup ?? string.Empty),
                    IsGlobal:     p.IsServer && p.ResourceID == Guid.Empty,
                    ResourceName: !string.IsNullOrEmpty(p.ResourcePath) ? p.ResourcePath : (p.ResourceName ?? string.Empty),
                    View:         p.View,
                    Execute:      p.Execute,
                    Contribute:   p.Contribute,
                    DeployTo:     p.DeployTo,
                    DeployFrom:   p.DeployFrom,
                    Administrator: p.Administrator))
                .ToArray();
        }

        static string NormalizeGroupName(string groupName)
        {
            // The full server normalizes "BuiltIn\\Administrators" to "Warewolf Administrators"
            if (string.Equals(groupName, "BuiltIn\\Administrators", StringComparison.OrdinalIgnoreCase))
                return WindowsGroupPermission.BuiltInAdministratorsText;
            return groupName;
        }
    }
}
