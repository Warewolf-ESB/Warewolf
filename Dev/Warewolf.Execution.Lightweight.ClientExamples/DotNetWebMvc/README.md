# Warewolf Execution Engine — .NET 8 ASP.NET Core MVC client

A complete, runnable **server-rendered web app** that demonstrates calling the Warewolf
Execution Engine **on behalf of a signed-in user**. It is a **confidential client**: it signs
users in with **Microsoft Entra ID** using the **OpenID Connect Authorization Code flow**, caches
tokens server-side, and uses `Microsoft.Identity.Web` to silently acquire and inject a **delegated**
access token for the engine.

```
DotNetWebMvc/
├─ WwExecutionWebMvc.csproj          # net8.0, nullable, Microsoft.Identity.Web (+UI, +DownstreamApi)
├─ appsettings.json                  # AzureAd + WwExecution (downstream API) config
├─ Program.cs                        # OIDC sign-in + token acquisition + downstream API + MVC
├─ Properties/launchSettings.json    # https://localhost:5001
├─ Controllers/
│  ├─ HomeController.cs              # public landing ([AllowAnonymous])
│  └─ WorkflowController.cs          # [Authorize] — calls a secure workflow on the user's behalf
├─ Services/
│  ├─ IWwExecutionService.cs         # engine API abstraction
│  └─ WwExecutionService.cs          # public/secure/services calls; Bearer + x-functions-key
├─ Views/                            # _Layout, _LoginPartial, Home, Workflow, _ViewImports/_ViewStart
└─ wwwroot/css/site.css              # minimal styling
```

## Auth + call flow

```
  Browser                 This MVC app (confidential client)        Entra ID            Warewolf Engine
     │                              │                                  │                       │
     │  GET /Workflow (no session)  │                                  │                       │
     │─────────────────────────────▶                                  │                       │
     │                              │  302 → /authorize (OIDC)         │                       │
     │◀─────────────────────────────                                  │                       │
     │  sign in + consent ──────────────────────────────────────────▶ │                       │
     │  302 back with auth CODE → /signin-oidc                         │                       │
     │─────────────────────────────▶                                  │                       │
     │                              │  redeem CODE (client secret) ───▶│                       │
     │                              │◀── id_token + refresh + access ──│  (tokens cached       │
     │                              │                                  │   server-side)        │
     │  session cookie ◀────────────│                                  │                       │
     │                              │                                  │                       │
     │  GET /Workflow (authenticated)                                  │                       │
     │─────────────────────────────▶                                  │                       │
     │                              │  GetAccessTokenForUser           │                       │
     │                              │  (silent, scope =                │                       │
     │                              │   api://{resourceAppId}/         │                       │
     │                              │   user_impersonation) ──────────▶│                       │
     │                              │◀── delegated access token ───────│                       │
     │                              │  GET /secure/Hello World.json     │                       │
     │                              │  Authorization: Bearer <token> ──────────────────────────▶│
     │                              │◀──────────────── workflow JSON ───────────────────────────│
     │  rendered result ◀───────────│                                  │                       │
```

1. **OIDC sign-in (auth code flow).** Unauthenticated requests are redirected to Entra ID.
   The user signs in (and consents on first use). Entra ID returns an authorization **code** to
   `/signin-oidc`.
2. **Code redemption.** `Microsoft.Identity.Web` redeems the code using the app's **client secret**
   and stores the resulting tokens (id/access/refresh) in the **server-side token cache**.
3. **Token acquisition for downstream.** When the app calls the engine, `IDownstreamApi` /
   `ITokenAcquisition` silently obtains a **delegated** access token for
   `api://{resourceAppId}/user_impersonation` from the cache (refreshing via the refresh token when
   expired) and injects it as `Authorization: Bearer <token>`.
4. **Engine call.** The token represents the **signed-in user**, so the engine authorizes the call
   against that user's assigned app role.

## The engine API

| Route                              | Auth                                   | This client |
|------------------------------------|----------------------------------------|-------------|
| `GET /public/{workflow}.json`      | anonymous                              | `ExecutePublicAsync` (plain HttpClient) |
| `GET\|POST /secure/{workflow}.json`| Bearer (delegated user token)          | `ExecuteSecureAsync` (token auto-injected) |
| `GET\|POST /services/{workflow}.json`| Bearer **and** `x-functions-key`     | `ExecuteServiceAsync` (token + function key) |
| `GET /apis.json`                   | anonymous (discovery)                  | `GetApisAsync` |

Sample secure call: `GET /secure/Hello%20World.json?Name=Alice` with a Bearer token. Workflow names
are URI-encoded by `WwExecutionService.BuildRelativePath`, so spaces (`Hello World`) round-trip.

> **Roleless users are rejected.** The signed-in account must be assigned an **app role** on the
> engine's **resource service principal** in Entra ID. A user who authenticates but has no role gets
> a denial (the engine currently wraps authorization denials as **HTTP 500**, not 403 — pending
> WOLF-8418). `WorkflowController` surfaces this as a friendly "assign an app role" message.

## Token lifecycle

- **Authorization code** → redeemed once at sign-in for id + access + refresh tokens.
- **Token cache** → in-memory per user (`AddInMemoryTokenCaches`). For production / multi-instance,
  switch to `AddDistributedTokenCaches()` backed by Redis or SQL so tokens survive restarts and are
  shared across instances.
- **Silent refresh** → expired access tokens are renewed automatically from the cached refresh
  token; no user interaction.
- **Incremental consent / re-auth** → if a scope hasn't been consented or interaction is required,
  `IDownstreamApi` throws `MicrosoftIdentityWebChallengeUserException`. The
  `[AuthorizeForScopes(ScopeKeySection = "WwExecution:Scopes")]` filter on the action catches it and
  redirects the user back to Entra ID to consent, then resumes the original request.

## Entra ID setup

You need **two** app registrations: the **engine** (resource/API) and **this web app** (client).

**Engine (resource) registration** — usually already exists:
- Exposes an API with scope **`user_impersonation`** → Application ID URI `api://{resourceAppId}`.
- Defines **app roles** assigned to users (so they aren't roleless).

**This web app (client) registration:**
1. **Redirect URI** (Web platform): `https://localhost:5001/signin-oidc`.
2. **Front-channel logout / sign-out callback**: `https://localhost:5001/signout-callback-oidc`.
3. **Client secret** (Certificates & secrets) — copy the value into config (see below). Prefer a
   **certificate** in production.
4. **API permissions** → add the engine's delegated **`user_impersonation`** scope and grant admin
   consent (or rely on incremental consent at first call).
5. Assign your test user an **app role** on the engine's enterprise application.

## Configuration

Edit `appsettings.json` (placeholders shown). **Do not commit the client secret** — supply it via
user-secrets, environment variable, or Key Vault.

```jsonc
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "TenantId": "<your-tenant-id-guid>",
  "ClientId": "<this-web-apps-client-id-guid>",
  "ClientSecret": "<set-via-user-secrets>",      // do NOT commit
  "CallbackPath": "/signin-oidc",
  "SignedOutCallbackPath": "/signout-callback-oidc"
},
"WwExecution": {
  "BaseUrl": "https://WWExecutionEngine.azurewebsites.net",
  "Scopes": [ "api://<engine-resource-app-id-guid>/user_impersonation" ],
  "FunctionKey": ""                               // only for /services/* routes
}
```

Supply the secret out-of-band, e.g. with user-secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureAd:ClientSecret" "<the-secret-value>"
```

## Run

```bash
cd Dev/Warewolf.Execution.Lightweight.ClientExamples/DotNetWebMvc
dotnet restore
dotnet run
```

Browse to **https://localhost:5001**, sign in, then open **Run Workflow** to invoke
`GET /secure/Hello World.json?Name=Alice` on your behalf and view the JSON response.

(The first HTTPS run may prompt to trust the dev cert: `dotnet dev-certs https --trust`.)

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `AADSTS50011` redirect URI mismatch | Registered redirect URI ≠ `https://localhost:5001/signin-oidc` | Add the exact URI (scheme, host, port, `/signin-oidc`) to the app registration |
| `AADSTS7000215` invalid client secret | Wrong/expired `ClientSecret` | Regenerate the secret; set via user-secrets/env, not committed config |
| Redirected to sign in repeatedly / consent loop | Scope not consented or `[AuthorizeForScopes]` missing | Grant admin consent for `user_impersonation`, or let incremental consent run |
| `MicrosoftIdentityWebChallengeUserException` surfaced | Token cache miss / interaction required | Expected — `[AuthorizeForScopes]` re-challenges; ensure the attribute's `ScopeKeySection` matches config |
| Engine returns **HTTP 500** (wrapped denial) on `/secure` | Signed-in user is **roleless** on the engine resource SP | Assign the user an app role on the engine's enterprise application |
| **401 Unauthorized** from engine | Missing/invalid Bearer; wrong audience/scope | Verify `WwExecution:Scopes` is `api://{resourceAppId}/user_impersonation` and matches the engine's App ID URI |
| **404** on a workflow path | Workflow name wrong or not deployed | Check the exact name (URL-encoded) and `/apis.json` discovery output |
| `/services/*` call fails with 401 despite Bearer | Missing `x-functions-key` | Set `WwExecution:FunctionKey` to the engine host/function key |
| `api://...` token has no roles claim | App roles not assigned / not in token | Assign roles; ensure the resource app defines and emits app roles |

## Production notes

- Replace `AddInMemoryTokenCaches()` with `AddDistributedTokenCaches()` (Redis/SQL).
- Prefer a **certificate** credential over a client secret (`ClientCertificates` in `AzureAd`).
- Keep secrets in **Key Vault** / managed identity, never in `appsettings.json`.
- Behind a reverse proxy, configure `ForwardedHeaders` so redirect URIs use the public HTTPS host.
