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

### Track C — Trivial-tier blitz (cross-cutting)
- The 380 T1 classes (1–5 lines each) are spread across many assemblies and need little domain knowledge. Run them as a **separate one-week all-hands blitz** or as onboarding/junior tasks — *not* buried inside per-assembly queues where they get ignored in favor of the big stuff.
- Banks ~930 lines and 380 ratcheted floors fast, with minimal risk.

#### Progress
- **2026-05-27 — `RsOp*` recordset-search validators (Dev2.Activities), 34 classes.** New test class `Dev2.Activities.Tests/BussinessLogic/RsOpSearchValidationTests.cs` (34 tests, all passing) covers the whole `AbstractRecsetSearchValidation` family in the `Dev2.BussinessLogic`/`Dev2.DataList` namespaces:
  - 20 single-line `Is/Not` validators (Base64, Binary, Hex, Alphanumeric, Date, Email, Numeric, Text, XML) plus `IsError`/`IsNoError`.
  - `IsNull`/`IsNotNull` — both `all`/`any` branches.
  - 12 comparison ops (`=`, `<>`, `>`, `>=`, `<`, `<=`, Contains, Not-Contains, StartsWith, Not-StartsWith, EndsWith, Not-EndsWith) — both `all`/`any` branches each.
  - Each test asserts `HandlesType()` + `ArgumentCount` and exercises the returned predicate with a matching and non-matching `WarewolfAtom` (behavior, not just line hits).
- **Blocked — `Dev2.Comparer.*` trivial group (7 classes).** Those comparers are `internal` with no `InternalsVisibleTo`, and their remaining lines (`GetHashCode`, null-guards) are unreachable via the activity `Equals` path (`OrderBy` dereferences entries before the comparer runs; `SequenceEqual` never calls `GetHashCode`). Needs an `InternalsVisibleTo` decision before it can be picked up.

### Track D — Monsters & exclude-candidates (special-cased)
- Pull the genuine monsters out of all team slices: `Dev2.Activities.WF.WorkflowToX6Converter` (+1,115 lines), `Dev2.Activities.WF.X6ToWorkflowConverter` (+755), `Dev2.Services.Execution.DatabaseServiceExecution` (+485). Each is a dedicated mini-project or an EXCLUDE decision (see §6) — folding them into a team's normal slice wrecks that team's velocity.

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

### Track A — Dev2.Core (in progress)

Working `coverage-worklist/Dev2.Core.csv` cheapest-first. Tests land in `Warewolf.Core.Tests` (references Warewolf.Core → Dev2.Core transitively). Note: that test project sets `EnableDefaultItems=false`, so each new test file needs an explicit `<Compile Include>` entry in the `.csproj`.

**Batch 1 (2026-05-27) — the four 1-line classes. 11 tests added, all passing.**

| Class | Was | Tests | Status |
|---|---|---|---|
| `DataTableInterrogator` | 50.0% | 2 — CreateMapper → DataTableMapper; CreateNavigator throws | ✅ done |
| `DataTablePath` | 76.4% | 5 — both ctors, table-name branch, both NotImplementedException throws | ✅ done |
| `PocoInterrogator` | 71.4% | 1 — CreateNavigator exception path (non-IPath type) | ✅ done |
| `RetryState` | 50.0% | 1 — both properties get/set | ✅ done |

Each now clears the 80% floor by the worklist line-math. Tests verified passing via `dotnet test`; a full coverage instrumentation run (`TestRun.ps1`) has **not** yet been done to confirm the exact new % or to ratchet the per-file floors.

**Remaining Dev2.Core T1 (2–5 lines each), cheapest-first:** `VariableUtils`, `CaseConvertTO`, `DataBrowser`, `DataSourceShapeComparer`, `CustomContainer` / `CustomContainer<T>`, `DeletedFileMetadata`, `DynamicServices.Validator`, then `PocoPathSegment`, `JsonPathSegment`, `OutputDescription`. Then T2/T3 per the worklist.

---

*Source: Azure DevOps build #29872 code coverage tab. 1,952 total classes, 909 under 80%. Refresh §2/§3 tables from the §7 query after each significant build.*
