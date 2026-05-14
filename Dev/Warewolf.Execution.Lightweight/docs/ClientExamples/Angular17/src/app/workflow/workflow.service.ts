/**
 * workflow.service.ts — HTTP client facade for the Warewolf wwexecution
 * Azure Function App.
 *
 * ── Route Structure ────────────────────────────────────────────────────────────
 *
 *  /public/{workflow}.json   GET/POST  Anonymous — no auth required.
 *  /secure/{workflow}.json   GET/POST  Delegated — requires user Bearer token.
 *  /services/{workflow}.json GET/POST  Delegated — user token via MsalInterceptor.
 *
 * ── Token Attachment ──────────────────────────────────────────────────────────
 *
 *  The MsalInterceptor (configured in app.config.ts) automatically intercepts
 *  every HttpClient request and checks if its URL matches an entry in the
 *  protectedResourceMap defined in auth.config.ts.
 *
 *  Matching URLs → MsalInterceptor calls acquireTokenSilent and injects:
 *    Authorization: Bearer <access_token>
 *
 *  Non-matching URLs (e.g. /public/*) → no header added.
 *
 *  This service therefore makes plain HttpClient calls — token management
 *  is fully handled at the interceptor layer, keeping this service clean.
 */
import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { WorkflowInputs, WorkflowResult } from './workflow.model';

@Injectable({ providedIn: 'root' })
export class WorkflowService {
  private readonly base = environment.functionAppUrl;

  constructor(private readonly http: HttpClient) {}

  // ── Public endpoint ────────────────────────────────────────────────────────

  /**
   * Calls /public/{workflow}.json with an optional GET query string.
   *
   * No authentication required — the Function App validates the absence of
   * auth header and serves the workflow to any caller.
   *
   * @param workflowName  Warewolf workflow name (URI-encoded automatically).
   * @param inputs        Optional input key/value pairs appended as query params.
   */
  callPublic(workflowName: string, inputs: WorkflowInputs = {}): Observable<WorkflowResult> {
    const encoded = encodeURIComponent(workflowName);
    let params = new HttpParams();
    for (const [k, v] of Object.entries(inputs)) {
      params = params.set(k, String(v));
    }
    return this.http.get<WorkflowResult>(
      `${this.base}/public/${encoded}.json`,
      { params },
    );
  }

  // ── Secure endpoint (delegated) ────────────────────────────────────────────

  /**
   * Calls /secure/{workflow}.json — requires the signed-in user's token.
   *
   * Token flow (handled by MsalInterceptor):
   *  1. acquireTokenSilent for scope api://<resourceAppId>/user_impersonation
   *  2. If silent fails → interactive redirect login
   *  3. Token injected as: Authorization: Bearer <user_access_token>
   *
   * The Function App validates the token and can extract the user's identity
   * (name, UPN, object ID) from the JWT claims.
   *
   * @param workflowName  Warewolf workflow name.
   * @param body          JSON body of input variables.
   */
  callSecure(workflowName: string, queryParams: WorkflowInputs = {}, body: WorkflowInputs = {}): Observable<WorkflowResult> {
    const encoded = encodeURIComponent(workflowName);
    let params = new HttpParams();
    for (const [k, v] of Object.entries(queryParams)) {
      params = params.set(k, String(v));
    }
    return this.http.post<WorkflowResult>(
      `${this.base}/secure/${encoded}.json`,
      body,
      { params },
    );
  }

  // ── Services endpoint (delegated POST) ────────────────────────────────────

  /**
   * Calls /services/{workflow}.json — standard service integration endpoint.
   *
   * Identical auth flow to callSecure. The /services/ route is intended for
   * service-to-service integration patterns where a delegated token is
   * available (e.g. On-Behalf-Of flow from a middle tier).
   *
   * When called directly from the SPA, it behaves the same as /secure/ from
   * the browser's perspective — MsalInterceptor attaches the user token.
   *
   * @param workflowName  Warewolf workflow name.
   * @param body          JSON body of input variables.
   */
  callServices(workflowName: string, queryParams: WorkflowInputs = {}, body: WorkflowInputs = {}): Observable<WorkflowResult> {
    const encoded = encodeURIComponent(workflowName);
    let params = new HttpParams();
    for (const [k, v] of Object.entries(queryParams)) {
      params = params.set(k, String(v));
    }
    return this.http.post<WorkflowResult>(
      `${this.base}/services/${encoded}.json`,
      body,
      { params },
    );
  }
}
