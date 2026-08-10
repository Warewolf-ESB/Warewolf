/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Warewolf.Execution.Lightweight.Mcp;

/// <summary>
/// The static "Toolbox subset (v3)" table from <c>warewolf-lee-mcp-v3-spec.md</c>'s
/// <c>list_tools</c> section — every activity type the converter pair in
/// <c>Dev/Dev2.Activities/WorkflowConverters/</c> can place inside a workflow body via
/// <c>create_workflow</c>/<c>edit_workflow</c>/<c>add_step</c>.
///
/// <para>
/// Each row was cross-checked directly against
/// <see cref="!:X6ToWorkflowConverter.CreateActivityFromNode"/>'s switch (not just the spec
/// table) and each activity's own <c>ToX6Json(Cell)</c> method, which always writes
/// <c>cell.data["type"] = Constants.&lt;NAME&gt;.ToLower()</c> — i.e. <see cref="DataTypes"/>
/// below are the literal, lower-cased values this server will actually see in a workflow's
/// converted body, not names inferred indirectly.
/// </para>
///
/// <para>
/// <b>Known gap (verified, not a copy of the spec's own claim): "Calculate".</b> The spec
/// table lists <c>dsfdotnetcalculateactivity</c> as supported. <c>Dev2.Common.X6.Constants
/// .DSFDOTNETCALCULATEACTIVITY</c> and a <c>DsfDotNetCalculateActivity.ToX6Json</c> exist, and
/// there's an <c>X6ToWorkflowConverter_CalculateActivityHelper.CreateCalculateActivity</c> —
/// but that helper is <b>never called</b> from <c>CreateActivityFromNode</c>'s switch (only
/// "Aggregate Calculate" is wired in). A "Calculate" node fed back through
/// <c>X6JsonToWorkflow</c> today falls through to the <c>default</c> case and throws
/// <c>UnsupportedActivityTypeException</c>. Kept in this catalog anyway (per explicit product
/// decision) to match the spec table literally; <see cref="Entry.ActivityType"/> is still
/// accurate (it names the class the XAML-writer already emits), it is simply not yet
/// reachable from the JSON-writer direction. <c>validate_workflow</c>/<c>create_workflow</c>/
/// <c>edit_workflow</c> will surface the real converter exception if a caller ever tries to
/// use it — this catalog does not paper over that.
/// </para>
/// </summary>
internal static class ToolCatalog
{
    /// <summary>
    /// One toolbox entry, per <c>list_tools</c>' output shape. <see cref="DataTypes"/> holds
    /// every alias the converter's switch accepts for this Studio name (e.g. Switch accepts
    /// both <c>dsfflowswitchactivity</c> and <c>flowswitch</c>); <see cref="Resolve"/> matches
    /// against all of them.
    /// </summary>
    internal sealed record Entry(
        string Name,
        string ActivityType,
        IReadOnlyList<string> DataTypes,
        string Category,
        string Description,
        bool RequiresSource);

    /// <summary>
    /// The full catalog, in the spec table's own order. 66 entries — the spec's ~65 plus the
    /// one verified-unreachable "Calculate" row kept per explicit decision (see class remarks).
    /// </summary>
    internal static readonly IReadOnlyList<Entry> Entries = new List<Entry>
    {
        new("Assign", "DsfDotNetMultiAssignActivity", new[] { "dsfdotnetmultiassignactivity" }, "Data",
            "Assigns literal or expression values to one or more variables.", false),
        new("Assign Object", "DsfDotNetMultiAssignObjectActivity", new[] { "dsfdotnetmultiassignobjectactivity" }, "Data",
            "Assigns a JSON object literal or expression to a JSON-typed variable.", false),
        new("Decision", "DsfFlowDecisionActivity", new[] { "flowdecision" }, "Control Flow",
            "Branches execution based on a true/false condition.", false),
        new("Decision (legacy)", "DsfDecision", new[] { "dsfdecision" }, "Control Flow",
            "Legacy true/false branch activity, superseded by Decision.", false),
        new("Switch", "DsfFlowSwitchActivity", new[] { "dsfflowswitchactivity", "flowswitch" }, "Control Flow",
            "Branches execution across multiple labeled cases.", false),
        new("Sequence", "DsfSequenceActivity", new[] { "dsfsequenceactivity" }, "Control Flow",
            "Groups a nested chain of activities to run in order.", false),
        new("For Each", "DsfForEachActivity", new[] { "dsfforeachactivity", "foreach" }, "Control Flow",
            "Repeats a nested chain of activities once per recordset row (or a fixed count).", false),
        new("Select and apply", "DsfSelectAndApplyActivity", new[] { "dsfselectandapplyactivity" }, "Control Flow",
            "Selects recordset rows matching a condition and applies a nested chain to each.", false),
        new("Gate", "GateActivity", new[] { "gateactivity" }, "Control Flow",
            "Blocks execution until a condition becomes true.", false),
        new("Suspend Execution", "SuspendExecutionActivity", new[] { "suspendexecutionactivity" }, "Control Flow",
            "Suspends the workflow until externally resumed.", false),
        new("Manual Resumption", "ManualResumptionActivity", new[] { "dsfmanualresumptionactivity" }, "Control Flow",
            "Waits for an explicit manual-resume call before continuing.", false),
        new("Service (sub-workflow)", "DsfWorkflowActivity", new[] { "dsfworkflowactivity" }, "Control Flow",
            "Invokes another workflow or service by name.", true),
        new("Comment", "DsfCommentActivity", new[] { "dsfcommentactivity" }, "Utility",
            "A non-executing annotation node.", false),
        new("Calculate", "DsfDotNetCalculateActivity", new[] { "dsfdotnetcalculateactivity" }, "Data",
            "Evaluates a single arithmetic/formula expression. Not yet reachable via X6JsonToWorkflow — see class remarks.", false),
        new("Aggregate Calculate", "DsfDotNetAggregateCalculateActivity", new[] { "dsfaggregatecalculateactivity", "dsfdotnetaggregatecalculateactivity" }, "Data",
            "Evaluates an aggregate expression (sum/avg/etc.) over a recordset column.", false),
        new("Create JSON", "DsfCreateJsonActivity", new[] { "dsfcreatejsonactivity" }, "Data",
            "Builds a JSON object from a set of name/value mappings.", false),
        new("Data Merge", "DsfDataMergeActivity", new[] { "dsfdatamergeactivity" }, "Data",
            "Merges values into a templated string.", false),
        new("Data Split", "DsfDataSplitActivity", new[] { "dsfdatasplitactivity" }, "Data",
            "Splits a string into a recordset by delimiter/index/character rules.", false),
        new("Base Conversion", "DsfBaseConvertActivity", new[] { "dsfbaseconvertactivity" }, "Data",
            "Converts a value's text encoding/base (e.g. Base64, hex).", false),
        new("Case Conversion", "DsfCaseConvertActivity", new[] { "dsfcaseconvertactivity" }, "Data",
            "Converts text to upper/lower/proper case.", false),
        new("Replace", "DsfReplaceActivity", new[] { "dsfreplaceactivity" }, "Data",
            "Finds and replaces text within a value.", false),
        new("Find Index", "DsfIndexActivity", new[] { "dsfindexactivity" }, "Data",
            "Finds the index of a substring or recordset match.", false),
        new("Format Number", "DsfNumberFormatActivity", new[] { "dsfnumberformatactivity" }, "Data",
            "Formats a numeric value (decimal places, rounding).", false),
        new("Random", "DsfRandomActivity", new[] { "dsfrandomactivity" }, "Data",
            "Generates a random number or GUID.", false),
        new("XPath", "DsfXPathActivity", new[] { "dsfxpathactivity" }, "Data",
            "Evaluates an XPath expression against XML input.", false),
        new("Find Records", "DsfFindRecordsMultipleCriteriaActivity", new[] { "dsffindrecordsmultiplecriteriaactivity" }, "Recordset",
            "Finds recordset rows matching one or more criteria.", false),
        new("Delete Records", "DsfDeleteRecordActivity", new[] { "dsfdeleterecordactivity", "dsfdeleterecordnullhandleractivity" }, "Recordset",
            "Deletes rows from a recordset.", false),
        new("Sort Records", "DsfSortRecordsActivity", new[] { "dsfsortrecordsactivity" }, "Recordset",
            "Sorts a recordset by one or more fields.", false),
        new("Count Records", "DsfCountRecordsetNullHandlerActivity", new[] { "dsfcountrecordsetnullhandleractivity" }, "Recordset",
            "Counts the rows in a recordset.", false),
        new("Length", "DsfRecordsetNullhandlerLengthActivity", new[] { "dsfrecordsetnullhandlerlengthactivity" }, "Recordset",
            "Returns the length of a string or recordset field.", false),
        new("Unique Records", "DsfUniqueActivity", new[] { "dsfuniqueactivity" }, "Recordset",
            "Removes duplicate rows from a recordset.", false),
        new("Advanced Recordset", "AdvancedRecordsetActivity", new[] { "advancedrecordsetactivity" }, "Recordset",
            "Performs advanced recordset filtering/projection operations.", false),
        new("Read File", "DsfFileRead", new[] { "dsffileread", "filereadwithbase64" }, "Files & Folders",
            "Reads a file's contents (optionally as Base64).", false),
        new("Write File", "FileWriteActivity", new[] { "filewritewithbase64" }, "Files & Folders",
            "Writes content to a file.", false),
        new("Folder Read", "DsfFolderReadActivity", new[] { "dsffolderreadactivity", "dsffolderread" }, "Files & Folders",
            "Lists the contents of a folder.", false),
        new("Create (path)", "DsfPathCreate", new[] { "dsfpathcreate" }, "Files & Folders",
            "Creates a file or folder at a path.", false),
        new("Copy", "DsfPathCopy", new[] { "dsfpathcopy" }, "Files & Folders",
            "Copies a file or folder.", false),
        new("Move", "DsfPathMove", new[] { "dsfpathmove" }, "Files & Folders",
            "Moves a file or folder.", false),
        new("Rename", "DsfPathRename", new[] { "dsfpathrename" }, "Files & Folders",
            "Renames a file or folder.", false),
        new("Delete (path)", "DsfPathDelete", new[] { "dsfpathdelete" }, "Files & Folders",
            "Deletes a file or folder.", false),
        new("Zip", "DsfZip", new[] { "dsfzip" }, "Files & Folders",
            "Compresses files/folders into a zip archive.", false),
        new("UnZip", "DsfUnZip", new[] { "dsfunzip" }, "Files & Folders",
            "Extracts a zip archive.", false),
        new("GET Web Method", "WebGetActivity", new[] { "webgetactivity" }, "Integration",
            "Issues an HTTP GET request.", false),
        new("POST Web Method", "WebPostActivityNew", new[] { "webpostactivitynew" }, "Integration",
            "Issues an HTTP POST request.", false),
        new("PUT Web Method", "WebPutActivity", new[] { "webputactivity" }, "Integration",
            "Issues an HTTP PUT request.", false),
        new("DELETE Web Method", "DsfWebDeleteActivity", new[] { "dsfwebdeleteactivity" }, "Integration",
            "Issues an HTTP DELETE request.", false),
        new("Web Request", "DsfWebGetRequestWithTimeoutActivity", new[] { "dsfwebgetrequestwithtimeoutactivity" }, "Integration",
            "Issues an HTTP GET request with an explicit timeout.", false),
        new("SQL Server Database", "DsfSqlServerDatabaseActivity", new[] { "dsfsqlserverdatabaseactivity" }, "Database",
            "Executes a stored procedure/query against a SQL Server source.", true),
        new("PostgreSQL Database", "DsfPostgreSqlActivity", new[] { "dsfpostgresqlactivity" }, "Database",
            "Executes a query against a PostgreSQL source.", true),
        new("MySQL Database", "DsfMySqlDatabaseActivity", new[] { "dsfmysqldatabaseactivity" }, "Database",
            "Executes a query against a MySQL source.", true),
        new("Oracle Database", "DsfOracleDatabaseActivity", new[] { "dsforacledatabaseactivity" }, "Database",
            "Executes a query against an Oracle source.", true),
        new("ODBC Database", "DsfODBCDatabaseActivity", new[] { "dsfodbcdatabaseactivity" }, "Database",
            "Executes a query against an ODBC source.", true),
        new("SQL Bulk Insert", "DsfSqlBulkInsertActivity", new[] { "dsfsqlbulkinsertactivity" }, "Database",
            "Bulk-inserts recordset rows into a database table.", true),
        new("Redis Cache", "RedisCacheActivity", new[] { "rediscacheactivity" }, "Cache",
            "Reads or writes a value in a Redis cache.", true),
        new("Redis Remove", "RedisRemoveActivity", new[] { "redisremoveactivity" }, "Cache",
            "Removes a key from a Redis cache.", true),
        new("RabbitMQ Publish", "DsfPublishRabbitMQActivity", new[] { "dsfpublishrabbitmqactivity", "publishrabbitmqactivity" }, "Communication",
            "Publishes a message to a RabbitMQ queue/exchange.", true),
        new("RabbitMQ Consume", "DsfConsumeRabbitMQActivity", new[] { "dsfconsumerabbitmqactivity" }, "Communication",
            "Consumes a message from a RabbitMQ queue.", true),
        new("Send Email (SMTP)", "DsfSendEmailActivity", new[] { "dsfsendemailactivity" }, "Communication",
            "Sends an email via an SMTP source.", true),
        new("Exchange Email", "DsfExchangeEmailNewActivity", new[] { "dsfexchangeemailnewactivity" }, "Communication",
            "Sends an email via an Exchange source.", true),
        new("JavaScript", "DsfJavascriptActivity", new[] { "dsfjavascriptactivity" }, "Scripting",
            "Runs an inline JavaScript snippet.", false),
        new("Ruby", "DsfRubyActivity", new[] { "dsfrubyactivity" }, "Scripting",
            "Runs an inline Ruby snippet.", false),
        new("Python", "DsfPythonActivity", new[] { "dsfpythonactivity" }, "Scripting",
            "Runs an inline Python snippet.", false),
        new("Execute Command Line", "DsfExecuteCommandLineActivity", new[] { "dsfexecutecommandlineactivity" }, "Scripting",
            "Runs an OS command line and captures its output.", false),
        new("Date and Time", "DsfDotNetDateTimeActivity", new[] { "dsfdotnetdatetimeactivity", "dsfdatetimeactivity" }, "Date & Time",
            "Formats/converts a date-time value.", false),
        new("Date and Time Difference", "DsfDateTimeDifferenceActivity", new[] { "dsfdatetimedifferenceactivity", "dsfdotnetdatetimedifferenceactivity" }, "Date & Time",
            "Computes the difference between two date-time values.", false),
        new("Gather System Information", "DsfGatherSystemInformationActivity", new[] { "dsfgathersysteminformationactivity", "dsfdotnetgathersysteminformationactivity" }, "Utility",
            "Collects host system information (OS, memory, etc.).", false),
    };

    /// <summary>
    /// Resolves <paramref name="dataType"/> (a workflow body node's <c>data.type</c> value) to
    /// its catalog entry, using the same case-insensitive substring match
    /// <c>X6ToWorkflowConverter.CreateActivityFromNode</c> applies. Returns <c>null</c> when no
    /// entry matches (an unsupported/unknown activity type).
    /// </summary>
    internal static Entry? Resolve(string dataType)
    {
        if (string.IsNullOrWhiteSpace(dataType))
        {
            return null;
        }

        return Entries.FirstOrDefault(entry =>
            entry.DataTypes.Any(alias => dataType.Contains(alias, StringComparison.OrdinalIgnoreCase)));
    }
}
