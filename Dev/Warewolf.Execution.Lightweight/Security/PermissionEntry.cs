/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Minimal, immutable representation of a single permission entry extracted from
    /// a deserialized <c>secure.config</c> file.
    ///
    /// Mirrors the fields of <c>WindowsGroupPermission</c> that are relevant to the
    /// lightweight engine's access-control decisions, without pulling in the full
    /// WPF-bound observable model.
    /// </summary>
    internal sealed record PermissionEntry(
        /// <summary>Windows group or role name (e.g. "Public", "DOMAIN\\Users").</summary>
        string GroupName,

        /// <summary>
        /// <c>true</c> when the permission applies server-wide (no specific resource).
        /// Corresponds to <c>IsServer == true &amp;&amp; ResourceID == Guid.Empty</c>.
        /// </summary>
        bool IsGlobal,

        /// <summary>
        /// Name of the specific resource this permission targets, or <c>""</c> for
        /// global entries.  Populated from <c>WindowsGroupPermission.ResourceName</c>
        /// which is resolved by the Warewolf server when the config is saved.
        /// </summary>
        string ResourceName,

        /// <summary>Whether the group has View access to this resource.</summary>
        bool View,

        /// <summary>Whether the group can trigger workflow execution.</summary>
        bool Execute = false,

        /// <summary>Whether the group can create and modify workflow definitions.</summary>
        bool Contribute = false,

        /// <summary>Whether the group can deploy workflows to a target environment.</summary>
        bool DeployTo = false,

        /// <summary>Whether the group can pull workflow deployments from a source environment.</summary>
        bool DeployFrom = false,

        /// <summary>Whether the group has full administrative permission over all workflow operations.</summary>
        bool Administrator = false)
    {
        /// <summary>
        /// <c>true</c> when this entry represents the built-in public/anonymous group.
        /// Checks group name directly so it covers both server-level global permissions
        /// (<c>IsServer=true</c>) and resource-specific public permissions
        /// (<c>IsServer=false</c>), which would otherwise be missed if using
        /// <c>WindowsGroupPermission.IsBuiltInGuests</c> (that requires <c>IsServer</c>).
        /// </summary>
        internal bool IsPublicGroup =>
            string.Equals(GroupName, "Public", StringComparison.OrdinalIgnoreCase);
    }
}
