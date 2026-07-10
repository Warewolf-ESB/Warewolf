# wwexecution Angular 17 Client

A reference **Single-Page Application (SPA)** demonstrating how to call a **Warewolf Execution Azure Function App** from Angular 17 using **Microsoft Entra ID** (Azure AD) authentication via the **Authorization Code + PKCE** flow.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Authentication Flow](#authentication-flow)
- [Project Structure](#project-structure)
- [Key Files](#key-files)
- [Routing](#routing)
- [API Endpoints](#api-endpoints)
- [Token Lifecycle](#token-lifecycle)
- [CORS Configuration](#cors-configuration)
- [Environment Configuration](#environment-configuration)
- [Running Locally](#running-locally)

---

## Overview

| Property | Value |
|---|---|
| Framework | Angular 17 (standalone components) |
| Auth library | `@azure/msal-angular` v3 + `@azure/msal-browser` v3 |
| Auth flow | Authorization Code + PKCE (no client secret in browser) |
| UI library | Angular Material |
| Backend | Azure Function App (`wwexecutiondev.azurewebsites.net`) |
| Identity provider | Microsoft Entra ID (Azure AD) |

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                  Angular 17 SPA (browser)                │
│                                                          │
│  ┌──────────┐   ┌────────────┐   ┌──────────────────┐  │
│  │ NavbarC. │   │ HomeComp.  │   │ WorkflowComp.    │  │
│  │ (toolbar)│   │ (landing)  │   │ (demo page)      │  │
│  └──────────┘   └────────────┘   └──────┬───────────┘  │
│                                          │               │
│  ┌──────────────────┐  ┌───────────────────────────┐   │
│  │   AuthService    │  │     WorkflowService        │   │
│  │ (MSAL façade)    │  │ (HttpClient façade)        │   │
│  └────────┬─────────┘  └──────────┬────────────────┘   │
│           │                        │                     │
│  ┌────────▼─────────┐  ┌──────────▼────────────────┐   │
│  │   MsalService    │  │     MsalInterceptor        │   │
│  │ (token cache,    │  │ (auto-attaches Bearer      │   │
│  │  login/logout)   │  │  token to HTTP requests)   │   │
│  └──────────────────┘  └───────────────────────────┘   │
└──────────────────────────────┬──────────────────────────┘
                                │  HTTPS
         ┌──────────────────────▼──────────────────────┐
         │         Azure Function App                    │
         │         wwexecutiondev.azurewebsites.net      │
         │                                               │
         │  GET  /public/{workflow}.json   (anonymous)   │
         │  POST /secure/{workflow}.json   (Bearer token) │
         │  POST /services/{workflow}.json (Bearer token) │
         └───────────────────────────────────────────────┘
```

---

## Authentication Flow

```
User clicks "Sign In"
        │
        ▼
MSAL redirects to login.microsoftonline.com/authorize
  ├── User enters credentials
  └── Entra ID issues authorization code
        │
        ▼
MSAL exchanges code + PKCE verifier → POST /token
  └── Receives access_token + refresh_token + id_token
        │
        ▼
Tokens cached in localStorage
        │
        ▼
User navigates to /workflow (guarded by MsalGuard)
        │
        ▼
User clicks "Call Secure" or "Call Services"
        │
        ▼
MsalInterceptor.intercept()
  ├── Calls acquireTokenSilent() — checks cache first
  ├── If token expired → silent refresh via refresh token
  └── Injects: Authorization: Bearer <access_token>
        │
        ▼
Azure Function App validates JWT → executes Warewolf workflow
        │
        ▼
Response returned to Angular (WorkflowResult)
```

### Sign-Out Flow

```
User clicks "Sign Out"
        │
        ▼
AuthService.logout() → msalSvc.logoutRedirect()
        │
        ▼
Entra ID /logout → clears session cookie
        │
        ▼
Redirects back to postLogoutRedirectUri (http://localhost:4201)
        │
        ▼
LOGOUT_SUCCESS broadcast event fires
        │
        ▼
AuthService clears account$ and accessToken$
Router navigates to /home
```

---

## Project Structure

```
src/
├── app/
│   ├── app.component.ts          # Shell — navbar + router-outlet + MSAL redirect handler
│   ├── app.config.ts             # App-level providers: MSAL, HttpClient, Router
│   ├── app.routes.ts             # Route definitions
│   │
│   ├── auth/
│   │   ├── auth.config.ts        # MSAL factory functions (PCA, Guard, Interceptor config)
│   │   └── auth.service.ts       # AuthService — login, logout, token, account$ observable
│   │
│   ├── home/
│   │   └── home.component.ts     # Landing page (public, no auth required)
│   │
│   ├── nav/
│   │   └── navbar.component.ts   # Sticky toolbar — sign-in/sign-out, user chip, nav links
│   │
│   ├── workflow/
│   │   ├── workflow.component.ts # Demo page — calls public/secure/services endpoints
│   │   ├── workflow.service.ts   # HTTP façade for the Function App API
│   │   └── workflow.model.ts     # TypeScript interfaces (WorkflowInputs, WorkflowResult)
│   │
│   ├── client-credentials/
│   │   └── client-credentials.component.ts  # Informational page (app-only flow)
│   │
│   └── shared/
│       ├── error-panel/
│       │   └── error-panel.component.ts      # Displays HTTP/MSAL errors
│       └── token-display/
│           └── token-display.component.ts    # JWT inspector (dev/demo only)
│
└── environments/
    ├── environment.ts            # Development config (Entra IDs, function URL)
    └── environment.prod.ts       # Production config
```

---

## Key Files

### `auth/auth.config.ts`
Factory functions consumed by `app.config.ts`:
- **`MSALInstanceFactory`** — creates `PublicClientApplication` with tenant/client IDs, `localStorage` token cache.
- **`MSALGuardConfigFactory`** — configures `MsalGuard` with redirect interaction type and required scopes.
- **`MSALInterceptorConfigFactory`** — maps URL patterns to OAuth2 scopes so the interceptor knows which requests need a Bearer token.

### `auth/auth.service.ts`
Application-level façade over `MsalService`:
- `account$` — `Observable<AccountInfo | null>` — emits the signed-in account or null.
- `isAuthenticated$` — `Observable<boolean>`.
- `accessToken$` — raw JWT for display/diagnostics.
- `login()` — triggers `loginRedirect`.
- `logout()` — triggers `logoutRedirect`, then navigates to `/home` on `LOGOUT_SUCCESS`.
- `acquireTokenSilently()` — manual silent token acquire for the token inspector.

### `workflow/workflow.service.ts`
HTTP façade for the Function App:

| Method | HTTP | URL | Auth |
|---|---|---|---|
| `callPublic(name, queryParams)` | GET | `/public/{name}.json?...` | None |
| `callSecure(name, queryParams, body)` | POST | `/secure/{name}.json?...` | Bearer (MsalInterceptor) |
| `callServices(name, queryParams, body)` | POST | `/services/{name}.json?...` | Bearer (MsalInterceptor) |

### `workflow/workflow.component.ts`
Interactive demo page with:
- **Workflow Name** — input field (URI-encoded automatically).
- **Query String** — key=value pairs (e.g. `Name=Alice&Age=30`) appended to URL for all 3 call types.
- **Request Body** — key=value pairs sent as JSON body for POST calls (Secure & Services); ignored for the Public GET.
- Buttons: **Call Public**, **Call Secure**, **Call Services**, **Show My Token**.
- Call history panel showing timestamped results and errors.

---

## Routing

| Path | Component | Guard | Description |
|---|---|---|---|
| `/home` | `HomeComponent` | None | Public landing page |
| `/workflow` | `WorkflowComponent` | `MsalGuard` | Auth-required demo page |
| `/client-credentials` | `ClientCredentialsComponent` | None | Informational (app-only flow) |
| `/` | — | — | Redirects to `/home` |
| `/**` | — | — | Redirects to `/home` |

`MsalGuard` on `/workflow`: if no cached account, MSAL triggers an interactive redirect login before rendering the page.

---

## API Endpoints

All routes are relative to `environment.functionAppUrl` (`https://wwexecutiondev.azurewebsites.net`).

### `GET /public/{workflow}.json`
- **Auth:** None — anonymous access.
- **Inputs:** Query string parameters (`?Name=Alice&Age=30`).
- **Use case:** Workflows that don't need identity information.

### `POST /secure/{workflow}.json`
- **Auth:** `Authorization: Bearer <user_access_token>` (attached by MsalInterceptor).
- **Inputs:** Query string + JSON body.
- **Use case:** Workflows that need the signed-in user's identity (name, UPN, OID from JWT claims).

### `POST /services/{workflow}.json`
- **Auth:** Same as `/secure/` — delegated Bearer token.
- **Inputs:** Query string + JSON body.
- **Use case:** Service-to-service integration patterns; on-behalf-of delegated token from a middle tier.

### Response schema (`WorkflowResult`)
```json
{
  "status":      "Success | Failure",
  "outputs":     { "OutputVar": "value" },
  "message":     "optional error description",
  "executionId": "correlation-id-for-tracing"
}
```

---

## Token Lifecycle

| Token | Lifetime (default) | Storage | Purpose |
|---|---|---|---|
| Access token | ~60–90 min | localStorage | Sent as `Authorization: Bearer` header |
| Refresh token | 24 h / 90 days (sliding) | localStorage | Used by MSAL to silently renew the access token |
| ID token | ~60–90 min | localStorage | User identity claims (not sent to the API) |

**Silent refresh:** `MsalInterceptor` calls `acquireTokenSilent()` before every matching request. If the access token is expired but the refresh token is valid, MSAL silently gets a new token — no user interaction.

**Refresh token expired:** MSAL throws `InteractionRequiredAuthError`. `MsalGuard` / the next navigation to a guarded route triggers an interactive redirect login.

**Shorten lifetime for testing (minimum 10 min enforced by Entra ID):**
```powershell
$policy = New-AzureADPolicy `
  -Type "TokenLifetimePolicy" `
  -DisplayName "ShortTokenTest" `
  -Definition @('{"TokenLifetimePolicy":{"Version":1,"AccessTokenLifetime":"0:10:00"}}') `
  -IsOrganizationDefault $false

Add-AzureADServicePrincipalPolicy -Id <SP-ObjectId> -RefObjectId $policy.Id
```

---

## CORS Configuration

The Azure Function App must allow the Angular dev origin.

### Azure Portal
**App Services → wwexecutiondev → API → CORS**
- Add `http://localhost:4201` to Allowed Origins.
- Check **Enable Access-Control-Allow-Credentials**.

### AZ CLI
```bash
# Add allowed origin
az functionapp cors add \
  --resource-group <rg> \
  --name wwexecutiondev \
  --allowed-origins "http://localhost:4201"

# Enable credentials support
az resource update \
  --resource-group <rg> \
  --name wwexecutiondev \
  --resource-type "Microsoft.Web/sites" \
  --set properties.siteConfig.cors.supportCredentials=true

# Verify
az functionapp cors show --resource-group <rg> --name wwexecutiondev
```

---

## Environment Configuration

Edit `src/environments/environment.ts`:

```typescript
export const environment = {
  entra: {
    tenantId:      '<your-tenant-id>',
    spaClientId:   '<spa-app-registration-client-id>',
    resourceAppId: '<function-app-app-registration-id>',
  },
  functionAppUrl: 'https://wwexecutiondev.azurewebsites.net',
  // Derived getters: authority, userScope, redirectUri
};
```

The `redirectUri` (`http://localhost:4201`) must be registered as a **Single-Page Application Redirect URI** in the SPA's Entra ID App Registration.

---

## Running Locally

```bash
# Install dependencies
npm install

# Start dev server on port 4201
ng serve --open
# → http://localhost:4201

# Run unit tests
ng test

# Production build
ng build --configuration production
```

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via a platform of your choice. To use this command, you need to first add a package that implements end-to-end testing capabilities.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.io/cli) page.
