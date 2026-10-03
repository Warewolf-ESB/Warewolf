# Plan: legacy workflow tests + coverage routes on the Lightweight engine

Status: **draft for approval**. The open questions are answered (section 8, 2026-10-02). No code or
tests have been written. Per `CLAUDE.md`, both this plan and the unit-test plan in section 7 need
sign-off before implementation starts.

## 1. Goal

Let a project that used to run its workflow tests on a Warewolf Server container run the same
tests, and get the same coverage report, from the Lightweight engine:

```
GET /Secure/{folder}/.tests          -> test results JSON           (Server: EmitionTypes.TEST)
GET /Secure/{folder}/.coverage       -> coverage report HTML        (Server: EmitionTypes.Cover)
GET /Secure/{folder}/.coverage.json  -> coverage + test summary JSON (Server: EmitionTypes.CoverJson)
```

`{folder}` may be empty (all workflows) or a workflow name (`{folder}/{workflow}.tests`), as on the
Server. The first consumer is the profiler's Dev pipeline (`Unlimited.ProfilerServices`,
`ProfilerService-CI-Dev.yml`). Its old PR job called exactly these three routes from
`SolutionItems/Scripts/RunAllWarewolfServerTests.ps1` against a container built from its
`Dockerfile`, which copied `Warewolf.ProfilerServices.Tests/Profiler` to
`C:/ProgramData/Warewolf/Tests/`. Those are 85 legacy `.test` files in 43 folders named by workflow
GUID; 82 of them define `TestSteps`, 477 top-level steps in total.

Out of scope: `.tests.trx`; persisting run results or coverage reports to disk (the Server writes
`LastRunDate`/`TestPassed` and saves coverage reports; the engine runs from a read-only package);
Studio interop; the engine's own `.test.json` format and `execute_test` tool, which stay unchanged.

## 2. Findings this plan rests on

| # | Finding | Evidence | Confidence |
|---|---|---|---|
| F1 | The engine does not serve `*.tests`, `*.tests.trx`, `*.coverage*` today. | `Functions/WorkflowHttpFunction.cs:73-74`; `docs/WorkflowTestFramework-Plan.md` §1 | high, read directly |
| F2 | Server coverage is **static**: it compares each test's `TestSteps` (`ActivityID`, else `UniqueID`, recursing `Children`) with the workflow's nodes from its XAML flowchart. Pass/fail and execution data are not inputs. | `Dev2.Data/SingleTestNodesCovered.cs:20-50`, `Dev2.Data/WorkflowCoverageReports.cs`, `Dev2.Runtime.Services/ServiceTestCoverageModelToFactory.cs`, `Dev2.Runtime.Services/ServiceModel/Data/Workflow.cs:112-200,251-300` | high, read directly |
| F3 | The Server only generates the coverage report as a side effect of a test run, then saves it. Totals are aggregated by `WarewolfWorkflowReports.Calculte`. | `Dev2.Runtime.WebServer/DataObjectExtensions.cs:470-523`; `Dev2.Data/WarewolfWorkflowReports.cs:47-70` | high, read directly |
| F4 | The engine already has every assembly the calculation needs: `Dev2.Activities` → `Dev2.Data`, `Dev2.Runtime.Services`. | `Warewolf.Execution.Lightweight.csproj:173-181`; `Dev2.Activities.csproj` references | high, read directly |
| F5 | The engine can already run one legacy-model test: `WorkflowExecutor.ExecuteTest(TestExecutionRequest)` takes an `IServiceTestModelTO` and evaluates steps, mocks and outputs with the shared `Dev2.Activities` code the Server uses. | `Execution/WorkflowExecutor.cs:524-660` | high, read directly |
| F6 | The Server loads legacy tests with `Dev2JsonSerializer` (`$type` Json.NET) from `{AppDataPath}/Tests/{resourceId}/*.test`, sorted by name. Its `TestCatalog` is a static singleton bound to `EnvironmentVariables.TestPath` and writes to disk, so it cannot be reused as-is. | `Dev2.Runtime.Services/TestCatalog.cs:35-60,431-468`; `Dev2.Diagnostics/Logging/EnvironmentVariables.cs:60-75` | high, read directly |
| F7 | The HTML builder (`ServiceTestCoverageModelHTMLResultBuilder`, 490 lines) is in `Dev2.Runtime.WebServer`, which the engine does not reference, and it writes through `System.Web.UI.HtmlTextWriter`. Where that type comes from in the net10 build is not yet confirmed. | `Dev2.Runtime.WebServer/ServiceTesting/ServiceTestCoverageModelHTMLResultBuilder.cs:19,132` | medium |
| F8 | The deploy script copies `-WorkflowsSourcePath` into the package's `Resources/` and, with `-EncryptResources`, encrypts connection strings only. There is no tests parameter. | `Scripts/Deploy-WwExecutionEngine.ps1:2956-2966,3087-3100` | high, read directly |
| F9 | `TestDebugMessageRepo` is a process-wide static keyed by `(resourceId, testName)`, so concurrent runs of the same test can see each other's container-child states. | `Execution/TestDebugCapturer.cs` remarks | high, read directly |
| F10 | `functionTimeout` is 10 minutes, but Azure's front end drops HTTP requests after about 230 s. | `host.json:3`; Azure Functions HTTP limits | high / medium |

## 3. Design

### 3.1 Test source: read-only legacy catalog

New `Testing/LegacyTestCatalog.cs` (namespace `Warewolf.Execution.Lightweight.Testing`):

- **Location:** loads `{TestsDirectory}/{guid}/*.test` once, lazily, with `Dev2JsonSerializer`. It
  skips non-GUID folders, and logs and skips files it cannot deserialize, as `TestCatalog.GetTestList`
  does. Files are read in ordinal-ignore-case order.
- **Read-only:** it never writes; results are kept in memory for the duration of one request.
- **`TestsDirectory`:** a new `HostEnvironmentConfig` setting that defaults to `Tests/` beside the
  package's `Resources/`. It comes from the deploy-bundled settings file, like `WorkflowsDirectory`
  (WOLF-8516: no env-var fallback).
- **GUID to workflow:** `WorkflowGuidIndex` scans `WorkflowsDirectory` for `.bite` files, reads each
  file's resource `ID` with the existing `ExtractResourceIdentity` logic, and maps GUID to file path
  and relative name. It is cached and invalidated the same way `WorkflowIndex` is.
- **Orphans:** a test folder whose GUID matches no workflow is skipped and logged as a warning,
  never reported as a result. That is what the Server does: the profiler's three `acb75027…` tests
  appear nowhere in build 73014's results (section 9).

### 3.2 Running tests: `Testing/LegacyTestRunner.cs`

- **Scope:** resolves the requested folder or workflow to workflows that have tests.
- **Permissions:** keeps only the workflows the caller holds **Execute** on, using the same matcher
  call as `ExecuteTestTool`.
- **Execution:** runs each test through `WorkflowExecutor.ExecuteTest`, one at a time (F9), and
  copies the evaluated `TestSteps` and outputs back onto the returned `IServiceTestModelTO`. The
  shape matches what the Server's `ServiceTestExecutor` returns.
- **Prerequisite engine fixes (phase 0, section 9):** as it stands, `ExecuteTest` passes only 50 of
  the 82 profiler tests that pass on the Server. Two gaps must be fixed first:
  - **G1, pass rule:** adopt the Server's `Evaluator.UpdateFailureMessageArgs` rule.
  - **G2, recordset inputs:** load the test's recordset inputs as recordsets.

  G3, the remaining single difference, is investigated in phase 2.
- **Time budget:** runs against a request deadline (default 200 s, configurable). When the deadline
  passes, the remaining tests come back as `TestInvalid` with "not run: request time budget
  exceeded" instead of the request dying at 230 s (F10). Callers that need more time narrow the
  scope (section 5).

### 3.3 Coverage: reuse, don't reimplement

- **Node lists:** built with the Server's `Dev2.Runtime.Services.ServiceModel.Data.Workflow` node
  walk, but **not** through its `Workflow(XElement)` XAML read.
  - **Why not the XAML read:** phase 0 showed it fails on net10, Windows and Linux alike, with
    `Cannot set unknown member '…/presentation}VirtualizedContainerService.HintSize'`. It catches and
    logs the failure and returns zero nodes.
  - **Instead:** clean the XAML exactly as `WorkflowExecutor.LoadDynamicActivity` does
    (`Dev2XamlCleaner.CleanServiceDef` + `Dev2XamlLoader.RemoveWindowsElements`). Load it with
    `ActivityXamlServices.CreateBuilderReader`, and hand the flowchart's nodes to the existing
    `Workflow(Collection<FlowNode>)` constructor.
  - **Result:** this reproduces the container's node counts exactly (section 9). `Workflow` itself is
    not changed.
- **Scope:** every workflow in the requested folder goes into the calculation, including workflows
  with no tests. The Server's total uses them in the denominator: 350 nodes = 318 in tested
  workflows + 32 in untested ones.
- **Calculation:** feeds the existing `WarewolfWorkflowReports.Calculte(ITestCoverageCatalog,
  ITestCatalog)` through two small in-memory adapters:
  - `InMemoryTestCatalog` returns this request's run results.
  - `InMemoryTestCoverageCatalog` builds reports with the same `SingleTestNodesCovered` logic as
    `ServiceTestCoverageModelToFactory`, without `IResourceCatalog` and without disk writes.

  Only the members `Calculte` calls are implemented; the rest throw `NotSupportedException`. The
  percentages are then produced by the Server's own code, which is the point: identical numbers.
- **Stateless:** `.coverage` and `.coverage.json` run the tests for their scope themselves, because
  the engine keeps no run state between requests. A caller hitting all three routes runs the tests
  three times; acceptable while the suite is fast (phase 0 measures it).

### 3.4 Output builders

- **JSON:** port `RunCoverageAndReturnJSON`'s object shape (`StartTime`, `EndTime`,
  `CoverageSummary`, `TestSummary`, `TestResults`) and the `.tests` result shape
  (`ServiceTestModelJObjectResultBuilder`). Consumers read
  `CoverageSummary.TotalCoverage` and `TestSummary.TestsFailed`.
- **HTML:** move `ServiceTestCoverageModelHTMLResultBuilder` and the `Setup*Html` helpers it uses
  into a project both hosts reference, with `Dev2.Data` as the default candidate. The Server then
  calls the moved code, so its output does not change. `HtmlTextWriter` moves with it: phase 0
  found it is the MIT-licensed NuGet package `HtmlTextWriter` 2.1.1 (netstandard2.0/2.1), not
  `System.Web` (F7 resolved). No port is needed.
- **Duplicates:** the Server's `.tests` JSON lists every test twice (build 73014: 164 rows for 82
  tests). The engine lists each test once. No consumer counts those rows; the gate reads
  `TestSummary.TestsFailed`.

### 3.5 Routes

- **Dispatch:** in `WorkflowHttpFunction.ExecuteNamedWorkflow`, add `RouteCase.TestRun` and
  `RouteCase.Coverage`, detected by `NameSuffixParser` (new `ParseTestSuffix`). They are evaluated
  after `ApisJson` and only on the `Secure/` route.
- **Public:** on `Public/` these suffixes return the existing wrapped "invalid permissions" 500
  (WOLF-8418 convention). Tests can hit databases and must not be anonymously triggerable.
- **Authentication:** reuses the Secure principal check that already runs for `SecureExecution`. No
  change to `NotReservedSecureSubPathPattern`, since these are suffixes inside the catch-all, not
  sibling routes.
- **Response types:** `.tests` and `.coverage.json` return `application/json`; `.coverage` returns
  `text/html; charset=utf-8`.

### 3.6 Deploy script

`Deploy-WwExecutionEngine.ps1` gets `-TestsSourcePath`:

- **Staging:** copies into the package's `Tests/` and writes `TestsDirectory` into the bundled
  settings file. Absent means no `Tests/` folder, so the routes report "no tests found".
- **Encryption:** tests are not encrypted. They hold inputs and expected values, not sources;
  `-EncryptResources` stays limited to `Resources/` and settings.
- **Docs and summary:** the summary JSON and `Scripts/README.md` gain the parameter. The Pester
  suite gets cases (section 7).

## 4. Affected files

| Area | Files |
|---|---|
| New | `Testing/LegacyTestCatalog.cs`, `Testing/WorkflowGuidIndex.cs`, `Testing/LegacyTestRunner.cs`, `Testing/InMemoryTestCatalog.cs`, `Testing/InMemoryTestCoverageCatalog.cs`, `Testing/TestRouteResponseBuilder.cs` |
| Changed | `Functions/WorkflowHttpFunction.cs` (route cases, doc comment lines 73-74), `Services/NameSuffixParser.cs`, `Infrastructure/HostEnvironmentConfig.cs` (+ settings file model), DI registration in `Infrastructure/ServiceCollectionExtensions.cs` |
| Shared move (3.4) | `Dev2.Runtime.WebServer/ServiceTesting/ServiceTestCoverageModelHTMLResultBuilder.cs` → shared project; `DataObjectExtensions.cs` call sites updated |
| Scripts | `Scripts/Deploy-WwExecutionEngine.ps1`, `Scripts/Tests/Deploy-WwExecutionEngine.Tests.ps1`, `Scripts/README.md` |
| Docs | this plan; `Part1-Architecture.md` (supported routes); `WorkflowTestFramework-Plan.md` (cross-reference: legacy routes vs `execute_test`); `.claude/skills/warewolf-architecture` route list |

Consumer side (`Unlimited.ProfilerServices`, `ProfilerService-CI-Dev.yml`), after an engine release
that contains the above. Before the container was removed, the pipeline ran its test steps on **PR
builds only**: `Run an image`, `Unit Tests` (`RunAllWarewolfServerTests.ps1`), two failure emails,
two HTML report tabs and `container stop-remote`. The new pipeline only deploys on non-PR builds.
Decision (section 8, Q5): PR builds deploy to the shared Dev engine and run the gate there.

- **PR builds deploy:** the engine path runs on PR builds too:
  - Token replacement for `.bite`/`.txt`/license.
  - "Download Lightweight Engine Release", "Stage Profiler Resources", "Deploy Lightweight Engine".
  - "Configure Engine App" and the smoke test.

  Their `ne(Build.Reason, 'PullRequest')` conditions are lifted. The queue-side steps (Service Bus,
  shovel, RabbitMQ queues, QueueProcessor, load test) stay non-PR. The deploy gains
  `-TestsSourcePath '$(Build.SourcesDirectory)\Warewolf.ProfilerServices.Tests\Profiler'`.
- **Unit Tests (PR only):** a new step after the smoke test runs an engine-aware copy of
  `RunAllWarewolfServerTests.ps1`. It makes the same three root calls as before (`/secure/.tests`,
  `/secure/.coverage`, `/secure/.coverage.json`; section 8, Q3), with two changes:
  - An Entra bearer token for `dev-sp-devops` instead of Basic auth.
  - `https://devprofilerlw.azurewebsites.net` instead of `http://localhost:7994`.
- **Gates and reporting, unchanged from before (section 8, Q4):**
  - `TEST_CASE_PRECHECK_FLAG: $(profilerunittestsprecheck)`: `0` skips the step and writes
    placeholder HTML.
  - Any failed test fails the build.
  - `CoverageSummary.TotalCoverage` below `TEST_CASE_THRESOLD: $(profilerunittestthreshold)`
    fails the build.
  - On failure, both "Send an email with subject Unit Test" steps go to
    `microserviceteam@theunlimited.co.za` with `UnitTestCoverage.html` / `UnitTestResult.html`
    attached.
  - The `Code Coverage` and `Unit Test Result` HTML report tabs are published on success or
    failure.
  - New: the two HTML files are also published as a pipeline artifact.
- **The engine is real:** "Download Lightweight Engine Release" keeps taking the engine from
  warewolf.online's release notes, so PR and develop builds both run a published engine, as in
  production use. There is no locally built or side-loaded engine.
- **Consequence the user accepted:** after a PR build, `devprofilerlw` runs the PR's workflows
  until the next develop build redeploys it. Two PR builds at once would overwrite each other's
  deploy mid-test. Mitigation: serialize the job with an Azure DevOps exclusive lock (an
  environment with `lockBehavior: sequential` and a deployment job), decided when the pipeline
  change is made.

## 5. Phases

0. **Spike (throwaway, no product code).** Against the profiler's `.bite` and `.test` files, on
   net10 Linux:
   - (a) `Workflow(XElement).WorkflowNodes` loads.
   - (b) Test-step IDs match node IDs.
   - (c) `WarewolfWorkflowReports` gives a plausible total.
   - (d) Time to run all 85 tests through `ExecuteTest` in the in-process harness.
   - (e) Whether the profiler tests mock every external call. The old PR job skipped `.bite` token
     replacement, which suggests they do.
   - (f) Where `HtmlTextWriter` comes from (F7).

   Report results before phase 1. A failure in (a) or (b) sends this plan back for redesign.
   **Done 2026-10-02; results in section 9.** (a) passes only with cleaned XAML (design updated in
   3.3); (b), (c), (d), (e) and (f) pass. The spike also found engine gaps G1-G3, added to phase 2.
1. Legacy catalog + GUID index + `TestsDirectory` setting.
2. Runner with time budget and permission filter, plus the `ExecuteTest` gaps from section 9:
   - G1, the pass rule.
   - G2, recordset inputs.
   - G3, whose cause is not yet known.

   Exit criterion: the profiler's 82 tests pass in the in-process spike harness, as they did in the
   container.
3. Coverage adapters + JSON/HTML builders (including the shared move).
4. Routes + `NameSuffixParser`.
5. Deploy script parameter + Pester + docs.
6. Engine release. A push to the `7580` branch publishes a release automatically. The profiler
   pipeline downloads the engine from warewolf.online, so nothing reaches it before a release. The
   release made for this work is acceptable; it is deprecated once a real release supersedes it.
   Pushing still needs the user's explicit go-ahead at the time, per `CLAUDE.md`.
7. Profiler pipeline change. One parity run: the same commit through the old container
   (`docker build` + `RunAllWarewolfServerTests.ps1` locally) and through the engine. Pass/fail
   per test and total coverage must match, or every difference is explained.

## 6. Risks

- **XAML on Linux (F2/F4):** if `Workflow`'s builder read fails on the Flex host, the node list must
  come from the engine's own XAML load instead. Phase 0 (a).
- **Behavioural drift:** the engine is not Dev2.Server. Tests that relied on Server-only behaviour
  may flip. The phase 7 parity run is the guard.
- **Time limit (F10):** handled by the deadline and scoped calls. A long-running suite would need an
  async start/poll design, which is not in this plan.
- **Concurrency (F9):** tests run sequentially per request. Two pipelines testing the same engine at
  once could still collide on container-child asserts. Documented, not fixed.
- **Shared dev engine:** tests run against the deployed Dev engine and its real sources. Unmocked
  steps touch Dev SQL/RabbitMQ (phase 0 e).

## 7. Unit-test plan (for approval, none written yet)

| Area | Tests |
|---|---|
| `NameSuffixParser` | root `.tests`/`.coverage`/`.coverage.json`; `Folder/.tests`; `Folder/Wf.tests`; case-insensitivity; percent-encoded dots; no interference with `.debug`/`.xml`/`.api`/`apis.json`; a workflow literally named `X.tests` resolving per Server precedence. |
| `LegacyTestCatalog` | loads `$type` JSON fixture; skips non-GUID dirs; bad file logged and skipped; deterministic order; missing directory gives an empty catalog; never writes (file system mock asserts no writes). |
| `WorkflowGuidIndex` | maps GUID to path and name; duplicate GUIDs reported; invalidation on change. |
| `LegacyTestRunner` | permission filter excludes non-Execute workflows; sequential execution; deadline turns remaining tests `TestInvalid` with the budget message; orphan GUID reported invalid. |
| Coverage adapters | **parity:** same workflow + tests fixture through the Server path (`ServiceTestCoverageModelToFactory` + `WarewolfWorkflowReports`) and the engine path give equal `TotalCoverage`, covered/mocked/not-mocked IDs and node counts; nested decision/switch/sequence children; zero tests gives 0 %. |
| Builders | JSON shape keys and types; HTML snapshot parity with the Server builder for one fixture; existing `Dev2.Runtime.WebServer.Tests`/`Dev2.Runtime.Tests` coverage tests still green after the shared move. |
| Routes (in-process harness, `SendThroughPipelineAsync`) | Secure without token gives 401/403; Public returns the wrapped 500; Secure with an Execute grant gives 200 + JSON/HTML content types; workflow-scoped vs folder-scoped results. |
| Deploy Pester | `-TestsSourcePath` staged to `Tests/`; absent leaves no folder; tests untouched by `-EncryptResources`; settings file carries `TestsDirectory`; summary JSON field. |

## 8. Decisions (2026-10-02)

1. **Pipeline identity:** `dev-sp-devops`. It needs an Easy Auth allowed-client entry on
   `devprofilerlw` and an Execute grant in the profiler's `secure.config`.
2. **Engine release:** pushed to `7580`, which releases automatically; this release is used for the
   parity run and deprecated later. The pipeline must use a real release from warewolf.online.
3. **Call shape:** same as before the container was removed: one root call each to `.tests`,
   `.coverage` and `.coverage.json`. If phase 0 shows the root `.tests` call cannot finish in the
   200 s budget, that is reported back before phase 1 rather than silently switching to
   per-workflow calls.
4. **Gates:** unchanged from before the container was removed (precheck flag, any failed test,
   coverage threshold, failure emails, report tabs). See section 4.
5. **Where PR builds test:** PR builds deploy to the shared Dev engine `devprofilerlw` and run the
   gate there.
6. **G1/G2 scope (after phase 0):** fix them in `WorkflowExecutor.ExecuteTest` itself, so
   `execute_test` gets Server-parity verdicts and recordset inputs too. They are not applied only
   inside the legacy routes. See section 10.

## 9. Phase 0 results (2026-10-02)

Throwaway spikes. Nothing in either repo changed except this document. They ran against the
engine's net10 Debug build of 2026-09-25 (`Warewolf.Execution.Lightweight/bin/Debug/net10.0`) and
the profiler repo at `AD-2383-ShovelBridgeDevPipeline`. That repo's workflows and tests are
identical to PR 13841's source commit `e88d1aa`, which build 73014 tested.

- **Platforms:** Windows 11 with en-US culture, and Linux (WSL2 Ubuntu, .NET 10.0.12) with
  invariant culture. Results were identical on both.
- **Baseline:** build 73014, the last PR build of definition 63 (`ProfilerServices-CI-DEV`) before
  the container was removed, 2026-09-08. Its two published HTML reports give:
  - 82 tests, 82 passed, 0 failed, 0 invalid.
  - 350 nodes, 281 covered, 80.29 %.
  - 40 workflows listed.
  - `.tests` took about 55 s.

| Check | Result |
|---|---|
| (a) Workflow node extraction on net10 | **Pass, with a design change.** `Workflow(XElement)` returns 0 nodes on Windows and Linux: its raw XAML read throws `XamlObjectWriterException: Cannot set unknown member '{…/presentation}VirtualizedContainerService.HintSize'`, which it catches and logs. After the engine's own cleaning (`Dev2XamlCleaner` + `Dev2XamlLoader.RemoveWindowsElements`), all 45 workflows load (3.3). |
| (b) Test steps match workflow nodes | **Pass.** All 477 top-level steps (and children) of the 82 tests resolve to activities in their workflow. Three tests (`acb75027…`) belong to no workflow in the repo; the Server never ran them, so the engine skips them (3.1). |
| (c) Coverage parity | **Exact.** Engine-side calculation over the cleaned node lists gives **350 nodes, 281 covered, 80.29 %**, with **0 of 40** per-workflow rows differing from the container's report. Notes: the header total includes the 5 workflows with no tests (32 nodes); per-workflow rows use `WorkflowCoverageReports.WorkflowNodes` (flattened, distinct), not `Workflow.WorkflowNodes`. |
| (d) Run time | **Pass.** All 82 tests in-process: 9.1 s on Windows, 7.4 s on Linux, against about 55 s on the container. The 200 s budget and the single root call (section 8, Q3) are comfortable. |
| (e) External calls | **Pass in practice.** Static upper bound: 32 tests can reach an unmocked SQL (118 sites), web POST (9) or SQL bulk insert (6) activity on some branch, mostly through unmocked sub-workflows. Measured: in the 82 runs against the repo's untokenised sources (as in the old PR container), **no SQL, web or queue activity executed**. To re-check after G1-G3 are fixed, since the 12 G2 tests may then take other branches. |
| (f) `HtmlTextWriter` | **Resolved.** NuGet package `HtmlTextWriter` 2.1.1, MIT, netstandard2.0/2.1 (3.4). |

**Engine gaps.** As it stands, `WorkflowExecutor.ExecuteTest` passes 50 of 82 tests (32 failures).
They break down as follows:

- **G1, pass rule (19 tests).** The two rules differ:
  - Engine, `WorkflowExecutor.AllStepsPassed` (`WorkflowExecutor.cs:666-688`): fails a test on
    `TestFailed` or `TestInvalid` of **any** step, mocked or not, recursing into children.
  - Server, `Evaluator.UpdateFailureMessageArgs` (`Dev2.Runtime/ESB/Execution/Evaluator.cs:618-668`):
    considers **top-level, non-mock** steps only, failing on `TestPending`, `TestInvalid` or
    `TestFailed`. Outputs fail on the same three states.

  Mocked steps the flow never reaches stay `TestInvalid` or `TestPending`, so the engine fails tests
  the Server passes. With the Server's rule applied to the same runs, 69 of 82 pass.
- **G2, recordset inputs (12 tests, exactly the 12 tests that have recordset inputs).**
  `BuildTestInputsPayload` (`WorkflowExecutor.cs:701-726`) writes each input as a flat JSON key, so
  `MandateScoredResults(1).TotalScore` never becomes row 1 of a recordset. The first step that reads
  it fails (`recordset not found`, `Variable { … } is NULL`, `Error with variables in input`). This
  also affects `execute_test`, which shares `ExecuteTest`.
- **G3, one test** (`GetTrackingPeriod :: GetStandardTrackingDays_HasNegativeScore`). `Count Records`
  throws `Value cannot be null (Parameter 'source')` right after a mocked `Delete Record` inside a
  mocked ForEach, and `TrackingDays` then comes out 8 instead of 9. Its inputs are scalars and its
  dates are fixed (2025), so it is neither G2 nor date drift. Cause not yet known.

**Decision needed before phase 2:** G1 and G2 live in `ExecuteTest`, which `execute_test` also uses.
Fixing them engine-wide changes `execute_test`'s verdicts to match the Server. The alternative is
applying the Server rule only inside the legacy routes.

**Spike artefacts:** session scratchpad `phase0/`. Contents:
- `CoverageSpike/`, `ExecSpike/`.
- The baseline reports `b73014-*.html`.
- Run outputs `coverage-spike-*.txt` and `exec-spike-*.txt`.
- Analysis scripts.

A throwaway .NET 10 runtime is installed at `/tmp/phase0-dotnet` in WSL Ubuntu.

## 10. G1/G2 fix design and unit-test plan (for approval)

Both fixes are ports of Server code into `Execution/`, following the `TestMockActivityResolver`
precedent (a verbatim port with the Server file and line cited). Both are pure helpers, so they can
be unit-tested without workflow fixtures. `ExecuteTest` is the only caller, and it changes in two
places.

**G1: `Execution/TestVerdict.cs`** (new, internal static). It ports
`Evaluator.UpdateTestWithStepValues` + `UpdateFailureMessageArgs` + `UpdateFailureMessage`
(`Dev2.Runtime/ESB/Execution/Evaluator.cs:604-700`) and the final mapping in
`UpdateTestWithFinalResult` (`:735-750`).

- **Rule:** only top-level steps (`test.TestSteps`, no recursion) whose `Type != StepType.Mock`
  count. Any such step in `TestPending`, `TestInvalid` or `TestFailed` fails the test. Outputs
  (`test.Outputs`) fail on the same three states. Pass = outputs passed (from `TestOutputEvaluator`)
  and the steps pass under that rule.
- **Result:** `TestInvalid` if any counted step is invalid. Otherwise `TestPassed` or `TestFailed`.
  The message holds the Server's `Failed Step:` / `Invalid Step:` / `Pending Step:` lines after the
  output failure message.
- **`ExecuteTest`:** `AllStepsPassed` (`WorkflowExecutor.cs:666-688`) is deleted and the verdict
  comes from `TestVerdict.Evaluate`. A run that throws keeps today's `TestInvalid` path unchanged.

**G2: `Execution/TestInputLoader.cs`** (new, internal static). It ports
`ServiceTestExecutionContainer.AddTestInputsToJsonOrRecordset`, `AddRecordsetsInputs`,
`AddRecordsetInput` and `AddToRecordsetObjects` (`Dev2.Runtime/ESB/Execution/ServiceTestExecutionContainer.cs:177-215, 308-370`):

- **Recordsets:** inputs are grouped by recordset and then by row index, and assigned into
  `dataObject.Environment`. A row whose values are all empty is skipped.
- **JSON objects:** `[[@…]]` inputs are parsed and added with `AddToJsonObjects`. A parse failure is
  logged, as on the Server.
- **Scalars:** assigned directly. `EmptyIsNull` with an empty value skips the assignment. A value
  that is itself a variable expression adds "Cannot use variables as input value." to `AllErrors`.
- **`ExecuteTest`:** stops sending test inputs as `RawInputPayload` and calls
  `TestInputLoader.Load(test, dataObject)` after `BuildDataObject` instead. `BuildTestInputsPayload`
  (`WorkflowExecutor.cs:701-726`) is deleted.
- **Behavioural difference to note:** inputs are now assigned whether or not the workflow declares
  them as Input variables, as on the Server. The flat payload only bound declared inputs.

**Unit tests to add** (MSTest, in `Warewolf.Execution.Lightweight.Tests/Execution/`). None written yet.

| File | Tests |
|---|---|
| `TestVerdictTests.cs` | all steps and outputs passed gives `TestPassed`; a mocked top-level step left `TestInvalid` or `TestPending` still passes (the G1 case); an asserted top-level step `TestFailed` gives `TestFailed` with a `Failed Step:` line; an asserted step `TestInvalid` gives `TestInvalid` with an `Invalid Step:` line; an asserted step `TestPending` fails with a `Pending Step:` line; a failing **child** under a passing top-level step does not fail the test (Server parity); an output `TestPending` or `TestInvalid` fails; outputs failing (from `TestOutputEvaluator`) fails with that message first; null `TestSteps`/`Outputs` are treated as passed. |
| `TestInputLoaderTests.cs` (against a real `ExecutionEnvironment`) | scalar assigned; `EmptyIsNull` + empty skipped; empty without `EmptyIsNull` assigned as empty; variable-as-value adds the error and is not assigned; `rs(1).a` + `rs(1).b` + `rs(2).a` give a recordset with 2 rows and the right fields (the G2 case, shaped like `MandateScoredResults`); a recordset row with all values empty is not created; `[[@obj]]` JSON is loaded and invalid JSON is logged, not thrown; an input not declared in the data list is still assigned. |
| `WorkflowExecutorExecuteTestTests.cs` (one small in-code Flowchart: assign → decision → mocked activity on the untaken arm) | a recordset input reaches a downstream assign (G2 end to end); a mocked activity on the untaken branch does not fail the test (G1 end to end); an asserted output mismatch still fails. |

**Existing tests:** no current test exercises `ExecuteTest`'s verdict or inputs (the three
`IWorkflowExecutor` fakes are unaffected). After the change, run the whole
`Warewolf.Execution.Lightweight.Tests` project plus the phase 0 spike.

**Exit criterion:** the spike shows at least 81 of 82 profiler tests passing. The 82nd is G3,
investigated next with its own fix and test plan.
