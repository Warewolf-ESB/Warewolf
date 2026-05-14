/**
 * navbar.component.ts — Application toolbar with MSAL authentication controls.
 *
 * ── What this component does ──────────────────────────────────────────────────
 *  • Displays the app title and navigation links.
 *  • Shows the signed-in user's display name + initials avatar.
 *  • Provides "Sign In" and "Sign Out" buttons that delegate to AuthService.
 *  • Reacts to authentication state changes via AuthService.account$ observable.
 *
 * ── Auth state binding ────────────────────────────────────────────────────────
 *  The toolbar is data-driven: it subscribes to AuthService.account$ which is
 *  backed by an MSAL BroadcastService event listener. No polling — updates
 *  fire automatically after login/logout redirect callbacks complete.
 */
import { AsyncPipe, NgIf } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { AccountInfo } from '@azure/msal-browser';
import { map } from 'rxjs';
import { AuthService } from '../auth/auth.service';

@Component({
  selector:   'app-navbar',
  standalone: true,
  imports: [
    AsyncPipe, NgIf,
    RouterLink, RouterLinkActive,
    MatToolbarModule, MatButtonModule, MatIconModule,
    MatMenuModule, MatChipsModule, MatTooltipModule, MatDividerModule,
  ],
  template: `
    <mat-toolbar color="primary" class="navbar">
      <!-- Brand -->
      <span class="brand">
        <mat-icon class="brand-icon">cloud_sync</mat-icon>
        <span class="brand-name">wwexecution<span class="brand-env">dev</span><span class="brand-env">Angular App</span></span>
      </span>

      <!-- Navigation links -->
      <nav class="nav-links">
        <a mat-button routerLink="/home" routerLinkActive="nav-active"
           matTooltip="Home">
          <mat-icon>home</mat-icon> Home
        </a>
        <a mat-button routerLink="/workflow" routerLinkActive="nav-active"
           matTooltip="Delegated (user) auth demo">
          <mat-icon>play_circle</mat-icon> Workflow Demo
        </a>
        <!-- <a mat-button routerLink="/client-credentials" routerLinkActive="nav-active"
           matTooltip="App-only auth via backend proxy">
          <mat-icon>vpn_key</mat-icon> Client Credentials
        </a> -->
      </nav>

      <span class="spacer"></span>

      <!-- Auth state: signed out -->
      <ng-container *ngIf="!(authSvc.isAuthenticated$ | async)">
        <button mat-raised-button color="accent" (click)="authSvc.login()"
                matTooltip="Sign in with your Microsoft / Entra ID account">
          <mat-icon>login</mat-icon> Sign In
        </button>
      </ng-container>

      <!-- Auth state: signed in — show user chip + menu -->
      <ng-container *ngIf="authSvc.account$ | async as account">
        <button mat-button [matMenuTriggerFor]="userMenu"
                class="user-button"
                [matTooltip]="'Signed in as ' + account.username">
          <span class="avatar" [attr.aria-label]="'Avatar for ' + account.name">
            {{ initials(account) }}
          </span>
          <span class="user-name">&nbsp;</span>
          <span class="user-name">{{ account.name }}</span>
          <mat-icon>arrow_drop_down</mat-icon>
        </button>

        <mat-menu #userMenu="matMenu">
          <!-- Account info header -->
          <div class="menu-header" (click)="$event.stopPropagation()">
            <p class="menu-name">{{ account.name }}</p>
            <p class="menu-upn">{{ account.username }}</p>
            <p class="menu-tenant">Tenant: {{ account.tenantId }}</p>
          </div>
          <mat-divider></mat-divider>
          <button mat-menu-item routerLink="/home">
            <mat-icon>home</mat-icon> Home
          </button>
          <button mat-menu-item routerLink="/workflow">
            <mat-icon>play_circle</mat-icon> Workflow Demo
          </button>
          <!-- <button mat-menu-item routerLink="/client-credentials">
            <mat-icon>vpn_key</mat-icon> Client Credentials
          </button> -->
          <mat-divider></mat-divider>
          <button mat-menu-item (click)="authSvc.logout()">
            <mat-icon color="warn">logout</mat-icon>
            <span class="warn-text">Sign Out</span>
          </button>
        </mat-menu>
      </ng-container>
    </mat-toolbar>
  `,
  styles: [`
    .navbar         { position: sticky; top: 0; z-index: 100; gap: 8px; }
    .brand          { display: flex; align-items: center; gap: 6px; font-size: 1.15rem; font-weight: 700; }
    .brand-icon     { font-size: 28px; }
    .brand-env      { font-size: 0.7em; background: rgba(255,255,255,.25);
                      border-radius: 3px; padding: 1px 5px; margin-left: 4px; }
    .nav-links      { display: flex; gap: 4px; margin-left: 16px; }
    .nav-active     { background: rgba(255,255,255,.15); border-radius: 4px; }
    .spacer         { flex: 1; }
    .user-button    { display: flex; align-items: center; gap: 6px; }
    .avatar         { width: 30px; height: 30px; border-radius: 50%;
                      background: #fff; color: #1976d2;
                      display: inline-flex; align-items: center; justify-content: center;
                      font-weight: 700; font-size: 0.75rem; flex-shrink: 0; }
    .user-name      { max-width: 140px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .menu-header    { padding: 12px 16px; }
    .menu-name      { font-weight: 600; margin: 0 0 2px; }
    .menu-upn       { font-size: 0.8rem; color: #555; margin: 0 0 2px; }
    .menu-tenant    { font-size: 0.75rem; color: #888; margin: 0; font-family: monospace; }
    .warn-text      { color: #f44336; }
  `],
})
export class NavbarComponent {
  readonly authSvc = inject(AuthService);

  /** Derive two-letter initials from the account display name. */
  initials(account: AccountInfo): string {
    const parts = (account.name ?? account.username).split(/[\s@.]+/);
    return parts
      .filter(Boolean)
      .slice(0, 2)
      .map((p) => p[0].toUpperCase())
      .join('');
  }
}
