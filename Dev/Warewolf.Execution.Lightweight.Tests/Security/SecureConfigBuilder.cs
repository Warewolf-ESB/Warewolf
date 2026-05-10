/*
 * Test helper — NOT production code.
 * Builds, encrypts, and writes synthetic secure.config files for use in tests.
 *
 * The on-disk format mirrors what the full Warewolf server writes:
 *   SecurityEncryption.Encrypt( Dev2JsonSerializer.Serialize( SecuritySettingsTO ) )
 *
 * Dev2JsonSerializer uses TypeNameHandling.Objects so the JSON contains $type
 * annotations on interface-typed properties (e.g. AuthenticationOverrideWorkflow).
 * The deserialization path in SecuritySettings.ProcessSettingsFile uses plain
 * JsonConvert.DeserializeObject<SecuritySettingsTO> — Newtonsoft.Json populates
 * the existing NamedGuid instance created by the parameterless constructor, so the
 * round-trip works correctly.
 */

using Dev2.Common;
using Dev2.Services.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    /// <summary>
    /// Describes one permission entry for use with <see cref="SecureConfigBuilder"/>.
    /// </summary>
    public sealed record PermSpec(
        string GroupName,
        bool   IsServer,
        bool   View,
        bool   Execute      = false,
        Guid   ResourceId   = default,
        string ResourceName = "",
        string ResourcePath = "");

    /// <summary>
    /// Factory helpers for building and encrypting test <c>secure.config</c> files.
    /// </summary>
    public static class SecureConfigBuilder
    {
        // ── Well-known group names ────────────────────────────────────────────────

        public const string PublicGroup = "Public";
        public const string AdminGroup  = "Warewolf Administrators";

        // ── Config variant factories ──────────────────────────────────────────────

        /// <summary>
        /// Config where the built-in Public group has <b>global</b> View permission.
        /// Every workflow is visible on <c>/Public/</c> and a JWT user in any group
        /// that has View can also see everything.
        /// </summary>
        public static SecuritySettingsTO AllPublicGlobal(string secretKey) =>
            Build(secretKey,
                Admin(View: true),
                ServerPerm(PublicGroup, View: true));   // global = IsServer + Guid.Empty

        /// <summary>
        /// Config where the built-in Public group has <b>global Execute</b> (but no View).
        /// Workflows are not visible on <c>/Public/apis.json</c> but an authenticated
        /// user should still discover them via <c>/Secure/apis.json</c>.
        /// </summary>
        public static SecuritySettingsTO PublicExecuteGlobal(string secretKey) =>
            Build(secretKey,
                Admin(View: true),
                ServerPerm(PublicGroup, View: false, Execute: true));

        /// <summary>
        /// Config where the Public group has <b>no</b> View permission at all.
        /// Nothing is accessible without a JWT token.
        /// Mirrors the Warewolf server's out-of-the-box default after a fresh install.
        /// </summary>
        public static SecuritySettingsTO NoPublicAccess(string secretKey) =>
            Build(secretKey,
                Admin(View: true),
                ServerPerm(PublicGroup, View: false));

        /// <summary>
        /// Config where the Public group has View only on the named resources
        /// <paramref name="publicWorkflowNames"/>.  All other workflows are private.
        /// </summary>
        public static SecuritySettingsTO PartiallyPublic(string secretKey, params string[] publicWorkflowNames)
        {
            var perms = new List<PermSpec> { Admin(View: true), ServerPerm(PublicGroup, View: false) };
            foreach (var name in publicWorkflowNames)
                perms.Add(ResourcePerm(PublicGroup, name, View: true));
            return Build(secretKey, [.. perms]);
        }

        /// <summary>
        /// Config with group-scoped access: the Public group has no View; each entry
        /// in <paramref name="groupWorkflows"/> maps a group name to the resource names
        /// visible to members of that group.
        /// </summary>
        public static SecuritySettingsTO GroupBasedAccess(
            string secretKey,
            IEnumerable<(string group, string[] workflows)> groupWorkflows)
        {
            var perms = new List<PermSpec> { Admin(View: true), ServerPerm(PublicGroup, View: false) };
            foreach (var (group, workflows) in groupWorkflows)
                foreach (var wf in workflows)
                    perms.Add(ResourcePerm(group, wf, View: true));
            return Build(secretKey, [.. perms]);
        }

        // ── Serialise + encrypt ───────────────────────────────────────────────────

        /// <summary>
        /// Serialises <paramref name="settings"/> with <c>TypeNameHandling.Objects</c>
        /// (matching <c>Dev2JsonSerializer</c>) then encrypts with
        /// <see cref="SecurityEncryption.Encrypt"/>.
        /// </summary>
        public static string Encrypt(SecuritySettingsTO settings)
        {
            var json = JsonConvert.SerializeObject(settings, new JsonSerializerSettings
            {
                TypeNameHandling              = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            });
            return SecurityEncryption.Encrypt(json);
        }

        /// <summary>
        /// Writes an encrypted config to a temp file and returns the file path.
        /// The caller is responsible for deleting the file when finished.
        /// </summary>
        public static string WriteTempConfig(SecuritySettingsTO settings)
        {
            var path      = Path.GetTempFileName();
            var encrypted = Encrypt(settings);
            File.WriteAllText(path, encrypted);
            return path;
        }

        /// <summary>
        /// Generates a fresh, random Base64-encoded HMAC-SHA256 key suitable for use
        /// as the <c>SecretKey</c> field of a <see cref="SecuritySettingsTO"/>.
        /// </summary>
        public static string NewSecretKey()
        {
            using var hmac = new HMACSHA256();
            return Convert.ToBase64String(hmac.Key);
        }

        // ── Permission helpers ────────────────────────────────────────────────────

        /// <summary>Server-wide permission (no specific resource).</summary>
        public static PermSpec ServerPerm(string group, bool View,
            bool Execute     = false,
            bool Contribute  = false,
            bool Administrator = false) =>
            new(group, IsServer: true, View, Execute, ResourceId: Guid.Empty);

        /// <summary>Resource-specific permission.</summary>
        public static PermSpec ResourcePerm(string group, string resourceName, bool View,
            bool Execute = false) =>
            new(group, IsServer: false, View, Execute,
                ResourceId:   Guid.NewGuid(),
                ResourceName: resourceName);

        /// <summary>Default administrator entry (global, all permissions).</summary>
        public static PermSpec Admin(bool View = true) =>
            ServerPerm(AdminGroup, View, Execute: true, Contribute: true, Administrator: true);

        // ── Low-level builder ─────────────────────────────────────────────────────

        public static SecuritySettingsTO Build(string secretKey, params PermSpec[] specs)
        {
            var settings = new SecuritySettingsTO
            {
                SecretKey = secretKey,
            };

            foreach (var s in specs)
            {
                settings.WindowsGroupPermissions.Add(new WindowsGroupPermission
                {
                    WindowsGroup = s.GroupName,
                    IsServer     = s.IsServer,
                    ResourceID   = s.ResourceId,
                    ResourceName = s.ResourceName,
                    ResourcePath = s.ResourcePath,
                    View         = s.View,
                    Execute      = s.Execute,
                });
            }

            return settings;
        }
    }
}
