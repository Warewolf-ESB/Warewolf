import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OutputsExplorerComponent } from './outputs-explorer.component';

describe('OutputsExplorerComponent', () => {
  let component: OutputsExplorerComponent;
  let fixture: ComponentFixture<OutputsExplorerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [OutputsExplorerComponent ]
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
