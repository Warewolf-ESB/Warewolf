# WwExecution — Azure Entra ID + Easy Auth Setup
## AZ CLI Step-by-Step Runbook

> **How to use this guide**
> Run each block sequentially in a single PowerShell 7+ terminal session.
> Variables set in earlier steps are referenced by later steps — **do not close the terminal between steps**.
> Commands that produce IDs (client ID, object IDs, etc.) capture their output into variables automatically.

---

## Table of Contents

- [Prerequisites](#prerequisites)
- [Part 1 — Variable Definitions](#part-1--variable-definitions)
  - [1.1 Manual Input Variables](#11-manual-input-variables)
  - [1.2 Authenticate and Resolve Derived Variables](#12-authenticate-and-resolve-derived-variables)
  - [1.3 Application Variables](#13-application-variables)
  - [1.4 Role and Permission Definitions](#14-role-and-permission-definitions)
  - [1.5 Role → Permission Mapping](#15-role--permission-mapping)
  - [1.6 User Definitions](#16-user-definitions)
  - [1.7 User → Role Assignments](#17-user--role-assignments)
  - [1.8 Variable Summary](#18-variable-summary)
- [Part 2 — Step-by-Step Execution](#part-2--step-by-step-execution)
  - [Step 1 — Activate Subscription](#step-1--activate-subscription)
  - [Step 2 — App Registration](#step-2--app-registration)
  - [Step 3 — App Roles and Permissions](#step-3--app-roles-and-permissions)
  - [Step 4 — Client Secret](#step-4--client-secret)
  - [Step 5 — Service Principal](#step-5--service-principal)
  - [Step 6 — Create Users (Optional)](#step-6--create-users-optional)
  - [Step 7 — Assign Roles and Permissions to Users](#step-7--assign-roles-and-permissions-to-users)
  - [Step 8 — Configure Easy Auth](#step-8--configure-easy-auth)
  - [Step 9 — Set Function App Runtime Settings](#step-9--set-function-app-runtime-settings)
- [Part 3 — Verification](#part-3--verification)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

| Requirement | Minimum | Check |
|---|---|---|
| Azure CLI | 2.55+ | `az --version` |
| PowerShell | 7.0+ | `$PSVersionTable.PSVersion` |
| Entra ID role | Application Administrator | Azure Portal → Roles |
| Resource Group access | Contributor | `az role assignment list` |

```powershell
# Verify Azure CLI version
az --version

# Verify PowerShell version
$PSVersionTable.PSVersion

# Login to Azure (run this first — substitute your tenant ID)
az login --tenant "<your-tenant-id>"
```

---

## Part 1 — Variable Definitions

> **Run all blocks in Part 1 before moving to Part 2.**
> Some variables are set manually. Others are resolved automatically from Azure using `az` commands.

---

### 1.1 Manual Input Variables

These are the only values you need to supply by hand. Everything else is derived automatically.

```powershell
# ── REQUIRED: Replace all placeholder values ──────────────────────────────────

$TenantId       = "<your-tenant-id>"           # GUID — Azure AD → Overview → Tenant ID
$SubscriptionId = "<your-subscription-id>"     # GUID — Subscriptions blade
$ResourceGroup  = "<your-resource-group>"      # e.g. rg-wwexecution-prod


# ── PRE-FILLED: Change only if your setup differs ────────────────────────────

$FunctionAppName     = "wwexecution"
$SecretDisplayName   = "EasyAuth-Secret"
$SecretExpiryYears   = 2
$UserPassword        = "aaaAAA%%111"  # Must meet Azure AD complexity
                                                        # Min 8 chars, upper+lower+digit+symbol
                                                        # Leave as-is if not creating users
```

> ⚠️ **Security** — `$UserPassword` and `$ClientSecret` (set in Step 4) should be stored in
> Azure Key Vault or a secrets manager. Never commit them to source control.

---

### 1.2 Authenticate and Resolve Derived Variables

These values are read directly from Azure so you never have to look them up manually.

```powershell
# Set the active subscription for this session
az account set --subscription $SubscriptionId

# Resolve tenant primary domain (used to build user UPNs)
$TenantDomain = (az ad signed-in-user show --query userPrincipalName --output tsv).Split("@")[1]

# Resolve the currently signed-in user (for audit/verification)
$SignedInUser = (az ad signed-in-user show --query userPrincipalName --output tsv)

Write-Host "Tenant domain  : $TenantDomain"
Write-Host "Signed in as   : $SignedInUser"
Write-Host "Subscription   : $SubscriptionId"
```

> 💡 **Derived URLs** — built from `$TenantId` and `$FunctionAppName` set in 1.1.

```powershell
# Derived — do not edit
$AppDisplayName = $FunctionAppName
$IdentifierUri  = "api://$FunctionAppName"
$ReplyUrl       = "https://$FunctionAppName.azurewebsites.net/.auth/login/aad/callback"
$IssuerUrl      = "https://login.microsoftonline.com/$TenantId/v2.0"

Write-Host "Identifier URI : $IdentifierUri"
Write-Host "Reply URL      : $ReplyUrl"
Write-Host "Issuer URL     : $IssuerUrl"
```

---

### 1.3 Application Variables

> These variables (`$AppObjectId`, `$ClientId`, `$SpObjectId`, `$ClientSecret`) are populated
> automatically during execution in Part 2. They are declared here as empty placeholders
> so the variable summary in Section 1.8 shows the full picture.

```powershell
# Populated automatically in Step 2 (App Registration)
$AppObjectId  = $null

# Populated automatically in Step 2 (App Registration)
$ClientId     = $null

# Populated automatically in Step 4 (Client Secret)
$ClientSecret = $null

# Populated automatically in Step 5 (Service Principal)
$SpObjectId   = $null
```

---

### 1.4 Role and Permission Definitions

Define the roles and permissions that will be created as Entra ID App Roles.

```powershell
# ── ROLES — broad access groups ───────────────────────────────────────────────
# Each role becomes an App Role on the registration.
# 'value' must be alphanumeric + dots only (no spaces).

$RoleDefinitions = @(
    @{
        displayName = "Warewolf Administrators"
        value       = "WarewolfAdministrators"
        description = "Full administrative access to all workflows"
    },
    @{
        displayName = "PUBLIC"
        value       = "PUBLIC"
        description = "Public access group — used for anonymous-passthrough routes"
    }
)

# ── PERMISSIONS — fine-grained capabilities ───────────────────────────────────
# Stored as 'Permission.<n>' app roles so middleware can distinguish them
# from broad roles at runtime.

$PermissionDefinitions = @(
    @{
        displayName = "Permission View"
        value       = "Permission.View"
        description = "Can view workflow outputs and status"
    },
    @{
        displayName = "Permission Execute"
        value       = "Permission.Execute"
        description = "Can trigger workflow execution"
    },
    @{
        displayName = "Permission Contribute"
        value       = "Permission.Contribute"
        description = "Can create and modify workflow definitions"
    },
    @{
        displayName = "Permission DeployTo"
        value       = "Permission.DeployTo"
        description = "Can deploy workflows to a target environment"
    },
    @{
        displayName = "Permission DeployFrom"
        value       = "Permission.DeployFrom"
        description = "Can pull workflow deployments from a source environment"
    },
    @{
        displayName = "Permission Administrator"
        value       = "Permission.Administrator"
        description = "Full permission over all workflow operations"
    }
)

# Combined list used when building the app roles JSON payload
$AllRoleSpecs = @($RoleDefinitions) + @($PermissionDefinitions)

Write-Host "Roles defined      : $($RoleDefinitions.Count)"
Write-Host "Permissions defined: $($PermissionDefinitions.Count)"
Write-Host "Total app roles    : $($AllRoleSpecs.Count)"
```

---

### 1.5 Role → Permission Mapping

Defines which permissions each role grants. When a user is assigned a role in Step 7,
all permissions mapped to that role are also assigned automatically.

```powershell
# Keys   = role displayName (must match $RoleDefinitions[].displayName exactly)
# Values = array of permission displayNames (must match $PermissionDefinitions[].displayName)
#
# To grant ALL permissions to a role, list every permission value.
# To grant NO permissions, set the value to @() (empty array).

$RolePermissionMap = @{

    "Warewolf Administrators" = @(
        "Permission View",
        "Permission Execute",
        "Permission Contribute",
        "Permission DeployTo",
        "Permission DeployFrom",
        "Permission Administrator"
    )

    "PUBLIC" = @()   # No permissions — anonymous passthrough only
}

# Verify every role has an entry
foreach ($role in $RoleDefinitions) {
    if (-not $RolePermissionMap.ContainsKey($role.displayName)) {
        Write-Warning "Role '$($role.displayName)' has no mapping — it will receive no permissions."
    } else {
        $count = $RolePermissionMap[$role.displayName].Count
        Write-Host "Role '$($role.displayName)' → $count permission(s)"
    }
}
```

---

### 1.6 User Definitions

Define users to create in Entra ID. Set `$CreateUsers = $false` to skip user creation entirely.

```powershell
# Set to $false to skip all user creation (Step 6 becomes a no-op)
$CreateUsers = $true

# List of users to create. UPN is built as: $alias@$TenantDomain
# If the user already exists, the script reuses them (idempotent).
$UsersToCreate = @(
    @{ alias = "wwexecutionuser"; displayName = "Warewolf Execution Engine User"   }
)

# Resolved user Object IDs — populated automatically in Step 6
$ResolvedUserIds = @{}   # alias → Entra Object ID

Write-Host "Users to create: $($UsersToCreate.Count) (CreateUsers=$CreateUsers)"
```

---

### 1.7 User → Role Assignments

Defines which roles each user receives. Roles must match `$RoleDefinitions[].displayName`.
Permissions are assigned automatically based on the mapping in Section 1.5.

```powershell
# Keys   = user alias (must match $UsersToCreate[].alias or be an existing UPN)
# Values = array of role displayNames (must match $RoleDefinitions[].displayName)

$UserRoleAssignments = @{

    "alice" = @(
        "Warewolf Administrators"   # alice gets admin role + all its permissions
    )

    "bob"   = @(
        "PUBLIC"                    # bob gets PUBLIC role (no permissions)
    )

    "carol" = @(
        "Warewolf Administrators",  # carol gets both roles
        "PUBLIC"
    )
}


Write-Host "Users with role assignments: $($UserRoleAssignments.Count)"
```

---

### 1.8 Variable Summary

Run this block to print a complete overview of all variables before executing Part 2.
This is your final review checkpoint — **no Azure changes have been made yet**.

```powershell
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  VARIABLE SUMMARY — Review before running Part 2             " -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan

Write-Host ""
Write-Host "  ── Azure Target ──────────────────────────────────────────────" -ForegroundColor White
Write-Host ("  {0,-26} {1}" -f "Tenant ID:",       $TenantId)
Write-Host ("  {0,-26} {1}" -f "Subscription ID:", $SubscriptionId)
Write-Host ("  {0,-26} {1}" -f "Resource Group:",  $ResourceGroup)
Write-Host ("  {0,-26} {1}" -f "Tenant Domain:",   $TenantDomain)
Write-Host ("  {0,-26} {1}" -f "Signed In As:",    $SignedInUser)

Write-Host ""
Write-Host "  ── Application ───────────────────────────────────────────────" -ForegroundColor White
Write-Host ("  {0,-26} {1}" -f "Function App:",    $FunctionAppName)
Write-Host ("  {0,-26} {1}" -f "App Display Name:",$AppDisplayName)
Write-Host ("  {0,-26} {1}" -f "Identifier URI:",  $IdentifierUri)
Write-Host ("  {0,-26} {1}" -f "Reply URL:",       $ReplyUrl)
Write-Host ("  {0,-26} {1}" -f "Issuer URL:",      $IssuerUrl)
Write-Host ("  {0,-26} {1}" -f "Secret Label:",    $SecretDisplayName)
Write-Host ("  {0,-26} {1}" -f "Secret Expiry:",   "$SecretExpiryYears year(s)")

Write-Host ""
Write-Host "  ── Runtime IDs (set during Part 2) ──────────────────────────" -ForegroundColor White
Write-Host ("  {0,-26} {1}" -f "App Object ID:",   ($AppObjectId  ?? "(set in Step 2)"))
Write-Host ("  {0,-26} {1}" -f "Client ID:",       ($ClientId     ?? "(set in Step 2)"))
Write-Host ("  {0,-26} {1}" -f "Client Secret:",   ($ClientSecret ?? "(set in Step 4)"))
Write-Host ("  {0,-26} {1}" -f "SP Object ID:",    ($SpObjectId   ?? "(set in Step 5)"))

Write-Host ""
Write-Host "  ── Roles ($($RoleDefinitions.Count)) ──────────────────────────────────────────" -ForegroundColor White
foreach ($r in $RoleDefinitions) {
    Write-Host ("  · {0,-30} value: {1}" -f $r.displayName, $r.value)
}

Write-Host ""
Write-Host "  ── Permissions ($($PermissionDefinitions.Count)) ────────────────────────────────" -ForegroundColor White
foreach ($p in $PermissionDefinitions) {
    Write-Host ("  · {0,-30} value: {1}" -f $p.displayName, $p.value)
}

Write-Host ""
Write-Host "  ── Role → Permission Mapping ─────────────────────────────────" -ForegroundColor White
foreach ($roleName in $RolePermissionMap.Keys) {
    $perms = $RolePermissionMap[$roleName]
    if ($perms.Count -eq 0) {
        Write-Host ("  · {0,-30} → (no permissions)" -f $roleName) -ForegroundColor DarkGray
    } else {
        Write-Host ("  · {0,-30} →" -f $roleName) -ForegroundColor White
        foreach ($p in $perms) { Write-Host ("      - $p") -ForegroundColor Gray }
    }
}

Write-Host ""
Write-Host "  ── Users to Create ($($UsersToCreate.Count), CreateUsers=$CreateUsers) ────────────────────" -ForegroundColor White
foreach ($u in $UsersToCreate) {
    Write-Host ("  · {0,-20} UPN: {1}@{2}" -f $u.displayName, $u.alias, $TenantDomain)
}

Write-Host ""
Write-Host "  ── User → Role Assignments ($($UserRoleAssignments.Count) user(s)) ──────────────────────" -ForegroundColor White
foreach ($alias in $UserRoleAssignments.Keys) {
    $roles = $UserRoleAssignments[$alias]
    Write-Host ("  · {0,-20} → {1}" -f $alias, ($roles -join ", "))
    foreach ($roleName in $roles) {
        $perms = $RolePermissionMap[$roleName]
        if ($perms.Count -gt 0) {
            Write-Host ("      permissions via '$roleName': {0}" -f ($perms -join ", ")) -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  No Azure changes made yet. Proceed to Part 2 when ready.    " -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
```

---

## Part 2 — Step-by-Step Execution

> Each step shows:
> 1. **What it does** — purpose and effect
> 2. **Variables used** — which variables from Part 1 are referenced
> 3. **The command** — ready to run as-is
> 4. **Variables produced** — what gets set for use in later steps

---

### Step 1 — Activate Subscription

**What it does:** Sets the active Azure subscription for all subsequent `az` commands in this
session and verifies the tenant matches what you configured.

**Variables used:** `$SubscriptionId`, `$TenantId`

```powershell
# Set active subscription
az account set --subscription $SubscriptionId

# Verify subscription and tenant
$AccountInfo = az account show --output json | ConvertFrom-Json

Write-Host "Subscription : $($AccountInfo.name) ($($AccountInfo.id))"
Write-Host "Tenant       : $($AccountInfo.tenantId)"

if ($AccountInfo.tenantId -ne $TenantId) {
    throw "Tenant mismatch. Expected '$TenantId', got '$($AccountInfo.tenantId)'. " +
          "Run: az login --tenant $TenantId"
}

Write-Host "✓ Subscription and tenant verified." -ForegroundColor Green
```

**Variables produced:** none (verification only)

---

### Step 2 — App Registration

**What it does:** Creates (or reuses) the Entra ID app registration that represents the
`wwexecution` function app. Captures the **App Object ID** and **Client ID** used by all
subsequent steps.

**Variables used:** `$AppDisplayName`, `$ReplyUrl`, `$IdentifierUri`

> ⚠️ **Identifier URI note:** On create, `$IdentifierUri` is set to `api://$ClientId` (using the
> real Client ID just assigned by Entra) — this is always accepted. The Part 1 placeholder
> `api://$FunctionAppName` is used only on reuse where the URI already exists. Both values are
> valid; `api://$ClientId` is the canonical form recommended by Microsoft.

```powershell
# Check whether the registration already exists
$ExistingApp = az ad app list `
    --display-name $AppDisplayName `
    --query "[0]" `
    --output json | ConvertFrom-Json

if ($ExistingApp) {
    # ── REUSE PATH — registration already exists ──────────────────────────────
    Write-Host "App '$AppDisplayName' already exists — reusing." -ForegroundColor Yellow

    $AppObjectId = $ExistingApp.id
    $ClientId    = $ExistingApp.appId

} else {
    # ── CREATE PATH — registration does not exist ─────────────────────────────
    Write-Host "Creating app registration '$AppDisplayName'..."

    # Step 1 — Create the app WITHOUT --identifier-uris
    $NewApp = az ad app create `
        --display-name     $AppDisplayName `
        --sign-in-audience AzureADMyOrg `
        --web-redirect-uris $ReplyUrl `
        --output json | ConvertFrom-Json

    $AppObjectId = $NewApp.id
    $ClientId    = $NewApp.appId

    # Step 2 — Set the identifier URI using the real Client ID (always accepted by Entra)
    # NOTE: $IdentifierUri is updated here to api://$ClientId which is the canonical form.
    #       The Part 1 value api://$FunctionAppName is superseded by this assignment.
    $IdentifierUri = "api://$ClientId"

    az ad app update `
        --id              $AppObjectId `
        --identifier-uris $IdentifierUri

    Write-Host "App Object ID  : $AppObjectId"
    Write-Host "Client ID      : $ClientId"
    Write-Host "Identifier URI : $IdentifierUri"

    Write-Host "✓ App registration created." -ForegroundColor Green
}

Write-Host ""
Write-Host "App Object ID : $AppObjectId"
Write-Host "Client ID     : $ClientId"
```

> **Reuse vs Create:** Run the block as-is — it checks for an existing registration first.
> If one exists (e.g. from a previous run), it reuses it and prints a yellow notice.
> If not, it creates a new one.

**Variables produced:** `$AppObjectId`, `$ClientId`

---

### Step 3 — App Roles and Permissions

**What it does:** Reads the current app roles from the registration, merges in any new roles
and permissions from your definitions, then writes the complete updated array back to Entra ID.
Existing roles are always preserved — this operation is safe to re-run.

**Variables used:** `$AppObjectId`, `$AllRoleSpecs`

```powershell
# 1. Read current app roles from Entra ID
$CurrentApp  = az ad app show --id $AppObjectId --output json | ConvertFrom-Json
$CurrentVals = @($CurrentApp.appRoles | ForEach-Object { $_.value })

Write-Host "Existing app roles on registration: $($CurrentVals.Count)"

# 2. Start payload with all existing roles (Entra replaces the full array on PATCH)
$Payload = [System.Collections.Generic.List[hashtable]]::new()

foreach ($existing in $CurrentApp.appRoles) {
    $Payload.Add(@{
        id                 = $existing.id
        displayName        = $existing.displayName
        value              = $existing.value
        description        = $existing.description
        allowedMemberTypes = @($existing.allowedMemberTypes)
        isEnabled          = [bool]$existing.isEnabled
    })
}

# 3. Append new roles and permissions — skip any that already exist
foreach ($spec in $AllRoleSpecs) {
    if ($CurrentVals -contains $spec.value) {
        Write-Host "  Already exists: $($spec.value) — skipping" -ForegroundColor DarkGray
        continue
    }
    $Payload.Add(@{
        id                 = [System.Guid]::NewGuid().ToString()
        displayName        = $spec.displayName
        value              = $spec.value
        description        = $spec.description
        allowedMemberTypes = @("User", "Application")
        isEnabled          = $true
    })
    Write-Host "  Queued: $($spec.value)" -ForegroundColor Cyan
}

# 4. Write payload to temp file (avoids shell quoting issues with large JSON)
$TmpRolesFile = [System.IO.Path]::GetTempFileName() + ".json"
$Payload | ConvertTo-Json -Depth 5 -Compress | Set-Content -Path $TmpRolesFile -Encoding UTF8

# 5. PATCH the app registration with the complete role array
az ad app update `
    --id        $AppObjectId `
    --app-roles "@$TmpRolesFile"

# 6. Clean up
Remove-Item $TmpRolesFile -Force

Write-Host ""
Write-Host "✓ App roles committed. Total roles on registration: $($Payload.Count)" -ForegroundColor Green

# 7. Verify — list all roles now on the registration
Write-Host ""
Write-Host "Roles now on the registration:"
az ad app show `
    --id $AppObjectId `
    --query "appRoles[].{Name:displayName, Value:value, Enabled:isEnabled}" `
    --output table
```

**Variables produced:** none (app registration updated in Entra ID)

---

### Step 4 — Client Secret

**What it does:** Generates a new client secret on the app registration. Easy Auth uses this
credential to authenticate against Entra ID when validating tokens.

> ⚠️ **The secret value is shown only once. Copy `$ClientSecret` to a secure vault immediately.**

**Variables used:** `$AppObjectId`, `$SecretDisplayName`, `$SecretExpiryYears`

```powershell
$SecretJson = az ad app credential reset `
    --id           $AppObjectId `
    --display-name $SecretDisplayName `
    --years        $SecretExpiryYears `
    --output json | ConvertFrom-Json

$ClientSecret = $SecretJson.password

Write-Host ""
Write-Host "✓ Client secret created ($SecretExpiryYears-year expiry)." -ForegroundColor Green
Write-Host ""
Write-Host "  ┌─────────────────────────────────────────────────────" -ForegroundColor Yellow
Write-Host "  │  SAVE NOW — this value cannot be retrieved later    " -ForegroundColor Yellow
Write-Host "  │  CLIENT_SECRET = $ClientSecret" -ForegroundColor Yellow
Write-Host "  └─────────────────────────────────────────────────────" -ForegroundColor Yellow
```

**Variables produced:** `$ClientSecret`

---

### Step 5 — Service Principal

**What it does:** Creates (or retrieves) the Enterprise Application (service principal) object
in Entra ID. The service principal is what users are actually assigned to. It must exist before
role assignments can be made.

**Variables used:** `$ClientId`, `$AppObjectId`

```powershell
# Check whether the service principal already exists
$ExistingSp = az ad sp list `
    --filter  "appId eq '$ClientId'" `
    --query   "[0]" `
    --output  json | ConvertFrom-Json

if ($ExistingSp) {
    # ── REUSE PATH ────────────────────────────────────────────────────────────
    Write-Host "Service principal already exists — reusing." -ForegroundColor Yellow
    $SpObjectId = $ExistingSp.id

} else {
    # ── CREATE PATH ───────────────────────────────────────────────────────────
    Write-Host "Creating service principal..."

    $NewSp      = az ad sp create --id $ClientId --output json | ConvertFrom-Json
    $SpObjectId = $NewSp.id

    Write-Host "✓ Service principal created." -ForegroundColor Green
}

Write-Host "SP Object ID: $SpObjectId"

# Wait for Entra ID replication before building the role ID map
Write-Host "Waiting 10 seconds for Entra ID replication..."
Start-Sleep -Seconds 10

# Build role-value → role-ID lookup from the updated registration
$RefreshedApp = az ad app show --id $AppObjectId --output json | ConvertFrom-Json
$RoleIdMap    = @{}
foreach ($r in $RefreshedApp.appRoles) {
    $RoleIdMap[$r.value] = $r.id
}

Write-Host "✓ Role ID map built ($($RoleIdMap.Count) entries)." -ForegroundColor Green
```

**Variables produced:** `$SpObjectId`, `$RoleIdMap`

---

### Step 6 — Create Users (Optional)

**What it does:** Creates cloud-only Entra ID user accounts. Skipped entirely if
`$CreateUsers = $false`. Existing users are detected and reused — this step is safe to re-run.

**Variables used:** `$CreateUsers`, `$UsersToCreate`, `$TenantDomain`, `$UserPassword`

```powershell
if (-not $CreateUsers) {
    Write-Host "User creation skipped (CreateUsers=false)." -ForegroundColor DarkGray

} else {
    Write-Host "Creating $($UsersToCreate.Count) user(s)..."
    Write-Host ""

    foreach ($userSpec in $UsersToCreate) {
        $upn = "$($userSpec.alias)@$TenantDomain"

        # Check if user already exists
        $existing = az ad user list `
            --upn     $upn `
            --query   "[0]" `
            --output  json | ConvertFrom-Json

        if ($existing) {
            # ── REUSE PATH ────────────────────────────────────────────────────
            Write-Host "  User '$upn' already exists — reusing." -ForegroundColor Yellow
            $ResolvedUserIds[$userSpec.alias] = $existing.id

        } else {
            # ── CREATE PATH ───────────────────────────────────────────────────
            $newUser = az ad user create `
                --display-name                       $userSpec.displayName `
                --user-principal-name                $upn `
                --password                           $UserPassword `
                --force-change-password-next-sign-in false `
                --output json | ConvertFrom-Json

            $ResolvedUserIds[$userSpec.alias] = $newUser.id
            Write-Host "  ✓ Created: $upn  (OID: $($newUser.id))" -ForegroundColor Green
        }
    }

    Write-Host ""
    Write-Host "User Object IDs resolved:"
    foreach ($alias in $ResolvedUserIds.Keys) {
        Write-Host ("  {0,-20} {1}" -f $alias, $ResolvedUserIds[$alias])
    }
}
```

**Variables produced:** `$ResolvedUserIds` (hash: alias → Object ID)

---

### Step 7 — Assign Roles and Permissions to Users

**What it does:** Assigns each user their configured roles and all permissions mapped to those
roles via the Microsoft Graph `appRoleAssignments` endpoint. Duplicate assignments (HTTP 409)
are silently skipped — safe to re-run.

**Variables used:** `$UserRoleAssignments`, `$RolePermissionMap`, `$RoleDefinitions`,
`$PermissionDefinitions`, `$RoleIdMap`, `$SpObjectId`, `$ResolvedUserIds`, `$TenantDomain`

```powershell
# Helper function — posts a single app role assignment via Microsoft Graph
function Invoke-AppRoleAssignment {
    param(
        [string] $UserId,
        [string] $AppRoleId,
        [string] $RoleValue,
        [string] $UserLabel
    )

    $body = @{
        principalId = $UserId     # User object ID
        resourceId  = $SpObjectId # Service principal object ID
        appRoleId   = $AppRoleId  # GUID of the specific app role
    } | ConvertTo-Json -Compress

    $tmpFile = [System.IO.Path]::GetTempFileName() + ".json"
    Set-Content -Path $tmpFile -Value $body -Encoding UTF8

    try {
        az rest `
            --method  POST `
            --uri     "https://graph.microsoft.com/v1.0/users/$UserId/appRoleAssignments" `
            --headers "Content-Type=application/json" `
            --body    "@$tmpFile" | Out-Null

        Write-Host "    ✓ Assigned '$RoleValue'" -ForegroundColor Green

    } catch {
        if ($_ -match "already exists|AlreadyExists|Permission being assigned") {
            Write-Host "    · '$RoleValue' already assigned — skipping." -ForegroundColor DarkGray
        } else {
            Write-Warning "    ⚠ Failed to assign '$RoleValue': $($_.Exception.Message)"
        }
    } finally {
        Remove-Item $tmpFile -Force -ErrorAction SilentlyContinue
    }
}

# ── Process each user ──────────────────────────────────────────────────────────

if ($UserRoleAssignments.Count -eq 0) {
    Write-Host "No user-role assignments configured — skipping." -ForegroundColor DarkGray

} else {
    foreach ($userAlias in $UserRoleAssignments.Keys) {

        # Resolve the user's Entra Object ID
        if ($ResolvedUserIds.ContainsKey($userAlias)) {
            $userId = $ResolvedUserIds[$userAlias]
        } else {
            $upn  = ($userAlias -match "@") ? $userAlias : "$userAlias@$TenantDomain"
            $user = az ad user list --upn $upn --query "[0]" --output json | ConvertFrom-Json

            if (-not $user) {
                Write-Warning "User '$userAlias' not found in Entra ID — skipping all assignments."
                continue
            }
            $userId = $user.id
            $ResolvedUserIds[$userAlias] = $userId
        }

        Write-Host ""
        Write-Host "  User: $userAlias  ($userId)" -ForegroundColor Cyan

        foreach ($roleName in $UserRoleAssignments[$userAlias]) {

            Write-Host "  Role: $roleName" -ForegroundColor White

            # Find the role's value token from $RoleDefinitions
            $roleDef   = $RoleDefinitions | Where-Object { $_.displayName -eq $roleName } | Select-Object -First 1
            $roleValue = $roleDef ? $roleDef.value : ($roleName -replace '[^a-zA-Z0-9.]', '')
            $roleId    = $RoleIdMap[$roleValue]

            if ($roleId) {
                Invoke-AppRoleAssignment -UserId $userId -AppRoleId $roleId `
                    -RoleValue $roleValue -UserLabel $userAlias
            } else {
                Write-Warning "  Role value '$roleValue' not found in RoleIdMap — skipping."
            }

            # Assign all permissions mapped to this role
            $mappedPerms = $RolePermissionMap[$roleName]

            if ($mappedPerms -and $mappedPerms.Count -gt 0) {
                Write-Host "  Permissions via '$roleName':" -ForegroundColor White

                foreach ($permDisplayName in $mappedPerms) {
                    # Find the permission's value token from $PermissionDefinitions
                    $permDef   = $PermissionDefinitions | Where-Object { $_.displayName -eq $permDisplayName } | Select-Object -First 1
                    $permValue = $permDef ? $permDef.value : "Permission.$($permDisplayName -replace '[^a-zA-Z0-9.]', '')"
                    $permId    = $RoleIdMap[$permValue]

                    if ($permId) {
                        Invoke-AppRoleAssignment -UserId $userId -AppRoleId $permId `
                            -RoleValue $permValue -UserLabel $userAlias
                    } else {
                        Write-Warning "    Permission '$permValue' not in RoleIdMap — skipping."
                    }
                }
            } else {
                Write-Host "    (no permissions mapped to this role)" -ForegroundColor DarkGray
            }
        }
    }

    Write-Host ""
    Write-Host "✓ All role and permission assignments complete." -ForegroundColor Green
}
```

**Variables produced:** none (assignments written to Entra ID)

---

### Step 8 — Configure Easy Auth

**What it does:** Enables App Service Authentication (Easy Auth) on the Function App with the
Microsoft identity provider, then sets the unauthenticated request policy to
`AllowAnonymousRequests` so `/Public/*` routes remain accessible without a token.

**Variables used:** `$FunctionAppName`, `$ResourceGroup`, `$ClientId`, `$ClientSecret`,
`$IssuerUrl`, `$SubscriptionId`, `$TenantId`

> **Why AllowAnonymousRequests?**
> The `WorkflowAuthorizationMiddleware` in the function code enforces per-route security.
> Setting Easy Auth to reject unauthenticated requests at the platform level would block
> `/Public/*` routes before they reach the middleware.

---

#### 8a — Configure the Microsoft Identity Provider

> **`az webapp auth` is the correct command for Function Apps.**
> `az functionapp auth` does not exist — the `authV2` extension registers all auth commands
> under the `az webapp auth` namespace. Both Function Apps and Web Apps share the same
> underlying ARM resource type (`Microsoft.Web/sites`), so `az webapp auth` applies to both.

```powershell
# ── Primary: az webapp auth microsoft update (authV2, targets authsettingsV2) ─
Write-Host "Configuring Microsoft identity provider on '$FunctionAppName'..."

az webapp auth microsoft update `
    --name           $FunctionAppName `
    --resource-group $ResourceGroup `
    --client-id      $ClientId `
    --client-secret  $ClientSecret `
    --issuer         $IssuerUrl `
    --yes

Write-Host "✓ Microsoft identity provider configured." -ForegroundColor Green
```

> ⚠️ **`az webapp auth microsoft update` limitations:** This command sets the AAD provider
> registration fields only. It does **not** configure `globalValidation`,
> `login.tokenStore`, `httpSettings`, or remove `redirectToProvider`. If you need full
> control over all these fields (e.g. `AllowAnonymousRequests`, token store TTL, forward
> proxy), use the **az rest full-config alternative** in Step 8a-alt below instead of this
> command — they target the same resource and should not both be run.

---

#### 8a-alt — Full Config via az rest (Recommended — use instead of 8a when full control is needed)

This alternative reads the current config from Azure, patches only the fields we own, and
writes the complete payload back. It is the only reliable way to set all Easy Auth V2 fields
in a single operation without losing Azure-managed settings.

```powershell
# ════════════════════════════════════════════════════════════════════════════════
# STEP 8a-alt — Configure Easy Auth via az rest (full authsettingsV2 control)
# ════════════════════════════════════════════════════════════════════════════════

# ── Guard: verify all required variables are set ─────────────────────────────
$requiredVars = @{
    'SubscriptionId'  = $SubscriptionId
    'ResourceGroup'   = $ResourceGroup
    'FunctionAppName' = $FunctionAppName
    'ClientId'        = $ClientId
    'ClientSecret'    = $ClientSecret
    'IssuerUrl'       = $IssuerUrl
    'TenantId'        = $TenantId
}

foreach ($var in $requiredVars.GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace($var.Value)) {
        throw "`$$($var.Key) is empty. Re-run Part 1 variable blocks before continuing."
    }
}

Write-Host "✓ All required variables are set." -ForegroundColor Green

# ── Build the ARM URI ────────────────────────────────────────────────────────
$authUri = "https://management.azure.com/subscriptions/$SubscriptionId"  +
           "/resourceGroups/$ResourceGroup"                               +
           "/providers/Microsoft.Web/sites/$FunctionAppName"              +
           "/config/authsettingsV2"                                       +
           "?api-version=2022-03-01"

Write-Host "URI: $authUri"

# ── Step 8.1: Read current config from Azure ─────────────────────────────────
# Reading first preserves all Azure-managed fields (facebook, google, twitter,
# httpSettings, cookie settings, nonce, tags) that a fresh payload would wipe.
Write-Host ""
Write-Host "Reading current Easy Auth config from Azure..."

$currentConfig = az rest `
    --method GET `
    --uri    $authUri `
    --output json | ConvertFrom-Json

Write-Host "✓ Current config loaded." -ForegroundColor Green

# ── Step 8.2: Patch only the fields we own ───────────────────────────────────

# 8.2a — Platform: enable Easy Auth at the platform level
$currentConfig.properties.platform.enabled        = $true
$currentConfig.properties.platform.runtimeVersion = "~1"

# 8.2b — Global validation: allow anonymous requests through to middleware.
#         requireAuthentication and unauthenticatedClientAction are coupled —
#         both must be set together or Azure reverts the action to RedirectToLoginPage.
#         Also remove redirectToProvider if present — it overrides the action.
$currentConfig.properties.globalValidation.requireAuthentication       = $false
$currentConfig.properties.globalValidation.unauthenticatedClientAction = "AllowAnonymousRequests"

if ($currentConfig.properties.globalValidation.PSObject.Properties['redirectToProvider']) {
    $currentConfig.properties.globalValidation.PSObject.Properties.Remove('redirectToProvider')
    Write-Host "  · Removed 'redirectToProvider' override from globalValidation." -ForegroundColor DarkGray
}

# 8.2c — Azure Active Directory provider
$currentConfig.properties.identityProviders.azureActiveDirectory.enabled = $true

$currentConfig.properties.identityProviders.azureActiveDirectory.registration.clientId                = $ClientId
$currentConfig.properties.identityProviders.azureActiveDirectory.registration.clientSecretSettingName = "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
$currentConfig.properties.identityProviders.azureActiveDirectory.registration.openIdIssuer            = $IssuerUrl

$currentConfig.properties.identityProviders.azureActiveDirectory.validation.allowedAudiences = @(
    "api://$ClientId"
)

$currentConfig.properties.identityProviders.azureActiveDirectory.login.disableWWWAuthenticate = $false

# 8.2d — Token store
$currentConfig.properties.login.tokenStore.enabled                    = $true
$currentConfig.properties.login.tokenStore.tokenRefreshExtensionHours = 72.0

# 8.2e — HTTP settings
$currentConfig.properties.httpSettings.requireHttps            = $true
$currentConfig.properties.httpSettings.forwardProxy.convention = "NoProxy"
$currentConfig.properties.httpSettings.routes.apiPrefix        = "/.auth"

# ── Step 8.3: Preview all patched field values before sending ─────────────────
Write-Host ""
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Payload Preview — Fields Being Applied             " -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "  platform" -ForegroundColor White
Write-Host ("    {0,-44} {1}" -f "enabled:",        $currentConfig.properties.platform.enabled)
Write-Host ("    {0,-44} {1}" -f "runtimeVersion:", $currentConfig.properties.platform.runtimeVersion)
Write-Host ""
Write-Host "  globalValidation" -ForegroundColor White
Write-Host ("    {0,-44} {1}" -f "requireAuthentication:",       $currentConfig.properties.globalValidation.requireAuthentication)
Write-Host ("    {0,-44} {1}" -f "unauthenticatedClientAction:", $currentConfig.properties.globalValidation.unauthenticatedClientAction)
Write-Host ("    {0,-44} {1}" -f "redirectToProvider:",          ($currentConfig.properties.globalValidation.PSObject.Properties['redirectToProvider'] ? $currentConfig.properties.globalValidation.redirectToProvider : "(removed)"))
Write-Host ""
Write-Host "  identityProviders.azureActiveDirectory" -ForegroundColor White
Write-Host ("    {0,-44} {1}" -f "enabled:",                 $currentConfig.properties.identityProviders.azureActiveDirectory.enabled)
Write-Host ("    {0,-44} {1}" -f "clientId:",                $currentConfig.properties.identityProviders.azureActiveDirectory.registration.clientId)
Write-Host ("    {0,-44} {1}" -f "clientSecretSettingName:", $currentConfig.properties.identityProviders.azureActiveDirectory.registration.clientSecretSettingName)
Write-Host ("    {0,-44} {1}" -f "openIdIssuer:",            $currentConfig.properties.identityProviders.azureActiveDirectory.registration.openIdIssuer)
Write-Host ("    {0,-44} {1}" -f "allowedAudiences:",        ($currentConfig.properties.identityProviders.azureActiveDirectory.validation.allowedAudiences -join ", "))
Write-Host ("    {0,-44} {1}" -f "disableWWWAuthenticate:",  $currentConfig.properties.identityProviders.azureActiveDirectory.login.disableWWWAuthenticate)
Write-Host ""
Write-Host "  login.tokenStore" -ForegroundColor White
Write-Host ("    {0,-44} {1}" -f "enabled:",                    $currentConfig.properties.login.tokenStore.enabled)
Write-Host ("    {0,-44} {1}" -f "tokenRefreshExtensionHours:", $currentConfig.properties.login.tokenStore.tokenRefreshExtensionHours)
Write-Host ""
Write-Host "  httpSettings" -ForegroundColor White
Write-Host ("    {0,-44} {1}" -f "requireHttps:",  $currentConfig.properties.httpSettings.requireHttps)
Write-Host ("    {0,-44} {1}" -f "forwardProxy:",  $currentConfig.properties.httpSettings.forwardProxy.convention)
Write-Host ("    {0,-44} {1}" -f "apiPrefix:",     $currentConfig.properties.httpSettings.routes.apiPrefix)
Write-Host ""
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan

# ── Step 8.4: Serialize and write to temp file ───────────────────────────────
$finalJson = $currentConfig | ConvertTo-Json -Depth 20 -Compress
$tmpFile   = [System.IO.Path]::GetTempFileName() + ".json"
Set-Content -Path $tmpFile -Value $finalJson -Encoding UTF8

Write-Host ""
Write-Host "Sending configuration to Azure..."

# ── Step 8.5: PUT the patched config ─────────────────────────────────────────
az rest `
    --method  PUT `
    --uri     $authUri `
    --headers "Content-Type=application/json" `
    --body    "@$tmpFile"

Remove-Item $tmpFile -Force
Write-Host "✓ Easy Auth configuration applied." -ForegroundColor Green

# ── Step 8.6: Wait for ARM propagation then verify unauthenticatedClientAction ─
Write-Host ""
Write-Host "Waiting 5 seconds for ARM propagation..."
Start-Sleep -Seconds 5

$postCheck = az rest `
    --method GET `
    --uri    $authUri `
    --output json | ConvertFrom-Json

$appliedAction = $postCheck.properties.globalValidation.unauthenticatedClientAction

if ($appliedAction -eq "AllowAnonymousRequests") {
    Write-Host "✓ unauthenticatedClientAction confirmed: $appliedAction" -ForegroundColor Green
} else {
    Write-Host "✗ unauthenticatedClientAction is still: $appliedAction" -ForegroundColor Red
    Write-Host "  The portal may have a Restrict Access policy overriding this." -ForegroundColor Yellow
    Write-Host "  Check: Azure Portal → $FunctionAppName → Authentication → Edit → Restrict access" -ForegroundColor Yellow
}
```

---

#### Step 8b — ⚠️ REMOVED: az functionapp auth update (V1 — incompatible with V2)

> **This command has been removed from the runbook.**
>
> The original Step 8b used `az functionapp auth update` (which does not exist) and its
> equivalent `az webapp auth update`. Both target the **V1 authsettings** ARM endpoint
> (`/config/authsettings`), which is **incompatible** with the V2 payload written by Step 8a-alt.
>
> Running a V1 command after a V2 `az rest PUT` causes Azure to silently downgrade the
> authentication configuration, overwriting `authsettingsV2` fields including
> `globalValidation`, `tokenStore`, and `httpSettings` — undoing everything Step 8a-alt set.
>
> **The `az rest PUT` in Step 8a-alt already sets all required fields**, including:
> - `platform.enabled = true`
> - `globalValidation.unauthenticatedClientAction = "AllowAnonymousRequests"`
> - `login.tokenStore.enabled = true`
>
> No additional command is needed after Step 8a-alt completes.

---

### Step 9 — Set Function App Runtime Settings

**What it does:** Stores the client secret under the name referenced by
`clientSecretSettingName` in authsettingsV2, and writes `AZURE_CLIENT_ID` /
`AZURE_TENANT_ID` for runtime token validation by the middleware.

**Variables used:** `$FunctionAppName`, `$ResourceGroup`, `$ClientId`, `$TenantId`,
`$ClientSecret`

```powershell
# Store the client secret as a named app setting.
# authsettingsV2 references it by the name in clientSecretSettingName —
# the actual value must be stored here with that exact key name.
Write-Host ""
Write-Host "Writing app settings to Function App..."

az functionapp config appsettings set `
    --name           $FunctionAppName `
    --resource-group $ResourceGroup `
    --settings       "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET=$ClientSecret" `
                     "AZURE_CLIENT_ID=$ClientId"                              `
                     "AZURE_TENANT_ID=$TenantId"

Write-Host "✓ App settings saved:" -ForegroundColor Green
Write-Host "    MICROSOFT_PROVIDER_AUTHENTICATION_SECRET = (hidden)"
Write-Host "    AZURE_CLIENT_ID                          = $ClientId"
Write-Host "    AZURE_TENANT_ID                          = $TenantId"
```

**Variables produced:** none (settings written to Function App configuration)

---

## Part 3 — Verification

Run these read-only commands at any time to inspect the current state.

### Verify App Registration

```powershell
az ad app show `
    --id $AppObjectId `
    --query "{Name:displayName, ClientId:appId, IdentifierUris:identifierUris, ReplyUrls:web.redirectUris}" `
    --output json
```

### Verify App Roles

```powershell
az ad app show `
    --id $AppObjectId `
    --query "appRoles[].{Name:displayName, Value:value, Enabled:isEnabled}" `
    --output table
```

### Verify a User's Assignments

```powershell
# Replace $userId with the Object ID of the user to inspect
$userId = $ResolvedUserIds["alice"]   # or any ObjectId directly

az rest `
    --method GET `
    --uri "https://graph.microsoft.com/v1.0/users/$userId/appRoleAssignments" `
    --query "value[].{RoleId:appRoleId, Created:createdDateTime}" `
    --output table
```

### Verify Easy Auth Configuration

**Option 1 — az webapp auth show** (authV2 extension, preferred)

```powershell
# NOTE: az webapp auth show is the correct command — az functionapp auth show does not exist.
# az webapp auth applies to both Function Apps and Web Apps (same ARM resource type).

az webapp auth show `
    --name           $FunctionAppName `
    --resource-group $ResourceGroup `
    --query "{
        Enabled:   properties.platform.enabled,
        Action:    properties.globalValidation.unauthenticatedClientAction,
        TokenStore:properties.login.tokenStore.enabled,
        ClientId:  properties.identityProviders.azureActiveDirectory.registration.clientId,
        Issuer:    properties.identityProviders.azureActiveDirectory.registration.openIdIssuer
    }" `
    --output json
```

**Expected output:**
```json
{
  "Enabled":    true,
  "Action":     "AllowAnonymousRequests",
  "TokenStore": true,
  "ClientId":   "<your-client-id>",
  "Issuer":     "https://login.microsoftonline.com/<tenant-id>/v2.0"
}
```

**Option 2 — az rest GET** (always available, no extension required)

```powershell
$authUri = "https://management.azure.com/subscriptions/$SubscriptionId"  +
           "/resourceGroups/$ResourceGroup"                               +
           "/providers/Microsoft.Web/sites/$FunctionAppName"              +
           "/config/authsettingsV2"                                       +
           "?api-version=2022-03-01"

az rest --method GET --uri $authUri --output json | ConvertFrom-Json |
    Select-Object -ExpandProperty properties |
    Select-Object platform, globalValidation, @{n="tokenStore";e={$_.login.tokenStore}}
```

### Full Easy Auth Verification (Field-by-Field)

```powershell
$authUri = "https://management.azure.com/subscriptions/$SubscriptionId"  +
           "/resourceGroups/$ResourceGroup"                               +
           "/providers/Microsoft.Web/sites/$FunctionAppName"              +
           "/config/authsettingsV2"                                       +
           "?api-version=2022-03-01"

Write-Host ""
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Easy Auth — Full Verification                      " -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan

$verify      = az rest --method GET --uri $authUri --output json | ConvertFrom-Json
$aad         = $verify.properties.identityProviders.azureActiveDirectory
$appSettings = az functionapp config appsettings list `
    --name           $FunctionAppName `
    --resource-group $ResourceGroup `
    --output json | ConvertFrom-Json

function Check-Field {
    param(
        [string] $Label,
        [string] $Actual,
        [string] $Expected
    )
    $pass  = $Actual -eq $Expected
    $icon  = $pass ? "✓" : "✗"
    $color = $pass ? "Green" : "Red"
    $note  = $pass ? "" : "  ← expected: $Expected"
    Write-Host ("  {0} {1,-44} {2}{3}" -f $icon, $Label, $Actual, $note) -ForegroundColor $color
}

Write-Host ""
Write-Host "  Platform" -ForegroundColor White
Check-Field "platform.enabled"        "$($verify.properties.platform.enabled)"        "True"

Write-Host ""
Write-Host "  Global Validation" -ForegroundColor White
Check-Field "requireAuthentication"       "$($verify.properties.globalValidation.requireAuthentication)"       "False"
Check-Field "unauthenticatedClientAction" "$($verify.properties.globalValidation.unauthenticatedClientAction)" "AllowAnonymousRequests"

Write-Host ""
Write-Host "  Azure Active Directory" -ForegroundColor White
Check-Field "aad.enabled"                 "$($aad.enabled)"                                              "True"
Check-Field "aad.clientId"                "$($aad.registration.clientId)"                                "$ClientId"
Check-Field "aad.clientSecretSettingName" "$($aad.registration.clientSecretSettingName)"                 "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET"
Check-Field "aad.openIdIssuer"            "$($aad.registration.openIdIssuer)"                            "$IssuerUrl"
Check-Field "aad.allowedAudiences"        "$($aad.validation.allowedAudiences -join ', ')"               "api://$ClientId"
Check-Field "aad.disableWWWAuthenticate"  "$($aad.login.disableWWWAuthenticate)"                         "False"

Write-Host ""
Write-Host "  Token Store" -ForegroundColor White
Check-Field "tokenStore.enabled" "$($verify.properties.login.tokenStore.enabled)" "True"

Write-Host ""
Write-Host "  HTTP Settings" -ForegroundColor White
Check-Field "requireHttps" "$($verify.properties.httpSettings.requireHttps)"            "True"
Check-Field "forwardProxy" "$($verify.properties.httpSettings.forwardProxy.convention)" "NoProxy"
Check-Field "apiPrefix"    "$($verify.properties.httpSettings.routes.apiPrefix)"        "/.auth"

Write-Host ""
Write-Host "  App Settings" -ForegroundColor White

$secretSetting   = $appSettings | Where-Object { $_.name -eq "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET" }
$clientIdSetting = $appSettings | Where-Object { $_.name -eq "AZURE_CLIENT_ID" }
$tenantIdSetting = $appSettings | Where-Object { $_.name -eq "AZURE_TENANT_ID" }

$icon  = $secretSetting   ? "✓" : "✗"
$color = $secretSetting   ? "Green" : "Red"
Write-Host ("  $icon {0,-44} {1}" -f "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET", ($secretSetting   ? "set (value hidden)" : "MISSING")) -ForegroundColor $color

$icon  = $clientIdSetting ? "✓" : "✗"
$color = $clientIdSetting ? "Green" : "Red"
Write-Host ("  $icon {0,-44} {1}" -f "AZURE_CLIENT_ID",                          ($clientIdSetting ? $clientIdSetting.value : "MISSING")) -ForegroundColor $color

$icon  = $tenantIdSetting ? "✓" : "✗"
$color = $tenantIdSetting ? "Green" : "Red"
Write-Host ("  $icon {0,-44} {1}" -f "AZURE_TENANT_ID",                          ($tenantIdSetting ? $tenantIdSetting.value : "MISSING")) -ForegroundColor $color

Write-Host ""
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Verification complete. Fix any ✗ lines above.     " -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
```

### End-to-End Token Test

```powershell
# Get an access token for the signed-in user scoped to this app
$token = az account get-access-token --resource $ClientId --query accessToken --output tsv

# Call the secure endpoint with the token
Invoke-RestMethod `
    -Uri     "https://$FunctionAppName.azurewebsites.net/secure/order.json?id=111" `
    -Headers @{ Authorization = "Bearer $token" } `
    -Method  GET
```

---

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `Tenant mismatch` error in Step 1 | Logged into wrong tenant | `az login --tenant $TenantId` |
| `App role ID not found in map` in Step 7 | Step 5 ran before replication finished | Re-run the `$RoleIdMap` block at the end of Step 5 |
| HTTP 409 on role assignment | Assignment already exists | Expected — silently skipped |
| HTTP 401 from function endpoint | Easy Auth not yet propagated | Wait 30–60 seconds and retry |
| `'auth' is misspelled or not recognized` | `az functionapp auth` does not exist | Use `az webapp auth` — applies to Function Apps too |
| `az webapp auth microsoft update` fails | authV2 extension not installed | Run `az extension add --name authV2` |
| Easy Auth reverts to `RedirectToLoginPage` | V1 `az webapp auth update` ran after V2 PUT | Remove Step 8b; use `az rest PUT` (Step 8a-alt) only |
| Client secret lost | Cannot be retrieved from Azure | Re-run Step 4 — old secret is immediately invalidated |
| App roles not appearing in JWT | Token cached before roles were assigned | Sign out and back in, or wait for token expiry (default 1 hour) |
| `unauthenticatedClientAction` reverts after portal save | Portal Restrict Access override | Azure Portal → `$FunctionAppName` → Authentication → Edit → set **Restrict access** to _Allow unauthenticated_ |
