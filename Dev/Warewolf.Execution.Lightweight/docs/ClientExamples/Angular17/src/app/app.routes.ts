/**
 * app.routes.ts — Application route definitions.
 *
 * Route protection:
 *  /workflow            — guarded by MsalGuard (requires signed-in user)
 *  /client-credentials  — public (explains flow, demo calls a backend proxy)
 *  /                    — redirects to /workflow
 *
 * MsalGuard behaviour:
 *  When the user navigates to a canActivate: [MsalGuard] route without a
 *  cached account, MSAL triggers an interactive redirect to Entra login.
 *  After successful authentication, MSAL redirects back to the original URL.
 *
 * initialNavigation: 'enabledBlocking' (set in provideRouter options below)
 *  Required by MSAL to prevent duplicate navigation events during the
 *  redirect callback processing.
 */
import { Routes } from '@angular/router';
import { MsalGuard } from '@azure/msal-angular';
import { HomeComponent } from './home/home.component';
import { WorkflowComponent } from './workflow/workflow.component';
import { ClientCredentialsComponent } from './client-credentials/client-credentials.component';

export const routes: Routes = [
  {
    path:      'home',
    component: HomeComponent,
    // Public landing page — no auth required.
  },
  {
    path:        'workflow',
    component:   WorkflowComponent,
    // MsalGuard enforces authentication before rendering this page.
    // If the user is not signed in, MSAL redirects to Entra login.
    canActivate: [MsalGuard],
  },
  {
    path:      'client-credentials',
    component: ClientCredentialsComponent,
    // No guard — this page is informational and the demo calls a backend proxy.
  },
  {
    path:       '',
    redirectTo: 'home',
    pathMatch:  'full',
  },
  {
    path:       '**',
    redirectTo: 'home',
  },
];
