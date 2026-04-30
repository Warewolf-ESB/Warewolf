# Tutorial — Securing the `wwexecution` Azure Function with Easy Auth + Microsoft Entra ID

> Audience: .NET 8 / Azure Functions developers and platform engineers
> Target: An existing isolated‑worker Azure Function App named **`wwexecution`** running on a Consumption (Free) plan
> Stack: Easy Auth (App Service Authentication V2), Microsoft Entra ID, Clean Architecture middleware pipeline, claim‑based authorization, file‑backed `secure.config` permission map

---

## 0. What you already have (analysis)

Before introducing any new code, here is a reading of the current state of the project. Every step below is wired around the seams that already exist — we are *configuring* and *completing* the implementation, not rebuilding it.

### 0.1 `Program.cs`

Bootstraps an isolated Functions host through `HostBuilderExtensions.ConfigureWarewolf(...)`. That extension method is the single composition root and registers the auth middleware pipeline in the correct order. There is no logic to add to `Program.cs` — the file is already clean.

### 0.2 `Auth/Middleware/*` — three middlewares, in this order

1. `EasyAuthRedirectMiddleware` — first in the pipeline. Lets `/Public/*` through, returns a clean `401 application/json` for unauthenticated `/Secure/*` calls (defeats Easy Auth's "RedirectToLoginPage" behavior that would otherwise break API clients), and forwards anything that already carries `X‑MS‑CLIENT‑PRINCIPAL` or `Authorization: Bearer …`.
2. `ClaimsPrincipalBuilderMiddleware` — decodes the Easy Auth `X‑MS‑CLIENT‑PRINCIPAL` header (Base64 JSON), normalizes the claim type URIs, and stores a strongly‑typed `WorkflowClaimsPrincipal` in `FunctionContext.Items[AuthConstants.PrincipalContextKey]`.
3. `WorkflowAuthorizationMiddleware` — enforces the `secure.config` policy: extracts the workflow name from the URL (segment after `/Secure/`), looks up a `WorkflowAuthPolicy`, applies *group OR check* and *permission AND check*, and writes a 401/403 envelope on failure.

### 0.3 `Auth/WorkflowClaimsPrincipal.cs`

A `ClaimsPrincipal` subclass exposing `UserId`, `UserName`, `IsUserToken` / `IsAppOnlyToken`, `Groups`, `Permissions`, `PermissionFlags`, `HasPermissionFlag(WorkflowPermission)`, `IsInAnyGroup(...)`. This is the type your function code uses — it is the contract between the auth pipeline and the rest of the application.

### 0.4 `Auth/WorkflowAuthPolicyLoader.cs`

A singleton, immutable, in‑memory map of `workflowName → WorkflowAuthPolicy` built at startup from `SecureConfigLoader.Config.Permissions`. `Execute` is a hard requirement to be in `AllowedGroups`, and the policy carries the AND‑checked `RequiredPermissions = View | Execute`.

### 0.5 `Functions/WorkflowHttpFunction.cs`

Exposes the workflow routes. Authorization levels are deliberate:

| Route | `AuthorizationLevel` | Reason |
|------|----|----|
| `Public/{*name}` | `Anonymous` | Unauthenticated by design |
| `Secure/{*name}` | `Anonymous` | Easy Auth + middleware enforce auth before us — Functions key would conflict |
| `Services/{*name}` | `Function` | Function‑key path retained for legacy callers |
| `apis.json` (root) | `Anonymous` | Discovery, filtered by permission |

The function reads back the principal that the middleware deposited in `FunctionContext.Items` and falls back to a token check (`JwtValidator` then `EntraTokenValidator`) only when middleware didn't run (the legacy `Services/*` path).

### 0.6 `secure.config` integration

`SecureConfigLoader` reads either `WAREWOLF_SECURE_CONFIG` (env var) or `bin/secure.config`. The file is AES‑CBC encrypted JSON deserializing to a `SecuritySettingsTO` containing `SecretKey` and a list of `WindowsGroupPermission` rows. `PermissionChecker` answers two questions: "is workflow X visible to Public?" and "is workflow X visible to user with these groups?".

### 0.7 Existing `Scripts/authsettingsV2.json`

A template payload for `az rest PUT … /config/authsettingsV2`. Has `<TENANT_ID>` / `<CLIENT_ID>` placeholders. The PowerShell script in §1.7 fills it in from variables and PUT's it to ARM in one shot.

---

## 1. Set up Easy Auth and Microsoft Entra ID

This section walks through every Azure‑side change needed for the existing Function App `wwexecution`. The PowerShell script in §1.7 automates *all* of this; the manual steps below exist to explain *what* the script does and *why*.

### 1.1 Register the application in Microsoft Entra

We need a single Entra application registration that represents `wwexecution` to the platform.

The registration has to do four things:
1. Identify itself with a stable `clientId` and tenant.
2. Expose an API surface (`api://<clientId>`) so callers can request access tokens with the right `aud` claim.
3. Declare the **app roles** that map 1:1 to Warewolf groups and permissions.
4. Accept Easy Auth's redirect URI (`https://wwexecution.azurewebsites.net/.auth/login/aad/callback`).

Manual equivalent in the portal: *Microsoft Entra ID → App registrations → New registration → name `wwexecution-auth` → Accounts in this organizational directory only → Web platform with redirect `https://wwexecution.azurewebsites.net/.auth/login/aad/callback`.*

### 1.2 (Optional) Create the roles

Two app roles satisfy the requirement to "have roles":

| Display name | Value | Allowed member types |
|---|---|---|
| Warewolf Administrators | `WarewolfAdministrators` | Users |
| Public | `PUBLIC` | Users |

> The role **value** is what surfaces in the token's `roles` claim and what we match in `secure.config`. Spaces and case both matter.

### 1.3 (Optional) Create the permissions

Permissions are *also* modeled as app roles, so that one token contains both the role memberships and the granted permissions. The Permission roles are:

`Permission.View`, `Permission.Execute`, `Permission.Contribute`, `Permission.DeployTo`, `Permission.DeployFrom`, `Permission.Administrator`

The `WorkflowClaimsPrincipal` already understands the `Permission.*` prefix and surfaces them via `Permissions` and `PermissionFlags` (see `WorkflowClaimsPrincipal.cs` lines 47–60).

### 1.4 (Optional) Assign permissions to roles

Entra app roles cannot directly contain other app roles, so this mapping is realised at *assignment time*: every user gets the permission roles their group is supposed to carry. The script does it like this:

| Group | Permissions assigned alongside the group role |
|---|---|
| `WarewolfAdministrators` | `Permission.View`, `Permission.Execute`, `Permission.Contribute`, `Permission.DeployTo`, `Permission.DeployFrom`, `Permission.Administrator` |
| `PUBLIC` | `Permission.View` |

Result: when `ashley.lewis@theunlimited.co.za` logs in, her token contains `roles: ["WarewolfAdministrators", "Permission.View", "Permission.Execute", … "Permission.Administrator"]`.

### 1.5 (Optional) Assign roles to users

| User | Group role |
|---|---|
| `ashley.lewis@theunlimited.co.za` | `WarewolfAdministrators` |
| `aakash.gaikwad@theunlimited.co.za` | `PUBLIC` |

The script resolves each UPN to an object ID (`az ad user show --id <upn> --query id`) and posts an `appRoleAssignment` to `/users/{id}/appRoleAssignments`.

### 1.6 Browser‑based and non‑browser (token) authentication — both flows

The same Entra app registration backs both flows. They differ only in *who acquires the token*.

**Browser flow (interactive)** — used by humans hitting `https://wwexecution.azurewebsites.net/Secure/order.json?id=111` in a tab.
1. The function receives the request without a session cookie.
2. Easy Auth (`platform.enabled = true`, `globalValidation.requireAuthentication = false` plus our middleware) lets the request flow but the middleware sees no auth and our `EasyAuthRedirectMiddleware` returns `401`.
3. The browser is redirected to `/.auth/login/aad?post_login_redirect_uri=…` (the platform's OIDC entry point).
4. The user signs in against `https://login.microsoftonline.com/<tenant>/v2.0`, consents, and is redirected back to `/.auth/login/aad/callback`.
5. Easy Auth exchanges the auth code, stores the tokens server‑side in the Token Store, and redirects to the original URL with an Easy Auth session cookie.
6. Now the inbound request carries `X‑MS‑CLIENT‑PRINCIPAL`. `ClaimsPrincipalBuilderMiddleware` decodes it into `WorkflowClaimsPrincipal`. `WorkflowAuthorizationMiddleware` enforces `secure.config`. The function sees a fully populated principal in `FunctionContext.Items`.

**Non‑browser flow (Bearer token)** — used by SPAs, daemons, CI, Postman, integration partners.
1. The caller acquires a token from Entra. Two patterns:
   * **Delegated (user impersonation)** — auth code or device code flow with scope `api://<clientId>/.default`. Token has `scp` claim → `IsUserToken = true`.
   * **App‑only (client credentials)** — `POST /<tenant>/oauth2/v2.0/token` with `client_credentials`, `scope=api://<clientId>/.default`. Token has no `scp` → `IsAppOnlyToken = true`.
2. The caller sends `Authorization: Bearer <jwt>` to `/Secure/order.json`.
3. Easy Auth validates the JWT against the Entra tenant and emits `X‑MS‑CLIENT‑PRINCIPAL`. The rest is identical to the browser flow.
4. If, for any reason, Easy Auth doesn't run (e.g. `Services/*` legacy path), `WorkflowHttpFunction.ValidateJwt(...)` falls back to `EntraTokenValidator.GetRoles(...)` to extract roles directly from the bearer token.

In both cases the function code never touches a token directly — it reads `WorkflowClaimsPrincipal` from `FunctionContext.Items[AuthConstants.PrincipalContextKey]`.

### 1.7 PowerShell automation (`Scripts/Configure-WwExecutionAuth.ps1`)

Steps 1.1–1.6 are codified in `Scripts/Configure-WwExecutionAuth.ps1`. The script is **idempotent** — every operation is "create if missing, update if exists" — so it can be re‑run safely.

Variables at the top of the file:

```powershell
$SubscriptionId       = '<your-subscription-id>'
$ResourceGroupName    = 'rg-warewolf'
$FunctionAppName      = 'wwexecution'
$EntraAppDisplayName  = 'wwexecution-auth'
$TenantId             = 'ca0cc53b-9af4-4067-bcdf-be9c648450d1'
$AdminUsers           = @('ashley.lewis@theunlimited.co.za')
$PublicUsers          = @('aakash.gaikwad@theunlimited.co.za')
```

Run from any machine with Azure CLI ≥ 2.55:

```powershell
az login
az account set --subscription $SubscriptionId
./Scripts/Configure-WwExecutionAuth.ps1
```

Outputs (for the developer's records):
* App (client) ID
* Object ID of the service principal
* Tenant ID
* Generated client secret expiry
* Names of the assigned app roles per user

---

## 2. Authentication / authorization implementation in the Function App

This corresponds to what already exists; the table below shows where each concern lives, and what to verify after running the PowerShell script.

| Concern | Component | Verification step |
|---|---|---|
| Reject anonymous calls to `/Secure/*` | `Auth/Middleware/EasyAuthRedirectMiddleware.cs` | `curl -i https://wwexecution.azurewebsites.net/Secure/order.json?id=1` returns `401` with `WWW-Authenticate: Bearer realm="warewolf"` |
| Decode Easy Auth principal | `Auth/Middleware/ClaimsPrincipalBuilderMiddleware.cs` | After interactive sign‑in, the `Principal built` log line in App Insights shows User, Authenticated=True, Groups |
| Enforce `secure.config` policy | `Auth/Middleware/WorkflowAuthorizationMiddleware.cs` | A user without `Permission.Execute` for `order.json` gets `403 forbidden { workflow:"order", required:"View, Execute" }` |
| Fallback raw‑token validation | `Functions/WorkflowHttpFunction.ValidateJwt(...)` + `Security/EntraTokenValidator` | Disable Easy Auth temporarily and re‑request `/Secure/order.json` with a valid bearer; should still succeed |
| Reflection of policies in apis.json | `WorkflowHttpFunction.GetSecureFilter(...)` | A `WarewolfAdministrators` user sees every workflow on `/Secure/apis.json`; a `PUBLIC` user sees only `View`‑permitted workflows |

The pipeline is registered in `Infrastructure/HostBuilderExtensions.cs` — order is critical:

```csharp
worker.UseMiddleware<EasyAuthRedirectMiddleware>();
worker.UseMiddleware<ClaimsPrincipalBuilderMiddleware>();
worker.UseMiddleware<WorkflowAuthorizationMiddleware>();
```

Required Function App settings (set automatically by the PowerShell script):

| Setting | Value |
|---|---|
| `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` | Entra app client secret |
| `WAREWOLF_ENTRA_TENANT_ID` | `ca0cc53b-9af4-4067-bcdf-be9c648450d1` |
| `WAREWOLF_ENTRA_AUDIENCE` | `api://<entra-app-client-id>` |
| `WAREWOLF_SECURE_CONFIG` | `D:\home\site\wwwroot\secure.config` (or Azure File mount path) |

---

## 3. Carrying the authentication payload through the function

The end‑to‑end contract is: **every secure function reads `WorkflowClaimsPrincipal` out of `FunctionContext.Items`**.

Calling pattern from any function (already implemented in `WorkflowHttpFunction.ExecuteSecureWorkflow`):

```csharp
[Function("ExecuteSecureWorkflow")]
public async Task<HttpResponseData> ExecuteSecureWorkflow(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Secure/{*name}")]
        HttpRequestData req,
    string name,
    FunctionContext context)
{
    if (!context.Items.TryGetValue(AuthConstants.PrincipalContextKey, out var raw)
        || raw is not WorkflowClaimsPrincipal principal
        || principal.Identity?.IsAuthenticated != true)
    {
        return req.CreateResponse(HttpStatusCode.Unauthorized);
    }

    // principal.UserName, principal.Groups, principal.Permissions,
    // principal.HasPermissionFlag(WorkflowPermission.Execute),
    // principal.IsInAnyGroup(new[]{"WarewolfAdministrators"})

    return await ExecuteNamedWorkflow(req, name, isPublic: false, context);
}
```

Because `ClaimsPrincipalBuilderMiddleware` always populates an *anonymous* principal when no Easy Auth header is present (never throws), every function can read the key safely without null‑guarding the dictionary.

The principal object encapsulates four orthogonal concerns:

* **Identity** — `UserId` (Entra OID), `UserName` (UPN), `IsUserToken` vs `IsAppOnlyToken`, `CallerIdentity` (logging label).
* **Groups** — flat list of role claim values (e.g. `WarewolfAdministrators`, `PUBLIC`, `ashley.lewis@theunlimited.co.za`).
* **Permissions** — list filtered to claims starting with `Permission.` (e.g. `Permission.View`).
* **Bitwise flags** — `PermissionFlags` returns a `WorkflowPermission` enum for fast `HasFlag` checks.

---

## 4. Authorization helper — matching the request against the role/permission map

`secure.config` is the single source of truth for **which group can do what to which workflow**. It is deployed alongside the function app (or mounted from an Azure File share). The runtime objects are:

* `SecureConfigLoader` (singleton) — reads, decrypts, exposes `SecureConfigData`.
* `WorkflowAuthPolicyLoader` (singleton) — converts `PermissionEntry` rows into `WorkflowAuthPolicy` records.
* `PermissionChecker` (static) — answers visibility questions (used by `apis.json`).
* `WorkflowAuthorizationMiddleware` — enforces the policy on every `/Secure/*` invocation.

The matching algorithm (already implemented in `WorkflowAuthorizationMiddleware.Invoke`):

```text
1. Skip /Public/*
2. If WorkflowAuthPolicyLoader.PolicyCount == 0 → open access (warn & allow)
3. Extract workflow name = path-segment after /Secure/, strip extension
4. Lookup WorkflowAuthPolicy by name. Missing → 403
5. Group check (OR): principal must be in at least one entry in policy.AllowedGroups
   - Match by group claim OR by principal.UserName (UPN)  ← email entries supported
6. Permission check (AND): the matched group entry must hold ALL flags in policy.RequiredPermissions
   - Default RequiredPermissions = View | Execute
7. Allow → next(context)
```

Sample `secure.config.example.json` is included alongside the script. The Warewolf Studio (or a small CLI utility) encrypts this JSON into the binary `secure.config` that the function loads.

> **Important nuance** — `WorkflowAuthPolicyLoader` only ingests rows where `IsServer == false` and `ResourceName` is set. Server-wide admin rows (`IsServer = true`) drive *apis.json visibility* via `PermissionChecker.HasUserViewPermission`, but are **not** sufficient to execute a specific workflow. To grant execute rights to admins on a workflow, add a per-workflow row for `WarewolfAdministrators` as well. The example file does both.

Excerpt for the `order` workflow:

```json
{
  "WindowsGroupPermissions": [
    {
      "WindowsGroup":  "WarewolfAdministrators",
      "ResourceID":    "11111111-1111-1111-1111-111111111111",
      "ResourceName":  "order",
      "IsServer":      false,
      "View":          true,
      "Execute":       true,
      "Contribute":    true,
      "DeployTo":      true,
      "DeployFrom":    true,
      "Administrator": true
    },
    {
      "WindowsGroup":  "PUBLIC",
      "ResourceID":    "11111111-1111-1111-1111-111111111111",
      "ResourceName":  "order",
      "IsServer":      false,
      "View":          true,
      "Execute":       false
    },
    {
      "WindowsGroup":  "ashley.lewis@theunlimited.co.za",
      "ResourceID":    "11111111-1111-1111-1111-111111111111",
      "ResourceName":  "order",
      "IsServer":      false,
      "View":          true,
      "Execute":       true
    }
  ]
}
```

End‑to‑end:

1. `aakash.gaikwad@theunlimited.co.za` calls `GET /Secure/order.json?id=111`.
2. Easy Auth has redirected him through Entra; token contains `roles: ["PUBLIC", "Permission.View"]`.
3. `ClaimsPrincipalBuilderMiddleware` builds `principal.UserName = "aakash.gaikwad@…"`, `principal.Groups = ["PUBLIC"]`, `principal.Permissions = ["Permission.View"]`.
4. `WorkflowAuthPolicyLoader` excludes the `PUBLIC` row for `order` from `AllowedGroups` because it has no `Execute` flag. The resulting policy is `AllowedGroups = ["WarewolfAdministrators", "ashley.lewis@…"]`.
5. `WorkflowAuthorizationMiddleware` runs `IsInAnyGroup` — neither aakash's `Groups` nor his `UserName` match — group check fails.
6. Middleware writes `403 forbidden { workflow: "order", required: "View, Execute" }`.

If the same call had come from `ashley.lewis@theunlimited.co.za`, her direct UPN entry has `View=true Execute=true` → `IsInAnyGroup` matches via `UserName` → permission AND check passes (entry has `View|Execute`) → middleware allows the request and logs `Authorised 'ashley.lewis@…' for workflow 'order'`.

---

## 5. Verification checklist

Run after deploying:

1. `curl -i https://wwexecution.azurewebsites.net/Public/healthcheck.json` → `200`, anonymous access, content returned.
2. `curl -i https://wwexecution.azurewebsites.net/Secure/order.json?id=111` → `401`, JSON envelope, `WWW-Authenticate: Bearer`.
3. Open `https://wwexecution.azurewebsites.net/Secure/order.json?id=111` in a browser → redirected to Entra → consent → returns `403` for `aakash` and `200` for `ashley`.
4. App Insights → Logs → `traces` → search for `Authorised`, `Group check failed`, `Permission check failed`.
5. `https://wwexecution.azurewebsites.net/Secure/apis.json` returns the workflows the caller is permitted to view; `https://wwexecution.azurewebsites.net/Public/apis.json` returns only Public‑viewable workflows.

---

## 6. Operational notes

* **Free / Consumption tier limitations** — Easy Auth is fully supported; only the Token Store size and concurrency are constrained. Keep `tokenStore.enabled = true` (the default) but expect token refresh hits on cold start.
* **Rotating the client secret** — bump `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` in App Settings; Easy Auth picks it up without a deploy. Re‑run the PowerShell script with `-RotateSecret` to generate a new one.
* **Local development** — set `IsDevelopment` and the `X‑WW‑Bypass‑Auth: local-dev-bypass` header to skip policy checks (see `WorkflowAuthorizationMiddleware`). Never honoured in Production, by environment guard.
* **Multi‑tenant** — set `WAREWOLF_ENTRA_TENANT_ID` to the trusted tenant. `EntraTokenValidator` rejects tokens from other tenants.
* **Audit** — every authorized / denied request logs with the caller's `CallerIdentity` for full traceability.

---

## 7. Files delivered with this tutorial

| File | Purpose |
|---|---|
| `Scripts/Configure-WwExecutionAuth.ps1` | One‑shot AZ‑CLI script — registers app, app roles, role assignments, and pushes `authsettingsV2` |
| `Scripts/authsettingsV2.json` | Easy Auth payload template (already in repo, picked up by the script) |
| `Scripts/secure.config.example.json` | Plain‑text JSON shape of the encrypted `secure.config` — encrypt and copy to `D:\home\site\wwwroot\secure.config` |
| `docs/EasyAuth-Entra-Tutorial.md` | This document |
