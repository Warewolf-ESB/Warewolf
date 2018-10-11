import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DashboardCardsCreatorComponent } from './dashboard-cards-creator.component';

describe('DashboardCardsCreatorComponent', () => {
  let component: DashboardCardsCreatorComponent;
  let fixture: ComponentFixture<DashboardCardsCreatorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DashboardCardsCreatorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DashboardCardsCreatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
