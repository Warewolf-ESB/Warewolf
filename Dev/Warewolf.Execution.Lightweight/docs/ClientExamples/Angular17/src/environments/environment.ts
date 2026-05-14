/**
 * environment.ts — Development environment configuration.
 *
 * These values come from the outputs of:
 *   Configure-WwExecutionAuth.ps1          → TenantId, ResourceAppId
 *   Configure-WwExecutionAuth-Clients.ps1  → SpaClientId
 *
 * ⚠  Never commit real secrets here; rotate if accidentally exposed.
 */
export const environment = {
  production: false,

  // ── Entra ID / Microsoft Identity Platform ─────────────────────────────────
  entra: {
    /**
     * Azure AD Tenant ID.
     * All token requests are scoped to this tenant.
     */
    tenantId: 'ca0cc53b-9af4-4067-bcdf-be9c648450d1',

    /**
     * SPA App Registration Client ID (public client).
     * Registered as a Single-Page Application with PKCE support.
     * No client secret — PKCE replaces it for browser-based apps.
     */
    spaClientId: '1a328b9c-2f07-4ac4-b384-ad8ef23fdbb7',

    /**
     * Resource App ID — the Azure Function App's own app registration.
     * Used to construct the OAuth2 scope: api://<resourceAppId>/.default
     */
    resourceAppId: '05b557d6-e4b3-45bb-ad1c-23182e3060a2',
  },

  /**
   * Base URL of the wwexecutiondev Azure Function App.
   * Routes:
   *   /public/{workflow}   — anonymous, no auth required
   *   /secure/{workflow}   — requires delegated Bearer token (user signed in)
   *   /services/{workflow} — accepts delegated token or app-only token
   */
  functionAppUrl: 'https://wwexecutiondev.azurewebsites.net',

  // ── Derived helpers (computed from the values above) ──────────────────────

  /**
   * MSAL authority URL — points MSAL at the correct tenant's token endpoint.
   * Format: https://login.microsoftonline.com/{tenantId}
   */
  get authority(): string {
    return `https://login.microsoftonline.com/${this.entra.tenantId}`;
  },

  /**
   * OAuth2 scope requested for delegated (user) access.
   * Uses /.default to request all statically consented permissions.
   * The MsalInterceptor automatically attaches a Bearer token with this
   * scope to every HTTP request matching the protectedResourceMap URLs.
   */
  get userScope(): string {
    return `api://${this.entra.resourceAppId}/user_impersonation`;
  },

  /**
   * Redirect URI after login/logout.
   * Must be registered in the SPA's Redirect URIs in Azure Portal.
   */
  get redirectUri(): string {
    return 'http://localhost:4201';
  },
};
