# Prompt — "Deploy both" script (Execution Engine + Queue Processor)

> Paste everything below the line into Claude Code from the repo root.

---

## Task

Create a **new PowerShell script**, run by hand in **PowerShell 7+**, that deploys the Warewolf
Execution Engine and then the RabbitMQ Queue Processor in one pass by calling the two existing
orchestrators:

1. `Dev/Warewolf.Execution.Lightweight/Scripts/Deploy-WwExecutionEngine.ps1`
2. `Dev/Warewolf.Execution.Lightweight/Scripts/Deploy-WwQueueProcessor.ps1`

Suggested path: `Dev/Warewolf.Execution.Lightweight/Scripts/Deploy-WwEngineAndQueueProcessor.ps1`.
Confirm the name with me if `Scripts/README.md` conventions suggest a better one.

The new script is a **thin wrapper**. It must not re-implement, duplicate or second-guess anything
the two orchestrators already do (infra, auth, staging, encryption, image build, KEDA rules).

## Authoritative reference

`Dev/Warewolf.Execution.Lightweight/docs/prompt-assets/deploy-both-reference-runbook.ps1` is the
manual runbook this script is derived from. It is the **source of truth for the variable set, the
parameter set of each call, and the values passed**. Its header lists ten corrections already
applied to the original runbook, each citing the line in the orchestrator that proves it — read
those first, then read the full `param()` block and comment-based help of both orchestrators and
validate every parameter name against the real scripts. Do not reintroduce the defects.

Where the reference and the orchestrators disagree, the orchestrators win.

## The handover contract — read this before designing anything

The engine deploy is not a black box. It writes **two machine-readable outputs**, and the
QueueProcessor call plus every downstream command (app-role assignment, rollback, token
acquisition) is meant to be driven from them rather than from re-querying Azure:

| Output | Written by | Carries |
|---|---|---|
| `$LogDir\deploy-WwExecutionEngine-<stamp>.summary.json` | `Save-DeploySummary`, [Deploy-WwExecutionEngine.ps1:649-727](../Scripts/Deploy-WwExecutionEngine.ps1#L649-L727) | `status`, `dryRun`, `runId`, `resourceTags`, `created`, `endpoint`, `appName`, `appInsightsName`, `entraAppDisplayName`, `keyVault`, masked `appSettings` |
| `Scripts\Configure-WwExecutionAuth.output.json` | [Configure-WwExecutionAuth.ps1:1903-1921](../Scripts/Configure-WwExecutionAuth.ps1#L1903-L1921) | `ClientId`, `AppObjectId`, `SpObjectId`, `Audience`, `Issuer`, `AppRoles[].id`, `EntraAppDisplayName`, `FunctionAppName` |

Four properties of these files drive the design, and each has a failure mode if ignored:

1. **The summary is written incrementally after every phase**, with `status` of
   `in-progress` | `completed` | `failed`. Taking "the newest summary" without asserting
   `status -eq 'completed'` and `dryRun -eq $false` will happily hand an aborted run's values to
   the QueueProcessor.
2. **The auth output lives at a fixed path in the `Scripts` folder and is overwritten by every
   run.** It is not per-run and not in `$LogDir`. Read it immediately after the engine deploy and
   assert `FunctionAppName -eq $EngineApp` before trusting a single value from it. It is absent
   under `-SkipAuthProvisioning` and under `-DryRun`.
3. **The summary's `appInsightsConnectionString` is masked** ([:675-680](../Scripts/Deploy-WwExecutionEngine.ps1#L675-L680)),
   so it can never be the value for the QueueProcessor's `-AppInsightsConnectionString`. That one
   value must come from `az monitor app-insights component show`.
4. **`endpoint`, `appInsightsName` and `entraAppDisplayName` are authoritative** — do not hand-build
   `https://<app>.azurewebsites.net`, do not ask the operator to name the Entra app (it auto-derives
   to `<FunctionAppName>-auth`, [Configure-WwExecutionAuth.ps1:962,976-977](../Scripts/Configure-WwExecutionAuth.ps1#L962)),
   and do not assume the App Insights name.

**Additional requirement:** the wrapper must itself emit a **single consolidated handover file** —
`$LogDir\deploy-both-<stamp>.handover.json` — merging what it captured: `runId`, `resourceTags`,
engine `endpoint`, `appName`, `appInsightsName`, `EngineAppId` (ClientId), `EngineSpId`
(SpObjectId), `Audience`, the `Warewolf_QueueProcessor` app-role id, `RabbitMqSecretUri`, the
resolved QueueProcessor app names, and the paths of both source files. Secrets stay masked; the
App Insights connection string is **not** written to it. Downstream steps (the app-role grant, the
`Get-WwExecutionToken*` scripts, `Rollback-WwExecutionEngine.ps1 -SummaryPath`) then read one file
instead of re-deriving values. Print its path at the end of the run.

## Scope — deploy only

Implement exactly these steps, in order. Everything else in the reference runbook (Phase A resource
checks beyond A2, the B1/C1 dry runs, B4–B6 route verification, the C3 app-role grant, C4/Phase D
revision + KEDA checks, Phase E publish and replica watch) is **out of scope** — but C3 is what the
handover file exists to feed, so the file must carry everything C3 needs.

1. **Variable block** — placeholders, grouped and commented as in the reference.
2. **Publish-path preparation** — if `$EnginePublish` / `$QpPublish` is a `.zip`, extract it and
   repoint the variable at the extracted directory. `$QpPublish` **must** end up a directory;
   `Deploy-WwQueueProcessor.ps1` does not accept a zip.
3. **Validation, fail fast** — `Warewolf.Execution.Lightweight.dll` present under `$EnginePublish`;
   `Warewolf.Execution.QueueProcessor.dll` present under `$QpPublish`; and
   `(Get-ChildItem $QpPublish -Recurse -Filter *.bite).Count` **must be 0** (a stray `.bite` in the
   publish output is baked into the image and silently competes with the staged trigger). Abort with
   a clear message on any failure.
4. **Identity / subscription** — resolve `$Sub` / `$TenantId` from `az account show` when not
   supplied, then `az account set --subscription`. Fail with a clear message if `az` is absent or not
   logged in; do **not** call `az login` unattended.
5. **Resolve `$RabbitSecretUri`** — reference A2: read the vault URI from `$Kv` and append
   `/secrets/rabbitmq-uri`, versionless. Confirm the secret exists and is enabled before proceeding.
6. **Deploy the engine** — splat the B2 parameter set.
7. **Capture the handover values** — reference B3, per the contract above, and write the
   consolidated handover file.
8. **Deploy the QueueProcessor** — splat the C2 parameter set, consuming the captured values.

**The two calls are not independent.** `-EngineResourceAppId`, `-EngineBaseUrl` and
`-AppInsightsConnectionString` are produced by steps 6–7; `-RabbitMqSecretUri` by step 5. They must
never be placeholders in the QueueProcessor splat. Shared values (`$Rg`, `$Loc`, `$TenantId`, `$Kv`,
`$KvSecret`) are declared once and reused by both splats — never duplicated.

## Hard requirements

1. **Parameter coverage.** The two splats contain exactly the reference's parameter sets. Every
   *other* parameter of both orchestrators must still appear in the variable block, **commented
   out**, with a one-line note on what it does and why this wrapper does not pass it — so nothing is
   invisible to whoever edits the script later. Verify every name against the real `param()` blocks;
   do not invent any.
2. **Placeholders only.** `'<resource group>'`, `'<key vault name>'`, `'<engine app name>'` and so
   on, exactly as in the reference. **No** real tenant ids, subscription guids, app names, vault
   names, URIs or secrets — not from this repo, not from git history, not from the existing
   `*.summary.json` or `Configure-WwExecutionAuth.output.json` files under `Scripts/`.
   Values the orchestrators derive (`$EngineAi`, `$EngineAuthApp`, `$EngineUrl`) must be derived or
   read back, never offered as operator placeholders.
3. **Splatting.** One hashtable per call, splatted (`& $engineScript @engineParams`). Never build a
   command line as a string. A switch or `[nullable[bool]]` must only enter the hashtable when the
   user actually opted in — an unset placeholder must not be passed through as a real argument. If
   no App Insights connection string was resolved, omit `-EnableAppInsights` from the QueueProcessor
   splat rather than passing it with an empty string.
4. **Confirmation gate.** Prompt for an explicit `y/N` before each of the two real deploys, showing
   the resolved (masked) parameter set about to be used. `-NonInteractive` skips both prompts for
   unattended runs. Do **not** run a `-DryRun` pass first — the child scripts have their own plan
   phase. Still expose a `-DryRun` switch that forwards to both children.
5. **Ordering and failure behaviour.** The QueueProcessor runs only if the engine deploy succeeded
   *and* the capture step passed every assertion in the handover contract. Check `$LASTEXITCODE`,
   catch terminating errors, and abort with a clear message rather than deploying a worker that
   cannot authenticate. Provide `-SkipEngine` (deploy the QueueProcessor against an already-deployed
   engine, resolving the handover values from the existing summary + auth output, with the same
   assertions) and `-SkipQueueProcessor`.
6. **`az` argument quoting.** Carry over the reference's `Invoke-Az` helper and use it for every
   `az` call whose arguments contain `[`, `]`, `(`, `)` or `?` — the JMESPath queries in the capture
   step are exactly what it exists for.
7. **`-EncryptResources` is first-run-only.** Expose it as a wrapper parameter defaulting to off,
   with a comment explaining that later deploys stage already-encrypted sources as-is while still
   passing `-KeyVaultName` / `-KeyVaultSecretName`.
8. **Path resolution.** Resolve both child scripts and the auth output relative to `$PSScriptRoot`;
   fail with a clear message if a child script is missing. Create `$LogDir` before the first call and
   pass it to both.
9. **House style.** `#Requires -Version 7.0`, `[CmdletBinding()]`, comment-based help with an
   `.EXAMPLE`, `$ErrorActionPreference = 'Stop'`, and the section-header comment style already used
   by the orchestrators.

## Process

- **Plan first.** Per `CLAUDE.md`, produce a written plan — file to create, section layout, the full
  parameter inventory (passed vs commented-out) for both orchestrators, the handover-file schema,
  every assertion in the capture step, confirmation/skip switch behaviour, error handling — and
  **wait for my approval** before writing any code.
- **Propose tests, do not write them.** Name the Pester tests under `Scripts/Tests/` that should
  cover this (splat construction with unset placeholders, switch suppression, zip extraction and the
  `.bite`-count guard, each capture assertion — `status` not `completed`, `dryRun` true, mismatched
  `appName`, mismatched `FunctionAppName`, missing `ClientId`, missing auth output — handover-file
  contents and masking, `-SkipEngine` handover resolution, missing-child-script failure) and **wait
  for my go-ahead** before creating or editing any test file.
- **Verify without deploying.** Confirm the file parses
  (`[System.Management.Automation.Language.Parser]::ParseFile`), that every parameter name in both
  splats matches a real parameter on the target script, that every property the capture step reads
  exists in the two output schemas above, and that `-DryRun -NonInteractive` with dummy placeholders
  reaches both children's plan phase. **Never run a real deploy.**
- **Synchronize artefacts.** Per `CLAUDE.md`, tell me which docs need updating (`Scripts/README.md`,
  `docs/Deploy-RunGuide.md`, `docs/Deploy-EndToEnd-Runbook.md` — the last one documents the C3 grant
  that now consumes the handover file) and wait before editing them.
- **Do not commit.** Present a commit message only if I ask.

## Definition of done

- Script exists, parses under PowerShell 7+, and implements steps 1–8 above and nothing more.
- Both splats match the reference parameter sets; every other orchestrator parameter is present as a
  commented placeholder.
- None of the ten corrections listed in the reference header is reintroduced.
- Every assertion in the handover contract is enforced, and the consolidated handover file is written
  and its path printed.
- No real environment values anywhere in the file.
- `-DryRun -NonInteractive` with dummy placeholders reaches both children's plan phase.
- Test plan and doc-sync list presented to me, with nothing written until I approve.
