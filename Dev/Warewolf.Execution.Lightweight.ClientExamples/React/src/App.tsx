import { useMsal, AuthenticatedTemplate, UnauthenticatedTemplate } from '@azure/msal-react';

import { loginRequest } from './authConfig';
import { WorkflowCaller } from './WorkflowCaller';

/**
 * Top-level shell: header with sign-in / sign-out, and conditional rendering
 * via MSAL's <AuthenticatedTemplate> / <UnauthenticatedTemplate>. These render
 * their children based on whether any account is signed in.
 */
export function App() {
  const { instance, accounts } = useMsal();
  const activeAccount = instance.getActiveAccount() ?? accounts[0] ?? null;

  const handleLogin = (): void => {
    // Redirect flow is the most robust for SPAs (no popup blockers, works on
    // mobile). MSAL stores PKCE verifier + state and resumes after return.
    void instance.loginRedirect(loginRequest);
  };

  const handleLogout = (): void => {
    void instance.logoutRedirect({ account: activeAccount ?? undefined });
  };

  return (
    <div className="app">
      <header className="app-header">
        <div className="brand">
          <span className="brand-mark">⚙</span>
          <div>
            <h1>Warewolf Execution Engine</h1>
            <p className="subtitle">React 18 + MSAL (Auth Code + PKCE) reference client</p>
          </div>
        </div>

        <div className="auth-controls">
          <AuthenticatedTemplate>
            <span className="account-name" title={activeAccount?.username}>
              {activeAccount?.name ?? activeAccount?.username ?? 'Signed in'}
            </span>
            <button className="btn btn-secondary" onClick={handleLogout}>
              Sign out
            </button>
          </AuthenticatedTemplate>
          <UnauthenticatedTemplate>
            <button className="btn btn-primary" onClick={handleLogin}>
              Sign in with Microsoft
            </button>
          </UnauthenticatedTemplate>
        </div>
      </header>

      <main className="app-main">
        <AuthenticatedTemplate>
          <WorkflowCaller />
        </AuthenticatedTemplate>

        <UnauthenticatedTemplate>
          <section className="card welcome">
            <h2>Welcome</h2>
            <p>
              Sign in with your Microsoft Entra ID account to call <code>/secure</code> and{' '}
              <code>/services</code> workflows. Anonymous <code>/public</code> workflows can be
              called without signing in once you are on the page.
            </p>
            <p className="hint">
              You must be assigned an <strong>app role</strong> on the engine&rsquo;s API
              registration — roleless users are rejected by the engine.
            </p>
          </section>
          {/* Public calls do not need auth, so we still expose the caller. */}
          <WorkflowCaller publicOnly />
        </UnauthenticatedTemplate>
      </main>

      <footer className="app-footer">
        <span>
          Tokens are managed by MSAL: silent acquisition with interactive fallback on{' '}
          <code>InteractionRequiredAuthError</code>.
        </span>
      </footer>
    </div>
  );
}
