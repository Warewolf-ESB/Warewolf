/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.IO;
using ModelContextProtocol;

namespace Warewolf.Execution.Lightweight.Mcp;

/// <summary>
/// Persistence for workflow test definitions (the Lightweight-native "Service Tests" port — see
/// docs/WorkflowTestFramework-Plan.md §5/§7a.3). One JSON file per test case, under a directory
/// keyed by the owning workflow's relative name (extension-free, forward-slash — the same shape
/// every tool handler's own <c>relativePath</c> local already uses):
/// <code>{WorkflowsDirectory}/{relativePath}.tests/{TestName}.test.json</code>
///
/// <para>
/// Deliberately has NO in-memory cache, unlike <see cref="WorkflowIndex"/>'s <c>FrozenDictionary</c>
/// cache — <c>create_test</c>/<c>edit_test</c> each do at most one <see cref="Exists"/> check plus
/// one <see cref="Save"/>, and <c>execute_test</c> does exactly one <see cref="Load"/>; there is no
/// repeated-read hot path yet to amortise a cache against. Revisit once Phase 2's
/// <c>list_tests</c>/<c>get_test</c> add read pressure over a whole <c>.tests</c> directory.
/// </para>
/// </summary>
internal static class TestCatalog
{
    /// <summary>
    /// A test name is a single filename component, not a path — unlike every <c>name</c>-taking
    /// tool (<c>create_workflow</c>, <c>add_source</c>), which intentionally allow <c>/</c>-separated
    /// subpaths in <c>name</c>. Rejects <c>/</c>, <c>\</c>, <c>..</c>, and anything in
    /// <see cref="Path.GetInvalidFileNameChars"/>.
    /// </summary>
    internal static void ValidateTestName(string testName)
    {
        if (string.IsNullOrWhiteSpace(testName))
        {
            throw new McpException("`test.testName` is required.");
        }

        if (testName.Contains('/') || testName.Contains('\\') || testName.Contains("..") ||
            testName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new McpException(
                $"`test.testName` ('{testName}') must be a plain file name — it identifies a test within the workflow's .tests folder, not a path.");
        }
    }

    /// <summary>
    /// The directory holding every test for the workflow at <paramref name="workflowRelativePath"/>.
    /// </summary>
    internal static string TestsDirectoryFor(string workflowsDirectory, string workflowRelativePath) =>
        Path.Combine(workflowsDirectory, workflowRelativePath.Replace('/', Path.DirectorySeparatorChar) + ".tests");

    internal static string TestFilePath(string workflowsDirectory, string workflowRelativePath, string testName) =>
        Path.Combine(TestsDirectoryFor(workflowsDirectory, workflowRelativePath), testName + ".test.json");

    internal static bool Exists(string workflowsDirectory, string workflowRelativePath, string testName) =>
        File.Exists(TestFilePath(workflowsDirectory, workflowRelativePath, testName));

    internal static void Save(string workflowsDirectory, string workflowRelativePath, string testName, string jsonContents)
    {
        var directory = TestsDirectoryFor(workflowsDirectory, workflowRelativePath);
        Directory.CreateDirectory(directory);
        File.WriteAllText(TestFilePath(workflowsDirectory, workflowRelativePath, testName), jsonContents);
    }

    /// <summary>
    /// Reads the persisted test's raw JSON, or <c>null</c> when it does not exist. Added ahead of
    /// Phase 2's <c>get_test</c>/<c>list_tests</c> tools because <c>execute_test</c> (Phase 3) needs
    /// a read path now.
    /// </summary>
    internal static string? Load(string workflowsDirectory, string workflowRelativePath, string testName)
    {
        var path = TestFilePath(workflowsDirectory, workflowRelativePath, testName);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }
}
