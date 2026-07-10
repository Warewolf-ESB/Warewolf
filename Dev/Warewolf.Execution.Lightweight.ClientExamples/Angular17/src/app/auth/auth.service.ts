/**
 * auth.service.ts — Facade over MsalService for the application layer.
 *
 * Responsibilities:
 *  - Expose observables for authentication state (isAuthenticated$, account$).
 *  - Provide login / logout actions.
 *  - Expose the raw access token for display/diagnostic purposes.
 *
 * Components should inject AuthService rather than MsalService directly,
 * keeping MSAL implementation details isolated to this layer.
 */
import { Injectable, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import {
  AccountInfo,
  AuthenticationResult,
  EventMessage,
  EventType,
  InteractionStatus,
  SilentRequest,
} from '@azure/msal-browser';
import {
  BehaviorSubject,
  Observable,
  Subject,
  filter,
  map,
  takeUntil,
} from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService implements OnDestroy {
  // ── Internal state ─────────────────────────────────────────────────────────
  private readonly _account$ = new BehaviorSubject<AccountInfo | null>(null);
  private readonly _token$   = new BehaviorSubject<string | null>(null);
  private readonly _destroy$ = new Subject<void>();

  // ── Public observables ─────────────────────────────────────────────────────

  /** Emits the currently signed-in account, or null if not authenticated. */
  readonly account$: Observable<AccountInfo | null> = this._account$.asObservable();

  /** True while an account is present in MSAL's cache. */
  readonly isAuthenticated$: Observable<boolean> = this._account$.pipe(
    map((a) => !!a),
  );

  /**
   * The raw JWT access token (for display/diagnostics only).
   * NEVER forward this to untrusted parties.
   * The MsalInterceptor handles token attachment for API calls automatically.
   */
  readonly accessToken$: Observable<string | null> = this._token$.asObservable();

  constructor(
    private readonly msalSvc: MsalService,
    private readonly broadcastSvc: MsalBroadcastService,
    private readonly router: Router,
  ) {
    this.initializeAccountState();
    this.subscribeToMsalEvents();
  }

  // ── Initialisation ─────────────────────────────────────────────────────────

  /**
   * Called once at app startup (via APP_INITIALIZER in app.config.ts).
   * Handles the redirect callback after login and sets the active account.
   *
   * MSAL redirect flow:
   *  1. User clicks "Login" → MSAL redirects to Entra /authorize.
   *  2. Entra redirects back with an auth code in the URL hash.
   *  3. handleRedirectObservable() exchanges the code for tokens and clears
   *     the auth code from the URL, then the BroadcastService emits
   *     EventType.LOGIN_SUCCESS.
   */
  initialize(): Observable<AuthenticationResult | null> {
    return this.msalSvc.handleRedirectObservable();
  }

  private initializeAccountState(): void {
    // After the redirect observable resolves, set the active account.
    // msalSvc.instance.getActiveAccount() returns null if no account is cached.
    const active = this.msalSvc.instance.getActiveAccount()
      ?? this.msalSvc.instance.getAllAccounts()[0]
      ?? null;

    if (active) {
      this.msalSvc.instance.setActiveAccount(active);
    }
    this._account$.next(active);
  }

  /**
   * Subscribe to MSAL broadcast events to keep local state in sync.
   *
   * Key events:
   *  LOGIN_SUCCESS  — user completed interactive login
   *  LOGOUT_SUCCESS — user signed out
   *  ACQUIRE_TOKEN_SUCCESS — silent or interactive token acquired
   *  SSO_SILENT_SUCCESS — single-sign-on via hidden iframe succeeded
   */
  private subscribeToMsalEvents(): void {
    // Wait for interaction to finish before reading account state.
    this.broadcastSvc.inProgress$
      .pipe(
        filter((s) => s === InteractionStatus.None),
        takeUntil(this._destroy$),
      )
      .subscribe(() => this.initializeAccountState());

    // Listen for login / token events.
    this.broadcastSvc.msalSubject$
      .pipe(
        filter(
          (e: EventMessage) =>
            e.eventType === EventType.LOGIN_SUCCESS ||
            e.eventType === EventType.ACQUIRE_TOKEN_SUCCESS ||
            e.eventType === EventType.SSO_SILENT_SUCCESS,
        ),
        takeUntil(this._destroy$),
      )
      .subscribe((event: EventMessage) => {
        const result = event.payload as AuthenticationResult;
        if (result?.account) {
          this.msalSvc.instance.setActiveAccount(result.account);
          this._account$.next(result.account);
        }
        if (result?.accessToken) {
          this._token$.next(result.accessToken);
        }
      });

    // Listen for logout events.
    this.broadcastSvc.msalSubject$
      .pipe(
        filter((e: EventMessage) => e.eventType === EventType.LOGOUT_SUCCESS),
        takeUntil(this._destroy$),
      )
      .subscribe(() => {
        this._account$.next(null);
        this._token$.next(null);
        this.router.navigate(['/home']);
      });
  }

  // ── Public Actions ─────────────────────────────────────────────────────────

  /**
   * Trigger an interactive login redirect.
   * MSAL redirects the browser to the Entra login page.
   * After sign-in, Entra redirects back and handleRedirectObservable()
   * exchanges the auth code + PKCE verifier for tokens.
   */
  login(): void {
    this.msalSvc.loginRedirect({ scopes: [environment.userScope] });
  }

  /**
   * Sign out the current user.
   * MSAL clears the token cache and redirects to Entra /logout,
   * which then redirects back to postLogoutRedirectUri.
   */
  logout(): void {
    const account = this._account$.getValue();
    this.msalSvc.logoutRedirect({
      account: account ?? undefined,
      postLogoutRedirectUri: environment.redirectUri,
    });
  }

  /**
   * Silently acquire an access token for the current user.
   *
   * MSAL first checks its in-memory / localStorage cache. If the cached
   * token is still valid it returns immediately. If expired, MSAL uses a
   * refresh token (via a hidden iframe) to get a new token without user
   * interaction.
   *
   * Use this for advanced scenarios (WebSockets, manual Authorization header).
   * Normal API calls via HttpClient do NOT need this — MsalInterceptor handles
   * token attachment automatically.
   */
  async acquireTokenSilently(): Promise<string | null> {
    const account = this._account$.getValue();
    if (!account) return null;

    const request: SilentRequest = {
      scopes:  [environment.userScope],
      account,
    };

    try {
      const result = await this.msalSvc.instance.acquireTokenSilent(request);
      this._token$.next(result.accessToken);
      return result.accessToken;
    } catch {
      // Token refresh failed — user must sign in interactively again.
      return null;
    }
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
