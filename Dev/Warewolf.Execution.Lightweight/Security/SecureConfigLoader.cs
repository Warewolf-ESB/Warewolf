/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

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
        static readonly Lazy<SecureConfigData> _config =
            new(Load, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>Returns the security config loaded at first access (thread-safe).</summary>
        internal static SecureConfigData Config => _config.Value;

        // ── Internal for unit tests ───────────────────────────────────────────

        /// <summary>
        /// Loads config from an explicit file path.  Used by tests to inject a
        /// specific config without touching the real <c>AppContext.BaseDirectory</c>.
        /// </summary>
        internal static SecureConfigData LoadFrom(string configPath) => ReadConfig(configPath);

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
                    return SecureConfigData.AllowAll;

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
            catch
            {
                return SecureConfigData.AllowAll;
            }
        }

        static IReadOnlyList<PermissionEntry> BuildPermissions(SecuritySettingsTO settings)
        {
            if (settings.WindowsGroupPermissions is null or { Count: 0 })
                return Array.Empty<PermissionEntry>();

            return settings.WindowsGroupPermissions
                .Select(p => new PermissionEntry(
                    GroupName:    p.WindowsGroup  ?? string.Empty,
                    IsGlobal:     p.IsServer && p.ResourceID == Guid.Empty,
                    ResourceName: p.ResourceName  ?? string.Empty,
                    View:         p.View))
                .ToArray();
        }
    }
}
