/**
 * token-display.component.ts — JWT token inspector for diagnostics.
 *
 * Decodes the three parts of a JWT access token (header, payload, signature)
 * and renders them in a Material expansion panel.  Highlights key claims:
 *
 *  aud   — Audience: must match the Function App's resource app ID.
 *  iss   — Issuer: Entra ID tenant endpoint.
 *  oid   — Object ID: unique, stable identifier for the user.
 *  name  — Display name.
 *  upn   — User Principal Name (email).
 *  scp   — Delegated scopes granted.
 *  exp   — Expiry (Unix timestamp).
 *
 * ⚠ This component is for DEVELOPMENT / DEMO purposes only.
 *   Never display raw JWT tokens in production UIs.
 *
 * Usage:
 *   <app-token-display [token]="accessToken" />
 */
import { DatePipe, JsonPipe, NgIf, NgFor, KeyValuePipe } from '@angular/common';
import { Component, Input, OnChanges } from '@angular/core';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';

interface TokenClaims { [key: string]: unknown; }

@Component({
  selector:   'app-token-display',
  standalone: true,
  imports:    [NgIf, NgFor, KeyValuePipe, DatePipe, JsonPipe, MatExpansionModule, MatChipsModule, MatIconModule, MatDividerModule],
  template: `
    <mat-expansion-panel *ngIf="token" class="token-panel">
      <mat-expansion-panel-header>
        <mat-panel-title>
          <mat-icon class="token-icon">token</mat-icon>
          &nbsp;Access Token Inspector
        </mat-panel-title>
        <mat-panel-description>{{ expiryLabel }}</mat-panel-description>
      </mat-expansion-panel-header>

      <!-- Key claims summary -->
      <div class="claims-summary">
        <div class="claim-row" *ngIf="claims['name']">
          <span class="claim-label">User</span>
          <span class="claim-value">{{ claims['name'] }}</span>
        </div>
        <div class="claim-row" *ngIf="claims['upn'] || claims['preferred_username']">
          <span class="claim-label">UPN</span>
          <span class="claim-value">{{ claims['upn'] ?? claims['preferred_username'] }}</span>
        </div>
        <div class="claim-row" *ngIf="claims['oid']">
          <span class="claim-label">Object ID</span>
          <span class="claim-value mono">{{ claims['oid'] }}</span>
        </div>
        <div class="claim-row" *ngIf="claims['aud']">
          <span class="claim-label">Audience</span>
          <span class="claim-value mono">{{ claims['aud'] }}</span>
        </div>
        <div class="claim-row" *ngIf="claims['scp']">
          <span class="claim-label">Scopes</span>
          <mat-chip-set>
            <mat-chip *ngFor="let s of scopes">{{ s }}</mat-chip>
          </mat-chip-set>
        </div>
        <div class="claim-row" *ngIf="claims['exp']">
          <span class="claim-label">Expires</span>
          <span class="claim-value">{{ expiry | date:'medium' }}</span>
        </div>
      </div>

      <mat-divider></mat-divider>

      <!-- Full raw claims -->
      <mat-expansion-panel class="raw-panel">
        <mat-expansion-panel-header>
          <mat-panel-title>Raw Claims (JSON)</mat-panel-title>
        </mat-expansion-panel-header>
        <pre class="raw-json">{{ claims | json }}</pre>
      </mat-expansion-panel>

      <!-- Raw token (truncated) -->
      <mat-expansion-panel class="raw-panel">
        <mat-expansion-panel-header>
          <mat-panel-title>Raw Token</mat-panel-title>
        </mat-expansion-panel-header>
        <p class="raw-token-warning">⚠ Never share this token — treat it like a password.</p>
        <pre class="raw-json token-text">{{ token }}</pre>
      </mat-expansion-panel>
    </mat-expansion-panel>
  `,
  styles: [`
    .token-panel   { margin: 8px 0; }
    .token-icon    { vertical-align: middle; color: #1976d2; }
    .claims-summary { padding: 16px 0; display: flex; flex-direction: column; gap: 10px; }
    .claim-row     { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; }
    .claim-label   { min-width: 90px; font-size: 0.8rem; font-weight: 600;
                     color: #555; text-transform: uppercase; letter-spacing: .05em; }
    .claim-value   { font-size: 0.9rem; color: #222; }
    .mono          { font-family: monospace; font-size: 0.8rem; }
    .raw-panel     { margin-top: 8px; box-shadow: none; border: 1px solid #e0e0e0; }
    .raw-json      { font-family: monospace; font-size: 0.78rem; white-space: pre-wrap;
                     word-break: break-all; max-height: 240px; overflow-y: auto; }
    .token-text    { color: #1976d2; }
    .raw-token-warning { font-size: 0.8rem; color: #e65100; margin: 0 0 8px; }
  `],
})
export class TokenDisplayComponent implements OnChanges {
  /**
   * The raw JWT access_token string.
   * Pass the value from AuthService.accessToken$.
   */
  @Input() token: string | null = null;

  claims: TokenClaims = {};
  scopes: string[]    = [];
  expiry: Date | null = null;
  expiryLabel         = '';

  ngOnChanges(): void {
    this.claims     = {};
    this.scopes     = [];
    this.expiry     = null;
    this.expiryLabel = '';

    if (!this.token) return;

    try {
      // JWT structure: header.payload.signature (Base64URL encoded)
      const payloadB64 = this.token.split('.')[1];
      const json       = atob(payloadB64.replace(/-/g, '+').replace(/_/g, '/'));
      this.claims      = JSON.parse(json) as TokenClaims;

      if (typeof this.claims['scp'] === 'string') {
        this.scopes = (this.claims['scp'] as string).split(' ');
      }

      if (typeof this.claims['exp'] === 'number') {
        this.expiry     = new Date((this.claims['exp'] as number) * 1000);
        const mins      = Math.round((this.expiry.getTime() - Date.now()) / 60_000);
        this.expiryLabel = mins > 0 ? `Expires in ${mins} min` : 'Expired';
      }
    } catch {
      this.claims = { error: 'Failed to decode token payload.' };
    }
  }
}
