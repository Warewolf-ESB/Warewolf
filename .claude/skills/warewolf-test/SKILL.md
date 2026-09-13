---
name: warewolf-test
description: How to run and write Warewolf tests — TestRun.ps1 unit/CI jobs, the in-process Lightweight integration harness, pipeline parity, unit-test best practices, the mandatory failure-summary template, and common failure patterns. Invoke whenever running, writing, or fixing any test (unit, integration, or SpecFlow). Defers full detail to TestRun.md.
---

# Testing Warewolf

## Unit tests / CI jobs — `TestRun.ps1`

Run from inside `Bin\ServerTests\` (direct mode) or from the repo root (catalog mode).

```powershell
cd Bin\ServerTests
.\TestRun.ps1 -Projects "Dev2.Activities.Tests"
.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -Filter "FullyQualifiedName~CalculateActivity"
.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -Category "UnitTest"
.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -ExcludeCategories "CannotParallelize","Integration"
.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -TestsToRun "TestName1,TestName2"
.\TestRun.ps1 -Projects "Dev2.Activities.Tests" -Coverage   # -> TestResults\Cobertura.xml
.\TestRun.ps1 -Jobs "Unit_Tests"   # run a CI pipeline job locally
.\TestRun.ps1 -List                # list all CI jobs
.\TestRun.ps1                       # run all CI jobs (full local pipeline)
```

Jobs needing external dependencies (FTP, SQL Server, MySQL, Elasticsearch, RabbitMQ, Redis) use `-Start*` flags — e.g. `-StartFTPServer`, `-StartMySQLServer` (Docker on Linux runtime, native on Windows). Full reference: [TestRun.md](../../../TestRun.md).

## Lightweight integration tests (in-process — no live engine)

`Warewolf.Execution.Lightweight.Integration.Tests` runs **fully in-process** via the `LightweightInProcessHost` harness (`InProcess/`). It does **not** require the Azure Functions engine on port 7071 and has **no external dependencies** (no public `httpbin.org`, no Elasticsearch, no Docker). These projects are not compiled into `Bin\ServerTests\`, so `TestRun.ps1` is not used — build once, then `dotnet test`:

```powershell
cd Dev
dotnet test "Warewolf.Execution.Lightweight.Integration.Tests\Warewolf.Execution.Lightweight.Integration.Tests.csproj" --logger "console;verbosity=normal"
dotnet test "...Integration.Tests\..." --no-build --filter "FullyQualifiedName~WebGetToolIntegrationTests"
dotnet test "...Integration.Tests\..." --no-build --filter "TestCategory=Authorization_Mapping"
```

How in-process dependencies are satisfied — **do not reintroduce live equivalents**:

- **httpbin** — in-process WireMock emulator (`InProcess/HttpbinEmulator.cs`) starts once per assembly on `localhost:4000`. This is the **sole** httpbin mechanism. The Web-tool `.bite` WebSources (`Dev/Warewolf.Execution.Lightweight/Resources/tools/http {get,post}/httpbin.bite`) and `TestConstants.cs` are pinned to `localhost:4000`. Do **not** add a go-httpbin sidecar (collides on port 4000 — removed from `Dev/.azure/pipeline.yml`).
- **Engine** — `LightweightInProcessHost` exposes `ExecutePublicAsync(routeName)` (direct `/public`) and `SendThroughPipelineAsync(method, path, headers)` (full EasyAuth → claims → policy pipeline), seeds a per-test `secure.config` via `SecureConfigBuilder`, and disables the license gate. `[DoNotParallelize]` is required for tests mutating the process-wide `SecureConfigLoader` singleton or env vars.
- **secure.config for security tests** — `CiTestSetup` (`[AssemblyInitialize]`, unit project) provisions a config so the 7 `F_RealConfig_*` tests run instead of skipping. Precedence: existing `WAREWOLF_TEST_SECURE_CONFIG` file → real machine config at `C:\ProgramData\Warewolf\Server Settings\secure.config` → synthetic `AllPublicGlobal` fallback.

> A **live** lightweight engine on port 7071 is only needed by **SpecFlow acceptance tests** (`-ServerType LightweightExecution`), not by `*.Integration.Tests`. Run one manually: `cd Dev\Warewolf.Execution.Lightweight; func start --port 7071` (or `dotnet run`). Free the port: `Get-NetTCPConnection -LocalPort 7071 -State Listen | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }`. SpecFlow `ServerType`: `FullServer` (port 3142) or `LightweightExecution` (port 7071).

## MCP tools — plain REST, no live-engine JSON-RPC tests

The 14 workflow-authoring MCP tools (`Mcp/ToolHandlers/`) are exposed as plain REST endpoints
under `/mcp-api/{tool_name}` (`Functions/McpApiFunctions.cs`), each a thin HTTP-trigger wrapper
around an unchanged, transport-agnostic `Handle(...)` method. The `/mcp` JSON-RPC/SSE endpoint
(`McpFunction`) and its dedicated `Warewolf.Execution.Lightweight.Mcp.Integration.Tests` live-engine
project have been **retired** — see "Known limitation" below for why. In-process coverage of the
new endpoints (auth gate, request binding, error mapping) lives in
`Warewolf.Execution.Lightweight.Tests\Functions\McpApiFunctionsTests.cs`; per-tool business logic
is still covered by each tool's own unit tests under `Warewolf.Execution.Lightweight.Tests\Mcp\`.
The actual MCP protocol surface (JSON-RPC framing, tool registration for AI clients) is now hosted
by a separate Node/Express server (`warewolf-devops-mcp`), which calls these REST endpoints via
plain `fetch()`.

> **Known limitation that motivated this move (confirmed, not something we can fix here):** the
> official `ModelContextProtocol.Client` SDK's `HttpClientTransport` always POSTs with
> `Transfer-Encoding: chunked` (no `Content-Length`) on .NET. Azure Functions' isolated-worker gRPC
> relay (`azure-functions-host`'s `GrpcMessageConversionExtensions.ToRpcHttp`) only forwards the HTTP
> body to the worker when `request.ContentLength > 0` — chunked requests have no `Content-Length`
> header, so the body is silently dropped before the worker ever sees it. This is an open,
> upstream host bug (`Azure/azure-functions-host#7930`), reproduced against both `func start` and
> real deployed Function Apps, not a local-dev-only artifact. Plain REST endpoints with ordinary
> string-bodied JSON requests (via `fetch()`/`HttpClient` + `StringContent`, which set
> `Content-Length` and never chunk) sidestep it entirely.

## Local verification and pipeline parity

When you change/add **unit tests** (or the code they cover):

1. **Run the affected tests locally until green.** Start any needed dependency first — a `TestRun.ps1 -Start*` flag, a local container (`docker run -d --name redis -p 6379:6379 redis:7-alpine`), a local service, or the in-process harness (`LightweightInProcessHost`). State which setup you used.
2. **Confirm `Dev/.azure/pipeline.yml` provides the same dependencies.** Check the CI job that runs those tests starts the same dependency and routes the tests to it — matching `-Start*` flags, `TestCategory` filters/exclusions, `-ServerType`, and host/port. If the pipeline wouldn't satisfy what the local run needed, adjust the categorisation or job so a local pass implies a pipeline pass.

## Unit-test best practices

- **Minimise external dependencies in unit tests.** Prefer an in-process replacement (fakes/mocks/in-memory harness such as `LightweightInProcessHost`) over a live service so tests are deterministic. Reserve real dependencies for `*.Integration.Tests` and `*.Specs`. Don't rewrite existing passing tests solely to remove a dependency.
- When a test genuinely requires a platform/elevation/external service that may be unavailable, degrade to **Inconclusive** (not fail) with a message explaining the prerequisite and how to run it.
- Keep tests **in sync** with code — a behaviour change must be matched by a test change. Plan tests *with* a new feature, not bolted on afterwards.
- **Never create or update unit tests automatically.** Always propose the tests and wait for the user's go-ahead. Present viable harness/type alternatives (unit vs integration vs spec; in-process fake vs live) and let the user choose. Prompt before adopting a newer framework feature/assertion style.
- **A regression test that has never failed proves nothing.** After writing one for a bug, *reintroduce
  the defect* behind a temporary hook (an env-var branch is enough), confirm the new tests fail, then
  remove the hook and confirm they pass. Report both numbers. Measured value on 2026-08-11: 4/8 pool
  tests and 11/11 pump tests failed with their defects restored — without that check, several
  assertions would have passed for reasons unrelated to the fix.
- **Watch for existing tests that encode the bug as intended behaviour.** Two did here
  (`Delivery_ConsumerFailed_IsLeftUnackedSoTheBrokerRedelivers` and its exception twin) and both still
  *passed* after the fix, because they asserted only "never acked" — which a nack also satisfies. Their
  names and comments documented a deadlock as a deliberate contract. Fixing a defect means auditing the
  tests that describe it, not just adding new ones.
- **Assert the invariant, not the symptom.** The concurrency defect needed no database: two renters must
  never hold the same activity instance, so the tests assert *object identity*. That runs in
  milliseconds with no SQL Server, no engine and no network, and cannot flake.

## After every test run: failure summary and fix

**Always produce a failure summary** whenever any tests fail:

```
## Test run summary
Total: N | Passed: N | Failed: N | Skipped: N | Duration: Xm Ys

### Failed tests
| # | Test name | File:line | Error (first line) |
|---|---|---|---|
| 1 | SomeTest | SomeFile.cs:42 | Assert.AreEqual failed … |

### Failure analysis
For each failure:
- Classification: network | auth | workflow-not-found | data-assertion | timeout | configuration | other
- Root cause: <2-3 sentences citing the specific error message and code path>
- Recommended fix: <concrete next step>
```

**After the summary, always prompt:**
> "Would you like me to fix the failed tests? If yes, I will produce a plan for each failure before making any changes."

## Common failure patterns

| Error | Likely cause | Fix |
|---|---|---|
| HTTP 500 wrapped error on a `/secure` or `/public` route (denial) | Policy denial — engine wraps authorization denials as **500**, not 403 (403 path commented out, WOLF-8418). `secure.config` group/permission lacks required `View`/`Execute` | Grant the group the needed perms in the per-test config; for `/public` add `View: true` + `Execute: true` to the `Public` group, or set `WAREWOLF_BYPASS_SECURE_CONFIG=true` |
| `Address already in use` / WireMock fails to bind on port 4000 | Stray process (or reintroduced go-httpbin container) holds 4000 | Kill the process on 4000; do not run a httpbin sidecar — the in-process `HttpbinEmulator` owns 4000 |
| Web-tool assertion `Expected <http://localhost:4000/...> Actual <https://httpbin.org/...>` | `HttpbinEmulator` host/url out of sync with `TestConstants` / `.bite` address | Keep all three on `localhost:4000` (emulator `HttpbinHost`/`HttpbinBase`, `.bite` `Address`, `TestConstants`) |
| HTTP 404 on a workflow path | Workflow `.bite` file missing from `Resources/` | Verify the workflow file exists; check `workflow-index.json` |
| `Assert.AreEqual failed` with mismatched values | Workflow logic or output mapping changed | Read the workflow XML and align test expectations |
| MCP-style tool call gets a 400 `"bad_request"` from `/mcp-api/{tool}` | A tool `Handle(...)` threw `McpException` (validation, not-found, or permission-denied) — all map uniformly to 400 | Read the `message` field for the specific reason; it's the tool's own `McpException.Message` |
