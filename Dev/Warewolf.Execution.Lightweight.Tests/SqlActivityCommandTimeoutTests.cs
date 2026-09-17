/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Guards the gap behind the 2026-09-16 20000-message ShovelBridge load test (build 30698):
 *  a DsfSqlServerDatabaseActivity with no CommandTimeout runs at SqlCommand's 30s default
 *  (see DatabaseServiceExecution.MssqlCreateCommand, which only assigns cmd.CommandTimeout
 *  when the activity supplies one). Profiler.usp_GetProfilerRecommendation timing out at ~31s
 *  dead-lettered two of that run's three lost messages, and a failed workflow is dead-lettered
 *  on the FIRST delivery - maxDeliveryCount never applies to it. The engine's database sits at
 *  100% CPU for the whole of a load run, so a proc silently shipping on the 30s default is the
 *  difference between a slow execution and a permanently lost message.
 *
 *  The misses were always partial: 202372b7f3 and 70a3fbc7d1 each bumped some resources and
 *  left their neighbours behind - including RabbitProcess2.bite, whose sibling
 *  RabbitProcess.bite was bumped in the same commit. Hence a guard over the whole Resources
 *  tree rather than the folders that happen to contain SQL activities today.
 */

using System.Net;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Execution.Lightweight.Tests;

[TestClass]
public class SqlActivityCommandTimeoutTests
{
    /// <summary>
    /// The floor every shipped SQL activity must meet, matching the value the profiler and
    /// rabbit resources already carry, so the whole tree stays on one budget rather than
    /// drifting resource by resource.
    /// </summary>
    private const int MinimumCommandTimeoutSeconds = 120;

    /// <summary>
    /// Resources that are deliberately left on SqlCommand's 30s default. Keep this list as
    /// short as the reasons justify - every entry is a workflow that will fail rather than
    /// wait when the database is under load.
    /// </summary>
    private static readonly string[] ExcludedFiles =
    {
        // A tool fidelity fixture (dbo.FidelityPing), not a production workflow. Its recorded
        // fidelity baseline covers the serialised activity, so changing an attribute on it
        // would move that baseline for no runtime benefit.
        "Fidelity_SqlServerDatabase.bite",
    };

    /// <summary>
    /// Matches the *element start tag* of a SQL activity, escaped (&lt;) or not, in either the
    /// outer .bite XML or the XAML payload it carries. The mandatory whitespace after the name
    /// is what keeps this off the child property elements
    /// (DsfSqlServerDatabaseActivity.AmbientDataList and friends).
    /// </summary>
    private static readonly Regex SqlActivityStartTag = new(
        @"(?:<|&lt;)(?:\w+:)?DsfSqlServerDatabaseActivity\s+(?<attrs>[^>]*?)/?(?:>|&gt;)",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex CommandTimeoutAttribute = new(
        @"CommandTimeout=""(?<value>[^""]*)""",
        RegexOptions.Compiled);

    private static readonly Regex ProcedureNameAttribute = new(
        @"ProcedureName=""(?<value>[^""]*)""",
        RegexOptions.Compiled);

    private static string EngineResourcesDirectory
    {
        get
        {
            var path = Path.Combine(AppContext.BaseDirectory, "EngineResources");
            Assert.IsTrue(
                Directory.Exists(path),
                $"The engine .bite resources were not found at '{path}' - are they still linked into the test project?");
            return path;
        }
    }

    private static List<(string File, string Procedure, string Attributes)> EnumerateSqlActivities()
    {
        var found = new List<(string, string, string)>();

        foreach (var file in Directory.EnumerateFiles(EngineResourcesDirectory, "*.bite", SearchOption.AllDirectories))
        {
            var name = Path.GetFileName(file);
            if (ExcludedFiles.Contains(name))
            {
                continue;
            }

            // The .bite files mix escaped and unescaped XAML, so decode once and match against
            // a single representation rather than covering both spellings in the regex.
            var text = WebUtility.HtmlDecode(File.ReadAllText(file));
            foreach (Match match in SqlActivityStartTag.Matches(text))
            {
                var attrs = match.Groups["attrs"].Value;
                var procedure = ProcedureNameAttribute.Match(attrs) is { Success: true } p
                    ? p.Groups["value"].Value
                    : "<unknown procedure>";
                found.Add((name, procedure, attrs));
            }
        }

        return found;
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void EngineResources_ContainSqlActivities_SoTheGuardsBelowAreNotVacuous()
    {
        // Without this, a regex or a link that silently stops matching would turn every
        // assertion in this class into a green test over an empty sequence.
        var found = EnumerateSqlActivities();

        Assert.IsTrue(
            found.Count >= 20,
            $"Expected at least 20 DsfSqlServerDatabaseActivity nodes across the engine resources but found {found.Count} - " +
            "either the resources are no longer linked into the test output or SqlActivityStartTag no longer matches them, " +
            "which would make the CommandTimeout guards vacuous.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void EverySqlActivity_DeclaresACommandTimeout()
    {
        var offenders = EnumerateSqlActivities()
            .Where(a =>
            {
                var match = CommandTimeoutAttribute.Match(a.Attributes);
                return !match.Success || !int.TryParse(match.Groups["value"].Value, out _);
            })
            .Select(a => $"{a.File} ({a.Procedure})")
            .Distinct()
            .ToList();

        Assert.AreEqual(
            0,
            offenders.Count,
            "These SQL activities have no numeric CommandTimeout, so they run at SqlCommand's 30s default and " +
            $"dead-letter under load: {string.Join(", ", offenders)}");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void EverySqlActivity_CommandTimeoutMeetsTheAgreedFloor()
    {
        var offenders = EnumerateSqlActivities()
            .Select(a => (a.File, a.Procedure, Match: CommandTimeoutAttribute.Match(a.Attributes)))
            .Where(x => x.Match.Success
                        && int.TryParse(x.Match.Groups["value"].Value, out var seconds)
                        && seconds < MinimumCommandTimeoutSeconds)
            .Select(x => $"{x.File} ({x.Procedure}, {x.Match.Groups["value"].Value}s)")
            .Distinct()
            .ToList();

        Assert.AreEqual(
            0,
            offenders.Count,
            $"These SQL activities are below the agreed {MinimumCommandTimeoutSeconds}s floor: {string.Join(", ", offenders)}");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ExcludedFiles_StillExist_SoTheListDoesNotRotSilently()
    {
        // An exclusion naming a file that has since been renamed or deleted looks like a
        // deliberate carve-out while actually guarding nothing.
        var present = Directory
            .EnumerateFiles(EngineResourcesDirectory, "*.bite", SearchOption.AllDirectories)
            .Select(Path.GetFileName)
            .ToHashSet();

        var stale = ExcludedFiles.Where(f => !present.Contains(f)).ToList();

        Assert.AreEqual(
            0,
            stale.Count,
            $"These entries in ExcludedFiles no longer name a shipped resource and should be removed: {string.Join(", ", stale)}");
    }
}
