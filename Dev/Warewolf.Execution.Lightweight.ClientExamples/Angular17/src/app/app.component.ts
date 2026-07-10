/**
 * app.component.ts — Application shell.
 *
 * ── Role of this component ────────────────────────────────────────────────────
 *
 *  1. Renders the <app-navbar> (login/logout + user info).
 *  2. Hosts the <router-outlet> where page components are rendered.
 *  3. Hosts <app-msal-redirect> — an invisible component provided by
 *     @azure/msal-angular that processes the Entra redirect callback.
 *
 * ── MSAL Redirect Component ────────────────────────────────────────────────────
 *  When Entra redirects back to the app after login, the URL contains an
 *  auth code (e.g. ?code=xyz&state=...). MsalRedirectComponent detects this
 *  and triggers handleRedirectObservable() to exchange the code for tokens.
 *  It must be present in the DOM for redirects to be processed.
 *
 * ── isIframe guard ────────────────────────────────────────────────────────────
 *  MSAL uses a hidden iframe for silent token refresh. If the app is rendered
 *  inside an iframe, we suppress the router-outlet to avoid nested routing
 *  conflicts during iframe-based silent token acquisition.
 */
import { NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MsalModule } from '@azure/msal-angular';
import { NavbarComponent } from './nav/navbar.component';

@Component({
  selector:   'app-root',
  standalone: true,
  imports:    [NgIf, RouterOutlet, MsalModule, NavbarComponent],
  template: `
    <!-- Main shell — hidden inside MSAL's silent refresh iframes -->
    <ng-container *ngIf="!isIframe">
      <app-navbar></app-navbar>

      <main class="main-content">
        <!-- Active route component (WorkflowComponent or ClientCredentialsComponent) -->
        <router-outlet></router-outlet>
      </main>
    </ng-container>
  `,
  styles: [`
    .main-content {
      min-height: calc(100vh - 64px);
      background: #fafafa;
    }
  `],
})
export class AppComponent implements OnInit {
  /**
   * True if the app is running inside a hidden iframe.
   * MSAL creates iframes for silent token refresh (acquireTokenSilent via iframe).
   * We must NOT render the app UI inside those iframes to avoid routing conflicts.
   */
  isIframe = false;

  ngOnInit(): void {
    this.isIframe = window !== window.parent && !window.opener;
  }
}

