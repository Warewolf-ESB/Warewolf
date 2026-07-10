import type { Configuration, PopupRequest, RedirectRequest } from '@azure/msal-browser';
import { LogLevel } from '@azure/msal-browser';

/**
 * Reads a required `VITE_`-prefixed environment variable, throwing a clear,
 * actionable error if it is missing. This fails fast at startup rather than
 * surfacing confusing auth errors later.
 */
function requireEnv(name: string): string {
  const value = import.meta.env[name as keyof ImportMetaEnv] as string | undefined;
  if (!value || value.trim().length === 0) {
    throw new Error(
      `Missing environment variable "${name}". ` +
        `Copy .env.example to .env.local, fill in the value, then restart "npm run dev".`,
    );
  }
  return value.trim();
}

const tenantId = requireEnv('VITE_TENANT_ID');
const spaClientId = requireEnv('VITE_SPA_CLIENT_ID');
const resourceAppId = requireEnv('VITE_RESOURCE_APP_ID');

/** Base URL of the Warewolf Execution Engine Function App (no trailing slash). */
export const functionAppUrl: string = requireEnv('VITE_FUNCTION_APP_URL').replace(/\/+$/, '');

/**
 * The delegated scope that the engine's protected API exposes. Requesting this
 * scope yields an access token whose `aud` is the resource app, which Easy Auth
 * validates. App-role assignment on the user controls authorization inside the
 * engine (roleless users are rejected).
 */
export const apiScope = `api://${resourceAppId}/user_impersonation`;

/**
 * MSAL core configuration.
 *
 * - `authority` is the tenant-specific Entra ID v2 endpoint.
 * - `redirectUri` must be registered as a **SPA** redirect URI on the app
 *   registration (Authentication blade -> Single-page application platform).
 * - Tokens are cached in `localStorage` so they survive full page reloads and
 *   are shared across tabs; the refresh token cookie kept by Entra ID enables
 *   silent renewal without re-prompting the user.
 */
export const msalConfig: Configuration = {
  auth: {
    clientId: spaClientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
    navigateToLoginRequestUrl: true,
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      logLevel: import.meta.env.DEV ? LogLevel.Verbose : LogLevel.Warning,
      piiLoggingEnabled: false,
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) {
          return;
        }
        switch (level) {
          case LogLevel.Error:
            console.error(message);
            break;
          case LogLevel.Warning:
            console.warn(message);
            break;
          case LogLevel.Info:
            console.info(message);
            break;
          default:
            if (import.meta.env.DEV) {
              console.debug(message);
            }
        }
      },
    },
  },
};

/**
 * Scopes requested at interactive sign-in. We include `openid` / `profile` for
 * the ID token (user identity) and the engine's delegated API scope so the
 * resulting access token is accepted by the Function App.
 */
export const loginRequest: PopupRequest & RedirectRequest = {
  scopes: ['openid', 'profile', apiScope],
};

/** Scopes requested when acquiring an access token for the engine API. */
export const tokenRequestScopes: string[] = [apiScope];
