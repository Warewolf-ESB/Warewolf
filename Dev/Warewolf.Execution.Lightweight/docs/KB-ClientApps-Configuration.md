# KB — Configuring each client-example app for the wwexecution Execution Engine

How every client-example app under
[`Warewolf.Execution.Lightweight.ClientExamples`](../../Warewolf.Execution.Lightweight.ClientExamples)
is registered, configured, and how it acquires and passes a token to call
`/secure/{workflow}.json` on the **wwexecution** Azure Function App.

This document is the companion to the provisioning orchestrator
[`Configure-WwExecutionAuth-ClientApps.ps1`](../Scripts/Configure-WwExecutionAuth-ClientApps.ps1),
which creates one Entra ID registration per app by driving
[`Configure-WwExecutionAuth-Clients.ps1`](../Scripts/Configure-WwExecutionAuth-Clients.ps1).
The engine resource app + Easy Auth are provisioned separately by
[`Configure-WwExecutionAuth.ps1`](../Scripts/Configure-WwExecutionAuth.ps1).

---

## 0. The big picture

```
   ┌────────────┐  1. acquire token (per flow)   ┌──────────────────────────┐
   │ Client app │ ─────────────────────────────► │  Microsoft Entra ID      │
   │            │ ◄──────── access_token ──────── │  login.microsoftonline   │
   │            │                                 └──────────────────────────┘
   │            │  2. GET /secure/{workflow}.json
   │            │     Authorization: Bearer <token>
   │            │ ─────────────────────────────► ┌──────────────────────────┐
   │            │ ◄──────── JSON result ───────── │  wwexecution Function App│
   └────────────┘                                 │  (Easy Auth validates aud)│
                                                   └──────────────────────────┘
```

Two things must both be true for a `/secure/*` call to succeed:

1. **Authentication** — the access token's audience (`aud`) is `api://<ResourceAppId>`.
   Easy Auth validates this; a wrong audience → **HTTP 401**.
2. **Authorization** — the caller carries a role/scope the engine accepts for the
   workflow. A roleless app-only caller, or a user with no assignment, is **rejected**.
   Denials are wrapped as **HTTP 500** (not 403) pending WOLF-8418.

For **app-only callers** (the AzureFunction / ServiceBus daemons and the Console
client-credentials flow) authorization is a two-part contract:

- **Token side** — the client's service principal / Managed Identity holds a group app
  role on the resource app (assigned via `-AppRolesToAssign`). The role values are the
  `GroupPermissions` keys from the deploy auth config
  ([`Deploy-WwExecutionEngine.authconfig.example.json`](../Scripts/Deploy-WwExecutionEngine.authconfig.example.json)),
  which ships a dedicated **`Warewolf_ClientApps`** group for these callers.
- **Policy side** — the engine's `secure.config` must contain a `WindowsGroupPermissions`
  row with `WindowsGroup` equal to the app-role value (per-workflow, `IsServer=false`,
  `Execute=true`) for every workflow the client calls. Permissions are resolved from
  `secure.config` at request time, never from the token.

| Route | Auth | Header(s) |
|---|---|---|
| `GET /public/{workflow}.json` | none | — |
| `GET\|POST /secure/{workflow}.json` | yes | `Authorization: Bearer <token>` |
| `GET\|POST /services/{workflow}.json` | yes | `Authorization: Bearer <token>` **and** `x-functions-key: <key>` |

| Scope | Used by | Token kind |
|---|---|---|
| `api://<ResourceAppId>/user_impersonation` | SPA, Web MVC, Console (interactive) | delegated (`scp`) |
| `api://<ResourceAppId>/.default` | Console (client-creds), Daemon, Service Bus | app-only (`roles`) |

---

## 1. Provision everything

```powershell
az login

# All six example apps, then validate the app-only flows automatically.
./Scripts/Configure-WwExecutionAuth-ClientApps.ps1 `
    -ResourceAppId   <resourceAppId> `
    -TenantId        <tenantId> `
    -FunctionAppName <wwexecution> -FunctionAppResourceGroup <rg> `
    -AppRolesToAssign Warewolf_ClientApps `
    -Validate -NonInteractive
```

Outputs (written next to the script):

| File | Contents |
|---|---|
| `Configure-WwExecutionAuth-ClientApps.<app>.output.json` | Per-app `ClientId`, `ClientSecret`, roles, scope, authority. |
| `Configure-WwExecutionAuth-ClientApps.summary.json` | Aggregate, **secrets masked**, with per-app status + validation result. |
| `logs/Configure-WwExecutionAuth-ClientApps-<stamp>.log` | Full transcript. |

Map the orchestrator output to each app's config:

| Output field | Goes into |
|---|---|
| `Clients[].ClientId` (per app) | the app's SPA/client id setting |
| `ResourceAppId` | the app's `ResourceAppId` / resource-app id |
| `TenantId` | the app's tenant id |
| `Scope` / `Authority` | derived: `api://<ResourceAppId>/...`, `https://login.microsoftonline.com/<TenantId>` |
| `Clients[].ClientSecret` (confidential/daemon) | the app's secret store (user-secrets / Key Vault) |

> The orchestrator **does not** write the example apps' config files. Copy the values
> from the per-app output JSON into each config as described below.

---

## 2. Per-app configuration

### 2.1 Angular 17 SPA — `Angular17/`

- **Registration:** `wwexecution-angular` · type **SPA** · redirect `http://localhost:4201` · CORS origin added to the Function App.
- **Flow:** Authorization Code + PKCE (delegated). No secret in the browser.
- **Config file:** [`src/environments/environment.ts`](../../Warewolf.Execution.Lightweight.ClientExamples/Angular17/src/environments/environment.ts)

```ts
entra: {
  tenantId:      '<TenantId>',
  spaClientId:   '<angular ClientId>',     // Clients.angular.ClientId
  resourceAppId: '<ResourceAppId>',
},
functionAppUrl: 'https://<wwexecution>.azurewebsites.net',
// redirectUri getter must equal the registered redirect (http://localhost:4201)
```

- **Token retrieval & passing:** MSAL Angular acquires the token silently and the
  `MsalInterceptor` attaches `Authorization: Bearer <token>` to every request whose
  URL matches the `protectedResourceMap` (`/secure/`, `/services/`). You never set the
  header yourself.

```ts
const protectedResourceMap = new Map([
  [`${functionAppUrl}/secure/`,   [`api://${resourceAppId}/user_impersonation`]],
  [`${functionAppUrl}/services/`, [`api://${resourceAppId}/user_impersonation`]],
]);
```

- **Run:** `ng serve` → browse `http://localhost:4201`, sign in, call `/secure/Hello World.json`.

---

### 2.2 React SPA — `React/`

- **Registration:** `wwexecution-react` · type **SPA** · redirect `http://localhost:5173` · CORS origin added.
- **Flow:** Authorization Code + PKCE (delegated).
- **Config file:** `.env.local` (copy from [`.env.example`](../../Warewolf.Execution.Lightweight.ClientExamples/React/.env.example))

```dotenv
VITE_TENANT_ID=<TenantId>
VITE_SPA_CLIENT_ID=<react ClientId>          # Clients.react.ClientId
VITE_RESOURCE_APP_ID=<ResourceAppId>
VITE_FUNCTION_APP_URL=https://<wwexecution>.azurewebsites.net
```

- **Token retrieval & passing:** `useWorkflowApi` calls `acquireTokenSilent({ scopes: [api://<ResourceAppId>/user_impersonation] })` (interactive fallback) and then `fetch(url, { headers: { Authorization: \`Bearer ${token.accessToken}\` } })`.
- **Run:** `npm install` → `npm run dev` → `http://localhost:5173`.

---

### 2.3 .NET 8 Web App (MVC) — `DotNetWebMvc/`

- **Registration:** `wwexecution-webmvc` · type **Confidential** · redirect `https://localhost:5001/signin-oidc` (+ origin + `signout-callback-oidc`) · **client secret**.
- **Flow:** Authorization Code (confidential) + On-Behalf-Of (delegated). Tokens cached server-side.
- **Config file:** [`appsettings.json`](../../Warewolf.Execution.Lightweight.ClientExamples/DotNetWebMvc/appsettings.json) (put the **secret** in user-secrets / Key Vault, not the file)

```jsonc
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "TenantId": "<TenantId>",
  "ClientId": "<webmvc ClientId>",            // Clients.webmvc.ClientId
  "ClientSecret": "<webmvc ClientSecret>",    // user-secrets: dotnet user-secrets set "AzureAd:ClientSecret" "..."
  "CallbackPath": "/signin-oidc",
  "SignedOutCallbackPath": "/signout-callback-oidc"
},
"WwExecution": {
  "BaseUrl": "https://<wwexecution>.azurewebsites.net",
  "Scopes": [ "api://<ResourceAppId>/user_impersonation" ],
  "FunctionKey": ""
}
```

- **Token retrieval & passing:** `Microsoft.Identity.Web` (`IDownstreamApi.CallApiForUserAsync("WwExecution", ...)`) silently acquires the delegated token on the signed-in user's behalf and injects `Authorization: Bearer <token>` automatically. `/services/*` additionally adds `x-functions-key`.
- **Run:** `dotnet run` → browse `https://localhost:5001`.

---

### 2.4 .NET 8 Console — `DotNetConsole/`

- **Registration:** `wwexecution-console` · type **Console** (public-desktop **and** confidential on one reg) · public redirect `http://localhost` · **client secret** + app role.
- **Flows:** device-code / interactive (delegated, `user_impersonation`) **and** client-credentials (app-only, `.default`). The client-credentials flow **requires** an app role.
- **Config file:** [`appsettings.json`](../../Warewolf.Execution.Lightweight.ClientExamples/DotNetConsole/appsettings.json)

```jsonc
"WwExecution": {
  "BaseUrl":       "https://<wwexecution>.azurewebsites.net",
  "TenantId":      "<TenantId>",
  "ResourceAppId": "<ResourceAppId>",        // audience api://<this>
  "ClientId":      "<console ClientId>",      // Clients.console.ClientId
  "ClientSecret":  "<console ClientSecret>",  // ONLY for client-credentials; user-secrets preferred
  "RedirectUri":   "http://localhost",
  "FunctionKey":   "",                         // ONLY for /services/*
  "SampleWorkflow":"Hello World"
}
```

- **Token retrieval & passing:** `TokenAcquirer` picks the flow
  (`ClientCredentialsAsync` / `DeviceCodeAsync` / `InteractiveAsync` / `ManagedIdentityAsync`,
  silent-first). `Auth/BearerTokenHandler.cs` (a `DelegatingHandler`) injects
  `Authorization: Bearer <token>` for `/secure/*` and `/services/*` and adds
  `x-functions-key` for `/services/*`. `WwExecutionService` contains no auth code.
- **Run:** `dotnet run`, pick a flow from the menu.

---

### 2.5 Azure Function caller — `AzureFunction/`

- **Registration:** `wwexecution-azurefunction` · type **Daemon**. Three options:
  - **Managed Identity — existing SP (recommended):** pass `-AzureFunctionMiObjectId <mi-sp-object-id>` so the orchestrator assigns the app role to the function's existing MI (no secret, no app registration).
  - **Managed Identity — enable on the client Function App:** pass `-AzureFunctionClientAppName <name> -AzureFunctionClientResourceGroup <rg>` (no object id). The child script runs `az functionapp identity assign` to enable the app's system-assigned MI, reads its `principalId`, and assigns the role — automating the manual "enable MI + look up principalId" step.
  - **Client secret (local/dev):** omit both; a secret-bearing daemon app is created.
- **Flow:** app-only — Managed Identity in Azure, client-secret/az-CLI fallback locally. **App role required** (roleless → rejected). When `-AppRolesToAssign` is omitted the daemon path defaults to **`Warewolf_ClientApps`** and still fails loudly if that role does not exist on the resource app.
- **Config file:** [`local.settings.json`](../../Warewolf.Execution.Lightweight.ClientExamples/AzureFunction/local.settings.json) (local dev only)

```jsonc
"Values": {
  "WwExecution:BaseUrl":       "https://<wwexecution>.azurewebsites.net",
  "WwExecution:TenantId":      "<TenantId>",
  "WwExecution:ResourceAppId": "<ResourceAppId>",
  "WwExecution:ClientId":      "",    // client-creds fallback only; prefer az login
  "WwExecution:ClientSecret":  "",    // ditto
  "AZURE_CLIENT_ID":           ""     // user-assigned MI client id in Azure (omit for system-assigned)
}
```

- **Token retrieval & passing:** `DefaultAzureCredential` acquires an app-only token for `api://<ResourceAppId>/.default`; a `DelegatingHandler` injects `Authorization: Bearer <token>`. In Azure no secret is needed — the MI must hold the app role.
- **Run (local):** `az login` → `func start`.

---

### 2.6 Azure Service Bus worker — `AzureServiceBus/`

- **Registration:** `wwexecution-servicebus` · type **Daemon** (Managed Identity recommended via `-ServiceBusMiObjectId`, secret fallback otherwise). **App role required.**
- **Flow:** app-only client-credentials / Managed Identity, scope `api://<ResourceAppId>/.default`.
- **Config file:** [`appsettings.json`](../../Warewolf.Execution.ServiceBusWorker/appsettings.json) (non-secret) + `local.settings.json` for local secrets.

```jsonc
"WwExecution": {
  "BaseUrl":       "https://<wwexecution>.azurewebsites.net",
  "TenantId":      "<TenantId>",
  "ResourceAppId": "<ResourceAppId>",
  "UseClientSecretFallback": false,
  "TokenRefreshSkewSeconds": 120
}
```

- **Token retrieval & passing:** on each queue message, `Auth/WwExecutionTokenHandler.cs` (a `DelegatingHandler`) acquires/caches an app-only token via the injected `TokenCredential` (MI in Azure, secret/az-CLI locally), injects `Authorization: Bearer <token>`, and adds `x-functions-key` for `/services/*`.
- **Run (local):** `az login` → `func start`, then enqueue `{ "workflow": "Hello World", "inputs": { "Name": "FromServiceBus" } }`.

---

## 3. Validate access (what the orchestrator's `-Validate` does)

| App type | Validation | How |
|---|---|---|
| Daemon (secret), Console (client-creds) | **automatic** | acquire `client_credentials` token for `.default`, `GET /secure/{workflow}.json`, expect **2xx** |
| Daemon (Managed Identity) | skipped | no local secret — validate from inside Azure where the MI is available |
| SPA, Console (delegated) | **interactive opt-in** | device-code flow → `GET /secure/{workflow}.json` |
| Web MVC (confidential) | manual | sign in through the web app in a browser (device-code not supported by a confidential client) |

Manual app-only check (any daemon/console with a secret):

```powershell
$tok = (Invoke-RestMethod -Method Post `
  -Uri "https://login.microsoftonline.com/<TenantId>/oauth2/v2.0/token" `
  -ContentType 'application/x-www-form-urlencoded' `
  -Body @{ grant_type='client_credentials'; client_id='<ClientId>'; client_secret='<secret>'; scope="api://<ResourceAppId>/.default" }).access_token

Invoke-RestMethod -Uri "https://<wwexecution>.azurewebsites.net/secure/Hello%20World.json?Name=Validation" `
  -Headers @{ Authorization = "Bearer $tok" }
```

Interpreting the result: **2xx** = pass · **401** = audience/scope mismatch · **403** = role/permission denied · **500** with an `Error{…}` body = engine authorization denial (roleless caller, or no View/Execute — WOLF-8418 wraps denials as 500).

---

## 4. Troubleshooting

| Problem | Fix |
|---|---|
| `401` on `/secure/*` | Token `aud` ≠ `api://<ResourceAppId>`. Check the scope and `ResourceAppId`. |
| `500` with nested `Error{…}` | Engine authz denial. App-only: assign an app role (`-AppRolesToAssign`, e.g. `Warewolf_ClientApps`) and admin-consent, **and** ensure `secure.config` has a matching `WindowsGroup` row with `Execute=true` for the workflow. Delegated: assign the user/group + consent `user_impersonation`. |
| `AADSTS7000218` (device-code/interactive) | Client reg lacks public-client flows. SPA/Console regs set `isFallbackPublicClient`; confidential web app cannot use device-code. |
| `consent_required` | Re-run an interactive flow once, or admin-consent the delegated permission for the tenant. |
| SPA `CORS`/fetch blocked | The SPA origin must be in the Function App Allowed Origins (the orchestrator adds it unless `-SkipCorsConfiguration`). |
| `DefaultAzureCredential` fails locally | `az login`, or set `AZURE_CLIENT_ID`/`AZURE_CLIENT_SECRET`. |

---

## See also

- [`Scripts/Configure-WwExecutionAuth-ClientApps.ps1`](../Scripts/Configure-WwExecutionAuth-ClientApps.ps1) — the orchestrator described here.
- [`Scripts/Configure-WwExecutionAuth-Clients.ps1`](../Scripts/Configure-WwExecutionAuth-Clients.ps1) — per-type registration script it drives.
- [`Scripts/Get-WwExecutionToken-AllFlows.ps1`](../Scripts/Get-WwExecutionToken-AllFlows.ps1) — every OAuth flow A–I in one script.
- [`ClientExamples/README.md`](../../Warewolf.Execution.Lightweight.ClientExamples/README.md) — example-app catalogue.
- [`docs/README-Authentication.md`](README-Authentication.md) — end-to-end auth architecture.
