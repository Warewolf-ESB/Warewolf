# Part 5 — Client Token Management Guide
## Warewolf Execution Lightweight — Acquiring, Using, and Refreshing Tokens

> **Audience** — Developers building client applications that call the
> `Warewolf.Execution.Lightweight` Function App.  Covers all three token
> grant types, MSAL configuration, token storage, refresh, and sign-out.

---

## 5.1 Overview

Every non-public endpoint on the Function App requires a valid token in
the `Authorization: Bearer <token>` header.  The token must:

1. Be signed by **your Entra tenant** (RS256, verified via OIDC metadata)
2. Have `aud` claim equal to `api://<resourceClientId>`
3. Have `iss` equal to `https://login.microsoftonline.com/<tenantId>/v2.0`
4. Carry the `roles` claims that match `WindowsGroup` entries in `secure.config`
5. Not be expired

How a client obtains such a token depends on its type:

| Client type | Grant | User context | Token roles |
|---|---|---|---|
| SPA / Browser app | Authorization Code + PKCE | ✅ User | User's assigned app roles |
| Server-side Web App | Authorization Code (confidential) | ✅ User | User's assigned app roles |
| .NET Daemon / Service | Client Credentials | ❌ App identity | SP's assigned app roles |
| Middle-tier API | On-Behalf-Of (OBO) | ✅ Original user | User's roles, proxied |
| Legacy Warewolf client | HMAC JWT via `/login` | ✅ Credentials | `UserGroups` from workflow |

---

## 5.2 MSAL Scopes and Resource Identifiers

The scope to request is always:

```
api://<resourceClientId>/.default
```

For delegated flows this requests all delegated permissions (`user_impersonation`)
plus any app roles assigned to the user.  For client credentials it requests all
application roles assigned to the service principal.

**Never request `openid profile email` separately for the resource call** — those
are for the user's identity token, not the resource token.

---

## 5.3 Type A — Angular / SPA (Authorization Code + PKCE)

### 5.3.1 App registration

The SPA needs its **own** client app registration (separate from the resource app):

```
Type:                Public client (SPA)
Redirect URIs:       http://localhost:4200  (dev)
                     https://my-app.azurewebsites.net/auth/callback  (prod)
API permissions:     Delegated — api://<resourceClientId>/user_impersonation
```

### 5.3.2 MSAL Angular configuration

```typescript
// app.module.ts
import { MsalModule, MsalInterceptor } from '@azure/msal-angular';
import { PublicClientApplication, InteractionType } from '@azure/msal-browser';

const msalConfig = {
  auth: {
    clientId: '<SPA_CLIENT_ID>',
    authority: 'https://login.microsoftonline.com/<TENANT_ID>',
    redirectUri: 'http://localhost:4200',
  },
  cache: {
    cacheLocation: 'localStorage',   // persist across tabs
    storeAuthStateInCookie: false,
  },
};

const resourceScope = 'api://<RESOURCE_CLIENT_ID>/.default';

@NgModule({
  imports: [
    MsalModule.forRoot(
      new PublicClientApplication(msalConfig),
      {
        interactionType: InteractionType.Redirect,
        authRequest: { scopes: [resourceScope] },
      },
      {
        interactionType: InteractionType.Redirect,
        protectedResourceMap: new Map([
          ['https://<FUNCTION_APP>.azurewebsites.net/Secure', [resourceScope]],
          ['https://<FUNCTION_APP>.azurewebsites.net/Services', [resourceScope]],
        ]),
      }
    ),
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: MsalInterceptor, multi: true },
  ],
})
export class AppModule {}
```

The `MsalInterceptor` automatically attaches the bearer token to any `HttpClient`
request matching `protectedResourceMap` — **no manual header injection needed**.

### 5.3.3 Manual token acquisition (non-interceptor pattern)

```typescript
import { MsalService } from '@azure/msal-angular';

@Injectable()
export class WorkflowService {
  constructor(private msal: MsalService, private http: HttpClient) {}

  async callWorkflow(name: string, params: Record<string, string>): Promise<any> {
    const token = await this.msal.acquireTokenSilent({
      scopes: ['api://<RESOURCE_CLIENT_ID>/.default'],
      account: this.msal.instance.getAllAccounts()[0],
    }).catch(() =>
      this.msal.acquireTokenPopup({
        scopes: ['api://<RESOURCE_CLIENT_ID>/.default'],
      })
    );

    return this.http.post(
      `https://<FUNCTION_APP>.azurewebsites.net/Secure/${name}.json`,
      params,
      { headers: { Authorization: `Bearer ${token.accessToken}` } }
    ).toPromise();
  }
}
```

### 5.3.4 Sign-in and sign-out

```typescript
// Sign in
this.msal.loginRedirect({ scopes: ['api://<RESOURCE_CLIENT_ID>/.default'] });

// Sign out (clears local cache + Entra session)
this.msal.logoutRedirect({ postLogoutRedirectUri: 'http://localhost:4200' });
```

### 5.3.5 Token refresh

MSAL handles refresh automatically:
- `acquireTokenSilent` returns a cached token if not within 5 minutes of expiry
- If the token is expired and refresh is possible, MSAL calls the token endpoint silently (hidden iframe / refresh token)
- If silent refresh fails, fall back to `acquireTokenPopup` or `loginRedirect`

**Token lifetime:** Entra default is 60–90 minutes.  Refresh tokens last 90 days
(sliding window) for single-tenant apps.  Configure in Entra via Token Lifetime Policy
if shorter lifetimes are required.

---

## 5.4 Type B — .NET Client Credentials (Daemon / Service)

### 5.4.1 App registration

```
Type:                Confidential client
Client secret:       Created in Stage 7 of provisioning (or managed identity)
API permissions:     Application — app roles on the resource app
                     (e.g. Permission.Execute, WarewolfAdministrators)
Admin consent:       Required for application permissions
```

### 5.4.2 MSAL .NET configuration

```csharp
// Program.cs / DI registration
using Microsoft.Identity.Client;

var confidentialApp = ConfidentialClientApplicationBuilder
    .Create("<DAEMON_CLIENT_ID>")
    .WithClientSecret("<CLIENT_SECRET>")   // or .WithCertificate(cert)
    .WithAuthority($"https://login.microsoftonline.com/<TENANT_ID>")
    .Build();

// Token cache (in-memory for singletons)
// Use Microsoft.Identity.Client.Extensions.Msal for distributed cache
```

### 5.4.3 Acquiring and using the token

```csharp
public class WorkflowApiClient
{
    private readonly IConfidentialClientApplication _app;
    private readonly HttpClient _http;
    private static readonly string[] Scopes =
        { "api://<RESOURCE_CLIENT_ID>/.default" };

    public async Task<string> ExecuteWorkflowAsync(string workflowName, object body)
    {
        // MSAL caches the token and refreshes automatically
        var tokenResult = await _app
            .AcquireTokenForClient(Scopes)
            .ExecuteAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://<FUNCTION_APP>.azurewebsites.net/Secure/{workflowName}.json");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);

        request.Content = JsonContent.Create(body);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

### 5.4.4 Token refresh (client credentials)

Client credential tokens have no refresh token — a new token is requested when
the current one expires.  MSAL's `AcquireTokenForClient` caches the token and
automatically acquires a new one when it expires.  **No application-level
refresh logic is needed.**

### 5.4.5 Using Managed Identity instead of a client secret

For services deployed to Azure (App Service, AKS, Container Apps):

```csharp
using Azure.Identity;

// System-assigned managed identity
var credential = new DefaultAzureCredential();

// Request a token for the resource app
var tokenRequestContext = new TokenRequestContext(
    new[] { "api://<RESOURCE_CLIENT_ID>/.default" });

var token = await credential.GetTokenAsync(tokenRequestContext);
// Use token.Token as the bearer value
```

> No client secret is needed; the managed identity's service principal must be
> assigned the appropriate app roles (Stage 6 of provisioning).

---

## 5.5 Type C — .NET On-Behalf-Of (Middle-Tier API)

Used when a .NET Web API receives a user token and needs to call the Function App
using the **original user's identity and roles**.

### 5.5.1 OBO flow

```
User → [Bearer token for middle-tier API]
Middle-tier API → POST /oauth2/v2.0/token
    grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer
    client_id=<middle-tier-client-id>
    client_secret=<middle-tier-secret>
    assertion=<incoming-user-token>
    requested_token_use=on_behalf_of
    scope=api://<RESOURCE_CLIENT_ID>/.default
← [New token with user identity, valid for the resource app]
```

### 5.5.2 MSAL OBO implementation

```csharp
public class MiddleTierWorkflowProxy
{
    private readonly IConfidentialClientApplication _app;
    private readonly HttpClient _http;

    public async Task<string> ForwardToWorkflowAsync(
        string incomingBearerToken,
        string workflowName,
        object body)
    {
        // Exchange the user token for a resource token
        var userAssertion = new UserAssertion(incomingBearerToken);

        var oboToken = await _app
            .AcquireTokenOnBehalfOf(
                new[] { "api://<RESOURCE_CLIENT_ID>/.default" },
                userAssertion)
            .ExecuteAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://<FUNCTION_APP>.azurewebsites.net/Secure/{workflowName}.json");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", oboToken.AccessToken);
        request.Content = JsonContent.Create(body);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

---

## 5.6 Type D — Legacy Warewolf HMAC JWT via `/login`

For systems that cannot use Entra directly but can POST credentials to the
Warewolf login endpoint.

### 5.6.1 Acquiring a Warewolf JWT

```http
POST https://<FUNCTION_APP>.azurewebsites.net/login
Content-Type: application/json

{
  "Username": "alice",
  "Password": "secret123"
}
```

**Response (200 OK):**
```json
{ "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }
```

The token is HMAC-SHA256 signed using the `SecretKey` from `secure.config`.
It contains:

```json
{
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/authentication":
      "{\"UserGroups\": [\"WarewolfAdministrators\", \"EXECUTE\"]}",
  "nbf": 1700000000,
  "exp": 1700086400,
  "iat": 1700000000
}
```

### 5.6.2 Using the Warewolf JWT

```http
GET https://<FUNCTION_APP>.azurewebsites.net/Secure/order.json?id=42
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 5.6.3 Token lifetime and refresh

The token expiry is set by the `LoginFunction` at `+24 hours`.  There is no
refresh mechanism — re-POST to `/login` for a new token.

---

## 5.7 Token Storage Best Practices

| Client type | Recommended storage | Anti-pattern |
|---|---|---|
| SPA (Angular) | `localStorage` (MSAL default) — XSS risk accepted per MSAL guidance | `document.cookie` with no `HttpOnly` |
| Server Web App | Server-side session (MSAL token cache + database) | Storing raw tokens in browser `localStorage` |
| .NET Service | In-memory MSAL cache (automatic with `IConfidentialClientApplication`) | Writing tokens to disk or config files |
| Mobile / Desktop | MSAL platform secure cache (DPAPI / Keychain) | `appsettings.json` |

---

## 5.8 Token Validation Decisions (Server Side)

The Function App's `BearerTokenPrincipalParser` validates:

| Claim | Validation |
|---|---|
| `iss` | Must match `https://login.microsoftonline.com/<tenantId>/v2.0` or `https://sts.windows.net/<tenantId>/` |
| `aud` | Must match `WAREWOLF_ENTRA_AUDIENCE` (e.g. `api://<clientId>`) or `WAREWOLF_ENTRA_CLIENT_ID` |
| `exp` | Must be in the future |
| Signature | RS256 — verified against OIDC signing keys fetched from `<metadataAddress>` |
| `roles` | Extracted and mapped to `WorkflowClaimsPrincipal.Groups` and `Permissions` |

**Easy Auth path (production):** The token is validated by the platform before the
Function App worker process is invoked.  `EasyAuthPrincipalParser` reads the
pre-validated `X-MS-CLIENT-PRINCIPAL` header — no cryptographic validation in code.

**Direct bearer path (local dev / bypass):** `BearerTokenPrincipalParser` performs
full RS256 signature verification using OIDC metadata.

---

## 5.9 Debugging Token Issues

### Decode a JWT locally

```powershell
$token = "eyJ..."
$payload = $token.Split('.')[1]
# Pad base64
$padded = $payload.PadRight($payload.Length + (4 - $payload.Length % 4) % 4, '=')
[System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($padded)) | ConvertFrom-Json
```

### Check what roles the token carries

```json
{
  "aud": "api://11111111-...",
  "iss": "https://login.microsoftonline.com/<tid>/v2.0",
  "oid": "aaaaaaaa-...",
  "preferred_username": "alice@contoso.com",
  "roles": ["WarewolfAdministrators", "Permission.Execute", "Permission.View"],
  "scp": "user_impersonation",
  "exp": 1700086400
}
```

### Common token errors

| HTTP status | Symptom | Fix |
|---|---|---|
| `401` | No `Authorization` header | Add `Authorization: Bearer <token>` |
| `401` | `aud` mismatch | Request token with `api://<resourceClientId>/.default` scope |
| `401` | Token expired | Re-acquire; check clock skew |
| `401` | Wrong tenant `iss` | Ensure MSAL authority matches `WAREWOLF_ENTRA_TENANT_ID` |
| `403` | Token valid but `roles` empty | Assign user/SP to app roles in Entra (Stage 6) |
| `403` | Roles present but no `Execute` in `secure.config` | Add entry with `Execute = true` for the workflow |
| `403` | UPN entry doesn't match | UPN in `secure.config` must exactly match `preferred_username` claim |

---

## 5.7 Generic Client App Patterns (PRV-12 / PRV-13 / PRV-14)

These complement the registration recipes in Part 4 — they show the
runtime token-acquisition code each client type uses against wwexecution.

### PRV-12 — SPA / browser app (MSAL.js, Authorization Code + PKCE)

```javascript
// msalConfig.js
export const msalConfig = {
  auth: {
    clientId:    "<spa-clientId>",
    authority:   "https://login.microsoftonline.com/<tenantId>",
    redirectUri: "https://my-spa.contoso.com/auth/callback",
  },
  cache: { cacheLocation: "sessionStorage" }
};

export const wwexScopes = ["api://<resourceClientId>/user_impersonation"];

// callWwex.js
import { PublicClientApplication } from "@azure/msal-browser";
import { msalConfig, wwexScopes } from "./msalConfig";

const pca = new PublicClientApplication(msalConfig);
await pca.initialize();

async function callWwex(path) {
  const account = pca.getAllAccounts()[0]
                  ?? (await pca.loginPopup({ scopes: wwexScopes })).account;
  const result  = await pca.acquireTokenSilent({ account, scopes: wwexScopes });
  const r = await fetch(`https://wwexec.contoso.com${path}`, {
    headers: { Authorization: `Bearer ${result.accessToken}` }
  });
  if (r.status === 401) {
    // forced re-login (e.g., MFA stepup)
    const fresh = await pca.acquireTokenPopup({ scopes: wwexScopes });
    return fetch(`https://wwexec.contoso.com${path}`, {
      headers: { Authorization: `Bearer ${fresh.accessToken}` }
    });
  }
  return r;
}
```

Key points:
- Use `acquireTokenSilent` first; fall back to `acquireTokenPopup` /
  `acquireTokenRedirect` on `InteractionRequiredAuthError`.
- Never store tokens in `localStorage` for high-value APIs — MSAL’s
  `sessionStorage` cache is safer; the access token lives only in memory.

### PRV-13 — Confidential web app / API (MSAL.NET, Authorization Code + OBO)

```csharp
// Program.cs — minimal ASP.NET Core wiring
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(new[] { WwexScope })
    .AddInMemoryTokenCaches();           // production: AddDistributedTokenCaches()

const string WwexScope = "api://<resourceClientId>/user_impersonation";

// MyController.cs
[Authorize]
public class OrdersController(ITokenAcquisition tokens, IHttpClientFactory http) : ControllerBase
{
    [HttpGet("/orders/{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var token = await tokens.GetAccessTokenForUserAsync(new[] { WwexScope });
        var client = http.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        var resp = await client.GetAsync($"https://wwexec.contoso.com/secure/Orders/{id}");
        return new ContentResult { Content = await resp.Content.ReadAsStringAsync() };
    }
}
```

Notes:
- `GetAccessTokenForUserAsync` performs On-Behalf-Of when the inbound token
  is for the web app's own scope — that's the recommended OBO pattern.
- For pure server-to-server calls (no user) inject `ITokenAcquisition` and
  call `GetAccessTokenForAppAsync(WwexScope)` instead.
- Cache token-acquisition state outside the request scope; MSAL’s
  `AddDistributedTokenCaches` (Redis or SQL) is the production default.

### PRV-14 — Daemon / background service (Managed Identity preferred)

```csharp
// Worker.cs
using Azure.Core;
using Azure.Identity;

public sealed class WwexClient : IAsyncDisposable
{
    private readonly HttpClient _http;
    private readonly TokenCredential _cred = new DefaultAzureCredential();
    private readonly TokenRequestContext _ctx =
        new(new[] { "api://<resourceClientId>/.default" });

    public WwexClient(HttpClient http) => _http = http;

    public async Task<string> ExecuteAsync(string workflow, CancellationToken ct)
    {
        AccessToken token = await _cred.GetTokenAsync(_ctx, ct);
        using var req = new HttpRequestMessage(HttpMethod.Post,
            $"https://wwexec.contoso.com/secure/{workflow}");
        req.Headers.Authorization = new("Bearer", token.Token);
        using var resp = await _http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync(ct);
    }

    public ValueTask DisposeAsync() { _http.Dispose(); return ValueTask.CompletedTask; }
}
```

- `DefaultAzureCredential` chains MI → Visual Studio → CLI → Environment
  credentials, so the same code works in Azure (MI), local dev (CLI), and
  CI (federated workload identity).
- Tokens are cached internally by `Azure.Identity`; do not add another cache.
- App-only tokens carry only `roles`, never `scp`. wwexecution treats them
  as `IsAppOnlyToken == true`; UPN-based `secure.config` entries do not
  match — use a group entry that reflects the daemon's app-role assignment.

### Cross-cutting refresh / failure handling

| Symptom                                | Recommended action                              |
|----------------------------------------|--------------------------------------------------|
| 401 with `WWW-Authenticate: Bearer`    | Acquire a fresh token, retry once.              |
| 401 with `error="invalid_token"`       | Drop the cached token; fall back to interactive (SPA) or `GetTokenAsync` (daemon). |
| 403 from wwexecution                   | NOT a token problem — fix `secure.config` or app-role assignment. |
| MSAL `InteractionRequiredAuthError`    | Trigger interactive flow (popup / redirect).    |
| Rapid 429 from Entra                   | Increase MSAL token cache TTL; add jitter.      |
