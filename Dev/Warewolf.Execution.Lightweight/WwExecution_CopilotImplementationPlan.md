# WwExecution — Azure Function Authentication & Authorisation
## GitHub Copilot Implementation Plan
### Target: Visual Studio 2022+ · .NET 8 · Azure Functions v4 Isolated Worker

---

## How to Use This Plan

This plan is fully automated. It contains:

1. **Section 1** — a PowerShell scaffold script (`Scaffold-WwExecution.ps1`) that creates
   the entire project structure and writes every file with its complete starting content.
   Run it once and the project is ready to open in Visual Studio.

2. **Section 2** — GitHub Copilot prompts for each file, used to refine or regenerate
   individual files inside Visual Studio after the scaffold runs.

3. **Section 3** — wiring verification, local test script, and deployment script.

**Workflow:**
```
Step 1: Run Scaffold-WwExecution.ps1  →  creates all files with full content
Step 2: Open WwExecution.sln in Visual Studio 2022
Step 3: dotnet restore  →  pulls NuGet packages
Step 4: Use Copilot Chat to refine individual files if needed (Section 2)
Step 5: Run LocalTestHelper.ps1 to verify locally
Step 6: Run Deploy-WwExecution.ps1 to publish to Azure
```

---

## Section 1 — Scaffold Script (Run This First)

Save this file as `Scaffold-WwExecution.ps1` anywhere on your machine and run it.
It will create the full project under a `WwExecution` folder in the current directory.

```powershell
#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Scaffolds the complete WwExecution Azure Functions project structure,
    writes all source files, config files, and project files.

.DESCRIPTION
    Run this script once in the directory where you want the project created.
    It creates WwExecution/ with all folders and files pre-populated.
    After running: open WwExecution/WwExecution.sln in Visual Studio 2022.

.EXAMPLE
    cd C:\Projects
    ./Scaffold-WwExecution.ps1

    # Then open in Visual Studio
    start WwExecution/WwExecution.sln
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ─────────────────────────────────────────────────────────────────────────────
# HELPERS
# ─────────────────────────────────────────────────────────────────────────────

function Write-Step([string]$msg) {
    Write-Host "`n  ━━━ $msg" -ForegroundColor Cyan
}

function Write-Ok([string]$msg) {
    Write-Host "    ✓ $msg" -ForegroundColor Green
}

function New-Dir([string]$path) {
    if (-not (Test-Path $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
}

function Write-File([string]$path, [string]$content) {
    $dir = Split-Path $path -Parent
    if ($dir -and -not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    Set-Content -Path $path -Value $content -Encoding UTF8 -NoNewline
    Write-Ok $path
}

# ─────────────────────────────────────────────────────────────────────────────
# ROOT
# ─────────────────────────────────────────────────────────────────────────────

$root = Join-Path $PWD "WwExecution"
New-Dir $root

Write-Step "Creating project structure under: $root"

# ─────────────────────────────────────────────────────────────────────────────
# DIRECTORY TREE
# ─────────────────────────────────────────────────────────────────────────────

@(
    "src/Auth/Middleware"
    "src/Auth/Models"
    "src/Config"
    "src/Functions"
) | ForEach-Object { New-Dir (Join-Path $root $_) }

Write-Ok "Directory tree created"

# ═════════════════════════════════════════════════════════════════════════════
# FILE: WwExecution.csproj
# ═════════════════════════════════════════════════════════════════════════════

Write-Step "Writing project files"

Write-File (Join-Path $root "WwExecution.csproj") @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <AzureFunctionsVersion>v4</AzureFunctionsVersion>
    <OutputType>Exe</OutputType>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>WwExecution</RootNamespace>
    <AssemblyName>WwExecution</AssemblyName>
    <Optimize>false</Optimize>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Azure.Functions.Worker"                  Version="1.23.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk"              Version="1.17.4" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Http"  Version="3.2.0" />
    <PackageReference Include="Microsoft.Extensions.Logging"                      Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection"          Version="8.0.1" />
  </ItemGroup>

  <ItemGroup>
    <None Update="secure.config">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
    <None Update="local.settings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <ExcludeFromPublish>true</ExcludeFromPublish>
    </None>
  </ItemGroup>
</Project>
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: host.json
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "host.json") @'
{
  "version": "2.0",
  "extensions": {
    "http": {
      "routePrefix": ""
    }
  },
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true
      }
    },
    "logLevel": {
      "default":          "Information",
      "WwExecution":      "Debug",
      "Host.Results":     "Error",
      "Function":         "Error"
    }
  }
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: local.settings.json
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "local.settings.json") @'
{
  "_comment": "DO NOT COMMIT THIS FILE — contains secrets. It is listed in .gitignore.",
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage":                          "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME":                     "dotnet-isolated",
    "AZURE_CLIENT_ID":                              "<paste-your-client-id>",
    "AZURE_TENANT_ID":                              "<paste-your-tenant-id>",
    "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET":     "<paste-your-client-secret>"
  }
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: .gitignore
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root ".gitignore") @'
# Build outputs
bin/
obj/
publish/

# Local Azure Functions settings (contains secrets)
local.settings.json

# Visual Studio
.vs/
*.user
*.suo

# Rider
.idea/

# OS
.DS_Store
Thumbs.db
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: secure.config
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "secure.config") @'
{
  "_comment": "Workflow authorization policies. RequiredRoles uses OR logic (user needs at least ONE). RequiredPermissions uses AND logic (user needs ALL listed).",
  "policies": [
    {
      "workflow":            "order",
      "requiredRoles":       ["WarewolfAdministrators"],
      "requiredPermissions": ["Execute", "View"]
    },
    {
      "workflow":            "report",
      "requiredRoles":       ["WarewolfAdministrators"],
      "requiredPermissions": ["View"]
    },
    {
      "workflow":            "deploy",
      "requiredRoles":       ["WarewolfAdministrators"],
      "requiredPermissions": ["Execute", "DeployTo", "DeployFrom"]
    },
    {
      "workflow":            "audit",
      "requiredRoles":       ["WarewolfAdministrators"],
      "requiredPermissions": ["View", "Administrator"]
    },
    {
      "workflow":            "contribute",
      "requiredRoles":       ["WarewolfAdministrators"],
      "requiredPermissions": ["Contribute", "View"]
    }
  ]
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: Program.cs
# ═════════════════════════════════════════════════════════════════════════════

Write-Step "Writing source files"

Write-File (Join-Path $root "Program.cs") @'
/*
 * ARCHITECTURE CONTEXT — WwExecution Auth Pipeline
 *
 * AZURE ENVIRONMENT:
 *   Easy Auth is enabled with Microsoft identity provider (Azure Entra ID).
 *   Azure Policy forces: requireAuthentication=true, unauthenticatedClientAction=RedirectToLoginPage.
 *   We cannot override this via ARM API — a Modify policy reverts it on every PUT.
 *   Solution: handle ALL authentication and authorisation logic inside the function
 *   middleware pipeline. Easy Auth still validates and decodes tokens — we just do
 *   not rely on it to block requests.
 *
 * MIDDLEWARE ORDER (must not be changed):
 *   1. EasyAuthRedirectMiddleware
 *      - /public/* routes:  pass straight through, no token check
 *      - /secure/* no token: return 401 JSON before Easy Auth redirect fires
 *      - /secure/* with token: pass through to next middleware
 *
 *   2. ClaimsPrincipalBuilderMiddleware
 *      - Reads X-MS-CLIENT-PRINCIPAL header injected by Easy Auth
 *      - Decodes base64 JSON into ClaimsPrincipal → WorkflowClaimsPrincipal
 *      - Stores result in FunctionContext.Items[AuthConstants.PrincipalContextKey]
 *      - Stores Anonymous() if header is absent (public routes)
 *
 *   3. WorkflowAuthorizationMiddleware
 *      - /public/*: skip — no policy enforcement
 *      - /secure/*: extract workflow name from path (e.g. "order" from "/secure/order.json")
 *      - Load WorkflowAuthPolicy from secure.config via ISecureConfigLoader
 *      - Role check:       OR  — user needs at least ONE required role
 *      - Permission check: AND — user needs ALL required permissions
 *      - Returns 403 if policy missing, role mismatch, or permission mismatch
 *
 * TOKEN TYPES:
 *   User Impersonation (delegated) — has "scp" claim, has "upn" claim
 *     Browser clients. Per-user role enforcement fully works.
 *   App-Only (client credentials)  — no "scp", no "upn"
 *     Automated services. Roles from app-level assignments.
 *
 * ROLE / PERMISSION SCHEME:
 *   Roles        = broad groups      e.g. "WarewolfAdministrators", "PUBLIC"
 *   Permissions  = fine capabilities e.g. "Permission.View", "Permission.Execute"
 *   Both are Entra ID App Roles on the app registration.
 *   Permissions are identified by the "Permission." prefix in the claim value.
 *
 * SECURE.CONFIG:
 *   Deployed alongside the function binary.
 *   Maps workflow name → required roles + permissions.
 *   Loaded once at startup by SecureConfigLoader (singleton).
 */

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WwExecution.Auth.Middleware;
using WwExecution.Config;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        // ── Middleware pipeline — ORDER IS CRITICAL ──────────────────────────
        // 1. Intercept unauthenticated requests to /secure/* before Easy Auth
        //    redirect fires. Pass /public/* straight through.
        worker.UseMiddleware<EasyAuthRedirectMiddleware>();

        // 2. Decode X-MS-CLIENT-PRINCIPAL Easy Auth header into a strongly-typed
        //    WorkflowClaimsPrincipal and store it in FunctionContext.Items.
        worker.UseMiddleware<ClaimsPrincipalBuilderMiddleware>();

        // 3. Enforce secure.config role + permission policies on /secure/* routes.
        //    Returns 403 if the caller lacks required roles or permissions.
        worker.UseMiddleware<WorkflowAuthorizationMiddleware>();
    })
    .ConfigureServices(services =>
    {
        // SecureConfigLoader parses secure.config once at startup — singleton.
        services.AddSingleton<ISecureConfigLoader, SecureConfigLoader>();
        services.AddLogging(logging => logging.AddConsole());
    })
    .Build();

await host.RunAsync();
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: src/Auth/Models/WorkflowPermission.cs
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "src/Auth/Models/WorkflowPermission.cs") @'
namespace WwExecution.Auth.Models;

/// <summary>
/// Fine-grained permission flags assigned to users via Entra ID App Roles.
/// Each flag corresponds to a "Permission.*" app role value on the registration.
/// Multiple flags are combined with bitwise OR; all required flags must be present (AND logic).
/// </summary>
[Flags]
public enum WorkflowPermission
{
    /// <summary>No permissions.</summary>
    None          = 0,

    /// <summary>Can view workflow outputs and execution status.</summary>
    View          = 1 << 0,

    /// <summary>Can trigger workflow execution.</summary>
    Execute       = 1 << 1,

    /// <summary>Can create and modify workflow definitions.</summary>
    Contribute    = 1 << 2,

    /// <summary>Can deploy workflows to a target environment.</summary>
    DeployTo      = 1 << 3,

    /// <summary>Can pull workflow deployments from a source environment.</summary>
    DeployFrom    = 1 << 4,

    /// <summary>Full permission over all workflow operations.</summary>
    Administrator = 1 << 5,

    /// <summary>All permissions combined.</summary>
    All           = View | Execute | Contribute | DeployTo | DeployFrom | Administrator,
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: src/Auth/Models/WorkflowAuthPolicy.cs
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "src/Auth/Models/WorkflowAuthPolicy.cs") @'
namespace WwExecution.Auth.Models;

/// <summary>
/// Authorisation policy for a single named workflow.
/// Loaded from secure.config at startup.
/// </summary>
/// <remarks>
/// Role check uses OR logic — the caller must have at least ONE of <see cref="RequiredRoles"/>.
/// Permission check uses AND logic — the caller must have ALL flags in <see cref="RequiredPermissions"/>.
/// </remarks>
public sealed record WorkflowAuthPolicy(
    /// <summary>Workflow name (lowercase), e.g. "order".</summary>
    string WorkflowName,

    /// <summary>
    /// At least one role must match. Uses OR logic.
    /// Values must match Entra ID app role value field exactly (e.g. "WarewolfAdministrators").
    /// </summary>
    IReadOnlyList<string> RequiredRoles,

    /// <summary>
    /// All permission flags must be present. Uses AND logic.
    /// </summary>
    WorkflowPermission RequiredPermissions)
{
    /// <summary>
    /// Creates a <see cref="WorkflowAuthPolicy"/> from raw values.
    /// </summary>
    public static WorkflowAuthPolicy Create(
        string                    workflowName,
        IEnumerable<string>       requiredRoles,
        WorkflowPermission        requiredPermissions) =>
        new(
            workflowName.ToLowerInvariant(),
            requiredRoles.ToList().AsReadOnly(),
            requiredPermissions);
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: src/Auth/Models/AuthConstants.cs
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "src/Auth/Models/AuthConstants.cs") @'
namespace WwExecution.Auth.Models;

/// <summary>
/// Centralised string constants for the WwExecution auth pipeline.
/// All magic strings live here — never hardcode them in middleware or functions.
/// </summary>
public static class AuthConstants
{
    // ── Claim type URIs ───────────────────────────────────────────────────────

    /// <summary>Entra ID object identifier claim.</summary>
    public const string ObjectIdentifier  = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    /// <summary>Standard name claim URI.</summary>
    public const string Name              = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

    /// <summary>App roles claim name as used in Entra ID v2.0 tokens.</summary>
    public const string Roles             = "roles";

    /// <summary>Preferred username claim (UPN) in v2.0 tokens.</summary>
    public const string PreferredUsername = "preferred_username";

    /// <summary>Identity provider claim — set to "aad" by Easy Auth.</summary>
    public const string IdentityProvider  = "idp";

    /// <summary>Scope claim — present in delegated (user impersonation) tokens only.</summary>
    public const string Scope             = "scp";

    // ── Entra ID App Role values (must match the 'value' field exactly) ───────

    /// <summary>Warewolf Administrators role value.</summary>
    public const string RoleWarewolfAdministrators = "WarewolfAdministrators";

    /// <summary>Public access role value.</summary>
    public const string RolePublic                 = "PUBLIC";

    // ── Permission App Role values (Permission.* prefix) ─────────────────────

    /// <summary>View workflow outputs.</summary>
    public const string PermissionView          = "Permission.View";

    /// <summary>Execute workflows.</summary>
    public const string PermissionExecute       = "Permission.Execute";

    /// <summary>Contribute workflow definitions.</summary>
    public const string PermissionContribute    = "Permission.Contribute";

    /// <summary>Deploy workflows to a target.</summary>
    public const string PermissionDeployTo      = "Permission.DeployTo";

    /// <summary>Pull deployments from a source.</summary>
    public const string PermissionDeployFrom    = "Permission.DeployFrom";

    /// <summary>Full administrative permission.</summary>
    public const string PermissionAdministrator = "Permission.Administrator";

    // ── FunctionContext.Items keys ────────────────────────────────────────────

    /// <summary>
    /// Key used to store the <see cref="WwExecution.Auth.WorkflowClaimsPrincipal"/>
    /// in <see cref="Microsoft.Azure.Functions.Worker.FunctionContext.Items"/>.
    /// </summary>
    public const string PrincipalContextKey = "WorkflowClaimsPrincipal";

    // ── Route prefixes ────────────────────────────────────────────────────────

    /// <summary>Prefix for anonymous public routes.</summary>
    public const string PublicRoutePrefix = "/public/";

    /// <summary>Prefix for authenticated secure routes.</summary>
    public const string SecureRoutePrefix = "/secure/";

    // ── Easy Auth HTTP headers ────────────────────────────────────────────────

    /// <summary>Base64-encoded JSON principal injected by Easy Auth.</summary>
    public const string ClientPrincipalHeader     = "X-MS-CLIENT-PRINCIPAL";

    /// <summary>Display name injected by Easy Auth.</summary>
    public const string ClientPrincipalNameHeader = "X-MS-CLIENT-PRINCIPAL-NAME";

    /// <summary>Identity provider injected by Easy Auth.</summary>
    public const string ClientPrincipalIdpHeader  = "X-MS-CLIENT-PRINCIPAL-IDP";
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: src/Auth/WorkflowClaimsPrincipal.cs
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "src/Auth/WorkflowClaimsPrincipal.cs") @'
using System.Security.Claims;
using WwExecution.Auth.Models;

namespace WwExecution.Auth;

/// <summary>
/// Strongly-typed ClaimsPrincipal for WwExecution.
/// Built by <see cref="Middleware.ClaimsPrincipalBuilderMiddleware"/> from the
/// X-MS-CLIENT-PRINCIPAL header injected by Azure Easy Auth.
/// Supports both delegated (user impersonation) and app-only (client credentials) tokens.
/// </summary>
public sealed class WorkflowClaimsPrincipal : ClaimsPrincipal
{
    // Maps WorkflowPermission flag → Permission.* claim value
    private static readonly IReadOnlyDictionary<WorkflowPermission, string> PermissionFlagMap =
        new Dictionary<WorkflowPermission, string>
        {
            [WorkflowPermission.View]          = AuthConstants.PermissionView,
            [WorkflowPermission.Execute]       = AuthConstants.PermissionExecute,
            [WorkflowPermission.Contribute]    = AuthConstants.PermissionContribute,
            [WorkflowPermission.DeployTo]      = AuthConstants.PermissionDeployTo,
            [WorkflowPermission.DeployFrom]    = AuthConstants.PermissionDeployFrom,
            [WorkflowPermission.Administrator] = AuthConstants.PermissionAdministrator,
        };

    /// <summary>Initialises a new instance from a <see cref="ClaimsIdentity"/>.</summary>
    public WorkflowClaimsPrincipal(ClaimsIdentity identity) : base(identity)
    {
        UserId   = FindFirst(AuthConstants.ObjectIdentifier)?.Value
                ?? FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;

        UserName = FindFirst(ClaimTypes.Name)?.Value
                ?? FindFirst(AuthConstants.PreferredUsername)?.Value
                ?? string.Empty;

        IsUserToken    = Claims.Any(c => c.Type == AuthConstants.Scope);
        IsAppOnlyToken = !IsUserToken;
        CallerIdentity = IsUserToken ? UserName : $"app:{UserId}";

        var roleClaims = Claims
            .Where(c => c.Type is AuthConstants.Roles or ClaimTypes.Role)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        Roles = roleClaims
            .Where(v => !v.StartsWith("Permission.", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();

        Permissions = roleClaims
            .Where(v => v.StartsWith("Permission.", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Entra ID object ID of the user or app service principal.</summary>
    public string UserId { get; }

    /// <summary>UPN or display name of the signed-in user. Empty for app-only tokens.</summary>
    public string UserName { get; }

    /// <summary>
    /// True when the token was issued via user impersonation (delegated flow).
    /// The "scp" claim is present only in delegated tokens.
    /// </summary>
    public bool IsUserToken { get; }

    /// <summary>True when the token was issued via client credentials (app-only flow).</summary>
    public bool IsAppOnlyToken { get; }

    /// <summary>
    /// Human-readable caller label for logging.
    /// UserName for delegated tokens; "app:{UserId}" for app-only tokens.
    /// </summary>
    public string CallerIdentity { get; }

    /// <summary>
    /// App roles assigned to this principal (excludes Permission.* values).
    /// e.g. ["WarewolfAdministrators", "PUBLIC"]
    /// </summary>
    public IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Permission app roles assigned to this principal (Permission.* prefix only).
    /// e.g. ["Permission.View", "Permission.Execute"]
    /// </summary>
    public IReadOnlyList<string> Permissions { get; }

    // ── Role methods ──────────────────────────────────────────────────────────

    /// <summary>Returns true if this principal has the specified role (case-insensitive).</summary>
    public bool HasRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if this principal has at least ONE of the specified roles (OR logic).
    /// </summary>
    public bool HasAnyRole(IEnumerable<string> roles) =>
        roles.Any(HasRole);

    /// <summary>
    /// Returns true if this principal has ALL of the specified roles (AND logic).
    /// </summary>
    public bool HasAllRoles(IEnumerable<string> roles) =>
        roles.All(HasRole);

    // ── Permission methods ────────────────────────────────────────────────────

    /// <summary>Returns true if this principal has the specified permission claim (case-insensitive).</summary>
    public bool HasPermission(string permissionValue) =>
        Permissions.Contains(permissionValue, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if this principal has ALL permission flags specified (AND logic).
    /// Each flag is mapped to its "Permission.*" claim value and checked individually.
    /// </summary>
    public bool HasPermissionFlag(WorkflowPermission permission)
    {
        if (permission == WorkflowPermission.None) return true;

        foreach (var (flag, claimValue) in PermissionFlagMap)
        {
            if (permission.HasFlag(flag) && !HasPermission(claimValue))
                return false;
        }
        return true;
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>Creates an unauthenticated anonymous principal with no claims.</summary>
    public static WorkflowClaimsPrincipal Anonymous() =>
        new(new ClaimsIdentity());

    // ── Overrides ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override string ToString() =>
        $"User:{UserName}|Roles:{string.Join(",", Roles)}|Perms:{Permissions.Count}";
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: src/Auth/Middleware/EasyAuthRedirectMiddleware.cs
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "src/Auth/Middleware/EasyAuthRedirectMiddleware.cs") @'
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using WwExecution.Auth.Models;

namespace WwExecution.Auth.Middleware;

/// <summary>
/// First middleware in the pipeline.
/// Handles the Azure Policy constraint that forces Easy Auth into RedirectToLoginPage mode.
/// Instead of allowing Easy Auth to redirect unauthenticated requests (which breaks API clients),
/// this middleware intercepts them and returns a clean 401 JSON response.
///
/// /public/* routes are passed straight through with no token check.
/// /secure/* routes without a token receive a 401 immediately.
/// /secure/* routes with a valid token header are passed to the next middleware.
/// </summary>
public sealed class EasyAuthRedirectMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<EasyAuthRedirectMiddleware> _logger;

    /// <summary>Initialises the middleware with a logger.</summary>
    public EasyAuthRedirectMiddleware(ILogger<EasyAuthRedirectMiddleware> logger)
        => _logger = logger;

    /// <inheritdoc/>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is null)
        {
            await next(context);
            return;
        }

        var path = request.Url.AbsolutePath;

        // Public routes bypass all auth checks
        if (path.StartsWith(AuthConstants.PublicRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Public route {Path} — bypassing auth check", path);
            await next(context);
            return;
        }

        // Check for Easy Auth principal header (set by Azure after token validation)
        var hasPrincipalHeader = request.Headers
            .TryGetValues(AuthConstants.ClientPrincipalHeader, out var principalValues)
            && principalValues.Any(v => !string.IsNullOrWhiteSpace(v));

        // Check for raw Bearer token in Authorization header
        var hasAuthHeader = request.Headers
            .TryGetValues("Authorization", out var authValues)
            && authValues.Any(v => v.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase));

        if (!hasPrincipalHeader && !hasAuthHeader)
        {
            _logger.LogWarning(
                "Unauthenticated request to protected route {Path} — returning 401", path);

            var response = request.CreateResponse(HttpStatusCode.Unauthorized);
            response.Headers.Add("Content-Type", "application/json");
            response.Headers.Add("WWW-Authenticate", "Bearer realm=\"wwexecution\"");
            await response.WriteStringAsync(
                $"{{\"error\":\"unauthorized\",\"message\":\"A valid Bearer token is required.\",\"path\":\"{path}\"}}");

            context.GetInvocationResult().Value = response;
            return;
        }

        _logger.LogDebug("Authenticated request to {Path} — passing to next middleware", path);
        await next(context);
    }
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: src/Auth/Middleware/ClaimsPrincipalBuilderMiddleware.cs
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "src/Auth/Middleware/ClaimsPrincipalBuilderMiddleware.cs") @'
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using WwExecution.Auth.Models;

namespace WwExecution.Auth.Middleware;

/// <summary>
/// Second middleware in the pipeline.
/// Reads the X-MS-CLIENT-PRINCIPAL header injected by Azure Easy Auth,
/// decodes it from base64 JSON, and builds a <see cref="WorkflowClaimsPrincipal"/>
/// stored in <see cref="FunctionContext.Items"/> for downstream use.
/// Stores an anonymous principal if the header is absent or invalid — never throws.
/// </summary>
public sealed class ClaimsPrincipalBuilderMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ClaimsPrincipalBuilderMiddleware> _logger;

    /// <summary>Initialises the middleware with a logger.</summary>
    public ClaimsPrincipalBuilderMiddleware(ILogger<ClaimsPrincipalBuilderMiddleware> logger)
        => _logger = logger;

    /// <inheritdoc/>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is not null)
        {
            var principal = BuildPrincipal(request);
            context.Items[AuthConstants.PrincipalContextKey] = principal;

            _logger.LogDebug(
                "Principal built: User={User} Authenticated={Auth} Roles=[{Roles}] Permissions={PermCount}",
                principal.UserName,
                principal.Identity?.IsAuthenticated,
                string.Join(", ", principal.Roles),
                principal.Permissions.Count);
        }

        await next(context);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private WorkflowClaimsPrincipal BuildPrincipal(HttpRequestData request)
    {
        if (!request.Headers.TryGetValues(AuthConstants.ClientPrincipalHeader, out var headerValues))
            return WorkflowClaimsPrincipal.Anonymous();

        var encoded = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(encoded))
            return WorkflowClaimsPrincipal.Anonymous();

        try
        {
            var json     = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var document = JsonDocument.Parse(json);
            var root     = document.RootElement;
            var claims   = new List<Claim>();

            // Identity provider
            if (root.TryGetProperty("auth_typ", out var authTyp))
                claims.Add(new Claim(AuthConstants.IdentityProvider, authTyp.GetString() ?? string.Empty));

            // All claims from the Easy Auth payload
            if (root.TryGetProperty("claims", out var claimsArray))
            {
                foreach (var element in claimsArray.EnumerateArray())
                {
                    var typ = element.TryGetProperty("typ", out var t) ? t.GetString() : null;
                    var val = element.TryGetProperty("val", out var v) ? v.GetString() : null;

                    if (!string.IsNullOrEmpty(typ) && val is not null)
                        claims.Add(new Claim(NormalizeClaimType(typ), val));
                }
            }

            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "EasyAuth",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            return new WorkflowClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decode X-MS-CLIENT-PRINCIPAL header — using anonymous principal");
            return WorkflowClaimsPrincipal.Anonymous();
        }
    }

    private static string NormalizeClaimType(string typ) => typ switch
    {
        "http://schemas.microsoft.com/identity/claims/objectidentifier" => ClaimTypes.NameIdentifier,
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"   => ClaimTypes.Name,
        "roles"                                                         => ClaimTypes.Role,
        _                                                               => typ,
    };
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: src/Auth/Middleware/WorkflowAuthorizationMiddleware.cs
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "src/Auth/Middleware/WorkflowAuthorizationMiddleware.cs") @'
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using WwExecution.Auth.Models;
using WwExecution.Config;

namespace WwExecution.Auth.Middleware;

/// <summary>
/// Third middleware in the pipeline.
/// Enforces secure.config role and permission policies on /secure/* routes.
/// /public/* routes are passed straight through.
/// Role check: OR logic — caller needs at least ONE required role.
/// Permission check: AND logic — caller needs ALL required permissions.
/// </summary>
public sealed class WorkflowAuthorizationMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ISecureConfigLoader                    _configLoader;
    private readonly ILogger<WorkflowAuthorizationMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Initialises the middleware with config loader and logger.</summary>
    public WorkflowAuthorizationMiddleware(
        ISecureConfigLoader configLoader,
        ILogger<WorkflowAuthorizationMiddleware> logger)
    {
        _configLoader = configLoader;
        _logger       = logger;
    }

    /// <inheritdoc/>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is null)
        {
            await next(context);
            return;
        }

        var path = request.Url.AbsolutePath;

        // Public routes — no policy enforcement
        if (path.StartsWith(AuthConstants.PublicRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // Non-secure, non-public routes — pass through
        if (!path.StartsWith(AuthConstants.SecureRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // ── Secure route — enforce policy ─────────────────────────────────────

        // Get principal built by ClaimsPrincipalBuilderMiddleware
        if (!context.Items.TryGetValue(AuthConstants.PrincipalContextKey, out var principalObj)
            || principalObj is not Auth.WorkflowClaimsPrincipal principal
            || principal.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("No authenticated principal for secure route {Path}", path);
            await WriteErrorAsync(request, context, HttpStatusCode.Unauthorized,
                "unauthorized", "Authentication required.", path);
            return;
        }

        // Extract workflow name from path: /secure/order.json → "order"
        var workflowName = ExtractWorkflowName(path);
        if (string.IsNullOrEmpty(workflowName))
        {
            await WriteErrorAsync(request, context, HttpStatusCode.BadRequest,
                "bad_request", "Could not determine workflow name from path.", path);
            return;
        }

        // Load policy from secure.config
        var policy = _configLoader.GetPolicy(workflowName);
        if (policy is null)
        {
            _logger.LogWarning(
                "No policy found for workflow '{Workflow}' — denying access to {User}",
                workflowName, principal.CallerIdentity);
            await WriteErrorAsync(request, context, HttpStatusCode.Forbidden,
                "forbidden", $"No policy configured for workflow '{workflowName}'.", path);
            return;
        }

        // Role check (OR logic)
        if (!principal.HasAnyRole(policy.RequiredRoles))
        {
            _logger.LogWarning(
                "Role check failed for '{User}' on '{Workflow}'. Has=[{Has}] Required=[{Required}]",
                principal.CallerIdentity, workflowName,
                string.Join(", ", principal.Roles),
                string.Join(", ", policy.RequiredRoles));

            await WriteErrorAsync(request, context, HttpStatusCode.Forbidden,
                "forbidden", "Insufficient role.", path,
                new { workflow = workflowName, required = policy.RequiredRoles });
            return;
        }

        // Permission check (AND logic)
        if (!principal.HasPermissionFlag(policy.RequiredPermissions))
        {
            _logger.LogWarning(
                "Permission check failed for '{User}' on '{Workflow}'. Required={Required}",
                principal.CallerIdentity, workflowName, policy.RequiredPermissions);

            await WriteErrorAsync(request, context, HttpStatusCode.Forbidden,
                "forbidden", "Insufficient permissions.", path,
                new { workflow = workflowName, required = policy.RequiredPermissions.ToString() });
            return;
        }

        _logger.LogInformation(
            "Authorised '{Caller}' for workflow '{Workflow}'",
            principal.CallerIdentity, workflowName);

        await next(context);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string? ExtractWorkflowName(string path)
    {
        var segment = path
            .Substring(AuthConstants.SecureRoutePrefix.Length)
            .Split('/')[0]
            .Split('?')[0];

        return Path.GetFileNameWithoutExtension(segment)
            .ToLowerInvariant() is var name && !string.IsNullOrEmpty(name)
            ? name
            : null;
    }

    private static async Task WriteErrorAsync(
        HttpRequestData request,
        FunctionContext context,
        HttpStatusCode  statusCode,
        string          error,
        string          message,
        string          path,
        object?         extra = null)
    {
        var body = new Dictionary<string, object>
        {
            ["error"]   = error,
            ["message"] = message,
            ["path"]    = path,
        };

        if (extra is not null)
        {
            foreach (var prop in extra.GetType().GetProperties())
                body[prop.Name.ToLowerInvariant()] = prop.GetValue(extra) ?? string.Empty;
        }

        var response = request.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(body, JsonOptions));
        context.GetInvocationResult().Value = response;
    }
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: src/Config/SecureConfigLoader.cs
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "src/Config/SecureConfigLoader.cs") @'
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WwExecution.Auth.Models;

namespace WwExecution.Config;

/// <summary>Loads and provides workflow auth policies from secure.config.</summary>
public interface ISecureConfigLoader
{
    /// <summary>
    /// Returns the policy for the named workflow, or null if not configured.
    /// </summary>
    WorkflowAuthPolicy? GetPolicy(string workflowName);

    /// <summary>Total number of loaded policies.</summary>
    int PolicyCount { get; }
}

/// <summary>
/// Singleton implementation of <see cref="ISecureConfigLoader"/>.
/// Parses secure.config once at application startup and caches the result.
/// Thread-safe by construction (immutable after init).
/// </summary>
public sealed class SecureConfigLoader : ISecureConfigLoader
{
    private readonly IReadOnlyDictionary<string, WorkflowAuthPolicy> _policies;
    private readonly ILogger<SecureConfigLoader>                     _logger;

    /// <inheritdoc/>
    public int PolicyCount => _policies.Count;

    /// <summary>Initialises and loads secure.config from the application base directory.</summary>
    public SecureConfigLoader(ILogger<SecureConfigLoader> logger)
    {
        _logger   = logger;
        _policies = LoadPolicies();
        _logger.LogInformation("SecureConfigLoader initialised with {Count} policies.", _policies.Count);
    }

    /// <inheritdoc/>
    public WorkflowAuthPolicy? GetPolicy(string workflowName) =>
        _policies.TryGetValue(workflowName.ToLowerInvariant(), out var policy) ? policy : null;

    // ── Private ───────────────────────────────────────────────────────────────

    private IReadOnlyDictionary<string, WorkflowAuthPolicy> LoadPolicies()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "secure.config");

        if (!File.Exists(configPath))
        {
            _logger.LogWarning(
                "secure.config not found at {Path}. All /secure/* routes will return 403.",
                configPath);
            return new Dictionary<string, WorkflowAuthPolicy>();
        }

        try
        {
            var json     = File.ReadAllText(configPath);
            var document = JsonDocument.Parse(json);
            var dict     = new Dictionary<string, WorkflowAuthPolicy>(StringComparer.OrdinalIgnoreCase);

            foreach (var policyEl in document.RootElement.GetProperty("policies").EnumerateArray())
            {
                var workflow = policyEl.GetProperty("workflow").GetString()!;

                var requiredRoles = policyEl
                    .GetProperty("requiredRoles")
                    .EnumerateArray()
                    .Select(e => e.GetString()!)
                    .ToList();

                var requiredPermissions = policyEl
                    .GetProperty("requiredPermissions")
                    .EnumerateArray()
                    .Select(e => e.GetString()!)
                    .Aggregate(
                        WorkflowPermission.None,
                        (acc, p) => acc | Enum.Parse<WorkflowPermission>(p, ignoreCase: true));

                dict[workflow.ToLowerInvariant()] =
                    WorkflowAuthPolicy.Create(workflow, requiredRoles, requiredPermissions);

                _logger.LogDebug(
                    "Loaded policy: workflow={Workflow} roles=[{Roles}] permissions={Perms}",
                    workflow,
                    string.Join(", ", requiredRoles),
                    requiredPermissions);
            }

            return dict;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to parse secure.config at {Path}. All /secure/* routes will return 403.",
                configPath);
            return new Dictionary<string, WorkflowAuthPolicy>();
        }
    }
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: src/Functions/PublicWorkflowFunction.cs
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "src/Functions/PublicWorkflowFunction.cs") @'
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WwExecution.Auth;
using WwExecution.Auth.Models;

namespace WwExecution.Functions;

/// <summary>
/// Handles anonymous public workflow execution requests.
/// Route: GET|POST /public/{workflow}.json
/// No authentication required. If a valid token is present, caller identity is included in response.
/// </summary>
public sealed class PublicWorkflowFunction
{
    private readonly ILogger<PublicWorkflowFunction> _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Initialises the function with a logger.</summary>
    public PublicWorkflowFunction(ILogger<PublicWorkflowFunction> logger)
        => _logger = logger;

    /// <summary>Executes a public workflow. No token required.</summary>
    [Function("PublicWorkflow")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",
            Route = "public/{workflow}.json")] HttpRequestData req,
        string workflow,
        FunctionContext context)
    {
        // Principal may be anonymous — that is fine for public routes
        var principal = context.Items.TryGetValue(AuthConstants.PrincipalContextKey, out var p)
            ? p as WorkflowClaimsPrincipal
            : null;

        var callerIdentity  = principal?.CallerIdentity ?? "anonymous";
        var isAuthenticated = principal?.Identity?.IsAuthenticated ?? false;
        var id              = req.Query["id"];

        _logger.LogInformation(
            "Public workflow '{Workflow}' executed by '{Caller}' (authenticated={Auth})",
            workflow, callerIdentity, isAuthenticated);

        var responseBody = new
        {
            workflow,
            status          = "executed",
            access          = "public",
            executedBy      = callerIdentity,
            isAuthenticated,
            parameters      = new { id },
            timestamp       = DateTimeOffset.UtcNow.ToString("O"),
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(responseBody, JsonOptions));
        return response;
    }
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# FILE: src/Functions/SecureWorkflowFunction.cs
# ═════════════════════════════════════════════════════════════════════════════

Write-File (Join-Path $root "src/Functions/SecureWorkflowFunction.cs") @'
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WwExecution.Auth;
using WwExecution.Auth.Models;

namespace WwExecution.Functions;

/// <summary>
/// Handles authenticated secure workflow execution requests.
/// Route: GET|POST /secure/{workflow}.json
/// By the time this function runs, WorkflowAuthorizationMiddleware has already
/// verified the principal has the required roles and permissions.
/// </summary>
public sealed class SecureWorkflowFunction
{
    private readonly ILogger<SecureWorkflowFunction> _logger;

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Initialises the function with a logger.</summary>
    public SecureWorkflowFunction(ILogger<SecureWorkflowFunction> logger)
        => _logger = logger;

    /// <summary>Executes a secure workflow. Bearer token with required roles required.</summary>
    [Function("SecureWorkflow")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",
            Route = "secure/{workflow}.json")] HttpRequestData req,
        string workflow,
        FunctionContext context)
    {
        // Middleware guarantees a valid principal exists here.
        // The null fallback is defensive — should never trigger in production.
        var principal = context.Items.TryGetValue(AuthConstants.PrincipalContextKey, out var p)
            ? p as WorkflowClaimsPrincipal
            : null;

        if (principal is null || principal.Identity?.IsAuthenticated != true)
        {
            var errResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            errResponse.Headers.Add("Content-Type", "application/json");
            await errResponse.WriteStringAsync("{\"error\":\"unauthorized\"}");
            return errResponse;
        }

        var id = req.Query["id"];

        _logger.LogInformation(
            "Secure workflow '{Workflow}' executed by '{Caller}' IsUserToken={IsUser} Roles=[{Roles}]",
            workflow,
            principal.CallerIdentity,
            principal.IsUserToken,
            string.Join(", ", principal.Roles));

        var responseBody = new
        {
            workflow,
            status      = "executed",
            access      = "secure",
            executedBy  = principal.CallerIdentity,
            isUserToken = principal.IsUserToken,
            userId      = principal.UserId,
            userName    = principal.UserName,
            roles       = principal.Roles,
            permissions = principal.Permissions,
            parameters  = new { id },
            timestamp   = DateTimeOffset.UtcNow.ToString("O"),
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(responseBody, JsonOptions));
        return response;
    }
}
'@

# ═════════════════════════════════════════════════════════════════════════════
# SUMMARY
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  Scaffold complete ✓                                " -ForegroundColor Green
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "  Project location: $root"
Write-Host ""
Write-Host "  Next steps:"
Write-Host "    1. cd WwExecution"                                             -ForegroundColor Cyan
Write-Host "    2. Fill in local.settings.json  (AZURE_CLIENT_ID etc.)"       -ForegroundColor Cyan
Write-Host "    3. dotnet restore"                                             -ForegroundColor Cyan
Write-Host "    4. dotnet build"                                               -ForegroundColor Cyan
Write-Host "    5. dotnet run --project WwExecution.csproj"                   -ForegroundColor Cyan
Write-Host "    6. Run LocalTestHelper.ps1 to verify endpoints"               -ForegroundColor Cyan
Write-Host ""

# Print the full file tree created
Write-Host "  Files created:" -ForegroundColor White
Get-ChildItem $root -Recurse -File | ForEach-Object {
    $rel = $_.FullName.Substring($root.Length + 1)
    Write-Host "    · $rel" -ForegroundColor Gray
}
```

---

## Section 2 — Copilot Refinement Prompts

After the scaffold runs, use these prompts in GitHub Copilot Chat
(`Ctrl+Alt+I` in Visual Studio) to refine individual files or generate
additional code. Open the target file first so Copilot has it as context.

### Refine `WorkflowClaimsPrincipal.cs`

```
Open file: src/Auth/WorkflowClaimsPrincipal.cs

Add a method named GetPermissionSummary() that returns a Dictionary<string, bool>
showing every known permission value (from AuthConstants) and whether this
principal has it. This is useful for diagnostic logging and the secure endpoint
response body.

Also add a property named PermissionFlags of type WorkflowPermission that returns
the combined flags for all permissions this principal holds, built by iterating
PermissionFlagMap in reverse.
```

---

### Refine `WorkflowAuthorizationMiddleware.cs`

```
Open file: src/Auth/Middleware/WorkflowAuthorizationMiddleware.cs

Add support for an optional X-WW-Bypass-Auth header for local development only.
When the header value is "local-dev-bypass" AND the environment is Development
(check IHostEnvironment or ASPNETCORE_ENVIRONMENT env var),
skip all policy checks and pass the request through with a debug log warning.
This must never work in Production — add an environment guard.
```

---

### Add xUnit Tests

```
Create a new file: tests/WorkflowClaimsPrincipalTests.cs

Generate xUnit test class for WorkflowClaimsPrincipal covering:

1. Anonymous() returns unauthenticated principal with empty Roles and Permissions
2. HasRole returns true for exact match, false for wrong case false (OrdinalIgnoreCase)
3. HasAnyRole returns true when at least one matches
4. HasAllRoles returns false when one is missing
5. HasPermissionFlag(WorkflowPermission.View) returns true when Permission.View is in claims
6. HasPermissionFlag(WorkflowPermission.View | WorkflowPermission.Execute) returns false
   when Execute is missing
7. IsUserToken is true when "scp" claim is present
8. IsAppOnlyToken is true when "scp" claim is absent
9. CallerIdentity returns UserName for user tokens and "app:{UserId}" for app-only tokens
10. Roles list excludes Permission.* values
11. Permissions list includes only Permission.* values

Use ClaimsIdentity with manually added claims to construct test principals.
```

---

### Add a New Workflow Policy

```
I want to add a new workflow called "invoice" to the system.

1. Add to secure.config:
   workflow: "invoice"
   requiredRoles: ["WarewolfAdministrators"]
   requiredPermissions: ["View", "Execute", "Contribute"]

2. Create src/Functions/InvoiceWorkflowFunction.cs following the exact same
   pattern as SecureWorkflowFunction.cs with route "secure/invoice.json".
   Add invoice-specific response fields: invoiceId (from query param "id"),
   and processingStatus = "queued".
```

---

### Add a Deployment Script

```
Create a file: Deploy-WwExecution.ps1

Generate a PowerShell 7 script with parameters:
- ResourceGroup   (mandatory)
- FunctionAppName (default "wwexecution")
- Configuration   (default "Release")

Steps:
1. Guard: verify dotnet CLI and az CLI are available
2. dotnet build --configuration $Configuration
3. dotnet publish --configuration $Configuration --output ./publish
4. Compress-Archive ./publish/* to ./wwexecution-deploy.zip (force overwrite)
5. az functionapp deployment source config-zip with the zip file
6. az functionapp show to confirm State = Running
7. Print coloured summary of each step pass/fail

Add full error handling. Each az/dotnet failure should print the error and exit.
```

---

## Section 3 — Local Test Script

Save this as `LocalTestHelper.ps1` in the project root and run it while
`func start` is running locally:

```powershell
#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Tests WwExecution Azure Function endpoints running locally at http://localhost:7071.
    Simulates Easy Auth by building fake X-MS-CLIENT-PRINCIPAL headers.
.EXAMPLE
    # In one terminal: func start
    # In another:      ./LocalTestHelper.ps1
#>

$BaseUrl = "http://localhost:7071"
$Passed  = 0
$Failed  = 0

# ── Helpers ───────────────────────────────────────────────────────────────────

function Build-FakePrincipal {
    param(
        [string]   $UserId      = "test-user-id-001",
        [string]   $Upn         = "alice@yourtenant.onmicrosoft.com",
        [string[]] $Roles       = @("WarewolfAdministrators"),
        [string[]] $Permissions = @("Permission.View","Permission.Execute",
                                    "Permission.Contribute","Permission.DeployTo",
                                    "Permission.DeployFrom","Permission.Administrator")
    )
    $claims = @(
        @{ typ = "http://schemas.microsoft.com/identity/claims/objectidentifier"; val = $UserId }
        @{ typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";   val = $Upn }
        @{ typ = "scp"; val = "user_impersonation" }
    )
    foreach ($r in $Roles)       { $claims += @{ typ = "roles"; val = $r } }
    foreach ($p in $Permissions) { $claims += @{ typ = "roles"; val = $p } }

    $payload = @{ auth_typ = "aad"; claims = $claims } | ConvertTo-Json -Depth 5 -Compress
    return [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($payload))
}

function Test-Endpoint {
    param(
        [string]    $Description,
        [string]    $Url,
        [string]    $Method         = "GET",
        [hashtable] $Headers        = @{},
        [int]       $ExpectedStatus = 200
    )

    try {
        $response = Invoke-RestMethod -Uri $Url -Method $Method -Headers $Headers `
                        -ResponseHeadersVariable rh -StatusCodeVariable sc `
                        -ErrorAction Stop -SkipHttpErrorCheck
        $actual = [int]$sc
    } catch {
        $actual = [int]$_.Exception.Response.StatusCode
    }

    $pass  = $actual -eq $ExpectedStatus
    $icon  = $pass ? "✓" : "✗"
    $color = $pass ? "Green" : "Red"

    if ($pass) { $script:Passed++ } else { $script:Failed++ }

    Write-Host ("  {0} {1,-55} expected={2} actual={3}" -f `
        $icon, $Description, $ExpectedStatus, $actual) -ForegroundColor $color
}

# ── Tests ─────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  WwExecution Local Endpoint Tests                   " -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

$adminHeader  = @{ "X-MS-CLIENT-PRINCIPAL" = Build-FakePrincipal }
$publicHeader = @{ "X-MS-CLIENT-PRINCIPAL" = Build-FakePrincipal -Roles @("PUBLIC") -Permissions @() }

Write-Host "  Public routes (no auth required)" -ForegroundColor White
Test-Endpoint "GET /public/order.json — no token"   "$BaseUrl/public/order.json?id=111"  ExpectedStatus 200
Test-Endpoint "GET /public/report.json — no token"  "$BaseUrl/public/report.json?id=222" ExpectedStatus 200
Test-Endpoint "GET /public/order.json — with token" "$BaseUrl/public/order.json?id=333"  Headers $adminHeader ExpectedStatus 200

Write-Host ""
Write-Host "  Secure routes — unauthenticated (expect 401)" -ForegroundColor White
Test-Endpoint "GET /secure/order.json — no token"   "$BaseUrl/secure/order.json?id=111" ExpectedStatus 401
Test-Endpoint "GET /secure/report.json — no token"  "$BaseUrl/secure/report.json"       ExpectedStatus 401

Write-Host ""
Write-Host "  Secure routes — authenticated admin (expect 200)" -ForegroundColor White
Test-Endpoint "GET /secure/order.json — admin"    "$BaseUrl/secure/order.json?id=111"  Headers $adminHeader  ExpectedStatus 200
Test-Endpoint "GET /secure/report.json — admin"   "$BaseUrl/secure/report.json"        Headers $adminHeader  ExpectedStatus 200
Test-Endpoint "GET /secure/deploy.json — admin"   "$BaseUrl/secure/deploy.json"        Headers $adminHeader  ExpectedStatus 200

Write-Host ""
Write-Host "  Secure routes — wrong workflow name (expect 403)" -ForegroundColor White
Test-Endpoint "GET /secure/unknown.json — admin"  "$BaseUrl/secure/unknown.json"       Headers $adminHeader  ExpectedStatus 403

Write-Host ""
Write-Host "  Secure routes — PUBLIC role, insufficient perms (expect 403)" -ForegroundColor White
Test-Endpoint "GET /secure/order.json — PUBLIC role" "$BaseUrl/secure/order.json"      Headers $publicHeader ExpectedStatus 403

Write-Host ""
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ("  Results: {0} passed  {1} failed" -f $Passed, $Failed) `
    -ForegroundColor ($Failed -eq 0 ? "Green" : "Red")
Write-Host "══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
```

---

## Section 4 — Quick Reference

### Run Scaffold

```powershell
cd C:\Projects
./Scaffold-WwExecution.ps1
```

### Build and Run Locally

```powershell
cd WwExecution

# Fill in secrets first
code local.settings.json

dotnet restore
dotnet build
func start
```

### Run Local Tests

```powershell
# In a second terminal while func start is running
./LocalTestHelper.ps1
```

### Deploy to Azure

```powershell
./Deploy-WwExecution.ps1 `
    -ResourceGroup   "DEV2" `
    -FunctionAppName "wwexecution" `
    -Configuration   "Release"
```

### Add a New Workflow (3 steps)

```powershell
# 1. Add to secure.config
# 2. Create a new function file in src/Functions/
# 3. Use Copilot Chat: "Add a new workflow called X following SecureWorkflowFunction pattern"
```
