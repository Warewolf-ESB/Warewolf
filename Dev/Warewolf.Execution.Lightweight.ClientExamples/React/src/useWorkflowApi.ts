import { useCallback } from 'react';
import {
  InteractionRequiredAuthError,
  type AccountInfo,
  type IPublicClientApplication,
} from '@azure/msal-browser';
import { useMsal } from '@azure/msal-react';

import { functionAppUrl, loginRequest, tokenRequestScopes } from './authConfig';

/** Shape of every workflow call result returned to the UI. */
export interface WorkflowResult {
  /** HTTP status code returned by the engine. */
  status: number;
  /** Whether `status` is in the 2xx range. */
  ok: boolean;
  /** Parsed JSON body when the response is JSON, else the raw text. */
  body: unknown;
  /** The access token used (null for anonymous /public calls) — for inspection. */
  accessToken: string | null;
}

/** How interactive fallback should be performed when silent acquisition fails. */
export type InteractionMode = 'redirect' | 'popup';

function buildWorkflowUrl(
  route: 'public' | 'secure' | 'services',
  workflow: string,
  query: Record<string, string>,
): string {
  // The workflow name may contain spaces (e.g. "Hello World") — encode it.
  const encoded = encodeURIComponent(workflow.trim());
  const url = new URL(`${functionAppUrl}/${route}/${encoded}.json`);
  for (const [key, value] of Object.entries(query)) {
    if (key.trim().length > 0) {
      url.searchParams.set(key, value);
    }
  }
  return url.toString();
}

async function readBody(response: Response): Promise<unknown> {
  const text = await response.text();
  if (text.length === 0) {
    return null;
  }
  const contentType = response.headers.get('content-type') ?? '';
  if (contentType.includes('application/json')) {
    try {
      return JSON.parse(text) as unknown;
    } catch {
      return text;
    }
  }
  // The engine may return text/plain or HTML for some errors — return as-is.
  return text;
}

/**
 * Acquire an access token for the engine API.
 *
 * Strategy (the centrepiece of robust token management):
 *   1. `acquireTokenSilent` — uses the cached access token, or transparently
 *      uses the refresh token to mint a new one. No user interaction.
 *   2. On `InteractionRequiredAuthError` (cache miss, expired refresh token,
 *      consent / MFA required, conditional-access challenge) fall back to an
 *      interactive flow — redirect (default) or popup.
 *
 * A redirect navigates away and the promise never resolves; the app re-renders
 * after returning from Entra ID and the retried silent call then succeeds.
 */
async function acquireToken(
  instance: IPublicClientApplication,
  account: AccountInfo,
  interaction: InteractionMode,
): Promise<string> {
  try {
    const result = await instance.acquireTokenSilent({
      scopes: tokenRequestScopes,
      account,
    });
    return result.accessToken;
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      if (interaction === 'popup') {
        const result = await instance.acquireTokenPopup({
          ...loginRequest,
          scopes: tokenRequestScopes,
          account,
        });
        return result.accessToken;
      }
      // Redirect: navigates away; control returns via handleRedirectPromise.
      await instance.acquireTokenRedirect({
        ...loginRequest,
        scopes: tokenRequestScopes,
        account,
      });
      // Unreachable in practice — the redirect leaves the page.
      throw new Error('Redirecting for interactive authentication…');
    }
    throw error;
  }
}

/**
 * React hook exposing typed helpers to call the Warewolf Execution Engine.
 * Tokens are acquired (silent + interactive fallback) and attached as a
 * Bearer header automatically for secure / services routes.
 */
export function useWorkflowApi(interaction: InteractionMode = 'redirect') {
  const { instance, accounts } = useMsal();

  const getActiveAccount = useCallback((): AccountInfo => {
    const active = instance.getActiveAccount() ?? accounts[0];
    if (!active) {
      throw new Error('No signed-in account. Please sign in first.');
    }
    return active;
  }, [instance, accounts]);

  /** GET /public/{workflow}.json — anonymous, no token attached. */
  const callPublic = useCallback(
    async (workflow: string, query: Record<string, string> = {}): Promise<WorkflowResult> => {
      const response = await fetch(buildWorkflowUrl('public', workflow, query), {
        method: 'GET',
        headers: { Accept: 'application/json' },
      });
      return {
        status: response.status,
        ok: response.ok,
        body: await readBody(response),
        accessToken: null,
      };
    },
    [],
  );

  /**
   * GET|POST /secure/{workflow}.json or /services/{workflow}.json with a Bearer
   * token. The token is acquired silently with interactive fallback, then
   * auto-attached. POST sends query inputs as a JSON body; GET as query string.
   */
  const callSecured = useCallback(
    async (
      route: 'secure' | 'services',
      workflow: string,
      query: Record<string, string> = {},
      method: 'GET' | 'POST' = 'GET',
    ): Promise<WorkflowResult> => {
      const account = getActiveAccount();
      const accessToken = await acquireToken(instance, account, interaction);

      const headers: Record<string, string> = {
        Accept: 'application/json',
        Authorization: `Bearer ${accessToken}`,
      };

      const init: RequestInit = { method, headers };
      let target: string;
      if (method === 'POST') {
        headers['Content-Type'] = 'application/json';
        init.body = JSON.stringify(query);
        target = buildWorkflowUrl(route, workflow, {});
      } else {
        target = buildWorkflowUrl(route, workflow, query);
      }

      const response = await fetch(target, init);
      return {
        status: response.status,
        ok: response.ok,
        body: await readBody(response),
        accessToken,
      };
    },
    [instance, interaction, getActiveAccount],
  );

  /** GET /apis.json — discovery document of executable workflows. */
  const callDiscovery = useCallback(async (): Promise<WorkflowResult> => {
    const account = getActiveAccount();
    const accessToken = await acquireToken(instance, account, interaction);
    const response = await fetch(`${functionAppUrl}/apis.json`, {
      method: 'GET',
      headers: {
        Accept: 'application/json',
        Authorization: `Bearer ${accessToken}`,
      },
    });
    return {
      status: response.status,
      ok: response.ok,
      body: await readBody(response),
      accessToken,
    };
  }, [instance, interaction, getActiveAccount]);

  /** Acquire (silent + fallback) and return the current access token for display. */
  const getAccessToken = useCallback(async (): Promise<string> => {
    const account = getActiveAccount();
    return acquireToken(instance, account, interaction);
  }, [instance, interaction, getActiveAccount]);

  return { callPublic, callSecured, callDiscovery, getAccessToken };
}
