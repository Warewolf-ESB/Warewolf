# Azure Provisioning — Client Apps for wwexecution

This guide explains how to provision client app registrations in Microsoft Entra ID that can call the **wwexecution** Function App. Use `Scripts/Configure-WwExecutionAuth-Clients.ps1`.

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

## Quick Start

```powershell
# Provision all three client types
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "11111111-1111-1111-1111-111111111111" `
    -TenantId "22222222-2222-2222-2222-222222222222"
```

---

## Provisioning Each Client Type

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
- Sets SPA platform redirect URIs (enables PKCE)
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
- Creates a client secret
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
    -AppRolesToAssign @('Permission.Execute','Permission.View')

# With Managed Identity (no secret)
./Scripts/Configure-WwExecutionAuth-Clients.ps1 `
    -ResourceAppId "<resource-app-id>" `
    -TenantId "<tenant-id>" `
    -ClientType Daemon `
    -DaemonUseManagedIdentity
```

**What it configures:**
- Creates `<prefix>-daemon` app registration
- Creates client secret (unless MI mode)
- Creates service principal
- Assigns app roles directly to the SP

**Token acquisition:** Client Credentials grant (`grant_type=client_credentials`).

---

## Parameters Reference

| Parameter | Description | Default |
|---|---|---|
| `-ResourceAppId` | Client ID of the wwexecution resource app | (required) |
| `-TenantId` | Entra tenant GUID | (required) |
| `-ClientType` | `SPA`, `Confidential`, `Daemon`, or `All` | `All` |
| `-ClientDisplayNamePrefix` | Prefix for app names | `wwexecution` |
| `-SpaRedirectUris` | Redirect URIs for SPA | `localhost:4200`, `localhost:3000` |
| `-WebRedirectUris` | Redirect URIs for web app | `localhost:5001/signin-oidc` |
| `-DaemonUseManagedIdentity` | Skip secret, use MI | `$false` |
| `-SecretLifetimeYears` | Secret validity (1–2) | 1 |
| `-AppRolesToAssign` | Roles for daemon SP | `Permission.Execute`, `Permission.View` |
| `-DryRun` | Print plan only | `$false` |
| `-NonInteractive` | No prompts | `$false` |

---

## Output

The script writes `Scripts/Configure-WwExecutionAuth-Clients.output.json`:

```json
{
  "Timestamp": "2024-01-15T10:30:00Z",
  "TenantId": "22222222-...",
  "ResourceAppId": "11111111-...",
  "Scope": "api://11111111-.../.default",
  "Authority": "https://login.microsoftonline.com/22222222-...",
  "Clients": {
    "SPA": { "DisplayName": "wwexecution-spa", "ClientId": "...", "RedirectUris": [...] },
    "Confidential": { "DisplayName": "wwexecution-web", "ClientId": "...", "ClientSecret": "..." },
    "Daemon": { "DisplayName": "wwexecution-daemon", "ClientId": "...", "ClientSecret": "..." }
  }
}
```

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
