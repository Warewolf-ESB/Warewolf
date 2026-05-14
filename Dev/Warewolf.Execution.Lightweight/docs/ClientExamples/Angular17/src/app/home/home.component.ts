import { NgIf, AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { AuthService } from '../auth/auth.service';

@Component({
  selector:   'app-home',
  standalone: true,
  imports: [
    NgIf, AsyncPipe, RouterLink,
    MatCardModule, MatButtonModule, MatIconModule, MatDividerModule, MatChipsModule,
  ],
  template: `
    <div class="home-container">

      <!-- Hero -->
      <mat-card class="hero-card">
        <mat-card-content class="hero-content">
          <mat-icon class="hero-icon">cloud_sync</mat-icon>
          <h1 class="hero-title">wwexecution<span class="brand-env">dev</span></h1>
          <p class="hero-sub">Angular 17 · MSAL · Azure Function App</p>
          <p class="hero-desc">
            A reference Single-Page Application that demonstrates how to call a
            <strong>Warewolf Execution Azure Function App</strong> from an Angular
            front-end using <strong>Microsoft Entra ID</strong> (Azure AD) authentication.
          </p>

          <div class="hero-actions">
            <ng-container *ngIf="authSvc.isAuthenticated$ | async; else signInBlock">
              <button mat-raised-button color="primary" routerLink="/workflow">
                <mat-icon>play_circle</mat-icon> Open Workflow Demo
              </button>
            </ng-container>
            <ng-template #signInBlock>
              <button mat-raised-button color="accent" (click)="authSvc.login()">
                <mat-icon>login</mat-icon> Sign In with Microsoft
              </button>
            </ng-template>
          </div>
        </mat-card-content>
      </mat-card>

      <!-- Feature cards -->
      <div class="features-grid">

        <mat-card class="feature-card">
          <mat-card-header>
            <mat-icon mat-card-avatar color="primary">public</mat-icon>
            <mat-card-title>Public Endpoint</mat-card-title>
            <mat-card-subtitle>GET /public/&#123;workflow&#125;.json</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p>No authentication required. Query string inputs are passed directly to the
            Warewolf workflow. Ideal for anonymous or read-only workflows.</p>
          </mat-card-content>
        </mat-card>

        <mat-card class="feature-card">
          <mat-card-header>
            <mat-icon mat-card-avatar color="primary">lock</mat-icon>
            <mat-card-title>Secure Endpoint</mat-card-title>
            <mat-card-subtitle>POST /secure/&#123;workflow&#125;.json</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p>Requires a signed-in user. The <strong>MsalInterceptor</strong> automatically
            attaches a Bearer token. The Function App validates the token and extracts user
            identity claims.</p>
          </mat-card-content>
        </mat-card>

        <mat-card class="feature-card">
          <mat-card-header>
            <mat-icon mat-card-avatar color="primary">settings_ethernet</mat-icon>
            <mat-card-title>Services Endpoint</mat-card-title>
            <mat-card-subtitle>POST /services/&#123;workflow&#125;.json</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p>Same delegated-auth flow as Secure. Intended for service-to-service integration
            patterns where an On-Behalf-Of delegated token is available from a middle tier.</p>
          </mat-card-content>
        </mat-card>

      </div>

      <!-- Auth flow overview -->
      <mat-card class="flow-card">
        <mat-card-header>
          <mat-icon mat-card-avatar>vpn_lock</mat-icon>
          <mat-card-title>Authorization Code + PKCE Flow</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <ol class="flow-steps">
            <li>User clicks <strong>Sign In</strong> — MSAL redirects to <code>login.microsoftonline.com</code>.</li>
            <li>Entra ID authenticates the user and issues an <em>authorization code</em>.</li>
            <li>MSAL exchanges the code (+ PKCE verifier) for an <strong>access token</strong> and <strong>refresh token</strong>.</li>
            <li>Tokens are cached in <code>localStorage</code>. The MsalInterceptor attaches the token to every protected HTTP request automatically.</li>
            <li>When the access token expires, MSAL silently refreshes it using the refresh token — no user interaction needed.</li>
          </ol>
          <mat-divider></mat-divider>
          <div class="chip-row">
            <mat-chip>Angular 17</mat-chip>
            <mat-chip>MSAL Angular</mat-chip>
            <mat-chip>Entra ID</mat-chip>
            <mat-chip>PKCE</mat-chip>
            <mat-chip>Azure Function App</mat-chip>
            <mat-chip>Warewolf</mat-chip>
          </div>
        </mat-card-content>
      </mat-card>

    </div>
  `,
  styles: [`
    .home-container   { max-width: 900px; margin: 32px auto; padding: 0 16px; display: flex; flex-direction: column; gap: 24px; }
    .hero-card        { background: linear-gradient(135deg, #1565c0 0%, #1976d2 100%); color: #fff; }
    .hero-content     { display: flex; flex-direction: column; align-items: center; text-align: center; padding: 40px 24px; }
    .hero-icon        { font-size: 64px; width: 64px; height: 64px; margin-bottom: 16px; opacity: .9; }
    .hero-title       { margin: 0 0 4px; font-size: 2.2rem; font-weight: 700; letter-spacing: -0.5px; }
    .brand-env        { font-size: 0.55em; background: rgba(255,255,255,.25); border-radius: 4px; padding: 2px 7px; margin-left: 6px; vertical-align: middle; }
    .hero-sub         { margin: 0 0 16px; opacity: .8; font-size: .95rem; letter-spacing: 1px; }
    .hero-desc        { max-width: 580px; line-height: 1.7; margin: 0 0 28px; font-size: 1rem; opacity: .95; }
    .hero-actions     { display: flex; gap: 12px; flex-wrap: wrap; justify-content: center; }
    .features-grid    { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 16px; }
    .feature-card p   { font-size: 0.9rem; line-height: 1.6; color: #555; margin: 8px 0 0; }
    .flow-card        {}
    .flow-steps       { padding-left: 20px; line-height: 2; font-size: 0.92rem; color: #444; }
    .flow-steps code  { background: #f0f0f0; padding: 1px 5px; border-radius: 3px; font-size: .85em; }
    mat-divider       { margin: 16px 0; }
    .chip-row         { display: flex; flex-wrap: wrap; gap: 8px; }
  `],
})
export class HomeComponent {
  readonly authSvc = inject(AuthService);
}
