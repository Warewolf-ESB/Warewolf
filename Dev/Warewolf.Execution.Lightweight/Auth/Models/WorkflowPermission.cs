/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// Fine-grained permission flags that map 1-to-1 with the boolean fields in
/// <c>secure.config</c> (<see cref="Security.PermissionEntry"/>).
/// Multiple flags are combined with bitwise OR; AND logic is used when checking
/// that a caller holds ALL required flags.
/// </summary>
[Flags]
public enum WorkflowPermission
{
    /// <summary>No permissions.</summary>
    None          = 0,

    /// <summary>Can view workflow outputs and execution status.</summary>
    View          = 1 << 0,

    /// <summary>Can trigger workflow execution.</summary>
    Execute       = 1 << 1,

    /// <summary>Can create and modify workflow definitions.</summary>
    Contribute    = 1 << 2,

    /// <summary>Can deploy workflows to a target environment.</summary>
    DeployTo      = 1 << 3,

    /// <summary>Can pull workflow deployments from a source environment.</summary>
    DeployFrom    = 1 << 4,

    /// <summary>Full permission over all workflow operations.</summary>
    Administrator = 1 << 5,

    /// <summary>All permissions combined.</summary>
    All           = View | Execute | Contribute | DeployTo | DeployFrom | Administrator,
}
