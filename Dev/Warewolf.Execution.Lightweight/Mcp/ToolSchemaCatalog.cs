/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text.Json;

namespace Warewolf.Execution.Lightweight.Mcp;

/// <summary>
/// Per-tool <c>data</c> field documentation backing the <c>get_tool_schema</c> MCP tool — one JSON schema
/// document per <see cref="ToolCatalog.Entry.Name"/>.
///
/// <para>
/// <b>Sourcing (per spec: "required/optional data fields... sourced from the corresponding
/// Constants.* field-name group and X6ToWorkflowConverter_&lt;Type&gt;ActivityHelper.cs/
/// WorkflowToX6Converter_&lt;Type&gt;ActivityHelper.cs pair").</b> Every field name and literal
/// JSON key string below was read directly from each activity class's own
/// <c>ToX6Json(Cell cell)</c> override (the JSON-writer direction — the same source
/// <see cref="ToolCatalog"/>'s own remarks cite as authoritative for "what fields this cell
/// actually has") in <c>Dev/Dev2.Activities/Activities/**</c>, cross-checked against the literal
/// string constants in <c>Dev/Dev2.Common/X6/X6Models.cs</c> (<c>Constants</c>). Decision/Switch
/// branch and nesting mechanics were verified against
/// <c>Dev/Dev2.Activities/WorkflowConverters/{WorkflowToX6Converter,
/// SwitchActivityDataHelper,CellOrganizer}.cs</c> directly, not inferred.
/// </para>
///
/// <para>
/// <b>Common fields, documented once here instead of per entry.</b> Every activity's own
/// <c>ToX6Json</c> sets <c>type</c> (the lower-cased <c>dataType</c>), <c>displayname</c>, and
/// (for all but a handful of pure-passthrough activities) <c>UniqueID</c> directly — these are
/// <b>not</b> written by a shared base method, despite the "calls base.ToX6Json" pattern seen in
/// every override. The one field every activity's base <c>ToX6Json</c> genuinely does contribute
/// is <c>onerrordata</c> (<c>{ IsEndedOnError, OnErrorVariable, OnErrorWorkflow }</c>) — see
/// <c>DsfNativeActivity.ToX6Json</c>/<c>SetOnErrorData</c>. <c>get_workflow_schema</c>'s
/// <c>body_schema</c> already documents <c>type</c>/<c>displayname</c>/<c>isNested</c>/
/// <c>parentId</c> at the graph level, so per-entry schemas below list only the fields specific
/// to that activity, plus <c>UniqueID</c>/<c>onerrordata</c> where confirmed present.
/// </para>
///
/// <para>
/// <b>Nesting (ForEach/Sequence/Select and apply).</b> Confirmed generic and container-agnostic:
/// <see cref="!:CellOrganizer.BuildHierarchy"/> groups any cell with <c>data.isNested == true</c>
/// under the cell named by <c>data.parentId</c> — there is no per-container-type nesting
/// mechanism despite each container activity also serializing a legacy
/// <c>dataFunc</c>/<c>applyActivityFunc</c>/<c>activityFunc</c> field for read-compatibility with
/// older XAML; callers authoring new bodies should use <c>isNested</c>/<c>parentId</c> only, per
/// <c>get_workflow_schema</c>'s <c>body_schema</c>.
/// </para>
/// </summary>
internal static class ToolSchemaCatalog
{
    static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    /// <summary>Looks up the <c>get_tool_schema</c> document for <paramref name="studioName"/> (a <see cref="ToolCatalog.Entry.Name"/>). Returns <c>false</c> when no schema is authored for that name.</summary>
    internal static bool TryGet(string studioName, out JsonElement schema)
    {
        if (!string.IsNullOrWhiteSpace(studioName) && _schemas.Value.TryGetValue(studioName, out var element))
        {
            schema = element;
            return true;
        }

        schema = default;
        return false;
    }

    static readonly System.Lazy<FrozenDictionary<string, JsonElement>> _schemas = new(BuildSchemas);

    static FrozenDictionary<string, JsonElement> BuildSchemas()
    {
        var raw = new Dictionary<string, string>
        {
            ["Assign"] = """
                {
                  "fields": {
                    "fields": "array, required — a JSON array of { \"FieldName\": string, \"FieldValue\": string } (Constants.FIELDS; DsfDotNetMultiAssignActivity.ToX6Json/FromX6Json). Send a real JSON array, not a string; a string containing a JSON array is also accepted, and anything else is rejected by validate_workflow. A field name is a Warewolf variable expression, e.g. \"[[a]]\"."
                  },
                  "notes": [
                    "FromX6Json prefers Constants.UPDATEDFIELDS ('updatedfields') over 'fields' when both are present — an edit round-trip convention, not a required input field."
                  ]
                }
                """,
            ["Assign Object"] = """
                {
                  "fields": {
                    "fields": "array, required — a JSON array of { \"FieldName\": string, \"FieldValue\": string } (Constants.FIELDS; DsfDotNetMultiAssignObjectActivity.ToX6Json). Send a real JSON array, not a string; a string containing a JSON array is also accepted, and anything else is rejected by validate_workflow. FieldValue is typically a JSON object/array literal assigned to a JSON-typed variable, unlike Assign's scalar FieldValue."
                  },
                  "notes": [
                    "Same UPDATEDFIELDS/fields precedence on read as Assign."
                  ]
                }
                """,
            ["Decision"] = """
                {
                  "fields": {
                    "displaytext": "string, optional — the decision node's label (Constants.DISPLAYTEXT).",
                    "truearmtext": "string, optional — label shown on the true-branch edge (Constants.TRUEARMTEXT).",
                    "falsearmtext": "string, optional — label shown on the false-branch edge (Constants.FALSEARMTEXT).",
                    "expression": "string, required — the decision condition, serialized from a Dev2DecisionStack via Conditions.ToWebModel() (Constants.EXPRESSION). It is a JSON *string*, not a nested object, shaped {\"TheStack\":[{\"Col1\":\"[[var]]\",\"Col2\":\"comparand\",\"Col3\":\"\",\"PopulatedColumnCount\":2,\"EvaluationFn\":\"IsContains\"}],\"TotalDecisions\":1,\"ModelName\":\"Dev2DecisionStack\",\"Mode\":\"AND\"}. Col1 is the left operand, Col2 the right (Col3 is the upper bound for IsBetween/NotBetween, so PopulatedColumnCount is 1, 2 or 3). Mode is \"AND\" or \"OR\" and matches the `and` field.",
                    "and": "boolean, optional — true to AND-combine multiple decision conditions, false to OR-combine (Constants.AND).",
                    "EvaluationFn": "string, required within each TheStack entry — an enDecisionType *member name*, NOT the operator text the Studio displays: passing a display value such as \"Contains\" or \"=\" fails at execution with 'Error converting value \"…\" to type Dev2.Data.Decisions.Operations.enDecisionType'. Legal values: Choose, IsError, IsNotError, IsNull, IsNotNull, IsNumeric, IsNotNumeric, IsText, IsNotText, IsAlphanumeric, IsNotAlphanumeric, IsXML, IsNotXML, IsDate, IsNotDate, IsEmail, IsNotEmail, IsRegEx, NotRegEx, IsEqual, IsNotEqual, IsLessThan, IsLessThanOrEqual, IsGreaterThan, IsGreaterThanOrEqual, IsContains, NotContain, IsEndsWith, NotEndsWith, IsStartsWith, NotStartsWith, IsBetween, NotBetween, IsBinary, IsNotBinary, IsHex, IsNotHex, IsBase64, IsNotBase64."
                  },
                  "branching": {
                    "mechanism": "Two outgoing edges from this node, distinguished by edge data, not by label: the true arm carries data.isDecisionArm=true with data.isTrue=true, the false arm data.isDecisionArm=true with data.isTrue=false — the exact flags X6ToWorkflowConverter.HandleDecisionConnection reads back and validate_workflow requires. An edge whose only marker is label \"true\"/\"false\" is not recognised as an arm. Both arms are optional individually, but at least one is expected for a useful graph.",
                    "dataType": "Both the modern Decision ('flowdecision') and legacy Decision (legacy) ('dsfdecision') dataType values compile through the same DsfDecision field shape above — the converter always writes type=flowdecision on read-back regardless of which was authored."
                  },
                  "notes": [
                    "Verified directly in DsfDecision.cs's ToX6Json (the class every FlowDecision node is converted through) and WorkflowToX6Converter.ProcessFlowDecision — not inferred."
                  ]
                }
                """,
            ["Decision (legacy)"] = """
                {
                  "fields": {
                    "displaytext": "string, optional — see Decision.",
                    "truearmtext": "string, optional — see Decision.",
                    "falsearmtext": "string, optional — see Decision.",
                    "expression": "string, required — see Decision.",
                    "and": "boolean, optional — see Decision."
                  },
                  "branching": {
                    "mechanism": "Identical to Decision — same DsfDecision.ToX6Json path is used for both dataType aliases; see Decision's branching notes."
                  },
                  "notes": [
                    "Kept as a distinct list_tools row per the spec table, but there is no field-shape difference from Decision — both dataType values ('flowdecision', 'dsfdecision') resolve through DsfDecision."
                  ]
                }
                """,
            ["Switch"] = """
                {
                  "fields": {
                    "expression": "string, required — the switch variable expression, e.g. \"[[a]]\" (Constants.EXPRESSION; SwitchActivityDataHelper.ExtractSwitchVariable).",
                    "switchExpression": "string, required — JSON-encoded { \"SwitchVariable\": string, \"Cases\": [{ \"Key\": string, \"Value\": string }], \"DefaultCase\": \"Default\" | null } (Constants.SWITCH_EXPRESSION; SwitchActivityDataHelper.CreateSwitchExpressionJson). Always emitted, even with an empty Cases array."
                  },
                  "branching": {
                    "mechanism": "One outgoing edge per case, each labeled with that case's value (matches a Cases[].Key entry in switchExpression); one additional edge labeled \"Default\" (Constants.SWITCH_DEFAULT) for the default/fallthrough branch — matches WorkflowToX6Converter.ProcessFlowSwitch."
                  },
                  "notes": [
                    "Verified in SwitchActivityDataHelper.cs and WorkflowToX6Converter.ProcessFlowSwitch directly."
                  ]
                }
                """,
            ["Sequence"] = """
                {
                  "fields": {},
                  "nesting": {
                    "mechanism": "A Sequence node has no data fields of its own beyond type/displayname — its child activities are separate cells elsewhere in the same cells array with data.isNested=true and data.parentId set to this node's id (CellOrganizer.BuildHierarchy). Children run in the order they are connected by edges among themselves."
                  },
                  "notes": [
                    "DsfSequenceActivity.ToX6Json (DsfSequenceActivity.cs:368-369) writes only type=Constants.DSFSEQUENCE and displayname; confirmed no other fields are written."
                  ]
                }
                """,
            ["For Each"] = """
                {
                  "fields": {
                    "forEachType": "string, required — enum name of the ForEach mode (e.g. NumericTo/InRecordset/InCSV — matches DsfForEachActivity's ForEachType enum, serialized via .ToString()).",
                    "forEachElementName": "string, optional — the loop variable name.",
                    "from": "string, optional — start index/value, used by numeric/range ForEach modes.",
                    "to": "string, optional — end index/value, used by numeric/range ForEach modes.",
                    "recordset": "string, optional — the recordset to iterate, used by recordset ForEach modes.",
                    "csvIndexes": "string, optional — a CSV list of explicit indexes, used by index-list ForEach modes.",
                    "numOfExecutions": "string, optional — a fixed iteration count, used by count-based ForEach modes.",
                    "failOnFirstError": "boolean, optional — stop the loop on the first iteration error."
                  },
                  "nesting": {
                    "mechanism": "Same generic isNested/parentId mechanism as Sequence — child activities are separate cells with data.parentId pointing at this node's id. A ForEach runs one handler, so when more than one child is nested under it they are wrapped in a Sequence in index order rather than only the first surviving."
                  },
                  "notes": [
                    "DsfForEachActivity.ToX6Json also writes a legacy droppedNodes ([]) and dataFunc (serialized nested-activity info) field for backward read compatibility; callers authoring new bodies should not need to set either — use isNested/parentId nodes instead.",
                    "Field key casing is exactly as shown (\"displayName\" is camelCase here, unlike most other activities' lower-case \"displayname\") — confirmed directly in DsfForEachActivity.cs; the converter's case-insensitive read tolerates either."
                  ]
                }
                """,
            ["Select and apply"] = """
                {
                  "fields": {
                    "alias": "string, optional — the per-row iteration variable name (Constants.SELECTANDAPPLY_ALIAS).",
                    "dataSource": "string, optional — the recordset/expression to select rows from (Constants.SELECTANDAPPLY_DATASOURCE)."
                  },
                  "nesting": {
                    "mechanism": "Same generic isNested/parentId mechanism as Sequence/For Each — the applied nested chain is a separate cell (or chain of cells) with data.parentId pointing at this node's id."
                  },
                  "notes": [
                    "Also writes a legacy applyActivityFunc field for backward read compatibility; not required when authoring new bodies."
                  ]
                }
                """,
            ["Gate"] = """
                {
                  "fields": {
                    "gate_conditions": "string, required — JSON-serialized Dev2DecisionStack describing the gate's condition (Constants.GATE_CONDITIONS).",
                    "gate_retryentrypointid": "string, optional — the node id execution jumps back to while the gate condition is false (Constants.GATE_RETRYENTRYPOINTID).",
                    "gate_gateoptions": "string, optional — JSON-serialized gate retry/timeout options (Constants.GATE_GATEOPTIONS)."
                  },
                  "nesting": {
                    "mechanism": "Same generic isNested/parentId mechanism as Sequence for any activities the gate itself wraps."
                  },
                  "notes": [
                    "Also writes a legacy dataFunc field (Constants.GATE_DATAFUNC) for backward read compatibility."
                  ]
                }
                """,
            ["Suspend Execution"] = """
                {
                  "fields": {
                    "suspendoption": "integer, required — the SuspendOptionType enum value cast to int (Constants.SUSPENDEXECUTION_SUSPENDOPTION).",
                    "persistvalue": "string, optional — a variable/expression whose value is persisted across the suspend (Constants.SUSPENDEXECUTION_PERSISTVALUE).",
                    "allowmanualresumption": "boolean, optional — whether a Manual Resumption node elsewhere in the workflow may resume this suspend point (Constants.SUSPENDEXECUTION_ALLOWMANUALRESUMPTION).",
                    "encryptdata": "boolean, optional — encrypt the persisted suspend state (Constants.SUSPENDEXECUTION_ENCRYPTDATA).",
                    "result": "string, optional — output variable for the suspend/resume result (Constants.SUSPENDEXECUTION_RESPONSE, serialized under the shared 'result' key)."
                  },
                  "notes": [
                    "Also writes a legacy savedatafunc field (Constants.SUSPENDEXECUTION_SAVEDATAFUNC) for backward read compatibility."
                  ]
                }
                """,
            ["Manual Resumption"] = """
                {
                  "fields": {
                    "suspensionid": "string, optional — identifies the Suspend Execution point being resumed (Constants.MANUALRESUMPTION_SUSPENSIONID).",
                    "overrideinputvariables": "string, optional — variables to override on resumption (Constants.MANUALRESUMPTION_OVERRIDEINPUTVARIABLE).",
                    "result": "string, optional — output variable for the resumption result (Constants.RESULT)."
                  },
                  "notes": [
                    "Also writes a legacy OverrideDataFunc field (Constants.MANUALRESUMPTION_ACTIVITYFUNC) for backward read compatibility."
                  ]
                }
                """,
            ["Service (sub-workflow)"] = """
                {
                  "fields": {
                    "workflow_resourceid": "string, optional — the target workflow/service's resource id expression (Constants.WORKFLOW_RESOURCEID).",
                    "workflow_servicename": "string, required — the target workflow/service name to invoke (Constants.WORKFLOW_SERVICENAME).",
                    "workflow_serviceserver": "string, optional — the server/environment the target service lives on (Constants.WORKFLOW_SERVICESERVER).",
                    "workflow_sourceid": "string, optional — connection source id, when the target is a connector-backed service (Constants.WORKFLOW_SOURCEID).",
                    "workflow_runworkflowasync": "boolean, optional — invoke the target workflow asynchronously (Constants.WORKFLOW_RUNWORKFLOWASYNC).",
                    "workflow_isobject": "boolean, optional — map the result onto a single JSON object variable instead of scalar outputs (Constants.WORKFLOW_ISOBJECT).",
                    "workflow_objectname": "string, optional — the object variable to receive the result when workflow_isobject is true (Constants.WORKFLOW_OBJECTNAME).",
                    "workflow_objectresult": "string, optional — raw object-mode result payload (Constants.WORKFLOW_OBJECTRESULT).",
                    "workflow_inputmapping": "string, optional — input variable mapping to the target service's inputs (Constants.WORKFLOW_INPUTMAPPING).",
                    "workflow_outputmapping": "string, optional — output variable mapping from the target service's outputs (Constants.WORKFLOW_OUTPUTMAPPING).",
                    "workflow_isworkflow": "boolean, optional — true when the target is a workflow (vs. a plain service) (Constants.WORKFLOW_ISWORKFLOW).",
                    "workflow_inputs": "array, optional — input parameter list, or a JSON-encoded string of one (Constants.WORKFLOW_INPUTS).",
                    "workflow_outputs": "array, optional — output parameter list, or a JSON-encoded string of one (Constants.WORKFLOW_OUTPUTS). Each element is {MappedFrom, MappedTo, RecordSetName, Path?} — these are the ONLY keys read; an element using any other key names is silently treated as an all-empty mapping.",
                    "workflow_category": "string, optional — the target service's toolbox category (Constants.WORKFLOW_CATEGORY).",
                    "workflow_type": "string, optional — the target service's type discriminator (Constants.WORKFLOW_TYPE)."
                  },
                  "notes": [
                    "requiresSource: true per list_tools — workflow_sourceid/workflow_resourceid must reference a workflow/service that already exists on this instance; this MCP server does not create target workflows on the caller's behalf."
                  ]
                }
                """,
            ["Comment"] = """
                {
                  "fields": {
                    "comment_text": "string, optional — the comment's display text (Constants.COMMENT_TEXT)."
                  },
                  "notes": [
                    "A non-executing annotation node — no OnError/onerrordata semantics apply."
                  ]
                }
                """,
            ["Calculate"] = """
                {
                  "fields": {
                    "expression": "string, required — the arithmetic/formula expression to evaluate (Constants.CALCULATE_EXPRESSION).",
                    "result": "string, optional — output variable for the calculation result (Constants.CALCULATE_RESULT)."
                  }
                }
                """,
            ["Aggregate Calculate"] = """
                {
                  "fields": {
                    "expression": "string, required — the aggregate expression, e.g. Sum([[Recordset().Field]]) (Constants.AGGREGATECALCULATE_EXPRESSION).",
                    "result": "string, optional — output variable for the aggregate result (Constants.AGGREGATECALCULATE_RESULT)."
                  }
                }
                """,
            ["Create JSON"] = """
                {
                  "fields": {
                    "jsonmappings": "array, required — a JSON array of name/value mapping entries used to build the output object (Constants.CREATEJSON_JSONMAPPINGS).",
                    "jsonstring": "string, optional — the resulting/target JSON variable (Constants.CREATEJSON_JSONSTRING)."
                  },
                  "notes": [
                    "FromX6Json prefers Constants.CREATEJSON_UPDATEDJSONMAPPINGS ('updatedjsonmappings') over 'jsonmappings' when both are present — an edit round-trip convention."
                  ]
                }
                """,
            ["Data Merge"] = """
                {
                  "fields": {
                    "mergecollection": "array, required — a JSON array describing each merge-into-template mapping (Constants.MERGECOLLECTION).",
                    "result": "string, optional — output variable for the merged string (Constants.RESULT)."
                  },
                  "notes": [
                    "FromX6Json prefers Constants.UPDATEDMERGECOLLECTION ('updatedmergecollection') over 'mergecollection' when both are present."
                  ]
                }
                """,
            ["Data Split"] = """
                {
                  "fields": {
                    "sourcestring": "string, required — the input string to split (Constants.DATASPLIT_SOURCESTRING).",
                    "reverseorder": "boolean, optional — split from the end of the string backwards (Constants.DATASPLIT_REVERSEORDER).",
                    "skipblankrows": "boolean, optional — omit empty rows from the resulting recordset (Constants.DATASPLIT_SKIPBLANKROWS).",
                    "resultscollection": "array, required — a JSON array of split-rule entries (delimiter/index/character) producing the output recordset (Constants.RESULTSCOLLECTION)."
                  }
                }
                """,
            ["Base Conversion"] = """
                {
                  "fields": {
                    "convertcollection": "array, required — a JSON array of { value, from-encoding, to-encoding, result } conversion rows (Constants.CONVERTCOLLECTION)."
                  },
                  "notes": [
                    "FromX6Json prefers Constants.UPDATEDCONVERTCOLLECTION ('updatedconvertcollection') over 'convertcollection' when both are present. Case Conversion (below) shares this exact field shape."
                  ]
                }
                """,
            ["Case Conversion"] = """
                {
                  "fields": {
                    "convertcollection": "string, required — JSON-encoded array of { value, case-rule (upper/lower/proper), result } conversion rows (Constants.CONVERTCOLLECTION) — same field name/shape as Base Conversion, semantics differ by activity."
                  },
                  "notes": [
                    "Same UPDATEDCONVERTCOLLECTION read-precedence as Base Conversion."
                  ]
                }
                """,
            ["Replace"] = """
                {
                  "fields": {
                    "FieldsToSearch": "string, required — the field(s)/expression to search within (Constants.REPLACE_FIELDS_TO_SEARCH — note PascalCase key, unlike most other activities' lower-case keys).",
                    "Find": "string, required — the text/pattern to find (Constants.REPLACE_FIND).",
                    "ReplaceWith": "string, optional — the replacement text (Constants.REPLACE_REPLACE_WTIH).",
                    "CaseMatch": "boolean, optional — case-sensitive match (Constants.REPLACE_CASE_MATCH).",
                    "Result": "string, optional — output variable for the replaced value (Constants.REPLACE_RESULT)."
                  },
                  "notes": [
                    "Field key casing is exactly PascalCase for this activity, confirmed directly in Dev2.Common.X6.Constants — do not lower-case these keys when authoring a body."
                  ]
                }
                """,
            ["Find Index"] = """
                {
                  "fields": {},
                  "notes": [
                    "DsfIndexActivity.ToX6Json (confirmed) writes only type/displayname/UniqueID — no activity-specific fields were found on the ToX6Json override itself; its input/output configuration is carried entirely via the shared FindIndex property set inherited from its base and not re-serialized into cell.data by this override. Confidence: medium — the override was read directly, but its base class's own ToX6Json contribution (beyond onerrordata) was not separately re-verified."
                  ]
                }
                """,
            ["Format Number"] = """
                {
                  "fields": {
                    "expression": "string, required — the numeric value/expression to format (Constants.NUMBERFORMAT_EXPRESSION).",
                    "roundingtype": "string, optional — rounding mode name (Constants.NUMBERFORMAT_ROUNDINGTYPE).",
                    "roundingdecimalplaces": "string, optional — decimal places to round to (Constants.NUMBERFORMAT_ROUNDINGDECIMALPLACES).",
                    "decimalplacestoshow": "string, optional — decimal places to display in the formatted result (Constants.NUMBERFORMAT_DECIMALPLACESTOSHOW).",
                    "result": "string, optional — output variable for the formatted value (Constants.NUMBERFORMAT_RESULT)."
                  }
                }
                """,
            ["Random"] = """
                {
                  "fields": {
                    "randomtype": "string, required — enum name of the random mode (e.g. Number/GUID/Letters — RandomType.ToString()) (Constants.RANDOM_TYPE).",
                    "from": "string, optional — lower bound, used by numeric random modes (Constants.RANDOM_FROM).",
                    "to": "string, optional — upper bound, used by numeric random modes (Constants.RANDOM_TO).",
                    "length": "string, optional — generated value length, used by string/GUID-like modes (Constants.RANDOM_LENGTH).",
                    "result": "string, optional — output variable for the generated value (Constants.RANDOM_RESULT)."
                  }
                }
                """,
            ["XPath"] = """
                {
                  "fields": {
                    "sourcestring": "string, required — the XML input and XPath expression pairing (Constants.XPATH_SOURCESTRING).",
                    "resultscollection": "array, required — a JSON array of XPath query/result-variable rows (Constants.XPATH_RESULTSCOLLECTION)."
                  },
                  "notes": [
                    "FromX6Json prefers Constants.XPATH_UPDATEDRESULTSCOLLECTION ('updatedresultscollection') over 'resultscollection' when both are present."
                  ]
                }
                """,
            ["Find Records"] = """
                {
                  "fields": {
                    "fieldsToSearch": "string, required — JSON-encoded search-field/criteria list (Constants.FINDRECORDS_FIELDSTOSEARCH).",
                    "result": "string, optional — output variable for the primary result (Constants.RESULT).",
                    "startIndex": "string, optional — recordset row index to start searching from (Constants.FINDRECORDS_STARTINDEX).",
                    "matchCase": "boolean, optional — case-sensitive matching (Constants.FINDRECORDS_MATCHCASE).",
                    "requireAllTrue": "boolean, optional — require all criteria to match (AND) rather than any (OR) (Constants.FINDRECORDS_REQUIREALLTRUE).",
                    "requireAllFieldsToMatch": "boolean, optional — require every listed field to match on the same row (Constants.FINDRECORDS_REQUIREALLFIELDSTOMATCH).",
                    "resultsCollection": "string, required — output recordset name/shape receiving matching rows (Constants.FINDRECORDS_RESULTSCOLLECTION)."
                  }
                }
                """,
            ["Delete Records"] = """
                {
                  "fields": {
                    "recordsetname": "string, required — the recordset to delete rows from (Constants.DELETERECORDS_RECORDSETNAME).",
                    "result": "string, optional — output variable for the delete result (Constants.RESULT)."
                  },
                  "notes": [
                    "The dsfdeleterecordnullhandleractivity alias (DsfDeleteRecordNullHandlerActivity) adds one extra field: treatNullAsZero (boolean, optional — Constants.DELETERECORDS_TREATNULLASZERO). The dsfdeleterecordactivity alias (DsfDeleteRecordActivity) does not have this field."
                  ]
                }
                """,
            ["Sort Records"] = """
                {
                  "fields": {
                    "sortfield": "string, required — the recordset field to sort by (Constants.SORTACTIVITY_FIELD).",
                    "selectedsort": "string, required — sort direction/mode (Constants.SORTACTIVITY_SELECTEDSORT)."
                  }
                }
                """,
            ["Count Records"] = """
                {
                  "fields": {
                    "recordsetname": "string, required — the recordset to count rows in (Constants.COUNTRECORDS_RECORDSETNAME).",
                    "countnumber": "string, optional — output variable for the row count (Constants.COUNTRECORDS_COUNTNUMBER).",
                    "treatnullaszero": "boolean, optional — treat a null/missing recordset as a count of zero rather than erroring (Constants.COUNTRECORDS_TREATNULLASZERO)."
                  }
                }
                """,
            ["Length"] = """
                {
                  "fields": {
                    "recordsetname": "string, required — the recordset (or field) whose length is measured (Constants.LENGTH_RECORDSETNAME).",
                    "recordslength": "string, optional — output variable for the length (Constants.LENGTH_RECORDSLENGTH).",
                    "treatnullaszero": "boolean, optional — treat a null/missing value as length zero rather than erroring (Constants.LENGTH_TREATNULLASZERO)."
                  }
                }
                """,
            ["Unique Records"] = """
                {
                  "fields": {
                    "infields": "string, required — JSON-encoded list of fields to de-duplicate on (Constants.UNIQUEACTIVITY_INFIELDS).",
                    "resultfields": "string, optional — output recordset field mapping (Constants.UNIQUEACTIVITY_RESULTFIELDS).",
                    "result": "string, optional — output variable for the operation result (Constants.RESULT)."
                  }
                }
                """,
            ["Advanced Recordset"] = """
                {
                  "fields": {
                    "sqlquery": "string, required — a SQL-like query run against an in-memory recordset (Constants.ADVANCEDRECORDSET_SQLQUERY).",
                    "recordsetname": "string, required — the output recordset name (Constants.ADVANCEDRECORDSET_RECORDSETNAME).",
                    "declarevariables": "string, optional — variable declarations available to the query (Constants.ADVANCEDRECORDSET_DECLAREVARIABLES).",
                    "sourceId": "string, optional — connection source id, if the query reads from an external source rather than an in-memory recordset (Constants.WEBMETHOD_SOURCEID).",
                    "outputs": "array, optional — output parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_OUTPUTS). Each element is {MappedFrom, MappedTo, RecordSetName, Path?} — these are the ONLY keys read; an element using any other key names is silently treated as an all-empty mapping."
                  }
                }
                """,
            ["Read File"] = """
                {
                  "fields": {
                    "inputpath": "string, required — the file path to read (Constants.FILEREAD_INPUTPATH).",
                    "result": "string, optional — output variable for the file contents (Constants.RESULT)."
                  },
                  "notes": [
                    "The filereadwithbase64 alias (FileReadWithBase64) adds one extra field: isresultbase64 (boolean, optional — Constants.FILEREAD_ISRESULTBASE64, return contents Base64-encoded). Both aliases inherit username/password/privatekeyfile from DsfAbstractFileActivity (Constants.FILE_FOLDER_USERNAME/PASSWORD/PRIVATEKEYFILE) for remote (SFTP/FTP-style) paths."
                  ]
                }
                """,
            ["Write File"] = """
                {
                  "fields": {
                    "filecontentsasbase64": "string, optional — Base64-encoded content to write, mutually exclusive with filecontents (Constants.FILEWRITE_ASBASE64).",
                    "outputpath": "string, required — the file path to write to (Constants.FILEWRITE_OUTPUTPATH).",
                    "filecontents": "string, optional — plain-text content to write (Constants.FILEWRITE_FILECONTENTS).",
                    "overwrite": "boolean, optional — overwrite an existing file at outputpath (Constants.FILEWRITE_OVERWRITE).",
                    "appendtop": "boolean, optional — prepend content to an existing file instead of overwriting (Constants.FILEWRITE_APPENDTOP).",
                    "appendbottom": "boolean, optional — append content to an existing file instead of overwriting (Constants.FILEWRITE_APPENDBOTTOM).",
                    "result": "string, optional — output variable for the write result (Constants.RESULT)."
                  },
                  "notes": [
                    "Inherits username/password/privatekeyfile from DsfAbstractFileActivity for remote paths."
                  ]
                }
                """,
            ["Folder Read"] = """
                {
                  "fields": {
                    "inputpath": "string, required — the folder path to list (Constants.FOLDERREAD_INPUTPATH).",
                    "result": "string, optional — output recordset variable for the listing (Constants.RESULT).",
                    "isfilesselected": "boolean, optional — include files in the listing (Constants.FOLDERREAD_ISFILESSELECTED).",
                    "isfoldersselected": "boolean, optional — include subfolders in the listing (Constants.FOLDERREAD_ISFOLDERSSELECTED).",
                    "isfilesandfoldersselected": "boolean, optional — include both files and subfolders (Constants.FOLDERREAD_ISFILESANDFOLDERSSELECTED)."
                  },
                  "notes": [
                    "Inherits username/password/privatekeyfile from DsfAbstractFileActivity for remote paths. The dsffolderread alias (DsfFolderRead) has the identical field shape to dsffolderreadactivity (DsfFolderReadActivity) — confirmed both classes serialize the same keys."
                  ]
                }
                """,
            ["Create (path)"] = """
                {
                  "fields": {
                    "outputpath": "string, required — the file or folder path to create (Constants.PATHCREATE_OUTPUTPATH).",
                    "overwrite": "boolean, optional — overwrite an existing path (Constants.PATHCREATE_OVERWRITE).",
                    "result": "string, optional — output variable for the create result (Constants.RESULT)."
                  }
                }
                """,
            ["Copy"] = """
                {
                  "fields": {
                    "inputpath": "string, required — the source file/folder path (Constants.PATHCOPY_INPUTPATH).",
                    "outputpath": "string, required — the destination path (Constants.PATHCOPY_OUTPUTPATH).",
                    "result": "string, optional — output variable for the copy result (Constants.RESULT).",
                    "overwrite": "boolean, optional — overwrite an existing file/folder at outputpath (Constants.PATHCOPY_OVERWRITE).",
                    "destinationusername": "string, optional — credentials for a remote destination (Constants.PATHCOPY_DESTINATIONUSERNAME).",
                    "destinationpassword": "string, optional — credentials for a remote destination (Constants.PATHCOPY_DESTINATIONPASSWORD).",
                    "destinationprivatekeyfile": "string, optional — private key for a remote destination (Constants.PATHCOPY_DESTINATIONPRIVATEKEYFILE)"
                  },
                  "notes": [
                    "Inherits username/password/privatekeyfile from DsfAbstractFileActivity for the source path."
                  ]
                }
                """,
            ["Move"] = """
                {
                  "fields": {
                    "inputpath": "string, required — the source file/folder path (Constants.PATHMOVE_INPUTPATH).",
                    "outputpath": "string, required — the destination path (Constants.PATHMOVE_OUTPUTPATH).",
                    "result": "string, optional — output variable for the move result (Constants.RESULT).",
                    "overwrite": "boolean, optional — overwrite an existing file/folder at outputpath (Constants.PATHMOVE_OVERWRITE).",
                    "destinationusername": "string, optional — credentials for a remote destination (Constants.PATHMOVE_DESTINATIONUSERNAME).",
                    "destinationpassword": "string, optional — credentials for a remote destination (Constants.PATHMOVE_DESTINATIONPASSWORD).",
                    "destinationprivatekeyfile": "string, optional — private key for a remote destination (Constants.PATHMOVE_DESTINATIONPRIVATEKEYFILE)"
                  },
                  "notes": [
                    "DsfPathMove.ToX6Json calls only base.ToX6Json (DsfAbstractMultipleFilesActivity) — confirmed no additional fields beyond the ones listed. Inherits username/password/privatekeyfile from DsfAbstractFileActivity for the source path."
                  ]
                }
                """,
            ["Rename"] = """
                {
                  "fields": {
                    "inputpath": "string, required — the source file/folder path (Constants.PATHMOVE_INPUTPATH) — Rename shares DsfAbstractMultipleFilesActivity's field names with Move/Copy; outputpath is the new name/path.",
                    "outputpath": "string, required — the new path/name (Constants.PATHMOVE_OUTPUTPATH).",
                    "result": "string, optional — output variable for the rename result (Constants.RESULT).",
                    "overwrite": "boolean, optional — overwrite an existing file/folder at outputpath (Constants.PATHMOVE_OVERWRITE).",
                    "destinationusername": "string, optional — credentials for a remote destination (Constants.PATHMOVE_DESTINATIONUSERNAME).",
                    "destinationpassword": "string, optional — credentials for a remote destination (Constants.PATHMOVE_DESTINATIONPASSWORD).",
                    "destinationprivatekeyfile": "string, optional — private key for a remote destination (Constants.PATHMOVE_DESTINATIONPRIVATEKEYFILE)"
                  },
                  "notes": [
                    "DsfPathRename.ToX6Json calls only base.ToX6Json (DsfAbstractMultipleFilesActivity), identical field shape to Move — confirmed directly, not inferred."
                  ]
                }
                """,
            ["Delete (path)"] = """
                {
                  "fields": {
                    "inputpath": "string, required — the file/folder path to delete (Constants.PATHDELETE_INPUTPATH).",
                    "result": "string, optional — output variable for the delete result (Constants.RESULT)."
                  }
                }
                """,
            ["Zip"] = """
                {
                  "fields": {
                    "inputpath": "string, required — the file(s)/folder to compress (Constants.ZIP_INPUTPATH).",
                    "outputpath": "string, optional — destination folder for the archive (Constants.ZIP_OUTPUTPATH).",
                    "archivename": "string, required — the archive file name (Constants.ZIP_ARCHIVENAME).",
                    "compressionratio": "string, optional — compression level setting (Constants.ZIP_COMPRESSIONRATIO).",
                    "archivepassword": "string, optional — password-protect the archive (Constants.ZIP_ARCHIVEPASSWORD).",
                    "overwrite": "boolean, optional — overwrite an existing archive (Constants.ZIP_OVERWRITE).",
                    "username": "string, optional — credentials for a remote source path (Constants.ZIP_USERNAME).",
                    "password": "string, optional — credentials for a remote source path (Constants.ZIP_PASSWORD).",
                    "privatekeyfile": "string, optional — private key for a remote source path (Constants.ZIP_PRIVATEKEYFILE).",
                    "destinationusername": "string, optional — credentials for a remote destination (Constants.ZIP_DESTINATIONUSERNAME).",
                    "destinationpassword": "string, optional — credentials for a remote destination (Constants.ZIP_DESTINATIONPASSWORD).",
                    "destinationprivatekeyfile": "string, optional — private key for a remote destination (Constants.ZIP_DESTINATIONPRIVATEKEYFILE)"
                  }
                }
                """,
            ["UnZip"] = """
                {
                  "fields": {
                    "inputpath": "string, required — the archive file to extract (Constants.UNZIP_INPUTPATH).",
                    "username": "string, optional — credentials for a remote archive path (Constants.UNZIP_USERNAME).",
                    "password": "string, optional — credentials for a remote archive path (Constants.UNZIP_PASSWORD).",
                    "privatekeyfile": "string, optional — private key for a remote archive path (Constants.UNZIP_PRIVATEKEYFILE).",
                    "outputpath": "string, required — destination folder for extracted contents (Constants.UNZIP_DESTINATIONOUTPUTPATH).",
                    "destinationusername": "string, optional — credentials for a remote destination (Constants.UNZIP_DESTINATIONUSERNAME).",
                    "destinationpassword": "string, optional — credentials for a remote destination (Constants.UNZIP_DESTINATIONPASSWORD).",
                    "destinationprivatekeyfile": "string, optional — private key for a remote destination (Constants.UNZIP_DESTINATIONPRIVATEKEYFILE).",
                    "overwrite": "boolean, optional — overwrite existing extracted files (Constants.UNZIP_OVERWRITE).",
                    "archivepassword": "string, optional — password of a protected archive (Constants.UNZIP_ARCHIVEPASSWORD).",
                    "result": "string, optional — output variable for the extract result (Constants.RESULT)."
                  }
                }
                """,
            ["GET Web Method"] = """
                {
                  "fields": {
                    "headers": "array, optional — HTTP headers, or a JSON-encoded string of them (Constants.WEBMETHOD_HEADERS).",
                    "querystring": "string, required — the request URL/query string (Constants.WEBMETHOD_QUERYSTRING). Accepts requestUrl/url as caller-supplied aliases per the Angular chatbot normalizer leniency the spec references — this MCP server should apply the same tolerance when authoring bodies.",
                    "sourceId": "string, optional — connection source id, when calling through a saved web source rather than an ad-hoc URL (Constants.WEBMETHOD_SOURCEID).",
                    "outputdescription": "string, optional — a sample/description of the expected response shape, used to derive output mappings (Constants.WEBMETHOD_OUTPUTDESCRIPTION).",
                    "inputs": "array, optional — input parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_INPUTS).",
                    "outputs": "array, optional — output parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_OUTPUTS). Each element is {MappedFrom, MappedTo, RecordSetName, Path?} — these are the ONLY keys read; an element using any other key names (e.g. {name, mapsTo}) is silently treated as an all-empty mapping. MappedFrom is the source expression (e.g. '[[ResponseBody]]'), MappedTo the destination variable, RecordSetName set only when mapping into a recordset field. Path is optional: {ActualPath, DisplayPath, OutputExpression, SampleData}.",
                    "isOutputToObject": "boolean, optional — map the response onto a single JSON object variable (Constants.WEBMETHOD_ISOBJECT).",
                    "objectname": "string, optional — the object variable to receive the response when isOutputToObject is true (Constants.WEBMETHOD_OBJECTNAME).",
                    "objectresult": "string, optional — raw object-mode response payload (Constants.WEBMETHOD_OBJECTRESULT).",
                    "isresponsebase64": "boolean, optional — treat the response body as Base64-encoded (Constants.WEBMETHOD_ISRESPONSEBASE64)."
                  }
                }
                """,
            ["POST Web Method"] = """
                {
                  "fields": {
                    "headers": "array, optional — HTTP headers, or a JSON-encoded string of them (Constants.WEBMETHOD_HEADERS).",
                    "querystring": "string, required — the request URL/query string (Constants.WEBMETHOD_QUERYSTRING).",
                    "settings": "string, optional — additional request settings (Constants.WEBMETHOD_SETTINGS).",
                    "conditions": "string, optional — pre-request condition rules (Constants.WEBMETHOD_CONDITIONS).",
                    "timeout": "string, optional — request timeout (Constants.WEBMETHOD_TIMEOUT).",
                    "postdata": "string, optional — the POST body content (Constants.WEBMETHOD_POSTDATA).",
                    "sourceId": "string, optional — connection source id (Constants.WEBMETHOD_SOURCEID).",
                    "outputdescription": "string, optional — sample/description of the expected response shape (Constants.WEBMETHOD_OUTPUTDESCRIPTION).",
                    "inputs": "array, optional — input parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_INPUTS).",
                    "outputs": "array, optional — output parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_OUTPUTS). Each element is {MappedFrom, MappedTo, RecordSetName, Path?} — these are the ONLY keys read; an element using any other key names (e.g. {name, mapsTo}) is silently treated as an all-empty mapping. MappedFrom is the source expression (e.g. '[[ResponseBody]]'), MappedTo the destination variable, RecordSetName set only when mapping into a recordset field. Path is optional: {ActualPath, DisplayPath, OutputExpression, SampleData}.",
                    "isOutputToObject": "boolean, optional — map the response onto a single JSON object variable (Constants.WEBMETHOD_ISOBJECT).",
                    "objectname": "string, optional — object variable for isOutputToObject (Constants.WEBMETHOD_OBJECTNAME).",
                    "objectresult": "string, optional — raw object-mode response payload (Constants.WEBMETHOD_OBJECTRESULT)."
                  }
                }
                """,
            ["PUT Web Method"] = """
                {
                  "fields": {
                    "headers": "array, optional — HTTP headers, or a JSON-encoded string of them (Constants.WEBMETHOD_HEADERS).",
                    "querystring": "string, required — the request URL/query string (Constants.WEBMETHOD_QUERYSTRING).",
                    "isputdatabase64": "boolean, optional — treat postdata as Base64-encoded (Constants.WEBMETHOD_ISPUTDATABASE64).",
                    "postdata": "string, optional — the PUT body content (Constants.WEBMETHOD_POSTDATA, reused as PutData for PUT).",
                    "sourceId": "string, optional — connection source id (Constants.WEBMETHOD_SOURCEID).",
                    "outputdescription": "string, optional — sample/description of the expected response shape (Constants.WEBMETHOD_OUTPUTDESCRIPTION).",
                    "inputs": "array, optional — input parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_INPUTS).",
                    "outputs": "array, optional — output parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_OUTPUTS). Each element is {MappedFrom, MappedTo, RecordSetName, Path?} — these are the ONLY keys read; an element using any other key names (e.g. {name, mapsTo}) is silently treated as an all-empty mapping. MappedFrom is the source expression (e.g. '[[ResponseBody]]'), MappedTo the destination variable, RecordSetName set only when mapping into a recordset field. Path is optional: {ActualPath, DisplayPath, OutputExpression, SampleData}.",
                    "isOutputToObject": "boolean, optional — map the response onto a single JSON object variable (Constants.WEBMETHOD_ISOBJECT).",
                    "objectname": "string, optional — object variable for isOutputToObject (Constants.WEBMETHOD_OBJECTNAME).",
                    "objectresult": "string, optional — raw object-mode response payload (Constants.WEBMETHOD_OBJECTRESULT)."
                  }
                }
                """,
            ["DELETE Web Method"] = """
                {
                  "fields": {
                    "headers": "array, optional — HTTP headers, or a JSON-encoded string of them (Constants.WEBMETHOD_HEADERS).",
                    "querystring": "string, required — the request URL/query string (Constants.WEBMETHOD_QUERYSTRING).",
                    "sourceId": "string, optional — connection source id (Constants.WEBMETHOD_SOURCEID).",
                    "outputdescription": "string, optional — sample/description of the expected response shape (Constants.WEBMETHOD_OUTPUTDESCRIPTION).",
                    "inputs": "array, optional — input parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_INPUTS).",
                    "outputs": "array, optional — output parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_OUTPUTS). Each element is {MappedFrom, MappedTo, RecordSetName, Path?} — these are the ONLY keys read; an element using any other key names (e.g. {name, mapsTo}) is silently treated as an all-empty mapping. MappedFrom is the source expression (e.g. '[[ResponseBody]]'), MappedTo the destination variable, RecordSetName set only when mapping into a recordset field. Path is optional: {ActualPath, DisplayPath, OutputExpression, SampleData}.",
                    "isOutputToObject": "boolean, optional — map the response onto a single JSON object variable (Constants.WEBMETHOD_ISOBJECT).",
                    "objectname": "string, optional — object variable for isOutputToObject (Constants.WEBMETHOD_OBJECTNAME).",
                    "objectresult": "string, optional — raw object-mode response payload (Constants.WEBMETHOD_OBJECTRESULT)."
                  }
                }
                """,
            ["Web Request"] = """
                {
                  "fields": {
                    "webrequest_method": "string, required — the HTTP method to use (Constants.WEBREQUEST_METHOD). One of GET, POST, PUT, DELETE, PATCH; any other value is rejected at execution time.",
                    "webrequest_timeoutseconds": "number, optional — request timeout in seconds (Constants.WEBREQUEST_TIMEOUTSECONDS).",
                    "webrequest_timeouttext": "string, optional — human-readable timeout description (Constants.WEBREQUEST_TIMEOUTTEXT).",
                    "webrequest_url": "string, required — the request URL (Constants.WEBREQUEST_URL).",
                    "webrequest_headers": "array, optional — HTTP headers, or a JSON-encoded string of them (Constants.WEBREQUEST_HEADERS).",
                    "webrequest_postdata": "string, optional — request body for verbs that carry one (POST/PUT/PATCH); ignored for GET/DELETE. Omitted or empty sends an empty body (Constants.WEBREQUEST_POSTDATA).",
                    "webrequest_result": "string, optional — output variable for the response (Constants.WEBREQUEST_RESULT)."
                  },
                  "notes": [
                    "Unlike the other web method tools, this activity's own field keys are prefixed with webrequest_ rather than sharing the WEBMETHOD_* keys — confirmed directly in DsfWebGetRequestWithTimeoutActivity.ToX6Json.",
                    "Unlike the WEBMETHOD_* tools this activity resolves no connection source, so webrequest_url takes a full absolute URL and no sourceId is involved."
                  ]
                }
                """,
            ["SQL Server Database"] = """
                {
                  "fields": {
                    "procedurename": "string, required — the stored procedure to execute (Constants.DATABASE_PROCEDURENAME).",
                    "executeactionstring": "string, optional — a raw SQL statement, used instead of procedurename for ad-hoc queries (Constants.DATABASE_EXECUTEACTIONSTRING) — SQL Server is the only database tool with this field.",
                    "serviceserver": "string, optional — the database server/connection label (Constants.DATABASE_SERVICESERVER).",
                    "sourceId": "string, required — the SQL Server connection source id (Constants.WEBMETHOD_SOURCEID).",
                    "commandtimeout": "string, optional — command timeout (Constants.DATABASE_COMMANDTIMEOUT).",
                    "isOutputToObject": "boolean, optional — map the result onto a single JSON object variable (Constants.WEBMETHOD_ISOBJECT).",
                    "objectname": "string, optional — object variable for isOutputToObject (Constants.WEBMETHOD_OBJECTNAME).",
                    "objectresult": "string, optional — raw object-mode result payload (Constants.WEBMETHOD_OBJECTRESULT).",
                    "inputs": "array, optional — stored-procedure parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_INPUTS).",
                    "outputs": "array, optional — output/result-column mapping, or a JSON-encoded string of one (Constants.WEBMETHOD_OUTPUTS). Each element is {MappedFrom, MappedTo, RecordSetName, Path?} — these are the ONLY keys read; an element using any other key names is silently treated as an all-empty mapping."
                  },
                  "notes": [
                    "requiresSource: true per list_tools — sourceId must reference an existing SQL Server connection resource on this instance."
                  ]
                }
                """,
            ["PostgreSQL Database"] = """
                {
                  "fields": {
                    "procedurename": "string, required — the stored procedure/function to execute (Constants.DATABASE_PROCEDURENAME).",
                    "serviceserver": "string, optional — the database server/connection label (Constants.DATABASE_SERVICESERVER).",
                    "sourceId": "string, required — the PostgreSQL connection source id (Constants.WEBMETHOD_SOURCEID).",
                    "commandtimeout": "string, optional — command timeout (Constants.DATABASE_COMMANDTIMEOUT).",
                    "isOutputToObject": "boolean, optional — see SQL Server Database (Constants.WEBMETHOD_ISOBJECT).",
                    "objectname": "string, optional — see SQL Server Database (Constants.WEBMETHOD_OBJECTNAME).",
                    "objectresult": "string, optional — see SQL Server Database (Constants.WEBMETHOD_OBJECTRESULT).",
                    "inputs": "array, optional — parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_INPUTS).",
                    "outputs": "array, optional — output/result-column mapping, or a JSON-encoded string of one (Constants.WEBMETHOD_OUTPUTS). Each element is {MappedFrom, MappedTo, RecordSetName, Path?} — these are the ONLY keys read; an element using any other key names is silently treated as an all-empty mapping."
                  },
                  "notes": [
                    "requiresSource: true. Unlike SQL Server Database, PostgreSQL has no executeactionstring field — only procedurename (confirmed directly in DsfPostgreSqlActivity.ToX6Json)."
                  ]
                }
                """,
            ["MySQL Database"] = """
                {
                  "fields": {
                    "procedurename": "string, required — the stored procedure to execute (Constants.DATABASE_PROCEDURENAME).",
                    "serviceserver": "string, optional — the database server/connection label (Constants.DATABASE_SERVICESERVER).",
                    "sourceId": "string, required — the MySQL connection source id (Constants.WEBMETHOD_SOURCEID).",
                    "commandtimeout": "string, optional — command timeout (Constants.DATABASE_COMMANDTIMEOUT).",
                    "isOutputToObject": "boolean, optional — see SQL Server Database (Constants.WEBMETHOD_ISOBJECT).",
                    "objectname": "string, optional — see SQL Server Database (Constants.WEBMETHOD_OBJECTNAME).",
                    "objectresult": "string, optional — see SQL Server Database (Constants.WEBMETHOD_OBJECTRESULT).",
                    "inputs": "array, optional — parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_INPUTS).",
                    "outputs": "array, optional — output/result-column mapping, or a JSON-encoded string of one (Constants.WEBMETHOD_OUTPUTS). Each element is {MappedFrom, MappedTo, RecordSetName, Path?} — these are the ONLY keys read; an element using any other key names is silently treated as an all-empty mapping."
                  },
                  "notes": [
                    "requiresSource: true. No executeactionstring field, same as PostgreSQL/Oracle/ODBC."
                  ]
                }
                """,
            ["Oracle Database"] = """
                {
                  "fields": {
                    "procedurename": "string, required — the stored procedure to execute (Constants.DATABASE_PROCEDURENAME).",
                    "serviceserver": "string, optional — the database server/connection label (Constants.DATABASE_SERVICESERVER).",
                    "sourceId": "string, required — the Oracle connection source id (Constants.WEBMETHOD_SOURCEID).",
                    "commandtimeout": "string, optional — command timeout (Constants.DATABASE_COMMANDTIMEOUT).",
                    "isOutputToObject": "boolean, optional — see SQL Server Database (Constants.WEBMETHOD_ISOBJECT).",
                    "objectname": "string, optional — see SQL Server Database (Constants.WEBMETHOD_OBJECTNAME).",
                    "objectresult": "string, optional — see SQL Server Database (Constants.WEBMETHOD_OBJECTRESULT).",
                    "inputs": "array, optional — parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_INPUTS).",
                    "outputs": "array, optional — output/result-column mapping, or a JSON-encoded string of one (Constants.WEBMETHOD_OUTPUTS). Each element is {MappedFrom, MappedTo, RecordSetName, Path?} — these are the ONLY keys read; an element using any other key names is silently treated as an all-empty mapping."
                  },
                  "notes": [
                    "requiresSource: true. No executeactionstring field."
                  ]
                }
                """,
            ["ODBC Database"] = """
                {
                  "fields": {
                    "commandtext": "string, required — a raw SQL command (Constants.DATABASE_COMMANDTEXT) — ODBC is the only database tool that uses commandtext instead of procedurename.",
                    "serviceserver": "string, optional — the database server/connection label (Constants.DATABASE_SERVICESERVER).",
                    "sourceId": "string, required — the ODBC connection source id (Constants.WEBMETHOD_SOURCEID).",
                    "commandtimeout": "string, optional — command timeout (Constants.DATABASE_COMMANDTIMEOUT).",
                    "isOutputToObject": "boolean, optional — see SQL Server Database (Constants.WEBMETHOD_ISOBJECT).",
                    "objectname": "string, optional — see SQL Server Database (Constants.WEBMETHOD_OBJECTNAME).",
                    "objectresult": "string, optional — see SQL Server Database (Constants.WEBMETHOD_OBJECTRESULT).",
                    "inputs": "array, optional — parameter list, or a JSON-encoded string of one (Constants.WEBMETHOD_INPUTS).",
                    "outputs": "array, optional — output/result-column mapping, or a JSON-encoded string of one (Constants.WEBMETHOD_OUTPUTS). Each element is {MappedFrom, MappedTo, RecordSetName, Path?} — these are the ONLY keys read; an element using any other key names is silently treated as an all-empty mapping."
                  },
                  "notes": [
                    "requiresSource: true. Confirmed directly in DsfODBCDatabaseActivity.ToX6Json — this is the one database activity using DATABASE_COMMANDTEXT ('commandtext') rather than DATABASE_PROCEDURENAME."
                  ]
                }
                """,
            ["SQL Bulk Insert"] = """
                {
                  "fields": {
                    "tablename": "string, required — the destination table (Constants.SQLBULKINSERT_TABLENAME).",
                    "batchsize": "string, optional — rows per batch (Constants.SQLBULKINSERT_BATCHSIZE).",
                    "timeout": "string, optional — command timeout (Constants.SQLBULKINSERT_TIMEOUT).",
                    "checkconstraints": "boolean, optional — enforce table constraints during insert (Constants.SQLBULKINSERT_CHECKCONSTRAINTS).",
                    "firetriggers": "boolean, optional — fire insert triggers (Constants.SQLBULKINSERT_FIRETRIGGERS).",
                    "useinternaltransaction": "boolean, optional — wrap the bulk insert in its own transaction (Constants.SQLBULKINSERT_USEINTERNALTRANSACTION).",
                    "keepidentity": "boolean, optional — preserve identity column values from the source data (Constants.SQLBULKINSERT_KEEPIDENTITY).",
                    "keeptablelock": "boolean, optional — hold a table lock for the duration of the insert (Constants.SQLBULKINSERT_KEEPTABLELOCK).",
                    "ignoreblankrows": "boolean, optional — skip blank source rows (Constants.SQLBULKINSERT_IGNOREBLANKROWS).",
                    "inputmappings": "array, required — source-recordset-field-to-column mapping, or a JSON-encoded string of one (Constants.SQLBULKINSERT_INPUTMAPPINGS).",
                    "database": "string, required — the target database connection's resource id (Constants.SQLBULKINSERT_DATABASE).",
                    "result": "string, optional — output variable for the insert result (Constants.RESULT)."
                  },
                  "notes": [
                    "requiresSource: true — 'database' must reference an existing SQL Server connection resource."
                  ]
                }
                """,
            ["Redis Cache"] = """
                {
                  "fields": {
                    "key": "string, required — the cache key to read/write (Constants.REDISCACHE_KEY).",
                    "ttl": "string, optional — time-to-live for the cached value (Constants.REDISCACHE_TTL).",
                    "sourceid": "string, required — the Redis connection source id (Constants.REDISCACHE_SOURCEID).",
                    "response": "string, optional — output variable for the cache value/response (Constants.REDISCACHE_RESPONSE).",
                    "result": "string, optional — output variable for the operation result (Constants.RESULT)."
                  },
                  "notes": [
                    "requiresSource: true. Also writes a legacy ActivityFunc field (Constants.REDISCACHE_ACTIVITYFUNC) not required for authoring new bodies."
                  ]
                }
                """,
            ["Redis Remove"] = """
                {
                  "fields": {
                    "key": "string, required — the cache key to remove (Constants.REDIS_KEY).",
                    "response": "string, optional — output variable for the removal response (Constants.REDIS_RESPONSE).",
                    "sourceid": "string, required — the Redis connection source id (Constants.REDIS_SOURCEID).",
                    "result": "string, optional — output variable for the operation result (Constants.RESULT)."
                  },
                  "notes": [
                    "requiresSource: true."
                  ]
                }
                """,
            ["RabbitMQ Publish"] = """
                {
                  "fields": {
                    "rabbitmqsourceresourceid": "string, required — the RabbitMQ connection source id (Constants.RABBITMQPUBLISH_SOURCEID).",
                    "queuename": "string, required — the target queue/exchange name (Constants.RABBITMQPUBLISH_QUEUENAME).",
                    "isdurable": "boolean, optional — declare the queue as durable (Constants.RABBITMQPUBLISH_SETTINGS_DURABLE).",
                    "isexclusive": "boolean, optional — declare the queue as exclusive (Constants.RABBITMQPUBLISH_SETTINGS_EXCLUSIVE).",
                    "isautodelete": "boolean, optional — declare the queue as auto-delete (Constants.RABBITMQPUBLISH_SETTINGS_AUTODELETE).",
                    "message": "string, required — the message body to publish (Constants.RABBITMQPUBLISH_MESSAGE).",
                    "result": "string, optional — output variable for the publish result (Constants.RESULT)."
                  },
                  "notes": [
                    "requiresSource: true. The publishrabbitmqactivity alias (PublishRabbitMQActivity) additionally writes basicproperties (string, optional — Constants.RABBITMQPUBLISH_BASICPROPERTIES, AMQP basic-properties JSON) not present on the dsfpublishrabbitmqactivity alias (DsfPublishRabbitMQActivity)."
                  ]
                }
                """,
            ["RabbitMQ Consume"] = """
                {
                  "fields": {
                    "rabbitmqsourceresourceid": "string, required — the RabbitMQ connection source id (Constants.RABBITMQCONSUME_SOURCEID).",
                    "queuename": "string, required — the queue to consume from (Constants.RABBITMQCONSUME_QUEUENAME).",
                    "isobject": "boolean, optional — map the consumed message onto a single JSON object variable (Constants.RABBITMQCONSUME_ISOBJECT).",
                    "objectname": "string, optional — object variable for isobject (Constants.RABBITMQCONSUME_OBJECTNAME).",
                    "response": "string, optional — output variable for the raw consumed message (Constants.RABBITMQCONSUME_RESPONSE).",
                    "prefetch": "string, optional — prefetch count (Constants.RABBITMQCONSUME_PREFETCH).",
                    "timeout": "string, optional — consume timeout (Constants.RABBITMQCONSUME_TIMEOUT).",
                    "acknowledge": "boolean, optional — acknowledge the message after consuming (Constants.RABBITMQCONSUME_ACKNOWLEDGE).",
                    "requeue": "boolean, optional — requeue the message if processing fails (Constants.RABBITMQCONSUME_REQUEUE).",
                    "result": "string, optional — output variable for the consume result (Constants.RESULT)."
                  },
                  "notes": [
                    "requiresSource: true. Matches the exact field set the spec itself calls out (\"queuename/isobject/prefetch/acknowledge/requeue\") — verified directly in DsfConsumeRabbitMQActivity.ToX6Json, not just copied from the spec text."
                  ]
                }
                """,
            ["Send Email (SMTP)"] = """
                {
                  "fields": {
                    "smtpemail_sourceid": "string, required — the SMTP connection source id (Constants.SMTPEMAIL_SOURCEID).",
                    "smtpemail_fromaccount": "string, optional — override the source's From address (Constants.SMTPEMAIL_FROMACCOUNT).",
                    "smtpemail_password": "string, optional — override the source's password (Constants.SMTPEMAIL_PASSWORD).",
                    "smtpemail_to": "string, required — recipient address(es) (Constants.SMTPEMAIL_TO).",
                    "smtpemail_cc": "string, optional — CC address(es) (Constants.SMTPEMAIL_CC).",
                    "smtpemail_bcc": "string, optional — BCC address(es) (Constants.SMTPEMAIL_BCC).",
                    "smtpemail_priority": "string, optional — email priority (Constants.SMTPEMAIL_PRIORITY).",
                    "smtpemail_subject": "string, optional — email subject (Constants.SMTPEMAIL_SUBJECT).",
                    "smtpemail_attachments": "string, optional — CSV/JSON list of attachment paths (Constants.SMTPEMAIL_ATTACHMENTS).",
                    "smtpemail_body": "string, optional — email body (Constants.SMTPEMAIL_BODY).",
                    "smtpemail_ishtml": "boolean, optional — treat the body as HTML (Constants.SMTPEMAIL_ISHTML).",
                    "result": "string, optional — output variable for the send result (Constants.RESULT)."
                  },
                  "notes": [
                    "requiresSource: true. Field keys are prefixed smtpemail_ — confirmed directly in DsfSendEmailActivity.ToX6Json."
                  ]
                }
                """,
            ["Exchange Email"] = """
                {
                  "fields": {
                    "exchangesourceid": "string, required — the Exchange connection source id (Constants.EXCHANGEEMAIL_SOURCEID).",
                    "to": "string, required — recipient address(es) (Constants.EXCHANGEEMAIL_TO).",
                    "cc": "string, optional — CC address(es) (Constants.EXCHANGEEMAIL_CC).",
                    "bcc": "string, optional — BCC address(es) (Constants.EXCHANGEEMAIL_BCC).",
                    "subject": "string, optional — email subject (Constants.EXCHANGEEMAIL_SUBJECT).",
                    "attachments": "string, optional — CSV/JSON list of attachment paths (Constants.EXCHANGEEMAIL_ATTACHMENTS).",
                    "body": "string, optional — email body (Constants.EXCHANGEEMAIL_BODY).",
                    "ishtml": "boolean, optional — treat the body as HTML (Constants.EXCHANGEEMAIL_ISHTML).",
                    "result": "string, optional — output variable for the send result (Constants.RESULT)."
                  },
                  "notes": [
                    "requiresSource: true. Unlike Send Email (SMTP), Exchange Email's field keys are plain (to/cc/bcc/subject/...) rather than prefixed — confirmed directly in DsfExchangeEmailNewActivity.ToX6Json. Only the source id key (exchangesourceid) differs in naming convention from smtpemail_sourceid."
                  ]
                }
                """,
            ["JavaScript"] = """
                {
                  "fields": {
                    "script": "string, required — the JavaScript source to run (Constants.JAVASCRIPT_SCRIPT).",
                    "escapescript": "boolean, optional — escape the script text on serialization (Constants.JAVASCRIPT_ESCAPESCRIPT).",
                    "includefile": "string, optional — an additional script file to include (Constants.JAVASCRIPT_INCLUDEFILE).",
                    "result": "string, optional — output variable for the script's return value (Constants.JAVASCRIPT_RESULT)."
                  }
                }
                """,
            ["Ruby"] = """
                {
                  "fields": {
                    "script": "string, required — the Ruby source to run (Constants.RUBY_SCRIPT).",
                    "escapescript": "boolean, optional — escape the script text on serialization (Constants.RUBY_ESCAPESCRIPT).",
                    "includefile": "string, optional — an additional script file to include (Constants.RUBY_INCLUDEFILE).",
                    "result": "string, optional — output variable for the script's return value (Constants.RUBY_RESULT)."
                  }
                }
                """,
            ["Python"] = """
                {
                  "fields": {
                    "python_script": "string, required — the Python source to run (Constants.PYTHON_SCRIPT).",
                    "python_escapescript": "boolean, optional — escape the script text on serialization (Constants.PYTHON_ESCAPESCRIPT).",
                    "python_includefile": "string, optional — an additional script file to include (Constants.PYTHON_INCLUDEFILE).",
                    "python_result": "string, optional — output variable for the script's return value (Constants.PYTHON_RESULT)."
                  },
                  "notes": [
                    "Unlike JavaScript/Ruby, Python's field keys are prefixed python_ — confirmed directly in DsfPythonActivity.ToX6Json."
                  ]
                }
                """,
            ["Execute Command Line"] = """
                {
                  "fields": {
                    "commandfilename": "string, required — the executable/command to run (Constants.COMMANDLINE_COMMANDFILENAME).",
                    "commandpriority": "string, optional — the process priority, serialized as the underlying enum's int value (Constants.COMMANDLINE_COMMANDPRIORITY).",
                    "commandresult": "string, optional — output variable for the command's captured output (Constants.COMMANDLINE_COMMANDRESULT)."
                  }
                }
                """,
            ["Date and Time"] = """
                {
                  "fields": {
                    "datetime": "string, required — the input date/time value or expression (Constants.DOTNETDATETIME_DATETIME).",
                    "inputformat": "string, optional — the input's date/time format string (Constants.DOTNETDATETIME_INPUTFORMAT).",
                    "outputformat": "string, optional — the desired output format string (Constants.DOTNETDATETIME_OUTPUTFORMAT).",
                    "timemodifiertype": "string, optional — the unit to modify by (e.g. Days/Hours) (Constants.DOTNETDATETIME_TIMEMODIFIERTYPE).",
                    "timemodifieramountdisplay": "string, optional — display text for the modifier amount (Constants.DOTNETDATETIME_TIMEMODIFIERAMOUNTDISPLAY).",
                    "timemodifieramount": "number, optional — the numeric modifier amount (Constants.DOTNETDATETIME_TIMEMODIFIERAMOUNT).",
                    "result": "string, optional — output variable for the formatted/modified value (Constants.DOTNETDATETIME_RESULT)."
                  }
                }
                """,
            ["Date and Time Difference"] = """
                {
                  "fields": {
                    "input1": "string, required — the first date/time value (Constants.DATETIMEDIFF_INPUT1).",
                    "input2": "string, required — the second date/time value (Constants.DATETIMEDIFF_INPUT2).",
                    "inputformat": "string, optional — the shared input date/time format string (Constants.DATETIMEDIFF_INPUTFORMAT).",
                    "outputtype": "string, optional — the unit to express the difference in (e.g. Seconds/Minutes/Hours/Days) (Constants.DATETIMEDIFF_OUTPUTTYPE).",
                    "result": "string, optional — output variable for the difference (Constants.DATETIMEDIFF_RESULT)."
                  }
                }
                """,
            ["Gather System Information"] = """
                {
                  "fields": {
                    "systeminformationcollection": "array, required — a JSON array of { system-property, result-variable } rows selecting which system properties (OS, memory, etc.) to gather (Constants.GATHERSYSINFO_SYSTEMINFOCOLLECTION)."
                  }
                }
                """,
        };

        var builder = new Dictionary<string, JsonElement>(raw.Count);
        foreach (var (name, json) in raw)
        {
            builder[name] = Parse(json);
        }

        return builder.ToFrozenDictionary();
    }
}
