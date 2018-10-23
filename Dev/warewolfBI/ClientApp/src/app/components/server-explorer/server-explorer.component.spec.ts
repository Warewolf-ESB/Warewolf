import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ServerExplorerComponent } from './server-explorer.component';

describe('ServerExplorerComponent', () => {
  let component: ServerExplorerComponent;
  let fixture: ComponentFixture<ServerExplorerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ServerExplorerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ServerExplorerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
