/**
 * main.ts — Angular application entry point.
 *
 * bootstrapApplication uses the standalone component API (Angular 14+).
 * appConfig provides all providers (router, MSAL, Material animations, HttpClient).
 * No NgModule is needed — all wiring is done via standalone components and
 * the ApplicationConfig in app.config.ts.
 */
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));

