# Part 6 — Full Implementation Task List
## Warewolf Execution Lightweight — Authentication & Authorization

> **Format** — each task has a unique ID, status, priority, owner area, and
> concise description.  Tasks are grouped by theme and ordered within each group
> from foundation to polish.  Cross-references to source files and earlier
> document sections are included where applicable.
>
> **Status key:** ✅ Done | 🔲 Pending | ⚠ Partial | 🚫 Blocked

---

## Group 1 — Core Authentication Infrastructure

| ID | Status | Priority | Task | Reference |
|---|---|---|---|---|
| AUTH-01 | ✅ | Critical | Add `IPrincipalParser` strategy interface with `TryParseAsync(HttpRequestData)` signature | `Auth/Parsers/IPrincipalParser.cs` |
| AUTH-02 | ✅ | Critical | Implement `EasyAuthPrincipalParser` — read and decode `X-MS-CLIENT-PRINCIPAL` header | `Auth/Parsers/EasyAuthPrincipalParser.cs` |
| AUTH-03 | ✅ | Critical | Implement `BearerTokenPrincipalParser` — validate RS256 JWT via OIDC metadata | `Auth/Parsers/BearerTokenPrincipalParser.cs` |
| AUTH-04 | ✅ | Critical | Add `EntraAuthOptions` — read `WAREWOLF_ENTRA_TENANT_ID`, `WAREWOLF_ENTRA_AUDIENCE`, `WAREWOLF_ENTRA_CLIENT_ID` from env | `Auth/Models/EntraAuthOptions.cs` |
| AUTH-05 | ✅ | Critical | Add `WorkflowClaimsPrincipal` typed principal with `UserId`, `UserName`, `Groups`, `Permissions`, `IsUserToken`, `IsAppOnlyToken` | `Auth/WorkflowClaimsPrincipal.cs` |
| AUTH-06 | ✅ | Critical | Implement `ClaimsPrincipalBuilderMiddleware` — iterate parser chain; store result in `FunctionContext.Items` | `Auth/Middleware/ClaimsPrincipalBuilderMiddleware.cs` |
| AUTH-07 | ✅ | High | Implement `EasyAuthRedirectMiddleware` — convert Easy Auth browser redirects to 401 for API callers on `/secure/*` | `Auth/Middleware/EasyAuthRedirectMiddleware.cs` |
| AUTH-08 | ✅ | High | Add `AuthConstants` — define route prefixes, context key, and header names | `Auth/Models/AuthConstants.cs` |
| AUTH-09 | 🔲 | Medium | Register `EntraAuthOptions` as DI singleton via `services.AddSingleton(_ => EntraAuthOptions.FromEnvironment())` for testability | `Infrastructure/ServiceCollectionExtensions.cs` — §3.7.3 |
| AUTH-10 | 🔲 | Medium | Extend `EasyAuthRedirectMiddleware` browser-navigation heuristic to also guard `/services/*` with consistent 401 | `Auth/Middleware/EasyAuthRedirectMiddleware.cs` — §3.7.1 |
| AUTH-11 | 🔲 | Low | Add `AuthConstants.VerboseAuthLogging` compile-time flag; set `false` for production builds to reduce log volume | `Auth/Models/AuthConstants.cs` |

---

## Group 2 — Policy Infrastructure

| ID | Status | Priority | Task | Reference |
|---|---|---|---|---|
| POL-01 | ✅ | Critical | Add `WorkflowPermission` flags enum — `View`, `Execute`, `Contribute`, `DeployTo`, `DeployFrom`, `Administrator` | `Auth/Models/WorkflowPermission.cs` |
| POL-02 | ✅ | Critical | Add `WorkflowAuthPolicy` model — `WorkflowName`, `AllowedGroups`, `GroupEntries` | `Auth/Models/WorkflowAuthPolicy.cs` |
| POL-03 | ✅ | Critical | Add `IWorkflowAuthPolicyLoader` interface with `GetPolicy(workflowName)` | `Auth/IWorkflowAuthPolicyLoader.cs` |
| POL-04 | ✅ | Critical | Implement `WorkflowAuthPolicyLoader` — build immutable policy map from `SecureConfigLoader.Config.Permissions` at startup | `Auth/WorkflowAuthPolicyLoader.cs` |
| POL-05 | ✅ | Critical | Add `IWorkflowPolicyMatcher` interface with `Evaluate(workflowName, principal, requiredPermissions)` | `Auth/IWorkflowPolicyMatcher.cs` |
| POL-06 | ✅ | Critical | Implement `WorkflowPolicyMatcher` — group OR / permission AND logic; returns `PolicyMatchResult` | `Auth/WorkflowPolicyMatcher.cs` — §3.3.2 |
| POL-07 | ✅ | Critical | Add `PolicyMatchResult` — `Allow()`, `DenyGroup(reason)`, `DenyPermission(reason, entry)`, `NoPolicyFound()` factory methods | `Auth/Models/PolicyMatchResult.cs` |
| POL-08 | 🔲 | High | Add `secure.config` hot-reload via `FileSystemWatcher` in `WorkflowAuthPolicyLoader`; thread-safe dictionary swap | `Auth/SecureConfigWatcher.cs` (new) — §3.7.2 |
| POL-09 | 🔲 | Medium | Add `Reload()` to `IWorkflowAuthPolicyLoader` to support programmatic policy refresh (required by POL-08) | `Auth/IWorkflowAuthPolicyLoader.cs` |
| POL-10 | 🔲 | Medium | Add global-policy fallback: if per-workflow policy is absent but a global/server entry for the group exists, apply it | `Auth/WorkflowAuthPolicyLoader.cs` |
| POL-11 | 🔲 | Low | Emit structured log on `NoPolicyFound` including `workflowName`, `callerIdentity`, and source IP for audit trail | `Auth/Middleware/WorkflowAuthorizationMiddleware.cs` |

---

## Group 3 — Route Authorization

| ID | Status | Priority | Task | Reference |
|---|---|---|---|---|
| RTE-01 | ✅ | Critical | Add `RequireWorkflowPermissionAttribute` — method-level attribute declaring required `WorkflowPermission` flags | `Auth/RequireWorkflowPermissionAttribute.cs` |
| RTE-02 | ✅ | Critical | Implement `RouteAuthorizationRegistry` — reflect over `[Function] + [RequireWorkflowPermission]` pairs at startup | `Auth/RouteAuthorizationRegistry.cs` — §3.3.3 |
| RTE-03 | ✅ | Critical | Add `IRouteAuthorizationRegistry` interface with `GetRequiredPermissions(functionName)` | `Auth/IRouteAuthorizationRegistry.cs` |
| RTE-04 | ✅ | High | Decorate `ExecuteService`, `ExecuteSecureWorkflow`, `ExecuteWorkflowByName`, `ExecuteWorkflow` with `[RequireWorkflowPermission(View \| Execute)]` | `Functions/WorkflowHttpFunction.cs` — §3.8 |
| RTE-05 | 🔲 | Medium | Decorate the `/secure/apis.json` discovery route with `[RequireWorkflowPermission(WorkflowPermission.View)]` to make intent explicit | `Functions/WorkflowHttpFunction.cs` — §3.7.6 |
| RTE-06 | 🔲 | Low | Add `[RequireWorkflowPermission(WorkflowPermission.Contribute)]` example in `WorkflowHttpFunction.cs` comments for admin route pattern | `Functions/WorkflowHttpFunction.cs` — §3.8 |

---

## Group 4 — Authorization Middleware

| ID | Status | Priority | Task | Reference |
|---|---|---|---|---|
| MWA-01 | ✅ | Critical | Implement `WorkflowAuthorizationMiddleware` — `/public/*` pass-through; `/secure/*` and `/services/*` enforce principal + policy | `Auth/Middleware/WorkflowAuthorizationMiddleware.cs` — §3.1.3 |
| MWA-02 | ✅ | Critical | Integrate `IRouteAuthorizationRegistry` in middleware to resolve per-route `WorkflowPermission` requirements | `Auth/Middleware/WorkflowAuthorizationMiddleware.cs` |
| MWA-03 | ✅ | Critical | Integrate `IWorkflowPolicyMatcher` in middleware — delegate policy evaluation; map `Allowed/Forbidden/NoPolicyFound` to response | `Auth/Middleware/WorkflowAuthorizationMiddleware.cs` |
| MWA-04 | ✅ | High | Add dev-bypass header (`X-WW-Bypass-Auth: local-dev-bypass`) gated on `AZURE_FUNCTIONS_ENVIRONMENT == Development` | `Auth/Middleware/WorkflowAuthorizationMiddleware.cs` |
| MWA-05 | 🔲 | High | Inject `AuditLogger` into `WorkflowAuthorizationMiddleware`; emit audit event on 401 and 403 outcomes — §3.7.5 | `Auth/Middleware/WorkflowAuthorizationMiddleware.cs`, `Security/AuditLogger.cs` |
| MWA-06 | 🔲 | Medium | Add `WorkflowName` extraction test cases — paths with suffix, sub-folders, URL-encoded spaces | `Tests/Auth/WorkflowAuthorizationMiddlewareTests.cs` (new) |
| MWA-07 | 🔲 | Low | Add correlation ID (`X-WW-Correlation-Id`) to 401/403 JSON error responses for distributed tracing | `Auth/Middleware/WorkflowAuthorizationMiddleware.cs` |

---

## Group 5 — DI Wiring & Startup

| ID | Status | Priority | Task | Reference |
|---|---|---|---|---|
| DI-01 | ✅ | Critical | Register `IWorkflowAuthPolicyLoader` singleton in `ServiceCollectionExtensions.AddCoreServices` | `Infrastructure/ServiceCollectionExtensions.cs` |
| DI-02 | ✅ | Critical | Register `IWorkflowPolicyMatcher` singleton in `ServiceCollectionExtensions.AddCoreServices` | `Infrastructure/ServiceCollectionExtensions.cs` |
| DI-03 | ✅ | Critical | Register `IRouteAuthorizationRegistry` singleton via `RouteAuthorizationRegistry.BuildFrom(typeof(WorkflowHttpFunction))` | `Infrastructure/ServiceCollectionExtensions.cs` |
| DI-04 | ✅ | Critical | Register `IPrincipalParser` chain: `EasyAuthPrincipalParser` first, `BearerTokenPrincipalParser` second | `Infrastructure/ServiceCollectionExtensions.cs` |
| DI-05 | ✅ | Critical | Register middleware pipeline in order: `EasyAuthRedirectMiddleware → ClaimsPrincipalBuilderMiddleware → WorkflowAuthorizationMiddleware` | `Infrastructure/HostBuilderExtensions.cs` |
| DI-06 | 🔲 | High | Register `EntraAuthOptions` as DI singleton (see AUTH-09) | `Infrastructure/ServiceCollectionExtensions.cs` |
| DI-07 | 🔲 | Medium | Register `AuditLogger` in DI for injection into `WorkflowAuthorizationMiddleware` (see MWA-05) | `Infrastructure/ServiceCollectionExtensions.cs` |

---

## Group 6 — Unit Tests

| ID | Status | Priority | Task | Reference |
|---|---|---|---|---|
| TST-01 | 🔲 | Critical | `WorkflowPolicyMatcherTests` — Allow (group match + sufficient permissions) | `Tests/Auth/WorkflowPolicyMatcherTests.cs` |
| TST-02 | 🔲 | Critical | `WorkflowPolicyMatcherTests` — DenyGroup (caller not in any allowed group) | `Tests/Auth/WorkflowPolicyMatcherTests.cs` |
| TST-03 | 🔲 | Critical | `WorkflowPolicyMatcherTests` — DenyPermission (in group but missing flag) | `Tests/Auth/WorkflowPolicyMatcherTests.cs` |
| TST-04 | 🔲 | Critical | `WorkflowPolicyMatcherTests` — NoPolicyFound returns Allow + warning | `Tests/Auth/WorkflowPolicyMatcherTests.cs` |
| TST-05 | 🔲 | High | `RouteAuthorizationRegistryTests` — attribute present, correct mapping; absent attribute → default; multiple types combined | `Tests/Auth/RouteAuthorizationRegistryTests.cs` |
| TST-06 | 🔲 | High | `EasyAuthPrincipalParserTests` — valid header, roles, name; malformed base64; missing header | `Tests/Auth/EasyAuthPrincipalParserTests.cs` |
| TST-07 | 🔲 | High | `BearerTokenPrincipalParserTests` — mock `IConfiguration<OpenIdConnectConfiguration>`; valid token; expired; wrong aud; wrong iss | `Tests/Auth/BearerTokenPrincipalParserTests.cs` |
| TST-08 | 🔲 | High | `WorkflowAuthPolicyLoaderTests` — policy count; execute-only filter; UPN-style entry | `Tests/Auth/WorkflowAuthPolicyLoaderTests.cs` |
| TST-09 | 🔲 | High | `WorkflowClaimsPrincipalTests` — IsInAnyGroup UPN match; IsAppOnlyToken detection; permission flag logic | `Tests/Auth/WorkflowClaimsPrincipalTests.cs` |
| TST-10 | 🔲 | Medium | `WorkflowAuthorizationMiddlewareTests` — public path pass-through; 401 when no principal; 403 DenyGroup; dev-bypass | `Tests/Auth/WorkflowAuthorizationMiddlewareTests.cs` |
| TST-11 | 🔲 | Medium | `EasyAuthRedirectMiddlewareTests` — browser nav → 302; API caller no auth → 401; bearer present → next; public path → next | `Tests/Auth/EasyAuthRedirectMiddlewareTests.cs` |
| TST-12 | 🔲 | Low | Integration test: full middleware pipeline with fake Easy Auth header → policy allow | `Tests/Auth/MiddlewarePipelineIntegrationTests.cs` |

---

## Group 7 — Azure / Entra Provisioning

| ID | Status | Priority | Task | Reference |
|---|---|---|---|---|
| PRV-01 | ✅ | Critical | Create Entra resource app registration with `api://` identifier URI | Part 4 — Stage 1, 3 |
| PRV-02 | ✅ | Critical | Enable `enableIdTokenIssuance = true` on the app (fix AADSTS700054) | Part 4 — Stage 2 |
| PRV-03 | ✅ | Critical | Expose `user_impersonation` OAuth2 scope on the resource app (fix AADSTS650057) | Part 4 — Stage 3b |
| PRV-04 | ✅ | Critical | Declare all group and permission app roles (`WarewolfAdministrators`, `Permission.*`, etc.) | Part 4 — Stage 4 |
| PRV-05 | ✅ | Critical | Create service principal for the resource app | Part 4 — Stage 5 |
| PRV-06 | ✅ | Critical | Assign users to group and permission roles via Graph API | Part 4 — Stage 6 |
| PRV-07 | ✅ | Critical | Create/rotate client secret; store as `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` app setting | Part 4 — Stage 7 |
| PRV-08 | ✅ | Critical | Write function app settings: `WAREWOLF_ENTRA_TENANT_ID`, `WAREWOLF_ENTRA_AUDIENCE`, `WAREWOLF_SECURE_CONFIG` | Part 4 — Stage 8 |
| PRV-09 | ✅ | Critical | Enable Easy Auth V2 with Microsoft provider, correct issuer, audience, and `AllowAnonymous` action | Part 4 — Stage 9 |
| PRV-10 | ✅ | Critical | Assert `login.tokenStore.enabled = true` via ARM PUT (CLI does not reliably set this) | Part 4 — Stage 9c |
| PRV-11 | ✅ | High | End-to-end verification of all Easy Auth and app settings properties | Part 4 — Stage 10 |
| PRV-12 | ✅ | High | Generic SPA client app registration with delegated `user_impersonation` (recipe + MSAL.js sample) | Part 4 — Generic Client, Part 5 — §5.7 |
| PRV-13 | ✅ | High | Confidential web app / API registration with delegated + app-role assignments (recipe + MSAL.NET OBO sample) | Part 4 — Generic Client, Part 5 — §5.7 |
| PRV-14 | ✅ | Medium | Daemon / Managed Identity registration recipe + DefaultAzureCredential sample | Part 4 — Generic Client, Part 5 — §5.7 |
| PRV-15 | ✅ | Medium | `-DryRun` alias for `-WhatIfOnly` on provisioning script | `Scripts/Configure-WwExecutionAuth.ps1`, `Scripts/README.md` |
| PRV-16 | ✅ | Medium | Documented module-split layout (`Wwx.Provisioning.*.psm1`) for future extraction; current single-file orchestrator preserved | `Scripts/README.md` |
| PRV-17 | ✅ | Low | `-UseManagedIdentity` switch added to suppress optional secret rotations | `Scripts/Configure-WwExecutionAuth.ps1`, `Scripts/README.md` |
| PRV-18 | ✅ | Low | `-SecretLifetimeYears` validated (1–2y Entra cap) and documented | `Scripts/Configure-WwExecutionAuth.ps1`, `Scripts/README.md` |

---

## Group 8 — `secure.config` Policy Management

| ID | Status | Priority | Task | Reference |
|---|---|---|---|---|
| CFG-01 | ✅ | Critical | `secure.config.example.json` — document server-wide vs per-workflow entries | `Scripts/secure.config.example.json` |
| CFG-02 | ✅ | High | Encrypt `secure.config` at rest using AES-256-GCM key stored in Key Vault | `Security/SecureConfigLoader.cs` |
| CFG-03 | ✅ | High | `secure.config` CI/CD deployment documented (validate → encrypt → deploy with hot-reload) | `Scripts/README.md` |
| CFG-04 | ✅ | Medium | UPN entries documented in `secure.config.example.json` header block | `Scripts/secure.config.example.json` |
| CFG-05 | ✅ | Medium | `AuthenticationOverrideWorkflow` documented with `/login` flow notes | `Scripts/secure.config.example.json` |
| CFG-06 | ✅ | Low | `WorkflowAuthPolicyLoader.ValidateForLockout` warns when no entries hold `Execute = true` | `Auth/WorkflowAuthPolicyLoader.cs` |
| CFG-07 | ✅ | Low | JSON schema validates `secure.config` shape (server-vs-per-workflow, UPN-friendly groups) | `Scripts/secure.config.schema.json` |

---

## Group 9 — Observability & Auditing

| ID | Status | Priority | Task | Reference |
|---|---|---|---|---|
| OBS-01 | ✅ | High | Log policy loader initialization count at startup | `Auth/WorkflowAuthPolicyLoader.cs` |
| OBS-02 | 🔲 | High | Wire `AuditLogger` into `WorkflowAuthorizationMiddleware` — log 401/403 outcomes with caller identity and workflow name | §3.7.5, MWA-05 |
| OBS-03 | 🔲 | High | Add Application Insights custom metric `auth.policy.denied` on 403 outcomes | `Auth/Middleware/WorkflowAuthorizationMiddleware.cs` |
| OBS-04 | 🔲 | Medium | Add structured log property `{WorkflowName}` to all auth-related log statements for KQL query support | All auth middleware files |
| OBS-05 | 🔲 | Medium | Add `X-WW-Correlation-Id` header to all 401/403 responses (see MWA-07) and log alongside outcome | `Auth/Middleware/WorkflowAuthorizationMiddleware.cs` |
| OBS-06 | 🔲 | Low | Add startup health check that verifies `EntraAuthOptions.IsEnabled` and logs a warning if bearer validation is inactive | `Infrastructure/ServiceCollectionExtensions.cs` |

---

## Group 10 — Documentation

| ID | Status | Priority | Task | Reference |
|---|---|---|---|---|
| DOC-01 | ✅ | High | Write Part 1 — Architecture Plan | `docs/Part1-Architecture.md` |
| DOC-02 | ✅ | High | Write Part 3 — Detailed Implementation Plan | `docs/Part3-ImplementationPlan.md` |
| DOC-03 | ✅ | High | Write Part 4 — Resource Provisioning Step-by-Step | `docs/Part4-ResourceProvisioning.md` |
| DOC-04 | ✅ | High | Write Part 5 — Client Token Management Guide | `docs/Part5-ClientTokenManagement.md` |
| DOC-05 | ✅ | High | Write Part 6 — Full Implementation Task List (this document) | `docs/Part6-FullImplementationTaskList.md` |
| DOC-06 | ✅ | Medium | Part 2 — Data Flow Diagrams (Mermaid for browser, bearer, app-only, OBO, HMAC, hot-reload, decision matrix) | `docs/Part2-DataFlowDiagrams.md` |
| DOC-07 | ✅ | Medium | XML doc-comments on all public auth interfaces, attributes, models and `WorkflowClaimsPrincipal` | `Auth/` public types |
| DOC-08 | ✅ | Low | `CONTRIBUTING.md` covers parser, permission, route, secure.config extension paths and reviewer checklist | `Warewolf.Execution.Lightweight/CONTRIBUTING.md` |
| DOC-09 | ✅ | Low | Cross-doc review pass; Parts 1–6 + CONTRIBUTING + Scripts/README cross-reference each item | `docs/`, `Scripts/README.md` |

---

## Summary by Status

| Status | Count |
|---|---|
| ✅ Done | 36 |
| 🔲 Pending | 36 |
| ⚠ Partial | 0 |
| 🚫 Blocked | 0 |
| **Total** | **72** |

### Top 5 Highest-Priority Pending Items

| ID | Task |
|---|---|
| AUTH-09 | Register `EntraAuthOptions` as DI singleton for testability |
| MWA-05 | Wire `AuditLogger` into authorization middleware for 401/403 audit trail |
| POL-08 | `secure.config` hot-reload via `FileSystemWatcher` |
| TST-01–04 | `WorkflowPolicyMatcher` unit tests (core auth logic) |
| PRV-12–13 | Generic client app registration docs (SPA + daemon) |
