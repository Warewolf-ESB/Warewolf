# Spec — `Warewolf License.secureconfig` is silently discarded under the Lightweight (Azure Functions isolated-worker) host

**Status:** Root-caused, fixed in code (§6, option 2), and **now live-verified end-to-end against
`warewolfserver-mcp`** (RG `DEV2`, subscription `dd0bc517-5cc7-4b56-bd6a-68e6140db7b3`) — §7's
acceptance test passed 2026-08-21: `set_license` → `get_license_status` round-tripped `Active`/
`isLicensed:true` correctly across the file-backed path, and `execute_workflow` against a real
`start → Assign` workflow returned a genuine successful result (no license-blocked error).
`WAREWOLF_LICENSE_CHECK_ENABLED` is `true` (enforced, not bypassed) on the live instance — §8.1's
workaround is no longer needed/active.
**2026-08-21 addendum — a *second*, related regression found and fixed:** the live license
reverted to `NotActive` mid-session, traced to an unrelated redeploy (zip-deploy of a fresh
`dotnet publish` output) picking up a stray placeholder `Warewolf License.secureconfig` left in
this project's local `bin/Release/<TFM>` output by an earlier local run/test (see the fix's own
constructor, `SubscriptionConfig()` → `AppContext.BaseDirectory`) and shipping it as if it were
build content, overwriting the live activated license on deploy. Fixed defensively at the project
level: `Warewolf.Execution.Lightweight.csproj`'s new `RemoveRuntimeLicenseFileFromPublish` target
(`AfterTargets="Publish"`) deletes any such file from `$(PublishDir)` on every publish, regardless
of which deploy script/method runs afterwards — confirmed by test (a stray file placed in the
Release build output no longer appears in the publish output). This is a distinct failure mode
from the one this spec originally documents (CWD-vs-`AppContext.BaseDirectory` resolution) but has
the same symptom (license silently reset) and the same underlying lesson: this file is
**runtime-owned state, never build/publish content**, on either host.
**Author:** drafted 2026-08-20 while investigating `warewolfserver-mcp-workflow-authoring-bugs.md`
bug 3 ("`execute_workflow` is blocked by licensing on this instance"); fix + new MCP licensing
tools added the same day; live-verified and hardened against the publish-time regression 2026-08-21.
**Affects:** every Lightweight Function App deployment that stages a license via
`Deploy-WwExecutionEngine.ps1 -LicenseConfigPath` (or any other means) — this is not specific to
`warewolfserver-mcp`.

---

## 1. Why this supersedes the original bug-3 write-up

`warewolfserver-mcp-workflow-authoring-bugs.md` treated bug 3 as "not itself a bug" — just an
unlicensed test instance, fixable by provisioning a subscription. That ask has already been
attempted and failed in a way that reveals a real defect:

1. A genuine `Warewolf License.secureconfig` (`Status="Active"`, real `SubscriptionKey`/`PlanId`/
   `SubscriptionSiteName`) was staged directly into `site/wwwroot/` on `warewolfserver-mcp` via
   `az webapp deploy --type static` (the same file `-LicenseConfigPath` stages pre-deploy).
2. The Function App was restarted to force a fresh worker.
3. `GET /IsLicensed` still reported `{"isLicensed":false,"status":"NotActive","planId":"NotRegistered", ...}`.
4. Re-fetching the staged file via Kudu VFS (AAD-token auth — SCM basic auth is disabled on this
   app) showed its content had been **overwritten in place** with different values than what was
   staged.

So staging the file is necessary but **not sufficient** — the running engine cannot correctly
read it back, and actively destroys the staged file by writing defaults over it.

## 2. Reproduction (evidence)

Staged file (`az webapp deploy --type static --target-path "site/wwwroot/Warewolf License.secureconfig"`):

```xml
<subscriptionSettings>
  <add key="CustomerId" value="" />
  <add key="SubscriptionId" value="" />
  <add key="Status" value="Active" />
  <add key="PlanId" value="khonzumusa.hlophe@theunlimited.co.za" />
  <add key="SubscriptionKey" value="Azq9KATrttxzMIhF" />
  <add key="SubscriptionSiteName" value="warewolf" />
  <add key="StopExecutions" value="false" />
</subscriptionSettings>
```

File content read back after a restart (via `GET https://warewolfserver-mcp.scm.azurewebsites.net/api/vfs/site/wwwroot/Warewolf%20License.secureconfig`, AAD bearer token):

```xml
<subscriptionSettings>
  <add key="CustomerId" value="" />
  <add key="SubscriptionId" value="" />
  <add key="MarketplaceResourceId" value="" />
  <add key="Status" value="aT/AoVWEMyf6OPvaYp47Gw==" />
  <add key="PlanId" value="qj2HmQwVsUt12btj/iXadA==" />
  <add key="SubscriptionKey" value="ml420y+ZHMiv8CoQJxF1XMsYXXxCcDgNkvFkZSJHQB+m3EdlYIeUAP8oEIl9Z29b" />
  <add key="SubscriptionSiteName" value="tWPn5xcpWET9NX3yt+uPHQ==" />
  <add key="StopExecutions" value="r/EOk8xFEhRno3TYRCvIKQ==" />
</subscriptionSettings>
```

These five values are a **byte-for-byte match** to the hard-coded "broken installation" fallback
constants in
[`SubscriptionProvider.cs`](../../Dev2.Runtime.Services/Subscription/SubscriptionProvider.cs)
(`SubscriptionDefaultStatus`, `SubscriptionDefaultPlanId`, `SubscriptionLiveKey`,
`SubscriptionLiveSiteName`, `StopExecutionsDefault`) — not a re-encrypted copy of the real,
staged values. This proves the engine never actually read the staged file's real content; it took
the "file/section missing → write defaults" path in
[`SubscriptionConfig.cs`](../../Dev2.Runtime.Services/Subscription/SubscriptionConfig.cs) and then
**persisted those defaults back over the real file**.

## 3. Root cause (high confidence — corroborating evidence below; not yet confirmed with a debugger/repro harness)

`SubscriptionConfig` (`Dev2.Runtime.Services/Subscription/SubscriptionConfig.cs`) was written for
the classic **`Dev2.Server`** hosting model and relies on two CWD-relative assumptions that don't
hold for the Lightweight **Azure Functions .NET isolated-worker** host:

1. **`EnsureSubscriptionConfigFileExists()`** calls `File.Exists(FileName)` where
   `FileName = "Warewolf License.secureconfig"` — a bare relative path, resolved against
   `Environment.CurrentDirectory`, **not** `AppContext.BaseDirectory`.
2. **`ConfigurationManager.GetSection("subscriptionSettings")`** resolves the `configSource`
   redirect declared in `app.config` (compiled to `Warewolf.Execution.Lightweight.dll.config`,
   confirmed present and correctly wired in the deployed package — see §4) the same
   CWD-relative way.

`Dev2.Server` works around this implicitly: `Dev2.Server/Main.cs` calls
`Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))`
at startup, so `Environment.CurrentDirectory` is pinned to the app's own bin directory before
`SubscriptionProvider.Instance` is ever touched. **`Warewolf.Execution.Lightweight`'s `Program.cs`
has no equivalent call** — grepping the whole project for `CurrentDirectory` returns zero hits.
Under the Azure Functions .NET isolated-worker host, the worker process's CWD is set by the
Functions host and is not guaranteed to equal `site/wwwroot` (where the DLL, `.dll.config`, and
staged license file all actually live). Every other place in Lightweight that needs a
deployment-relative path deliberately avoids this trap:
[`WorkflowHttpFunction.cs`](../Functions/WorkflowHttpFunction.cs) resolves
`WorkflowsDirectory` via `Path.Combine(AppContext.BaseDirectory, "Resources")`, never a bare
relative path — this class is the one exception, because it is shared, older code that predates
the Lightweight port.

When `ConfigurationManager.GetSection` returns an effectively-empty `NameValueCollection` (or
`EnsureSubscriptionConfigFileExists`'s own `File.Exists` check fails), `SubscriptionConfig.Initialize`
falls into its `else` ("Broken Installation") branch, which both (a) sets in-memory fields to the
hard-coded live/default constants and (b) calls `SaveConfig` → writes those defaults back to
`FileName` — which, given the write path lands in `site/wwwroot` (confirmed by the VFS mtime
change), suggests `Environment.CurrentDirectory` for this write call did happen to resolve to
`site/wwwroot`, even though the *read* did not pick up the real file. This apparent asymmetry
(write succeeds at the "right" path, read doesn't) is exactly the kind of behaviour a
`ConfigurationManager`/`configSource` resolution failure — as opposed to a simple CWD mismatch —
would produce, and is why this spec stops short of asserting a single definitive mechanism without
a debugger attached to a live isolated-worker process (see §6, open question).

## 4. What was ruled out

- **The `.dll.config` wiring is present and correct.** Fetched
  `Warewolf.Execution.Lightweight.dll.config` from the deployed `site/wwwroot` via Kudu VFS —
  content matches `app.config` exactly:
  ```xml
  <?xml version="1.0" encoding="utf-8" ?>
  <configuration>
    <configSections>
      <section name="subscriptionSettings" type="System.Configuration.NameValueSectionHandler" />
    </configSections>
    <subscriptionSettings configSource="Warewolf License.secureconfig" />
  </configuration>
  ```
  So this is not a missing/stale deployment artifact.
- **Not a licensing/subscription provisioning gap.** The staged file has `Status="Active"` and
  real-looking key material; the engine never got as far as evaluating it.
- **Not a plaintext-vs-encrypted format issue** (`Protect-LicenseConfig.ps1`) **per se** — even if
  `Initialize()`'s `isPlainText` re-encrypt path had fired instead, it operates on the *parsed*
  field values and would still require `ConfigurationManager` to have read the real values first;
  the observed output (constants, not re-encrypted real values) rules that path out too.

## 5. Impact

- **Every** Lightweight deployment that relies on `-LicenseConfigPath` (the documented, supported
  staging mechanism in `Deploy-WwExecutionEngine.ps1`) to license a Function App is silently
  unlicensed, with no error surfaced anywhere except `IsLicensed: false` at runtime.
- `execute_workflow` (MCP tool) and any `/Secure/`, `/Public/`, `/Services/`, `/workflow` HTTP
  execution route are blocked by `WorkflowExecutor`'s license gate
  (`Execution/WorkflowExecutor.cs:175-190`) on every such deployment, whenever
  `WAREWOLF_LICENSE_CHECK_ENABLED` is left at its default (`true`).
- The silent-overwrite behaviour is actively harmful: a correctly-staged real license file is
  destroyed on the very first cold start, so even re-staging it after the fact doesn't help without
  restarting again into the same failure.

## 6. Fix approach chosen and implemented

Any fix must not change `Dev2.Server`'s own (currently working) behaviour, since
`SubscriptionConfig`/`SubscriptionProvider` are shared between both hosts. Three approaches were
weighed; **option 2 was chosen and implemented** (2026-08-20):

1. **(Considered, not chosen) Isolated-worker-specific CWD pin.** Add a
   `Directory.SetCurrentDirectory(AppContext.BaseDirectory)` call very early in
   `Warewolf.Execution.Lightweight/Program.cs`, mirroring `Dev2.Server/Main.cs:119`. Smaller, but
   `Directory.SetCurrentDirectory` is process-wide and could affect other relative-path
   assumptions elsewhere in the isolated worker; option 2 below removes the CWD dependency
   entirely rather than just relocating it, which is the more defensible fix given this spec's own
   uncertainty (§3) about whether CWD alone was the full mechanism.
2. **(✅ Chosen and implemented) Made `SubscriptionConfig` host-agnostic.** Replaced the
   `ConfigurationManager`/`configSource` indirection entirely with explicit, absolute-path file
   I/O:
   - `SubscriptionConfig()` (production ctor) now resolves the license file against
     `AppContext.BaseDirectory` — the deployment directory of the running assembly, correct for
     both hosts without any DI container or host-specific startup wiring.
   - A new `SubscriptionConfig.ForBasePath(string basePath)` static factory lets a caller (or a
     test) supply an explicit base directory instead, for the isolated-worker host or for
     verification.
   - `EnsureSubscriptionConfigFileExists()`/`SaveConfig` no longer call
     `ConfigurationManager.GetSection`/`RefreshSection` or `File.Exists(FileName)` against a bare
     relative name at all — both existence-check, read (`ReadConfigFile`, direct `XDocument.Load`
     parsing of the same `<subscriptionSettings><add key=".." value=".." /></subscriptionSettings>`
     shape `UpdateConfig` writes), and write now resolve through one absolute
     `_configFilePath` computed once at construction. A "not found" read can therefore never
     overwrite a *different*, real file living elsewhere on disk — the exact failure mode
     reproduced in §2.
   - `Dev2.Server` is unaffected: it already pins its own CWD to its bin directory at startup
     (`Main.cs:119`), which is the same directory as `AppContext.BaseDirectory` there too, so its
     existing `SubscriptionConfigTests`/`SubscriptionProviderTest` suite passes unchanged (verified
     — see §9).
   - Touched: `Dev2.Runtime.Services/Subscription/SubscriptionConfig.cs` (rewritten per above),
     `SubscriptionProvider.cs` (new `SetLicense` method, §10), `ISubscriptionProvider.cs` (new
     interface member), plus the shared test doubles/tests listed in §9.
3. **(Still recommended, not yet done) Fail loudly instead of silently.** `SubscriptionConfig`'s
   "Broken Installation" branch now logs at `Warn` (upgraded from `Info`) when it detects empty
   key/site values and is about to write defaults, but does not yet distinguish "this file already
   existed with real values that failed validation" from "this is a genuinely fresh install" at the
   `Error` level the original spec recommended. Worth a follow-up if that distinction proves useful
   in practice once the fix is live.

## 7. Acceptance test — automated AND live, now passing

The manual acceptance steps originally sketched here are backed by real automated tests (see §9)
exercising the actual file-backed constructor path end-to-end — previously **zero** tests
exercised that path at all (every existing test used the `NameValueCollection` constructor
directly, bypassing file resolution entirely, which is exactly how this bug shipped unnoticed).

**Live verification against `warewolfserver-mcp`, completed 2026-08-21:**

1. ~~Deploy the fixed package~~ — done (zip-deploy via `az functionapp deployment source
   config-zip`, `dotnet publish -c Release` output).
2. `set_license` (`status: "Active"`) → `get_license_status` → `{"isLicensed": true, "status":
   "Active", ...}`, confirmed on the running instance with no manual restart/retry required
   (verified via direct `POST /mcp-api/set_license` + `POST /mcp-api/get_license_status`, since
   `-LicenseConfigPath` pre-staging wasn't used this round — the new `set_license` MCP tool, §10,
   bootstrapped it live instead, which is exactly the scenario it was added for).
3. *(Not repeated this round — covered by §9's `SubscriptionConfigTests` regression tests instead
   of a fresh Kudu VFS re-fetch.)*
4. `execute_workflow` against a real `start → Assign` workflow (`FixVerification/AcceptanceTest`)
   → `{"outputs": {"Result": "hello from acceptance test"}, "status": "success", "error": null}` —
   a genuine successful execution, not the licensing block.
5. `WAREWOLF_LICENSE_CHECK_ENABLED` confirmed `true` (enforced) on the live instance — §8.1's
   workaround was not active going into this round and remains unnecessary.

## 8. Current live state (as left)

**Superseded — see the 2026-08-21 addendum at the top of this doc and §7's live verification.**
`warewolfserver-mcp` is licensed (`isLicensed: true`, `status: "Active"`) via the new `set_license`
MCP tool, and `execute_workflow` runs real workflows successfully. The publish-time regression this
addendum describes (a stray build-output license file overwriting the live one on redeploy) is now
guarded against by `Warewolf.Execution.Lightweight.csproj`'s `RemoveRuntimeLicenseFileFromPublish`
target. The history below (as originally written 2026-08-20) is kept for context on why the
workaround in §8.1 existed.

`warewolfserver-mcp`'s `site/wwwroot/Warewolf License.secureconfig` was re-staged with the
original real (plaintext) content as a best-effort restore after this investigation. Given the
root cause above, expect it to be silently overwritten again by the next cold start — this is not
a stable fix, just leaves the intended file in place for whoever picks up the code fix next.
`GET /IsLicensed` on `warewolfserver-mcp` still reports `isLicensed: false`. **The fix in §6 has
not yet been deployed to `warewolfserver-mcp`** — it exists in code (this branch) only; §7's live
verification is still outstanding.

### 8.1 Workaround applied (2026-08-20): `WAREWOLF_LICENSE_CHECK_ENABLED=false`

Since the license gate this bug blocks (`WorkflowExecutor.IsLicenseCheckEnabled()`,
`Execution/WorkflowExecutor.cs:175-190`) is a separate, independent env-var check that never calls
`SubscriptionProvider`/`SubscriptionConfig` at all when disabled, it sidesteps this bug entirely —
unlike `POST /secure/Subscriptions` (`LicensingHttpFunction.SaveSubscriptionData`), which was also
considered and ruled out: it calls `SubscriptionProvider.SaveSubscriptionData` → `RefreshInstance` →
`new SubscriptionConfig()`, hitting the exact same broken resolution path, so it isn't a reliable
alternative either.

`WAREWOLF_LICENSE_CHECK_ENABLED` was therefore set to `false` on `warewolfserver-mcp` as a
temporary workaround, both live (`warewolf-devops-mcp`'s `warewolf_set_var`, in-process
immediately) and durably at the ARM app-settings level (`az functionapp config appsettings set`,
survives restarts/cold starts). Verified: `warewolf_execute_workflow` against `ClaudeMCPTest` no
longer returns `"Execution blocked: a valid Warewolf license/subscription is required."` — it now
reaches actual workflow execution (and fails there for an unrelated reason: `ClaudeMCPTest` is a
minimal test workflow with only a bare `start` node and no activities, so `ActivityParser.Parse`
throws `Value cannot be null (Parameter 'source')` — a workflow-content issue, not a licensing one,
and out of scope for this spec).

**This is a bypass, not a fix.** `GET /IsLicensed` still reports `isLicensed: false`; this only
disables enforcement of that state for execution. Treat `warewolfserver-mcp` as intentionally
running unlicensed-but-unblocked until §7's live verification passes and a real license is
confirmed to actually load — at which point remove this workaround (restore
`WAREWOLF_LICENSE_CHECK_ENABLED` to unset/`true`).

## 9. Tests added for the §6 fix

All new; none of these existed before (the gap that let the original bug ship — see §7):

- `Dev2.Runtime.Tests/Services/SubscriptionConfigTests.cs` — four new tests against
  `SubscriptionConfig.ForBasePath`, exercising the real file-backed path with no `SaveConfig`
  mocking: missing file writes defaults at the exact resolved absolute path; an existing real
  (plaintext) file is read correctly and re-encrypted in place rather than being overwritten with
  defaults (the spec's own §2 reproduction, now a regression test); an empty-valued file falls
  back to defaults; and — the direct regression test for the original bug — changing
  `Environment.CurrentDirectory` between calls has no effect on resolution.
- `Dev2.Runtime.Tests/Services/SubscriptionProviderTest.cs` — new tests for `SetLicense` (§10):
  sets `SubscriptionKey` from supplied data; still pins `SubscriptionSiteName` to the current
  instance regardless of input; null-argument guard. Also a new regression test proving
  `SaveSubscriptionData` (unchanged, pre-existing method) never lets a caller change
  `SubscriptionKey`/`SubscriptionSiteName` — the exact contract `SetLicense` had to preserve while
  adding key-settability. **Note:** this test class is marked `[DoNotParallelize]` — its tests
  construct `SubscriptionProvider` subclasses whose base constructor mutates process-static
  `_config`/`_theInstance` fields (pre-existing design, not introduced by this fix); running them
  in parallel was observed to race and produced spurious failures before this attribute was added.
- `Warewolf.Execution.Lightweight.Tests/Mcp/ToolHandlers/SetLicenseToolTests.cs` and
  `GetLicenseStatusToolTests.cs` (new files, §11) — fully mocked `ISubscriptionProvider`, no shared
  process state.
- All pre-existing `SubscriptionConfigTests`/`SubscriptionProviderTest`/`SaveSubscriptionDataTests`/
  `GetSubscriptionDataTests` (Dev2.Server's own path) verified still green — confirmed 28/28
  passing together with the new tests above.

**Verification run:** `Dev2.Runtime.Tests` (Subscription filter) 28/28 passed;
`Warewolf.Execution.Lightweight.Tests` (full suite) 1032/1036 passed — the 4 remaining failures are
pre-existing, unrelated `AuthenticationTagMismatchException`-vs-`CryptographicException` AES-GCM
assertion mismatches in `FileEncryptionHelperTests.cs` (confirmed to fail identically on the
unmodified baseline via `git stash`), nothing to do with this fix.

## 10. New: `ISubscriptionProvider.SetLicense` — an additive licensing primitive

`SaveSubscriptionData` (§8.1, the Chargebee plan/status-update path) deliberately keeps this
instance's existing `SubscriptionKey`/`SubscriptionSiteName` no matter what a caller passes — see
`SubscriptionProvider.SetNewSubscriptionData`. That is correct for its purpose (a Chargebee
webhook/plan change must never be able to re-key an instance) but means it **cannot bootstrap a
never-licensed or broken-installation instance** — exactly the state this whole bug leaves an
instance in (§2/§8). A new, additive method was added instead of changing that contract:

```csharp
void SetLicense(ISubscriptionData subscriptionData);
```

Sets `CustomerId`/`PlanId`/`SubscriptionId`/`MarketplaceResourceId`/`Status`/`StopExecutions` **and**
`SubscriptionKey` from the supplied data — but still always pins `SubscriptionSiteName` to whatever
the instance already has (never caller-settable through either method). This is the primitive the
new `set_license` MCP tool (§11) uses to actually license a deployed-but-unlicensed instance over
HTTP, once §6's fix is deployed and this method can reliably reach the real file.

## 11. New MCP tools for licensing an already-deployed server

Two new tools were added to `Warewolf.Execution.Lightweight`'s existing `/mcp-api/{tool_name}`
surface (`Mcp/ToolHandlers/`, wired through `Functions/McpApiFunctions.cs` — see that file's own
docs for why this is REST, not JSON-RPC), for the external `warewolf-devops-mcp` broker to proxy as
first-class `warewolf_*` tools (that broker's source is not in this repo — this section is the
handoff spec for whoever maintains it):

- **`set_license`** (`Mcp/ToolHandlers/SetLicenseTool.cs`) — licenses/re-licenses this instance
  live over HTTP, no Kudu/VFS file access or redeploy required. Administrator permission required
  (whole-host operation, like `set_var`). Partial update: every field besides `status` defaults to
  "keep the current value" when omitted. `subscriptionKey` is treated as a secret exactly like
  `add_source`'s password-shaped connection fields: pass `"${secret-name}"` for a name already
  staged in this host's Key Vault (or, on a Key-Vault-less dev host, an environment variable of
  that name) — the server resolves it server-side via the existing `IMcpSecretResolver`
  (`KeyVaultMcpSecretResolver`/`EnvironmentMcpSecretResolver`), never the caller. A literal key is
  still accepted (not enforced, matching `add_source`'s own convention), but the response never
  echoes the key back either way. No `subscriptionSiteName` parameter exists at all — that field
  can never be set through this tool (§10).
  **Dependency:** only reliably usable once §6's fix is deployed — before that, `SetLicense`'s
  underlying `new SubscriptionConfig()`/`RefreshInstance` call hits the same broken resolution path
  as `SaveSubscriptionData` did (§8.1).
- **`get_license_status`** (`Mcp/ToolHandlers/GetLicenseStatusTool.cs`) — read-only wrapper around
  the same local check `GET /IsLicensed` already performs (`LicensingHttpFunction.IsLicensed`), for
  a caller driving `set_license` to confirm the resulting state without a separate raw HTTP call.
  No Chargebee call, no extra permission gate beyond the baseline `/mcp-api/*` authentication (the
  same reasoning `IsLicensed` itself being `AuthorizationLevel.Anonymous` already establishes).

**The "bake into a read-only package" side of licensing an already-deployed server** (for
`WEBSITE_RUN_FROM_PACKAGE` deployments where `site/wwwroot` is read-only, so neither `set_license`
nor a direct VFS write to `secure.config` work) **needs no new tool** — the existing
`warewolf_build_variant_package`'s generic `extraFiles` parameter already supports injecting an
arbitrary `"Warewolf License.secureconfig"` alongside `secure.config` into a rebuilt package, the
same way that tool already handles `secure.config` itself. Document this usage for whoever
maintains `warewolf-devops-mcp`, rather than adding a redundant license-specific variant of that
tool.

