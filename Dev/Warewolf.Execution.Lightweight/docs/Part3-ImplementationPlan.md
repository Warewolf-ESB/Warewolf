# Part 3 — Detailed Implementation Plan
## Warewolf Execution Lightweight — Authentication & Authorization

> **Audience** — Developers implementing or extending the auth system.
> Cross-references the actual source files so every item is traceable.
>
> **Status key** — ✅ Implemented | 🔲 Pending | ⚠ Partial

---

## 3.1 Middleware Pipeline

### 3.1.1 `EasyAuthRedirectMiddleware`  ✅
**File:** `Auth/Middleware/EasyAuthRedirectMiddleware.cs`

- [x] Pass `/public/*` through unconditionally
- [x] On `/secure/*` without auth header and no `X-MS-CLIENT-PRINCIPAL` → detect browser navigation → `302` to `/.auth/login/aad`
- [x] On `/secure/*` without auth and **not** a browser → `401 JSON` with `WWW-Authenticate: Bearer`
- [x] On `/secure/*` with `Authorization: Bearer` or `X-MS-CLIENT-PRINCIPAL` present → `next()`
- [x] All other paths → `next()`

**Implementation notes:**
```csharp
// Browser navigation heuristic — checks Accept header for text/html
// and absence of X-Requested-With: XMLHttpRequest
private bool LooksLikeBrowserNavigation(HttpRequestData req) { ... }
```

**Pending:**
- 🔲 Extend redirect guard to `/services/*` routes (currently only `/secure/*` is intercepted; function-key routes are excluded by auth level but a consistent guard is desirable for uniformity)

---

### 3.1.2 `ClaimsPrincipalBuilderMiddleware`  ✅
**File:** `Auth/Middleware/ClaimsPrincipalBuilderMiddleware.cs`

- [x] Iterate `IEnumerable<IPrincipalParser>` in registration order
- [x] First non-null result wins
- [x] On parse failure → store `WorkflowClaimsPrincipal.Anonymous()` (unauthenticated principal)
- [x] Store result at `context.Items[AuthConstants.PrincipalContextKey]`
- [x] Per-parser exception isolation — one failing parser does not abort the chain

**Registration order (DI):**
```
1. EasyAuthPrincipalParser   (reads X-MS-CLIENT-PRINCIPAL — always preferred in prod)
2. BearerTokenPrincipalParser (validates RS256 JWT directly — dev / non-Easy-Auth)
```

---

### 3.1.3 `WorkflowAuthorizationMiddleware`  ✅
**File:** `Auth/Middleware/WorkflowAuthorizationMiddleware.cs`

- [x] `/public/*` → pass through
- [x] `/secure/*` and `/services/*` → enforce; all other paths → pass through
- [x] Dev bypass header (`X-WW-Bypass-Auth: local-dev-bypass`) — Development only
- [x] No authenticated principal → `401`
- [x] Extract workflow name from path (strips prefix, removes extension suffix)
- [x] Resolve required permissions from `IRouteAuthorizationRegistry` (falls back to `View | Execute`)
- [x] Delegate to `IWorkflowPolicyMatcher.Evaluate()`
- [x] `Allowed` → `next()`
- [x] `NoPolicyFound` → `next()` + warning log
- [x] `Forbidden` → `403 JSON` with reason

---

## 3.2 Identity Parsers

### 3.2.1 `EasyAuthPrincipalParser`  ✅
**File:** `Auth/Parsers/EasyAuthPrincipalParser.cs`

- [x] Read `X-MS-CLIENT-PRINCIPAL` header
- [x] Base64-decode JSON payload
- [x] Extract `auth_typ`, `name_typ`, `claims[]` array
- [x] Normalize claim types (`typ` field) to standard `ClaimTypes.*` URIs
- [x] Return `WorkflowClaimsPrincipal` with `authenticationType = "EasyAuth"`
- [x] Return `null` when header is absent or payload cannot be parsed

**Claim normalization map:**

| Easy Auth `typ` | Normalized to |
|---|---|
| `roles` | `AuthConstants.Roles` |
| `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` | `ClaimTypes.Role` |
| `preferred_username` | `AuthConstants.PreferredUsername` |
| `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name` | `ClaimTypes.Name` |
| `http://schemas.microsoft.com/identity/claims/objectidentifier` | `AuthConstants.ObjectIdentifier` |
| `scp` | `AuthConstants.Scope` |

---

### 3.2.2 `BearerTokenPrincipalParser`  ✅
**File:** `Auth/Parsers/BearerTokenPrincipalParser.cs`

- [x] Read `Authorization: Bearer <token>` header
- [x] Return `null` when `EntraAuthOptions.IsEnabled == false`
- [x] Use `ConfigurationManager<OpenIdConnectConfiguration>` with 24 h auto-refresh and 5 min retry
- [x] Validate with `JwtSecurityTokenHandler` (RS256, OIDC signing keys)
- [x] Validate: issuer (v1 + v2 accepted), audience, lifetime, signing key
- [x] Normalize claims: `oid`, `name`, `preferred_username`, `roles`, `scp`
- [x] Return `WorkflowClaimsPrincipal` with `authenticationType = "Bearer"`
- [x] Return `null` on any validation failure (no throw to caller)

**Required NuGet packages:**
```xml
<PackageReference Include="Microsoft.IdentityModel.Protocols.OpenIdConnect" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" />
```

---

## 3.3 Policy Infrastructure

### 3.3.1 `WorkflowAuthPolicyLoader`  ✅
**File:** `Auth/WorkflowAuthPolicyLoader.cs`

- [x] Read `SecureConfigLoader.Config` at constructor time (singleton)
- [x] Group non-global `PermissionEntry` records by `ResourceName.ToLowerInvariant()`
- [x] Only include entries where `Execute == true` in `AllowedGroups`
- [x] Build immutable `Dictionary<string, WorkflowAuthPolicy>`
- [x] Log policy count at startup

**Edge case — group with View only:** Group entries that have `View = true, Execute = false`
are excluded from `AllowedGroups`.  This means a View-only group will not appear in policy
allowed lists, and the middleware's group-OR check will fail → `Forbidden`.
This correctly prevents View-only users from executing workflows via `/secure/*`.

**Pending:**
- 🔲 Hot-reload support: reload policies when `secure.config` is updated without a restart. Requires a file-watcher and thread-safe dictionary swap in `WorkflowAuthPolicyLoader`.

---

### 3.3.2 `WorkflowPolicyMatcher`  ✅
**File:** `Auth/WorkflowPolicyMatcher.cs`

- [x] Return `NoPolicyFound` when `GetPolicy()` returns null
- [x] Group OR: `principal.IsInAnyGroup(policy.AllowedGroups)` — also checks `principal.UserName` for UPN-style entries
- [x] Permission AND: find first matching `WorkflowGroupEntry`, check `HasFlag(requiredPermissions)`
- [x] Return `PolicyMatchResult.Allow()`, `DenyGroup(reason)`, or `DenyPermission(reason, entry)`

**To substitute a different strategy:**
```csharp
// In ServiceCollectionExtensions.AddCoreServices:
services.AddSingleton<IWorkflowPolicyMatcher, MyCustomPolicyMatcher>();
```

---

### 3.3.3 `RouteAuthorizationRegistry`  ✅
**File:** `Auth/RouteAuthorizationRegistry.cs`

- [x] Reflect over all public instance/static methods of supplied types
- [x] Match methods that carry both `[FunctionAttribute]` and `[RequireWorkflowPermissionAttribute]`
- [x] Build case-insensitive `Dictionary<string, WorkflowPermission>`
- [x] Keyed by `FunctionAttribute.Name` (same as `FunctionContext.FunctionDefinition.Name`)

**Current registrations in `WorkflowHttpFunction`:**

| Function name | Required permissions |
|---|---|
| `ExecuteService` | `View \| Execute` |
| `ExecuteSecureWorkflow` | `View \| Execute` |
| `ExecuteWorkflowByName` | `View \| Execute` |
| `ExecuteWorkflow` | `View \| Execute` |
| `ExecutePublicWorkflow` | *(none — not decorated)* |
| `ExecuteRootApisJson` | *(none — not decorated)* |

---

## 3.4 Principal Model

### 3.4.1 `WorkflowClaimsPrincipal`  ✅
**File:** `Auth/WorkflowClaimsPrincipal.cs`

- [x] `UserId` — from `oid` / `nameid` claim
- [x] `UserName` — from `name` / `preferred_username`
- [x] `IsUserToken` — true when `scp` claim present (delegated flow)
- [x] `IsAppOnlyToken` — true when no `scp` claim (client credentials)
- [x] `CallerIdentity` — `UserName` for users, `app:{UserId}` for daemons
- [x] `Groups` — all role claim values (group membership + app roles)
- [x] `Permissions` — values starting with `Permission.`
- [x] `IsInGroup(name)` — case-insensitive lookup in `Groups`
- [x] `IsInAnyGroup(names)` — OR logic; also checks `UserName` for UPN entries
- [x] `HasPermission(value)` — case-insensitive lookup in `Permissions`
- [x] `Anonymous()` — static factory; unauthenticated placeholder

---

## 3.5 DI Registration Summary

**File:** `Infrastructure/ServiceCollectionExtensions.cs` (`AddCoreServices`)

```csharp
// Policy infrastructure
services.AddSingleton<IWorkflowAuthPolicyLoader, WorkflowAuthPolicyLoader>();
services.AddSingleton<IWorkflowPolicyMatcher,    WorkflowPolicyMatcher>();
services.AddSingleton<IRouteAuthorizationRegistry>(
    _ => RouteAuthorizationRegistry.BuildFrom(typeof(WorkflowHttpFunction)));

// Principal parsers (order = priority)
services.AddSingleton<IPrincipalParser, EasyAuthPrincipalParser>();
services.AddSingleton<IPrincipalParser, BearerTokenPrincipalParser>();
```

**File:** `Infrastructure/HostBuilderExtensions.cs` (middleware order)

```csharp
worker.UseMiddleware<EasyAuthRedirectMiddleware>();       // 1 — redirect guard
worker.UseMiddleware<ClaimsPrincipalBuilderMiddleware>();  // 2 — identity
worker.UseMiddleware<WorkflowAuthorizationMiddleware>();   // 3 — policy
```

---

## 3.6 Configuration Objects

### `EntraAuthOptions`  ✅
**File:** `Auth/Models/EntraAuthOptions.cs`

Read from env vars at startup. Used by `BearerTokenPrincipalParser`.

```
WAREWOLF_ENTRA_TENANT_ID  →  TenantId
WAREWOLF_ENTRA_AUDIENCE   →  Audience   (e.g. api://<clientId>)
WAREWOLF_ENTRA_CLIENT_ID  →  ClientId   (optional alternative audience)
```

`IsEnabled = TenantId != null && (Audience != null || ClientId != null)`

### `AuthConstants`  ✅
**File:** `Auth/Models/AuthConstants.cs`

| Constant | Value |
|---|---|
| `PublicRoutePrefix` | `/public/` |
| `SecureRoutePrefix` | `/secure/` |
| `ServicesRoutePrefix` | `/services/` |
| `PrincipalContextKey` | `WorkflowClaimsPrincipal` |
| `ClientPrincipalHeader` | `X-MS-CLIENT-PRINCIPAL` |
| `VerboseAuthLogging` | `true` (compile-time const — set `false` for prod) |

---

## 3.7 Pending Implementation Items

### 3.7.1 🔲 `EasyAuthRedirectMiddleware` — `/services/*` guard
**Description:** The redirect guard currently only intercepts `/secure/*`.
`/services/*` uses function-key auth (not Easy Auth), so the platform does not
redirect, but a consistent guard would return a clean `401` for clients sending
no function key rather than a platform-level `401`.

**File to edit:** `Auth/Middleware/EasyAuthRedirectMiddleware.cs`

**Change:** Add `|| path.StartsWith(AuthConstants.ServicesRoutePrefix, ...)` to the
secure-route check block.

---

### 3.7.2 🔲 Hot-reload for `secure.config`
**Description:** `WorkflowAuthPolicyLoader` reads config once at startup.
To pick up permission changes without restarting the function app, a
`FileSystemWatcher` should invalidate and rebuild the policy dictionary.

**File to create:** `Auth/SecureConfigWatcher.cs`  
**Interface to update:** `IWorkflowAuthPolicyLoader` — add `Reload()` method

---

### 3.7.3 🔲 `EntraAuthOptions` DI registration
**Description:** `BearerTokenPrincipalParser` currently receives `EntraAuthOptions`
by direct constructor injection. The options should be registered as a DI singleton
via `services.AddSingleton(_ => EntraAuthOptions.FromEnvironment())` so they can be
mocked in tests.

**File to edit:** `Infrastructure/ServiceCollectionExtensions.cs`

```csharp
services.AddSingleton(_ => EntraAuthOptions.FromEnvironment());
```

---

### 3.7.4 🔲 Unit test coverage
The following types have no unit tests yet and are candidates for immediate coverage:

| Type | Key test scenarios |
|---|---|
| `WorkflowPolicyMatcher` | Allow, DenyGroup, DenyPermission, NoPolicyFound |
| `RouteAuthorizationRegistry` | Attribute present, absent, multiple types |
| `EasyAuthPrincipalParser` | Valid header, malformed base64, missing claims |
| `BearerTokenPrincipalParser` | Valid RS256 token, expired, wrong audience, wrong issuer |
| `WorkflowAuthPolicyLoader` | Policy count, group filter, execute-only entries |
| `WorkflowClaimsPrincipal` | IsInAnyGroup UPN match, IsAppOnlyToken, PermissionFlags |

---

### 3.7.5 🔲 Audit log on authorization failure
**Description:** `AuditLogger` exists in `Security/AuditLogger.cs` but is not
wired into the authorization middleware.  403 and 401 outcomes should be emitted
to the audit log for compliance.

**File to edit:** `Auth/Middleware/WorkflowAuthorizationMiddleware.cs`  
**Change:** Inject `AuditLogger` and call `LogAuthorizationFailure(...)` on 401/403.

---

### 3.7.6 🔲 `[RequireWorkflowPermission(WorkflowPermission.View)]` on apis.json routes
**Description:** Currently the `ExecuteRootApisJson` and public apis.json routes
carry no permission attribute and are not enforced by the middleware.  This is
intentional for discovery, but the secure apis.json endpoint
(`ExecuteSecureWorkflow` when path ends in `apis.json`) could carry a View-only
attribute to make the intent explicit.

---

## 3.8 `WorkflowHttpFunction` Route Decoration Summary

**File:** `Functions/WorkflowHttpFunction.cs`

```csharp
// Authenticated execution routes (all require View + Execute from secure.config)
[Function("ExecuteService")]
[RequireWorkflowPermission(WorkflowPermission.View | WorkflowPermission.Execute)]
public async Task<HttpResponseData> ExecuteService(...)

[Function("ExecuteSecureWorkflow")]
[RequireWorkflowPermission(WorkflowPermission.View | WorkflowPermission.Execute)]
public async Task<HttpResponseData> ExecuteSecureWorkflow(...)

[Function("ExecuteWorkflowByName")]
[RequireWorkflowPermission(WorkflowPermission.View | WorkflowPermission.Execute)]
public async Task<HttpResponseData> ExecuteByName(...)

[Function("ExecuteWorkflow")]
[RequireWorkflowPermission(WorkflowPermission.View | WorkflowPermission.Execute)]
public async Task<HttpResponseData> Execute(...)

// Discovery routes (no permission attribute — always accessible)
[Function("ExecutePublicWorkflow")]   // anonymous
[Function("ExecuteRootApisJson")]     // anonymous
```

**How to add a Contribute-restricted admin route:**

```csharp
[Function("UpdateWorkflow")]
[RequireWorkflowPermission(WorkflowPermission.Contribute)]
public async Task<HttpResponseData> UpdateWorkflow(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Secure/admin/{name}")] HttpRequestData req,
    string name, FunctionContext context)
    => await ExecuteNamedWorkflow(req, name, isPublic: false, context);
```

The middleware reads the attribute at runtime from the registry and enforces
`Contribute` as the required permission — no changes to the middleware are needed.
