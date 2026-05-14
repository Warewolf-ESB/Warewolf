/**
 * app.config.ts — Root application configuration (Angular 17 standalone bootstrap).
 *
 * This is the Angular 17 equivalent of AppModule. All providers that were
 * previously declared in @NgModule.providers are registered here.
 *
 * ── Provider breakdown ────────────────────────────────────────────────────────
 *
 *  provideRouter                — sets up the router with hash-free URLs.
 *    withEnabledBlockingInitialNavigation()
 *      Required by MSAL: delays the first navigation until after the redirect
 *      callback has been processed, preventing duplicate route activations.
 *
 *  provideHttpClient
 *    withInterceptorsFromDi()
 *      Enables class-based HTTP interceptors registered via HTTP_INTERCEPTORS.
 *      MsalInterceptor uses this pattern.
 *
 *  MsalModule providers:
 *    MSAL_INSTANCE        — the PublicClientApplication instance.
 *    MSAL_GUARD_CONFIG    — guard interaction type & scopes.
 *    MSAL_INTERCEPTOR_CONFIG — protected resource map.
 *    MsalService          — core MSAL service (singleton).
 *    MsalGuard            — route guard.
 *    MsalBroadcastService — event bus for login/logout/token events.
 *
 *  MsalInterceptor — HTTP_INTERCEPTORS token (multi: true):
 *    Intercepts outgoing HttpClient requests, checks the URL against
 *    protectedResourceMap, and injects the Bearer token when matched.
 */
import { APP_INITIALIZER, ApplicationConfig } from '@angular/core';
import { provideRouter, withEnabledBlockingInitialNavigation } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import {
  MSAL_GUARD_CONFIG,
  MSAL_INSTANCE,
  MSAL_INTERCEPTOR_CONFIG,
  MsalBroadcastService,
  MsalGuard,
  MsalInterceptor,
  MsalService,
} from '@azure/msal-angular';
import { routes } from './app.routes';
import {
  MSALGuardConfigFactory,
  MSALInstanceFactory,
  MSALInterceptorConfigFactory,
} from './auth/auth.config';
import { AuthService } from './auth/auth.service';

export const appConfig: ApplicationConfig = {
  providers: [
    // ── Router ──────────────────────────────────────────────────────────────
    provideRouter(
      routes,
      // REQUIRED by MSAL: blocks initial navigation until the redirect
      // callback finishes processing the auth code from the URL.
      withEnabledBlockingInitialNavigation(),
    ),

    // ── HTTP ─────────────────────────────────────────────────────────────────
    // withInterceptorsFromDi() enables legacy class-based interceptors
    // registered via the HTTP_INTERCEPTORS token (required by MsalInterceptor).
    provideHttpClient(withInterceptorsFromDi()),

    // ── Animations (required by Angular Material) ────────────────────────────
    provideAnimations(),

    // ── MSAL core ────────────────────────────────────────────────────────────
    { provide: MSAL_INSTANCE,           useFactory: MSALInstanceFactory           },
    { provide: MSAL_GUARD_CONFIG,       useFactory: MSALGuardConfigFactory        },
    { provide: MSAL_INTERCEPTOR_CONFIG, useFactory: MSALInterceptorConfigFactory  },
    MsalService,
    MsalGuard,
    MsalBroadcastService,

    // ── MSAL HTTP Interceptor ────────────────────────────────────────────────
    // multi: true — does not replace other interceptors (e.g. error interceptors).
    {
      provide:  HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi:    true,
    },

    // ── App initializer ──────────────────────────────────────────────────────
    // Calls AuthService.initialize() (handleRedirectObservable) during Angular's
    // APP_INITIALIZER phase, BEFORE any component is rendered.
    // This ensures the auth code in the URL is exchanged for tokens before
    // the router activates any route, preventing a flash of the login page.
    {
      provide:    APP_INITIALIZER,
      useFactory: (authSvc: AuthService) => () => authSvc.initialize(),
      deps:       [AuthService],
      multi:      true,
    },
  ],
};
