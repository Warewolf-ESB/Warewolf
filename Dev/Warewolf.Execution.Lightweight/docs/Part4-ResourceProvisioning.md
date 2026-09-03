# Part 4 — Resource Provisioning Step-by-Step
## Warewolf Execution Lightweight — Azure & Entra ID Setup

> **Script reference:** `Scripts/Configure-WwExecutionAuth-debug.ps1`  
> Run the script for automated provisioning; this document explains *why* each
> step exists and what to verify manually if automation fails.

---

## Prerequisites

| Requirement | Minimum version / role |
|---|---|
| Azure CLI | `>= 2.55` — must support `az webapp auth microsoft update` and `--enable-id-token-issuance` |
| Caller role (Azure) | Contributor on the Function App resource group |
| Caller role (Entra) | Application Administrator (or `Application.ReadWrite.All`) |
| Function App | Must already exist before running the script |

```powershell
az login
az account set --subscription <subscriptionId>
```

---

## Stage 0 — Pre-flight Checks

**Purpose:** Validate that the target Function App exists and the subscription
is correct before any change is made.

```powershell
az functionapp show `
  --name <FunctionAppName> `
  --resource-group <ResourceGroupName> `
  --query name -o tsv
```

**Expected result:** The function app name is echoed back.  
**If it fails:** Create the Function App first or correct `$FunctionAppName`.

---

## Stage 1 — Entra App Registration

**Purpose:** Create (or locate) the Entra app registration that represents the
resource.  All permissions, scopes, and Easy Auth will reference this app.

```powershell
# Check for existing registration
az ad app list --display-name "<FunctionAppName>-auth" --query "[0]" -o json

# Create if not found
az ad app create `
  --display-name "<FunctionAppName>-auth" `
  --sign-in-audience AzureADMyOrg `
  --web-redirect-uris "https://<FunctionAppName>.azurewebsites.net/.auth/login/aad/callback"
```

**Record:** `appId` (ClientId) and `id` (AppObjectId) — needed in all later stages.

**Idempotency note:** The script uses `--display-name` to look up an existing app
and upgrades it in-place.  Re-runs are safe.

---

## Stage 2 — Enable Implicit Grant ID Token Issuance

**Purpose:** Fix `AADSTS700054` — Easy Auth uses `response_type=code+id_token`
(hybrid flow).  Without this flag the browser sign-in stalls at the callback.

```powershell
az ad app update `
  --id <ClientId> `
  --enable-id-token-issuance true
```

**Verify:**
```powershell
az ad app show --id <ClientId> `
  --query "web.implicitGrantSettings.enableIdTokenIssuance"
# Expected: true
```

**Fallback (older az CLI):** The script patches via Graph REST API if the flag
is not supported by the installed CLI version.

---

## Stage 3 — Set Identifier URI (api://)

**Purpose:** Make the app a valid OAuth 2.0 resource so clients can request
tokens scoped to it with `api://<clientId>/.default`.

```powershell
az ad app update `
  --id <ClientId> `
  --identifier-uris "api://<ClientId>"
```

**Verify:**
```powershell
az ad app show --id <ClientId> --query "identifierUris"
# Expected: ["api://<ClientId>"]
```

---

## Stage 3b — Expose `user_impersonation` OAuth2 Scope

**Purpose:** Fix `AADSTS650057` — without at least one delegated scope, any attempt
to acquire a delegated token via `az account get-access-token --resource api://<clientId>`
or MSAL returns an empty resource error.

The script reads existing scopes and only PATCHes if `user_impersonation` is absent,
preserving the GUID of any previously-added scope so consents remain valid.

```powershell
# Via Graph REST (script handles this automatically)
PATCH https://graph.microsoft.com/v1.0/applications/<AppObjectId>
{
  "api": {
    "requestedAccessTokenVersion": 2,
    "oauth2PermissionScopes": [{
      "id": "<new-guid>",
      "value": "user_impersonation",
      "type": "User",
      "isEnabled": true,
      "adminConsentDisplayName": "Access <FunctionAppName>",
      "adminConsentDescription": "Allow the app to access <FunctionAppName> on behalf of the signed-in user."
    }]
  }
}
```

**Verify:**
```powershell
az ad app show --id <ClientId> --query "api.oauth2PermissionScopes[].value"
# Expected: ["user_impersonation"]
```

---

## Stage 4 — App Roles (Declarative Replace)

**Purpose:** Declare Warewolf group roles and permission roles on the Entra app
so they appear as assignable roles for users and applications.

**Groups to create (from `$GroupPermissions` hashtable):**

| Role value | Type | Description |
|---|---|---|
| `WarewolfAdministrators` | Group | Full admin access group |
| `PUBLIC` | Group | View-only public group |
| `EXECUTE` | Group | Execute-only group |
| `DEPLOY` | Group | Deployment group |
| `Permission.View` | Permission | Can view workflow results |
| `Permission.Execute` | Permission | Can trigger execution |
| `Permission.Contribute` | Permission | Can modify workflows |
| `Permission.DeployTo` | Permission | Can deploy to target |
| `Permission.DeployFrom` | Permission | Can pull from source |
| `Permission.Administrator` | Permission | Full control |

All roles: `allowedMemberTypes: ["User", "Application"]`

**Important:** The script preserves existing role GUIDs by looking up current
`appRoles` and matching by `value`, then reusing the ID.  This prevents
invalidating live `appRoleAssignment` records.

**Verify:**
```powershell
az ad app show --id <ClientId> --query "appRoles[].value"
```

---

## Stage 5 — Service Principal

**Purpose:** Create the service principal for the app registration.  The SP is
the tenant-level object that users are assigned to.

```powershell
az ad sp create --id <ClientId>
```

**Graph propagation delay:** The script uses exponential backoff retry (2→4→8→16→32→60s,
up to 6 attempts) because the SP may not be immediately visible after creation.

**Verify:**
```powershell
az ad sp show --id <ClientId> --query "id"
# Returns the SP objectId
```

---

## Stage 6 — User → Role Assignments

**Purpose:** Assign users to their group roles AND all `Permission.*` roles that
the group implies.

**Input shape (`$UserAssignments`):**
```powershell
$UserAssignments = @(
    @{ Upn = 'ashley.lewis@theunlimited.co.za'; Group = 'WarewolfAdministrators' },
    @{ Upn = 'aakash.gaikwad@theunlimited.co.za'; Group = 'PUBLIC' },
    @{ Upn = 'sehul.shah@theunlimited.co.za'; Group = 'EXECUTE' }
)
```

For each user, the script assigns:
1. The group role (e.g. `WarewolfAdministrators`)
2. Every `Permission.*` role listed in `$GroupPermissions` for that group

**Assignment API:**
```
POST https://graph.microsoft.com/v1.0/users/<userOid>/appRoleAssignments
{
  "principalId": "<userOid>",
  "resourceId": "<spObjectId>",
  "appRoleId": "<roleId>"
}
```

**Idempotency:** The script checks existing assignments keyed to the SP and
skips assignments that already exist.  Race-condition duplicates are caught
by the `InvalidUpdate` error code.

**Verify:**
```powershell
az ad user show --id ashley.lewis@theunlimited.co.za --query id -o tsv
# then:
az rest --method GET `
  --url "https://graph.microsoft.com/v1.0/users/<oid>/appRoleAssignments" `
  --query "value[?resourceId=='<spObjectId>'].appRoleId"
```

---

## Stage 7 — Client Secret

**Purpose:** Create a client secret for Easy Auth to authenticate to Entra
when exchanging authorization codes.

**Rotation triggers (the script auto-rotates when any is true):**
1. `-RotateSecret` flag passed
2. No existing credential with > 30 days remaining
3. `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` app setting is missing (old value unrecoverable)

```powershell
az ad app credential reset `
  --id <ClientId> `
  --display-name "easyauth-$(Get-Date -Format yyyyMMddHHmm)" `
  --end-date <yyyy-MM-dd> `
  --append
```

**`--append`** keeps existing credentials so non-expired secrets stay valid during
a rotation window.

**Security note:** The secret value is stored **only** in the Function App settings
as `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET`. It is never written to disk or logs.

---

## Stage 8 — Function App Settings

**Purpose:** Write all configuration the Function App needs at runtime.  Must
run before Stage 9 (Easy Auth references these settings by name).

```powershell
az functionapp config appsettings set `
  --name <FunctionAppName> `
  --resource-group <ResourceGroupName> `
  --settings `
    'WAREWOLF_ENTRA_CONFIG={"tenantId":"<tenantId>","audience":"api://<ClientId>"}' `
    "WAREWOLF_SECURE_CONFIG=D:\home\site\wwwroot\secure.config" `
    "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET=<clientSecret>"
```

WOLF-8516: `WAREWOLF_ENTRA_TENANT_ID`/`WAREWOLF_ENTRA_AUDIENCE`/`WAREWOLF_ENTRA_CLIENT_ID` were
merged into one `WAREWOLF_ENTRA_CONFIG` JSON app setting.

| Setting | Description |
|---|---|
| `WAREWOLF_ENTRA_CONFIG` | JSON — `tenantId` (Tenant GUID, used by `EntraAuthOptions` for bearer validation), `audience` (`api://<clientId>` — expected `aud` claim), optionally `clientId`/`serviceBusAudience` |
| `WAREWOLF_SECURE_CONFIG` | Full path to the encrypted `secure.config` file on the host |
| `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` | Easy Auth client secret reference |

---

## Stage 9 — Easy Auth Configuration

**Purpose:** Enable the Microsoft identity provider on Easy Auth V2, set the
correct issuer/audience, and configure token store.

### 9-pre: Migrate V1 → V2 if needed

```powershell
az webapp auth config-version show `
  --name <FunctionAppName> --resource-group <ResourceGroupName>

# If not "v2":
az webapp auth config-version upgrade `
  --name <FunctionAppName> --resource-group <ResourceGroupName> --yes
```

### 9a: Configure Microsoft provider

```powershell
az webapp auth microsoft update `
  --name <FunctionAppName> `
  --resource-group <ResourceGroupName> `
  --client-id <ClientId> `
  --client-secret-setting-name MICROSOFT_PROVIDER_AUTHENTICATION_SECRET `
  --issuer "https://login.microsoftonline.com/<TenantId>/v2.0" `
  --allowed-token-audiences "api://<ClientId>" `
  --yes
```

### 9b: Enable + set AllowAnonymous

```powershell
az webapp auth update `
  --name <FunctionAppName> `
  --resource-group <ResourceGroupName> `
  --enabled true `
  --action AllowAnonymous
```

> **Why `AllowAnonymous`?**  `/Public/*` workflows must be reachable without
> credentials.  `EasyAuthRedirectMiddleware` and `WorkflowAuthorizationMiddleware`
> enforce the boundary inside the worker process.

### 9c: Re-assert tokenStore via ARM PUT

The `az webapp auth` CLI does not reliably toggle `login.tokenStore.enabled`
across all authV2 extension versions, so the script reads the live config,
sets `login.tokenStore.enabled = true`, and PUTs the entire properties subtree
back.

```
PUT https://management.azure.com/subscriptions/<sub>/resourceGroups/<rg>/
    providers/Microsoft.Web/sites/<app>/config/authsettingsV2?api-version=2022-03-01
```

---

## Stage 10 — End-to-End Verification

The script verifies every critical property and throws if any fails:

| Check | Expected value |
|---|---|
| `platform.enabled` | `true` |
| `identityProviders.azureActiveDirectory.registration.clientId` | `<ClientId>` |
| `identityProviders.azureActiveDirectory.registration.openIdIssuer` | `https://login.microsoftonline.com/<TenantId>/v2.0` |
| `identityProviders.azureActiveDirectory.validation.allowedAudiences[0]` | `api://<ClientId>` |
| `globalValidation.unauthenticatedClientAction` | `AllowAnonymous` |
| `login.tokenStore.enabled` | `true` |
| `web.implicitGrantSettings.enableIdTokenIssuance` | `true` |
| `api.oauth2PermissionScopes` contains `user_impersonation` (enabled) | `true` |
| App settings `WAREWOLF_ENTRA_CONFIG` (tenantId/audience non-empty — WOLF-8516), `WAREWOLF_SECURE_CONFIG`, `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` | non-empty |
| Redirect URI `https://<app>.azurewebsites.net/.auth/login/aad/callback` | registered |

---

## Stage 11 — Smoke Tests (Optional)

```powershell
# Public workflow (expect 200)
Invoke-WebRequest -Uri "https://<app>.azurewebsites.net/Public/Hello%20World.json?Name=ping"

# Secure workflow without token (expect 401 or 302)
Invoke-WebRequest -Uri "https://<app>.azurewebsites.net/Secure/order.json?id=1" `
  -MaximumRedirection 0 -ErrorAction SilentlyContinue
```

---

## Generic Client App Provisioning

The provisioning script is not tied to fixed app names.  Client apps are
configured by **type**.  The following table shows the three canonical types:

### Type A — Interactive User App (SPA / Web App)

A client app whose users sign in interactively (Authorization Code + PKCE).

```powershell
# Register a new CLIENT app (separate from the resource app)
az ad app create `
  --display-name "my-angular-app" `
  --sign-in-audience AzureADMyOrg `
  --public-client-redirect-uris "http://localhost:4200" `
  --web-redirect-uris "https://my-angular-app.azurewebsites.net/auth/callback"

# Grant delegated permission to the resource app
az ad app permission add `
  --id <SpaClientId> `
  --api <ResourceAppId> `
  --api-permissions <user_impersonation-scope-id>=Scope

az ad app permission grant `
  --id <SpaClientId> `
  --api <ResourceAppId>
```

**Token contains:** user identity + app roles from the resource app.  
**Required `secure.config` entries:** per user or per group with `Execute = true`.

### Type B — Daemon / Service App (Client Credentials)

A headless service that authenticates as itself (no user context).

```powershell
az ad app create `
  --display-name "my-backend-service" `
  --sign-in-audience AzureADMyOrg

# Grant application permission (not delegated) to the resource app roles
az ad app permission add `
  --id <DaemonClientId> `
  --api <ResourceAppId> `
  --api-permissions <role-id>=Role   # e.g. Permission.Execute role id

az ad admin-consent grant --id <DaemonClientId>  # requires admin

# Assign the group role directly to the service principal
az ad sp create --id <DaemonClientId>
az rest --method POST `
  --url "https://graph.microsoft.com/v1.0/servicePrincipals/<daemonSpId>/appRoleAssignments" `
  --body '{ "principalId": "<daemonSpId>", "resourceId": "<resourceSpId>", "appRoleId": "<roleId>" }'
```

**Token contains:** service principal identity + `roles` claim.  
**Required `secure.config` entries:** entry with `WindowsGroup = "my-backend-service"` (or the SP objectId).

### Type C — On-Behalf-Of (Middle-Tier API)

A middle-tier .NET API that calls the Function App on behalf of a signed-in user.

```powershell
# The middle-tier is itself registered as a client app
az ad app create --display-name "my-api-tier"
# ... (similar to Type A but with both web and API permissions)

# The middle-tier exchanges its incoming token for a resource token using OBO
# This requires admin consent for the delegated permission on the resource app
```

**Token contains:** user identity relayed through the service.  
**Required `secure.config` entries:** per-user or per-group, same as Type A.

---

## `secure.config` Deployment

1. Create plain-text config following the shape in `Scripts/secure.config.example.json`.
2. Encrypt it with the Warewolf encryption tool:
   ```
   warewolf-encrypt secure.config.plain.json --out secure.config
   ```
3. Deploy to the Function App:
   - **Production:** `D:\home\site\wwwroot\secure.config` (set via `WAREWOLF_SECURE_CONFIG`)
   - **Local dev:** `bin\secure.config`
4. Verify `SecureConfigLoader` finds and loads it (check startup logs for
   `"WorkflowAuthPolicyLoader initialised with N workflow policies."`).

---

## Script Usage Reference

```powershell
# Full interactive setup
./Scripts/Configure-WwExecutionAuth-debug.ps1

# Rotate secret only
./Scripts/Configure-WwExecutionAuth-debug.ps1 -RotateSecret

# Skip user assignment (limited Graph permissions)
./Scripts/Configure-WwExecutionAuth-debug.ps1 -SkipUserAssignment

# CI/CD non-interactive
./Scripts/Configure-WwExecutionAuth-debug.ps1 `
  -NonInteractive `
  -SubscriptionId <id> `
  -TenantId <tid> `
  -ResourceGroupName DEV2 `
  -FunctionAppName wwexecution4 `
  -SkipSmokeTest

# Dry-run (print config and exit)
./Scripts/Configure-WwExecutionAuth-debug.ps1 -WhatIfOnly
```

---

## Troubleshooting Common Errors

| Error | Root cause | Fix |
|---|---|---|
| `AADSTS700054` | `enableIdTokenIssuance = false` | Run Stage 2 |
| `AADSTS650057: Invalid resource` | No `oauth2PermissionScopes` | Run Stage 3b |
| `AADSTS50011: Reply URL mismatch` | Redirect URI not registered | Run Stage 1 (re-assert URI) |
| `401` on `/Secure/*` with valid token | `WAREWOLF_ENTRA_CONFIG.audience` mismatch (WOLF-8516) | Check app setting vs token `aud` claim |
| `403` for known-good user | User not assigned to role | Run Stage 6 |
| `403` — no policy found (but warning, not error) | Workflow not in `secure.config` | Add `WindowsGroupPermissions` entry |
| Easy Auth V1 blocked V2 command | Legacy auth settings present | Run `az webapp auth config-version upgrade` |
| `401` on first deploy — `tokenStore.enabled = false` | ARM PUT not applied | Run Stage 9c manually |

---

## Generic Client App Patterns (PRV-12 / PRV-13 / PRV-14)

The wwexecution function app is *resource-only* in Entra terms — it exposes
the API and the app roles, but does **not** host any client.  Every calling
application registers itself as its own Entra app and consents to either the
`api://<resourceClientId>/user_impersonation` scope (delegated) or the
`Permission.*` app roles (app-only).

### PRV-12 — SPA / browser client app registration

```bash
az ad app create \
  --display-name "wwexecution-spa-prod" \
  --sign-in-audience AzureADMyOrg \
  --spa-redirect-uris "https://my-spa.contoso.com/auth/callback"

RESOURCE_APPID=<api://wwexecution clientId>
az ad app permission add \
  --id <spa-clientId> --api $RESOURCE_APPID \
  --api-permissions <user_impersonation-scopeId>=Scope
az ad app permission admin-consent --id <spa-clientId>
```

The SPA uses MSAL.js with `loginRedirect` / `acquireTokenSilent` (Part 5 § 5.3).

### PRV-13 — Confidential web app / API registration

```bash
az ad app create \
  --display-name "wwexecution-webapi-prod" \
  --sign-in-audience AzureADMyOrg \
  --web-redirect-uris "https://my-webapi.contoso.com/signin-oidc"

az ad app credential reset \
  --id <webapi-clientId> --years 1 \
  --display-name "rotation-$(Get-Date -Format yyyyMMdd)"

az ad app permission add \
  --id <webapi-clientId> --api $RESOURCE_APPID \
  --api-permissions <user_impersonation-scopeId>=Scope \
                    <Permission.Execute-roleId>=Role
az ad app permission admin-consent --id <webapi-clientId>
```

For OBO the confidential app additionally exposes its own API so its callers
can request its scope (Part 5 § 5.5).

### PRV-14 — Daemon / background-service registration

Prefer Managed Identity when the daemon runs in Azure — no secrets to rotate:

```bash
az functionapp identity assign --name <daemon-funcapp> --resource-group <rg>
DAEMON_OID=$(az functionapp identity show \
  --name <daemon-funcapp> --resource-group <rg> --query principalId -o tsv)

RESOURCE_SP_ID=$(az ad sp show --id $RESOURCE_APPID --query id -o tsv)
PERM_EXECUTE_ID=<Permission.Execute-roleId>

az rest --method post \
  --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$DAEMON_OID/appRoleAssignments" \
  --body "{\"principalId\":\"$DAEMON_OID\",\"resourceId\":\"$RESOURCE_SP_ID\",\"appRoleId\":\"$PERM_EXECUTE_ID\"}"
```

The daemon acquires tokens via `DefaultAzureCredential` /
`ManagedIdentityCredential` (Part 5 § 5.4).

### Cleanup contract

When a calling app is decommissioned its owning team deletes the Entra
registration.  `Cleanup-WwExecutionAuth.ps1` only removes resource-side
artefacts (the wwexecution registration, app roles, and app settings).
