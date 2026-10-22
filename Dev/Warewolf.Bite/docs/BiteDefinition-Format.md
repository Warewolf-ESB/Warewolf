# BiteDefinition format, version 1

This is the JSON body for `.bite` workflow resources, introduced by the Windows Workflow Foundation (WF) removal, task T1.5. It replaces the WF XAML in `<XamlDefinition>`.

- **Reader:** `Warewolf.Bite.BiteReader`. It still reads legacy XAML and always will.
- **Writer core:** `Warewolf.Bite.Json.BiteJsonWriter`.

> **Status: readable, not yet written by default.** Only an engine with `BiteReader` as its reader (T1.8) can run a JSON body. Saving in JSON comes later, behind a minimum-engine-version gate (T3.4). Until then the writer is used for tests and migration tooling only.

## Envelope

The `<Service>` XML envelope is unchanged: the catalog, explorer, versioning and deploy tools all read it as they do now. Only the body changes:

```xml
<Service ID="…" Name="…" ResourceType="WorkflowService" …>
  …
  <Action Name="InvokeWorkflow" Type="Workflow">
    <BiteDefinition FormatVersion="1">{ …json… }</BiteDefinition>
  </Action>
  …
</Service>
```

- **Detection:** when `<BiteDefinition>` is present, it is used even if a `<XamlDefinition>` is also there.
- **Versioning:** `FormatVersion` must be `1`. Any other value fails with a `BiteFormatException`. There is no best-effort reading of unknown versions.

## Body

```json
{
  "start": "n0",
  "nodes": {
    "n0": { "kind": "step", "activity": { … }, "next": "n1" },
    "n1": { "kind": "decision", "displayName": "Decision", "condition": { … }, "true": "n2", "false": null },
    "n2": { "kind": "switch", "displayName": "Switch", "expression": { … }, "cases": { "1": "n3" }, "default": null },
    "n3": { "kind": "step", "activity": { … }, "next": null }
  }
}
```

| Field | Meaning |
|---|---|
| `name` | Optional. The workflow's name, as XAML's `x:Class` carries it. |
| `start` | The ID of the start node, or `null` for a workflow with no start node. |
| `nodes` | Every flowchart node, in document order, **including nodes not reachable from `start`**. Those are designer content, and execution ignores them. |
| `kind` | `step`, `decision` or `switch`. |
| `next` / `true` / `false` / `default` / `cases.*` | A successor node ID, or `null`. IDs may point to any node, including back-edges and shared successors. |
| `displayName` | On decisions and switches. It defaults to `"Decision"` or `"Switch"` when absent, as WF does. |
| `layout` | Optional `{ "<nodeId>": { "x": …, "y": … } }` for designers. Engines ignore it. |

## Values

| Value | JSON |
|---|---|
| string, bool, number | JSON primitive, culture-invariant |
| enum | Its name (`"NumOfExecution"`) |
| `Guid` | `"D"` format |
| `DateTime` | ISO 8601 round-trip (`"O"`) |
| `TimeSpan` | `"c"` format |
| `Type` | The full CLR name (`"System.String"`), resolved through the allowlist |
| object | `{ "$type": "<Namespace.TypeName>", "<Member>": value, … }`, with members in ordinal name order |
| collection | Array |
| dictionary | Object keyed by the key's text |
| WF `InArgument` holding a literal | The plain value |
| WF argument holding an expression | `{ "$expr": "<expression text>" }`, kept as text and never compiled |
| WF argument with nothing in it | `{}` |
| `ActivityFunc` / `ActivityAction` (container bodies) | `{ "argument": "<delegate argument name>", "body": <activity> }` |

Notes on these rules:

- **Types:** `$type` holds a namespace-qualified CLR name without the assembly. It resolves through the same allowlist as XAML: Warewolf types, a fixed set of BCL value and collection types, and the WF argument and delegate types. Anything else fails loudly.
- **Members:** every public, readable member that is settable, or is a collection that items are added to, is written, and nulls are written too. There are three exceptions:
  - runtime state is skipped: `Compiler`, `DataObject`, `ExecutionEnvironment` and `ScopingObject`;
  - on activities, the constructor-set VB references are skipped: `HasError`, `IsValid` and `InstructionList`;
  - members declared on WF base types are skipped, except `DisplayName`.
- **The argument and body shapes deliberately ignore the WF types.** When Workstream 2 replaces those types with plain ones, existing JSON bodies stay valid.

## Schema

[`BiteDefinition-v1.schema.json`](BiteDefinition-v1.schema.json) is the JSON Schema (draft 2020-12) for the body.

- **Generated, not hand-written:** `Warewolf.Bite.Json.BiteJsonSchema` derives it from the same rules the reader and writer use. It covers every public, constructible `IDev2Activity` type in the assemblies `Dev2.Activities` references, and every type their members reach. A member's schema follows the Values table above.
- **Scope:** it checks shape — node kinds, member names, `$type` names and value types. It does not check what a reader resolves at run time, such as whether a successor ID exists or an expression compiles.
- **Kept current by a test:** `BiteJsonSchema_CheckedInCopy_IsCurrent` fails when a change to an activity or to the rules alters the output. Regenerate with `WAREWOLF_REGENERATE_BITE_SCHEMA=1` and commit the result.

## IDs

- **Stored IDs win.** The writer stores every activity's `UniqueID`, including IDs that were minted when a legacy XAML file without them was read.
- **Missing IDs** (for example, hand-written JSON) get a stable ID derived from the resource ID and the JSON path, never a new random one on each load.

## Guarantees and their tests

| Guarantee | Test |
|---|---|
| Each of the 903 corpus workflows (113 generated, 680 repo, 110 external) goes XAML → JSON → graph equal to both the XAML read and the WF path | `Dev2.Activities.Tests` → `WfParity_JsonRoundTrip_*` |
| Writer output is deterministic and independent of the current culture | `Warewolf.Bite.Tests` → `BiteJsonTests` |
| Unknown versions, disallowed types and dangling successor IDs fail loudly, naming the JSON path | `Warewolf.Bite.Tests` → `BiteJsonTests` |
| The JSON body of every corpus workflow validates against the schema, and the checked-in schema matches the generator | `Warewolf.Bite.Tests` → `BiteJsonSchemaTests` |
| XAML written from the model (`BiteXamlWriter`, used by Web Studio and MCP saves) loads on the WF path to the same graph as the original, for all 903 corpus workflows | `Dev2.Activities.Tests` → `WfParity_XamlRewrite_*` |

**Size:** measured against the original XAML body, the JSON body is 78% for the generated corpus, 76% for the repo workflows, and 62% for the WPF-authored external workflows, whose view state goes away.
