/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth;

/// <summary>
/// Provides workflow authorisation policies derived from <c>secure.config</c>
/// <c>WindowsGroupPermissions</c> entries.
/// </summary>
public interface IWorkflowAuthPolicyLoader
{
    /// <summary>
    /// Returns the authorisation policy for the named workflow, or <c>null</c>
    /// when no matching permission entries exist in <c>secure.config</c>.
    /// </summary>
    WorkflowAuthPolicy? GetPolicy(string workflowName);

    /// <summary>Total number of distinct workflow policies available.</summary>
    int PolicyCount { get; }
}
