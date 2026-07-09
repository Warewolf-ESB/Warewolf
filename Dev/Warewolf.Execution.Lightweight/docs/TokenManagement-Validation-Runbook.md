# Token-Management Validation Runbook — Caller → Execution Engine

A manual, copy-paste **PowerShell 7 + Azure CLI** runbook that validates the **full app-only token
lifecycle** between a client caller Function App and the Warewolf Execution Engine, end-to-end:

1. **Token issue** — the caller's Managed Identity acquires an app-only token for the engine.
2. **Token refresh** — the caller caches and re-acquires the token proactively.
3. **Token expiration** — expiry handling on the caller (proactive) and the engine (rejection).
4. **Token validation** — the engine validates signature, issuer, audience, lifetime, and roles.

**Reference setup (edit the two variables in [§1](#1-session-setup) for your own apps):**

| Role | App | URL |
|---|---|---|
| Caller client app | `wwenginecaller103` | `https://wwenginecaller103.azurewebsites.net` |
| Execution Engine | `wwenginetest103` | `https://wwenginetest103.azurewebsites.net` |

> **Source of truth.** Caller token lifecycle: `Warewolf.Execution.Lightweight.ClientExamples/AzureFunction`
> (`Auth/WwExecutionTokenHandler.cs`, `WwExecutionCallerOptions.cs`, `Functions/CallWorkflowOnHttpTrigger.cs`).
> Engine token validation: `Warewolf.Execution.Lightweight`
> (`Auth/Parsers/BearerTokenPrincipalParser.cs`, `Auth/Middleware/ClaimsPrincipalBuilderMiddleware.cs`,
> `Auth/Models/EntraAuthOptions.cs`, `Security/EntraTokenValidator.cs`).

---

## How token management works (what each check proves)

**Caller (`wwenginecaller103`).** A single `DelegatingHandler` owns the whole lifecycle:

- **Issue** — `DefaultAzureCredential` (Managed Identity in Azure) acquires a token for
  `api://<ResourceAppId>/.default`; an MSAL client-credentials fallback is used only when
  `WwExecution:ClientId` + `:ClientSecret` are set (local dev).
- **Refresh** — the token is cached in-memory and re-acquired when within a **5-minute buffer** of
  expiry, single-flighted with a semaphore so concurrent requests trigger only one acquisition.
- **Expiration** — a token is "valid" while `ExpiresOn − 5 min > UtcNow`. Refresh is **proactive
  only — there is no reactive 401→refresh retry** on the downstream call.
- **`/api/info`** returns the raw token + decoded header/claims. It acquires the token **directly
  from the credential, bypassing the handler's cache**, so it is a pure issuance/claims probe.

**Engine (`wwenginetest103`).** Two validators, by route:

- **Execution (`/secure`, `/services`)** — `ClaimsPrincipalBuilderMiddleware` runs
  `EasyAuthPrincipalParser` then `BearerTokenPrincipalParser`. The Bearer parser does **full**
  validation: issuer, audience, lifetime, and **RS256 signature** against Microsoft's OIDC signing
  keys (cached, auto-refreshed), with a 2-minute clock skew.
- **Discovery (`apis.json`)** — falls back to `EntraTokenValidator`, which checks issuer/expiry/tenant/
  audience but **does not verify the signature** (it trusts Easy Auth's upstream validation).
- **Authorization** — the token's `roles` claim is matched against `secure.config`
  `WindowsGroupPermissions`; a roleless caller or a missing per-workflow `Execute` row is **denied as
  HTTP 500** (WOLF-8418), not 403.

---

## 0. Prerequisites

| Requirement | Why |
|---|---|
| PowerShell 7+, Azure CLI (`az`) logged in | All checks run through `az` / `Invoke-RestMethod` |
| Caller MI assigned the engine app role **`Warewolf_ClientApps`** | Otherwise every `/secure` call is denied (500) |
| Engine app settings `WAREWOLF_ENTRA_TENANT_ID` + `WAREWOLF_ENTRA_AUDIENCE` (and/or `_CLIENT_ID`) set | Enables the engine's Bearer validation pipeline |
| `secure.config` has a `Warewolf_ClientApps` row (`IsServer=false`, `Execute=true`) for the tested workflow | The policy half of the two-part authorization contract |
| Reader access to the caller's logs (Application Insights or `az webapp log tail`) | Observing the refresh/single-flight behaviour |

```powershell
$PSVersionTable.PSVersion   # >= 7.0
az account show             # confirms login
```

---

## 1. Session setup

```powershell
az login

# ── Targets (edit for your environment) ──────────────────────────────────────
$CallerUrl = 'https://wwenginecaller103.azurewebsites.net'
$EngineUrl = 'https://wwenginetest103.azurewebsites.net'
$Workflow  = 'Hello World'                       # must have a Warewolf_ClientApps Execute row
$WfPath    = [Uri]::EscapeDataString($Workflow)  # 'Hello%20World'

# Engine identifiers (used to assert aud/iss/tid on the token)
$TenantId      = az account show --query tenantId -o tsv
$ResourceAppId = az webapp auth show -g '<engine-rg>' -n '<engine-app>' `
                   --query 'identityProviders.azureActiveDirectory.registration.clientId' -o tsv
# If that returns blank, retry with 'properties.identityProviders...clientId' (CLI-version dependent).

# ── JWT decode helper (no signature check — inspection only) ──────────────────
function ConvertFrom-Jwt([string]$Jwt) {
    $p = $Jwt.Split('.')[1].Replace('-', '+').Replace('_', '/')
    switch ($p.Length % 4) { 2 { $p += '==' } 3 { $p += '=' } }
    [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($p)) | ConvertFrom-Json
}
```

---

## 2. Area 1 — Token **issue**

Confirm the caller's identity can acquire a token and that its claims target the engine.

```powershell
$info = Invoke-RestMethod "$CallerUrl/api/info"
$info | ConvertTo-Json -Depth 6

$claims = $info.claims
"aud   = $($claims.aud)"     # expect: api://<ResourceAppId>  (or the bare <ResourceAppId>)
"iss   = $($claims.iss)"     # expect: https://login.microsoftonline.com/<TenantId>/v2.0 or sts.windows.net/<TenantId>/
"tid   = $($claims.tid)"     # expect: <TenantId>
"appid = $($claims.appid)"   # the caller MI / client app id
"roles = $($claims.roles)"   # expect: Warewolf_ClientApps
"exp   = $([DateTimeOffset]::FromUnixTimeSeconds([long]$claims.exp).UtcDateTime)  (UTC)"
```

| Check | Pass criteria |
|---|---|
| Endpoint responds | `200`, body has `rawToken`, `expiresOn`, `header`, `claims` |
| Audience | `aud` = `api://$ResourceAppId` (or `$ResourceAppId`) — matches the engine's `WAREWOLF_ENTRA_AUDIENCE` |
| Issuer / tenant | `iss` contains `$TenantId`; `tid` = `$TenantId` |
| Role present | `roles` contains **`Warewolf_ClientApps`** (proves the app-role assignment) |
| Algorithm | `header.alg` = `RS256`, `header.typ` = `JWT` |

> **If `roles` is missing:** the caller MI is not assigned the engine app role — re-run
> `Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -DaemonUseManagedIdentity …`. A roleless
> token authenticates but is denied at authorization (see [§5](#5-area-4--token-validation-engine)).

---

## 3. Area 2 — Token **refresh** (cache reuse + single-flight)

`/api/info` bypasses the cache, so refresh is validated through the **proxy** route `/api/run`, which
uses the cached handler. Observe the caller's logs while driving traffic.

**Start a log stream (choose one), then run the calls in a second shell:**

```powershell
# Option A — live log tail
az webapp log tail -g '<caller-rg>' -n '<caller-app>'

# Option B — Application Insights (last 15 min)
az monitor app-insights query --app '<caller-ai>' `
  --analytics-query "traces | where timestamp > ago(15m) | where message has 'Acquired Warewolf Execution Engine token' | project timestamp, message | order by timestamp asc"
```

```powershell
# Drive 10 sequential calls
1..10 | ForEach-Object { (Invoke-WebRequest "$CallerUrl/api/run/$WfPath`?Name=Refresh$_").StatusCode }

# Drive a concurrent burst (single-flight test)
1..20 | ForEach-Object -Parallel { Invoke-WebRequest "$using:CallerUrl/api/run/$using:WfPath`?Name=Burst$_" | Out-Null } -ThrottleLimit 20
```

| Check | Pass criteria | Evidence |
|---|---|---|
| Cache reuse | The **"Acquired … token"** log appears **once**, not per request, across the 10 calls | log stream |
| Single-flight | The concurrent burst of 20 produces **at most one** new acquisition | log stream |
| Warm reuse | All 30 calls return the workflow result (2xx) without re-authenticating | HTTP status |

> A fresh cold start (or a >~55-min gap) legitimately logs one acquisition. What you are proving is
> that *steady-state* traffic reuses the cached token and that a cold-cache burst collapses to a
> single Entra request (the `SemaphoreSlim` in `GetTokenAsync`).

---

## 4. Area 3 — Token **expiration**

```powershell
$info = Invoke-RestMethod "$CallerUrl/api/info"
$exp  = [DateTimeOffset]::FromUnixTimeSeconds([long]$info.claims.exp)
$life = $exp - [DateTimeOffset]::UtcNow
"Token expires $($exp.UtcDateTime)Z — ~$([int]$life.TotalMinutes) min remaining"
```

| Check | How | Pass criteria |
|---|---|---|
| Proactive refresh window | Read `exp`; the caller re-acquires when < **5 min** remain | `exp` is a normal ~60–90 min Entra lifetime; refresh is not deferred to the instant of expiry |
| Engine rejects an expired token | Capture a token, wait until **past `exp` + 2-min skew**, then replay it directly against the engine (see snippet) | Engine returns **401** |
| No reactive retry (known gap) | Note that the caller has no 401→refresh loop; a client-clock skew > 5 min could surface a hard failure | Documented as a risk, not a pass/fail |

```powershell
# Replay a (now-expired) captured token straight at the engine — run AFTER exp + 2 min:
$tok = (Invoke-RestMethod "$CallerUrl/api/info").rawToken   # capture, then wait out its lifetime
Invoke-WebRequest "$EngineUrl/secure/$WfPath`?Name=Expired" -Headers @{ Authorization = "Bearer $tok" } -SkipHttpErrorCheck |
  Select-Object -ExpandProperty StatusCode                  # expect 401
```

---

## 5. Area 4 — Token **validation** (engine)

Drive the engine directly with tokens minted via the caller, exercising each validation dimension.

```powershell
$tok = (Invoke-RestMethod "$CallerUrl/api/info").rawToken

function Invoke-Engine([string]$Route, [string]$Bearer) {
    $h = @{}; if ($Bearer) { $h.Authorization = "Bearer $Bearer" }
    (Invoke-WebRequest "$EngineUrl/$Route" -Headers $h -SkipHttpErrorCheck).StatusCode
}
```

| # | Case | Command | Expected |
|---|---|---|---|
| 5.1 | **Valid token** | `Invoke-Engine "secure/$WfPath`?Name=Valid" $tok` | **2xx** (workflow JSON) |
| 5.2 | **No token** on `/secure` | `Invoke-Engine "secure/$WfPath" $null` | **401** (auth enforced) |
| 5.3 | **Tampered signature** | flip the last char of the signature segment (below) | **401** (RS256 check fails) |
| 5.4 | **Wrong audience** | acquire a Graph-scoped token and send it (below) | **401** (`aud` mismatch) |
| 5.5 | **Expired token** | replay a captured token past `exp`+skew ([§4](#4-area-3--token-expiration)) | **401** |
| 5.6 | **Roleless / no policy row** | call a workflow with **no** `Warewolf_ClientApps` `Execute` row, or from an identity lacking the role | **500** (WOLF-8418 denial, nested `Error{…}`) |
| 5.7 | **Public route** (anonymous) | `Invoke-Engine "public/$WfPath`?Name=Anon" $null` | **2xx** (no token required) |

```powershell
# 5.3 tamper the signature
$parts = $tok.Split('.'); $sig = $parts[2].ToCharArray()
$sig[-1] = ($sig[-1] -eq 'A') ? 'B' : 'A'; $parts[2] = -join $sig
Invoke-Engine "secure/$WfPath`?Name=Tampered" ($parts -join '.')   # expect 401

# 5.4 wrong audience (token for Microsoft Graph, not the engine)
$graph = az account get-access-token --resource 'https://graph.microsoft.com' --query accessToken -o tsv
Invoke-Engine "secure/$WfPath`?Name=WrongAud" $graph               # expect 401
```

> **5.6 clarifies the two-part contract:** a *valid* token that carries the role still needs a matching
> `secure.config` `Execute` row for that workflow. Test the policy half by calling a workflow the role
> is **not** granted → the engine authenticates the token but denies authorization → **500**.

---

## 6. Cross-cutting configuration checks

```powershell
# Engine Entra settings must match the caller's target
az functionapp config appsettings list -g '<engine-rg>' -n '<engine-app>' `
  --query "[?starts_with(name,'WAREWOLF_ENTRA_')].{name:name,value:value}" -o table
# WAREWOLF_ENTRA_TENANT_ID == $TenantId
# WAREWOLF_ENTRA_AUDIENCE  == api://$ResourceAppId  (matches token aud from §2)

# Easy Auth should be enabled on the engine (so the no-signature EntraTokenValidator fallback is never the sole gate)
az webapp auth show -g '<engine-rg>' -n '<engine-app>' --query 'platform.enabled'

# Caller app settings target the engine
az functionapp config appsettings list -g '<caller-rg>' -n '<caller-app>' `
  --query "[?starts_with(name,'WwExecution')].{name:name,value:value}" -o table
```

---

## 7. Results matrix (pass criteria)

| Area | Check | Result |
|---|---|---|
| Issue | `/api/info` returns RS256 JWT, `aud`/`iss`/`tid` match, `roles=Warewolf_ClientApps` | ☐ |
| Refresh | Steady-state reuse (one acquisition/cycle); burst collapses to one acquisition | ☐ |
| Expiration | Normal lifetime + proactive 5-min refresh; engine 401s an expired token | ☐ |
| Validation 5.1 | Valid token → 2xx | ☐ |
| Validation 5.2/5.3/5.4/5.5 | No/tampered/wrong-aud/expired token → 401 | ☐ |
| Validation 5.6 | Roleless / missing policy row → 500 (WOLF-8418) | ☐ |
| Validation 5.7 | `/public` anonymous → 2xx | ☐ |
| Config | Engine `WAREWOLF_ENTRA_*` matches token; Easy Auth enabled; caller targets engine | ☐ |

---

## 8. Risks & notes surfaced by this validation

1. **No reactive 401→refresh on the caller.** Expiry is handled only by the 5-minute proactive buffer;
   a client-clock skew larger than that (or a token revoked mid-life) yields a hard failure with no
   retry. Consider adding a one-shot refresh-and-retry on a downstream 401.
2. **`EntraTokenValidator` does not verify the signature.** It is used only for `apis.json` discovery
   filtering and trusts Easy Auth's upstream validation — so **Easy Auth must stay enabled** on the
   engine, or that path would accept unsigned/forged tokens. Execution routes are unaffected (they use
   the fully-validating `BearerTokenPrincipalParser`).
3. **`/api/info` discloses the raw bearer token** in its response body. Acceptable for a test/inspection
   tool, but it should not be exposed on a production caller without protection.
4. **Denials are HTTP 500, not 403** (WOLF-8418) — check 5.6 expects 500; do not treat it as a server
   fault.

---

## Related docs

- [`Deploy-EndToEnd-Runbook.md`](Deploy-EndToEnd-Runbook.md) — deploy the engine + register a caller as a Daemon.
- [`KB-ClientApps-Configuration.md`](KB-ClientApps-Configuration.md) — per-example client configuration + validation.
- [`README-Authentication.md`](README-Authentication.md) — end-to-end auth architecture.
- `Warewolf.Execution.Lightweight.ClientExamples/AzureFunction/README.md` — the caller sample.
