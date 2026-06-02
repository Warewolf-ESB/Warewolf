# Warewolf Code Coverage Plan: Road to 80% (class-level)

**Baseline:** Build #20260527.10 (3.0.2.69), branch `8431-enforce per-line coverage threshold`
**Current state:** 61.6% line coverage (75,808 / 123,034 coverable lines), 58.3% branch coverage
**Target:** 80% line coverage = 98,427 covered lines → **+22,619 lines to cover**
**Strategy:** drive individual *classes* over 80%, easiest-first — not whole assemblies to 100%.

---

## 1. Why class-level, not assembly-level

Getting a 22k-line assembly to 100% is a multi-month slog with diminishing returns. But overall coverage is just a weighted average of classes: **if every class is ≥80%, the project is ≥80% by definition.** So the efficient path is to rank all under-80% classes by how few lines each needs to cross the line, and work cheapest-first.

There are **909 classes below 80%**. Bringing each only up to the 80% floor would add 29,385 covered lines — more than the 22,619 we actually need, so we won't even have to finish them all.

### Guiding principles
- **Cheapest-first.** A class at 79% needing 1 line is a free win; bank hundreds of those before touching hard ones.
- **Floor, then overshoot is fine.** Tests written to cover a gap usually land the class at 85–95%, not exactly 80% — so real progress outruns the floor math below, and fewer classes are needed than the raw count suggests.
- **Test behavior, not lines.** Cover real branches and error paths; reject assertion-free tests that only inflate the number.
- **Ratchet per file.** When a class crosses 80%, raise its per-file threshold (the point of this branch) so it can't regress.
- **Decide test-vs-exclude for dead-weight classes** (generated, thin wrappers, interfaces) rather than forcing tests onto them.

---

## 2. The effort distribution (this is the whole plan in one table)

Every class below 80%, bucketed by how many lines it needs to *reach* 80%:

| Tier | Lines needed per class | # classes | Lines gained | Cumulative covered | Cumulative overall % |
|---|---|---|---|---|---|
| Baseline | — | — | — | 75,808 | 61.6% |
| **T1 Trivial** | 1–5 | 380 | +929 | 76,737 | 62.4% |
| **T2 Small** | 6–20 | 195 | +2,153 | 78,890 | 64.1% |
| **T3 Medium** | 21–60 | 201 | +6,868 | 85,758 | 69.7% |
| **T4 Large** | 61+ | 133 | +19,435 | 105,193 | 85.5% |
| **All under-80% classes** | | **909** | **+29,385** | 105,193 | 85.5% |

**Reading this:** Tiers T1–T3 are 776 classes for only ~9,950 lines — high count, low risk, ideal for parallelizing across the team and for junior devs. They alone take us to ~69.7%. The last leg to 80% comes from roughly **the cheaper ~85–90 of the 133 T4 classes** (~12,700 more lines). We do **not** need all of T4.

---

## 3. Wave plan

### Wave 0 — Foundation (Sprint 1)
- Stand up the per-file coverage gate from this branch in **warn-only** mode; snapshot every class's current % as its floor.
- Add a per-build coverage trend report.
- **Worklist is already generated** (from build #29872):
  - [`coverage-worklist.csv`](coverage-worklist.csv) — all 909 under-80% classes, sorted lines-needed ascending. Columns: `LinesNeededTo80, Tier, CoveragePct, Covered, Uncovered, Coverable, TotalLines, Assembly, Class`.
  - [`coverage-worklist/`](coverage-worklist/) — the same data split into 38 per-assembly CSVs (e.g. `coverage-worklist/Dev2.Activities.csv`) so each owning team gets only its slice. Largest: Dev2.Activities (212 classes), Dev2.Runtime.Services (211), Warewolf.Language.Parser (94).
  - Refresh both after any future build with the §7 query.
- Triage the zero-coverage classes (§6): tag each **TEST** or **EXCLUDE**.

### Wave 1 — Trivial sweep, T1 (Sprints 2–3)
- 380 classes, ~1–5 lines each. These are classes already at 76–79.9% (e.g. `DsfAbstractFileActivity` 79.8% needs 1 line; `EnvironmentOutputMappingManager` 79.5% needs 1; `SharepointSource` 79.8% needs 1).
- Batch by namespace so one test class/PR knocks out several. Treat as good first issues.
- **Exit:** ~62.4% overall; ratchet all 380 floors to 80%.

### Wave 2 — Small wins, T2 (Sprints 4–5)
- 195 classes, 6–20 lines each (~2,150 lines).
- Still mostly single-PR-per-class; cover the one or two untested branches/error paths each.
- **Exit:** ~64.1% overall.

### Wave 3 — Medium, T3 (Sprints 6–9)
- 201 classes, 21–60 lines each (~6,870 lines).
- Each is a focused half-day to a day of testing. Distribute across the team.
- **Exit:** ~69.7% overall; no quick wins left untaken.

### Wave 4 — Targeted large-class push, T4 (Sprints 10–14)
- 133 classes need 61+ lines each. We only need ~12,700 of the available 19,435 lines, so **pick the best-leverage subset** and stop once overall ≥80%.
- Prioritize T4 classes that are *also* risk hotspots (high crap score) so the work doubles as bug-risk reduction.
- Highest single-class line gains available (already 60–79%, one focused effort each):

  | +lines to 80% | Coverage | Covered/Coverable | Class |
  |---|---|---|---|
  | +131 | 65.8% | 607/922 | `DsfNativeActivity<T>` |
  | +113 | 60.1% | 339/564 | `DsfDataSplitActivity` |
  | +74 | 66.6% | 367/551 | `DsfEnhancedDotNetDllActivity` |
  | +70 | 62.8% | 254/404 | `DsfExecuteCommandLineActivity` |
  | +70 | 62.2% | 244/392 | `DsfDotNetMultiAssignObjectActivity` |
  | +63 | 64.9% | 269/414 | `SuspendExecutionActivity` |
  | +59 | 68.9% | 369/535 | `DsfSendEmailActivity` |
  | +59 | 67.3% | 313/465 | `AssignEvaluation` |
  | +53 | 64.7% | 222/343 | `DsfReplaceActivity` |

- **Exit:** overall **≥80%**; flip the gate to **blocking**.

### Wave 5 — Hold the line (ongoing)
- Blocking per-file gate: a PR can't push any touched file below its ratcheted floor.
- Tests required for new code (definition-of-done).
- Quarterly: re-pull the worklist, raise floors opportunistically.

---

## 4. Work breakdown & staffing

Dividing purely by assembly is the wrong default — two assemblies hold ~47% of the work (Dev2.Activities 212 classes, Dev2.Runtime.Services 211), so "one team per assembly" overloads two teams and starves the rest. Use four parallel tracks:

### Track A — Ownership teams (the bulk)
- **Unit of work = assembly**, assigned to the team that already owns that code (tests need domain knowledge). Each team takes its `coverage-worklist/<assembly>.csv` slice and works it cheapest-first.
- Right-sized for the ~34 small/medium assemblies (≤94 classes each).

### Track B — The two giants, subdivided by namespace
- Dev2.Activities (212) and Dev2.Runtime.Services (211) are **epics, not one person's backlog**. Split each by namespace and staff with several people in parallel, e.g.:
  - `Dev2.Runtime.ESB.*` vs `Dev2.Runtime.ServiceModel.*` vs `Dev2.Runtime.WebServer.*`
  - `Dev2.Activities.*` core vs `Unlimited.Applications.BusinessDesignStudio.Activities.*`
- Sort each sub-slice by `LinesNeededTo80`; favor classes that are also high crap-score hotspots so the work doubles as defect-risk reduction.

#### Progress
- **2026-05-28 — `Dev2.Runtime.Services` / `ESB.Management.Services.*` cheapest-first sweep, 10 classes.** Branches `8431-coverage-boost` (first 8 commits) and `8432-80PercentCoverage` (last 2). Each as a focused commit, all tests passing locally:
  - `DeleteVersion` — `GetResourceID` happy path, `resourcePath` branch, catch-block (`8431-Add GetResourceID/resourcePath/catch coverage for DeleteVersion`).
  - `LoggingSettingsRead` — new test class (was *untested*); `Execute` happy path + `HandlesType` + `CreateServiceEntry`.
  - `SaveTriggerQueueService` — missing-payload catch-block path.
  - `GetScheduledResources` — empty-collection serialize body + factory-throws catch, **cross-platform** (the existing happy path is gated on `RuntimeInformation.IsOSPlatform(Windows)`).
  - `GetScheduledResourceHistory` — empty-history branch + factory-throws catch, **cross-platform** (existing happy path is Windows-gated).
  - `FetchResourceDuplicates` — `LoadDuplicate` throws → `ExecStatus.Fail` `ExplorerRepositoryResult`.
  - `FindResourcesByType` — missing-type `ArgumentNullException` (caught + rethrown), null-result empty return, `HandlesType`, `CreateServiceEntry`.
  - `FetchRemoteDebugMessages` — new test class (was *untested*); missing-`InvokerID` throw, empty-guid empty-return, metadata.
  - `ChatbotSettingsRead` / `PersistenceSettingsRead` — structural twins of `LoggingSettingsRead`; `Execute` happy path + metadata for both.
- **2026-05-28 — `Dev2.Runtime.Services` / `Hosting.*` + `ServiceModel.Data.*` + `ServiceModel.*` trivial sweep, 7 classes.** Branch `8432-80PercentCoverage`:
  - `VersionStrategy` — `GetNextVersion` `"Rename"` branch (keeps existing `VersionNumber`/`VersionId`) + `GetCurrentVersion(IResource, IVersionInfo, …)` null-`oldresource` branch.
  - `ResourceCatalogFactory` / `TriggersCatalogFactory` — `New()` returns the shared singleton instance (both previously 0%).
  - `RecordsetListWrapper` / `RecordsetList` — property round-trip + `Description` setter (both previously 0%).
  - `NamespaceList` / `NamespaceItem` / `ServiceMethodList` — `ToString` JSON-serialization coverage (all previously 0%).
  - `WebExecuteStringArgs` (`ServiceModel.WebSources.cs`) — DTO property round-trip.
- **Cross-platform takeaway.** The two `GetScheduled*` Windows-gated tests reveal a recurring pattern in `Dev2.Runtime.Tests`: real Windows Task Scheduler wrappers in test setup force `Assert.Inconclusive` on Linux unit jobs, so those `Execute` bodies contribute zero Linux coverage. The pattern used here — mock `IServerSchedulerFactory`/`IScheduledResourceModel` to return empty collections / throw — fills the gap cheaply and applies to any of the other `Scheduler*`/`*ScheduledResource*` services.
- **Note — local Track B verification gap.** The pre-existing `SaveTriggerQueueServiceTests.SaveTriggerQueueService_Execute` happy-path test fails in the local Windows dev environment because it touches the real `TriggersCatalog`/`FileWrapper` against the filesystem; the catch-path addition is isolated and unaffected. Should pass on the CI Windows agent with a proper workspace — worth confirming on the next pipeline run.
- **Not yet refreshed.** Per-file instrumentation re-run not done yet — these classes are *expected* to clear 80% based on the worklist baseline, but the exact ratcheted floors and new overall % won't be known until §7 is re-pulled against a fresh build. Hold floor-ratcheting until then.

### Track C — Trivial-tier blitz (cross-cutting)
- The 380 T1 classes (1–5 lines each) are spread across many assemblies and need little domain knowledge. Run them as a **separate one-week all-hands blitz** or as onboarding/junior tasks — *not* buried inside per-assembly queues where they get ignored in favor of the big stuff.
- Banks ~930 lines and 380 ratcheted floors fast, with minimal risk.

#### Progress

##### Rollup (2026-05-27 → 2026-05-28)

**120 new tests across ~84 worklist classes** in 6 batches, all passing locally via `dotnet test`. Full `TestRun.ps1` instrumentation run still pending to confirm exact new % and ratchet the per-file floors. Behavior-based assertions throughout (each test asserts true/false outcome, branch taken, or exception thrown — no assertion-free line-hitters).

| # | Date | Assembly | Test file | Tests | Worklist classes |
|---|---|---|---|---:|---|
| 1 | 2026-05-27 | Dev2.Activities | `Dev2.Activities.Tests/BussinessLogic/RsOpSearchValidationTests.cs` | 34 | 34 (`RsOp*`) |
| 2 | 2026-05-28 | Warewolf.Data | `Warewolf.Data.Tests/DecisionsTests/DecisionOperationsTests.cs` | 40 | 38 ops + `DecisionUtils` |
| 3 | 2026-05-28 | Warewolf.Data | `Warewolf.Data.Tests/Options/GateAndPublishOptionsTests.cs` | 24 | 4 worklist + 5 nested ride-alongs |
| 4 | 2026-05-28 | Dev2.Common | `Dev2.Common.Tests/Wrappers/WrapperFactoriesTests.cs` | 5 | 3 wrappers + `FilePathWrapper` Linux gap-fill |
| 5 | 2026-05-28 | Dev2.Activities | `Dev2.Activities.Tests/ActivityTests/ForEachValueObjectsTests.cs` | 6 | 2 value objects |
| 6 | 2026-05-28 | Dev2.Runtime.WebServer | `Dev2.Runtime.WebServer.Tests/WebServerTrivialClusterTests.cs` | 11 | 3 + 1 POCO (trimmed against existing `ExtensionsTests.cs`) |
| **Total** | | | | **120** | **~84** |

All tests are platform-independent (no `Assert.Inconclusive` guards), so they contribute coverage on the Linux unit jobs too — important because several pre-existing tests in the same areas (`FilePathWrapperTests`, the Windows-gated parts of `GetScheduledResources*Tests`, etc.) are guarded with `RuntimeInformation.IsOSPlatform(Windows)` and contribute zero on Linux.

**Skipped on purpose** (recorded so the next maintainer doesn't re-evaluate):

- `Warewolf.OS.WorkerMonitor` — abstract; the 1-line gap needs heavy mock infrastructure (`IProcessThreadList` / `IJobConfig` / `ProcessThreadList`).
- `Warewolf.Auditing.WebSocketPool` — `Acquire`/`Release` call into a real `ClientWebSocket.Connect`; flaky without network and not worth it for 1 line.
- `Dev2.Common.Common.ExitHelper` and `Warewolf.PauseHelper` (and `Warewolf.ExitHelper`) — thin pass-through wrappers around `Environment.Exit(0)` / `Console.ReadLine()`; effectively untestable without killing the runner. Per §1 principle 5 and §6 these belong in the Wave 0 **EXCLUDE** triage, not Track C.
- `Dev2.DataList.Contract.DateTimeVerifyPart`, `Binary_Objects.Dev2Column`, `DataList.Contract.OutputTO` (Dev2.Data) — `internal` ctors with no `InternalsVisibleTo` from `Dev2.Data` to `Dev2.Data.Tests`; reflection-based instantiation is messy for 1-line wins. Same blocker family as the `Dev2.Comparer.*` entry below.
- `Dev2.TaskScheduler.Wrappers.*` cluster — wraps `Microsoft.Win32.TaskScheduler`; existing tests guard on Windows with `Assert.Inconclusive`, so adding more there would contribute zero on Linux unit jobs.

##### Per-batch detail

- **2026-05-27 — `RsOp*` recordset-search validators (Dev2.Activities), 34 classes.** New test class `Dev2.Activities.Tests/BussinessLogic/RsOpSearchValidationTests.cs` (34 tests, all passing) covers the whole `AbstractRecsetSearchValidation` family in the `Dev2.BussinessLogic`/`Dev2.DataList` namespaces:
  - 20 single-line `Is/Not` validators (Base64, Binary, Hex, Alphanumeric, Date, Email, Numeric, Text, XML) plus `IsError`/`IsNoError`.
  - `IsNull`/`IsNotNull` — both `all`/`any` branches.
  - 12 comparison ops (`=`, `<>`, `>`, `>=`, `<`, `<=`, Contains, Not-Contains, StartsWith, Not-StartsWith, EndsWith, Not-EndsWith) — both `all`/`any` branches each.
  - Each test asserts `HandlesType()` + `ArgumentCount` and exercises the returned predicate with a matching and non-matching `WarewolfAtom` (behavior, not just line hits).
- **2026-05-28 — `Warewolf.Data.Decisions.Operations.*` family (Warewolf.Data), 38 op classes + `DecisionUtils`.** New test class `Warewolf.Data.Tests/DecisionsTests/DecisionOperationsTests.cs` (40 tests, all passing) covers the newer `IDecisionOperation` family (distinct from the already-tested `Dev2.Data.Decisions.Operations.*` namespace):
  - All `Is*`/`Not*` ops: Error/NotError, Null/NotNull, Numeric, Text, Alphanumeric, Date, Email, Base64, Binary, Hex, Xml, RegEx, Contains, StartsWith, EndsWith (and their negations), plus Equal/NotEqual, the four relational ops, and Between/NotBetween.
  - Each test asserts `HandlesType()` and exercises `Invoke()` with a matching + non-matching input; multi-branch ops (relational, Between, IsXml) cover both the numeric/string and inside/outside branches, and the empty-string short-circuits.
  - `DecisionUtils.IsNumericComparison` — both the all-numeric (parses out-array) and non-numeric branches.
  - These are pure-logic, platform-independent (no `Assert.Inconclusive` guards), so they contribute coverage on the Linux unit jobs too. Per-file coverage instrumentation run not yet done to confirm exact new % / ratchet floors.
- **2026-05-28 — `Warewolf.Data.Options.*` Gate/Publish/File cluster, 4 worklist classes + 5 ride-along nested types.** New test class `Warewolf.Data.Tests/Options/GateAndPublishOptionsTests.cs` (24 tests, all passing) covers:
  - `GateOptions` — default `GateOpts = new Continue()`, `Notify` with/without subscriber (null-conditional event path), assigning `EndWorkflow`.
  - `Continue` / `EndWorkflow` — default `Resume` value + `Continue.Strategy` defaulting to `NoBackoff`.
  - `NoBackoff` — default `RetryAlgorithm` + `MaxRetries = 3`; `Create()` enumerated end-to-end (default, zero, and custom `MaxRetries`) to assert the `true × N` then `false` shape.
  - `RabbitMqPublishOptions` — default `AutoCorrelation = new ExecutionID()`, `Notify` with/without subscriber.
  - `ExecutionID` / `CustomTransactionID` / `Manual` — each subclass's `Correlation` setting (and `Manual.CorrelationID` round-trip).
  - `FileParameter` — property round-trip + `FileBytes` happy path; both throw branches (`ArgumentNullException` on null/empty `FileBase64`, `FormatException` re-thrown from the inner `catch`); `RenderDescription` content; `IsEmptyRow` (`&=` "all empty") and `IsIncompleteRow` (`|=` "any empty") truth tables.
  - `TextParameter` (same file, ride-along) — `IsEmptyRow`/`IsIncompleteRow` semantics.
  - All pure-logic, platform-independent. Per-file coverage instrumentation run not yet done.
- **2026-05-28 — `Dev2.Common.Wrappers.*` factories + cross-platform path (Dev2.Common), 4 classes.** New test class `Dev2.Common.Tests/Wrappers/WrapperFactoriesTests.cs` (5 tests, all passing) — explicit `Compile Include` added to the csproj (`EnableDefaultItems=false`). Covers:
  - `FilePathWrapper.GetDirectoryName` and `IsPathRooted(relative)` — **cross-platform**, since the existing `FilePathWrapperTests` is gated on `RuntimeInformation.IsOSPlatform(Windows)` (`Assert.Inconclusive` on Linux) and so contributes zero coverage on Linux unit jobs; these new tests fill that gap using `Path.Combine` + `Path.GetDirectoryName` as the platform-correct oracle.
  - `FileSystemWatcherFactory.New()` returns a non-null `IFileSystemWatcherWrapper`/`FileSystemWatcherWrapper`.
  - `TimerWrapperFactory.New(callback, state, Timeout.Infinite, Timeout.Infinite)` returns a non-null `ITimer`/`TimerWrapper`; callback never fires (no race). `ITimer` fully qualified because `System.Threading` defines one too in newer BCLs.
  - `TimerWrapper.Dispose()` called twice — exercises the `if (_timer is null) return;` early-return branch that the existing single-Dispose `TimerWrapper_Construct` test misses.
- **2026-05-28 — `DsfForEachItem` + `ForEachInnerActivityTO` value objects (Dev2.Activities), 2 classes.** New test class `Dev2.Activities.Tests/ActivityTests/ForEachValueObjectsTests.cs` (6 tests, all passing) — explicit `Compile Include` added. Covers:
  - `DsfForEachItem` — Name/Value/RowIndex/GroupID property round-trip + the static `EmptyList` get/set (state captured + restored in `try/finally` so test order is irrelevant).
  - `ForEachInnerActivityTO` ctor branches — null `IDev2ActivityIOMapping` (no mappings stored), non-null with populated mappings (stored verbatim), and non-null with empty-string mappings (stored as `null` via the ternary). Plus the four `IList<Tuple<string,string>>` ride-along setters.
  - Uses Moq for `IDev2ActivityIOMapping`; pure logic, platform-independent.
- **2026-05-28 — `Dev2.Runtime.WebServer` trivial cluster (Dev2.Runtime.WebServer), 3 classes + 1 POCO.** New test class `Dev2.Runtime.WebServer.Tests/WebServerTrivialClusterTests.cs` (11 tests, all passing) — explicit `Compile Include` added (`EnableDefaultItems=false`). Trimmed against existing `ExtensionsTests.cs` so the new tests are net-new lines:
  - `StatusResponseWriter` (previously *untested*) — default ctor (NoContent) + explicit-status ctor, both verified by `Write` mutating a mocked `IResponseMessageContext.ResponseMessage`.
  - `Extensions.GetHttpStringContent` — XML and TRX both map to `application/xml`; JSON to `application/json`. (Not in existing tests.)
  - `Extensions.IsAuthenticated(null)` — exercises the `Dev2Logger.Debug("Null User", ...)` branch via a direct null `IPrincipal` (existing tests only mock identities).
  - `Extensions.GetContentEncoding` — null content, missing header, known encoding, *and* invalid encoding (catch + UTF-8 fallback). All four branches.
  - `Extensions.CreateWarewolfErrorResponse(Uri, args)` — verifies status code propagates and JSON URI picks `application/json`. (Existing tests only cover the `HttpActionContext`/`HttpContext` overloads.)
  - `WarewolfErrorResponseArgs` POCO round-trip.
- **Blocked — `Dev2.Comparer.*` trivial group (7 classes).** Those comparers are `internal` with no `InternalsVisibleTo`, and their remaining lines (`GetHashCode`, null-guards) are unreachable via the activity `Equals` path (`OrderBy` dereferences entries before the comparer runs; `SequenceEqual` never calls `GetHashCode`). Needs an `InternalsVisibleTo` decision before it can be picked up.

### Track D — Monsters & exclude-candidates (special-cased)
- Pull the genuine monsters out of all team slices: `Dev2.Activities.WF.WorkflowToX6Converter` (+1,115 lines), `Dev2.Activities.WF.X6ToWorkflowConverter` (+755), `Dev2.Services.Execution.DatabaseServiceExecution` (+485). Each is a dedicated mini-project or an EXCLUDE decision (see §6) — folding them into a team's normal slice wrecks that team's velocity.

  | Class | Baseline | Current | Lines banked | Status |
  |---|---|---|---|---|
  | `WorkflowToX6Converter` | 15.6% | **88.6%** (1537/1735) | +1,266 | ✅ done — ratchet floor |
  | `X6ToWorkflowConverter` | 32.9% | 32.9% | 0 | pending |
  | `DatabaseServiceExecution` | 7.4% | 7.4% | 0 | pending (test-vs-exclude call still owed per §6) |

#### Progress
- **2026-05-28 — `WorkflowToX6Converter` (Dev2.Activities), 15.6% → 88.6%; +1,266 covered lines (target was +1,115).** New test class `Dev2.Activities.Tests/ActivityTests/WorkflowToX6ConverterCoverageTests.cs` (90 tests, all passing) — explicit `Compile Include` added to the csproj. The converter is a ~3,400-line partial class split across ~60 `_XxxActivityHelper` files; almost every helper file was at 0% line coverage before this. Strategy: drive each helper through the public `ConvertToX6Json(ActivityBuilder, xml)` entry point by handing it a workflow whose `Implementation` is the activity under test, then assert on the produced `X6WorkflowLoadModel`. Coverage:
  - **Leaf-activity fan-out (~55 tests)** — one `[TestMethod]` per activity type covered by the big `CreateActivityNode` dispatcher: recordset/data (`DataSplit`, `DataMerge`, `BaseConvert`, `Replace`, `CaseConvert`, `FindIndex`, `FindRecords`, `DeleteRecord` × 2, `SortRecords`, `CountRecordset`, `RecordsetLength`, `Unique`, `AdvancedRecordset`); web (`WebGet`, `WebGetRequestWithTimeout`, `WebPost`, `WebPut`, `WebDelete`); database (`SqlServer`, `PostgreSql`, `MySql`, `SqlBulkInsert`, `Oracle`, `Odbc`); file/path (`FileRead` × 2, `FileWrite` × 2, `FolderRead` × 2, `PathCreate`/`Copy`/`Move`/`Rename`/`Delete`, `Zip`, `UnZip`); scripting/calc/misc (`Javascript`, `Ruby`, `Python`, `CommandLine`, `Comment`, `CreateJson`, `XPath`, `Random`, `NumberFormat`, `Calculate_DotNet`, `AggregateCalculate` × 2, `DateTime` × 2, `DateTimeDifference` × 2, `GatherSystemInformation` × 2, `SendEmail`, `ExchangeEmail`, `WorkflowActivity`); messaging (`PublishRabbitMq` × 2, `ConsumeRabbitMq`, `RedisRemove`); plus both multi-assign variants.
  - **Container activities (7 tests)** — `Gate`, `SuspendExecution`, `ManualResumption`, `RedisCache`, `SelectAndApply`, `ForEach` converted empty (drives `Create` + `Process` + the null-handler guard in each `Process*NestedActivities`). `DsfSequenceActivity` covered both empty and with two nested children — fully exercises `ProcessSequenceNestedActivities` and asserts the `isNested` metadata is set on children.
  - **Control flow (12 tests)** — `If` (both branches / only-then / no-branches), `While`/`DoWhile` (with-body asserts the "Loop" back-edge / no-body), `TryCatch` (try+finally / empty), `Parallel` (with branches / empty), `System.Activities.Statements.Sequence`, `Flowchart` (step chain / empty / decision with True+False arms via a minimal `CodeActivity<bool>` stub).
  - **Entry-point edge cases** — null `Implementation` produces start-node only (no edges); `WorkflowXml` is preserved through serialisation.
  - One isolated `[TestMethod]` per activity type rather than one big batch test — so a single activity's `ToX6Json` regressing localises to one failure instead of voiding the whole batch's coverage.
- **Two operational notes worth recording before the next monster:**
  - **MSTest 3.8 runner mode races on the converter's static reflection caches.** Running with the csproj's default `EnableMSTestRunner=true` produced 2 NRE failures inside `DsfSqlBulkInsertActivity.ToX6Json` and `DsfWorkflowActivity.ToX6Json` — almost certainly from parallel test threads racing on `WorkflowToX6Converter._typePropertyCache` / `_childActivityPropertiesCache`. The same MSTest-runner path also produced an empty cobertura (0 hits across all 1,735 lines despite tests executing) — the `XPlat Code Coverage` collector doesn't attach in that mode. Re-running with `-p:EnableMSTestRunner=false` (vstest, single-threaded by default) gave 90/90 passing and correct coverage capture. **Coverage runs for this test project should pass `-p:EnableMSTestRunner=false` until the runner/collector interaction is fixed.**
  - **Orphan `testhost` / `vstest.console` processes lock `obj/Debug/net8.0/Dev2.Activities.dll`** between back-to-back runs and break subsequent builds with `CS2012: cannot open for writing`. Worth a kill step (`Stop-Process -Name testhost,vstest.console -Force`) before each coverage iteration.
- **Remaining gap (198 lines to 100%, not blocking the 80% gate):**
  - `WorkflowToX6Converter.cs` main file at 79.6% (587/737) — most of the gap is the legacy commented-out `#region unused code` switch helpers (reported by cobertura as a method but never executed), the unreachable `else if (activity is DsfDecision decision)` dispatcher arm (`DsfDecision` only enters via `CreateDecisionNode(FlowDecision)` today), and the `ConvertToX6Json` exception path.
  - Container helpers at 74–91% — the populated `Process*NestedActivities` branch isn't covered. Reaching it needs `ActivityFunc<string,bool>` handlers populated with `Activity<bool>` instances; the early-return guard *is* covered.

**Coordination:** the regenerated worklist (§7) is the shared system of record — a class disappears when it crosses 80%, so cross-team progress is visible without manual bookkeeping. Teams use the per-assembly CSVs only for in-flight "who's on what."

---

## 5. Milestones

| Milestone | Overall line coverage | Driver |
|---|---|---|
| M1 (Wave 0) | 61.6% (floors locked) | Gate warn-mode + worklist |
| M2 (Wave 1) | ~62.4% | 380 trivial classes |
| M3 (Wave 2) | ~64.1% | 195 small classes |
| M4 (Wave 3) | ~69.7% | 201 medium classes |
| M5 (Wave 4) | **≥80%** | Targeted ~85–90 large classes |
| M6 (Wave 5) | ≥80% sustained | Blocking gate + DoD |

---

## 6. Zero / near-zero classes — decide test or exclude (Wave 0)

These live mostly in the untested driver/wrapper assemblies. Each needs an explicit call so it doesn't silently fail the per-file gate:

| Assembly | Coverage | Likely decision | Note |
|---|---|---|---|
| Warewolf.AI.Harness | 0% | _TBD_ | If it's itself a test harness → EXCLUDE |
| Dev2.SignalR.Wrappers.New | 0% | _TBD_ | Thin wrapper → likely EXCLUDE |
| Warewolf.Driver.Persistence | 0% | _TBD_ | TEST needs integration strategy |
| Dev2.Data.Interfaces | 0% | _TBD_ | Interface-only → likely EXCLUDE |
| Warewolf.Driver.RabbitMQ | 2.1% | _TBD_ | Mock vs. real broker |
| Warewolf.Exchange.Email.Wrapper | 3.0% | _TBD_ | Small, testable |
| Warewolf.Trigger.Queue | 9.6% | _TBD_ | |
| Warewolf.Sharepoint | 9.7% | _TBD_ | |

Classes marked TEST here are large gaps and belong in Wave 4 sizing; classes marked EXCLUDE are removed from the denominator, which *raises* overall % for free.

---

## 7. Reproducing / refreshing the worklist

The class rankings above come from the ReportGenerator table on the build's Code Coverage tab. To regenerate after a new build, open the coverage tab and run against the report iframe:

```js
// In the build's Code Coverage tab, against the report iframe document:
const t = document.querySelector('iframe').contentDocument.querySelectorAll('table')[4];
let asm=null; const rows=[];
for (const r of [...t.tBodies[0].rows]) {
  const c=[...r.cells].map(x=>x.innerText.trim());
  if(!c[0]) continue;
  if(!r.cells[0].querySelector('a')){asm=c[0];continue;}      // assembly header row
  const covered=+c[1], coverable=+c[3], pct=parseFloat(c[5]);
  if(isNaN(pct)||!coverable) continue;
  const need=Math.max(0, Math.ceil(0.8*coverable)-covered);    // lines to reach 80%
  if(pct<80) rows.push([need,pct,covered,coverable,asm,c[0]]);
}
rows.sort((a,b)=>a[0]-b[0]);                                    // cheapest first
console.table(rows);
```

Export `rows` to `coverage-worklist.csv` and assign top-down.

---

## 8. Risks & watch-items

- **Branch-coverage data looks incomplete** in the baseline (only 12 total branches reported). Verify branch instrumentation before any branch-based gating.
- **Floor math is a lower bound.** Bringing classes to exactly 80% is conservative; real tests overshoot, so expect to need fewer classes than listed — re-check overall % after each wave and stop Wave 4 early once ≥80%.
- **Trivial-tier temptation.** 380 one-line wins are satisfying but only worth 929 lines total; don't let the team over-invest there and neglect T3/T4 where the real lines are.
- **Brittle/assertion-free tests.** Review test quality, not just the number.
- **Hotspot alignment.** Where possible, pull T4 classes that are also high crap-score (e.g. `ResourceLoadProvider.CheckType` crap 506) so coverage work also cuts defect risk.

---

## 9. Progress log

### Track A — Dev2.Core (T1 + T2 complete; T3 cherry-picked, 1 blocker)

Working `coverage-worklist/Dev2.Core.csv` cheapest-first. Tests land in `Warewolf.Core.Tests` (references Warewolf.Core → Dev2.Core transitively). Note: that test project sets `EnableDefaultItems=false`, so each new test file needs an explicit `<Compile Include>` entry in the `.csproj`. Test namespace must be flat `Dev2.Tests` — nested namespaces like `Dev2.Tests.Common` shadow real `Dev2.*` namespaces and break sibling test files.

#### Rollup (2026-05-28)

**119 new tests across 22 classes** in `Warewolf.Core.Tests`, all passing locally via `dotnet test --no-build`. Full `TestRun.ps1` instrumentation run still pending to confirm exact new % and ratchet the per-file floors. One blocker (`ConflictTreeNode`) — see Batch 6 below.

| Tier | Classes | Tests | Batches |
|---|---|---|---|
| **T1** (1–5 lines each) | 13 | 64 | 1–3 |
| **T2** (6–20 lines each) | 7 | 45 | 4–5 |
| **T3** (21–60 lines each) | 2 | 10 | 6 |
| **Total** | **22** | **119** | |

Classes ratcheted (all expected ≥80% after coverage run; behavior-based assertions, not assertion-free fluff):

- T1: `DataTableInterrogator`, `DataTablePath`, `PocoInterrogator`, `RetryState`, `VariableUtils`, `CaseConvertTO`, `DataSourceShapeComparer`, `DeletedFileMetadata`, `DynamicServices.Validator`, `OutputDescription`, `DataBrowser`, `PocoPathSegment`, `JsonPathSegment`.
- T2: `Dev2ActivityComparer`, `Dev2UniqueActivityComparer`, `ServiceActionInput`, `PooledServiceActivity`, `GatherSystemInformationTO`, `BaseConvertTO`, `DataSourceShape`.
- T3: `Dev2XamlLoader`, `ServiceAction`.

Blocked: `Common.ConflictTreeNode` (see Batch 6).

#### Per-batch detail


**Batch 1 (2026-05-27) — the four 1-line classes. 11 tests added, all passing.**

| Class | Was | Tests | Status |
|---|---|---|---|
| `DataTableInterrogator` | 50.0% | 2 — CreateMapper → DataTableMapper; CreateNavigator throws | ✅ done |
| `DataTablePath` | 76.4% | 5 — both ctors, table-name branch, both NotImplementedException throws | ✅ done |
| `PocoInterrogator` | 71.4% | 1 — CreateNavigator exception path (non-IPath type) | ✅ done |
| `RetryState` | 50.0% | 1 — both properties get/set | ✅ done |

Each now clears the 80% floor by the worklist line-math. Tests verified passing via `dotnet test`; a full coverage instrumentation run (`TestRun.ps1`) has **not** yet been done to confirm the exact new % or to ratchet the per-file floors.

**Batch 2 (2026-05-28) — six T1 classes (2–5 lines each). 45 tests added, all passing.**

| Class | Was | Tests | New file | Status |
|---|---|---|---|---|
| `VariableUtils` | 77.9% | 16 — AddError null-guards (3), delegating one-liners, all four `TryParseVariables` overloads, `ParseVariables` inputs-match / no-match / default-text branches | `Common\VariableUtilsTests.cs` | ✅ done |
| `CaseConvertTO` | 77.7% | 14 — both ctors + default-`UPPER` branch, `StringToConvert`→`Result` sync, null-`ConvertType` guard, blank-`Result` fallback, CanAdd/CanRemove, ClearRow, all 3 `GetRuleSet` branches, typed+object `Equals`, `GetHashCode` | `ConverterTests\Base\CaseConvertTOTests.cs` | ✅ done |
| `DataSourceShapeComparer` | 50.0% | 4 — `Equals` both-null / one-null / delegate, `GetHashCode` | `Comparers\DataSourceShapeComparerTests.cs` | ✅ done |
| `DeletedFileMetadata` | 0% | 2 — all props round-trip + defaults | `DeletedFileMetadataTests.cs` | ✅ done |
| `DynamicServices.Validator` | 0% | 2 — ctor sets ObjectType, `ValidatorType` round-trip | `DynamicServices\ValidatorTests.cs` | ✅ done |
| `OutputDescription` | 62.9% | 8 — ctor, typed `Equals` same/diff format, object `Equals` null/ref/type/equal, `GetHashCode` (also exercises `DataSourceShapeComparer`) | `ConverterTests\GraphTests\OutputTests\OutputDescriptionTests.cs` | ✅ done |

Note: `CaseConvertTO.GetHashCode` and `ValidateName` returning null for valid names were discovered during testing — assertions adjusted to match real (reference-hashed `Errors`/`Error`) behaviour rather than forcing it. Verified via `dotnet test` (45/45). Full `TestRun.ps1` instrumentation run still pending to confirm exact % and ratchet floors. `namespace Dev2.Tests` (flat) is required — nested `Dev2.Tests.Common` / `.Comparers` etc. shadow real `Dev2.*` namespaces and break sibling test files.

**Batch 3 (2026-05-28) — `DataBrowser` + `PocoPathSegment` + `JsonPathSegment` (T1 tail). 8 tests added, all passing.**

| Class | Was | Tests | Approach | Status |
|---|---|---|---|---|
| `DataBrowser` | 75.0% | 4 — `SelectScalar` / `SelectEnumerable` / `SelectEnumerablesAsRelated` null-navigator error branches (string data + `DataTablePath`); `SelectEnumerablesAsRelated` empty-paths short-circuit | extend `DataBrowserTests` | ✅ done |
| `PocoPathSegment` | 63.3% | 2 — `As<PocoPathSegment>()` self-return; `As<JsonPathSegment>()` throws `NotImplementedException` | extend `PocoPathSegmentTests` | ✅ done |
| `JsonPathSegment` | 63.3% | 2 — mirror of above (`As<JsonPathSegment>` / `As<PocoPathSegment>` throws) | extend `JsonPathSegmentTests` | ✅ done |

Null-navigator is only reachable via `StringInterrogator` + a `pathType` it doesn't dispatch on (`PocoInterrogator` accepts any IPath; `DataTableInterrogator.CreateNavigator` throws `NotImplementedException` before the null check) — `DataTablePath` against a plain string is the simplest trigger. Path-segment `As<T>` is `where T : class, IPathSegment`, so both the cast-success and the `NotImplementedException` branch are reachable from sibling segment types. `Warewolf.Core.Tests` has `InternalsVisibleTo` via `AssemblyCommonInfo.cs`, so the internal `PocoPathSegment` / `JsonPathSegment` types are usable directly from tests.

Cumulative for Track A Dev2.Core: **batches 1–3 = 64 new tests (11 + 45 + 8) across 13 classes**, all passing. Coverage instrumentation run still pending to confirm exact new % and ratchet the per-file floors.

**Batch 4 (2026-05-28) — T2 entry: 5 small classes. 18 tests added, all passing.**

| Class | Was | Tests | Where | Status |
|---|---|---|---|---|
| `Dev2ActivityComparer` | 0% | 4 — `Equals` both-null / one-null / delegate-to-instance; `GetHashCode` constant `1` | new `Dev2ActivityComparerTests.cs` | ✅ done |
| `Dev2UniqueActivityComparer` | 0% | 5 — `Equals` both-null / one-null / matching-UniqueID / differing-UniqueID; `GetHashCode` delegates | new `Dev2UniqueActivityComparerTests.cs` | ✅ done |
| `ServiceActionInput` | 0% | 2 — ctor sets `ObjectType` + allocates `Validators`; all properties round-trip | new `DynamicServices\ServiceActionInputTests.cs` | ✅ done |
| `PooledServiceActivity` | 0% | 1 — internal ctor exposes `Generation` + `Value` (reachable via InternalsVisibleTo) | new `DynamicServices\PooledServiceActivityTests.cs` | ✅ done |
| `GatherSystemInformationTO` | 67.2% | 6 — 4-arg ctor with `Inserted`, `ClearRow`, `IsResultFocused`, all 3 `GetRuleSet` branches (merged into existing root-level `GatherSystemInformationTOTests.cs`) | extend existing | ✅ done |

Comparers use `Mock<IDev2Activity>` (existing pattern in `ConflictTreeNodeTests`). `PooledServiceActivity`'s `internal` ctor takes `System.Activities.Activity`; the test project already references `System.Activities`, but passing `null` for `Value` exercises every line without needing a concrete `Activity` subclass.

Collision gotcha: there was already a root-level `Warewolf.Core.Tests\GatherSystemInformationTOTests.cs`. My initial new file under `ConverterTests\Base\` declared the same `Dev2.Tests.GatherSystemInformationTOTests` class → CS0101. Resolved by appending the new tests to the existing root-level file (which is already in csproj).

Cumulative for Track A Dev2.Core: **batches 1–4 = 82 new tests across 18 classes**, all passing (87 in the full Batch 1–4 filter run, which includes a handful of pre-existing tests sharing the test categories).

**Batch 5 (2026-05-28) — T2 finish: `BaseConvertTO` + `DataSourceShape`. 27 tests added, all passing first try.**

| Class | Was | Tests | New file | Status |
|---|---|---|---|---|
| `BaseConvertTO` | 65.2% | 14 — full ctor + default-fallback branch (`Base 64`/`Text`), `FromType`/`ToType` null-guards, CanAdd/CanRemove, ClearRow, `IsFromExpressionFocused`, all 3 `GetRuleSet` branches, typed+object `Equals`, `GetHashCode` stability | `ConverterTests\Base\BaseConvertTOTests.cs` | ✅ done |
| `DataSourceShape` | 50.0% | 13 — ctor, typed `Equals` null/ref/empty/matching/differing-ActualPath/different-IPath-impl branches (exercises private `EqualsMethod` `equalTypes` check via `PocoPath` vs `JsonPath`), object `Equals` null/ref/type/equal, `GetHashCode` non-null + null `Paths` branches | `ConverterTests\GraphTests\OutputTests\DataSourceShapeTests.cs` | ✅ done |

`DataSourceShape.EqualsMethod` (private) is reachable indirectly through `CommonEqualityOps.CollectionEquals` from `Equals(IDataSourceShape)`; comparing two single-element shapes with sibling `IPath` types (`PocoPath` vs `JsonPath`) is the cleanest way to hit the `equalTypes` false branch.

Cumulative for Track A Dev2.Core: **batches 1–5 = 109 new tests across 20 classes**, all passing. T2 for Dev2.Core is now complete.

**Batch 6 (2026-05-28) — T3 entry: `Dev2XamlLoader` + `ServiceAction`. 10 tests added, all passing first try.**

| Class | Was | Tests | New file | Status |
|---|---|---|---|---|
| `Dev2XamlLoader` | 40.5% | 3 — `Load` null/empty xamlDefinition `ArgumentNullException` branches, `RemoveWindowsElements` exercising all 3 loops (mva element, sap element, sap attribute removal) | `DynamicServices\Dev2XamlLoaderTests.cs` | ✅ done |
| `ServiceAction` | 37.3% | 7 — ctor (ObjectType, ActionType, collections), simple property round-trips (12 props), `SetActivity`, `PopActivity` empty-pool branch, `Compile` no-input + propagate-input-errors branches, `Dispose` no-stream idempotent | `DynamicServices\ServiceActionTests.cs` | ✅ done |

**Blocked — `Common.ConflictTreeNode` (0%, T3, +44 lines).** The existing `Warewolf.Core.Tests\Common\ConflictTreeNodeTests.cs` is wrapped in `#if WINDOWS || NETFRAMEWORK` (uses `System.Windows.Point`); the test project targets `net8.0`, so the file compiles to *nothing* on the coverage TFM — which is why coverage is 0% despite the file's existence. The class itself has `Activity` as `{ get; }` (read-only) set only by the WPF-only `Point` constructor; on net8.0 there is no public way to create a `ConflictTreeNode` with a non-null `Activity`, and every non-trivial method (`Equals`, `ChildrenEquals`, `GetHashCode`) NREs without one. Needs either (a) a non-WPF constructor / `init` setter on the source, or (b) tagging the class as an `EXCLUDE` candidate per §6, before more test work here is justified.

Cumulative for Track A Dev2.Core: **batches 1–6 = 119 new tests across 22 classes**, all passing. T1 + T2 done; T3 cherry-picked the achievable wins.

**Next up for Track A Dev2.Core.** With T1–T2 done and the cheap half of T3 banked, the remaining `coverage-worklist/Dev2.Core.csv` entries are either the `ConflictTreeNode` blocker or T4-scale work. Per the cheapest-first principle, rotate Track A to a fresh small assembly (`coverage-worklist/*.csv`, smallest first) before tackling Dev2.Core's T4.

**Pending verification (all batches).** None of the per-file floors have been ratcheted yet — that step requires a full `TestRun.ps1` instrumentation run so the actual post-batch class % is known. Until then, the "Was" % columns above are the pre-batch baseline; the post-batch % is expected to land at 85–95% per class (overshoot is fine per §3 principles).

---

*Source: Azure DevOps build #29872 code coverage tab. 1,952 total classes, 909 under 80%. Refresh §2/§3 tables from the §7 query after each significant build.*
