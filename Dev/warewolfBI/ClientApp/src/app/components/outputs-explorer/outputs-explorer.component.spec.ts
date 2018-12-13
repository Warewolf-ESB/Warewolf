import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { RouterTestingModule } from '@angular/router/testing';
import { OutputsExplorerComponent } from './outputs-explorer.component';

describe('OutputsExplorerComponent', () => {
  let component: OutputsExplorerComponent;
  let fixture: ComponentFixture<OutputsExplorerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [OutputsExplorerComponent],
      imports: [RouterTestingModule],
      schemas: [NO_ERRORS_SCHEMA]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OutputsExplorerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
