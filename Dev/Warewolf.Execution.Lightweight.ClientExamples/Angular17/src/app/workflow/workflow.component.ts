/**
 * workflow.component.ts — Delegated-auth workflow demo page.
 *
 * ── What this page demonstrates ──────────────────────────────────────────────
 *
 *  Three call patterns, all using the SAME HttpClient — the MsalInterceptor
 *  automatically differentiates them based on the URL:
 *
 *  ┌─────────────────┬───────────────┬───────────────────────────────────────┐
 *  │ Button          │ URL           │ Auth                                  │
 *  ├─────────────────┼───────────────┼───────────────────────────────────────┤
 *  │ Call Public     │ /public/  │ None — anonymous access               │
 *  │ Call Secure     │ /secure/  │ Bearer token (MsalInterceptor)        │
 *  │ Call Services   │ /services/│ Bearer token (MsalInterceptor, POST)  │
 *  └─────────────────┴───────────────┴───────────────────────────────────────┘
 *
 *  The component is protected by MsalGuard (see app.routes.ts).
 *  If the user is not signed in when navigating here, MSAL triggers an
 *  interactive redirect login before rendering this page.
 *
 *  Token display: After calling a secured endpoint, the current access token
 *  is passed to <app-token-display> for inspection of JWT claims.
 */
import { AsyncPipe, DatePipe, JsonPipe, NgFor, NgIf } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { Observable, finalize } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { ErrorPanelComponent } from '../shared/error-panel/error-panel.component';
import { TokenDisplayComponent } from '../shared/token-display/token-display.component';
import { CallContext, WorkflowInputs, WorkflowResult } from './workflow.model';
import { WorkflowService } from './workflow.service';

interface CallRecord {
  context:   CallContext;
  result:    WorkflowResult | null;
  error:     unknown;
  timestamp: Date;
}

@Component({
  selector:   'app-workflow',
  standalone: true,
  imports: [
    AsyncPipe, DatePipe, JsonPipe, NgFor, NgIf, FormsModule,
    MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule,
    MatIconModule, MatDividerModule, MatProgressSpinnerModule,
    MatTooltipModule, MatExpansionModule, MatChipsModule,
    ErrorPanelComponent, TokenDisplayComponent,
  ],
  template: `
    <div class="page-container">

      <!-- ── Page header ──────────────────────────────────────────────────── -->
      <mat-card class="info-card">
        <mat-card-header>
          <mat-icon mat-card-avatar color="primary">play_circle</mat-icon>
          <mat-card-title>Workflow Demo (Angular App) — Authorization Code + PKCE</mat-card-title>
          <mat-card-subtitle>
            Calls the wwexecutiondev Azure Function App with delegated (user) tokens.
          </mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <p class="info-text">
            This page is protected by <strong>MsalGuard</strong>.
            The guard checked that you have a valid account in MSAL's cache before allowing
            navigation here. Each secured call below uses your access token — attached
            automatically by the <strong>MsalInterceptor</strong> HTTP interceptor.
          </p>
        </mat-card-content>
      </mat-card>

      <!-- ── Inputs ───────────────────────────────────────────────────────── -->
      <mat-card class="inputs-card">
        <mat-card-header>
          <mat-card-title>Workflow Inputs</mat-card-title>
        </mat-card-header>
        <mat-card-content class="inputs-grid">
          <mat-form-field appearance="outline">
            <mat-label>Workflow Name</mat-label>
            <input matInput [(ngModel)]="workflowName" placeholder="Hello World" />
            <mat-hint>Warewolf workflow name (URI-encoded automatically)</mat-hint>
          </mat-form-field>

          <mat-form-field appearance="outline" class="inputs-full">
            <mat-label>Query String</mat-label>
            <textarea matInput [(ngModel)]="queryString" rows="2"
                      placeholder="Name=Alice&Age=30"></textarea>
            <mat-hint>key=value pairs separated by &amp; — appended to URL for all 3 call types</mat-hint>
          </mat-form-field>

          <mat-form-field appearance="outline" class="inputs-full">
            <mat-label>Request Body</mat-label>
            <textarea matInput [(ngModel)]="bodyString" rows="3"
                      placeholder="Name=Alice&#10;Age=30"></textarea>
            <mat-hint>key=value pairs separated by &amp; or newlines — sent as JSON body (Secure &amp; Services POST only; ignored for Public GET)</mat-hint>
          </mat-form-field>
        </mat-card-content>
      </mat-card>

      <!-- ── Action buttons ───────────────────────────────────────────────── -->
      <mat-card class="actions-card">
        <mat-card-header>
          <mat-card-title>Call the Function App</mat-card-title>
        </mat-card-header>
        <mat-card-content class="actions-row">

          <button mat-raised-button color="basic"
                  (click)="callPublic()" [disabled]="loading"
                  matTooltip="GET /public/{workflow} — no auth header sent">
            <mat-icon>public</mat-icon>
            Call Public
          </button>

          <button mat-raised-button color="primary"
                  (click)="callSecure()" [disabled]="loading"
                  matTooltip="POST /secure/{workflow} — MsalInterceptor attaches Bearer token">
            <mat-icon>lock</mat-icon>
            Call Secure
          </button>

          <!-- <button mat-raised-button color="accent"
                  (click)="callServices()" [disabled]="loading"
                  matTooltip="POST /services/{workflow} — same as Secure, different route">
            <mat-icon>settings_ethernet</mat-icon>
            Call Services
          </button> -->

          <button mat-stroked-button
                  (click)="fetchToken()" [disabled]="loading"
                  matTooltip="Call acquireTokenSilent() and show the raw JWT for inspection">
            <mat-icon>token</mat-icon>
            Show My Token
          </button>

        </mat-card-content>

        <!-- Loading indicator -->
        <div class="loading-row" *ngIf="loading">
          <mat-spinner diameter="20"></mat-spinner>
          <span>Calling {{ loadingContext }}…</span>
        </div>
      </mat-card>

      <!-- ── Token display ─────────────────────────────────────────────────── -->
      <div *ngIf="(authSvc.accessToken$ | async) as token">
        <app-token-display [token]="token" />
      </div>

      <!-- ── Results ──────────────────────────────────────────────────────── -->
      <ng-container *ngFor="let rec of callHistory">
        <mat-card class="result-card" [class.result-error]="rec.error">
          <mat-card-header>
            <mat-icon mat-card-avatar [color]="rec.error ? 'warn' : 'primary'">
              {{ contextIcon(rec.context) }}
            </mat-icon>
            <mat-card-title>{{ contextLabel(rec.context) }}</mat-card-title>
            <mat-card-subtitle>{{ rec.timestamp | date:'HH:mm:ss' }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <app-error-panel [error]="rec.error" />
            <pre *ngIf="rec.result" class="result-json">{{ rec.result | json }}</pre>
          </mat-card-content>
        </mat-card>
      </ng-container>

    </div>
  `,
  styles: [`
    .page-container  { max-width: 860px; margin: 24px auto; padding: 0 16px; display: flex; flex-direction: column; gap: 16px; }
    .info-card       { background: #e3f2fd; }
    .info-text       { margin: 8px 0 0; font-size: 0.92rem; line-height: 1.6; }
    .inputs-grid     { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; padding-top: 8px; }
    .inputs-full     { grid-column: 1 / -1; }
    .actions-row     { display: flex; flex-wrap: wrap; gap: 12px; padding-top: 8px; }
    .loading-row     { display: flex; align-items: center; gap: 8px; padding: 8px 16px; font-size: 0.9rem; color: #555; }
    .result-card     { border-left: 4px solid #1976d2; }
    .result-error    { border-left-color: #f44336; }
    .result-json     { font-family: monospace; font-size: 0.8rem; background: #f5f5f5;
                       padding: 12px; border-radius: 4px; overflow-x: auto; white-space: pre-wrap; }
  `],
})
export class WorkflowComponent implements OnInit {
  readonly authSvc = inject(AuthService);
  private readonly workflowSvc = inject(WorkflowService);

  workflowName = 'Hello World';
  queryString  = 'Name=Angular User';
  bodyString   = '';
  loading      = false;
  loadingContext: CallContext | '' = '';
  callHistory: CallRecord[] = [];

  ngOnInit(): void {
    // Eagerly show the token panel if already available from cache.
    this.authSvc.acquireTokenSilently();
  }

  callPublic(): void {
    this.execute(
      'public',
      this.workflowSvc.callPublic(this.workflowName, this.parseKvString(this.queryString)),
    );
  }

  callSecure(): void {
    // MsalInterceptor intercepts this HTTP request, calls acquireTokenSilent,
    // and injects: Authorization: Bearer <access_token>
    // If no valid token exists, MSAL triggers an interactive login redirect.
    this.execute(
      'secure',
      this.workflowSvc.callSecure(
        this.workflowName,
        this.parseKvString(this.queryString),
        this.parseKvString(this.bodyString),
      ),
    );
  }

  callServices(): void {
    this.execute(
      'services',
      this.workflowSvc.callServices(
        this.workflowName,
        this.parseKvString(this.queryString),
        this.parseKvString(this.bodyString),
      ),
    );
  }

  async fetchToken(): Promise<void> {
    this.loading        = true;
    this.loadingContext = '';
    // acquireTokenSilently() updates AuthService.accessToken$ which the
    // template binds via (authSvc.accessToken$ | async).
    await this.authSvc.acquireTokenSilently();
    this.loading = false;
  }

  private parseKvString(raw: string): WorkflowInputs {
    // Supports both "&" and newline as separators.
    // e.g. "Name=Alice&Age=30" or "Name=Alice\nAge=30"
    const normalised = raw.trim().replace(/\n/g, '&');
    const params = new URLSearchParams(normalised);
    const inputs: WorkflowInputs = {};
    params.forEach((value, key) => { inputs[key] = value; });
    return inputs;
  }

  private execute(context: CallContext, call$: Observable<WorkflowResult>): void {
    this.loading        = true;
    this.loadingContext = context;

    call$.pipe(finalize(() => { this.loading = false; this.loadingContext = ''; }))
      .subscribe({
        next:  (result) => this.pushRecord(context, result, null),
        error: (error)  => this.pushRecord(context, null, error),
      });
  }

  private pushRecord(context: CallContext, result: WorkflowResult | null, error: unknown): void {
    // Prepend so newest result appears at top.
    this.callHistory = [{ context, result, error, timestamp: new Date() }, ...this.callHistory];
  }

  contextLabel(ctx: CallContext): string {
    return {
      public:               'Public Call — GET /public/',
      secure:               'Secure Call — POST /secure/',
      services:             'Services Call — POST /services/',
      'client-credentials': 'Client Credentials (app-only)',
    }[ctx] ?? ctx;
  }

  contextIcon(ctx: CallContext): string {
    return { public: 'public', secure: 'lock', services: 'settings_ethernet',
             'client-credentials': 'vpn_key' }[ctx] ?? 'info';
  }
}
