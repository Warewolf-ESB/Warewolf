/**
 * client-credentials.component.ts — Client Credentials flow explainer + demo.
 *
 * ── About the Client Credentials Flow ────────────────────────────────────────
 *
 *  The OAuth 2.0 Client Credentials grant is an APP-ONLY flow used for
 *  server-to-server communication where NO user is involved.
 *
 *  ⚠ This flow CANNOT run in a browser because it requires a CLIENT SECRET
 *    (or certificate) which must never be exposed to end users.
 *
 *  Architecture — Backend Proxy Pattern:
 *
 *  ┌────────────────┐  HTTP (no secret)  ┌─────────────────────┐
 *  │  Angular SPA   │ ─────────────────► │  Your Backend API   │
 *  │  (Browser)     │                   │  (Node / .NET / etc)│
 *  └────────────────┘                   └──────────┬──────────┘
 *                                                   │  POST /token
 *                                                   │  client_id + client_secret
 *                                                   ▼
 *                                        ┌──────────────────────┐
 *                                        │  Entra ID /token     │
 *                                        │  → app-only token    │
 *                                        └──────────┬───────────┘
 *                                                   │  Authorization: Bearer <app_token>
 *                                                   ▼
 *                                        ┌──────────────────────┐
 *                                        │  wwexecution         │
 *                                        │  Azure Function App  │
 *                                        └──────────────────────┘
 *
 *  Steps (backend, not browser):
 *  1. POST https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token
 *     Body: grant_type=client_credentials
 *           client_id=<daemon-app-client-id>
 *           client_secret=<daemon-app-secret>
 *           scope=api://<resourceAppId>/.default
 *  2. Receive { access_token, expires_in, token_type: "Bearer" }
 *  3. Call Function App with: Authorization: Bearer <access_token>
 *
 *  The token contains app identity claims (oid = service principal OID,
 *  appid = daemon client ID) but NO user claims (no name, upn, etc.).
 *
 * ── This Demo ─────────────────────────────────────────────────────────────────
 *  Since the secret must stay on the server, this page:
 *  1. Shows the full flow diagram and explanation above.
 *  2. Provides a "Call Backend Proxy" button that calls your own backend API
 *     (configured via environment.backendProxyUrl).
 *  3. The backend API performs the client credentials exchange and proxies
 *     the call to the Function App.
 *  4. Results are displayed identically to the Workflow Demo page.
 */
import { JsonPipe, NgFor, NgIf } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize } from 'rxjs';
import { ErrorPanelComponent } from '../shared/error-panel/error-panel.component';
import { WorkflowResult } from '../workflow/workflow.model';
import { environment } from '../../environments/environment';

@Component({
  selector:   'app-client-credentials',
  standalone: true,
  imports: [
    NgIf, NgFor, JsonPipe, FormsModule,
    MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule,
    MatIconModule, MatDividerModule, MatProgressSpinnerModule,
    MatStepperModule, MatTooltipModule,
    ErrorPanelComponent,
  ],
  template: `
    <div class="page-container">

      <!-- ── Header ─────────────────────────────────────────────────────── -->
      <mat-card class="warn-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>vpn_key</mat-icon>
          <mat-card-title>Client Credentials Flow — App-Only Auth</mat-card-title>
          <mat-card-subtitle>
            This flow runs on a <strong>backend server</strong>, never in the browser.
          </mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p class="info-text">
            The <strong>OAuth 2.0 Client Credentials</strong> grant issues tokens for an
            application identity (service principal), not a user. Use this flow for daemons,
            background services, and automated pipelines that call the Function App without
            a human signing in.
          </p>
          <div class="security-banner">
            <mat-icon class="sec-icon">security</mat-icon>
            <span>
              The <code>client_secret</code> (or certificate) MUST be stored securely on the
              server (e.g. Azure Key Vault) and never sent to the browser. This page
              demonstrates the pattern via a <strong>backend proxy</strong>.
            </span>
          </div>
        </mat-card-content>
      </mat-card>

      <!-- ── Flow diagram ───────────────────────────────────────────────── -->
      <mat-card>
        <mat-card-header>
          <mat-card-title>How It Works</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <mat-stepper orientation="vertical" [linear]="false">
            <mat-step label="Angular SPA → Your Backend">
              <p>The browser makes an HTTP call to your own backend API.
              No credentials leave the browser — the backend holds the secret.</p>
            </mat-step>
            <mat-step label="Backend → Entra ID /token (Client Credentials)">
              <p>The backend POSTs to:</p>
              <pre class="code-block">POST https://login.microsoftonline.com/{{ tenantId }}/oauth2/v2.0/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
client_id=&lt;daemon-app-id&gt;
client_secret=&lt;SECRET — stays on server&gt;
scope=api://{{ resourceAppId }}/.default</pre>
              <p>Entra returns a short-lived <strong>app-only</strong> access token.</p>
            </mat-step>
            <mat-step label="Backend → wwexecution Function App">
              <p>The backend calls the Function App with:</p>
              <pre class="code-block">GET {{ functionAppUrl }}/services/&lt;workflow&gt;.json
Authorization: Bearer &lt;app_access_token&gt;</pre>
              <p>The token contains the service principal's OID — no user claims.</p>
            </mat-step>
            <mat-step label="Backend → Angular SPA (response)">
              <p>The backend returns the Function App response to the browser.</p>
            </mat-step>
          </mat-stepper>
        </mat-card-content>
      </mat-card>

      <!-- ── Demo: call backend proxy ──────────────────────────────────── -->
      <mat-card>
        <mat-card-header>
          <mat-card-title>Live Demo — Backend Proxy Call</mat-card-title>
          <mat-card-subtitle>
            Calls your local backend proxy which performs the Client Credentials exchange.
            Configure <code>environment.backendProxyUrl</code> to point at your backend.
          </mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <div class="proxy-url-display">
            <mat-icon>dns</mat-icon>
            <span>Backend proxy: <code>{{ backendProxyUrl }}</code></span>
          </div>

          <div class="inputs-grid">
            <mat-form-field appearance="outline">
              <mat-label>Workflow Name</mat-label>
              <input matInput [(ngModel)]="workflowName" placeholder="Hello World" />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Input Parameter</mat-label>
              <input matInput [(ngModel)]="inputParam" placeholder="Value" />
            </mat-form-field>
          </div>

          <button mat-raised-button color="primary"
                  (click)="callProxy()" [disabled]="loading">
            <mat-icon>send</mat-icon>
            Call via Backend Proxy
          </button>

          <div class="loading-row" *ngIf="loading">
            <mat-spinner diameter="20"></mat-spinner>
            <span>Calling backend proxy…</span>
          </div>
        </mat-card-content>
      </mat-card>

      <!-- ── Result ─────────────────────────────────────────────────────── -->
      <mat-card *ngIf="result || error" [class.result-error]="!!error" class="result-card">
        <mat-card-header>
          <mat-icon mat-card-avatar [color]="error ? 'warn' : 'primary'">
            {{ error ? 'error' : 'check_circle' }}
          </mat-icon>
          <mat-card-title>{{ error ? 'Error' : 'Success' }}</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <app-error-panel [error]="error" />
          <pre *ngIf="result" class="result-json">{{ result | json }}</pre>
        </mat-card-content>
      </mat-card>

    </div>
  `,
  styles: [`
    .page-container    { max-width: 860px; margin: 24px auto; padding: 0 16px; display: flex; flex-direction: column; gap: 16px; }
    .warn-card         { background: #fff8e1; }
    .info-text         { font-size: 0.92rem; line-height: 1.6; margin: 8px 0; }
    .security-banner   { display: flex; align-items: flex-start; gap: 8px; background: #fce4ec;
                         border-radius: 4px; padding: 10px 14px; margin-top: 12px; font-size: 0.88rem; }
    .sec-icon          { color: #c62828; flex-shrink: 0; }
    code               { font-family: monospace; background: rgba(0,0,0,.07); padding: 1px 4px; border-radius: 3px; }
    .code-block        { font-family: monospace; font-size: 0.78rem; background: #f5f5f5;
                         padding: 12px; border-radius: 4px; white-space: pre-wrap; word-break: break-all; }
    .proxy-url-display { display: flex; align-items: center; gap: 8px; margin-bottom: 16px; font-size: 0.9rem; }
    .inputs-grid       { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 12px; }
    .loading-row       { display: flex; align-items: center; gap: 8px; margin-top: 8px; font-size: 0.9rem; color: #555; }
    .result-card       { border-left: 4px solid #1976d2; }
    .result-error      { border-left-color: #f44336; }
    .result-json       { font-family: monospace; font-size: 0.8rem; background: #f5f5f5;
                         padding: 12px; border-radius: 4px; overflow-x: auto; white-space: pre-wrap; }
  `],
})
export class ClientCredentialsComponent {
  private readonly http = inject(HttpClient);

  readonly tenantId      = environment.entra.tenantId;
  readonly resourceAppId = environment.entra.resourceAppId;
  readonly functionAppUrl = environment.functionAppUrl;

  /**
   * Backend proxy URL.
   * The backend receives this request, performs the client_credentials token
   * exchange with Entra, then calls the Function App and returns the result.
   *
   * Set this to your local or deployed backend API URL.
   * Example: http://localhost:5000/api/proxy/workflow
   */
  readonly backendProxyUrl = 'http://localhost:5000/api/proxy/workflow';

  workflowName = 'Hello World';
  inputParam   = 'DemoValue';
  loading      = false;
  result: WorkflowResult | null = null;
  error: unknown = null;

  callProxy(): void {
    this.loading = true;
    this.result  = null;
    this.error   = null;

    // This is a plain HTTP call to YOUR backend — no MSAL token attached
    // because the backend proxy URL is not in MsalInterceptor's protectedResourceMap.
    this.http
      .post<WorkflowResult>(this.backendProxyUrl, {
        workflowName: this.workflowName,
        inputs: { Value: this.inputParam },
      })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next:  (r) => (this.result = r),
        error: (e) => (this.error  = e),
      });
  }
}
