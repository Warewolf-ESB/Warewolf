# Azure Provisioning — Client Apps for wwexecution

This guide explains how to provision and remove client app registrations in Microsoft Entra ID that can call the **wwexecution** Function App.

| Script | Purpose |
|---|---|
| `Scripts/Configure-WwExecutionAuth-Clients.ps1` | Create / update client registrations |
| `Scripts/Remove-WwExecutionAuth-Clients.ps1` | Remove client registrations (cleanup) |

---

## Prerequisites

- The wwexecution resource app must already be provisioned (see [AzureProvisioning-FunctionApp.md](AzureProvisioning-FunctionApp.md))
- You need the **Resource App ID** (client ID of the `<functionAppName>-auth` registration)
- Azure CLI logged in with Application Administrator permissions

---

## Client Types Overview

| Type | Grant Flow | Use Case | Secret Required |
|---|---|---|---|
| **SPA** | Authorization Code + PKCE | Browser apps (React, Angular) | No (public client) |
| **Confidential** | Authorization Code + OBO | Server-side web apps, APIs | Yes |
| **Daemon** | Client Credentials | Background services, CI/CD, Managed-Identity workers | Yes (or Managed Identity) |
| **Console** | Device Code + Interactive **and** Client Credentials on one registration | .NET console / CLI tools that mix interactive sign-in with unattended runs | Yes (public-client + secret) |

> `SPA`, `Confidential` and `Daemon` are provisioned by `-ClientType All`.
> **`Console` is opt-in** (`-ClientType Console`) — it is a combined
> public-desktop + confidential registration and is not included in `All`.

### Mapping to runnable client examples

Each example under [`Warewolf.Execution.Lightweight.ClientExamples/`](../../Warewolf.Execution.Lightweight.ClientExamples/)
maps to one of the registration types provisioned by this script:

| Example | Stack | Registration type | Identity / flow |
|---|---|---|---|
| Example | Stack | Registration type | `-ClientType` invocation |
|---|---|---|---|
| [`Angular17/`](../../Warewolf.Execution.Lightweight.ClientExamples/Angular17/) | Angular SPA | **SPA** (own reg) | `SPA -ClientDisplayNamePrefix wwexecution-angular -SpaRedirectUris http://localhost:4201` |
| [`React/`](../../Warewolf.Execution.Lightweight.ClientExamples/React/) | React SPA | **SPA** (own reg) | `SPA -ClientDisplayNamePrefix wwexecution-react -SpaRedirectUris http://localhost:5173` |
| [`DotNetWebMvc/`](../../Warewolf.Execution.Lightweight.ClientExamples/DotNetWebMvc/) | ASP.NET Core MVC | **Confidential** | `Confidential` (default `https://localhost:5001/signin-oidc`) |
| [`DotNetConsole/`](../../Warewolf.Execution.Lightweight.ClientExamples/DotNetConsole/) | .NET 8 console | **Console** | `Console -AppRolesToAssign <group-role>` |
| [`AzureFunction/`](../../Warewolf.Execution.Lightweight.ClientExamples/AzureFunction/) | .NET 8 isolated Function | **Daemon** (Managed Identity) | `Daemon -DaemonUseManagedIdentity -ManagedIdentityObjectId <miSpId> -AppRolesToAssign <group-role>` |
| [`AzureServiceBus/`](../../Warewolf.Execution.Lightweight.ClientExamples/AzureServiceBus/) | SB-triggered worker | **Daemon** (Managed Identity) | `Daemon -DaemonUseManagedIdentity -ManagedIdentityObjectId <miSpId> -AppRolesToAssign <group-role>` |

> The two SPA examples use **separate registrations** (distinct dev ports: Angular
> `4201`, React `5173`), so run the script once per app with a different
> `-ClientDisplayNamePrefix` and `-SpaRedirectUris`.

> **App-role assignment is mandatory for app-only callers.** A daemon / Managed
> Identity (and the Console **client-credentials** flow) with **no** app role
> carries no `roles` claim and is rejected by the engine's authorization
> middleware — see [README-Authentication](README-Authentication.md). For that
> reason `-AppRolesToAssign` has **no default**, and the Daemon / MI path **fails
> loudly** (throws) when no role resolves rather than creating a roleless client.
> The values you pass must be **group-role values that exist on the resource app**
> (created by `Configure-WwExecutionAuth.ps1 -GroupPermissions`, e.g.
> `Warewolf_Developers`) — they are **not** `Permission.*` names. For Managed
> Identities, assign the role to the **existing MI service principal** with
> `-DaemonUseManagedIdentity -ManagedIdentityObjectId` (see [Type C](#type-c--daemon--service)).

---

## Script Output — Console Colors

Both scripts use a consistent color convention:

| Color | Prefix | Meaning |
|---|---|---|
| Cyan | `[...]` | Step about to execute |
| Green | `[OK]` | Operation succeeded |
| Yellow | `[WRN]` | Non-fatal warning |
| Red | `[ERR]` | Fatal failure |
| DarkGray | `[INF]` / `> az ...` | Informational / command echo |

Every `az` CLI command is printed to the console **before** it executes so you can see exactly what is being run.

---

## Provisioning

### Quick Start

```powershell
# Provision all three client types (interactive — prompts for inputs, shows summary, asks to confirm)
./Scripts/Configure-WwExecutionAuth-Clients.ps1

# Non-interactive (CI/CD)
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "11111111-1111-1111-1111-111111111111" `
    -TenantId "22222222-2222-2222-2222-222222222222" `
    -NonInteractive

# Dry-run (print resolved config, make no changes)
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "11111111-..." -TenantId "22222222-..." `
    -DryRun -NonInteractive
```

### Type A — SPA (Single Page Application)

```powershell
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "<resource-app-id>" `
    -TenantId "<tenant-id>" `
    -ClientType SPA `
    -SpaRedirectUris @('http://localhost:4200', 'http://localhost:3000') `
    -FunctionAppName "<function-app-name>" `
    -FunctionAppResourceGroup "<resource-group>"
```

**What it configures:**
- Creates `<prefix>-spa` app registration
- Sets SPA platform redirect URIs via Graph PATCH (enables PKCE)
- Grants delegated `user_impersonation` scope
- Grants admin consent
- **Enables CORS on the Function App** and adds each SPA origin (scheme + host derived from `-SpaRedirectUris`) to the Function App Allowed Origins

#### Function App Allowed Origins (CORS)

Browser-based SPA clients are subject to the browser's same-origin policy.  Without a matching Allowed Origin entry on the Function App, the browser will block the `Authorization: Bearer` header on cross-origin requests.

The script automatically derives the origin (scheme + host, e.g. `http://localhost:4200`) from each entry in `-SpaRedirectUris` and adds it with:

```bash
az functionapp cors add \
  --name <function-app-name> \
  --resource-group <resource-group> \
  --allowed-origins http://localhost:4200
```

The operation is idempotent — origins already present are skipped.

**To skip CORS configuration** (e.g. it is managed elsewhere):

```powershell
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "<resource-app-id>" `
    -TenantId "<tenant-id>" `
    -ClientType SPA `
    -SkipCorsConfiguration
```

**Why CORS is NOT configured for Confidential or Daemon clients:**

| Client Type | Runs in browser? | Needs CORS? |
|---|---|---|
| SPA | Yes — MSAL.js / fetch from browser | **Yes** — browser enforces same-origin policy |
| Confidential Web | No — server-side HTTP calls | No — server-to-server, no browser origin restriction |
| Daemon / Service | No — background service / MI | No — server-to-server, no browser origin restriction |

**Token acquisition:** Device Code Flow or MSAL.js `acquireTokenSilent` / `loginRedirect`.

---

### Type B — Confidential Web App

```powershell
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "<resource-app-id>" `
    -TenantId "<tenant-id>" `
    -ClientType Confidential `
    -WebRedirectUris @('https://myapp.contoso.com/signin-oidc')
```

**What it configures:**
- Creates `<prefix>-web` app registration
- Sets web redirect URIs **plus** the site origin and a front-channel logout URL
  (`/signout-callback-oidc`) so Microsoft.Identity.Web sign-out is accepted
- Creates a client secret (appended, does not replace existing)
- Grants delegated `user_impersonation` scope
- Grants admin consent
- *(optional)* assigns any `-AppRolesToAssign` as application permissions — not
  needed for the delegated-only WebMvc example

**Token acquisition:** Authorization Code flow (server-side), or On-Behalf-Of for downstream calls.

---

### Type C — Daemon / Service

```powershell
# With client secret — roles are REQUIRED (must exist on the resource app)
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "<resource-app-id>" `
    -TenantId "<tenant-id>" `
    -ClientType Daemon `
    -AppRolesToAssign @('Warewolf_Developers')

# With a NEW Managed-Identity daemon registration (no secret)
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "<resource-app-id>" `
    -TenantId "<tenant-id>" `
    -ClientType Daemon `
    -DaemonUseManagedIdentity

# Assign roles to an EXISTING managed identity (Azure Function / Service Bus worker)
# — no new app registration is created; roles go straight onto the MI's SP.
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "<resource-app-id>" `
    -TenantId "<tenant-id>" `
    -ClientType Daemon `
    -DaemonUseManagedIdentity `
    -ManagedIdentityObjectId "<mi-sp-object-id>"
```

**What it configures:**
- Creates `<prefix>-daemon` app registration (skipped when `-ManagedIdentityObjectId` is supplied)
- Creates client secret unless `-DaemonUseManagedIdentity` is set
- Creates service principal (skipped for an existing MI)
- Assigns app roles directly to the daemon SP **or** to the supplied managed-identity SP

> **Fail-loud:** because an app-only caller is authorised solely by its app roles,
> the Daemon / MI path **throws** if `-AppRolesToAssign` resolves to no role on the
> resource app (rather than silently creating a roleless client). Pass values that
> exist on the resource app (e.g. `Warewolf_Developers`).

**Token acquisition:** Client Credentials grant (`grant_type=client_credentials`),
or Managed Identity via `DefaultAzureCredential` for the Azure Function / Service Bus
worker examples.

---

### Type D — Console (public-desktop + client-credentials)

For the .NET console example, which uses **one** registration for device-code,
interactive browser, **and** client-credentials flows:

```powershell
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "<resource-app-id>" `
    -TenantId "<tenant-id>" `
    -ClientType Console `
    -AppRolesToAssign @('Warewolf_Developers')   # only needed for the client-credentials flow
```

**What it configures:**
- Creates `<prefix>-console` app registration
- Sets **public-client (Mobile & desktop)** redirect URIs (`-ConsoleRedirectUris`,
  default `http://localhost`) and `isFallbackPublicClient=true` — enabling
  device-code and interactive browser sign-in
- Creates a client secret — enabling the client-credentials flow on the **same** registration
- Grants delegated `user_impersonation` + admin consent (for the interactive/device-code flows)
- Assigns app roles to the SP when `-AppRolesToAssign` is supplied (for client-credentials)

> Roles are **optional** here: the delegated device-code/interactive flows work
> without them. If you omit `-AppRolesToAssign`, the script **warns** that the
> client-credentials flow will be rejected, but does not fail.

**Token acquisition:** Device Code / Interactive (delegated) or Client Credentials (app-only).

---

## Provisioning Parameters Reference

| Parameter | Description | Default |
|---|---|---|
| `-ResourceAppId` | Client ID of the wwexecution resource app | (required) |
| `-TenantId` | Entra tenant GUID | (required) |
| `-ClientType` | `SPA`, `Confidential`, `Daemon`, `Console`, or `All` (`All` = SPA+Confidential+Daemon; `Console` is opt-in) | `All` |
| `-ClientDisplayNamePrefix` | Prefix for app names | `wwexecution` |
| `-SpaRedirectUris` | Redirect URIs for SPA | `localhost:4200`, `localhost:3000` |
| `-WebRedirectUris` | Redirect URIs for web app | `localhost:5001/signin-oidc` |
| `-ConsoleRedirectUris` | Public-desktop redirect URIs for the Console client | `http://localhost` |
| `-DaemonUseManagedIdentity` | Skip secret, use MI | `$false` |
| `-ManagedIdentityObjectId` | Object ID of an existing MI SP — assign roles to it, create no daemon app | (empty) |
| `-SecretLifetimeYears` | Secret validity (1–2) | `1` |
| `-AppRolesToAssign` | App-role values for app-only clients (Daemon/MI/Console). Must exist on the resource app (group-role values, e.g. `Warewolf_Developers`) — **not** `Permission.*`. Daemon/MI **throws** if none resolve. | (none) |
| `-FunctionAppName` | Azure Function App name — used to add Allowed Origins (SPA only) | (empty — prompts or skips) |
| `-FunctionAppResourceGroup` | Resource group of the Function App (SPA CORS only) | (empty — prompts or skips) |
| `-SkipCorsConfiguration` | Skip Function App CORS / Allowed Origins setup for SPA | `$false` |
| `-DryRun` | Print plan only, no changes | `$false` |
| `-NonInteractive` | Skip all prompts; fail on missing values | `$false` |

---

## Output

The script writes `Scripts/Configure-WwExecutionAuth-Clients.output.json` (BOM-free UTF-8):

```json
{
  "Timestamp": "2024-01-15T10:30:00.0000000Z",
  "TenantId": "22222222-...",
  "ResourceAppId": "11111111-...",
  "ResourceSpId": "33333333-...",
  "Scope": "api://11111111-.../.default",
  "Authority": "https://login.microsoftonline.com/22222222-...",
  "ClientDisplayNamePrefix": "wwexecution",
  "FunctionAppName": "myfuncapp",
  "FunctionAppResourceGroup": "myRG",
  "Clients": {
    "SPA":          { "DisplayName": "wwexecution-spa",    "ClientId": "...", "RedirectUris": [...], "CorsOriginsAdded": ["http://localhost:4200","http://localhost:3000"], "GrantType": "Authorization Code + PKCE (public client)" },
    "Confidential": { "DisplayName": "wwexecution-web",    "ClientId": "...", "ClientSecret": "...", "SecretExpiry": "2026-01-15", "GrantType": "Authorization Code (confidential) + OBO" },
    "Daemon":       { "DisplayName": "wwexecution-daemon", "ClientId": "...", "ClientSecret": "...", "SpObjectId": "...", "RolesAssigned": [...], "GrantType": "Client Credentials" },
    "Console":      { "DisplayName": "wwexecution-console","ClientId": "...", "ClientSecret": "...", "SpObjectId": "...", "RedirectUris": ["http://localhost"], "RolesAssigned": [...], "GrantType": "Device Code + Interactive (delegated) + Client Credentials (app-only)" }
  }
}
```

> **Security:** `ClientSecret` values in the output JSON should be moved to Azure Key Vault before use in production. The script prints a `[WRN]` reminder for each secret.

---

## Cleanup — Removing Client Registrations

Use `Remove-WwExecutionAuth-Clients.ps1` to reverse all provisioning steps.

```powershell
# Interactive (shows removal plan, default answer is N — safe)
./Scripts/Remove-WwExecutionAuth-Clients.ps1

# Non-interactive
./Scripts/Remove-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "11111111-..." -TenantId "22222222-..." `
    -NonInteractive

# Remove only the daemon registration
./Scripts/Remove-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "11111111-..." -TenantId "22222222-..." `
    -ClientType Daemon -NonInteractive

# Dry-run — print what would be removed, make no changes
./Scripts/Remove-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "11111111-..." -TenantId "22222222-..." `
    -DryRun -NonInteractive
```

**What it removes (in order):**

| Stage | Action |
|---|---|
| 3 | Revoke daemon app-role assignments from resource SP |
| 4 | Remove delegated + application permission grants |
| 5 | Delete client service principals |
| 6 | Delete client app registrations |

Operations are idempotent — already-removed resources are logged as `[INF]` skips, not errors.

### Cleanup Parameters Reference

| Parameter | Description | Default |
|---|---|---|
| `-ResourceAppId` | Client ID of the wwexecution resource app | (required) |
| `-TenantId` | Entra tenant GUID | (required) |
| `-ClientType` | `SPA`, `Confidential`, `Daemon`, or `All` | `All` |
| `-ClientDisplayNamePrefix` | Prefix used when registrations were created | `wwexecution` |
| `-DryRun` | Print what would be removed, no changes | `$false` |
| `-NonInteractive` | Skip all prompts; fail on missing values | `$false` |

---

## End-to-End Example

See [Example-ClientApps-OrdersSales.ps1](../Scripts/Example-ClientApps-OrdersSales.ps1) for a complete working example that:
1. Provisions two client apps (Orders SPA + Sales Daemon)
2. Acquires tokens for both
3. Calls secured workflows
4. Demonstrates error handling and token refresh patterns

---

## Next Steps

- [Client Examples (Curl)](ClientsExamples.md) — curl commands for every grant type
- [README-Authentication](README-Authentication.md) — full end-to-end setup guide
