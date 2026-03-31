/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;

namespace Warewolf.Execution.Lightweight.Models
{
    /// <summary>
    /// Resource type parsed from the ResourceType attribute on the root &lt;Service&gt; element.
    /// Mirrors Warewolf's ResourceType enum — only the values relevant to the lightweight executor.
    /// </summary>
    internal enum WorkflowResourceType
    {
        Unknown,
        WorkflowService,
        DbService,
        PluginService,
        WebService,
        Server
    }

    /// <summary>
    /// Immutable metadata extracted from a Warewolf resource XML file.
    /// Only the root &lt;Service&gt; element attributes are read, so extraction
    /// is O(bytes-to-first-element) — no full-file parse.
    ///
    /// XML shape:
    ///   &lt;Service ID="…" ServerID="…" Name="…" ResourceType="…" …&gt;
    /// </summary>
    internal sealed record WorkflowResourceEntry(
        Guid   ResourceId,
        Guid   ServerId,
        string Name,
        WorkflowResourceType ResourceType,
        string FilePath)
    {
        /// <summary>Parse ResourceType string to enum, defaulting to Unknown.</summary>
        internal static WorkflowResourceType ParseResourceType(string? value) =>
            Enum.TryParse<WorkflowResourceType>(value, ignoreCase: true, out var rt)
                ? rt
                : WorkflowResourceType.Unknown;
    }
}
