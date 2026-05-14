# Azure Provisioning — wwexecution Function App (Resource API)

This guide explains how to provision Microsoft Entra ID authentication and Azure Easy Auth for the **wwexecution** Azure Function App using `Scripts/Configure-WwExecutionAuth.ps1`.

---

## Prerequisites

| Requirement | Details |
|---|---|
| Azure CLI | >= 2.55 (`az --version`) |
| Azure subscription | With Contributor role on the target resource group |
| Entra ID permissions | Application Administrator (or `Application.ReadWrite.All`) |
| Function App | Already created (Linux or Windows, .NET 8 isolated worker) |
| PowerShell | 5.1+ or PowerShell 7+ |

---

## Quick Start

```powershell
# 1. Login to Azure
az login

# 2. Run the provisioning script (interactive mode)
./Scripts/Configure-WwExecutionAuth.ps1

# 3. Or non-interactive for CI/CD
./Scripts/Configure-WwExecutionAuth.ps1 `
    -SubscriptionId "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" `
    -TenantId "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy" `
    -ResourceGroupName "rg-warewolf-prod" `
    -FunctionAppName "wwexecution" `
    -NonInteractive -SkipSmokeTest
```

---

## What the Script Does (Stage by Stage)

### Stage 0 — Pre-flight

- Sets the Azure subscription context
- Validates the function app exists in the specified resource group
- Fails loudly if the app is not found (prevents silent misconfiguration)

### Stage 1 — Entra App Registration

- Creates (or finds existing) an Entra ID app registration named `<FunctionAppName>-auth`
- Sets `signInAudience` = `AzureADMyOrg` (single-tenant)
- Configures the redirect URI: `https://<FunctionAppName>.azurewebsites.net/.auth/login/aad/callback`

### Stage 2 — Implicit Grant ID Token

- Enables `enableIdTokenIssuance = true` on the app registration
- **Why**: Easy Auth uses hybrid flow (`response_type=code+id_token`); without this, browser sign-in fails with `AADSTS700054`

### Stage 3 — Expose API

- Sets `identifierUris = api://<clientId>` on the registration
- This makes the app a valid OAuth 2.0 resource

### Stage 3b — Expose `user_impersonation` Scope

- Adds an `oauth2PermissionScopes` entry with value `user_impersonation`
- **Why**: Without at least one delegated scope, token acquisition via `az login --scope` or MSAL fails with `AADSTS650057`

### Stage 4 — App Roles

- Creates declarative app roles for groups and permissions:
  - Group roles (from `-GroupPermissions` parameter)
  - Permission roles: `Permission.View`, `Permission.Execute`, `Permission.Contribute`, `Permission.DeployTo`, `Permission.DeployFrom`, `Permission.Administrator`
- Preserves existing role IDs to avoid invalidating live assignments

### Stage 5 — Service Principal

- Creates the service principal for the app registration (if missing)
- Required before any role assignments can be made

### Stage 6 — User Role Assignments

- Assigns users to groups and permission roles based on `-UserAssignments`
- Skippable with `-SkipUserAssignment`
- Idempotent: skips already-assigned roles

### Stage 7 — Client Secret

- Creates or rotates a client secret for the Easy Auth confidential client flow
- Rotation triggers:
  - `-RotateSecret` flag
  - No credential with > 30 days remaining
  - App setting `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` is missing
- Secret lifetime controlled by `-SecretLifetimeYears` (1–2)

### Stage 8 — Function App Settings

Writes the following app settings to the function app:

| Setting | Value |
|---|---|
| `WAREWOLF_ENTRA_TENANT_ID` | Tenant GUID |
| `WAREWOLF_ENTRA_AUDIENCE` | `api://<clientId>` |
| `WAREWOLF_SECURE_CONFIG` | Path to secure.config |
| `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` | Client secret (if rotated) |

### Stage 9 — Easy Auth Configuration

1. Migrates Easy Auth V1 → V2 if needed
2. Configures the Microsoft (Entra) provider with:
   - Client ID
   - Client secret setting name reference
   - Issuer: `https://login.microsoftonline.com/<tenantId>/v2.0`
   - Allowed audience: `api://<clientId>`
3. Enables the platform with `AllowAnonymous` action (so `/public/*` stays open)
4. Asserts `tokenStore.enabled = true`

### Stage 10 — End-to-End Verification

Cross-checks all configuration:
- Easy Auth enabled, clientId, issuer, audience, action, tokenStore
- Entra app: ID token issuance, user_impersonation scope, redirect URI
- All required app settings present and non-empty

### Stage 11 — Smoke Test (Optional)

- Probes `/Public/Hello World.json` expecting 200
- Probes `/Secure/Hello World.json` expecting 401 or 302
- Skip with `-SkipSmokeTest`

### Stage 12 — Output

Writes a JSON summary to `Scripts/Configure-WwExecutionAuth.output.json` containing all provisioned IDs.

---

## Key Parameters

| Parameter | Description | Default |
|---|---|---|
| `-SubscriptionId` | Azure subscription GUID | (prompted) |
| `-TenantId` | Entra tenant GUID | (prompted) |
| `-ResourceGroupName` | Resource group containing the function app | (prompted) |
| `-FunctionAppName` | Name of the existing function app | (prompted) |
| `-EntraAppDisplayName` | App registration display name | `<FunctionAppName>-auth` |
| `-SecretLifetimeYears` | Secret validity (1–2) | 1 |
| `-GroupPermissions` | Hashtable: group name → Permission.* array | `@{}` |
| `-UserAssignments` | Array of `@{Upn; Group}` | `@()` |
| `-RotateSecret` | Force secret rotation | `$false` |
| `-SkipUserAssignment` | Skip Stage 6 | `$false` |
| `-SkipSmokeTest` | Skip Stage 11 | `$false` |
| `-WhatIfOnly` / `-DryRun` | Print plan, no changes | `$false` |
| `-UseManagedIdentity` | Prefer MI over extra secrets | `$false` |
| `-NonInteractive` | Skip all prompts (CI/CD) | `$false` |

---

## Example: Production Setup

```powershell
./Scripts/Configure-WwExecutionAuth.ps1 `
    -SubscriptionId "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" `
    -TenantId "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb" `
    -ResourceGroupName "rg-warewolf-prod" `
    -FunctionAppName "wwexecution" `
    -GroupPermissions @{
        'Developers' = @('Permission.View','Permission.Execute','Permission.Contribute')
        'Operators'  = @('Permission.View','Permission.Execute')
        'Admins'     = @('Permission.Administrator')
    } `
    -UserAssignments @(
        @{ Upn = 'alice@contoso.com'; Group = 'Admins' }
        @{ Upn = 'bob@contoso.com';   Group = 'Developers' }
    ) `
    -NonInteractive -SkipSmokeTest
```

---

## Dry-Run / CI Validation

```powershell
./Scripts/Configure-WwExecutionAuth.ps1 `
    -SubscriptionId "..." -TenantId "..." `
    -ResourceGroupName "..." -FunctionAppName "..." `
    -DryRun -NonInteractive
```

This prints the resolved configuration and exits without touching Azure.

---

## How the Auth Middleware Uses This Configuration

The function app runtime has a three-stage middleware pipeline:

```
Request → EasyAuthRedirectMiddleware → ClaimsPrincipalBuilderMiddleware → WorkflowAuthorizationMiddleware → Function
```

1. **EasyAuthRedirectMiddleware**: Passes `/public/*` through; returns 401 JSON for API callers on `/secure/*` without a token; redirects browsers to `/.auth/login/aad`.
2. **ClaimsPrincipalBuilderMiddleware**: Builds the authenticated principal from either:
   - `X-MS-CLIENT-PRINCIPAL` header (injected by Azure Easy Auth after browser sign-in)
   - `Authorization: Bearer <jwt>` header (validated locally using OIDC metadata from `WAREWOLF_ENTRA_TENANT_ID` / `WAREWOLF_ENTRA_AUDIENCE`)
3. **WorkflowAuthorizationMiddleware**: Enforces `secure.config` policies (group membership + permission flags).

This means the provisioning script configures **both** browser-based sign-in (via Easy Auth redirect) and non-browser token-based access (via Bearer JWT validation).

---

## Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `AADSTS700054` on browser sign-in | `enableIdTokenIssuance` not set | Re-run script (Stage 2 fixes it) |
| `AADSTS650057` on token acquisition | No `user_impersonation` scope | Re-run script (Stage 3b fixes it) |
| 401 with valid token | Audience mismatch | Check `WAREWOLF_ENTRA_AUDIENCE` matches token `aud` |
| Easy Auth shows "enabled: null" | Function app name wrong | Verify `-FunctionAppName` matches Azure |
| 403 on `/secure/*` | Missing group/permission in secure.config | Edit secure.config to grant access |

---

## Next Steps

After provisioning the function app, proceed to:
- [Client App Provisioning](AzureProvisioning-ClientApps.md) — register SPA, web, and daemon clients
- [Client Examples (Curl)](ClientsExamples.md) — acquire tokens and call workflows
- [README-Authentication](README-Authentication.md) — full end-to-end setup guide
