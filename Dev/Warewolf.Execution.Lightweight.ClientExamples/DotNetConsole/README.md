# Warewolf Execution Engine — .NET console reference client

A small, self-contained **.NET 8 console application** that calls the
[Warewolf Execution Engine](../../Warewolf.Execution.Lightweight/) over HTTP. Although written in .NET, it is a *pure HTTP
client*: the patterns (acquire an Entra ID token → send `Authorization: Bearer <token>`) apply
from **any** stack — Python, Node, Java, curl, Postman.

It demonstrates:

- **Four token-acquisition flows** via MSAL.NET (`Microsoft.Identity.Client`) and `Azure.Identity`.
- **Silent, cache-first** acquisition with a **persistent** MSAL token cache (tokens survive restarts).
- **Automatic bearer-token injection** through an `HttpClient` `DelegatingHandler`.
- A **typed HTTP service** for `/public`, `/secure`, `/services` and `/apis.json`.

---

## The engine API

Base URL (configurable): `https://WWExecutionEngine.azurewebsites.net`
Protected by **Microsoft Entra ID + Azure App Service Easy Auth**.

| Route | Auth required |
|---|---|
| `GET  /public/{workflow}.json`        | None (anonymous) |
| `GET\|POST /secure/{workflow}.json`   | `Authorization: Bearer <token>` |
| `GET\|POST /services/{workflow}.json` | Bearer token **and** `x-functions-key` header |
| `GET  /apis.json`                     | Discovery (per engine config) |

Workflow names with spaces are URI-encoded by the client, e.g.
`GET /secure/Hello%20World.json?Name=Alice`.

---

## Which flow to use when

| Flow | Method on `TokenAcquirer` | Client type | Scope | Use when |
|---|---|---|---|---|
| **Client credentials** | `ClientCredentialsAsync` | Confidential (secret) | `api://{resourceAppId}/.default` | Unattended daemon / service-to-service, **no user**. |
| **Device code** | `DeviceCodeAsync` | Public | `api://{resourceAppId}/user_impersonation` | Interactive user on a **headless / CLI / SSH / container** box (no browser). |
| **Interactive** | `InteractiveAsync` | Public | `api://{resourceAppId}/user_impersonation` | Desktop with a browser; opens a sign-in popup. |
| **Managed identity** | `ManagedIdentityAsync` | `DefaultAzureCredential` / `ManagedIdentityCredential` | `api://{resourceAppId}/.default` | Running **inside Azure** (App Service, Functions, VM, Container Apps). No secret. |
| **Silent (cache)** | `TrySilentAsync` | Public | (delegated) | Always tried first for the delegated flows — reuses a cached/refreshed token before prompting. |

`AcquireAsync(flow)` wires this together: the **delegated** flows (device-code, interactive) call
`TrySilentAsync` first and only prompt when nothing usable is cached.

---

## Auth configuration (Entra ID)

| Setting | Value |
|---|---|
| Authority | `https://login.microsoftonline.com/{tenantId}` |
| Audience (`aud`) | `api://{resourceAppId}` |
| Delegated scope | `api://{resourceAppId}/user_impersonation` |
| App-only scope | `api://{resourceAppId}/.default` |

### Role assignment is mandatory

The engine's authorization middleware **rejects roleless callers**. After registering the apps you
must assign access on the **engine's resource service principal**:

- **App-only flows** (client-credentials, managed identity): add an **app role** to the engine's app
  registration (Expose an API / App roles), then grant it to the client / managed identity and
  **admin-consent** it. The token must carry a `roles` claim.
- **Delegated flows** (device-code, interactive): the signed-in **user** (or group) must be assigned
  to the engine application (Enterprise application → Users and groups), and the
  `user_impersonation` scope consented. The token carries a `scp` claim (and optionally `roles`).

A token with the right `aud` but **no** role/assignment will still be denied by the engine.

---

## Token lifecycle

- **Silent-first:** every secure call asks `TrySilentAsync` for a cached token. MSAL transparently
  uses the cached **refresh token** to mint a fresh access token when the old one has expired — no
  prompt. Only when there is no usable cached token does an interactive flow run.
- **Expiry:** access tokens are typically valid ~60–90 minutes. You don't track this yourself —
  MSAL (and `DefaultAzureCredential`) handle refresh/expiry. App-only tokens are cached by MSAL and
  re-minted automatically.
- **Persistent cache:** backed by `Microsoft.Identity.Client.Extensions.Msal.MsalCacheHelper`, so
  tokens survive process restarts. Cache location:
  - **Windows:** `%APPDATA%\WwExecutionClient\ww_execution_msal_cache.bin` (DPAPI-encrypted).
  - **macOS:** Keychain (service `com.warewolf.execution.client`).
  - **Linux:** libsecret keyring (`com.warewolf.execution.client.tokencache`); falls back to a
    plaintext file on headless boxes without a keyring.
  - File name is configurable via `WwExecution:CacheFileName`.

---

## Automatic token injection

You never set the `Authorization` header by hand. `Auth/BearerTokenHandler.cs` is a
`DelegatingHandler` registered on the typed `HttpClient`. Per request it:

1. Leaves **`/public/*`** untouched (anonymous).
2. Acquires a token via `TokenAcquirer.AcquireAsync(Flow)` (silent-first) and sets
   `Authorization: Bearer <token>` for **`/secure/*`** and **`/services/*`**.
3. Adds **`x-functions-key`** for **`/services/*`** (from `WwExecution:FunctionKey`).

`WwExecutionService` therefore contains zero auth code — it just builds URI-encoded paths and sends.

---

## Configure & run

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Two Entra app registrations (or one resource + one client) and the role assignment above.

### 2. Configure
Edit `appsettings.json` (or override via environment variables prefixed `WWEXEC_`, e.g.
`WWEXEC_WwExecution__ClientSecret=...`, or command-line `--WwExecution:TenantId=...`):

```jsonc
{
  "WwExecution": {
    "BaseUrl": "https://WWExecutionEngine.azurewebsites.net",
    "TenantId": "<tenant-guid>",
    "ResourceAppId": "<engine-app-guid>",     // audience api://<this>
    "ClientId": "<client-app-guid>",
    "ClientSecret": "",                         // only for client-credentials
    "FunctionKey": "",                          // only for /services/*
    "SampleWorkflow": "Hello World"
  }
}
```

> **Do not commit secrets.** For local dev prefer
> `dotnet user-secrets set "WwExecution:ClientSecret" "<secret>"`, environment variables, or Key Vault.

### 3. Run

```bash
dotnet run
```

Pick a flow from the menu. Options 1–4 call `GET /secure/Hello World.json?Name=Alice` using the
chosen flow; option 5 calls the anonymous `/public` route; option 6 calls `/apis.json`.

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| **401 Unauthorized**, log shows audience mismatch | Token `aud` ≠ `api://{resourceAppId}` (wrong scope/resource, or v1/v2 token mismatch) | Ensure scopes are `api://{resourceAppId}/.default` (app) or `/user_impersonation` (delegated) and `ResourceAppId` matches the engine app. |
| **HTTP 500** with a nested `Error{…}` body on `/secure` or `/services` | Engine authorization **denial** — caller has no role/assignment, or lacks `View`/`Execute`. Denials are wrapped as 500 (403 path disabled pending WOLF-8418). | Assign the app role (app-only) or user/group + `user_impersonation` (delegated) on the engine SP and admin-consent. |
| **`consent_required` / `interaction_required`** during silent | No prior consent, or MFA/conditional-access requires interaction | Run an interactive/device-code flow once; admin-consent delegated permissions for the tenant. |
| **`AADSTS7000218`** (request body must contain `client_assertion`/`client_secret`) | The client app registration is treated as **confidential** but a public-client flow (device-code/interactive) was used | Enable **Allow public client flows** on the client app registration, and register a public redirect URI (`http://localhost`). |
| **`DefaultAzureCredential` fails locally** | No managed identity outside Azure; no fallback credential available | Sign in with `az login`, or use Visual Studio / `AZURE_*` env vars; or pick a different flow locally. Set `ManagedIdentityClientId` for a user-assigned identity in Azure. |
| **`requires a function key` exception** on `/services/*` | `WwExecution:FunctionKey` not set | Provide the Azure Functions host/function key. |
| **Token cache not persisting on Linux** | No libsecret keyring on a headless box | The helper falls back to a plaintext cache file; ensure the app-data dir is writable. |

---

## File map

| File | Responsibility |
|---|---|
| `WwExecutionClient.csproj` | Project + NuGet references (.NET 8, nullable). |
| `appsettings.json` | Entra config, credentials, base URL, function key. |
| `WwExecutionClientOptions.cs` | Typed options + derived authority/scope/audience values. |
| `TokenAcquirer.cs` | The five flows + persistent MSAL cache wiring. |
| `Auth/BearerTokenHandler.cs` | `DelegatingHandler` that injects the bearer token (+ `x-functions-key`). |
| `WwExecutionService.cs` | Typed `/public`, `/secure`, `/services`, `/apis.json` calls. |
| `Program.cs` | Host builder, DI, interactive demo menu. |
