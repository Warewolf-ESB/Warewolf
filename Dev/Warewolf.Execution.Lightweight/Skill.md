# Skill — Deploy the Warewolf Lightweight Execution Engine to Azure Functions

> **Audience:** an LLM agent (or an engineer driving one) that must take a built
> `Warewolf.Execution.Lightweight` output and stand it up correctly in Azure —
> **configured, licensed, secured, and logging** — with no prior context.
>
> **Companion script:** [`Deploy-ToAzure.ps1`](./Deploy-ToAzure.ps1) creates the Azure
> resources and zip-deploys the package. This document is the **brain** around that
> script: it tells you *what to put in the package*, *which app settings to set*, and
> *how to prove each subsystem is working*. The script alone is **not** sufficient for a
> production deployment — it does not set application settings, licensing, or auth. Follow
> this Skill end-to-end.

---

## Terminology — what "a release" is

A **release of Warewolf lightweight execution** = a **zip containing the `Warewolf.Execution.Lightweight`
Release-config publish output only** (i.e. `dotnet publish -c Release` artifacts — binaries,
`host.json`, runtime config, the deploy/helper scripts, **this `Skill.md`**, plus the staged
`Resources/`, `secure.config`, `Warewolf License.secureconfig`, and
`Settings/ElasticsearchLoggingSource.bite`). It contains **no source code**.

> Two distinct zips — do not confuse them:
> - **Release publish zip** (this artifact, handed to the deployer): **includes `Skill.md`** so an
>   operator/LLM deploying from the zip alone has the deploy guide. Excludes source and the
>   `docs/` deep-dive guides.
> - **Azure deployment package** (what `Deploy-ToAzure.ps1` uploads to the running Function App):
>   **excludes** `Skill.md`, `*.md`, `docs/`, and `local.settings.json` to stay lean — see §3.1.

---

## 0. TL;DR — the golden path

```text
1. Build/publish the engine            → dotnet publish -c Release -o ./publish
2. Stage the package:                  → ./publish/
     Resources/*.bite                  (your workflows; REQUIRED by the script)
     secure.config                     (authorization policy; see §5)
     Warewolf License.secureconfig      (license; ENCRYPTED via Protect-LicenseConfig.ps1; see §4)
     Settings/ElasticsearchLoggingSource.bite  (logging sink; ships with the build; see §6)
3. Create + deploy:                    → ./Deploy-ToAzure.ps1 -AppName <globally-unique>
4. **Set application settings (§3.4):** → az functionapp config appsettings set ...   ← Deploy-ToAzure.ps1 sets NONE; the instance has zero env vars until you run this
5. Configure Entra Easy Auth (§5):     → ./Scripts/Configure-WwExecutionAuth.ps1
6. Verify each subsystem (§7):         → licensing, security, logging smoke tests
```

If you do only step 3 (deploy) and skip 4–6, the app will be **unlicensed**
(executions blocked), will **deny every request with HTTP 503** (no effective
`secure.config`), and will emit **no logs**. Do not stop at step 3.

> ⚠️ **`Deploy-ToAzure.ps1` does NOT set a single application setting / environment variable.**
> A freshly deployed app has **none** of the `WAREWOLF_*` / `AZURE_*` / logging settings — they
> exist only after you run the `az functionapp config appsettings set` command in **§3.4**.
> Step 4 is mandatory, not optional.

---

## 1. What this engine is (mental model)

`Warewolf.Execution.Lightweight` is an **Azure Functions v4 isolated-worker (.NET 8 `Exe`)**
host that executes Warewolf `.bite` workflows over HTTP. It is one of two Warewolf
execution engines (the other is the full `Dev2.Server` on port 3142). The lightweight
engine shares `Dev2.Activities` / `Dev2.Runtime.*` but is hosted entirely by the
Functions runtime.

### 1.1 Cold-start sequence (`Program.cs`)

Knowing this order tells you *why* each piece of configuration matters and *when* it is read:

| Step | Action | Reads |
|---|---|---|
| 1 | Load `HostEnvironmentConfig` | `WorkflowsDirectory`, `AZURE_KEYVAULT_NAME`, `KEYVAULT_SECRET_NAME`, `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `WEBSITE_INSTANCE_ID`, `AZURE_FUNCTIONS_ENVIRONMENT` |
| 2 | Bootstrap console logger (so no startup log is lost) | `EXECUTIONLOGLEVEL` |
| 3 | Build host (`ConfigureWarewolf` + `AddExecutionLogging`) | logging flags |
| 4 | `StartupOrchestrator.RunStartupAsync` — wires the **Key Vault AES decrypt hook**, warms the workflow index | Key Vault secret |
| 5 | Upgrade `Dev2Logger.ExternalSink` to the full **CompositeExecutionLogger** (Console + App Insights + Elasticsearch + Audit). Resolved **after** step 4 so the decrypt hook is live before the Elasticsearch `.bite` connection string is read | `ENABLEAPPLICATIONINSIGHTS`, `ENABLEELASTICSEARCHLOGGING` |
| 6 | License check via `SubscriptionProvider.Instance` | `Warewolf License.secureconfig` |
| 7 | `host.RunAsync()` | — |

### 1.2 Function classes (`Functions/`)

| Class | Routes | Auth |
|---|---|---|
| `WorkflowHttpFunction` | `/Public/*`, `/Secure/*`, `/Services/*`, by-name (`.debug`/`.xml`/`.api`), `/apis.json` | per route (see §5.4) |
| `LoginFunction` | `POST /login` → JWT | anonymous |
| `LicensingHttpFunction` | `GET /IsLicensed`, `GET /Subscriptions`, `POST /secure/Subscriptions` | anon / function-key / JWT-admin |
| `LogFileFunction` | execution log retrieval | — |
| `DropboxOAuthFunction` | OAuth callback | — |

`host.json` sets `routePrefix: ""`, so routes live at the **root** (no `/api` prefix):
`https://<app>.azurewebsites.net/Public/Hello%20World.json`.

---

## 2. Prerequisites

| Tool | Why |
|---|---|
| **Azure CLI (`az`)**, logged in (`az login`) | resource creation + zip deploy + app settings |
| **.NET 8 SDK** | `dotnet publish` |
| **PowerShell 7+** | `Protect-LicenseConfig.ps1`, `Encrypt-Config.ps1`, auth scripts |
| **Azure Functions Core Tools v4** *(optional)* | `func azure functionapp publish` as an alternative to the zip script |
| An Azure subscription with rights to create Resource Groups, Storage, Function Apps, and (for auth) Entra app registrations | — |

> **Azure best practice:** if an `azmcp_bestpractices_get` tool is available, invoke it
> before provisioning. Always prefer **Managed Identity** over secrets, **HTTPS-only**,
> **TLS 1.2+**, and least-privilege RBAC on Key Vault.

---

## 3. The deployment package & Azure Function App configuration

### 3.1 What `Deploy-ToAzure.ps1` does

Running the script from a folder (`$ScriptDir`):

1. Prompts for / accepts `AppName`, `ResourceGroup` (default `<AppName>-rg`), `Location`
   (default `eastus`), `StorageAccountName` (derived from `AppName`).
2. Checks for `secure.config` next to the script — the script only **reports** its absence; it
   still deploys. An automating agent must instead treat a missing `secure.config` as a
   **blocking preflight** and resolve it first (see §5.2), or the deployment will 503.
3. **Requires** a `Resources/` folder next to the script; warns if it has no `.bite` files.
4. Creates the **resource group**, **storage account** (`Standard_LRS`), and **Function App**
   (`--consumption-plan-location`, `--runtime dotnet-isolated --runtime-version 8 --functions-version 4`)
   if they do not already exist.
5. Zips the contents of `$ScriptDir` — **excluding documentation and dev-only artifacts**
   (`*.md`, the `docs/` folder, `Skill.md`, `local.settings.json`) — and runs
   `az functionapp deployment source config-zip`.

> The script is copied into the build output (`CopyToOutputDirectory`), so the intended
> workflow is: **publish → run the script from the publish folder**. Whatever sits next to
> the script at that moment (minus the excluded docs/dev artifacts) becomes the deployed
> package. `Skill.md` and the `docs/` guides are **never** shipped to Azure — they live in
> the source tree only.

### 3.2 What the script does **NOT** do (you must do these)

> 🚨 **The deployed instance starts with ZERO application settings.** `Deploy-ToAzure.ps1`
> only creates infrastructure and uploads the zip — it never calls
> `az functionapp config appsettings set`. Every `WAREWOLF_*`, `AZURE_*`, Entra, Key Vault
> and logging variable is **absent** until you set it yourself with the command in **§3.4**.
> This is the single most common deployment defect: the app runs on bare defaults
> (unlicensed-gate behaviour, no Key Vault decrypt, no Entra validation, no logging).

- It does **not** set any application settings (logging, licensing, Entra, Key Vault) — **see §3.4 for the exact command to run immediately after deploy.**
- It does **not** enable Easy Auth / Entra (see §5.5).
- It does **not** enforce `--https-only` (set it yourself, below).
- It does **not** license the app (see §4).

### 3.3 Required / recommended files in the package

| File / folder | Required? | Purpose | Section |
|---|---|---|---|
| Function app binaries (`*.dll`, `host.json`) | ✅ | the engine | — |
| `Resources/**/*.bite` | ✅ (script errors without the folder) | your workflows | — |
| `Settings/ElasticsearchLoggingSource.bite` | ✅ (ships with build, `CopyToOutputDirectory=Always`) | Elasticsearch logging sink config | §6 |
| `secure.config` | ⚠️ Strongly recommended | authorization policy; **without it every request is 503** unless `BYPASS_SECURE_CONFIG=true` | §5 |
| `Warewolf License.secureconfig` | ⚠️ Recommended | license (encrypted); without it the app self-creates an unlicensed default and **blocks executions** | §4 |

### 3.4 Application settings — complete reference

Set with:

```bash
az functionapp config appsettings set --name <app> --resource-group <rg> --settings KEY=VALUE [KEY=VALUE ...]
az functionapp update --name <app> --resource-group <rg> --set httpsOnly=true   # enforce HTTPS
```

> ⚠️ **Mandatory post-deploy step — `Deploy-ToAzure.ps1` sets none of these.** Run the command
> below **immediately after** the deploy. First **prompt the operator for the Key Vault name and
> secret name** (§6.4) and substitute the `<...>` placeholders. Omit any line that does not apply
> to your environment, but do **not** skip the command entirely.

```bash
az functionapp config appsettings set --name <app> --resource-group <rg> --settings \
  FUNCTIONS_WORKER_RUNTIME=dotnet-isolated \
  WAREWOLF_LICENSE_CHECK_ENABLED=true \
  AZURE_KEYVAULT_NAME=<vault-name> \
  KEYVAULT_SECRET_NAME=<secret-name> \
  WAREWOLF_ENTRA_TENANT_ID=<tenant-guid> \
  WAREWOLF_ENTRA_AUDIENCE=api://<client-id> \
  ENABLEAPPLICATIONINSIGHTS=true \
  EXECUTIONLOGLEVEL=4
# Then enforce HTTPS:
az functionapp update --name <app> --resource-group <rg> --set httpsOnly=true
```

Notes on the lines above:
- `WAREWOLF_LICENSE_CHECK_ENABLED` — leave `true` for prod (default); set `false` only for isolated tests (§4).
- `AZURE_KEYVAULT_NAME` / `KEYVAULT_SECRET_NAME` — required whenever any `.bite` carries `WFAES::` secrets; prompt for both (§6.4). Leave unset only when no encryption is used.
- `WAREWOLF_ENTRA_*` — required to validate JWTs on `/Secure/*` (§5.5); omit for anonymous/public-only deployments.
- `WAREWOLF_SECURE_CONFIG` — set **only** when `secure.config` is mounted at a non-default path; when it ships in the package (bin dir) leave it unset (§5).
- After setting, the app **restarts**; re-run the §7 verification suite.

The full set of recognised settings:

**Logging (§6)**

| Setting | Values | Default | Effect |
|---|---|---|---|
| `ENABLEAPPLICATIONINSIGHTS` | `true`/`false` | false | Adds the rich **AzureExecutionLogger** → Application Insights sink (`ENABLECONSOLELOGGING` accepted as a backward-compat alias). Console / Azure **Log Stream** is **always on** regardless. |
| `ENABLEELASTICSEARCHLOGGING` | `true`/`false` | false | Elasticsearch execution sink (also needs the `.bite` file) |
| `EXECUTIONLOGLEVEL` | `0`–`6` or name | `4` (INFO) | Min level for **all** execution sinks (Console/Log Stream, App Insights, Elasticsearch); `5`/`6` send Debug/Trace to App Insights too |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | conn string | — | App Insights export |
| `Elasticsearch__Uri` / `__IndexName` / `__Username` / `__Password` / `__ApiKey` | strings | from `.bite` | Override the `.bite` Elasticsearch source |

**Licensing (§4)**

| Setting | Values | Default | Effect |
|---|---|---|---|
| `WAREWOLF_LICENSE_CHECK_ENABLED` | `true`/`false`/`0` | `true` | When true, every execution requires `IsLicensed` (Active/InTrial) |

**Security & Entra (§5)**

| Setting | Values | Effect |
|---|---|---|
| `WAREWOLF_SECURE_CONFIG` | file path | Load `secure.config` from a mounted path instead of the bin dir |
| `WAREWOLF_ENTRA_TENANT_ID` | tenant GUID | Validate JWT issuer/tenant |
| `WAREWOLF_ENTRA_AUDIENCE` | `api://<clientId>` | Expected `aud` claim |
| `WAREWOLF_ENTRA_CLIENT_ID` | client GUID | Optional alternative audience |
| `WAREWOLF_SUPER_ADMIN_ENABLED` | `true`/`false` | Super-admin bypass (see `WorkflowAuthPolicyLoader`) |
| `BYPASS_SECURE_CONFIG` | `true`/`false` | **Dev only.** Open-access mode when `secure.config` is not effective. ⚠️ Note the name is `BYPASS_SECURE_CONFIG`, **not** `WAREWOLF_BYPASS_SECURE_CONFIG` |
| `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` | client secret | Easy Auth provider secret |

**Encryption / Key Vault (§6.4)**

| Setting | Values | Default | Effect |
|---|---|---|---|
| `AZURE_KEYVAULT_NAME` | vault name | — | Enables AES decrypt of `WFAES::` connection strings (incl. the Elasticsearch `.bite`) |
| `KEYVAULT_SECRET_NAME` | secret name | `dp-keyring-v1` | Key-ring secret inside the vault |
| `AZURE_CLIENT_ID` / `AZURE_TENANT_ID` | GUIDs | — | Managed-identity selection for `DefaultAzureCredential` |
| `WorkflowsDirectory` | path | `{BaseDir}/Resources` | Where `.bite` workflows are loaded from |

**Platform**

| Setting | Value |
|---|---|
| `FUNCTIONS_WORKER_RUNTIME` | `dotnet-isolated` |
| `AzureWebJobsStorage` | storage connection (set by `az functionapp create`) |

---

## 4. Licensing — `Warewolf License.secureconfig`

### 4.1 How it works

- `app.config` wires the section:
  `<subscriptionSettings configSource="Warewolf License.secureconfig" />`.
- On startup `SubscriptionConfig` loads that file via `ConfigurationManager`. It must sit in
  the **application base directory** (the function app root, next to the binaries).
- Each `<add key=.. value=..>` value is **AES-256-CBC encrypted** (same algorithm as
  `Dev2.Infrastructure/SecurityEncryption.cs`). Plain-text values are detected and
  re-encrypted in place on load.
- `IsLicensed == true` only when `Status` is **`Active`** or **`InTrial`**.
- **If the file is missing**, `SubscriptionConfig` *creates a default* (live key/site, default
  status) — which is **not** a paid/active subscription, so executions are blocked when the
  license gate is on.
- The execution gate lives in `WorkflowExecutor`:
  `WAREWOLF_LICENSE_CHECK_ENABLED` defaults to **enabled**. When enabled and the instance is
  not licensed, every workflow returns
  *"Execution blocked: a valid Warewolf license/subscription is required."*

### 4.2 Two ways to license

1. **File deploy (this section).** Ship an encrypted `Warewolf License.secureconfig` in the
   package. Best for CI/CD and headless deploys.
2. **UI / Chargebee registration.** The Angular studio calls `LicensingHttpFunction`:
   - `GET /IsLicensed` (anonymous) — fast local status, no Chargebee call.
   - `GET /Subscriptions` (function-key) — refresh from Chargebee.
   - `POST /secure/Subscriptions` (JWT, **Administrator** group) — create/link a subscription.
   The Chargebee **test vs live** site is chosen at compile time by the `#if DEBUG` branch in
   `SubscriptionConfig`/`SubscriptionProvider` — **Release builds target the live site.**

### 4.3 Encrypt the license file before deploying

Use [`Protect-LicenseConfig.ps1`](./Protect-LicenseConfig.ps1):

```powershell
# Inspect current values without changing the file (stdout only)
./Protect-LicenseConfig.ps1 -Decrypt -OutPath -

# Encrypt every non-empty value in place, ready to deploy
./Protect-LicenseConfig.ps1 -Path "Warewolf License.secureconfig" -Encrypt
```

> ⚠️ Running `-Encrypt` twice **double-encrypts**. If unsure of state, `-Decrypt` first.
> Empty values (e.g. an unset `CustomerId`) are left unchanged.

### 4.4 Licensing decision checklist

- [ ] Production: encrypted `Warewolf License.secureconfig` present in the package **or** the
      operator will register via the UI post-deploy.
- [ ] `WAREWOLF_LICENSE_CHECK_ENABLED` left **unset/true** for production (set `false` only for
      isolated testing — `local.settings.json` uses `false` for dev).
- [ ] Release build is intended → confirm Chargebee **live** site is correct.

---

## 5. Security — `secure.config` (authorization)

### 5.1 What it is

`secure.config` is a JSON-serialized `Dev2.Services.Security.SecuritySettingsTO`. It defines
**who can do what** per workflow and server-wide. It is loaded by `SecureConfigLoader` from the
bin directory, or from the path in `WAREWOLF_SECURE_CONFIG`. Changes are picked up live by
`SecureConfigWatcher`. The file may itself be encrypted.

### 5.2 ⚠️ Critical behavior — no config means *deny*, not *allow*

The `Deploy-ToAzure.ps1` console message ("open-access mode, all workflows public") is
**misleading**. The authorization code (`WorkflowAuthPolicyLoader` / `WorkflowPolicyMatcher` /
`WorkflowAuthorizationMiddleware`) does the opposite:

> When `secure.config` is **not effective** (missing/empty) and `BYPASS_SECURE_CONFIG` is
> **not** `true`, the loader returns **`PolicyLookupResult.Failed` → HTTP 503** and *all*
> requests are denied.

So for a working deployment you must do **one** of:
- ✅ Deploy a valid `secure.config` (recommended for any real environment), **or**
- ✅ Set `BYPASS_SECURE_CONFIG=true` (open-access; **dev/test only**, never production).

> **Agent directive — never deploy without resolving `secure.config` first.**
> A missing `secure.config` is almost always an oversight, not a deliberate choice: the result
> is a server that denies **every** request (503). If you are automating a deployment and there
> is no `secure.config` next to the package, **stop and resolve it in this order before
> continuing** — do not silently proceed:
>
> 1. **Reuse an existing one.** On any machine that runs Warewolf Studio/Server, security
>    settings are saved to `%ProgramData%\Warewolf\Server Settings\secure.config`
>    (`EnvironmentVariables.ServerSecuritySettingsFile`). Copy it into the deploy folder:
>    ```powershell
>    $src = Join-Path $env:ProgramData 'Warewolf\Server Settings\secure.config'
>    if (Test-Path $src) { Copy-Item $src .\secure.config -Force }
>    ```
>    If it is not on this machine, **ask the user** whether their team already maintains a
>    `secure.config` (e.g. from an existing Warewolf Server, a colleague's machine, or source
>    control / a secrets store) and have them supply it before falling back to step 2.
> 2. **Generate one from the user's intent.** If none exists, ask the user which workflows must
>    be **public**, which require **authentication**, and which groups/roles map to each — then
>    write a minimal `secure.config` using the structure in §5.3 (lock `Public` down
>    server-wide; grant only the named resources `View`+`Execute`). Show it back to the user
>    before deploying.
> 3. **Bypass only for throwaway dev/test.** As a last resort set `BYPASS_SECURE_CONFIG=true`
>    (open-access). Confirm this explicitly with the user; **never** use it for production.
>
> Treat "no `secure.config` and no bypass" as a **blocking** condition, not a warning.

### 5.3 Structure (key fields)

```json
{
  "SecretKey": "",                                  // HMAC key for locally-issued JWTs (LoginFunction)
  "AuthenticationOverrideWorkflow": { "Name": "", "Value": "00000000-0000-0000-0000-000000000000" },
  "WindowsGroupPermissions": [
    { "WindowsGroup": "Warewolf.Administrators", "IsServer": true,
      "View": true, "Execute": true, "Contribute": true,
      "DeployTo": true, "DeployFrom": true, "Administrator": true },
    { "WindowsGroup": "Public", "IsServer": true,
      "View": false, "Execute": false },             // server-wide Public = locked down
    { "WindowsGroup": "Public", "IsServer": false,   // per-resource grant
      "ResourceName": "Hello World", "View": true, "Execute": true }
  ],
  "CacheTimeout": "01:00:00"
}
```

- **`IsServer: true`** → the permission applies server-wide; **`false`** → scoped to one
  workflow identified by `ResourceName` (and/or `ResourceID`/`ResourcePath`).
- **Permission flags:** `View`, `Execute`, `Contribute`, `DeployTo`, `DeployFrom`,
  `Administrator`. Execution needs at least `View` **and** `Execute`.
- **Groups** are matched against the caller's `roles` claim (Entra app roles / group names).
- **`WindowsGroup` naming (Entra ID):** when the group is backed by an Entra **app role**, the
  role `value` must match `^[\w.:-]+$` — i.e. **alphanumeric plus `.` `_` `-` `:` only, no
  spaces** (Entra rejects spaces/punctuation with *"Entitlement ClaimValue contains invalid
  characters"*). The provisioning scripts sanitise automatically, replacing each run of
  disallowed characters with `_` (`Warewolf Administrators` → `Warewolf_Administrators`), so the
  value that arrives in the `roles` claim has **no spaces**. The `WindowsGroup` in `secure.config`
  must match that sanitised value **exactly** — use e.g. `Warewolf.Administrators` or
  `Warewolf_Administrators`, **not** `Warewolf Administrators`. (`Public` is a local pseudo-group,
  not an Entra role, so it is exempt.)

> A common integration failure is HTTP 500
> `resolved permissions [Execute] do not satisfy required [View, Execute]` — the `Public`
> group entry is missing `View: true`. Grant both.

### 5.4 Route → auth mapping

| Route | Auth required | Notes |
|---|---|---|
| `/Public/{workflow}.json` | none | still subject to `secure.config` (`Public` group must grant the resource) |
| `/Secure/{workflow}.json` | JWT Bearer / Easy Auth | group membership + permission flags enforced |
| `/Services/{workflow}.json` | function key | machine-to-machine |
| `/apis.json` | controlled (Secure/Public) | subject to `secure.config` |

### 5.5 Entra ID (Easy Auth) for `/Secure/*`

Browser users authenticate via Azure Easy Auth (redirect → `X-MS-CLIENT-PRINCIPAL`); API
clients send `Authorization: Bearer <jwt>` validated locally against OIDC metadata derived from
`WAREWOLF_ENTRA_TENANT_ID` / `WAREWOLF_ENTRA_AUDIENCE`. Provision both the API and clients with:

```powershell
./Scripts/Configure-WwExecutionAuth.ps1 -SubscriptionId <sub> -TenantId <tenant> `
    -ResourceGroupName <rg> -FunctionAppName <app> -NonInteractive -SkipSmokeTest
# then, using the emitted ClientId:
./Scripts/Configure-WwExecutionAuth-Clients.ps1 -ResourceAppId <clientId> -TenantId <tenant>
```

Full walk-throughs: [`docs/README-Authentication.md`](./docs/README-Authentication.md),
[`docs/AzureProvisioning-FunctionApp.md`](./docs/AzureProvisioning-FunctionApp.md),
[`docs/AzureProvisioning-ClientApps.md`](./docs/AzureProvisioning-ClientApps.md),
[`Auth/AUTHORIZATION_FLOW.md`](./Auth/AUTHORIZATION_FLOW.md).

### 5.6 Security checklist

- [ ] `secure.config` deployed **or** `BYPASS_SECURE_CONFIG=true` (dev only) — otherwise 503.
- [ ] `Public` group locked down server-wide; only intended workflows granted per-resource.
- [ ] `WAREWOLF_ENTRA_TENANT_ID` + `WAREWOLF_ENTRA_AUDIENCE` set and `aud` matches the token.
- [ ] `httpsOnly=true`; secrets (`MICROSOFT_PROVIDER_AUTHENTICATION_SECRET`) stored as app
      settings / Key Vault references, never in source.
- [ ] `BYPASS_SECURE_CONFIG` and `WAREWOLF_SUPER_ADMIN_ENABLED` **not** set in production.

---

## 6. Logging

### 6.1 Architecture (two pipelines)

```
Execution loggers (composite — each sink added by AddExecutionLogging):
  Dev2Logger → Dev2LoggerSinkAdapter → CompositeExecutionLogger
        ├── ConsoleExecutionLogger       → MEL → stdout → Azure Log Stream + App Insights traces   (ALWAYS on)
        ├── AzureExecutionLogger         → MEL → Application Insights (rich telemetry)   (ENABLEAPPLICATIONINSIGHTS=true)
        ├── ElasticsearchExecutionLogger → direct HTTP → Elasticsearch index            (ENABLEELASTICSEARCHLOGGING=true)
        └── AuditExecutionLogger         → MEL → security audit events                  (ALWAYS on)

Infrastructure loggers (MEL only — controlled by host.json):
  AuditLogger, InstanceCorrelationMiddleware, StartupOrchestrator
```

Two filter gates apply to every entry:
- **Gate 1 — `EXECUTIONLOGLEVEL`** (`ExecutionLoggerBase.ShouldLog`): gates **all** execution
  sinks. `DEBUG` (5) / `TRACE` (6) let Debug/Trace through to the Log Stream **and** App Insights.
- **Gate 2 — `host.json` `logging.logLevel`** (MEL category filters): affects only MEL sinks
  (Console/Log Stream, App Insights, Audit, middleware). **Elasticsearch bypasses MEL**, so
  `host.json` does **not** throttle it.

### 6.2 The three knobs

| Variable | Values | Default | Effect |
|---|---|---|---|
| `ENABLEAPPLICATIONINSIGHTS` | `true`/`false` | false | Adds the rich **AzureExecutionLogger** → Application Insights sink. (`ENABLECONSOLELOGGING` is accepted as a backward-compat alias.) The console / Azure **Log Stream** sink is **always on** regardless of this flag. |
| `ENABLEELASTICSEARCHLOGGING` | `true`/`false` | false | Elasticsearch logging — **also requires** `Settings/ElasticsearchLoggingSource.bite` to exist |
| `EXECUTIONLOGLEVEL` | `0`–`6` / name | `4` (INFO) | `0 OFF · 1 FATAL · 2 ERROR · 3 WARN · 4 INFO · 5 DEBUG · 6 TRACE` — gates **every** sink, including App Insights and the Log Stream |

```bash
# Elasticsearch + App Insights at INFO (typical prod)
az functionapp config appsettings set --name <app> --resource-group <rg> \
  --settings ENABLEELASTICSEARCHLOGGING=true ENABLEAPPLICATIONINSIGHTS=true EXECUTIONLOGLEVEL=4
# Live console (Log Stream) — always on; tail it with:
az webapp log tail --name <app> --resource-group <rg>
```

#### Debug & Trace in Application Insights and the Live Log Stream (WOLF-8436)

Setting `EXECUTIONLOGLEVEL=DEBUG` (5) or `TRACE` (6) lowers the gate so Debug/Trace execution
entries flow all the way to **Application Insights** and the Azure Portal **Live Log Stream**, not
just stdout. Two things to know when reading them in App Insights:

- Both **Trace** and **Debug** map to App Insights **`severityLevel` 0**, so filter by message,
  not severity, when you are after that level. Full mapping:

  | `EXECUTIONLOGLEVEL` | Dev2 value | MEL `LogLevel` | App Insights `severityLevel` |
  |---|---|---|---|
  | `TRACE` (6) | TRACE | Trace (0) | 0 |
  | `DEBUG` (5) | DEBUG | Debug (1) | 0 |
  | `INFO` (4)  | INFO  | Information (2) | 1 |
  | `WARN` (3)  | WARN  | Warning (3) | 2 |
  | `ERROR` (2) | ERROR | Error (4) | 3 |
  | `FATAL` (1) | FATAL | Critical (5) | 4 |

- Query the App Insights **`traces`** table (Debug/Trace included once the level gate is lowered):

  ```kusto
  traces
  | where message contains "[ExecutionId:"
  | project timestamp, severityLevel, message
  | order by timestamp desc
  ```

`DEBUG`/`TRACE` is verbose — raise `EXECUTIONLOGLEVEL` back to `INFO` (4) for steady-state prod to
keep App Insights ingestion cost down.

### 6.3 The Elasticsearch sink — `Settings/ElasticsearchLoggingSource.bite`

This file *is* the Elasticsearch logging source. It is a Warewolf `ElasticsearchSource`
resource whose `ConnectionString` is **AES-encrypted** in the `WFAES::...` format, e.g.:

```xml
<Source ID="f1dc0c3f-..." Name="ElasticsearchLoggingSource" ResourceType="ElasticsearchSource"
        ConnectionString="WFAES::3aKwZho6y8ef..." Type="ElasticsearchSource">
  <DisplayName>ElasticsearchLoggingSource</DisplayName>
</Source>
```

Behavior:
- The file ships with the build (`CopyToOutputDirectory=Always`) and is checked with
  `File.Exists` before the sink is created — if it is absent, Elasticsearch logging is
  silently skipped even when the flag is `true`.
- The `WFAES::` connection string is decrypted at runtime by the **Key Vault AES hook** wired in
  `StartupOrchestrator` (step 4 of §1.1). This requires `AZURE_KEYVAULT_NAME` (+ managed
  identity / `KEYVAULT_SECRET_NAME`). See §6.4.
- To point at your **own** cluster: open the source in the Warewolf Studio and re-save it, or
  re-encrypt the connection string with [`Scripts/Encrypt-Config.ps1`](./Scripts/Encrypt-Config.ps1)
  using your Key Vault key, **or** override per-field at runtime with the
  `Elasticsearch__Uri` / `Elasticsearch__IndexName` / `Elasticsearch__Username` /
  `Elasticsearch__Password` / `Elasticsearch__ApiKey` app settings (`ElasticsearchLoggingOptions`).

### 6.4 Connection-string encryption / Key Vault

`.bite` connection strings (DB sources *and* the Elasticsearch logging source) use
**AES-256-GCM** keyed by an Azure Key Vault secret, because DPAPI is unavailable in Azure.
Format: `WFAES::{Base64( [12-byte nonce][ciphertext][16-byte GCM tag] )}`.

- One-time infra: [`Scripts/KeyVaultSetup.azcli`](./Scripts/KeyVaultSetup.azcli) creates the
  vault, enables managed identity on the Function App, and assigns minimal RBAC.
- Encrypt/upload sources: [`Scripts/Encrypt-Config.ps1`](./Scripts/Encrypt-Config.ps1).
- Set `AZURE_KEYVAULT_NAME` (and optionally `KEYVAULT_SECRET_NAME`, default `dp-keyring-v1`).
- Leave `AZURE_KEYVAULT_NAME` unset locally to skip Key Vault entirely.

> **Workflows must be encrypted before deployment.** The `.bite` files in `Resources/`
> are *workflows*, and they routinely carry sensitive material — embedded source
> `ConnectionString`s, credentials, API keys, and hard-coded values inside the workflow
> XAML. Treat every workflow `.bite` as needing encryption: run
> [`Scripts/Encrypt-Config.ps1`](./Scripts/Encrypt-Config.ps1) over the whole `Resources/`
> tree (and `Settings/`) so every secret is stored as `WFAES::…`, and never ship a workflow
> whose secrets are still in clear text.

> **Agent directive — always prompt for the Key Vault name and secret name.**
> Before encrypting, decrypting, or wiring runtime decryption, **stop and ask the operator
> for the two Key Vault identifiers** (do not guess or hard-code them):
> 1. **Key Vault name** → used for `-VaultName` and the `AZURE_KEYVAULT_NAME` app setting
>    (vault name only, e.g. `kv-warewolf-prod`, **not** a full URI).
> 2. **Secret name** → used for `-SecretName` and the `KEYVAULT_SECRET_NAME` app setting
>    (the key-ring secret; default `dp-keyring-v1` — offer it as the default but still confirm).
>
> Use the supplied values consistently for `Encrypt-Config.ps1`, the Function App settings,
> and any verification step, so the key used to encrypt matches the key the runtime fetches.

> ⚠️ **Verify encryption state before packaging — the failure modes are asymmetric and easy to miss.**
> The runtime only AES-decrypts values carrying the `WFAES::` prefix. Any other state slips through:
> - **Plain-text** connection strings pass through **unchanged and the workflow still runs** — so
>   nothing errors, but the secret ships in clear text inside the `.bite`. There is **no** runtime
>   warning, so neither the deploy script nor an agent will catch this on its own.
> - **DPAPI-encrypted** values (e.g. copied from a full Warewolf Server) are **not** valid in Azure —
>   the DPAPI keys don't exist there, so the first use of that source throws `CryptographicException`.
>
> Always run [`Scripts/Encrypt-Config.ps1`](./Scripts/Encrypt-Config.ps1) over your `Resources/` and
> `Settings/` `.bite` files, then confirm **every** `ConnectionString` begins with `WFAES::`:
> ```powershell
> # Any match is an unencrypted (plain-text or DPAPI) connection string — re-encrypt it.
> Select-String -Path .\publish\Resources\*.bite, .\publish\Settings\*.bite `
>   -Pattern 'ConnectionString="(?!WFAES::)[^"]+"'
> ```

Details: [`docs/README-Encryption.md`](./docs/README-Encryption.md),
[`docs/KeyRotationRunbook.md`](./docs/KeyRotationRunbook.md).

### 6.5 Instance correlation & infrastructure logs

`InstanceCorrelationMiddleware` stamps each request with `WEBSITE_INSTANCE_ID` (or `local-env`)
so logs from a specific Azure Function instance can be correlated. `AuditLogger`
emits security events through MEL → tune via `host.json` category
`Warewolf.Execution.Lightweight.Security.AuditLogger`.

### 6.6 Logging verification

| Sink | How to confirm it is emitting |
|---|---|
| Console / App Insights | `az webapp log tail ...` shows `AzureExecutionLogger` lines; App Insights `traces` table populated |
| Elasticsearch | target index receives documents after a workflow run (check the `.bite` cluster) |
| Audit | security events appear under the `AuditLogger` category |

Full reference: [`docs/Warewolf-Lightweight-Logger-Guide.md`](./docs/Warewolf-Lightweight-Logger-Guide.md)
(enable/disable + verification, all environments).

---

## 7. End-to-end verification

After deploy + app settings + auth, prove each subsystem:

```bash
BASE="https://<app>.azurewebsites.net"

# 1. Host is up (any HTTP response means the runtime is ready)
curl -i "$BASE/apis.json"

# 2. Licensing — local status
curl -s "$BASE/IsLicensed"        # → {"isLicensed":true,"status":"Active",...}

# 3. Security — Public route (granted in secure.config)
curl -s "$BASE/Public/Hello%20World.json?Name=Anon"   # → Hello Anon.

# 4. Security — Secure route requires a token (expect 401 without one)
curl -i  "$BASE/Secure/Hello%20World.json?Name=Auth"
curl -s  "$BASE/Secure/Hello%20World.json?Name=Auth" -H "Authorization: Bearer $TOKEN"

# 5. Logging — run a workflow, then:
az webapp log tail --name <app> --resource-group <rg>   # console
# and check the Elasticsearch index / App Insights traces
```

A fully healthy deployment: step 1 responds, step 2 shows `isLicensed:true`, step 3 returns the
workflow output, step 4 is 401 without a token and 200 with one, and step 5 shows fresh log
entries in the enabled sinks.

---

## 8. Troubleshooting matrix

| Symptom | Likely cause | Fix |
|---|---|---|
| **Every request → HTTP 503** | `secure.config` missing/empty and `BYPASS_SECURE_CONFIG` not set | Deploy a valid `secure.config`, or set `BYPASS_SECURE_CONFIG=true` (dev only) — §5.2 |
| **Executions blocked: "valid Warewolf license required"** | Instance not licensed | Deploy encrypted `Warewolf License.secureconfig` or register via UI; for isolated tests `WAREWOLF_LICENSE_CHECK_ENABLED=false` — §4 |
| **401 on `/Secure/*` (no/invalid token)** | Missing, expired or invalid token — includes `WAREWOLF_ENTRA_AUDIENCE`/`WAREWOLF_ENTRA_TENANT_ID` ≠ token `aud`/`tid`, which fails validation so the caller is unauthenticated | Send a valid token; align the audience/tenant — §3.4 / §5.5 |
| **HTTP 500 with a valid token** (`Invalid Authentication Token or invalid permissions to Execute resource`, or `resolved permissions [Execute] do not satisfy required [View, Execute]`) | Caller's group not granted the required `View`/`Execute` for that workflow (e.g. `Public` group missing `View:true`) | Grant the group the needed permissions in `secure.config` — §5.3. (The authorization middleware returns **500** for this today; a `TODO` in `WorkflowAuthorizationMiddleware` tracks changing it to **403**.) |
| **No Elasticsearch logs** | flag off, `.bite` missing, or Key Vault decrypt failing | Set `ENABLEELASTICSEARCHLOGGING=true`, ensure `Settings/ElasticsearchLoggingSource.bite` is deployed, set `AZURE_KEYVAULT_NAME` + managed identity — §6.3/§6.4 |
| **No console / Log Stream logs in `az webapp log tail`** | `EXECUTIONLOGLEVEL=0` (OFF), a `host.json` category filter, or App Service Log Stream not enabled (console sink itself is always-on) | Raise `EXECUTIONLOGLEVEL`; check `host.json` category level; enable App Service logs — §6.1/§6.2 |
| **No Debug/Trace in App Insights / Live Log Stream** | `EXECUTIONLOGLEVEL` above `DEBUG`/`TRACE`, or `ENABLEAPPLICATIONINSIGHTS` off (no rich AI sink) | Set `EXECUTIONLOGLEVEL=DEBUG` (5) or `TRACE` (6); set `ENABLEAPPLICATIONINSIGHTS=true` for the AI sink. Trace/Debug land under `severityLevel` 0 — §6.2 |
| **`CryptographicException` decrypting a `.bite`** | Wrong/rotated Key Vault key, **or** a DPAPI-encrypted value copied from a full Warewolf Server (DPAPI can't be decrypted in Azure) | Re-encrypt the source with `Scripts/Encrypt-Config.ps1` using the current Key Vault key — §6.4; for rotation see KeyRotationRunbook |
| **A source connects using a plain-text secret (no error at all)** | A `.bite` `ConnectionString` shipped as plain text — values without the `WFAES::` prefix pass through unencrypted and **still work**, so nothing fails while the secret sits in clear text | Re-encrypt with `Scripts/Encrypt-Config.ps1`; verify every `ConnectionString` starts with `WFAES::` before packaging (see the `Select-String` check in §6.4) |
| **Deploy script errors: "Resources folder not found"** | no `Resources/` next to the script | Create `Resources/` and add `.bite` files before deploying — §3.1 |
| **`func`/`az` not found or not logged in** | tooling/login | Install Azure CLI / Core Tools; `az login` — §2 |

---

## 9. Pre-deployment checklist (run through this before every prod deploy)

1. [ ] `dotnet publish -c Release -o ./publish` succeeded.
2. [ ] `./publish/Resources/` contains the intended `.bite` workflows.
3. [ ] `./publish/Settings/ElasticsearchLoggingSource.bite` present (and points at your cluster).
4. [ ] `Warewolf License.secureconfig` present and **encrypted** (`Protect-LicenseConfig.ps1 -Encrypt`), or a UI-registration plan exists.
5. [ ] `secure.config` present and reviewed (Public locked down; intended grants only). **If missing, STOP and resolve per §5.2** (reuse from `%ProgramData%\Warewolf\Server Settings`, or generate from user intent) — do not deploy a deny-all (503) service.
6. [ ] Key Vault provisioned; managed identity has access. **Prompt the operator for the Key Vault name and secret name** (§6.4), then encrypt **every workflow `.bite` in `Resources/`** (and `Settings/`) with that key — **every** `ConnectionString` begins with `WFAES::`, with **no** plain-text or DPAPI values (verify with the `Select-String` check in §6.4). Workflows must not ship with clear-text secrets.
7. [ ] Run `Deploy-ToAzure.ps1 -AppName <unique> [-ResourceGroup ... -Location ...]`.
8. [ ] **Set application settings (§3.4) — MANDATORY:** run the concrete `az functionapp config appsettings set` command (logging, licensing, Entra, Key Vault). `Deploy-ToAzure.ps1` sets **none** of these; skipping this leaves the instance with **zero** environment variables. Confirm afterwards with `az functionapp config appsettings list --name <app> --resource-group <rg>`.
9. [ ] `az functionapp update --set httpsOnly=true`.
10. [ ] Configure Entra Easy Auth + client apps (§5.5).
11. [ ] Run the §7 verification suite; confirm licensing, security, and logging all green.
12. [ ] Confirm `BYPASS_SECURE_CONFIG` / `WAREWOLF_SUPER_ADMIN_ENABLED` are **not** set in prod.

---

## 10. Reference index

> **Working from a release zip?** This `Skill.md`, `Deploy-ToAzure.ps1`,
> `Protect-LicenseConfig.ps1`, `secure.config`, and `Settings/*.bite` ship **inside** the
> release publish output, so the links to them below resolve from the zip. The `docs/...`
> deep-dive guides below live in the **source repository** and are *not* bundled in a release
> zip — open them at `Dev/Warewolf.Execution.Lightweight/docs/` in source control if you need
> the long-form walk-through. Everything required to complete a deployment is contained in this
> `Skill.md` itself; the `docs/` links are supplementary.

| Topic | Document |
|---|---|
| Authentication & client tokens | [`docs/README-Authentication.md`](./docs/README-Authentication.md) |
| Function App / Entra provisioning | [`docs/AzureProvisioning-FunctionApp.md`](./docs/AzureProvisioning-FunctionApp.md) |
| Client app registrations | [`docs/AzureProvisioning-ClientApps.md`](./docs/AzureProvisioning-ClientApps.md) |
| Authorization flow internals | [`Auth/AUTHORIZATION_FLOW.md`](./Auth/AUTHORIZATION_FLOW.md) |
| Connection-string encryption | [`docs/README-Encryption.md`](./docs/README-Encryption.md) |
| Key rotation | [`docs/KeyRotationRunbook.md`](./docs/KeyRotationRunbook.md) |
| Security checklist | [`docs/SecurityChecklist.md`](./docs/SecurityChecklist.md) |
| Logging enable/disable & verification | [`docs/Warewolf-Lightweight-Logger-Guide.md`](./docs/Warewolf-Lightweight-Logger-Guide.md) |
| Application Insights | [`docs/README-ApplicationInsights.md`](./docs/README-ApplicationInsights.md) |
| License encryption script | [`Protect-LicenseConfig.ps1`](./Protect-LicenseConfig.ps1) |
| Deploy script | [`Deploy-ToAzure.ps1`](./Deploy-ToAzure.ps1) |
