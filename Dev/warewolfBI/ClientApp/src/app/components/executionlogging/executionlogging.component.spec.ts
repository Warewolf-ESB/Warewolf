import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ExecutionLoggingComponent } from './executionlogging.component';

describe('ExecutionloggingComponent', () => {
  let component: ExecutionLoggingComponent;
  let fixture: ComponentFixture<ExecutionLoggingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ExecutionLoggingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ExecutionLoggingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
