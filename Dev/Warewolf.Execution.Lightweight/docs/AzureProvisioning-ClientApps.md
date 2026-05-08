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
| **Daemon** | Client Credentials | Background services, CI/CD | Yes (or Managed Identity) |

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
    -SpaRedirectUris @('http://localhost:4200', 'http://localhost:3000')
```

**What it configures:**
- Creates `<prefix>-spa` app registration
- Sets SPA platform redirect URIs via Graph PATCH (enables PKCE)
- Grants delegated `user_impersonation` scope
- Grants admin consent

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
- Sets web redirect URIs
- Creates a client secret (appended, does not replace existing)
- Grants delegated `user_impersonation` scope
- Grants application permissions (`Permission.Execute`, `Permission.View`)
- Grants admin consent

**Token acquisition:** Authorization Code flow (server-side), or On-Behalf-Of for downstream calls.

---

### Type C — Daemon / Service

```powershell
# With client secret
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "<resource-app-id>" `
    -TenantId "<tenant-id>" `
    -ClientType Daemon `
    -AppRolesToAssign @('Permission.Execute', 'Permission.View')

# With Managed Identity (no secret)
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "<resource-app-id>" `
    -TenantId "<tenant-id>" `
    -ClientType Daemon `
    -DaemonUseManagedIdentity
```

**What it configures:**
- Creates `<prefix>-daemon` app registration
- Creates client secret unless `-DaemonUseManagedIdentity` is set
- Creates service principal
- Assigns app roles directly to the daemon SP

**Token acquisition:** Client Credentials grant (`grant_type=client_credentials`).

---

## Provisioning Parameters Reference

| Parameter | Description | Default |
|---|---|---|
| `-ResourceAppId` | Client ID of the wwexecution resource app | (required) |
| `-TenantId` | Entra tenant GUID | (required) |
| `-ClientType` | `SPA`, `Confidential`, `Daemon`, or `All` | `All` |
| `-ClientDisplayNamePrefix` | Prefix for app names | `wwexecution` |
| `-SpaRedirectUris` | Redirect URIs for SPA | `localhost:4200`, `localhost:3000` |
| `-WebRedirectUris` | Redirect URIs for web app | `localhost:5001/signin-oidc` |
| `-DaemonUseManagedIdentity` | Skip secret, use MI | `$false` |
| `-SecretLifetimeYears` | Secret validity (1–2) | `1` |
| `-AppRolesToAssign` | Roles for daemon SP (sanitized, underscores) | `Permission.Execute`, `Permission.View` |
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
  "Clients": {
    "SPA":          { "DisplayName": "wwexecution-spa",    "ClientId": "...", "RedirectUris": [...], "GrantType": "Authorization Code + PKCE (public client)" },
    "Confidential": { "DisplayName": "wwexecution-web",    "ClientId": "...", "ClientSecret": "...", "SecretExpiry": "2026-01-15", "GrantType": "Authorization Code (confidential) + OBO" },
    "Daemon":       { "DisplayName": "wwexecution-daemon", "ClientId": "...", "ClientSecret": "...", "SpObjectId": "...", "RolesAssigned": [...], "GrantType": "Client Credentials" }
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
