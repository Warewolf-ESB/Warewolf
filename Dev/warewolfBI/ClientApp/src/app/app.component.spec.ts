import { TestBed, inject, async, fakeAsync, tick, ComponentFixture } from '@angular/core/testing';
import { DebugElement, Component, Input, Directive } from "@angular/core";
import { By } from '@angular/platform-browser';

import { AppTestingModule } from './app-testing-module';
import { RouterTestingModule } from '@angular/router/testing';
import { RouterLinkDirective } from '../testing/router-link.directive';

import { AppComponent } from './app.component';
import { ServerExplorerComponent } from './components/server-explorer/server-explorer.component';
import { OutputsExplorerComponent } from './components/outputs-explorer/outputs-explorer.component';
import { ExecutionLoggingComponent } from './components/executionlogging/executionlogging.component';
import { SettingsComponent } from './components/settings/settings.component';

@Component({ selector: 'router-outlet', template: '' })
class RouterOutletStubComponent {

}

let fixture: ComponentFixture<AppComponent>;
let component: AppComponent;
const routerSpy = jasmine.createSpyObj('Router', ['navigateByUrl']);

describe('AppComponent & TestModule', () => {

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [
        AppTestingModule,
        RouterTestingModule
      ]
    })
      .compileComponents().then(() => {
        fixture = TestBed.createComponent(AppComponent);
        component = fixture.componentInstance;
      });
  }));
  tests();
});

import { AppModule } from './app.module';
import { AppRoutingModule } from './core/app-routing.module';

describe('AppComponent & AppModule', () => {

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [AppModule]
    })
      .overrideModule(AppModule, {
        remove: {
          imports: [AppRoutingModule]
        },
        add: {
          declarations: [RouterLinkDirective, RouterOutletStubComponent]
        }
      })

      .compileComponents()

      .then(() => {
        fixture = TestBed.createComponent(AppComponent);
        component = fixture.componentInstance;
      });
  }));

  tests();
});

function tests() {
  let routerLinks: RouterLinkDirective[];
  let linkDes: DebugElement[];

  beforeEach(() => {
    fixture.detectChanges();
    linkDes = fixture.debugElement.queryAll(By.directive(RouterLinkDirective));
    routerLinks = linkDes.map(de => de.injector.get(RouterLinkDirective));
  });

  it('should create the app', async(() => {
    expect(component).toBeTruthy();
  }));

  it('can instantiate the component', () => {
    expect(component).not.toBeNull();
  });

  it('can get RouterLinks from template', () => {
    expect(routerLinks.length).toBe(1, 'should have 1 routerLinks');
    expect(routerLinks[0].linkParams).toBe('/executionlogging');
  });

  it('can click executionlogging link in template', () => {
    const executionloggingLinkDe = linkDes[1];
    const executionloggingLink = routerLinks[1];

    expect(executionloggingLink.navigatedTo).toBeNull('should not have navigated yet');

    executionloggingLinkDe.triggerEventHandler('click', null);
    fixture.detectChanges();

    expect(executionloggingLink.navigatedTo).toBe('/executionlogging');
  });
}
