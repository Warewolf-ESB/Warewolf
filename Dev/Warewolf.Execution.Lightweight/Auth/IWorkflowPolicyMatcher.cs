/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth;

/// <summary>
/// Evaluates whether a <see cref="WorkflowClaimsPrincipal"/> satisfies the
/// <see cref="WorkflowAuthPolicy"/> for a named workflow.
///
/// <para>
/// Keeping this logic in a dedicated class (rather than inline in middleware)
/// means the matching strategy can be changed, tested, or swapped without
/// touching middleware or HTTP function code.
/// </para>
///
/// <para>Evaluation rules (mirrors <c>secure.config</c> semantics):</para>
/// <list type="bullet">
///   <item>
///     <b>Group check — OR logic</b>: the caller must match at least one entry in
///     <see cref="WorkflowAuthPolicy.AllowedGroups"/>.  Matching is case-insensitive
///     and also compares the caller's <see cref="WorkflowClaimsPrincipal.UserName"/>
///     directly (for UPN-style entries such as <c>ashley.lewis@theunlimited.co.za</c>).
///   </item>
///   <item>
///     <b>Permission check — AND logic</b>: the matched group entry must hold ALL
///     flags in <paramref name="requiredPermissions"/>.
///   </item>
/// </list>
/// </summary>
public interface IWorkflowPolicyMatcher
{
    /// <summary>
    /// Evaluates whether <paramref name="principal"/> is allowed to execute
    /// <paramref name="workflowName"/> under the loaded policies.
    /// </summary>
    /// <param name="workflowName">
    /// Case-insensitive workflow name extracted from the request path.
    /// </param>
    /// <param name="principal">
    /// Authenticated caller built by the claims-principal middleware.
    /// </param>
    /// <param name="requiredPermissions">
    /// Minimum permission flags the caller must possess.
    /// Defaults to <see cref="WorkflowPermission.View"/> |
    /// <see cref="WorkflowPermission.Execute"/> when not supplied.
    /// </param>
    /// <returns>A <see cref="PolicyMatchResult"/> describing the outcome.</returns>
    PolicyMatchResult Evaluate(
        string                  workflowName,
        WorkflowClaimsPrincipal principal,
        WorkflowPermission      requiredPermissions = WorkflowPermission.View | WorkflowPermission.Execute);
}
