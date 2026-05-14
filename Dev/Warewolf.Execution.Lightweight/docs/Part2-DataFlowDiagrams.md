# Part 2 — Data Flow Diagrams
## Warewolf Execution Lightweight — Authentication & Authorization

> Sequence diagrams (Mermaid) for every supported authentication path through
> `Warewolf.Execution.Lightweight`.  Read this alongside Part 1 (Architecture)
> and Part 3 (Implementation) when reasoning about a specific token or request
> shape.

Pipeline order (set in `HostBuilderExtensions.ConfigureWarewolf`):

```
EasyAuthRedirectMiddleware → ClaimsPrincipalBuilderMiddleware → WorkflowAuthorizationMiddleware → Function
```

---

## 2.1 Browser flow — interactive Easy Auth login

User navigates a browser to `/secure/Hello` without a session.

```mermaid
sequenceDiagram
    autonumber
    participant Browser
    participant AppService as Azure App Service<br/>(Easy Auth V2)
    participant FuncHost   as Functions Host
    participant ER  as EasyAuthRedirectMiddleware
    participant CPB as ClaimsPrincipalBuilderMiddleware
    participant AZ  as WorkflowAuthorizationMiddleware
    participant Fn  as ExecuteSecureWorkflow
    participant Entra as Microsoft Entra ID

    Browser->>AppService: GET /secure/Hello (no cookie)
    AppService->>FuncHost: pass-through (AllowAnonymous action)
    FuncHost->>ER: Invoke(/secure/Hello)
    ER->>ER: Accept: text/html → LooksLikeBrowserNavigation = true
    ER-->>Browser: 302 /.auth/login/aad?post_login_redirect_uri=/secure/Hello
    Browser->>AppService: GET /.auth/login/aad
    AppService->>Entra: Authorize (response_type=code)
    Entra-->>Browser: id_token + code (via redirect)
    Browser->>AppService: GET /secure/Hello (with .AspNetCore.Cookies)
    AppService->>AppService: validate cookie, inject X-MS-CLIENT-PRINCIPAL
    AppService->>FuncHost: GET /secure/Hello + X-MS-CLIENT-PRINCIPAL
    FuncHost->>ER: Invoke
    ER->>ER: principal header present → next
    ER->>CPB: next
    CPB->>CPB: EasyAuthPrincipalParser decodes header
    CPB->>AZ: WorkflowClaimsPrincipal stored in Items
    AZ->>AZ: lookup policy("hello") + check group + permission
    AZ->>Fn: next
    Fn-->>Browser: 200 OK
```

---

## 2.2 API flow — bearer token (delegated user_impersonation)

SPA / native client posts a JWT obtained via MSAL `acquireToken`.

```mermaid
sequenceDiagram
    autonumber
    participant Client as SPA / Native Client
    participant Entra  as Microsoft Entra ID
    participant ER     as EasyAuthRedirectMiddleware
    participant CPB    as ClaimsPrincipalBuilderMiddleware
    participant AZ     as WorkflowAuthorizationMiddleware
    participant Fn     as ExecuteSecureWorkflow

    Client->>Entra: acquireToken(scopes=[api://wwexec/.default])
    Entra-->>Client: access_token (RS256, scp=user_impersonation)
    Client->>ER: GET /secure/Hello<br/>Authorization: Bearer <jwt>
    ER->>ER: Authorization header present → next
    ER->>CPB: next
    CPB->>CPB: EasyAuthPrincipalParser yields (no header)
    CPB->>CPB: BearerTokenPrincipalParser<br/>fetch OIDC keys (cached)
    CPB->>CPB: validate iss/aud/sig/lifetime
    CPB->>AZ: WorkflowClaimsPrincipal (IsUserToken=true)
    AZ->>AZ: matcher.Evaluate("hello", principal)
    AZ->>Fn: next (Allowed)
    Fn-->>Client: 200 OK + result
```

---

## 2.3 Daemon flow — app-only client credentials

Background service uses application identity (`scp` absent, `roles` carries app permissions).

```mermaid
sequenceDiagram
    autonumber
    participant Daemon as Worker / CI Job
    participant Entra
    participant ER  as EasyAuthRedirectMiddleware
    participant CPB as ClaimsPrincipalBuilderMiddleware
    participant AZ  as WorkflowAuthorizationMiddleware
    participant Fn  as ExecuteSecureWorkflow

    Daemon->>Entra: client_credentials grant<br/>scope=api://wwexec/.default
    Entra-->>Daemon: access_token (no scp, roles=[Permission.Execute, ...])
    Daemon->>ER: POST /secure/SyncOrders<br/>Authorization: Bearer <jwt>
    ER->>CPB: next
    CPB->>CPB: BearerTokenPrincipalParser validates token
    CPB->>AZ: WorkflowClaimsPrincipal (IsAppOnlyToken=true,<br/>CallerIdentity="app:{oid}")
    AZ->>AZ: principal.IsInAnyGroup uses roles<br/>(no UPN match for app tokens)
    AZ->>Fn: next (Allowed)
    Fn-->>Daemon: 200 OK
```

---

## 2.4 OBO flow — middle-tier on-behalf-of

User-facing API exchanges its bearer token for a downstream token to call wwexecution.

```mermaid
sequenceDiagram
    autonumber
    participant User
    participant MidAPI as Middle-tier API
    participant Entra
    participant WW    as wwexecution<br/>(Function App)

    User->>MidAPI: GET /api/orders<br/>Authorization: Bearer <user-jwt>
    MidAPI->>Entra: OBO grant<br/>assertion=<user-jwt>,<br/>scope=api://wwexec/.default
    Entra-->>MidAPI: downstream access_token<br/>(scp=user_impersonation, on behalf of user)
    MidAPI->>WW: GET /secure/Hello<br/>Authorization: Bearer <downstream-jwt>
    WW->>WW: same Bearer flow as 2.2
    WW-->>MidAPI: 200 OK
    MidAPI-->>User: 200 OK
```

The downstream token preserves the original user's `oid`, `name`, and `roles`,
so policy decisions in wwexecution attribute the action to the real user — not
the middle-tier service principal.

---

## 2.5 Legacy HMAC flow — `/login` issued JWT

Pre-Entra clients exchange credentials for a Warewolf-signed JWT via the
optional `AuthenticationOverrideWorkflow` (CFG-05).

```mermaid
sequenceDiagram
    autonumber
    participant Client
    participant Login  as /login (LoginFunction)
    participant LoginWf as AuthenticationOverrideWorkflow
    participant Secure  as /secure/* route
    participant JV      as JwtValidator (HMAC)

    Client->>Login: POST /login {username, password}
    Login->>LoginWf: invoke override workflow
    LoginWf-->>Login: WorkflowExecutionResult { token, groups, perms }
    Login-->>Client: 200 OK { token: <hs256-jwt> }
    Client->>Secure: GET /secure/Hello<br/>Authorization: Bearer <hs256-jwt>
    Secure->>JV: ValidateJwt(secret = SecretKey)
    JV-->>Secure: groups, permissions
    Secure->>Secure: same group / permission check
    Secure-->>Client: 200 OK
```

> The legacy HMAC path uses the same `WorkflowAuthorizationMiddleware`; the
> only difference is which parser produced the principal (the legacy HMAC
> validator path runs inside `WorkflowHttpFunction`, not as a parser, for
> compatibility with the original Warewolf web server).

---

## 2.6 Hot-reload of `secure.config` (POL-08)

Operator updates the encrypted policy file on disk; no restart needed.

```mermaid
sequenceDiagram
    autonumber
    participant Op as Operator / CI Pipeline
    participant FS as Disk (D:\home\site\wwwroot)
    participant W  as SecureConfigWatcher
    participant SCL as SecureConfigLoader
    participant L  as WorkflowAuthPolicyLoader
    participant AZ as WorkflowAuthorizationMiddleware

    Op->>FS: write new secure.config (encrypted)
    FS-->>W: FileSystemWatcher.Changed
    W->>W: debounce 250 ms (coalesce bursts)
    W->>SCL: Reload() — atomic _override swap
    W->>L: Reload() — Volatile.Write new map
    L-->>W: PolicyCount logged
    Note over AZ: subsequent requests read<br/>the new snapshot — readers never block
```

---

## 2.7 Authorization decision matrix (single request)

Common path applied after the principal is built.

```mermaid
flowchart TD
    A[/Path classified/] --> B{path prefix?}
    B -->|/public/*| OK[next]
    B -->|/secure/*<br/>or /services/*| C{IsDevelopment<br/>and bypass header?}
    C -->|yes| OK
    C -->|no| D{principal<br/>authenticated?}
    D -->|no| U[401 unauthorized<br/>+ correlation id<br/>+ AuditLogger.LogAuthOutcome]
    D -->|yes| E[ExtractWorkflowName]
    E --> F[routeRegistry.GetRequiredPermissions<br/>fallback: View|Execute]
    F --> G[matcher.Evaluate]
    G --> H{outcome}
    H -->|Allowed| OK
    H -->|NoPolicyFound| OK[+ warning log]
    H -->|Forbidden| X[403 forbidden<br/>+ auth.policy.denied metric<br/>+ correlation id<br/>+ AuditLogger.LogAuthOutcome]
```

---

## Cross-references

| Diagram | Code                                                                                          |
|---------|-----------------------------------------------------------------------------------------------|
| 2.1     | `Auth/Middleware/EasyAuthRedirectMiddleware.cs`                                              |
| 2.2     | `Auth/Parsers/BearerTokenPrincipalParser.cs`, `Auth/Models/EntraAuthOptions.cs`              |
| 2.3     | `Auth/WorkflowClaimsPrincipal.cs` — `IsAppOnlyToken`                                         |
| 2.4     | Token validation is identical to 2.2; OBO is a client-side concern                           |
| 2.5     | `Functions/LoginFunction.cs`, `Security/JwtValidator.cs`                                     |
| 2.6     | `Auth/SecureConfigWatcher.cs`, `Auth/WorkflowAuthPolicyLoader.cs`                            |
| 2.7     | `Auth/Middleware/WorkflowAuthorizationMiddleware.cs`                                         |
