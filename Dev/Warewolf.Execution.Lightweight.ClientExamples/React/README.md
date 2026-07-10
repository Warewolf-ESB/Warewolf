# Warewolf Execution Engine — React Reference Client

A complete, runnable **React 18 + TypeScript + Vite** Single-Page Application (SPA) that calls
the **Warewolf Execution Engine** using **Microsoft Entra ID** with the **Authorization Code +
PKCE** flow. Authentication and token management are handled by
[`@azure/msal-browser`](https://www.npmjs.com/package/@azure/msal-browser) +
[`@azure/msal-react`](https://www.npmjs.com/package/@azure/msal-react).

There is **no client secret in the browser** — this is a public client. MSAL performs PKCE,
maintains the token cache, and silently refreshes access tokens, falling back to an interactive
prompt only when Entra ID requires it.

---

## What this demonstrates

- A `MsalProvider` wrapping the app, with the active account set on load and the auth-code
  redirect processed via `handleRedirectPromise()` before render.
- A `useWorkflowApi` hook that **acquires an access token silently**
  (`acquireTokenSilent`) and **falls back to interactive auth**
  (`acquireTokenRedirect` / `acquireTokenPopup`) on `InteractionRequiredAuthError`, then attaches
  it as `Authorization: Bearer <token>` automatically.
- `AuthenticatedTemplate` / `UnauthenticatedTemplate` driving the login/logout UI.
- A demo UI to call `/public`, `/secure`, `/services`, and `/apis.json`, with a result panel and a
  token inspector.

---

## The engine API

Base URL is configurable via `VITE_FUNCTION_APP_URL` (default
`https://WWExecutionEngine.azurewebsites.net`).

| Route                          | Method     | Auth                                   |
| ------------------------------ | ---------- | -------------------------------------- |
| `/public/{workflow}.json`      | `GET`      | Anonymous (no token)                   |
| `/secure/{workflow}.json`      | `GET\|POST`| `Authorization: Bearer <access token>` |
| `/services/{workflow}.json`    | `GET\|POST`| `Authorization: Bearer <access token>` |
| `/apis.json`                   | `GET`      | Discovery                              |

Sample: `GET /secure/Hello%20World.json?Name=Alice` with a Bearer token.

> The engine rejects **roleless** users. The signed-in user must be assigned an **app role** on
> the engine's API app registration.

---

## Authentication flow

```
 Browser (SPA)                 Entra ID                    Function App (engine)
 ─────────────                 ────────                    ─────────────────────
      │  loginRedirect()           │                                │
      │ ─────────────────────────► │                                │
      │   (PKCE: code_challenge)   │                                │
      │                            │  user signs in / consents      │
      │ ◄───────────────────────── │   redirect with auth code      │
      │                            │                                │
      │  handleRedirectPromise()   │                                │
      │  exchanges code + verifier │                                │
      │ ─────────────────────────► │                                │
      │ ◄───────────────────────── │  id_token + access_token       │
      │                            │  (+ refresh token in cache)    │
      │                                                             │
      │  acquireTokenSilent({ scopes, account })                    │
      │   → cached access token, or refresh silently                │
      │                                                             │
      │  fetch /secure/...  Authorization: Bearer <access token>    │
      │ ──────────────────────────────────────────────────────────►│
      │ ◄────────────────────────────────────────────────────────── │
      │                       workflow result (JSON)                │
```

### Token lifecycle

- **ID token** — proves *who* the user is (identity, name, `oid`). Used by MSAL to populate the
  account; not sent to the engine.
- **Access token** — the bearer credential sent to the engine. Its `aud` is the resource app
  (`api://{resourceAppId}`), `scp` contains `user_impersonation`, and `roles` carries the user's
  assigned app roles. Short-lived (≈60–90 min).
- **Refresh token** — held in MSAL's cache; used transparently by `acquireTokenSilent` to mint a
  new access token without prompting the user.
- **Silent refresh** — `acquireTokenSilent` returns the cached token if still valid, otherwise
  uses the refresh token. No UI.
- **InteractionRequired fallback** — when silent acquisition throws
  `InteractionRequiredAuthError` (no cached token, expired/revoked refresh token, consent or MFA
  required, conditional-access challenge), the hook falls back to `acquireTokenRedirect` (default)
  or `acquireTokenPopup`. After a redirect, the app reloads and the retried silent call succeeds.

---

## Prerequisites

- **Node.js 18+** and npm.
- An **Entra ID tenant** where you can register applications (or values supplied by your admin).
- The Warewolf Execution Engine Function App reachable at your `VITE_FUNCTION_APP_URL`.

---

## App registration setup (Entra ID)

You need **two** app registrations (the API one may already exist for the engine):

1. **Engine API app** — exposes a delegated scope `user_impersonation` (Application ID URI
   `api://{resourceAppId}`) and defines **app roles** assigned to users. This is the `aud` of the
   access token. (Usually already configured for the engine.)
2. **This SPA app** — a **public client** configured as a **Single-page application (SPA)**:
   - Authentication → Add a platform → **Single-page application**.
   - Add redirect URI **`http://localhost:5173`** for local Vite dev (and your production origin).
   - API permissions → Add a permission → *My APIs* → the engine API →
     **Delegated** → `user_impersonation` → Grant admin consent (if required).
   - Assign the user an **app role** (Enterprise applications → the engine API → Users and groups).

---

## CORS and SPA redirect-URI setup

The engine Function App must allow the SPA's browser origin via **CORS**, and the SPA app
registration must list that origin as a **SPA redirect URI**.

Allow the Vite dev origin on the Function App (Azure CLI):

```bash
az functionapp cors add \
  --name WWExecutionEngine \
  --resource-group <your-resource-group> \
  --allowed-origins http://localhost:5173
```

List / verify allowed origins:

```bash
az functionapp cors show --name WWExecutionEngine --resource-group <your-resource-group>
```

Add the SPA redirect URI to the app registration (Azure CLI; or use the portal as above):

```bash
az ad app update --id <VITE_SPA_CLIENT_ID> \
  --set spa.redirectUris="['http://localhost:5173','https://your-prod-host']"
```

> For production, replace `http://localhost:5173` with your deployed origin in both places.

---

## Environment configuration

Copy the example file and fill in your values:

```bash
cp .env.example .env.local            # macOS / Linux
Copy-Item .env.example .env.local     # PowerShell
```

| Variable                | Description                                                              |
| ----------------------- | ------------------------------------------------------------------------ |
| `VITE_TENANT_ID`        | Directory (tenant) ID (GUID).                                            |
| `VITE_SPA_CLIENT_ID`    | Application (client) ID of **this SPA** registration.                    |
| `VITE_RESOURCE_APP_ID`  | Application (client) ID of the **engine API** registration.              |
| `VITE_FUNCTION_APP_URL` | Base URL of the engine Function App (no trailing slash).                 |

Only `VITE_`-prefixed variables are exposed to the browser. Restart `npm run dev` after edits.

---

## Run it

```bash
npm install
npm run dev
```

Open <http://localhost:5173>. Click **Sign in with Microsoft**, then use the **Call a workflow**
panel. Try `Hello World` with a `Name` parameter against `/secure`.

Other scripts:

```bash
npm run build       # type-check + production build into dist/
npm run preview     # serve the production build locally
npm run typecheck   # strict type-check only
```

---

## Project layout

```
.
├── index.html              # Vite entry HTML
├── package.json
├── tsconfig.json
├── vite.config.ts          # dev server pinned to :5173 (matches redirect URI)
├── .env.example            # copy to .env.local
└── src/
    ├── main.tsx            # MsalProvider + active-account init + handleRedirectPromise
    ├── authConfig.ts       # msalConfig, scopes, function app URL, derived API scope
    ├── App.tsx             # login/logout, Authenticated/Unauthenticated templates
    ├── useWorkflowApi.ts   # token acquisition (silent + fallback) + fetch helpers
    ├── WorkflowCaller.tsx  # demo UI: inputs, call buttons, result + token inspector
    ├── styles.css          # light styling
    └── vite-env.d.ts       # typed import.meta.env
```

---

## Troubleshooting

| Symptom                                                              | Likely cause                                                            | Fix                                                                                                  |
| ------------------------------------------------------------------- | ----------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `AADSTS9002326: Cross-origin token redemption … 'Single-Page Application'` | Redirect URI registered under *Web*, not *SPA*.                         | Register `http://localhost:5173` under the **Single-page application** platform.                     |
| CORS error in console; request blocked / no response                | Function App CORS does not allow the origin.                            | `az functionapp cors add … --allowed-origins http://localhost:5173`.                                 |
| `AADSTS65001: The user or administrator has not consented`          | Delegated `user_impersonation` permission not consented.               | Grant admin consent on the SPA app's API permissions.                                                |
| HTTP 401 from `/secure` or `/services`                              | No / invalid / expired bearer token, or wrong `aud`.                    | Ensure the requested scope is `api://{resourceAppId}/user_impersonation`; re-sign in.                |
| HTTP 403 / 500 denial despite valid token                           | User has **no app role**, or the role lacks the workflow permission.    | Assign the user an app role on the engine API (Enterprise applications → Users and groups).          |
| `InteractionRequiredAuthError` loops                                | Silent refresh cannot proceed (revoked session / CA challenge).         | Allow the interactive fallback to complete; clear site data and sign in again if it persists.        |
| Blank page, console shows "Missing environment variable …"          | `.env.local` not created or value blank.                                | Copy `.env.example` → `.env.local`, fill values, restart `npm run dev`.                              |
| Redirect returns to app but stays signed-out                        | `handleRedirectPromise()` not awaited before render.                    | This is handled in `main.tsx`; ensure you did not bypass `bootstrap()`.                              |
| HTTP 404 on a workflow                                              | Workflow name wrong or not deployed.                                    | Verify the name (spaces are URL-encoded automatically); check `/apis.json` discovery.                |
| Port 5173 already in use                                            | Another Vite/process holds the port.                                    | Stop the other process; the port is pinned (`strictPort`) to match the redirect URI.                 |

---

## Security notes

- This is a **public client** — there is **no secret** in the browser; PKCE protects the code
  exchange.
- Tokens are cached in `localStorage` so they survive reloads and are shared across tabs. If your
  threat model requires it, switch `cacheLocation` to `sessionStorage` in `authConfig.ts`.
- Never commit `.env.local`; it is git-ignored.
- `npm audit` reports a **moderate, dev-server-only** advisory in `esbuild` (a transitive
  dependency of `vite`, GHSA-67mh-4wv8-2f99). It affects only the local `vite` dev server, not
  the production build (`npm run build`), which is verified to compile. The only fix is the
  breaking `vite@8` upgrade — deliberately **not** applied here to keep this reference sample on
  the stable Vite 5 line. Run `npm audit fix --force` if you choose to move to Vite 8.
