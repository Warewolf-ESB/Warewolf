/**
 * auth.config.ts — MSAL factory functions for the Angular 17 standalone app.
 *
 * ── Authentication Flow Overview ──────────────────────────────────────────────
 *
 *  DELEGATED (Authorization Code + PKCE) flow — used by this SPA:
 *  ┌────────┐  1. ng serve / click "Login"   ┌──────────────────────┐
 *  │  SPA   │ ──────────────────────────────► │  Entra ID /authorize │
 *  │        │ ◄────── auth code + PKCE ─────  │  (login.microsoft..) │
 *  │        │  2. Exchange code → tokens      └──────────────────────┘
 *  │        │ ──── POST /token (PKCE) ──────► │  Entra ID /token     │
 *  │        │ ◄────── access_token ─────────  └──────────────────────┘
 *  │        │  3. Call Function App
 *  │        │ ──── GET /secure/... ─────────► │  Azure Function App  │
 *  │        │     Authorization: Bearer <tok> └──────────────────────┘
 *  └────────┘
 *
 *  Key PKCE properties:
 *  • No client secret stored in the browser (PKCE code_verifier replaces it).
 *  • Access tokens are short-lived (~1 h); MSAL refreshes silently via iframe.
 *  • Tokens are stored in localStorage (BrowserCacheLocation.LocalStorage).
 *
 *  CLIENT CREDENTIALS flow (app-only) — demonstrated in ClientCredentials page:
 *  • Runs on a confidential backend server, NEVER in the browser.
 *  • Browser calls your backend proxy → backend exchanges client_id + secret
 *    for an app-only token → backend calls the Function App.
 */

import { MsalGuardConfiguration, MsalInterceptorConfiguration } from '@azure/msal-angular';
import {
  BrowserCacheLocation,
  InteractionType,
  IPublicClientApplication,
  LogLevel,
  PublicClientApplication,
} from '@azure/msal-browser';
import { environment } from '../../environments/environment';

// ── 1. PublicClientApplication (MSAL core instance) ──────────────────────────
/**
 * Creates the MSAL PublicClientApplication that manages:
 *  - Token cache (localStorage)
 *  - Redirect / popup orchestration
 *  - Silent token refresh via hidden iframe
 */
export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication({
    auth: {
      clientId:              environment.entra.spaClientId,
      authority:             environment.authority,
      redirectUri:           environment.redirectUri,
      postLogoutRedirectUri: environment.redirectUri,
      // navigateToLoginRequestUrl: true — navigate back to the page that
      // triggered login after the redirect completes.
    },
    cache: {
      cacheLocation:          BrowserCacheLocation.LocalStorage,
      // storeAuthStateInCookie: true  ← enable only for IE11 / legacy Edge
      storeAuthStateInCookie: false,
    },
    system: {
      loggerOptions: {
        // In development, set LogLevel.Verbose for full MSAL trace output.
        logLevel:          LogLevel.Warning,
        loggerCallback:    (level, message) => console.warn(`[MSAL:${LogLevel[level]}] ${message}`),
        piiLoggingEnabled: false, // Never enable PII in production
      },
    },
  });
}

// ── 2. MsalGuard configuration ────────────────────────────────────────────────
/**
 * MsalGuard protects routes that require authentication.
 * When a user navigates to a guarded route without a valid account,
 * MSAL triggers an interactive login using the configured interactionType.
 *
 * InteractionType.Redirect is preferred over Popup because:
 *  - Popups are blocked by default in many browsers without a user gesture.
 *  - Redirect gives a cleaner UX for first-time sign-in.
 */
export function MSALGuardConfigFactory(): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    authRequest: {
      // Request the user_impersonation scope so the guard's login
      // already consents to the API access scope in one round trip.
      scopes: [environment.userScope],
    },
  };
}

// ── 3. MsalInterceptor configuration ─────────────────────────────────────────
/**
 * MsalInterceptor is an Angular HTTP interceptor that automatically:
 *  1. Inspects every outgoing HttpClient request URL.
 *  2. Checks if the URL matches an entry in protectedResourceMap.
 *  3. If matched, calls acquireTokenSilent for the mapped scopes.
 *  4. If silent acquisition fails (e.g. consent needed), triggers
 *     an interactive login (redirect or popup per interactionType).
 *  5. Attaches the resulting token as: Authorization: Bearer <access_token>
 *
 * /public/* URLs are intentionally absent — they are anonymous.
 */
export function MSALInterceptorConfigFactory(): MsalInterceptorConfiguration {
  const protectedResourceMap = new Map<string, Array<string>>([
    // Delegated (user) token required for /secure/* and /services/*
    [`${environment.functionAppUrl}/secure/`,   [environment.userScope]],
    [`${environment.functionAppUrl}/services/`, [environment.userScope]],
    // /api/public/* intentionally omitted — no Bearer token attached.
  ]);

  return {
    interactionType:     InteractionType.Redirect,
    protectedResourceMap,
  };
}
