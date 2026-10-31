# Warewolf Mobile Studio — Plan

Status: **P2 app skeleton built (2026-10-31): Android builds, iOS not yet compiled; System 1 deprecated (2026-10-29); P0 engine spikes not started**. Branch `MobileStudio`, off
`7580-SelfHostedEngine` (the npm engine installer). The System 1 baseline (generator loop, LLM chooser,
corpus and evaluation harness) was built ahead of the spikes; see [System 1 baseline](#system-1-baseline).

> **Deprecated: System 1.** The [whole-workflow comparison](#whole-workflow-comparison-2026-10-28) showed that no
> System 1 workflow was rated usable (judge ≥4 on 0 of 50), while large models write whole workflows directly.
> Generation now goes to a large model: a self-hosted one (LM Studio at ask.leylew.is) or the cloud. §2, §3 and the
> System 1 sections below are kept as the record. The code (`shared/generator` choosers, `train`, the classifier and
> Markov contenders in `eval`) stays for comparison runs and gets no further work.

## Goal

A native Kotlin Multiplatform (Android + iOS) port of the Angular Web Studio (`~/angular`) designed to
run **offline on a phone**:

- a local Warewolf execution engine (the self-hosted Lightweight engine), and
- a small local LLM, from a selection of models so a wide range of phones is supported,
- driving a **new workflow-generation method** where a "System 1" decision model chooses the
  structure and the small LLM only fills in each tool's Warewolf-language inputs and outputs.

## Decisions taken

| Question | Decision |
|---|---|
| Where the app lives | `Mobile/Warewolf.Studio/` in this repo |
| Platforms | Kotlin Multiplatform: Android + iOS |
| System 1 | Our own model, trained on the `.bite` corpus, running on device |
| Engine reachability | Private Unix domain socket only; no other app can call it |

## What it builds on

Read directly in code (high confidence):

- `Dev/Warewolf.Execution.Lightweight/SelfHost/SelfHostedEngine.cs:66-93` runs every HTTP function on
  Kestrel through the same middleware, without the Azure Functions host.
- `engine/npm/Build-NpmPackages.ps1` publishes the engine self-contained per runtime (incl. `linux-arm64`).
- `Functions/McpApiFunctions.cs:112+` exposes `mcp-api/*` HTTP routes, and the self-host maps them.
- `Mcp/ToolHandlers/AddStepTool.cs` appends one node at a branch tail (same `findChainTail` rule as the
  Angular chatbot, `graph.component.ts:1952`) and wires Decision/Switch arms.
- `Mcp/ToolSchemaCatalog.cs` holds each tool's input/output field schema.
- The Angular chatbot (`chatbot-context-builder.service.ts:22-56`) asks a large cloud LLM to produce the
  workflow from one big server-built system prompt — not viable on a phone, hence the split below.

## Architecture

```
Mobile/Warewolf.Studio/
├─ shared/
│  ├─ generator      frontier loop + END handling (pure Kotlin, no platform code)
│  ├─ decision       System 1 on ONNX Runtime (both platforms)
│  ├─ llm            common interface; llama.cpp via JNI (Android), cinterop + Metal (iOS)
│  ├─ models         model catalog, device tiering (platform-specific RAM/SoC probing), download + SHA-256
│  ├─ engine-client  mcp-api/* over a Unix socket (Ktor)
│  └─ studio-ui      Compose Multiplatform: UI list, fullscreen UI, flow canvas, property sheets, run/debug
│                    (P2 built this as shared/app; see P2 build)
├─ androidApp/       Android host: engine as a child process under a foreground service
└─ iosApp/           iOS host: engine embedded in the app process
```

The engine stays authoritative for graph changes: the generator only calls `add_step`,
`get_tool_schema`, `set_var` and `validate_workflow`; it never writes `.bite` files itself.

### 1. Running the engine on the device

- **Android:** publish the self-host for `linux-bionic-arm64`, ship it in `jniLibs` as a native library
  (so Android allows executing it) and start it as a child process. Risk: .NET on bionic is
  community-supported; reflection and Newtonsoft make NativeAOT unlikely, so this uses CoreCLR.
  Fallback: a `net10.0-android` (Mono) host library with a thin bridge to Kotlin.
- **iOS:** no child processes and no JIT, so the engine is built as an embedded library and started
  in-process — Mono AOT with interpreter fallback, or NativeAOT. Reflection, Newtonsoft and dynamic
  activity loading make this the **highest-risk item**. If the spike fails, iOS ships as a remote
  studio (engine elsewhere) with generation still on the device.
- **Both:** Kestrel listens on a Unix domain socket in app-private storage instead of TCP. The npm
  README warns *"anything that can reach port 3142 can run every workflow"*; on a phone every installed
  app can reach `127.0.0.1`, so loopback TCP is not acceptable.
- Other engine changes: invariant globalization (no `libicu`), `WAREWOLF_DATA_DIR` set to app-private
  storage, engine lifetime tied to the app.
- Moving Android to in-process hosting too, to share more, is deferred until the spikes report.

### 2. Generation: System 1 decides, the small LLM fills in

*Deprecated 2026-10-29; see the status note at the top.*

State: the user's goal, the current graph, a **frontier** of open branch tails, the variables in scope.

Each step takes one frontier entry, and System 1 returns one of:

- `PLACE(tool)` — a tool from the catalog. A Decision or Switch adds one frontier entry per arm.
- `END(outputs)` — the branch is closed and those variables are marked as outputs. No more tools are
  placed on it.

Then:

1. The small LLM fills **only that tool's fields**, from `ToolSchemaCatalog`, with grammar-constrained
   decoding. The grammar is built from the tool's schema and the variables in scope, so even a tiny
   model can only emit valid Warewolf syntax (`[[var]]`, `[[rs(*).field]]`) or declare new outputs.
2. `add_step` places the node; `validate_workflow` checks it; failures get a capped number of
   repair retries.
3. Stop when the frontier is empty; a step and depth budget prevents runaway generation.

System 1 options:

- **(a) Trained classifier (the target):** a small sentence encoder plus a head over the tool catalog
  and END, trained on (goal, steps so far) → next action pairs extracted from the `.bite` corpus.
  Exported to ONNX.
- **(b) Baseline:** the same small LLM restricted to one token choice from the enumerated tool names.
  Built first; (a) has to beat it.

### 3. Model selection

Tier picked at runtime from RAM, SoC and GPU (Vulkan/Metal) support. Models are GGUF Q4, downloaded on
demand (never bundled in the app), SHA-256 verified. Sizes are candidates to benchmark in P0, not
final choices:

| Tier | Device | Candidate size |
|---|---|---|
| S | ≤ 4 GB RAM | ~0.5B params (~400 MB) |
| M | 6–8 GB | ~1–1.7B (~1 GB) |
| L | 12 GB+ | ~3–4B (~2.5 GB) |

A LoRA fine-tune on field-filling examples from the corpus is expected to matter more than the choice
of base model.

### 4. Porting the studio

The Angular canvas is AntV X6; on mobile it becomes a custom Compose canvas: top-down flow, pan/zoom,
tap-to-edit property sheets.

1. UI list, fullscreen UI, Edit in the Web Studio in the phone's browser (see [App layout](#app-layout-2026-10-31)).
2. Generate with the new method, then edit fields.
3. Hand-editing with drag and drop.

### App layout (2026-10-31)

The app opens on the organization's UIs. **There is no workflow explorer**: a workflow is reached only
through a UI that uses it. In the first version workflows are edited in the Web Studio, opened in the
phone's normal browser; a native designer (the Compose canvas above) can replace it later.

```
Launch ──(auto-start UI set and still viewable)──▶ Fullscreen UI ──back──▶ UI list
   │
   └──────────────────────────────────────────────▶ UI list (home)
                                                       │ per UI:
                                                       ├─ Open ──────▶ Fullscreen UI
                                                       ├─ Auto-start ─▶ toggle (at most one UI)
                                                       └─ Edit ──────▶ pick workflow ──▶ Web Studio editor (phone's browser)
```

| Screen | Behaviour |
|---|---|
| **UI list** (home) | `GET /api/studio/organization-uis`: only the UIs the caller may View. Each row shows name, description and last updated, with Open, Auto-start and Edit actions. Empty list: say there are none and who publishes them (an administrator, through the MCP server). |
| **Auto-start** | At most one UI per signed-in organization, stored on the device (not on the server). Setting it on another UI moves it. On launch the app opens that UI fullscreen. If it is no longer in the list (deleted, or View revoked), the app shows the list with a notice and clears the setting. |
| **Fullscreen UI** | Runs the UI the way the Web Studio's `/organization-ui/:id` page does (`organization-ui-page.component.ts`, the sandboxed frame from `organization-ui-frame.ts`), with no app chrome. Back (gesture or button) returns to the list, also when the UI was auto-started. |
| **Edit** | Lists the UI's `workflows` (`key`, `server`, `workflow` path; `organization-uis.service.ts:6-13`); a UI with one workflow skips the list. The chosen one opens in the Web Studio's workflow editor, opened in the phone's default browser. |

What it uses (read in `~/angular/src/app`, high confidence):

- `organization/organization-uis/organization-uis.service.ts:37-58`: the list/get API and its shapes
  (`OrganizationUiSummary.workflows`, `canEdit`).
- `app.routes.ts:97-100`: `organization-ui/:id` already runs a UI by id.

**Web Studio deep link (done 2026-10-31, `~/angular` develop).** Edit opens
`<studio>/workflow?engine=<engine address>&path=<workflow path>`, built from the UI workflow's `server` and
`workflow`. The path may use `/` or `\`, a leading slash and `.bite`. The designer opens that workflow in a tab
after the saved tabs (or focuses the tab it is already in), then removes the parameters from the URL. It is not
`?server=`: the studio takes that as its startup engine (`Common/StartupServer.ts`) and connects to it without
asking. An engine on the user's server list opens directly; the list is given 10 s to arrive, since the
organization's shared list joins after sign-in. Any other engine opens only after the user confirms it, and is
then known for that session only, so the workflow can be saved. The engine's own View/Contribute decide the
rest. Code: `parseWorkflowDeepLink` (`Services/lightweight-mcp-api.ts`),
`CommunicationService.waitForLightweightServer` / `linkLightweightServer`,
`WorkflowDesignerComponent.openWorkflowDeepLink`; tests in `lightweight-mcp-api.spec.ts`,
`communication.service.lightweight.spec.ts` and `workflow-designer.component.deep-link.spec.ts`.

**Decided (2026-10-31):**

1. **Web Studio in the phone's browser.** Edit hands the deep link above to the default browser app (Android
   `ACTION_VIEW` intent, iOS `UIApplication.open`), so the browser's existing studio sign-in cookie
   (Cloudflare Access) is used and no second sign-in is needed. Not an in-app WebView, which has its own cookie
   store, and not `SFSafariViewController`, which on iOS 11+ does not share Safari's cookies either. The user
   returns to the app with back / the app switcher. A native designer may replace this later.
2. **One workflow skips the picker.**
3. **Permissions are per server.** Edit is shown on every UI. Whether the caller may open or save a workflow is
   decided by the permissions on that workflow's own engine, not by the UI's `canEdit` (which is Contribute on
   the *UI*). A refusal is shown in the editor as the engine returns it.
4. **UIs run against the cloud engines.** Each workflow a UI uses runs on the `server` it names, a cloud-hosted
   engine reachable from the phone; the on-device engine (§1) is not involved in running UIs or editing.
5. **No workflow explorer.** This layout is the home screen of P2.

### P2 build (2026-10-31)

Built as `shared/app` (Kotlin Multiplatform: Android, iOS, desktop JVM) and `androidApp`; see [README](../README.md).

- **Native shell, UIs in a WebView (decided).** The list, auto-start and Edit picker are Compose
  screens. A UI runs on the studio's own `/organization-ui/:id?standalone=true` page in a WebView, so
  the studio's sandbox (`organization-ui-frame.ts`) and its engine calls run the UI unchanged. The
  studio shows that page without its side menu since `~/angular` 38fa4bb6 (`isStandalonePage`).
- **Sign-in.** `/api/studio/*` is behind Cloudflare Access (`studio-session.service.ts`), and the app
  has no Access session of its own. It signs in through `/api/studio/login` in a WebView and sends the
  `CF_Authorization` cookie from the WebView's store (Android `CookieManager`, iOS `WKWebsiteDataStore`)
  with its own calls. A redirect, 401/403 or an HTML reply means signed out. Sign-in and UI pages share
  that store, so the UI pages are signed in to the studio too.
- **Engine calls from a UI** use the studio's MSAL inside the WebView (redirect sign-in). Not yet tried
  on a device; some Entra tenants block WebView sign-in, and the Azure guest check's pop-up isn't opened.
- **Auto-start** is stored per organization (`autoStart.<orgId>`, from `/api/studio/me`).
- **iOS:** `iosMain` (WKWebView, NSUserDefaults, Close button in place of Back) is written; Kotlin/Native
  iOS needs macOS, so it is compiled only in CI (`Mobile_Studio_iOS_Build`, 2026-10-31), not yet green.
  The Xcode host is `iosApp/` (SwiftUI shell, XcodeGen `project.yml`), built unsigned in the same job. iOS plan (2026-10-31): P2 parity only, built on hosted CI (no Mac), ad hoc
  distribution (an `itms-services` link on `stwwreleases`) once there is a paid Apple Developer account and
  a registered iPhone. Hosted Azure macOS agents are Intel (arm64 preview paused) and Compose 1.12 publishes
  no `iosX64`, so iOS is compile-only in CI: no `iosTest`, no simulator smoke test.- Tests: `shared/app/src/commonTest` (API client, auto-start and launch, links, home model), run on the
  desktop JVM. No Android instrumented or iOS tests yet.
- CI and release (2026-10-31): `pipeline.yml` runs these tests (Test stage, `Mobile_Studio_Tests`) and, on
  main branches, builds a signed release APK in `Compile_Release`; `pipeline-MOBILE.yml` publishes it to
  `stwwreleases` with a download link (README, *CI and release*). Not yet run on a hosted agent.

## Phases

| Phase | Scope | Exit |
|---|---|---|
| **P0** | Spikes (below) | go/no-go per spike |
| P1 | Engine host + Unix socket on both platforms | a workflow runs over the socket from Kotlin |
| P2 | KMP skeleton + [app layout](#app-layout-2026-10-31) | UI list, auto-start, fullscreen UI, Edit in the Web Studio in the phone's browser. **Built** ([P2 build](#p2-build-2026-10-31)); not yet run on a device |
| P3 | Model catalog + tiering | right model downloaded and loaded per device |
| P4 | Generator (baseline System 1, then trained) | generated workflows pass `validate_workflow` |
| P5 | Editing parity | hand-editing matches the web studio's core flows |

### P0 spikes

1. Engine on Android (bionic): start it, run one workflow over the socket.
2. Engine on iOS, in-process: same test.
3. llama.cpp tokens/sec on a low-end and a mid-range device per platform.
4. System 1 next-tool accuracy on held-out corpus workflows.

## System 1 baseline

*Deprecated 2026-10-29; kept as the record.* Option (b) from §2, built first so the trained classifier has a number
to beat.

### Layout

| Path | What |
|---|---|
| `shared/generator` | Kotlin/JVM (moves under KMP with the app hosts): `FrontierLoop`, `NextActionChooser`, `LlmChoiceChooser`, `ToolCatalog.placeable` (never offers `Decision (legacy)`, which makes a workflow non-editable for `add_step`) |
| `eval` | Desktop-only harness: Ollama `LlmClient` (JSON-schema `format` holding the answer to one valid option index), held-out scoring against a majority-class floor |
| `corpus/Extractor` | C# console: `.bite` → `BiteFormat.XamlToX6` → branch walk → `examples.jsonl` (each decision with a `Steps` list: tool, label, the `[[variables]]` the step mentions), `workflows.jsonl`, `tools.json`. Maps data types with the engine's own `ToolCatalog.Resolve` (`InternalsVisibleTo` in `AssemblyAttributes.cs`) |
| `corpus/generate_goals.py` | One natural-language goal per workflow from the Claude CLI (no tools, no MCP, no settings), resumable |
| `corpus/Get-KuduBites.ps1` | Byte-exact `.bite` download from an engine's Kudu VFS with the caller's `az` token |
| `shared/generator` (classifier) | `ClassifierChooser` (option a) and a pure-Kotlin `WordPieceTokenizer` that matches Hugging Face's, checked against a golden fixture |
| `eval` (classifier) | `--export-features` (training rows built with `describeState`), `--client markov` (no-model baseline), `--client classifier` (`OnnxClassifier`, ONNX Runtime on the JVM) |
| `eval/bakeoff` | Whole-workflow comparison: `bakeoff select|run|judge|score|spotcheck` against a self-hosted engine (`WAREWOLF_DATA_DIR` outside the repo); contenders `ollama:`, `lmstudio:` (LM Studio, `LLM_BASE_URL` + `LLM_API_KEY`) and `claude:` |
| `eval/bakeoff` (artifacts) | Service tests and input forms: `bakeoff subjects`, `bakeoff tests|forms run|judge|score` (`Artifacts.kt`, `ArtifactBakeOff.kt`) |
| `train` | Python: fine-tunes a sentence encoder plus a linear head on the exported features, exports ONNX (float and int8); see `train/README.md` |

Build: `gradlew test` here; `dotnet test corpus/Extractor.Tests`.

### Corpus

The corpus is client production workflows and **never enters the repo**; it and everything derived from it
live outside (`C:\Users\ultra\wwc\` on the author's machine). Sources: Warewolf `Examples`, the
TheUnlimited CollectionServices / AccountHolderVerificationServices / ProfilerServices repos, and a personal
engine. 1,177 workflow files → 1,040 unique Service IDs, all converted → 12,299 decisions. Every workflow has
a goal generated by `generate_goals.py`.

- **Legacy activities** are mapped to their catalog replacement (`DsfMultiAssignActivity` → Assign, …).
  `DsfActivity` with `IsWorkflow=True` is a sub-workflow call (97% of its targets exist as corpus workflows);
  it was in 39% of workflows. Four workflows still hold an activity with no catalog equivalent.
- **Not represented:** steps inside containers (ForEach, Sequence, …) and branches that rejoin (they END where
  they meet a step already placed), because `add_step` attaches only at top-level tails.
- **Copy-pasted steps share a `UniqueID`** (34 corpus workflows). `WorkflowToX6Converter` used the
  `UniqueID` as the X6 node id, so those graphs had duplicate node ids and ambiguous or dropped edges. Fixed in
  the converter (`NodeIdFor`): a repeat gets a deterministic id derived from the `UniqueID` and its occurrence,
  which the next save persists. The extractor now rejects duplicate node ids explicitly.
- **Split:** whole sets are held out (`caps`, the personal engine); an eval example whose family also occurs
  in training is dropped, so near-copies cannot inflate accuracy.

### Baseline results (2026-10-28)

2,760 held-out decisions (`caps` + the personal engine; 268 dropped for sharing a family with training).
"Tool picks" scores only the 2,197 decisions where the author placed another step.

| Chooser | Overall | END (563) | Tool picks (2,197) |
|---|---|---|---|
| Always END (majority floor) | 20.4% | 100% | 0% |
| `qwen2.5:0.5b`, answer held to option numbers | 3.2% | 0% | — (answers "option 1" for almost everything) |
| `qwen2.5:0.5b`, answer held to tool names | 12.1% | 32.5% | 6.9% |
| `qwen2.5:1.5b`, answer held to tool names | 11.1% | 0% | 14.0% |
| TypeSafe Jev `jev-1.14.0`, one choice over tools + END | 20.4% | 16.3% | 21.4% |
| Jev, two-step ("finished?" then tool), threshold 0.5 | 17.4% | 0.4% | 21.8% |

- Jev, a non-generative decision model, names the right tool most often; the local generative models fall
  into default answers ("Assign Object", "Decision").
- Jev's "is this branch finished?" probability does not separate real ENDs from continuations
  (AUC 0.514; mean 0.115 vs 0.110), so no threshold rescues it.
- Likely cause: the state holds only the goal and the tool *names* so far, not what each step did.
- Run with `gradlew :eval:run --args="<derivedDir> [--model m] [--client jev [--two-step]] [--state names|rich]"`;
  Jev needs `JEV_API_KEY`. Sending client-derived goals to TypeSafe was approved for these runs. On Git Bash, pass
  `<derivedDir>` as a Windows path with `MSYS_NO_PATHCONV=1`, or Java cannot open it.

### Richer state (2026-10-28)

`--state rich` shows each earlier step as `Tool "label" [[var]], …` (label capped at 60 characters, 8 variables per
step) instead of the tool name alone. Same 2,760 held-out decisions; the re-extracted corpus has identical
paths, arms and answers. 68% of steps carry a label, 96% mention at least one variable. Inputs and outputs are not
told apart.

| Chooser | State | Overall | END (563) | Tool picks (2,197) |
|---|---|---|---|---|
| `qwen2.5:0.5b` | names | 12.1% | 32.5% | 6.9% |
| `qwen2.5:0.5b` | rich | 8.6% | 40.7% | 0.4% |
| `qwen2.5:1.5b` | names | 11.1% | 0% | 14.0% |
| `qwen2.5:1.5b` | rich | 12.8% | 2.1% | 15.5% |

- Rich state does not rescue the local generative models: 0.5b drifts to END and "Sort Records", 1.5b still
  answers "Decision" for most continuations. Neither reaches the 20.4% floor.
- Jev has not been run with rich state: that sends client step labels and variable names to TypeSafe, which was
  not part of the earlier approval.
- Jev with rich state (sending step labels and variable names approved on 2026-10-28): one choice **31.3%**
  overall (END 22.7%, tools 33.5%), the first chooser above the floor; two-step 25.9%, END AUC 0.683 (was 0.514).
  A decision model does use the extra state; the small generative models do not.

### Trained classifier (2026-10-28)

Features come from `--export-features`: the rich state text with only the last 6 steps (earlier ones counted), so
p99 is 454 tokens; a 384-token window truncated from the left keeps the most recent steps. Training families are
split 85/15 into train (7,861) and validation (1,406) by family hash; models are chosen on validation, and the
held-out sets are scored once. One seed (13) per run.

| Chooser | Held-out overall | END | Tool picks | Top-3 | END AUC | Validation overall |
|---|---|---|---|---|---|---|
| Always END (floor) | 20.4% | 100% | 0% | — | — | — |
| Jev, one choice, rich state | 31.3% | 22.7% | 33.5% | — | — | — |
| **Markov** (most frequent next step after the previous tool on that kind of arm) | **41.1%** | 65.2% | 34.9% | — | — | — |
| `bge-small-en-v1.5`, frozen encoder | 21.7% | 48.1% | 15.0% | 48.5% | 0.594 | 30.2% |
| `bge-small-en-v1.5`, fine-tuned | 36.6% | 63.6% | 29.7% | 60.4% | **0.845** | 41.9% |
| `all-MiniLM-L6-v2`, fine-tuned | 33.9% | 51.5% | 29.4% | 58.6% | 0.806 | 43.5% |

- **No trained model beats Markov yet.** The previous tool and arm carry most of the signal, and a mean-pooled
  encoder over a long goal does not pick it out as reliably. The fine-tuned models do separate real ENDs well
  (AUC 0.81–0.85, against Jev's 0.68).
- Validation prefers MiniLM, the held-out sets prefer bge-small: the held-out client sets differ from training,
  and a single seed cannot rank the two.
- The Kotlin chooser on the float ONNX model reproduces Python's held-out predictions on all 2,760 rows
  (tokenizer, state text and chooser verified end to end). **Dynamic int8 is not usable as is:** it changes 25%
  of answers and costs 4.8 points (bge-small 31.8%). Sizes: bge-small 134.5 MB float / 34.8 MB int8, MiniLM
  91.1 MB / 23.4 MB.
- Fine-tuned weights carry more of the client corpus than a frozen encoder; they are internal-only until shipping
  is cleared.
- Next levers: give the model the previous tool and arm directly (or combine its scores with the Markov prior,
  weight chosen on validation); several seeds per configuration; fp16 or static int8 instead of dynamic int8.

## Whole-workflow comparison (2026-10-28)

Question: is offline generation (System 1 or a small local model) good enough, or should Mobile Studio generate in the
cloud? 50 held-out corpus goals (`caps`, `asws`, families unseen in training; 16 small, 17 medium, 17 large). Every
contender ends with an `{envelope, body}` checked by the engine's `validate_workflow`; up to 2 repairs with the
validator's errors. Whole-workflow contenders all get the same prompt (schema, rules, all 66 tools with their fields,
the goal, about 22k tokens); local models are held to a JSON schema of the workflow. A blind Claude Opus judge rates
goal fit 1–5 from a normalised summary that hides the contender.

| Contender | Runs | Valid | Valid 1st try | Tool F1 vs author | Judge mean | Judge ≥4 | Median s |
|---|---|---|---|---|---|---|---|
| Claude Opus (cloud) | 50 | 100% | 96% | 0.44 | **3.96** | **82%** | 30 |
| Claude Sonnet (cloud) | 50 | 98% | 50% | 0.44 | 3.80 | 74% | 24 |
| System 1 classifier + qwen2.5:3b fields | 50 | 92% | 54% | 0.25 | 1.64 | 0% | 3 |
| System 1 Markov + qwen2.5:3b fields | 50 | 98% | 94% | 0.19 | 1.14 | 0% | 0 |
| qwen2.5:3b, whole workflow | 20 | 0% | 0% | 0.04 | 1.43 | 0% | 43 |
| qwen2.5:1.5b, whole workflow | 20 | 0% | 0% | 0.11 | 1.21 | 0% | 7 |

- **The offline options do not produce usable workflows.** System 1 passes the validator (its structure is valid by
  construction) but no System 1 workflow was rated 4 or more: wrong tools for the goal (a SQL call to build a URL)
  and a 3B model's field filling that misuses variables. Small models writing whole workflows never produced a valid
  one; most replies never closed the JSON. Markov ignores the goal and builds `Assign` then END for most goals.
- **The cloud models do**: Opus is valid first time 96% of the time and rated 4+ on 82% of goals, across all sizes
  (judge mean 3.75 small, 4.06 medium and large).
- Caveats: the judge is Opus rating Opus among others; a blind spot-check of 15 items (`spotcheck.html`) calibrates
  it. The qwen whole-workflow runs were capped at 20 goals each after 0 of the first 7 were valid (0/20 bounds the
  true rate below ~14% at 95%). System 1's code declares every variable as an output and none as inputs, which the
  judge sometimes marks down. Execution was not tested (no sources configured).
- **Engine findings** (to fix regardless of the decision):
  1. `add_step` accepts a Decision with no `expression`; the saved workflow then fails to convert back to X6
     (null reference) and is no longer editable.
  2. A Decision's node id changes on every `get_workflow_definition`, and `add_step` returns an id that is not in the
     saved graph, so nothing can be attached to a Decision's arms by id. System 1 therefore assembled its body
     itself and used the engine only to validate.
  3. `validate_workflow` rejects a Decision with an empty arm, while `get_tool_schema` says both arms are optional.
  4. 6 of 12 sampled author workflows are not editable at all: the legacy sub-workflow call (`DsfActivity`) has no
     X6 shape, and it appears in 39% of corpus workflows.
- Conclusion: on this evidence, generation belongs in the cloud. Offline generation would need a much larger
  on-device model or far more training data than 1,040 workflows; the phone can still browse, run and edit offline.

**Self-hosted large model (2026-10-29).** `lmstudio:google/gemma-4-31b-qat` on the same 50 goals, through LM Studio on
the author's PC (RTX 5070 Ti; the 18.9 GB Q4 model does not fit in 16 GB VRAM, so about 7 tokens/s). It runs against
port 4141 directly: through ask.leylew.is, Cloudflare ends a request after about 100 s (524). Gemma 4 loops on one token
under the JSON grammar at temperature 0, so this contender samples at 0.2 with seed 7. Results: *run in progress*.

## Service tests and input forms (2026-10-29)

Question: can a model write a **service test** for an existing workflow, and the **input form** a person fills in to
run it? Subjects: the 50 valid workflows Opus produced above (`bakeoff subjects` stores them in the engine as
`Bakeoff/<id>`, node ids rewritten to GUIDs; see finding 5). 46 of them have inputs and get a form.

- **Tests:** the model sees the goal, the workflow (steps by node id) and the engine's `test_schema`, and must mock every
  step that reaches outside the workflow (no sources are configured). `create_test` stores it (up to 2 repairs with the
  engine's error), then `execute_test` runs it.
- **Forms:** one JSON Schema (2020-12) document, which the studio would render. Checked against the meta-schema
  (`networknt` validator), for exactly one property per input, and for shape (scalar → primitive, object → object,
  recordset → array of objects with one property per field); up to 2 repairs with the problems.
- Both are rated 1–5 by the blind Opus judge (goal, workflow summary and artifact; the contender never shown).

| Tests | Runs | Stored 1st try | Stored | Ran | Passed | Outputs asserted | Mocks per test | Judge mean | Judge ≥4 | Median s |
|---|---|---|---|---|---|---|---|---|---|---|
| Claude Opus | 50 | 100% | 100% | 98% | 68% | 97% | 1.0 | 4.52 | 96% | 19 |
| Claude Sonnet | 50 | 100% | 100% | 100% | 72% | 96% | 1.1 | 4.24 | 90% | 14 |
| Gemma 4 31B (LM Studio) | | | | | | | | | | |

| Forms | Runs | Accepted 1st try | Accepted | Meta-schema valid | Inputs covered | Shapes right | Judge mean | Judge ≥4 | Median s |
|---|---|---|---|---|---|---|---|---|---|
| Claude Opus | 46 | 100% | 100% | 100% | 100% | 100% | 4.76 | 100% | 12 |
| Claude Sonnet | 46 | 100% | 100% | 100% | 100% | 100% | 4.70 | 100% | 10 |
| Gemma 4 31B (LM Studio) | | | | | | | | | | |

- Forms are easy for the cloud models. Tests are stored and run, but about 30% fail, almost all on expected values the
  model predicted wrongly (a validation outcome, a date, JSON formatting) or on an external step it did not mock (a
  web host, a file path). No repair is offered on a failing run; a "run, see the failure, fix the test" loop is the next
  lever, at the risk of weakening the assertions until they pass.
- Same judge caveat as above: Opus rates Opus.
- **Engine findings** (continuing the list above):
  5. `create_workflow` and `validate_workflow` accept non-GUID node ids (`getReadme`), but `execute_test` then fails
     with a null reference on a mocked step; the same test passes once the ids are GUIDs.
  6. An unmocked step whose source does not exist ends the test run with a null reference or "Unrecognized Guid
     format" instead of naming the missing source.
  7. `create_test`/`edit_test` pointed callers at `get_workflow_schema`'s `test_schema`, which did not exist. **Fixed:**
     `test_schema` is now returned, and `GetWorkflowSchemaToolTests` checks it lists every `AuthenticationType`,
     `StepType` and assert operator the engine accepts.
  8. The envelope had only `inputs` and `outputs`, so a working variable (Studio's `ColumnIODirection="None"`, the most
     common direction in the repo's workflows) could not be declared: `validate_workflow` rejected every reference to
     one, and `get_workflow_definition` dropped them, so a Studio workflow could not be read, edited and saved back.
     Local models lost whole goals to it (muse-glimmer's first four failures were each one undeclared scratch
     variable). **Fixed:** an optional `envelope.variables` array is written as `None`, read back by
     `get_workflow_definition`, and documented in `envelope_schema`; references to it validate. `add_step` still
     auto-declares as `Both`. Bakeoff runs before this fix are scored without it.
  9. A Decision arm was recognised only by the edge flags `data.isDecisionArm`/`data.isTrue`, while `body_schema`
     described Decision edges by their `true`/`false` label. Models that followed it (all 6 of Gemma 4 12B's arm
     errors, 6 of muse-glimmer's 12) connected both arms and still failed; LFM2 and the small Qwens left arms
     genuinely unconnected, and Opus and Sonnet never hit either. **Fixed:** `CommonHelper.TryGetDecisionArm`,
     shared by the converter, `validate_workflow` and `add_step`, uses the flags when present and otherwise a
     `true`/`false` label. The converter is shared with `Dev2.Server`, so both engines accept it.
  10. A JSON-object path written without its `@` (`[[ISSResponseObject.iss.latitude]]`) was reported as the
     undeclared variable `ISSResponseObject.iss.latitude`, although its root was declared. muse-glimmer (a4762f87,
     after the `envelope.variables` grammar fix) retried the same reference three times on that message. **Fixed:**
     `validate_workflow` still rejects it but names the fix: the `[[@...]]` form, and kind `object` when the root is
     declared as another kind. Still open: `add_step` auto-declares such a reference as a scalar named with the
     whole dotted path.

## Test strategy

The detailed test plan is presented for approval before any test is written (CLAUDE.md).

- **C#:** Unix-socket listening and mobile defaults in `SelfHostedEngineTests`.
- **Kotlin:** generator frontier/END handling, the LLM chooser, rich step summaries, the classifier chooser,
  the WordPiece tokenizer (golden fixture from Hugging Face), feature export and the Markov baseline (done), schema → grammar generation,
  tier selection, engine client; platform host tests for Android and iOS.
- **Corpus extractor (C#):** branch walk (linear, Decision, Switch, unknown types), legacy mapping, Service ID
  de-duplication, step labels and variables (done).
- **Comparison harness (`eval/bakeoff`):** engine client, workflow extraction and repair loop, System 1 body
  assembly (arms, cases, refills, declarations), field-filler constraints, structure scores, judge prompt and parsing,
  the LM Studio client (done).
- **Artifact tasks (`eval/bakeoff`):** service-test prompt, store/repair/run loop and output coverage; form check
  (meta-schema, coverage, shapes) and repair loop; GUID id rewrite; artifact judge (done).
- **Engine (C#):** `GetWorkflowSchemaToolTests` covers `test_schema` (done); `envelope.variables` is covered in
  `EnvelopeBiteWriterTests`, `ValidateWorkflowToolTests`, `GetWorkflowDefinitionToolTests` and
  `GetWorkflowSchemaToolTests`, including a round trip through the envelope writer (done).
- **Python (`train/tests`):** label encoding, class weights, ONNX export parity on a tiny synthetic model (done).
- **Evaluation harness:** System 1 next-tool accuracy; end-to-end `validate_workflow` pass rate per
  model tier.

## Artefacts to keep in sync

- `engine/npm/README.md` and `Build-NpmPackages.ps1` (new mobile runtimes, or a sibling build script).
- `warewolf-build` and `warewolf-architecture` skills, and `CLAUDE.md` (a third way to host the engine).
- `Dev/.azure/pipeline.yml` (`Mobile_Studio_Tests`, `build_release_apk`), `pipeline-MOBILE.yml` and `pipeline-MOBILE.Tests.ps1` (module or task names, signing, SDK level).
- `README.md` (build steps, modules) and `~/angular`'s `/organization-ui/` standalone page and `/workflow?engine=&path=` link, which the app depends on.
