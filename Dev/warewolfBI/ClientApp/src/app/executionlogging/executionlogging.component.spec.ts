import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ExecutionloggingComponent } from './executionlogging.component';

describe('ExecutionloggingComponent', () => {
  let component: ExecutionloggingComponent;
  let fixture: ComponentFixture<ExecutionloggingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ExecutionloggingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ExecutionloggingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
