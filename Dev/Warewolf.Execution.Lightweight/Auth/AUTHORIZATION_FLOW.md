# Warewolf Execution Lightweight — Authorization Flow

> Covers **Steps 1–7** of the policy-based authorization design plus the **object relationships** between every major type.

---

## Overview

Every inbound HTTP request to a `/secure/` or `/services/` route is evaluated against
a mandatory `secure.config` file.  Entra ID (Azure AD) tokens carry **roles only**;
all **permissions** (View, Execute, Contribute, …) are resolved from `secure.config` at
request time.

```
HTTP Request
     │
     ▼
[Step 1] ClaimsPrincipalBuilderMiddleware
     │   Extract roles → WorkflowClaimsPrincipal
     ▼
[Step 2] RouteAuthorizationRegistry
     │   Classify route (/public / /secure / /services / /workflow)
     ▼
[Step 3] WorkflowAuthorizationMiddleware
     │   Skip non-auth routes → otherwise call WorkflowPolicyMatcher
     ▼
[Step 4] WorkflowPolicyMatcher.Evaluate()
     │   Call IWorkflowAuthPolicyLoader.GetPolicy()
     ▼
[Step 5] IWorkflowAuthPolicyLoader.GetPolicy()
     │   Return PolicyLookupResult (Bypass | ConfigMissing | Policy)
     ▼
[Step 6] IWorkflowAuthPolicyLoader.GetEffectivePermissions()
     │   Resolve active scope (resource > global), union matched roles + Public
     │   Stamp result onto WorkflowClaimsPrincipal via SetResolvedPermissions()
     ▼
[Step 7] PolicyMatchResult → HTTP response
         Allowed → 200/next middleware
         Forbidden → 403 Forbidden
         ConfigMissingDeny → 503 Service Unavailable
```

---

## Step 1 — Build the Principal (`ClaimsPrincipalBuilderMiddleware`)

**Input:** Raw `FunctionContext` with Easy Auth header (`X-MS-CLIENT-PRINCIPAL`) or
`Authorization: Bearer <jwt>`.

**Action:**
1. Try `EasyAuthPrincipalParser` → parse base-64 JSON injected by Azure App Service auth.
2. Fall back to `JwtBearerPrincipalParser` → validate HMAC-signed JWT issued by Warewolf.
3. If both fail → `WorkflowClaimsPrincipal.Anonymous()`.

**Output:** `WorkflowClaimsPrincipal` stored in `FunctionContext.Items` under
`AuthConstants.PrincipalContextKey`.

**Key rule:** Only **role** claims from the token are kept.  `Permission.*` claims in the
token are treated as ordinary group names and carry no special meaning; effective
permissions are resolved later from `secure.config`.

---

## Step 2 — Classify the Route (`RouteAuthorizationRegistry`)

Each Azure Function is decorated with `[WorkflowRouteAuth]`; the registry builds a
lookup from function name → route type at startup.

| Route prefix | Auth required |
|---|---|
| `/public/` | None — skip auth entirely |
| `/secure/` | Full policy check (Steps 3–7) |
| `/services/` | Full policy check (Steps 3–7) |
| `/workflow/` | Internal engine route — no external auth |

---

## Step 3 — Enforce via Middleware (`WorkflowAuthorizationMiddleware`)

The middleware is the only place that writes HTTP status codes for auth outcomes.

```
Invoke(context)
  ├─ route = RouteAuthorizationRegistry.GetRouteType(context)
  ├─ if Public route → next()  (pass through)
  ├─ principal = context.Items[PrincipalContextKey]
  ├─ workflowName = ExtractWorkflowName(request.Url)
  ├─ result = WorkflowPolicyMatcher.Evaluate(workflowName, principal, requiredPermissions)
  └─ switch result.Outcome
       Allowed         → next()
       Forbidden       → 403 Forbidden  + audit log
       NoPolicyFound   → 403 Forbidden  (workflow not configured)
       ConfigMissingDeny → 503 Service Unavailable + error log
```

### BYPASS_SECURE_CONFIG mode

When the environment variable `BYPASS_SECURE_CONFIG=true` is set, the loader returns
`PolicyLookupResult.Bypass()` and the middleware calls `next()` unconditionally.
**This mode is for development only and must never be set in production.**

---

## Step 4 — Evaluate the Policy (`WorkflowPolicyMatcher`)

`WorkflowPolicyMatcher.Evaluate(workflowName, principal, requiredPermissions)`:

```
1. loader.GetPolicy(workflowName)
   ├─ Bypass        → PolicyMatchResult.Allow()  [dev mode]
   ├─ ConfigMissing → PolicyMatchResult.DenyConfigMissing()
   └─ FromPolicy(policy)
        ├─ policy == null  → PolicyMatchResult.NoPolicy()   [workflow not configured]
        └─ policy != null  → continue...

2. policy.HasPublicRole?
   └─ YES → effective perms include Public's flags (no role matching needed)

3. principal.IsInAnyGroup(policy.AllowedGroups)?
   └─ NO → PolicyMatchResult.DenyGroup()

4. loader.GetEffectivePermissions(workflowName, principal.Groups)
   → stamps result onto principal via SetResolvedPermissions()

5. principal.HasPermissionFlag(requiredPermissions)?
   ├─ YES → PolicyMatchResult.Allow()
   └─ NO  → PolicyMatchResult.DenyPermission()
```

---

## Step 5 — Load the Policy (`WorkflowAuthPolicyLoader.GetPolicy`)

`WorkflowAuthPolicyLoader` is a **singleton** built from `SecureConfigLoader.Config`.

### Config states

| State | Cause | `GetPolicy` returns |
|---|---|---|
| `BYPASS_SECURE_CONFIG=true` | Dev override | `PolicyLookupResult.Bypass()` |
| File missing / blank | Deployment error | `PolicyLookupResult.ConfigMissing()` |
| Loaded, workflow has resource entries | Normal | `PolicyLookupResult.FromPolicy(resourcePolicy)` |
| Loaded, no resource entries, global entries exist | Fallback | `PolicyLookupResult.FromPolicy(globalPolicy)` |
| Loaded, no entries at all | Workflow not configured | `PolicyLookupResult.FromPolicy(null)` |

### Two scope maps built at load time

```
secure.config WindowsGroupPermission[]
        │
        ├─ IsServer=true, ResourceID=Guid.Empty
        │         → _globalRoleMap  [GroupName → WorkflowPermission flags]
        │
        └─ IsServer=false, ResourceName=<workflow>
                  → _resourceRoleMap  [workflowName → [GroupName → flags]]
```

**Scope precedence:** When resource entries exist for a workflow, the global scope is
**discarded entirely** for that workflow.  A caller matched only in the global scope
does **not** inherit global permissions when the workflow has its own resource entries.

---

## Step 6 — Resolve Effective Permissions (`GetEffectivePermissions`)

```
GetEffectivePermissions(workflowName, callerRoles):

1. Super-admin check (hot-read, WAREWOLF_SUPER_ADMIN_ENABLED)
   └─ If enabled AND any callerRole holds Administrator flag in _globalRoleMap
      → return WorkflowPermission.All immediately

2. Determine active scope
   ├─ _resourceRoleMap[workflowName] exists → activeScope = resource map
   └─ else                                  → activeScope = _globalRoleMap

3. Union Public permissions (no role match required)
   └─ combined |= activeScope["Public"]  (if present)

4. Union matched-role permissions
   └─ foreach role in callerRoles: combined |= activeScope[role]

5. Return combined WorkflowPermission flags
```

The result is stamped onto `WorkflowClaimsPrincipal.Permissions` via
`SetResolvedPermissions(combined)`.

### Multi-role union example

| Role in token | Flags in scope |
|---|---|
| `DevOps` | `DeployTo \| DeployFrom` |
| `Developers` | `View \| Execute \| Contribute` |
| **Effective** | `View \| Execute \| Contribute \| DeployTo \| DeployFrom` |

---

## Step 7 — Translate Outcome to HTTP Response

| `PolicyMatchOutcome` | HTTP status | Log level |
|---|---|---|
| `Allowed` | — (call next middleware) | Debug |
| `Forbidden` (group mismatch) | 403 Forbidden | Warning + audit |
| `Forbidden` (permission mismatch) | 403 Forbidden | Warning + audit |
| `NoPolicyFound` (workflow unconfigured) | 403 Forbidden | Warning |
| `ConfigMissingDeny` | 503 Service Unavailable | Error |

Response body for 403/503 is a JSON object:
```json
{ "error": "<reason>", "workflow": "<name>", "caller": "<upn-or-app-id>" }
```

---

## Object Relationships

```
IWorkflowAuthPolicyLoader  (interface)
  └── WorkflowAuthPolicyLoader  (singleton implementation)
        │  reads ──► SecureConfigLoader.Config : SecureConfigData
        │               └── IReadOnlyList<PermissionEntry>
        │
        ├── _globalRoleMap   : Dictionary<GroupName, WorkflowPermission>
        ├── _resourceRoleMap : Dictionary<WorkflowName, Dictionary<GroupName, WorkflowPermission>>
        └── _policies        : Dictionary<WorkflowName, WorkflowAuthPolicy>
                                    └── RolePolicies : IReadOnlyList<ResolvedRolePolicy>
                                              ├── GroupName  : string
                                              ├── IsPublic   : bool
                                              └── EffectivePermissions : WorkflowPermission

WorkflowPolicyMatcher
  ├── uses ──► IWorkflowAuthPolicyLoader
  ├── reads ──► WorkflowClaimsPrincipal
  └── returns ► PolicyMatchResult
                    ├── Outcome : PolicyMatchOutcome  (Allowed | Forbidden | NoPolicyFound | ConfigMissingDeny)
                    └── MatchedPolicy : ResolvedRolePolicy?

PolicyLookupResult  (discriminated union)
  ├── Bypass()           → IsBypass = true
  ├── ConfigMissing()    → IsConfigMissing = true
  └── FromPolicy(policy) → HasPolicyScope = true, Value = WorkflowAuthPolicy?

WorkflowClaimsPrincipal  (extends ClaimsPrincipal)
  ├── Groups      : IReadOnlyList<string>   ← role claims from Entra token
  ├── Permissions : WorkflowPermission      ← stamped by WorkflowPolicyMatcher
  ├── IsInAnyGroup(groups)
  └── HasPermissionFlag(permission)         ← AND logic on Permissions flags

WorkflowPermission  [Flags enum]
  None | View | Execute | Contribute | DeployTo | DeployFrom | Administrator | All

WorkflowAuthorizationMiddleware
  ├── uses ──► WorkflowPolicyMatcher
  ├── uses ──► RouteAuthorizationRegistry
  └── writes → HttpResponseData  (403 / 503 / pass-through)
```

---

## Environment Variables Reference

| Variable | Purpose | Default |
|---|---|---|
| `WAREWOLF_SECURE_CONFIG` | Override path to `secure.config` | `<AppContext.BaseDirectory>/secure.config` |
| `BYPASS_SECURE_CONFIG` | Set to `true` to skip all policy checks (dev only) | *(not set)* |
| `WAREWOLF_SUPER_ADMIN_ENABLED` | Set to `true` to enable super-admin bypass for roles with `Administrator` flag | *(not set)* |
| `WAREWOLF_ENTRA_TENANT_ID` | Restrict token acceptance to this Entra tenant | *(not set — any tenant accepted)* |
| `WAREWOLF_ENTRA_AUDIENCE` | Required `aud` claim value in Entra tokens | *(not set — any audience accepted)* |
