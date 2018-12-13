import {
  async, ComponentFixture, fakeAsync, TestBed, tick,
} from '@angular/core/testing';


import { RouterTestingModule } from '@angular/router/testing';
import { SpyLocation } from '@angular/common/testing';

import * as r from '@angular/router';
import { Router, RouterLinkWithHref, NavigationEnd } from '@angular/router';

import { By } from '@angular/platform-browser';
import { DebugElement, Type } from '@angular/core';
import { Location } from '@angular/common';

import { click } from '../testing';

import { AppTestingModule } from './app-testing.module';

import { AppComponent } from './app.component';
import { ExecutionLoggingComponent } from './components/executionlogging/executionlogging.component';

let comp: AppComponent;
let fixture: ComponentFixture<AppComponent>;
let page: Page;
let router: Router;
let location: SpyLocation;

function createComponent() {
  fixture = TestBed.createComponent(AppComponent);
  comp = fixture.componentInstance;

  const injector = fixture.debugElement.injector;
  location = injector.get(Location) as SpyLocation;
  router = injector.get(Router);
  router.initialNavigation();
  advance();
  page = new Page();
}

function advance(): void {
  tick();
  fixture.detectChanges();
}

class Page {
  executionloggingLinkDe: DebugElement;
  recordedEvents: any[] = [];
  comp: AppComponent;
  location: SpyLocation; 
  fixture: ComponentFixture<AppComponent>;

  expectEvents(pairs: any[]) {
    const events = this.recordedEvents;
    console.log(events);
    console.log(pairs.length);
    expect(events.length).toEqual(pairs.length, 'actual/expected events length mismatch');
    for (let i = 0; i < events.length; ++i) {
      expect((<any>events[i].constructor).name).toBe(pairs[i][0].name, 'unexpected event name');
      expect((<any>events[i]).url).toBe(pairs[i][1], 'unexpected event url');
    }
  }

  constructor() {
    router.events.subscribe((val) => {
      this.recordedEvents.push(val)
      console.log(val instanceof NavigationEnd)
    });
    const links = fixture.debugElement.queryAll(By.directive(RouterLinkWithHref));
    this.executionloggingLinkDe = links[0];
    this.comp = comp;
    this.fixture = fixture;
    //this.router = router;
  }
}

function expectPathToBe(path: string, expectationFailOutput?: any) {
  expect(location.path()).toEqual(path, expectationFailOutput || 'location.path()');
}

function expectElementOf(type: Type<any>): any {
  const el = fixture.debugElement.query(By.directive(type));
  expect(el).toBeTruthy('expected an element for ' + type.name);
  return el;
}
