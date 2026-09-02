# Workflow Test Framework in Lightweight — Plan (not started)

> **Status:** planning only. No code in this document has been written yet. This file exists so
> the scope, phasing, and open decisions are recorded before implementation begins — per a
> request to expose `create_test`/`edit_test` MCP tools for Warewolf's in-app workflow testing
> framework ("Service Tests").
>
> **Companion document:** `docs/plans/create-edit-test-tools.md` in the `warewolf-devops-mcp`
> repo covers the MCP-server-side tool wrappers (Phase 4 below), which are blocked on this plan's
> Phase 1 shipping first.

## 1. Why this is a plan, not a small feature

`create_test`/`edit_test` look like natural siblings of the existing `create_workflow`/
`edit_workflow` MCP tools (`Mcp/ToolHandlers/CreateWorkflowTool.cs`,
`Mcp/ToolHandlers/EditWorkflowTool.cs`), which read/write `.bite` files directly. But Warewolf's
"Service Tests" feature — the in-app workflow testing framework — currently exists **only** in
`Dev2.Server`, and `Warewolf.Execution.Lightweight` has **no** test-framework code at all today:

- No `ServiceTest*`/`TestCatalog`/`IServiceTestModel` reference anywhere under
  `Dev/Warewolf.Execution.Lightweight`. *(confidence: high — repo-wide search, zero hits)*
- Explicitly excluded by a doc comment in
  [`Functions/WorkflowHttpFunction.cs:73-74`](../Functions/WorkflowHttpFunction.cs#L73-L74):
  *"Not supported in lightweight mode (require full Warewolf server): `*.tests, *.tests.trx,
  *.coverage*`, login, getlogfile."* *(confidence: high — read directly)*

So this isn't "add two tools" — it's "port a meaningful subset of a whole subsystem into
Lightweight, then add the tools." The rest of this document sizes that work and proposes phasing.

## 2. What exists today (`Dev2.Server`) — the feature being ported

*(confidence: high for all citations below — read directly during research)*

**Domain model** — parallel DTO/ViewModel hierarchy sharing interfaces in
`Dev2.Common.Interfaces`:
- `IServiceTestModel` (`Dev2.Common.Interfaces/IServiceTestModel.cs:12-54`) — `TestName`,
  `OldTestName`, `UserName`/`Password`, `LastRunDate`, `Inputs`/`Outputs`, `NoErrorExpected`/
  `ErrorExpected`/`ErrorContainsText`, `AuthenticationType`, `TestPassed`/`TestFailing`/
  `TestInvalid`/`TestPending`, `Enabled`, `TestSteps`.
- `IServiceTestInput` (`:56-61`) — `Variable`, `Value`, `EmptyIsNull`.
- `IServiceTestOutput` (`:63-76`) — `Variable`, `Value`, `From`, `To`, `AssertOp` (default `"="`),
  `HasOptionsForValue`/`OptionsForValue`, `Result`.
- `IServiceTestStep` (impl: `Dev2.Data/ServiceTestStepTO.cs:7-52`) — `ActivityID`/`UniqueID`,
  `ActivityType`, `Type` (`StepType`: Mock/Assert/…), `StepOutputs`, `Parent`/`Children` (nested,
  for decision/foreach steps), `StepDescription`, `Result`.
- Persistence DTOs: `Dev2.Data/ServiceTestModelTO.cs:18-53`, `ServiceTestStepTO.cs`,
  `ServiceTestInputTO.cs`, `ServiceTestOutputTO.cs`.

**Persistence** — one JSON file per test case, **not** embedded in the `.bite` file:
```
{AppDataPath}\Tests\{resourceId-guid}\{TestName}.test
```
- Root: `EnvironmentVariables.TestPath` (`Dev2.Diagnostics/Logging/EnvironmentVariables.cs:60-67`).
- Save/load/delete: `Dev2.Runtime.Services/TestCatalog.cs` — `SaveTestToDisk` (`:387-400`),
  `PersistTestToDisk` (`:402-425`), `Load`/`GetTestList` (`:427-462`), `DeleteTest` (`:489-506`).
- Format: Json.NET with `$type` discriminators (`Dev2JsonSerializer`), e.g. the fixture at
  `Dev/Server Tests Setup/Tests/b8cf4d58-3cc4-44bf-8780-57d8d39514ea/OutputIs10.test`. Passwords
  are DPAPI-encrypted before write.
- `TestCatalog.Instance` is a process-wide singleton fronted by an in-memory
  `ConcurrentDictionary<Guid, List<IServiceTestModelTO>>` cache.

**Server-side API** — internal ESB management-RPC, not discrete REST verbs:
- `SaveTests` (`Dev2.Runtime.Services/ESB/Management/Services/SaveTests.cs:152`) — create/update,
  bulk, args `resourceID`/`resourcePath`/`testDefinitions` (compressed serialized list).
- `FetchTests` (`.../FetchTests.cs:101`) — list, by `resourceID`.
- `DeleteTest` (`.../DeleteTest.cs:101`) — by `resourceID`/`testName`.
- Contract: `Dev2.Common.Interfaces/ITestCatalog.cs:9-24`.
- These travel over the same channel as workflow execution (`EsbExecuteRequest` →
  `EsbServicesEndpoint`), authorized via `AuthorizationContext.Contribute` per endpoint.
- **Run** is a genuine HTTP route reusing the workflow-execution controller:
  `Dev2.Runtime.WebServer/Controllers/WebServerController.cs:139-196` routes a `.tests`/
  `.tests.trx`/`.coverage*` suffix to `ExecuteFolderTests` (`:118-137`); execution logic lives in
  `Dev2.Runtime.WebServer/ServiceTesting/ServiceTestExecutor.cs` (`IServiceTestExecutor`).

**Studio (WPF) flow** — `ServiceTestViewModel`/`ServiceTestCommandHandlerModel`
(`Warewolf.Studio.ViewModels/`) call `IResourceRepository.SaveTests`/`LoadResourceTests`/
`ExecuteTest`/`DeleteTest` (`Dev2.Studio.Core/AppResources/Repositories/ResourceRepository.cs`),
which serialize an `EsbExecuteRequest` over the Studio's SignalR hub connection
(`Dev2.Studio.Core/Controller/CommunicationController.cs`). No Angular Web Studio equivalent
exists in this repo — the feature is WPF-Studio-only today.

## 3. Decisions already made (don't re-litigate these)

- **Target:** build a new, Lightweight-native test-definition capability. Do **not** bridge to
  `Dev2.Server`'s existing SignalR/`EsbExecuteRequest` channel or attempt file-format
  compatibility with `ServiceTestModelTO`'s `$type`-based Json.NET shape. Lightweight-created
  tests will not appear in WPF Studio's "Tests" tab; that interop gap is accepted, not a bug.
- **Schema fidelity:** full — including `TestSteps` (per-activity-step mocking/assertions,
  nested for decision/foreach), not just the input/output/error-expectation subset.
- **Tool names (engine side, unprefixed, matching `create_workflow`/`edit_workflow`):**
  `create_test`, `edit_test`.

## 4. Target schema (Lightweight-native JSON, System.Text.Json)

Modeled on `IServiceTestModel`/`IServiceTestStep` but expressed the way `create_workflow`'s
`envelope`/`body` already are — plain JSON, no `$type` discriminators, `System.Text.Json.JsonElement`
at the tool boundary:

```jsonc
{
  "testName": "OutputIs10",
  "enabled": true,
  "authenticationType": "Public",       // matches AuthenticationType values already used elsewhere
  "userName": null, "password": null,    // only meaningful for non-Public auth; DPAPI concerns don't
                                          // apply to Lightweight — see §6 open question on secrets
  "inputs": [
    { "variable": "[[a]]", "value": "4", "emptyIsNull": false }
  ],
  "outputs": [
    { "variable": "[[result]]", "value": "10", "assertOp": "=", "from": null, "to": null }
  ],
  "noErrorExpected": true, "errorExpected": false, "errorContainsText": null,
  "testSteps": [
    {
      "activityId": "guid-of-node-in-body",
      "activityType": "DsfDotNetMultiAssignActivity",
      "type": "Mock",                    // StepType: Mock | Assert
      "stepDescription": "Assign",
      "stepOutputs": [
        { "variable": "[[x]]", "value": "5", "assertOp": "=" }
      ],
      "children": []                     // nested steps for decision/foreach activities
    }
  ]
}
```

`activityId` values must resolve against the target workflow's current `body` (the X6 graph
nodes) — validated the same way `add_step`/`edit_workflow` already validate node references,
so a test written against a workflow shape that has since changed fails loudly at save time
rather than silently going stale.

## 5. Persistence design

New directory convention, keyed by the workflow's relative name (Lightweight resolves workflows
by name via `WorkflowIndex`, not by GUID resource ID the way `Dev2.Server` does):

```
{WorkflowsDirectory}/{relativePath}.tests/{TestName}.test.json
```

e.g. a workflow at `WorkflowsDirectory/Sales/CalculateTotal.bite` gets its tests under
`WorkflowsDirectory/Sales/CalculateTotal.tests/OutputIs10.test.json`.

New component: `Mcp/TestCatalog.cs` (namespace `Warewolf.Execution.Lightweight.Mcp`), mirroring
`WorkflowIndex`'s style (a small static/singleton cache over the filesystem, invalidated on
write) rather than `Dev2.Runtime.Services/TestCatalog.cs`'s heavier `ConcurrentDictionary<Guid,…>`
shape, since Lightweight has no resource-ID concept to key by.

## 6. Open questions / risks — resolve before starting the corresponding phase

1. **`execute_test` (Phase 3) is the large, risky part.** It requires injecting per-step mock
   outputs into a running workflow and evaluating `AssertOp` comparisons against actual step
   outputs — i.e. porting the intent of `ServiceTestExecutor`/`GetTestStepsAndOutputs`
   (`Dev2.Runtime/ESB/Execution/GetTestStepsAndOutputs.cs`). Mock injection means intercepting a
   specific activity's execution mid-workflow, which interacts directly with the shared-activity-
   instance caveat in `Part1-Architecture.md` (`Dev2.Activities/Activities/ActivityParser.cs:192-202`):
   Lightweight's `WorkflowExecutor` already pools *prepared workflows* per concurrent execution to
   avoid two executions racing on one activity's instance fields — a naive mock-injection design
   that mutates a pooled activity's state instead of the rented `DsfDataObject` would reintroduce
   exactly that bug. **Do not start Phase 3 without an explicit design review of how mocks are
   injected without touching pooled activity instances.** *(still open — unaffected by the
   Phase 1 decisions below.)*
2. **Secrets — RESOLVED for Phase 1.** Route through the existing `IMcpSecretResolver` +
   `DpapiWrapper.Encrypt` pipeline `AddSourceTool.cs` already uses for source passwords
   (`ResolveConfigAsync`/`ResolveSecretPlaceholdersAsync` at `AddSourceTool.cs:143,187-233,246+`,
   encryption at `AddSourceTool.cs:147`), not a new mechanism. This also answers the DPAPI
   cross-instance concern raised here: on this host, `DpapiWrapper.Encrypt` is not real per-machine
   Windows DPAPI — `Security/FileEncryptionHelper.cs` registers itself as
   `DpapiWrapper.AesEncryptHook` at startup (wired in `Infrastructure/ServiceCollectionExtensions.cs`),
   so the actual ciphertext is AES-256-GCM, which *does* survive across scaled Function App
   instances. See §7a for the concrete flow.
3. **Auth model — RESOLVED.** Adopted as proposed: Contribute permission on the **target
   workflow's** relative path (resource-if-present else global, Public OR'd,
   `ListWorkflowsTool.HasPermission`), no separate "test" permission concept. Tests have no
   independent ACL entry in `secure.config`.
4. **Format compatibility with `Dev2.Server`** is deliberately out of scope per §3. If that
   changes later, it's a breaking rework of §4/§5, not an additive one.
5. **activityId validation mechanism — RESOLVED for Phase 1.** Reuse
   `GetWorkflowDefinitionTool.BuildBody` (`GetWorkflowDefinitionTool.cs:110-200`) to resolve the
   target workflow's current body rather than writing an independent XAML scan. Accepted
   trade-off: a workflow containing any activity type that hasn't passed the fidelity gate
   (`FidelityAllowList`) cannot have tests authored against it in Phase 1 — same restriction
   `get_workflow_definition`/`add_step` already live with.
6. **Test-step/input/output variable scope — RESOLVED, deliberate asymmetry.** Unlike
   `validate_workflow`'s body-cell rule (`ValidateWorkflowTool.ValidateVariableReferences`,
   `ValidateWorkflowTool.cs:615`), `create_test`/`edit_test` do **not** require a test's
   `inputs`/`outputs`/`stepOutputs` variable names to already be declared in the workflow's
   envelope inputs/outputs. This matches `Dev2.Server`'s real behaviour — mocking/asserting
   mid-workflow state doesn't require that state to be a public envelope input/output — but it is
   a new, unprecedented validation rule (or rather, absence of one) in this codebase, so it's
   called out explicitly rather than left to look like an oversight.

## 7. Phased plan

| Phase | Scope | New engine-side artifacts |
|---|---|---|
| **1 (requested now)** | `create_test`, `edit_test` — write-only CRUD, full schema (§4) persisted per §5, validated against the workflow's current `body` (activity-id references must resolve; variable-name references are **not** cross-checked against the envelope, per §6.6) but **not executable** yet. | `Mcp/ToolHandlers/CreateTestTool.cs`, `Mcp/ToolHandlers/EditTestTool.cs`, `Mcp/TestCatalog.cs`, `McpApiFunctions.cs` routing + request records for `create_test`/`edit_test` — see §7a for the concrete design, now that Phase 1's open decisions (§6.2, §6.3, §6.5, §6.6) are resolved |
| **2** | `list_tests`, `get_test`, `delete_test` — completes read/delete so a caller can discover a test's current shape before editing it, rather than needing to remember the full definition it last wrote. | matching `ToolHandlers/*Tool.cs` + `TestCatalog` read/delete methods |
| **3** | `execute_test` — the real test runner: mock injection + `AssertOp` evaluation, pass/fail reporting. Needs the design review in §6.1 before starting. | `Mcp/LightweightServiceTestExecutor.cs` (name tentative), result-shape types |
| **4** | `warewolf-devops-mcp` repo: `warewolf_create_test`/`warewolf_edit_test` wrappers (then list/get/delete/execute in lockstep with phases 2-3) — see `docs/plans/create-edit-test-tools.md` in that repo. | (other repo) |

Each phase ships independently; Phase 2 is recommended before Phase 4 wraps only Phase 1's two
tools, since an `edit_test` caller with no `list_tests`/`get_test` has to already know the exact
current shape of the test it's editing.

## 7a. Phase 1 — concrete implementation plan

All of §6's Phase-1-relevant open questions (§6.2, §6.3, §6.5, §6.6) are now resolved. This
section is the approved-pending-review design; no code has been written yet.

### 7a.1 New / changed files

| File | Change |
|---|---|
| `Mcp/TestCatalog.cs` | **New.** Persistence — path resolution, existence check, save. No in-memory cache (see §7a.3). |
| `Mcp/ToolHandlers/CreateTestTool.cs` | **New.** `create_test` handler + shared parse/validate helpers reused by `EditTestTool`. |
| `Mcp/ToolHandlers/EditTestTool.cs` | **New.** `edit_test` handler — thin, mirrors `EditWorkflowTool` vs. `CreateWorkflowTool`. |
| `Functions/McpApiFunctions.cs` | Two new `[Function]` HTTP routes + one new request record (§7a.5). |

No changes needed to `GetWorkflowDefinitionTool.cs` — `BuildBody` (`:110`) is already `internal
static` and reusable as-is (§6.5).

### 7a.2 Tool signature

```csharp
internal static async Task<CreateTestResult> Handle(
    HostEnvironmentConfig hostConfig,
    IWorkflowAuthPolicyLoader authPolicyLoader,
    IMcpSecretResolver secretResolver,
    ClaimsPrincipal? user,
    [Description("The owning workflow's name — same lookup create_workflow/edit_workflow use. Must already exist.")]
    string name,
    [Description("The test definition — { testName, enabled?, authenticationType?, userName?, password?, inputs[], outputs[], noErrorExpected?, errorExpected?, errorContainsText?, testSteps[] } per §4's schema.")]
    JsonElement test,
    CancellationToken cancellationToken = default)
```

`edit_test` has the identical signature; behavioural differences are in §7a.4. There is
deliberately **no separate `testName` parameter** — unlike a workflow (whose file-path identity
`name` and its `envelope.name` display label can legitimately differ, which is why
`CreateWorkflowTool.ResolveDisplayName` exists), a test has no such split: its only identity is
its name within the owning workflow's `.tests` folder, so `test.testName` alone is both identity
and content.

### 7a.3 `Mcp/TestCatalog.cs`

```csharp
internal static class TestCatalog
{
    internal static string TestsDirectoryFor(string workflowsDirectory, string workflowRelativePath);
    internal static string TestFilePath(string workflowsDirectory, string workflowRelativePath, string testName);
    internal static bool Exists(string workflowsDirectory, string workflowRelativePath, string testName);
    internal static void Save(string workflowsDirectory, string workflowRelativePath, string testName, string jsonContents);
}
```

`TestsDirectoryFor` implements §5's convention: `{WorkflowsDirectory}/{relativePath}.tests/`
(`relativePath` = the workflow's name without the `.bite` extension, forward-slash separated,
matching `WorkflowIndex`'s own key normalisation). `Save` creates that directory if absent, then
`File.WriteAllText`s `{TestFilePath}`.

Deliberately **no in-memory cache**, unlike `WorkflowIndex`'s `FrozenDictionary` cache — Phase 1
has no repeated-read hot path (`create_test`/`edit_test` each do at most one `Exists()` check plus
one write); a cache only pays for itself once Phase 2's `list_tests`/`get_test` add read pressure
over a directory. This deviates from §5's "mirroring `WorkflowIndex`'s style" wording — noted here
as a deliberate Phase 1 simplification (this repo's CLAUDE.md: don't design for hypothetical
future requirements), to be revisited in Phase 2, not an oversight.

**New validation not mirrored from any existing tool:** `test.testName` must be a legal single
filename component — reject if it contains `/`, `\`, `..`, or any of
`Path.GetInvalidFileNameChars()`. Every existing `name`-taking tool (`create_workflow`,
`add_source`) *intentionally* allows `/`-separated subpaths in `name`; a test name is not a path,
so this is a new, stricter rule specific to `TestCatalog`.

### 7a.4 `create_test` pipeline, in order

1. `name` required; `test` must be a JSON object; `test.testName` required, non-blank, and legal
   per §7a.3's filename rule.
2. Resolve `name` via `WorkflowNameResolver.Resolve` — the workflow must already exist (a test
   always belongs to an existing workflow) — McpException if not found.
3. Permission: Contribute on the workflow's relativePath via `ListWorkflowsTool.HasPermission`
   (§6.3) — same rule, same resource, as `create_workflow`/`edit_workflow`.
4. `TestCatalog.Exists(...)` must be **false** — McpException "a test named '{testName}' already
   exists for workflow '{name}'" otherwise.
5. Resolve the workflow's current body via `GetWorkflowDefinitionTool.BuildBody(existingFilePath,
   headerName)` (§6.5). If `bodyEditable` is false, McpException surfacing the same
   `nonEditableReason` `get_workflow_definition` would report.
6. Parse + validate the rest of `test` (shared `internal static` helper on `CreateTestTool`,
   reused by `EditTestTool` — same sharing pattern as `CreateWorkflowTool.ResolveDisplayName`/
   `ResolveDescription`):
   - `authenticationType` must parse as `Dev2.Runtime.ServiceModel.Data.AuthenticationType`
     (Windows/User/Anonymous/Public/Password/API_Key).
   - Every `testSteps[].activityId`, and recursively every `children[].activityId`, must match a
     `cell.id` present in the resolved body's `cells` array — collect every failure, then throw
     one `McpException` joining them with `"; "` (mirrors `create_workflow`'s validation-failure
     reporting, `CreateWorkflowTool.cs:87-92`).
   - `testSteps[].type` (and recursively `children[].type`) must parse as
     `Dev2.Common.Interfaces.StepType` — confirmed closed 2-value enum, `Mock` or `Assert`
     (`Dev2.Common.Interfaces/StepType.cs`).
   - No cross-check of `inputs`/`outputs`/`stepOutputs` variable names against the envelope
     (§6.6, deliberate).
7. Secrets: if `password` is non-blank, it must contain a `${NAME}` placeholder — a literal
   secret is rejected outright, mirroring `AddSourceTool`'s tool-description rule
   (`AddSourceTool.cs:109`, `SourceCatalog`'s "Use ${secret-name} — never a literal secret").
   Resolve it via `secretResolver.ResolveAsync` (same regex/error handling as
   `AddSourceTool.ResolveSecretPlaceholdersAsync`), then `DpapiWrapper.Encrypt` the resolved value
   before it is serialized into the persisted JSON.
8. Serialize the validated, secret-resolved model to JSON (`System.Text.Json`, camelCase
   properties per §4) and write via `TestCatalog.Save(...)`.
9. Return `CreateTestResult(name, testName, created: true)`.

**`edit_test` differences (§7a.4a):**
- Step 4 inverts: `TestCatalog.Exists(...)` must be **true** — McpException "test '{testName}'
  was not found for workflow '{name}'; use create_test to create a new test" (mirrors
  `EditWorkflowTool.cs:107`'s not-found message).
- No rename support in Phase 1 — `test.testName` identifies which existing file to overwrite;
  same "does not move or rename" restriction `EditWorkflowTool.cs:51-52` already documents for
  workflows. A rename is `create_test` + (once Phase 2 exists) `delete_test`.
- Otherwise identical (steps 1-3, 5-9 unchanged) — overwrites the existing `.test.json` file in
  place.

### 7a.5 `Functions/McpApiFunctions.cs` additions

Two new `[Function]` routes, following `McpApiCreateWorkflow`/`McpApiEditWorkflow`'s exact shape
(`McpApiFunctions.cs:174-192`) — `mcp-api/create_test`, `mcp-api/edit_test` — plus one new request
record alongside the others at `McpApiFunctions.cs:408-428`:

```csharp
sealed record NamedTestRequest(string Name = "", JsonElement Test = default);
```

Both routes need `_secretResolver` passed through (already a constructor-injected field — `add_source`/`edit_source` use it today), so no new DI wiring.

### 7a.6 Response shapes

```csharp
internal sealed record CreateTestResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("testName")] string TestName,
    [property: JsonPropertyName("created")] bool Created);

internal sealed record EditTestResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("testName")] string TestName,
    [property: JsonPropertyName("updated")] bool Updated);
```

### 7a.7 Confirming what's out of scope (per §7's table, unchanged)

- No `list_tests`/`get_test`/`delete_test` (Phase 2) — an `edit_test` caller must already know
  the test's exact current shape; a Phase 1 test, once written, can only be inspected by reading
  the `.tests/*.test.json` file directly off disk.
- No execution (`execute_test`, Phase 3) — `testSteps`, mock outputs, and `AssertOp` are
  persisted but never evaluated.

## 8. Test strategy (Phase 1, when implementation starts)

Per this repo's CLAUDE.md, unit tests are proposed and require explicit go-ahead before being
written — not written automatically alongside this plan. When Phase 1 implementation begins, the
proposed plan is:

- `Warewolf.Execution.Lightweight.Tests/Mcp/ToolHandlers/CreateTestToolTests.cs` and
  `EditTestToolTests.cs`, following `CreateWorkflowToolTests.cs`'s established conventions
  (`Dev/Warewolf.Execution.Lightweight.Tests/Mcp/ToolHandlers/CreateWorkflowToolTests.cs:1-80`):
  temp-directory `WorkflowsDirectory`, a `StubAuthPolicyLoader`, and a `WorkflowClaimsPrincipal`
  builder helper.
- Cases: required-parameter validation, activity-id/variable-name reference validation against
  the target workflow's current `body`, Contribute-permission gating (reusing
  `ListWorkflowsTool.HasPermission`), name-already-exists (`create_test`) / not-found
  (`edit_test`) rejection, and the success path — a written `.test.json` file that round-trips
  correctly through a subsequent `get_test`/`list_tests` call once Phase 2 exists (or, until then,
  direct file-content assertions).
- `Warewolf.Execution.Lightweight.Tests/Mcp/TestCatalogTests.cs` for the new persistence
  component in isolation (save/overwrite behavior, filename-legality rejection per §7a.3).
