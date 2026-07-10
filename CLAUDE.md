# CLAUDE.md

Guidance for Claude Code in this repository. **These instructions override default behaviour.**

This file is deliberately lean — it loads on every prompt. Task-specific detail lives in skills
under `.claude/skills/` (loaded on demand) and in the deep reference docs. **Invoke the matching
skill before doing that kind of work** — don't reproduce its content from memory.

| When you are… | Invoke skill | Deep reference |
|---|---|---|
| Building (server, studio, tests, release) | `warewolf-build` | `Compile.md` |
| Running / writing / fixing tests | `warewolf-test` | `TestRun.md` |
| Analysing or changing a subsystem; reasoning about auth/policy | `warewolf-architecture` | `Dev/Warewolf.Execution.Lightweight/docs/Part1-Architecture.md` |
| Deploying the Lightweight engine / editing deploy scripts & plans | `warewolf-deploy` | `Dev/Warewolf.Execution.Lightweight/Scripts/README.md`, `docs/Deploy-RunGuide.md` |
| Producing a commit message | `warewolf-commit` | — |

## Collaboration rules

- **Never assume.** When a request is ambiguous — missing context, unclear scope, or multiple valid interpretations — ask a focused clarifying question before proceeding.
- **Show evidence and confidence.** For any non-trivial claim about the codebase (architecture, behaviour, root cause), cite the file and line range you read and state your confidence level (e.g. *high — observed directly in code*, *medium — inferred from surrounding code*, *low — haven't verified*).
- **Always plan before coding.** For any change beyond a trivial fix, produce a written plan (affected files, approach, test strategy) and wait for approval before touching code.
- **Always prompt to plan unit tests first.** For any change — new code or edits — identify the unit tests to create or update, present that test plan, and wait for the user's go-ahead before writing or modifying any test. **Never create/update tests automatically.** (Details: `warewolf-test`.)
- **Every change must leave tests passing.** After implementing a change and its agreed tests, run the affected tests locally until green (start any required dependency, or use an in-process harness) and report the result. Do not treat work as done while related tests fail.

## Change synchronization (mandatory)

Any change to the repo ripples outward. **Before finishing any change, identify and prompt the user to synchronize every affected artefact** — never silently leave them stale:

- **Code** — callers, dependents, and shared libraries the change touches.
- **Unit/integration tests** — tests covering the changed code must be created or updated in lockstep (propose them, wait for go-ahead — see `warewolf-test`). Confirm the change passes **all** relevant tests, not just new ones. Call out any test you could not run and why.
- **Docs** — `docs/` folders, `README*.md`, `*.md` runbooks, and this `CLAUDE.md` / the skills, when documented behaviour changes.
- **Scripts** — deployment/setup scripts and their `Tests/` when behaviour, roles, or requirements change (see `warewolf-deploy`).
- **Best practices / conventions** — keep them consistent with the change; prompt before adopting a newer approach.

State explicitly which artefacts you updated, or that none needed updating, and surface anything you could not reconcile.

## Expert context

Work at an enterprise-grade level against this stack:

- **.NET 8** (C# 12) — nullable reference types enabled, modern language features preferred
- **Azure Functions v4 isolated worker** — the Lightweight engine specifically (Entra ID, Easy Auth, Azure Key Vault, Application Insights)
- **Clean Architecture** — respect layer boundaries; domain logic must not depend on infrastructure
- **Code quality** — prefer refactoring/optimisation over new abstractions; reduce duplication; improve readability without changing behaviour unless asked

## Model selection

Pick the model tier that matches the task. If the targeted model is unavailable, **do not fall back silently** — prompt: *"Model `<model-id>` is not available. Please select a model to continue: Opus / Sonnet / other."*

| Task type | Model | Current model ID |
|---|---|---|
| **Heavy** — deep reasoning, root-cause analysis, architectural design, complex multi-file refactors, security review | Latest Claude **Opus** | `claude-opus-4-8` |
| **Normal** — routine edits, simple bug fixes, test stubs, documentation, straightforward single-file changes | Latest Claude **Sonnet** | `claude-sonnet-4-6` |

## Architecture orientation

Warewolf is a .NET 8 SOA/ESB platform with a visual flow-based designer; workflows authored in Warewolf Studio (WPF) / Warewolf Web Studio (Angular) execute as `.bite` files. `Dev/` holds ~133 source projects and ~55 test projects. There are **two execution engines** sharing the same activity/runtime libraries:

| | **Lightweight** | **Server** |
|---|---|---|
| Project | `Warewolf.Execution.Lightweight` (Azure Functions v4 isolated, port 7071) | `Dev2.Server` (Windows service, port 3142) |
| Auth | JWT/Entra ID, anonymous `/Public/*`, function-key `/Services/*` | Internal Warewolf auth |
| Build | included in `-ServerTests` | `.\Compile.ps1 -Server` |

Full layer/project breakdown and the Lightweight authorization model (EasyAuth → claims → policy; **denials wrapped as HTTP 500 not 403, WOLF-8418**; `apis.json` discovery rules) → invoke `warewolf-architecture`.

## Commit messages

**STRICTLY never auto-commit** — never run `git commit`/`git push` on your own. When asked to "create a commit message", only **stage** files and present the message as text; commit only on an explicit, unambiguous instruction. Full format (`{branch-number}-#{Feature|Fix}-{branch-name}`, body rules, model-named `Co-Authored-By` trailer) → invoke `warewolf-commit`.
