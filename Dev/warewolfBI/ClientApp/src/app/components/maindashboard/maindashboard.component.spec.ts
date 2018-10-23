
import { fakeAsync, ComponentFixture, TestBed } from '@angular/core/testing';

import { MaindashboardComponent } from './maindashboard.component';

describe('MaindashboardComponent', () => {
  let component: MaindashboardComponent;
  let fixture: ComponentFixture<MaindashboardComponent>;

  beforeEach(fakeAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ MaindashboardComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MaindashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should compile', () => {
    expect(component).toBeTruthy();
  });
});
