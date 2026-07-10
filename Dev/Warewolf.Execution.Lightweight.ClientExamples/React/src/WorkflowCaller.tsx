import { useCallback, useMemo, useState } from 'react';

import { useWorkflowApi, type WorkflowResult } from './useWorkflowApi';

interface QueryParam {
  key: string;
  value: string;
}

interface WorkflowCallerProps {
  /** When true, only the anonymous /public call is offered (no token needed). */
  publicOnly?: boolean;
}

/** Decode a JWT payload for display in the token inspector (no verification). */
function decodeJwtPayload(token: string): Record<string, unknown> | null {
  const parts = token.split('.');
  if (parts.length < 2) {
    return null;
  }
  try {
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => `%${c.charCodeAt(0).toString(16).padStart(2, '0')}`)
        .join(''),
    );
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

export function WorkflowCaller({ publicOnly = false }: WorkflowCallerProps) {
  const { callPublic, callSecured, callDiscovery, getAccessToken } = useWorkflowApi('redirect');

  const [workflow, setWorkflow] = useState('Hello World');
  const [params, setParams] = useState<QueryParam[]>([{ key: 'Name', value: 'Alice' }]);
  const [busy, setBusy] = useState(false);
  const [result, setResult] = useState<WorkflowResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [token, setToken] = useState<string | null>(null);

  const queryRecord = useMemo<Record<string, string>>(() => {
    const record: Record<string, string> = {};
    for (const { key, value } of params) {
      if (key.trim().length > 0) {
        record[key.trim()] = value;
      }
    }
    return record;
  }, [params]);

  const tokenClaims = useMemo(() => (token ? decodeJwtPayload(token) : null), [token]);

  const run = useCallback(
    async (action: () => Promise<WorkflowResult>) => {
      setBusy(true);
      setError(null);
      setResult(null);
      try {
        const response = await action();
        setResult(response);
        if (response.accessToken) {
          setToken(response.accessToken);
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : String(err));
      } finally {
        setBusy(false);
      }
    },
    [],
  );

  const updateParam = (index: number, patch: Partial<QueryParam>): void => {
    setParams((prev) => prev.map((p, i) => (i === index ? { ...p, ...patch } : p)));
  };

  const addParam = (): void => setParams((prev) => [...prev, { key: '', value: '' }]);

  const removeParam = (index: number): void =>
    setParams((prev) => prev.filter((_, i) => i !== index));

  const inspectToken = useCallback(async () => {
    setError(null);
    try {
      setToken(await getAccessToken());
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err));
    }
  }, [getAccessToken]);

  return (
    <div className="caller-grid">
      <section className="card">
        <h2>Call a workflow</h2>

        <label className="field">
          <span>Workflow name</span>
          <input
            type="text"
            value={workflow}
            onChange={(e) => setWorkflow(e.target.value)}
            placeholder="Hello World"
          />
          <small>
            Resolves to <code>/&#123;route&#125;/{encodeURIComponent(workflow.trim())}.json</code>
          </small>
        </label>

        <fieldset className="params">
          <legend>Query / input parameters</legend>
          {params.map((param, index) => (
            <div className="param-row" key={index}>
              <input
                type="text"
                value={param.key}
                onChange={(e) => updateParam(index, { key: e.target.value })}
                placeholder="name"
                aria-label="parameter name"
              />
              <input
                type="text"
                value={param.value}
                onChange={(e) => updateParam(index, { value: e.target.value })}
                placeholder="value"
                aria-label="parameter value"
              />
              <button
                className="btn btn-icon"
                onClick={() => removeParam(index)}
                aria-label="remove parameter"
                type="button"
              >
                ✕
              </button>
            </div>
          ))}
          <button className="btn btn-secondary btn-small" onClick={addParam} type="button">
            + Add parameter
          </button>
        </fieldset>

        <div className="actions">
          <button
            className="btn btn-primary"
            disabled={busy}
            onClick={() => run(() => callPublic(workflow, queryRecord))}
          >
            GET /public
          </button>

          {!publicOnly && (
            <>
              <button
                className="btn btn-primary"
                disabled={busy}
                onClick={() => run(() => callSecured('secure', workflow, queryRecord, 'GET'))}
              >
                GET /secure
              </button>
              <button
                className="btn btn-primary"
                disabled={busy}
                onClick={() => run(() => callSecured('secure', workflow, queryRecord, 'POST'))}
              >
                POST /secure
              </button>
              <button
                className="btn btn-primary"
                disabled={busy}
                onClick={() => run(() => callSecured('services', workflow, queryRecord, 'GET'))}
              >
                GET /services
              </button>
              <button
                className="btn btn-secondary"
                disabled={busy}
                onClick={() => run(() => callDiscovery())}
              >
                GET /apis.json
              </button>
            </>
          )}
        </div>
      </section>

      <section className="card">
        <h2>Result</h2>
        {busy && <p className="status status-busy">Calling engine…</p>}

        {error && (
          <div className="status status-error">
            <strong>Error:</strong> {error}
          </div>
        )}

        {result && (
          <div className="result">
            <p className={result.ok ? 'status status-ok' : 'status status-error'}>
              HTTP {result.status} {result.ok ? '(success)' : '(failure)'}
            </p>
            <pre className="code-block">
              {typeof result.body === 'string'
                ? result.body
                : JSON.stringify(result.body, null, 2)}
            </pre>
          </div>
        )}

        {!busy && !error && !result && <p className="hint">No call made yet.</p>}
      </section>

      {!publicOnly && (
        <section className="card token-inspector">
          <h2>Access token inspector</h2>
          <p className="hint">
            Inspect the delegated access token MSAL acquires for the engine. Useful for verifying
            the <code>aud</code> (resource app), <code>scp</code> (scope), and <code>roles</code>{' '}
            claims.
          </p>
          <button className="btn btn-secondary btn-small" onClick={() => void inspectToken()}>
            Acquire &amp; inspect token
          </button>

          {token && (
            <>
              <h3>Raw token</h3>
              <pre className="code-block code-block-wrap">{token}</pre>
              <h3>Decoded claims</h3>
              <pre className="code-block">
                {tokenClaims ? JSON.stringify(tokenClaims, null, 2) : 'Unable to decode token.'}
              </pre>
            </>
          )}
        </section>
      )}
    </div>
  );
}
