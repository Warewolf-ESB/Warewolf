/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.IO;
using System.Text.Json;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests.TestSupport;

/// <summary>
/// Shared helper for Mcp tool-handler tests: builds a <see cref="HostEnvironmentConfig"/> whose
/// <see cref="HostEnvironmentConfig.WorkflowsDirectory"/> is an arbitrary test-owned directory.
///
/// WOLF-8516 removed <c>HostEnvironmentConfig.Load()</c>'s <c>WorkflowsDirectory</c> environment
/// variable fallback — it now comes solely from a deploy-bundled
/// <c>Settings/executionengine.settings.json</c> file (see <see cref="HostEnvironmentConfig.Load"/>'s
/// <c>settingsDirectory</c> overload). This writes that file into a throwaway settings directory,
/// loads the config, then deletes the settings directory immediately — the returned config is an
/// immutable snapshot, so the file need not persist. Unlike the old env-var approach this mutates no
/// process-wide state, so it is safe under parallel test execution.
/// </summary>
internal static class McpToolTestHostConfig
{
    internal static HostEnvironmentConfig ForWorkflowsDirectory(string workflowsDirectory)
    {
        var settingsDir = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "mcp-hostconfig-" + Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            File.WriteAllText(
                Path.Combine(settingsDir, HostEnvironmentConfig.SettingsFileName),
                JsonSerializer.Serialize(new { workflowsDirectory }));
            return HostEnvironmentConfig.Load(settingsDir);
        }
        finally
        {
            Directory.Delete(settingsDir, recursive: true);
        }
    }
}
