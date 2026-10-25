# Warewolf Mobile Studio — Plan

Status: **draft, pre-spike**. Branch `MobileStudio`, off `7580-SelfHostedEngine` (the npm engine
installer). No code yet; P0 spikes decide whether the plan holds.

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

## Test strategy

The detailed test plan is presented for approval before any test is written (CLAUDE.md).

- **C#:** Unix-socket listening and mobile defaults in `SelfHostedEngineTests`.
- **Kotlin:** generator frontier/END handling, schema → grammar generation, tier selection, engine
  client; platform host tests for Android and iOS.
- **Evaluation harness:** System 1 next-tool accuracy; end-to-end `validate_workflow` pass rate per
  model tier.

## Artefacts to keep in sync

- `engine/npm/README.md` and `Build-NpmPackages.ps1` (new mobile runtimes, or a sibling build script).
- `warewolf-build` and `warewolf-architecture` skills, and `CLAUDE.md` (a third way to host the engine).
