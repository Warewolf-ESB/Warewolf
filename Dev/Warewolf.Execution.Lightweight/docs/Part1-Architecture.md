# Part 1 — Architecture Plan
## Warewolf Execution Lightweight — Authentication & Authorization System

> **Scope** — This document covers the complete security architecture of the
> `Warewolf.Execution.Lightweight` Azure Function App: identity, token flows,
> policy enforcement, extensibility seams, and the relationship between every
> moving part.  It is written for developers who need to understand, extend, or
> audit the system.

---

## 1. Overview

`Warewolf.Execution.Lightweight` is an Azure Functions (isolated worker, .NET 8)
host that exposes Warewolf workflows as HTTP endpoints.  Security is provided by
three co-operating layers:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  Caller (browser / API client / daemon)                                     │
└─────────────────────────────┬───────────────────────────────────────────────┘
                              │  HTTPS
┌─────────────────────────────▼───────────────────────────────────────────────┐
│  Azure App Service / Functions host                                         │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │  Easy Auth (Microsoft Entra ID)                                      │   │
│  │  • Browser flow   → redirect → Entra login page → callback           │   │
│  │  • Token flow     → validate token → inject X-MS-CLIENT-PRINCIPAL    │   │
│  │  • AllowAnonymous → /Public/* passes through, /Secure/* enforced     │   │
│  └─────────────────────────────┬────────────────────────────────────────┘   │
│                                │                                            │
│  ┌─────────────────────────────▼────────────────────────────────────────┐   │
│  │  Isolated Worker Middleware Pipeline (in-process)                    │   │
│  │                                                                      │   │
│  │  1. EasyAuthRedirectMiddleware                                       │   │
│  │     • Converts Easy Auth redirect → clean 401 for API clients        │   │
│  │     • /public/* → pass through                                       │   │
│  │                                                                      │   │
│  │  2. ClaimsPrincipalBuilderMiddleware                                  │   │
│  │     • IPrincipalParser chain (EasyAuth → Bearer fallback)            │   │
│  │     • Produces WorkflowClaimsPrincipal → FunctionContext.Items       │   │
│  │                                                                      │   │
│  │  3. WorkflowAuthorizationMiddleware                                  │   │
│  │     • /public/* → pass through                                       │   │
│  │     • /secure/* /services/* → require authenticated principal        │   │
│  │     • Resolves per-route WorkflowPermission from RouteRegistry       │   │
│  │     • Delegates matching to IWorkflowPolicyMatcher                   │   │
│  └─────────────────────────────┬────────────────────────────────────────┘   │
│                                │                                            │
│  ┌─────────────────────────────▼────────────────────────────────────────┐   │
│  │  Azure Function methods (WorkflowHttpFunction)                       │   │
│  │  • [RequireWorkflowPermission] attribute declared once per route     │   │
│  │  • No per-route auth boilerplate needed                              │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. Identity Layer — Microsoft Entra ID + Easy Auth

### 2.1 Entra App Registration (resource app)

One Entra app registration represents the Function App itself:

| Field | Value |
|---|---|
| Display name | `<FunctionAppName>-auth` |
| Identifier URI | `api://<clientId>` |
| Redirect URI | `https://<app>.azurewebsites.net/.auth/login/aad/callback` |
| Sign-in audience | `AzureADMyOrg` (single tenant) |
| Implicit grant | `enableIdTokenIssuance = true` (required for browser hybrid flow) |
| OAuth 2.0 scope | `user_impersonation` (required for delegated token acquisition) |

### 2.2 App Roles

App roles are defined on the **resource app registration** and are the bridge
between Entra identity and Warewolf `secure.config` groups:

| Role value | Purpose |
|---|---|
| `WarewolfAdministrators` | Maps to the admin group in `secure.config` |
| `PUBLIC` | Maps to the public group |
| `EXECUTE`, `DEPLOY`, … | Arbitrary named groups from `GroupPermissions` |
| `Permission.View` | Fine-grained permission flag |
| `Permission.Execute` | Fine-grained permission flag |
| `Permission.Contribute` | Fine-grained permission flag |
| `Permission.DeployTo` | Fine-grained permission flag |
| `Permission.DeployFrom` | Fine-grained permission flag |
| `Permission.Administrator` | Fine-grained permission flag |

> **Design rule** — the group app roles (`WarewolfAdministrators`, `PUBLIC`,
> etc.) represent *membership* in a Warewolf group.  The `Permission.*` roles
> represent individual capability flags.  Both are declared as app roles with
> `allowedMemberTypes: ["User", "Application"]` so both users and daemon clients
> can be assigned them.

### 2.3 Easy Auth Configuration

```
provider     : Microsoft (AAD v2.0)
enabled      : true
action       : AllowAnonymous   ← /Public/* works; middleware enforces /Secure/*
issuer       : https://login.microsoftonline.com/<tenantId>/v2.0
clientId     : <appId>
audience     : api://<appId>
tokenStore   : enabled          ← browser session management
```

Easy Auth injects the following headers after successful authentication:

| Header | Content |
|---|---|
| `X-MS-CLIENT-PRINCIPAL` | Base64-encoded JSON with claim array |
| `X-MS-CLIENT-PRINCIPAL-NAME` | Display name |
| `X-MS-CLIENT-PRINCIPAL-IDP` | Identity provider (`aad`) |

---

## 3. Token Flows

### 3.1 Browser Interactive Flow (Authorization Code + PKCE)

```
Browser                    Easy Auth                  Entra ID
  │                            │                          │
  │── GET /Secure/order ───────►│                          │
  │                            │── 302 → /login/aad ──────►│
  │                            │                          │ (user signs in)
  │◄── 302 → /.auth/login/aad ─│◄─ auth code ─────────────│
  │── GET /.auth/login/aad ───►│                          │
  │                            │── exchange code ──────────►│
  │                            │◄─ id_token + access_token ─│
  │◄── Set-Cookie .AspNetCore.Cookies ─────────────────────│
  │── GET /Secure/order ───────►│                          │
  │   (with session cookie)    │ validates cookie          │
  │                            │ injects X-MS-CLIENT-PRINCIPAL
  │                            │──► Middleware pipeline ──►│
```

**Key points:**
- Easy Auth manages the redirect, code exchange, and token refresh entirely.
- The Function App sees only the validated `X-MS-CLIENT-PRINCIPAL` header.
- `enableIdTokenIssuance = true` is required for the hybrid `code id_token` response type used by Easy Auth.

### 3.2 Non-Browser Token Flow (Bearer JWT)

For API clients, daemons, services, and Angular apps using `HttpClient`:

```
Client App                 Token Cache                 Entra ID
  │                            │                          │
  │── acquireToken ────────────►│                          │
  │   (cache miss)             │── POST /oauth2/v2.0/token►│
  │                            │   grant_type=... scope=api://<id>/.default
  │                            │◄─ access_token ───────────│
  │◄─ access_token ────────────│                          │
  │                                                       │
  │── POST /Secure/order ─────────────────────────────────►  Function App
  │   Authorization: Bearer <token>                       │
  │                                                       │  Easy Auth validates
  │                                                       │  X-MS-CLIENT-PRINCIPAL injected
  │                                                       │  OR BearerTokenPrincipalParser
  │                                                       │  validates RS256 directly
  │◄─ 200 JSON result ────────────────────────────────────│
```

**Two validation paths** for bearer tokens:

| Path | When active | Validator |
|---|---|---|
| Easy Auth validated | Deployed to Azure with Easy Auth enabled | `EasyAuthPrincipalParser` reads `X-MS-CLIENT-PRINCIPAL` |
| Direct RS256 validation | Local dev or Easy Auth disabled | `BearerTokenPrincipalParser` validates OIDC signature |

### 3.3 Token Acquisition Strategies by Client Type

| Client type | Grant type | MSAL method | Token contains |
|---|---|---|---|
| Angular / SPA | Authorization Code + PKCE | `acquireTokenSilent` / `acquireTokenPopup` | User identity + app roles |
| .NET API / daemon | Client Credentials | `AcquireTokenForClient` | App identity + app roles |
| .NET on behalf of user | On-Behalf-Of (OBO) | `AcquireTokenOnBehalfOf` | User identity proxied through service |
| Warewolf HMAC JWT | `/login` endpoint | `POST /login` with credentials | UserGroups list (legacy) |

---

## 4. Authentication Component Map

### 4.1 `EasyAuthRedirectMiddleware`

**File:** `Auth/Middleware/EasyAuthRedirectMiddleware.cs`

**Role:** First in pipeline.  Prevents the platform Easy Auth redirect
(`302 → /login`) from breaking API callers that cannot follow redirects.

**Decision logic:**

```
path starts with /public/*  → next()
path starts with /secure/*
  has Authorization: Bearer  → next()   ← let bearer parser handle it
  has X-MS-CLIENT-PRINCIPAL  → next()   ← Easy Auth already validated
  looks like browser nav     → 302 /login redirect
  else                       → 401 JSON
other paths                  → next()
```

### 4.2 `ClaimsPrincipalBuilderMiddleware`

**File:** `Auth/Middleware/ClaimsPrincipalBuilderMiddleware.cs`

**Role:** Second in pipeline.  Iterates the ordered `IEnumerable<IPrincipalParser>`
chain.  The first parser that returns a non-null `WorkflowClaimsPrincipal` wins.
The result is stored at `FunctionContext.Items[AuthConstants.PrincipalContextKey]`.

**Parser chain order (registered in `ServiceCollectionExtensions`):**

1. `EasyAuthPrincipalParser` — reads `X-MS-CLIENT-PRINCIPAL` (preferred; always set by Easy Auth in production)
2. `BearerTokenPrincipalParser` — validates the raw `Authorization: Bearer` JWT via OIDC metadata (local dev and non-Easy-Auth paths)

### 4.3 `WorkflowAuthorizationMiddleware`

**File:** `Auth/Middleware/WorkflowAuthorizationMiddleware.cs`

**Role:** Third in pipeline.  Enforces policy on `/secure/*` and `/services/*`.

**Execution sequence:**

```
1. Is path /public/*?        → pass through
2. Is path /secure/* or /services/*?  → continue; else pass through
3. Is dev bypass header present?  → pass through (Dev only)
4. Is principal authenticated?    → 401 if not
5. Extract workflow name from path
6. Lookup required permissions from IRouteAuthorizationRegistry
7. Call IWorkflowPolicyMatcher.Evaluate(workflow, principal, permissions)
   ├─ Allowed      → pass through
   ├─ NoPolicyFound → pass through + log warning
   └─ Forbidden    → 403 JSON
```

### 4.4 `IWorkflowPolicyMatcher` / `WorkflowPolicyMatcher`

**Files:** `Auth/IWorkflowPolicyMatcher.cs`, `Auth/WorkflowPolicyMatcher.cs`

The extracted matching strategy.  Encapsulates:
- Group OR check — caller must be in ≥1 `AllowedGroups` from the policy
- Permission AND check — matched group entry must hold **all** required flags
- `NoPolicyFound` when no `secure.config` entry covers the workflow

**To change matching behaviour:** implement `IWorkflowPolicyMatcher` and replace
the DI registration in `ServiceCollectionExtensions.AddCoreServices`.

### 4.5 `IRouteAuthorizationRegistry` / `RouteAuthorizationRegistry`

**Files:** `Auth/IRouteAuthorizationRegistry.cs`, `Auth/RouteAuthorizationRegistry.cs`

Built once at startup via reflection over `[Function] + [RequireWorkflowPermission]`
method pairs on `WorkflowHttpFunction`.  Maps function name → `WorkflowPermission`.

**To require different permissions on a route:**

```csharp
[Function("ExecuteAdminWorkflow")]
[RequireWorkflowPermission(WorkflowPermission.Contribute | WorkflowPermission.Administrator)]
public async Task<HttpResponseData> ExecuteAdminWorkflow(...) { ... }
```

### 4.6 `WorkflowAuthPolicyLoader` / `IWorkflowAuthPolicyLoader`

**Files:** `Auth/WorkflowAuthPolicyLoader.cs`, `Auth/IWorkflowAuthPolicyLoader.cs`

Reads `SecureConfigLoader.Config.Permissions` at startup and builds an immutable
dictionary of `WorkflowAuthPolicy` objects keyed by lowercase workflow name.

**Policy build rules:**
- Only non-global (per-workflow) entries with a non-empty `ResourceName` create policies
- Only entries with `Execute == true` are included in `AllowedGroups`
- Server-wide entries (`IsGlobal == true`) are excluded from per-workflow policies

---

## 5. Policy Data Model

### 5.1 `secure.config` structure (plain-text form)

```json
{
  "SecretKey": "<base64-hmac-secret>",
  "AuthenticationOverrideWorkflow": { "ResourceID": "...", "Name": "..." },
  "WindowsGroupPermissions": [
    {
      "WindowsGroup":  "WarewolfAdministrators",
      "ResourceID":    "00000000-...",
      "ResourceName":  "",
      "IsServer":      true,
      "View": true, "Execute": true, "Contribute": true,
      "DeployTo": true, "DeployFrom": true, "Administrator": true
    },
    {
      "WindowsGroup":  "WarewolfAdministrators",
      "ResourceID":    "11111111-...",
      "ResourceName":  "order",
      "IsServer":      false,
      "View": true, "Execute": true, ...
    }
  ]
}
```

### 5.2 Policy object model

```
SecureConfigData
  └── IReadOnlyList<PermissionEntry>
        ├── GroupName      (maps to WindowsGroup)
        ├── ResourceName   (workflow name, empty if IsGlobal)
        ├── IsGlobal       (IsServer == true)
        ├── IsPublicGroup  (GroupName == "Public" or "PUBLIC")
        └── View, Execute, Contribute, DeployTo, DeployFrom, Administrator

WorkflowAuthPolicy
  ├── WorkflowName      (lowercase key)
  ├── AllowedGroups     (List<string> — groups with Execute == true)
  ├── RequiredPermissions (WorkflowPermission flags — default View | Execute)
  └── GroupEntries
        └── WorkflowGroupEntry
              ├── GroupName
              └── Permissions  (WorkflowPermission flags)

WorkflowClaimsPrincipal : ClaimsPrincipal
  ├── UserId, UserName, CallerIdentity
  ├── IsUserToken / IsAppOnlyToken
  ├── Groups     (all role claim values)
  ├── Permissions (Permission.* role values)
  └── IsInAnyGroup(), IsInGroup(), HasPermissionFlag()
```

---

## 6. Route Security Matrix

| Route | Auth level | Principal required | Policy enforced | Attribute |
|---|---|---|---|---|
| `GET /apis.json` | Anonymous | No | No | — |
| `GET /Public/{*name}` | Anonymous | No | No | — |
| `GET /Public/{folder}/apis.json` | Anonymous | No | No | — |
| `GET /Secure/{*name}` | Easy Auth / Bearer | Yes | `View\|Execute` | `[RequireWorkflowPermission]` |
| `GET /Secure/{folder}/apis.json` | Anonymous (filtered) | No | No | — |
| `GET /Services/{*name}` | Function key | Yes | `View\|Execute` | `[RequireWorkflowPermission]` |
| `GET /workflow/{name}` | Function key | Yes | `View\|Execute` | `[RequireWorkflowPermission]` |
| `GET /workflow` | Function key | Yes | `View\|Execute` | `[RequireWorkflowPermission]` |
| `POST /login` | Anonymous | No | N/A (issues tokens) | — |

> **Note on `/Secure/apis.json`** — discovery requests always return without
> 401 but the workflow list is filtered to what the caller can *view*.

---

## 7. Environment Variable Reference

| Variable | Used by | Purpose |
|---|---|---|
| `WAREWOLF_ENTRA_TENANT_ID` | `EntraAuthOptions`, `SecureConfigData` | Entra tenant GUID for bearer validation |
| `WAREWOLF_ENTRA_AUDIENCE` | `EntraAuthOptions`, `SecureConfigData` | Expected `aud` claim (`api://<clientId>`) |
| `WAREWOLF_ENTRA_CLIENT_ID` | `EntraAuthOptions` | Alternative bare client GUID audience |
| `WAREWOLF_SECURE_CONFIG` | `SecureConfigLoader` | Path to `secure.config` on disk |
| `WorkflowsDirectory` | `WorkflowHttpFunction`, `LoginFunction` | Path to workflow resource files |
| `AZURE_KEYVAULT_NAME` | `HostEnvironmentConfig` | Key Vault name for AES-256 key |
| `KEYVAULT_SECRET_NAME` | `HostEnvironmentConfig` | Key Vault secret name (default `dp-keyring-v1`) |
| `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` | Easy Auth | Client secret reference |
| `AZURE_FUNCTIONS_ENVIRONMENT` | `HostEnvironmentConfig` | `Development` triggers dev bypasses |

---

## 8. Extensibility Seams

| What you want to change | Interface to implement | DI registration to update |
|---|---|---|
| Group / permission matching logic | `IWorkflowPolicyMatcher` | `services.AddSingleton<IWorkflowPolicyMatcher, YourMatcher>()` |
| How policies are loaded | `IWorkflowAuthPolicyLoader` | `services.AddSingleton<IWorkflowAuthPolicyLoader, YourLoader>()` |
| How the principal is built | `IPrincipalParser` | Add to `services.AddSingleton<IPrincipalParser, YourParser>()` chain |
| Per-route permission requirements | `[RequireWorkflowPermissionAttribute]` | Add attribute to function method |
| Route auth registry population | `IRouteAuthorizationRegistry` | `RouteAuthorizationRegistry.BuildFrom(...)` or replace with custom |

---

## 9. Security Decisions and Rationale

| Decision | Rationale |
|---|---|
| Easy Auth + in-process bearer fallback | Easy Auth handles browser flows, token refresh, and session cookies without code. Bearer fallback enables local dev and server-to-server calls that bypass the App Service layer. |
| `AllowAnonymous` Easy Auth action | Required for `/Public/*` workflows. Per-route enforcement is done inside the middleware pipeline, not at the platform level. |
| `secure.config` encrypted on disk | Workflow permissions, HMAC secret, and login workflow name are sensitive. AES-256-GCM encryption via Key Vault key protects them at rest. |
| `NoPolicyFound → allow` | Prevents accidental lockout when deploying new workflows before their `secure.config` entry is written. Operators are warned via log. |
| Group OR + Permission AND | Mirrors the Warewolf server's `ServerAuthorizationService` logic. A user in multiple groups gets the most permissive applicable entry. |
| `[RequireWorkflowPermission]` attribute | Centralises permission declaration on the route signature, eliminating repeated per-route boilerplate and making audit trivial (grep the attribute). |
| Principal stored in `FunctionContext.Items` | Avoids re-validating the token in each function; the middleware pipeline guarantees a single validation point. |
