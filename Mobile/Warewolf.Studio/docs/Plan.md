# Warewolf Mobile Studio — Plan

Status: **P4 baseline under way, P0 engine spikes not started**. Branch `MobileStudio`, off
`7580-SelfHostedEngine` (the npm engine installer). The System 1 baseline (generator loop, LLM chooser,
corpus and evaluation harness) is built ahead of the spikes; see [System 1 baseline](#system-1-baseline).

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
│  └─ studio-ui      Compose Multiplatform: explorer, flow canvas, property sheets, run/debug
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

1. Browse, view, run, debug.
2. Generate with the new method, then edit fields.
3. Hand-editing with drag and drop.

## Phases

| Phase | Scope | Exit |
|---|---|---|
| **P0** | Spikes (below) | go/no-go per spike |
| P1 | Engine host + Unix socket on both platforms | a workflow runs over the socket from Kotlin |
| P2 | KMP skeleton + read-only studio | browse, view, run, debug |
| P3 | Model catalog + tiering | right model downloaded and loaded per device |
| P4 | Generator (baseline System 1, then trained) | generated workflows pass `validate_workflow` |
| P5 | Editing parity | hand-editing matches the web studio's core flows |

### P0 spikes

1. Engine on Android (bionic): start it, run one workflow over the socket.
2. Engine on iOS, in-process: same test.
3. llama.cpp tokens/sec on a low-end and a mid-range device per platform.
4. System 1 next-tool accuracy on held-out corpus workflows.

## System 1 baseline

Option (b) from §2, built first so the trained classifier has a number to beat.

### Layout

| Path | What |
|---|---|
| `shared/generator` | Kotlin/JVM (moves under KMP with the app hosts): `FrontierLoop`, `NextActionChooser`, `LlmChoiceChooser`, `ToolCatalog.placeable` (never offers `Decision (legacy)`, which makes a workflow non-editable for `add_step`) |
| `eval` | Desktop-only harness: Ollama `LlmClient` (JSON-schema `format` holding the answer to one valid option index), held-out scoring against a majority-class floor |
| `corpus/Extractor` | C# console: `.bite` → `BiteFormat.XamlToX6` → branch walk → `examples.jsonl`, `workflows.jsonl`, `tools.json`. Maps data types with the engine's own `ToolCatalog.Resolve` (`InternalsVisibleTo` in `AssemblyAttributes.cs`) |
| `corpus/generate_goals.py` | One natural-language goal per workflow from the Claude CLI (no tools, no MCP, no settings), resumable |
| `corpus/Get-KuduBites.ps1` | Byte-exact `.bite` download from an engine's Kudu VFS with the caller's `az` token |

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
- Likely cause: the state holds only the goal and the tool *names* so far, not what each step did. Next lever:
  richer state (step labels and outputs) before larger models or new question layouts.
- Run with `gradlew :eval:run --args="<derivedDir> [--model m] [--client jev [--two-step]]"`; Jev needs
  `JEV_API_KEY`. Sending client-derived goals to TypeSafe was approved for these runs.

## Test strategy

The detailed test plan is presented for approval before any test is written (CLAUDE.md).

- **C#:** Unix-socket listening and mobile defaults in `SelfHostedEngineTests`.
- **Kotlin:** generator frontier/END handling and the LLM chooser (done), schema → grammar generation,
  tier selection, engine client; platform host tests for Android and iOS.
- **Corpus extractor (C#):** branch walk (linear, Decision, Switch, unknown types), legacy mapping, Service ID
  de-duplication (done).
- **Evaluation harness:** System 1 next-tool accuracy; end-to-end `validate_workflow` pass rate per
  model tier.

## Artefacts to keep in sync

- `engine/npm/README.md` and `Build-NpmPackages.ps1` (new mobile runtimes, or a sibling build script).
- `warewolf-build` and `warewolf-architecture` skills, and `CLAUDE.md` (a third way to host the engine).
