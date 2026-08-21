/*
 * Corpus discovery and classification helpers for the round-trip fidelity gate
 * (see warewolf-lee-mcp-v3-spec.md's "bodyEditable gating" section and
 * warewolf-lee-mcp-v3-addendum-a.md's recommended next step).
 *
 * This file is intentionally decoupled from the test class so the corpus/classification
 * logic can be exercised or extended independently of the MSTest lifecycle.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    /// <summary>
    /// One row of the v3 spec's "Toolbox subset (v3)" table: a Studio-facing activity type the
    /// X6 converter pair supports, plus the raw XAML class-name substrings used to find real
    /// corpus samples containing it.
    /// </summary>
    public sealed class ToolboxEntry
    {
        public string StudioName { get; init; }
        public string Category { get; init; }
        public bool RequiresSource { get; init; }

        /// <summary>
        /// One or more literal class-name substrings (case-insensitive) that identify this
        /// activity type inside a raw (HTML-escaped) XamlDefinition string — e.g.
        /// "DsfDotNetMultiAssignActivity". Multiple entries cover legacy/current aliases
        /// (mirrors the "/"-separated dataType aliases in the v3 spec table).
        /// </summary>
        public string[] SearchTokens { get; init; }
    }

    /// <summary>
    /// A real (or synthetic) sample workflow file classified against one <see cref="ToolboxEntry"/>.
    /// </summary>
    public sealed record CorpusSample(string ToolboxStudioName, string FilePath);

    public static class RoundTripFidelityCorpus
    {
        /// <summary>
        /// The "Toolbox subset (v3)" table from warewolf-lee-mcp-v3-spec.md, condensed to the
        /// class-name substrings needed to find real corpus samples. Kept in one place so the
        /// fidelity gate and any future MCP `list_tools` implementation can be diffed against
        /// each other for drift.
        /// </summary>
        public static readonly IReadOnlyList<ToolboxEntry> ToolboxSubset = new List<ToolboxEntry>
        {
            new() { StudioName = "Assign", Category = "Data", SearchTokens = new[] { "DsfDotNetMultiAssignActivity" } },
            new() { StudioName = "Assign Object", Category = "Data", SearchTokens = new[] { "DsfDotNetMultiAssignObjectActivity" } },
            new() { StudioName = "Decision", Category = "Control Flow", SearchTokens = new[] { "DsfFlowDecisionActivity" } },
            new() { StudioName = "Decision (legacy)", Category = "Control Flow", SearchTokens = new[] { "DsfDecision" } },
            new() { StudioName = "Switch", Category = "Control Flow", SearchTokens = new[] { "DsfFlowSwitchActivity" } },
            new() { StudioName = "Sequence", Category = "Control Flow", SearchTokens = new[] { "DsfSequenceActivity" } },
            new() { StudioName = "For Each", Category = "Control Flow", SearchTokens = new[] { "DsfForEachActivity" } },
            new() { StudioName = "Select and apply", Category = "Control Flow", SearchTokens = new[] { "DsfSelectAndApplyActivity" } },
            new() { StudioName = "Gate", Category = "Control Flow", SearchTokens = new[] { "GateActivity" } },
            new() { StudioName = "Suspend Execution", Category = "Control Flow", SearchTokens = new[] { "SuspendExecutionActivity" } },
            new() { StudioName = "Manual Resumption", Category = "Control Flow", SearchTokens = new[] { "DsfManualResumptionActivity" } },
            new() { StudioName = "Service (sub-workflow)", Category = "Control Flow", RequiresSource = true, SearchTokens = new[] { "DsfWorkflowActivity" } },
            new() { StudioName = "Comment", Category = "Utility", SearchTokens = new[] { "DsfCommentActivity" } },
            new() { StudioName = "Calculate", Category = "Data", SearchTokens = new[] { "DsfDotNetCalculateActivity" } },
            new() { StudioName = "Aggregate Calculate", Category = "Data", SearchTokens = new[] { "DsfAggregateCalculateActivity", "DsfDotNetAggregateCalculateActivity" } },
            new() { StudioName = "Create JSON", Category = "Data", SearchTokens = new[] { "DsfCreateJsonActivity" } },
            new() { StudioName = "Data Merge", Category = "Data", SearchTokens = new[] { "DsfDataMergeActivity" } },
            new() { StudioName = "Data Split", Category = "Data", SearchTokens = new[] { "DsfDataSplitActivity" } },
            new() { StudioName = "Base Conversion", Category = "Data", SearchTokens = new[] { "DsfBaseConvertActivity" } },
            new() { StudioName = "Case Conversion", Category = "Data", SearchTokens = new[] { "DsfCaseConvertActivity" } },
            new() { StudioName = "Replace", Category = "Data", SearchTokens = new[] { "DsfReplaceActivity" } },
            new() { StudioName = "Find Index", Category = "Data", SearchTokens = new[] { "DsfIndexActivity" } },
            new() { StudioName = "Format Number", Category = "Data", SearchTokens = new[] { "DsfNumberFormatActivity" } },
            new() { StudioName = "Random", Category = "Data", SearchTokens = new[] { "DsfRandomActivity" } },
            new() { StudioName = "XPath", Category = "Data", SearchTokens = new[] { "DsfXPathActivity" } },
            new() { StudioName = "Find Records", Category = "Recordset", SearchTokens = new[] { "DsfFindRecordsMultipleCriteriaActivity" } },
            new() { StudioName = "Delete Records", Category = "Recordset", SearchTokens = new[] { "DsfDeleteRecordActivity", "DsfDeleteRecordNullHandlerActivity" } },
            new() { StudioName = "Sort Records", Category = "Recordset", SearchTokens = new[] { "DsfSortRecordsActivity" } },
            new() { StudioName = "Count Records", Category = "Recordset", SearchTokens = new[] { "DsfCountRecordsetNullHandlerActivity" } },
            new() { StudioName = "Length", Category = "Recordset", SearchTokens = new[] { "DsfRecordsetNullhandlerLengthActivity" } },
            new() { StudioName = "Unique Records", Category = "Recordset", SearchTokens = new[] { "DsfUniqueActivity" } },
            new() { StudioName = "Advanced Recordset", Category = "Recordset", SearchTokens = new[] { "AdvancedRecordsetActivity" } },
            new() { StudioName = "Read File", Category = "Files & Folders", SearchTokens = new[] { "DsfFileRead", "FileReadWithBase64" } },
            new() { StudioName = "Write File", Category = "Files & Folders", SearchTokens = new[] { "FileWriteWithBase64" } },
            new() { StudioName = "Folder Read", Category = "Files & Folders", SearchTokens = new[] { "DsfFolderReadActivity", "DsfFolderRead" } },
            new() { StudioName = "Create (path)", Category = "Files & Folders", SearchTokens = new[] { "DsfPathCreate" } },
            new() { StudioName = "Copy", Category = "Files & Folders", SearchTokens = new[] { "DsfPathCopy" } },
            new() { StudioName = "Move", Category = "Files & Folders", SearchTokens = new[] { "DsfPathMove" } },
            new() { StudioName = "Rename", Category = "Files & Folders", SearchTokens = new[] { "DsfPathRename" } },
            new() { StudioName = "Delete (path)", Category = "Files & Folders", SearchTokens = new[] { "DsfPathDelete" } },
            new() { StudioName = "Zip", Category = "Files & Folders", SearchTokens = new[] { "DsfZip" } },
            new() { StudioName = "UnZip", Category = "Files & Folders", SearchTokens = new[] { "DsfUnzip", "DsfUnZip" } },
            new() { StudioName = "GET Web Method", Category = "Integration", SearchTokens = new[] { "WebGetActivity" } },
            new() { StudioName = "POST Web Method", Category = "Integration", SearchTokens = new[] { "WebPostActivityNew" } },
            new() { StudioName = "PUT Web Method", Category = "Integration", SearchTokens = new[] { "WebPutActivity" } },
            new() { StudioName = "DELETE Web Method", Category = "Integration", SearchTokens = new[] { "DsfWebDeleteActivity" } },
            new() { StudioName = "Web Request", Category = "Integration", SearchTokens = new[] { "DsfWebGetRequestWithTimeoutActivity" } },
            new() { StudioName = "SQL Server Database", Category = "Database", RequiresSource = true, SearchTokens = new[] { "DsfSqlServerDatabaseActivity" } },
            new() { StudioName = "PostgreSQL Database", Category = "Database", RequiresSource = true, SearchTokens = new[] { "DsfPostgreSqlActivity" } },
            new() { StudioName = "MySQL Database", Category = "Database", RequiresSource = true, SearchTokens = new[] { "DsfMySqlDatabaseActivity" } },
            new() { StudioName = "Oracle Database", Category = "Database", RequiresSource = true, SearchTokens = new[] { "DsfOracleDatabaseActivity" } },
            new() { StudioName = "ODBC Database", Category = "Database", RequiresSource = true, SearchTokens = new[] { "DsfOdbcDatabaseActivity" } },
            new() { StudioName = "SQL Bulk Insert", Category = "Database", RequiresSource = true, SearchTokens = new[] { "DsfSqlBulkInsertActivity" } },
            new() { StudioName = "Redis Cache", Category = "Cache", RequiresSource = true, SearchTokens = new[] { "RedisCacheActivity" } },
            new() { StudioName = "Redis Remove", Category = "Cache", RequiresSource = true, SearchTokens = new[] { "RedisRemoveActivity" } },
            new() { StudioName = "RabbitMQ Publish", Category = "Communication", RequiresSource = true, SearchTokens = new[] { "DsfPublishRabbitMQActivity", "PublishRabbitMQActivity" } },
            new() { StudioName = "RabbitMQ Consume", Category = "Communication", RequiresSource = true, SearchTokens = new[] { "DsfConsumeRabbitMQActivity" } },
            new() { StudioName = "Send Email (SMTP)", Category = "Communication", RequiresSource = true, SearchTokens = new[] { "DsfSendEmailActivity" } },
            new() { StudioName = "Exchange Email", Category = "Communication", RequiresSource = true, SearchTokens = new[] { "DsfExchangeEmailNewActivity" } },
            new() { StudioName = "JavaScript", Category = "Scripting", SearchTokens = new[] { "DsfJavascriptActivity" } },
            new() { StudioName = "Ruby", Category = "Scripting", SearchTokens = new[] { "DsfRubyActivity" } },
            new() { StudioName = "Python", Category = "Scripting", SearchTokens = new[] { "DsfPythonActivity" } },
            new() { StudioName = "Execute Command Line", Category = "Scripting", SearchTokens = new[] { "DsfExecuteCommandLineActivity" } },
            new() { StudioName = "Date and Time", Category = "Date & Time", SearchTokens = new[] { "DsfDotNetDateTimeActivity", "DsfDateTimeActivity" } },
            new() { StudioName = "Date and Time Difference", Category = "Date & Time", SearchTokens = new[] { "DsfDateTimeDifferenceActivity", "DsfDotNetDateTimeDifferenceActivity" } },
            new() { StudioName = "Gather System Information", Category = "Utility", SearchTokens = new[] { "DsfGatherSystemInformationActivity", "DsfDotNetGatherSystemInformationActivity" } },
        };

        /// <summary>
        /// Repo-relative directories known to contain real, previously-authored `.bite` workflows
        /// (Studio and Web-Studio saved). Mirrors the "Resources*"/"Server Tests Setup" scan the
        /// addendum's residual-activity-list pass already did manually.
        /// </summary>
        static readonly string[] CorpusRoots =
        {
            "Resources - Release",
            "Resources - ServerTests",
            "Resources - Load",
            "Server Tests Setup",
            Path.Combine("Warewolf.Execution.Lightweight", "Resources"),
        };

        /// <summary>
        /// Walks parents of the test assembly's base directory looking for the repo root
        /// (identified by the presence of any <see cref="CorpusRoots"/> directory), mirroring
        /// <c>WorkflowExecutorEndToEndTests.FindHelloWorldBite</c>'s ancestor-walk pattern.
        /// </summary>
        public static string FindRepoDevRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (var hops = 0; dir != null && hops < 10; hops++, dir = dir.Parent)
            {
                foreach (var root in CorpusRoots)
                {
                    if (Directory.Exists(Path.Combine(dir.FullName, root)))
                    {
                        return dir.FullName;
                    }
                    if (Directory.Exists(Path.Combine(dir.FullName, "Dev", root)))
                    {
                        return Path.Combine(dir.FullName, "Dev");
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Enumerates every `.bite` file under the known corpus roots. Returns an empty list
        /// (never null/throws) when the repo root cannot be located, so callers degrade to
        /// Inconclusive rather than failing.
        /// </summary>
        public static List<string> DiscoverBiteFiles(string devRoot)
        {
            var results = new List<string>();
            if (string.IsNullOrEmpty(devRoot))
            {
                return results;
            }

            foreach (var root in CorpusRoots)
            {
                var full = Path.Combine(devRoot, root);
                if (!Directory.Exists(full))
                {
                    continue;
                }
                try
                {
                    results.AddRange(Directory.EnumerateFiles(full, "*.bite", SearchOption.AllDirectories));
                }
                catch (IOException)
                {
                    // Best-effort: skip roots that vanish/lock mid-enumeration.
                }
            }
            return results;
        }

        /// <summary>
        /// Classifies the discovered `.bite` corpus against <see cref="ToolboxSubset"/>, returning
        /// up to <paramref name="maxSamplesPerType"/> sample file paths per toolbox entry whose raw
        /// file content contains one of that entry's <see cref="ToolboxEntry.SearchTokens"/>.
        /// Uses a simple case-insensitive substring search against the raw (HTML-escaped) file text
        /// rather than a full XAML parse — the class names appear verbatim inside the escaped
        /// XamlDefinition, so this is a cheap, reliable-enough classifier for corpus discovery
        /// (not for the actual conversion, which always goes through the real converter).
        /// </summary>
        public static Dictionary<string, List<string>> ClassifyCorpus(IEnumerable<string> biteFiles, int maxSamplesPerType = 2)
        {
            var byToolboxName = ToolboxSubset.ToDictionary(t => t.StudioName, t => new List<string>());

            foreach (var file in biteFiles)
            {
                string content;
                try
                {
                    content = File.ReadAllText(file, Encoding.UTF8);
                }
                catch (IOException)
                {
                    continue;
                }

                foreach (var entry in ToolboxSubset)
                {
                    var bucket = byToolboxName[entry.StudioName];
                    if (bucket.Count >= maxSamplesPerType)
                    {
                        continue;
                    }
                    if (entry.SearchTokens.Any(token => content.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        bucket.Add(file);
                    }
                }
            }

            return byToolboxName;
        }
    }
}
