import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, DebugElement, CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA } from '@angular/core';
import { By } from '@angular/platform-browser';
import { RouterTestingModule } from '@angular/router/testing';
import { AppComponent } from './app.component';
import { ServerExplorerComponent } from './components/server-explorer/server-explorer.component';
import { OutputsExplorerComponent } from './components/outputs-explorer/outputs-explorer.component';
import { ExecutionLoggingComponent } from './components/executionlogging/executionlogging.component';
import { SettingsComponent } from './components/settings/settings.component';
import { HttpClientModule } from '@angular/common/http';
import { MediatorService } from './services/mediator.service';
import { RouterLinkDirectiveStub } from '../testing';

import { APIService } from './services/api.service';

@Component({ selector: 'router-outlet', template: '' })
class RouterOutletStubComponent { }

let fixture: ComponentFixture<AppComponent>;
let component: AppComponent;
const routerSpy = jasmine.createSpyObj('Router', ['navigateByUrl']);

describe('Component: AppComponent', () => {
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientModule,RouterTestingModule],
      providers: [MediatorService, APIService, { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }],
      declarations: [
        AppComponent,
        RouterLinkDirectiveStub,
        RouterOutletStubComponent
      ],
      schemas: [
        CUSTOM_ELEMENTS_SCHEMA,
        NO_ERRORS_SCHEMA
      ],
    })
      .compileComponents().then(() => {
        fixture = TestBed.createComponent(AppComponent);
        component = fixture.componentInstance;
      });
  }));
  tests();
});

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

function tests() {
  let routerLinks: RouterLinkDirectiveStub[];
  let linkDes: DebugElement[];

  beforeEach(() => {
    fixture.detectChanges();
    linkDes = fixture.debugElement.queryAll(By.directive(RouterLinkDirectiveStub));
    routerLinks = linkDes.map(de => de.injector.get(RouterLinkDirectiveStub));
  });

  it('can instantiate the component', () => {
    expect(component).not.toBeNull();
  });

  it('should set animation event to auto', () => {
    component.animationEvent(null);
    expect(component.overflowState).toEqual('auto');
  });
  //Removed as these links are currently commented out on the HTML
  //it('can get RouterLinks from template', () => {
  //  expect(routerLinks.length).toBe(1, 'should have 1 routerLinks');
  //  expect(routerLinks[0].linkParams).toBe('/executionlogging');
  //});

  //it('can click executionlogging link in template', () => {
  //  const executionloggingLinkDe = linkDes[0];
  //  const executionloggingLink = routerLinks[0];

  //  expect(executionloggingLink.navigatedTo).toBeNull('should not have navigated yet');

  //  executionloggingLinkDe.triggerEventHandler('click', {button:1});
  //  fixture.detectChanges();

  //  expect(executionloggingLink.navigatedTo).toBe('/executionlogging');
  //});

  it('should set correct values on toggle side nav close', () => {
    component.sideNavState = 'close';

    spyOn(component, 'toggleSideNav');

    component.toggleSideNav();
    fixture.detectChanges();
    fixture.whenStable().then(() => {
      component.sideNavState = 'close';
      component.toggleSideNav();
      fixture.detectChanges();
      fixture.whenStable().then(() => {
        expect(component.toggleSideNav).toHaveBeenCalled();
        expect(component.sideNavState).toEqual('open');
        expect(component.overflowState).toEqual('hidden');
      });
    });
    expect(component.toggleSideNav).toHaveBeenCalled();
  });

  it('should set correct values on toggle side nav open', () => {
    component.sideNavState = 'open';

    spyOn(component, 'toggleSideNav');

    component.toggleSideNav();
    fixture.detectChanges();
    fixture.whenStable().then(() => {
      component.sideNavState = 'open';
      component.toggleSideNav();
      fixture.detectChanges();
      fixture.whenStable().then(() => {
        expect(component.toggleSideNav).toHaveBeenCalled();
        expect(component.sideNavState).toEqual('open');
        expect(component.overflowState).toEqual('hidden');
      });
    });
    expect(component.toggleSideNav).toHaveBeenCalled();
  });

  it('should set correct values on toggle side nav default', () => {
    spyOn(component, 'toggleSideNav');

    component.toggleSideNav();
    fixture.detectChanges();
    fixture.whenStable().then(() => {
      component.toggleSideNav();
      fixture.detectChanges();
      fixture.whenStable().then(() => {
        expect(component.toggleSideNav).toHaveBeenCalled();
        expect(component.sideNavState).toEqual('open');
        expect(component.overflowState).toEqual('hidden');
      });
    });
    expect(component.toggleSideNav).toHaveBeenCalled();
  });
}
