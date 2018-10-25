import {
  async, ComponentFixture, fakeAsync, TestBed, tick,
} from '@angular/core/testing';

import { RouterTestingModule } from '@angular/router/testing';
import { SpyLocation } from '@angular/common/testing';
import { Router, Routes } from '@angular/router';
import { By } from '@angular/platform-browser';
import { DebugElement, Type, CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA } from '@angular/core';
import { Location } from '@angular/common';

import { AppComponent } from '../app.component';
import { ExecutionLoggingComponent } from '../components/executionlogging/executionlogging.component';
import { SettingsComponent } from '../components/settings/settings.component';
import { AppRoutingModule } from '../core/app-routing.module';

const appRoutes: Routes = [
  { path: '', component: ExecutionLoggingComponent },
  { path: 'executionlogging', component: ExecutionLoggingComponent },
  { path: 'settings', component: SettingsComponent }
];
describe('Router: App', () => {

  let location: Location;
  let router: Router;
  let fixture;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        RouterTestingModule.withRoutes(appRoutes)
      ],
      declarations: [
        ExecutionLoggingComponent,
        SettingsComponent,
        AppComponent
      ],
      schemas: [
        CUSTOM_ELEMENTS_SCHEMA,
        NO_ERRORS_SCHEMA]
    });

    router = TestBed.get(Router);
    location = TestBed.get(Location);
    fixture = TestBed.createComponent(AppComponent);
    router.initialNavigation();
  });

  //it('fakeAsync works', fakeAsync(() => {
  //  let promise = new Promise((resolve) => {
  //    setTimeout(resolve, 10)
  //  });
  //  let done = false;
  //  promise.then(() => done = true);
  //  tick(50);
  //  expect(done).toBeTruthy();
  //}));

  //it('navigate to "" redirects you to /executionlogging', fakeAsync(() => {
  //  router.navigate(['']);
  //  tick(50);
  //  expect(location.path()).toBe('/executionlogging');
  //}));

  //it('navigate to "settings" takes you to /settings', fakeAsync(() => {
  //  router.navigate(['/settings']);
  //  tick(50);
  //  expect(location.path()).toBe('/settings');
  //}));
});
