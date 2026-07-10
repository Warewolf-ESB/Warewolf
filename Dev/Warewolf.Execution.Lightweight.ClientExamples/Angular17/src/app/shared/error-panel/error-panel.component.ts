/**
 * error-panel.component.ts — Reusable Material-styled error / info banner.
 *
 * Accepts an error object, string, or null and renders an appropriate
 * Angular Material card with icon, title and message.
 *
 * Usage:
 *   <app-error-panel [error]="myError" />
 */
import { NgIf } from '@angular/common';
import { Component, Input, OnChanges } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

export interface ErrorDetail {
  title:   string;
  message: string;
  hint?:   string;
}

@Component({
  selector:    'app-error-panel',
  standalone:  true,
  imports:     [NgIf, MatIconModule],
  template: `
    <div class="error-panel" *ngIf="detail" role="alert" aria-live="polite">
      <mat-icon class="error-icon">error_outline</mat-icon>
      <div class="error-body">
        <p class="error-title">{{ detail.title }}</p>
        <p class="error-message">{{ detail.message }}</p>
        <p class="error-hint" *ngIf="detail.hint">💡 {{ detail.hint }}</p>
      </div>
    </div>
  `,
  styles: [`
    .error-panel {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      background: #fdecea;
      border-left: 4px solid #f44336;
      border-radius: 4px;
      padding: 12px 16px;
      margin: 8px 0;
    }
    .error-icon { color: #f44336; margin-top: 2px; flex-shrink: 0; }
    .error-body { flex: 1; }
    .error-title   { font-weight: 600; margin: 0 0 4px; color: #b71c1c; }
    .error-message { margin: 0 0 4px; font-size: 0.9rem; color: #333; }
    .error-hint    { margin: 0; font-size: 0.85rem; color: #666; }
  `],
})
export class ErrorPanelComponent implements OnChanges {
  /**
   * Accepts an Error object, a plain string, or null/undefined.
   * When null, the panel is hidden.
   */
  @Input() error: unknown = null;

  detail: ErrorDetail | null = null;

  ngOnChanges(): void {
    this.detail = this.parse(this.error);
  }

  private parse(raw: unknown): ErrorDetail | null {
    if (!raw) return null;

    // HTTP error from Angular HttpClient
    if (typeof raw === 'object' && raw !== null && 'status' in raw) {
      const err = raw as { status: number; message?: string; error?: { message?: string } };
      return {
        title:   `HTTP ${err.status} Error`,
        message: err.error?.message ?? err.message ?? 'An unexpected error occurred.',
        hint:    err.status === 401
          ? 'Your session may have expired. Try signing in again.'
          : err.status === 403
            ? 'You do not have permission to call this workflow.'
            : undefined,
      };
    }

    if (raw instanceof Error) {
      return { title: raw.name, message: raw.message };
    }

    return { title: 'Error', message: String(raw) };
  }
}
