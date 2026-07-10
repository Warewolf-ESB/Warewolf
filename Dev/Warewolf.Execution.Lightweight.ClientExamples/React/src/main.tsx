import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import {
  EventType,
  PublicClientApplication,
  type AuthenticationResult,
  type EventMessage,
} from '@azure/msal-browser';
import { MsalProvider } from '@azure/msal-react';

import { App } from './App';
import { msalConfig } from './authConfig';
import './styles.css';

/**
 * A single MSAL instance is created for the whole application and shared via
 * the React context provided by <MsalProvider>. It must be initialized once
 * (`initialize()`), and `handleRedirectPromise()` must run before render so
 * that a redirect coming back from Entra ID is processed.
 */
const msalInstance = new PublicClientApplication(msalConfig);

/**
 * Restore the active account on a hard reload. MSAL keeps the token cache in
 * localStorage, but the "active account" pointer is in-memory, so we set it
 * from the cache when there is no active account yet.
 */
function ensureActiveAccount(): void {
  if (!msalInstance.getActiveAccount()) {
    const accounts = msalInstance.getAllAccounts();
    if (accounts.length > 0) {
      msalInstance.setActiveAccount(accounts[0]);
    }
  }
}

/**
 * Keep the active account in sync with successful login / token events. After
 * an interactive login the new account becomes the active one.
 */
msalInstance.addEventCallback((event: EventMessage) => {
  if (
    (event.eventType === EventType.LOGIN_SUCCESS ||
      event.eventType === EventType.ACQUIRE_TOKEN_SUCCESS ||
      event.eventType === EventType.SSO_SILENT_SUCCESS) &&
    event.payload
  ) {
    const payload = event.payload as AuthenticationResult;
    if (payload.account) {
      msalInstance.setActiveAccount(payload.account);
    }
  }
});

async function bootstrap(): Promise<void> {
  await msalInstance.initialize();

  // Process the auth-code redirect (if we are returning from Entra ID). This
  // resolves to the AuthenticationResult on a redirect, or null otherwise.
  const result = await msalInstance.handleRedirectPromise();
  if (result?.account) {
    msalInstance.setActiveAccount(result.account);
  }

  ensureActiveAccount();

  const rootElement = document.getElementById('root');
  if (!rootElement) {
    throw new Error('Root element #root not found in index.html');
  }

  createRoot(rootElement).render(
    <StrictMode>
      <MsalProvider instance={msalInstance}>
        <App />
      </MsalProvider>
    </StrictMode>,
  );
}

void bootstrap().catch((error: unknown) => {
  // Surface configuration / startup failures rather than rendering a blank page.
  const message = error instanceof Error ? error.message : String(error);
  document.body.innerHTML =
    `<pre style="padding:2rem;color:#b00020;font:14px/1.5 monospace;white-space:pre-wrap">` +
    `Failed to start Warewolf React client:\n\n${message}</pre>`;
  console.error(error);
});
