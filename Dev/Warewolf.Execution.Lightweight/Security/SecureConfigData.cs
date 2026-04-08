/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Collections.Generic;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Immutable snapshot of the security configuration loaded from <c>secure.config</c>.
    ///
    /// When <see cref="IsLoaded"/> is <c>false</c> no config file was found on disk and
    /// the engine operates in open-access mode: all workflows are accessible via the
    /// Public endpoint and JWT validation is not enforced.
    /// </summary>
    internal sealed class SecureConfigData
    {
        /// <summary>
        /// Sentinel instance returned when no <c>secure.config</c> file exists.
        /// Signals open-access mode — allow all traffic through the public endpoint.
        /// </summary>
        internal static readonly SecureConfigData AllowAll =
            new(isLoaded: false, secretKey: string.Empty, permissions: []);

        internal SecureConfigData(
            bool isLoaded,
            string secretKey,
            IReadOnlyList<PermissionEntry> permissions)
        {
            IsLoaded    = isLoaded;
            SecretKey   = secretKey;
            Permissions = permissions;
        }

        /// <summary><c>true</c> when a <c>secure.config</c> was successfully decrypted and loaded.</summary>
        internal bool IsLoaded { get; }

        /// <summary>
        /// Base-64–encoded HMAC-SHA256 secret used to sign and validate JWT tokens.
        /// Empty string when <see cref="IsLoaded"/> is <c>false</c>.
        /// </summary>
        internal string SecretKey { get; }

        /// <summary>Flat list of permission entries read from the config file.</summary>
        internal IReadOnlyList<PermissionEntry> Permissions { get; }
    }
}
